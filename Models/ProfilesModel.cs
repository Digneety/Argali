using System;
using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

namespace Argali.Models;

public class ProfilesModel
{
    [JsonProperty("profileId"), NotNull]
    public string? ProfileId { get; set; }
    
    [JsonProperty("userId")]
    public string? UserId { get; set; }
    
    [JsonProperty("platformType")]
    private string? PlatformType { get; set; }
    
    [JsonProperty("idOnPlatform")]
    public string? IdOnPlatform { get; set; }
    
    [JsonProperty("nameOnPlatform")]
    public string? NameOnPlatform { get; set; }
    
    public string? ProfileUrl => PlatformType switch
    {
        "steam" => $"https://steamcommunity.com/profiles/{IdOnPlatform}",
        "epic" => $"https://store.epicgames.com/en-US/u/{IdOnPlatform}",
        "uplay" => $"https://ubisoftconnect.com/en-US/profile/{NameOnPlatform}",
        "xbl" => $"https://account.xbox.com/en-US/profile?gamertag={Uri.EscapeDataString(NameOnPlatform!)}",
        "psn" => $"https://psnprofiles.com/{NameOnPlatform}",
        
        _ => "No profile URL found."
    };
    
    public string? Platform
    {
        get
        {
            return PlatformType switch
            {
                "uplay" => "Uplay",
                "steam" => "Steam",
                "epic" => "Epic",
                "xbl" => "Xbox",
                "psn" => "Playstation",
                "amazonstream " => "Luna",
                "amazon" => "Amazon",
                "switch" => "Nintendo Switch",
                "nintendo" => "Nintendo",
                "twitch" => "Twitch",
                _ => null
            };
        }
        set => PlatformType = value;
    }
}