using System;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Argali
{
    internal class Program
    {

        private static readonly HttpClient _httpClient = new HttpClient();

        private static UbisoftToken _token;

        /**
         * Setting the default request headers which will be used if not explicitly changed in the request.
         */
        public Program()
        {
            _httpClient.DefaultRequestHeaders.Add("Ubi-AppId", "2c2d31af-4ee4-4049-85dc-00dc74aef88f");
            _httpClient.DefaultRequestHeaders.Add("Ubi-RequestedPlatformType", "uplay");
            _httpClient.DefaultRequestHeaders.Add("user-agent", "Mozilla/5.0 (iPhone; CPU iPhone OS 5_1 like Mac OS X) AppleWebKit/534.46 (KHTML, like Gecko) Version/5.1 Mobile/9B179 Safari/7534.48.3");
        }
        /**
         * Running the code.
         */
        public static void Main(string[] args)
        {
            Console.ForegroundColor = ConsoleColor.DarkBlue;
            Console.WriteLine("Please enter the name of the Player.");
            Console.ForegroundColor = ConsoleColor.White;
            do
            {
                while (!Console.KeyAvailable)
                {
                    GetProfileConnections().GetAwaiter().GetResult();
                    break;
                }
            } while (Console.ReadKey(true).Key != ConsoleKey.Escape);
        }
        /**
         * Fetching the profile connections and UserId.
         */
        private static async Task GetProfileConnections()
        {
            if (_token == null || _token.IsExpired)
            {
                _token = await GetToken();
            }

            var userToCheck = Console.ReadLine();

            try
            {
                /**
                 * Sending a request to Ubisoft for the Profile and picking out the UserId
                 */
                var nameRequest = new HttpRequestMessage
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri($"https://public-ubiservices.ubi.com/v3/profiles?nameOnPlatform={userToCheck}&platformType=uplay"),
                    Headers = { Authorization = new AuthenticationHeaderValue("Ubi_v1", $"t={_token.Ticket}") }
                };
                var nameResponse = await _httpClient.SendAsync(nameRequest);
                var jsonContent = await nameResponse.Content.ReadAsStringAsync();
                var profileResponse = JsonConvert.DeserializeObject<JObject>(jsonContent);
                JObject jObject = JObject.Parse(jsonContent);
                var userId = jObject["profiles"].First["userId"];

                /**
                 * Fetching the Connections of the User.
                 */
                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri($"https://public-ubiservices.ubi.com/v3/profiles?userId={userId}"),
                    Headers = { Authorization = new AuthenticationHeaderValue("Ubi_v1", $"t={_token.Ticket}") }
                };
                var response = await _httpClient.SendAsync(request);

                var content = await response.Content.ReadAsStringAsync();
                var json = JsonConvert.DeserializeObject<JObject>(content);
                jObject.Merge(json, new JsonMergeSettings { MergeArrayHandling = MergeArrayHandling.Union });
                string connectionResponse = jObject.ToString();

                /**
                 * The output for the console.
                 */
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine($"The user {userToCheck} has following connections: ");
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine(connectionResponse);
            }
            catch
            {
                /*
                 * If we get an exception for the requests above the user does not exist.
                 */
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"The user {userToCheck} does not seem to have a ubisoft account.");
                Console.ForegroundColor = ConsoleColor.White;
            }
        }
        /**
         * Getting the Authentication Token from Ubisoft.
         */
        private static async Task<UbisoftToken> GetToken()
        {
            _httpClient.DefaultRequestHeaders.Add("Ubi-AppId", "afb4b43c-f1f7-41b7-bcef-a635d8c83822");
            _httpClient.DefaultRequestHeaders.Add("Ubi-RequestedPlatformType", "uplay");
            _httpClient.DefaultRequestHeaders.Add("user-agent",
                "Mozilla/5.0 (iPhone; CPU iPhone OS 5_1 like Mac OS X) AppleWebKit/534.46 (KHTML, like Gecko) Version/5.1 Mobile/9B179 Safari/7534.48.3");

            Console.WriteLine("Connecting to the Ubisoft Servers.");

            var basicCredentials = Convert.ToBase64String(
                Encoding.UTF8.GetBytes(ConfigurationManager.AppSettings["mail"] + ":" + ConfigurationManager.AppSettings["password"]));

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                Headers = { Authorization = new AuthenticationHeaderValue("Basic", basicCredentials) },
                RequestUri = new Uri("https://public-ubiservices.ubi.com/v3/profiles/sessions"),
                Content = new StringContent("{\"Content-Type\": \"application/json\"}", Encoding.UTF8,
                    "application/json")
            };
            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
                throw new UnauthorizedAccessException("Unable to retrieve authentication ticket.");
            return JsonConvert.DeserializeObject<UbisoftToken>(await response.Content.ReadAsStringAsync());
        }
    }
}