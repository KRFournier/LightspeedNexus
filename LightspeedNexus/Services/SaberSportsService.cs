using LiteDB;
using RestSharp;
using System;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using System.Security.Cryptography;
using LightspeedNexus.Models;
using System.Linq;
using System.Text.Json;

namespace LightspeedNexus.Services;

public static class SaberSportsService
{
#if DEBUG
    private static readonly RestClient _client = new("https://services-test.saber-sport.com");
#else
    private static readonly RestClient _client = new("https://services.saber-sport.com");
#endif

    private static string? _bearerToken;

    public static string? LastEmail { get; private set; }

    public static string? LastPassword { get; private set; }

    public static void SaveLastUsed()
    {
        if (string.IsNullOrEmpty(LastEmail) || string.IsNullOrEmpty(LastPassword))
            return;

        var lastUsed = new BsonDocument
        {
            ["_id"] = 748,
            ["email"] = LastEmail,
            ["password"] = AesStringEncryption.EncryptString(LastPassword)
        };
        StorageService.WriteDocument(lastUsed, "admin");
    }

    public static void LoadLastUsed()
    {
        var lastUsed = StorageService.ReadDocument(748, "admin");
        if (lastUsed != null)
        {
            LastEmail = lastUsed["email"].AsString;
            LastPassword = AesStringEncryption.DecryptString(lastUsed["password"].AsString);
        }
    }

    public static async Task<string> RegisterEmail(string email)
    {
        try
        {
            await _client.PostAsync(
                new RestRequest("/auth/register")
                .AddJsonBody($"{{\"email\":\"{email}\"}}")
                );
            return $"Password sent to \"{email}\".";
        }
        catch (Exception ex)
        {
            return ex.Message;
        }
    }

    public static async Task<string> ResendPassword(string email)
    {
        try
        {
            await _client.PostAsync(
                new RestRequest("/auth/resend")
                .AddJsonBody($"{{\"email\":\"{email}\"}}")
                );
            return $"Password resent to \"{email}\".";
        }
        catch (Exception ex)
        {
            return ex.Message;
        }
    }

    public static async Task<string> NewPassword(string email, string password)
    {
        try
        {
            await _client.PostAsync(
                new RestRequest("/auth/rotate")
                .AddJsonBody($"{{\"email\":\"{email}\",\"password\":\"{password}\"}}")
                );
            return $"New password sent to \"{email}\".";
        }
        catch (Exception ex)
        {
            return ex.Message;
        }
    }

    public static async Task<(bool Success, string Message)> Login(string email, string password)
    {
        try
        {
            var response = await _client.PostAsync(
                new RestRequest("/auth/token")
                .AddJsonBody($"{{\"email\":\"{email}\",\"password\":\"{password}\"}}")
                );
            if (response.IsSuccessful)
            {
                JsonNode? node = JsonNode.Parse(response.Content!);
                _bearerToken = node?["token"]?.GetValue<string>();
                SaveLastUsed();
                return (true, "");
            }
            else
                return (false, response.ErrorMessage ?? "");
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }

    public static async Task<(bool Success, string message, Fighter[] Fighters)> GetAllFighters()
    {
        try
        {
            var request = new RestRequest("v1/tm/fencers");
            var response = await _client.GetAsync(request);
            if (response.IsSuccessful)
            {
                JsonNode? node = JsonNode.Parse(response.Content!);
                if (node is JsonArray arr)
                {
                    Fighter[] fighters = [.. arr.Select(f => Fighter.FromSaberSport(f)).Where(f => f is not null)!];
                    return(true, "", fighters);
                }
                else
                    return (false, "Saber sport returned fencers in an unexpected format.", []);                
            }
            else
                return (false, response.ErrorMessage ?? response.ErrorException?.Message ?? "", []);
        }
        catch (Exception ex)
        {
            return (false, ex.Message, []);
        }
    }
}
