


using RestSharp;
using RestSharp.Authenticators;
using System.Net;

namespace ExamQAFinal
{
    public class ApiResponseDTO
    {
        public string Msg { get; set; }
        public string StoryId { get; set; }
    }

    public class StoryDTO
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string Url { get; set; }
    }
    [TestFixture]
    public class Tests
    {
        private const string BaseUrl = "https://d3s5nxhwblsjbi.cloudfront.net/api";
        private RestClient _client;
        private static string _token;
        private static string _storyId;
        [SetUp]
        public void Setup()
        {
            _client = new RestClient(BaseUrl);

            // authenticate (replace with your real username/password!)
            var request = new RestRequest("/User/Authentication", Method.Post);
            request.AddJsonBody(new { userName = "marto0981", password = "121212" });

            var response = _client.ExecuteAsync<dynamic>(request).Result;
            _token = response.Data.accessToken;

            _client = new RestClient(new RestClientOptions(BaseUrl)
            {
                Authenticator = new JwtAuthenticator(_token)
            });
        }

        [Test]
        public void CreateStorySpoiler()
        {
            var request = new RestRequest("/Story/Create", Method.Post);

            var story = new
            {
                title = "My First Exam Story",
                description = "This is a spoiler for the exam system",
                url = ""
            };

            request.AddJsonBody(story);

            var response = _client.ExecuteAsync<dynamic>(request).Result;

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));
            Assert.That((string)response.Data.msg, Is.EqualTo("Successfully created!"));
            Assert.IsNotNull(response.Data.storyId);

            _storyId = response.Data.storyId;
        }

        [Test, Order(2)]
        public void EditStorySpoiler()
        {
            var request = new RestRequest($"/Story/Edit/{_storyId}", Method.Put);

            var updatedStory = new
            {
                title = "Updated Story Title",
                description = "Updated spoiler description",
                url = ""
            };

            request.AddJsonBody(updatedStory);

            var response = _client.ExecuteAsync<dynamic>(request).Result;

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That((string)response.Data.msg, Is.EqualTo("Successfully edited"));
        }

        [Test, Order(3)]
        public void GetAllStories()
        {
            var request = new RestRequest("/Story/All", Method.Get);

            var response = _client.ExecuteAsync<dynamic>(request).Result;

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            Assert.IsNotEmpty(response.Data);

            TestContext.WriteLine($"Total stories returned: {response.Data.Count}");
        }

        [Test, Order(4)]
        public void DeleteStorySpoiler()
        {
            var request = new RestRequest($"/Story/Delete/{_storyId}", Method.Delete);

            var response = _client.ExecuteAsync<dynamic>(request).Result;

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That((string)response.Data.msg, Is.EqualTo("Deleted successfully!"));
        }

        [Test, Order(5)]
        public void CreateStoryWithoutRequiredFields()
        {
            var request = new RestRequest("/Story/Create", Method.Post);

            // Missing title and description
            var badStory = new { url = "" };
            request.AddJsonBody(badStory);

            var response = _client.ExecuteAsync<dynamic>(request).Result;

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }

        [Test, Order(6)]
        public void EditNonExistingStory()
        {
            var request = new RestRequest("/Story/Edit/999999", Method.Put);

            var story = new
            {
                title = "Ghost Story",
                description = "This story does not exist",
                url = ""
            };

            request.AddJsonBody(story);

            var response = _client.ExecuteAsync<dynamic>(request).Result;

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
            Assert.That((string)response.Data.msg, Is.EqualTo("No spoilers..."));
        }

        [Test, Order(7)]
        public void DeleteNonExistingStory()
        {
            var request = new RestRequest("/Story/Delete/999999", Method.Delete);

            var response = _client.ExecuteAsync<dynamic>(request).Result;

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That((string)response.Data.msg, Is.EqualTo("Unable to delete this story spoiler!"));
        }
    }

}