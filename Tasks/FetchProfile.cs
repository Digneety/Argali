using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Argali.Models;
using Newtonsoft.Json;

namespace Argali.Tasks;

public class FetchProfile
{
    private static UbisoftTokenModel? _token;
    
    private static readonly HttpClient Client = new ();

    public static async Task GetProfileConnections()
    {
        if (_token == null || _token.IsExpired)
        {
            _token = await FetchToken.GetToken(Client);
            Console.ForegroundColor = ConsoleColor.DarkBlue;
            Console.WriteLine("Please enter the name of the Player.");
            Console.ForegroundColor = ConsoleColor.White;
        }

        var userToCheck = Console.ReadLine();

        var nameRequest = new HttpRequestMessage
        {
            Method = HttpMethod.Get,
            RequestUri =
                new Uri(
                    $"https://public-ubiservices.ubi.com/v3/profiles?nameOnPlatform={userToCheck}&platformType=uplay"),
            Headers = { Authorization = new AuthenticationHeaderValue("Ubi_v1", $"t={_token?.Ticket}") }
        };
        var nameResponse = await Client.SendAsync(nameRequest);
        var jsonContent = await nameResponse.Content.ReadAsStringAsync();
        var profileResponse = JsonConvert.DeserializeObject<UbisoftProfileModel>(jsonContent);

        if (profileResponse == null)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Failed to retrieve the profile of {userToCheck}.");
            Console.ForegroundColor = ConsoleColor.White;
            return;
        }

        var profile = profileResponse.Profiles?.FirstOrDefault();

        if (profile == null)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"The user {userToCheck} does not seem to have a ubisoft account.");
            Console.ForegroundColor = ConsoleColor.White;
            return;
        }

        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri($"https://public-ubiservices.ubi.com/v3/profiles?userId={profile.UserId}"),
            Headers = { Authorization = new AuthenticationHeaderValue("Ubi_v1", $"t={_token?.Ticket}") }
        };
        var response = await Client.SendAsync(request);

        var profiles =
            JsonConvert.DeserializeObject<UbisoftProfileModel>(await response.Content.ReadAsStringAsync());

        if (profiles == null)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Failed to retrieve the connections of {userToCheck}.");
            Console.ForegroundColor = ConsoleColor.White;
            return;
        }

        var mergedProfiles = profiles.Profiles?.UnionBy(profileResponse.Profiles!, x => x.ProfileId);

        Console.ForegroundColor = ConsoleColor.Blue;
        Console.WriteLine($"The user {userToCheck} has following connections: ");
        Console.ForegroundColor = ConsoleColor.White;

        mergedProfiles?.ToList()?.ForEach(connection =>
            Console.WriteLine(JsonConvert.SerializeObject(new
            {
                ProfileUrl = connection.ProfileUrl,
                Platform = connection.Platform,
                Username = connection.NameOnPlatform,
                ProfileId = connection.ProfileId,
                IdOnPlatform = connection.IdOnPlatform,
                UserId = connection.UserId
            }, Formatting.Indented)));

    }
}