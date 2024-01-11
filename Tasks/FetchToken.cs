using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Argali.Models;
using Newtonsoft.Json;

namespace Argali.Tasks;

public class FetchToken
{

    public static async Task<UbisoftTokenModel?> GetToken(HttpClient client)
    {
        client.DefaultRequestHeaders.Add("Ubi-AppId", "e3d5ea9e-50bd-43b7-88bf-39794f4e3d40");
        client.DefaultRequestHeaders.Add("Ubi-RequestedPlatformType", "uplay");

        Console.WriteLine("Connecting to the Ubisoft Servers.");

        /*Encoding.UTF8.GetBytes(ConfigurationManager.AppSettings["mail"] + ":" + ConfigurationManager.AppSettings["password"]*/
        if (!File.Exists("logins.txt"))
        {
            Console.WriteLine("Logins.txt not found.");
            Console.WriteLine("Creating logins.txt");

            await using (var file = File.Create("logins.txt"))
            { 
                file.Close();
            }
            
            await File.WriteAllTextAsync("logins.txt", "mail:password187");
            
            Console.WriteLine("Please enter your Ubisoft credentials in logins.txt");
            Console.WriteLine("Press any key to exit.");
            
            Console.ReadKey();
            Environment.Exit(0);
            return null;
        }

        var logins = await File.ReadAllLinesAsync("logins.txt");
        var random = new Random();
        var index = random.Next(0, logins.Length);
        
        var basicCredentials = Convert.ToBase64String(Encoding.UTF8.GetBytes(logins.ElementAt(index)));

        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Post,
            Headers = { Authorization = new AuthenticationHeaderValue("Basic", basicCredentials) },
            RequestUri = new Uri("https://public-ubiservices.ubi.com/v3/profiles/sessions"),
            Content = new StringContent("{\"Content-Type\": \"application/json\"}", Encoding.UTF8,
                "application/json")
        };
        var response = await client.SendAsync(request);
        if (!response.IsSuccessStatusCode)
            throw new UnauthorizedAccessException("Unable to retrieve authentication ticket.");
        return JsonConvert.DeserializeObject<UbisoftTokenModel?>(await response.Content.ReadAsStringAsync());
    }
}