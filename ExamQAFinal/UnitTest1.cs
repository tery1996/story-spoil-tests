using RestSharp;
using RestSharp.Authenticators;
using System.Net;
using System.Text.Json;

namespace ExamQAFinal
{
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

            var request = new RestRequest("/User/Authentication", Method.Post);
            request.AddJsonBody(new { userName = "marto0981", password = "121212" });

            var response = _client.ExecuteAsync(request).Result;

            using var doc = JsonDocument.Parse(response.Content);
            var root = doc.RootElement;

            if (root.TryGetProperty("accessToken", out var at))
            {
                _token = at.GetString();
            }
            else
            {
                throw new Exception("accessToken was not found in response: " + response.Content);
            }

            _client = new RestClient(new RestClientOptions(BaseUrl)
            {
                Authenticator = new JwtAuthenticator(_token)
            });
        }

        [Test, Order(1)]
        public void CreateStorySpoiler()
        {
            var request = new RestRequest("/Story/Create", Method.Post);
            request.AddJsonBody(new
            {
                title = "My First Exam Story",
                description = "This is a spoiler for the exam system",
                url = ""
            });

            var response = _client.ExecuteAsync(request).Result;
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));

            using var doc = JsonDocument.Parse(response.Content);
            var root = doc.RootElement;

            Assert.That(root.GetProperty("msg").GetString(), Is.EqualTo("Successfully created!"));
            Assert.That(root.TryGetProperty("storyId", out var sid), "storyId field missing in response");
            _storyId = sid.GetString();
        }

        [Test, Order(2)]
        public void EditStorySpoiler()
        {
            var request = new RestRequest($"/Story/Edit/{_storyId}", Method.Put);
            request.AddJsonBody(new
            {
                title = "Updated Story Title",
                description = "Updated spoiler description",
                url = ""
            });

            var response = _client.ExecuteAsync(request).Result;
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            using var doc = JsonDocument.Parse(response.Content);
            Assert.That(doc.RootElement.GetProperty("msg").GetString(), Is.EqualTo("Successfully edited"));
        }

        [Test, Order(3)]
        public void GetAllStories()
        {
            var request = new RestRequest("/Story/All", Method.Get);
            var response = _client.ExecuteAsync(request).Result;

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            using var doc = JsonDocument.Parse(response.Content);
            Assert.IsTrue(doc.RootElement.GetArrayLength() > 0, "Returned stories array is empty");
        }

        [Test, Order(4)]
        public void DeleteStorySpoiler()
        {
            var request = new RestRequest($"/Story/Delete/{_storyId}", Method.Delete);
            var response = _client.ExecuteAsync(request).Result;

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            using var doc = JsonDocument.Parse(response.Content);
            Assert.That(doc.RootElement.GetProperty("msg").GetString(), Is.EqualTo("Deleted successfully!"));
        }

        [Test, Order(5)]
        public void CreateStoryWithoutRequiredFields()
        {
            var request = new RestRequest("/Story/Create", Method.Post);
            request.AddJsonBody(new { url = "" });

            var response = _client.ExecuteAsync(request).Result;
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }

        [Test, Order(6)]
        public void EditNonExistingStory()
        {
            var request = new RestRequest("/Story/Edit/999999", Method.Put);
            request.AddJsonBody(new
            {
                title = "Ghost Story",
                description = "This story does not exist",
                url = ""
            });

            var response = _client.ExecuteAsync(request).Result;
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));

            using var doc = JsonDocument.Parse(response.Content);
            Assert.That(doc.RootElement.GetProperty("msg").GetString(), Is.EqualTo("No spoilers..."));
        }

        [Test, Order(7)]
        public void DeleteNonExistingStory()
        {
            var request = new RestRequest("/Story/Delete/999999", Method.Delete);
            var response = _client.ExecuteAsync(request).Result;

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));

            using var doc = JsonDocument.Parse(response.Content);
            Assert.That(doc.RootElement.GetProperty("msg").GetString(), Is.EqualTo("Unable to delete this story spoiler!"));
        }
    }
}
