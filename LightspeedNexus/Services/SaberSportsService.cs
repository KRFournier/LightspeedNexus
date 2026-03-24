using RestSharp;
using System.Text.Json.Nodes;

namespace LightspeedNexus.Services;

public static class SaberSportsService
{
    private static readonly RestClient _client = new("https://services.saber-sport.com");
    private static readonly RestClient _testClient = new("https://services-test.saber-sport.com");

#if DEBUG
    private static RestClient Client => _testClient;
#else
    private static RestClient Client => _client;
#endif

    private static readonly DateTime _tokenExpiration = DateTime.MinValue;
    private static string? _bearerToken;

    public static string? LastEmail { get; private set; }

    public static string? LastPassword { get; private set; }

    public static bool LoginNeeded => string.IsNullOrEmpty(_bearerToken) || DateTime.Now >= _tokenExpiration;

    //public static void SaveLastUsed(bool savePassword)
    //{
    //    if (string.IsNullOrEmpty(LastEmail) || string.IsNullOrEmpty(LastPassword))
    //        return;

    //    var lastUsed = new BsonDocument
    //    {
    //        ["_id"] = 748,
    //        ["email"] = LastEmail,
    //        ["password"] = savePassword ? AesStringEncryption.EncryptString(LastPassword) : string.Empty
    //    };
    //    StorageService.WriteSettings(lastUsed);
    //}

    //public static void LoadLastUsed()
    //{
    //    var lastUsed = StorageService.ReadSettings(748);
    //    if (lastUsed != null)
    //    {
    //        LastEmail = lastUsed["email"].AsString;
    //        LastPassword = AesStringEncryption.DecryptString(lastUsed["password"].AsString);
    //    }
    //}

    public static async Task<string> RegisterEmail(string? email)
    {
        try
        {
            var response = await Client.PostAsync(
                new RestRequest("/auth/register")
                .AddJsonBody($"{{\"email\":\"{email}\"}}")
                );

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
                return $"Password sent to \"{email}\".";
        }
        catch (Exception)
        {
        }

        return """
            Register failed. Please check your email address and try again.
            If you've registered this email before, you can use the "Resend" option instead.
            """;
    }

    public static async Task<string> ResendPassword(string? email)
    {
        try
        {
            var response = await Client.PostAsync(
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
            var response = await Client.PostAsync(
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

    public static async Task<(bool Success, string Message)> Login(string email, string password, bool savePassword)
    {
        try
        {
            if (string.IsNullOrEmpty(password))
            {

            }

            var response = await Client.PostAsync(
                new RestRequest("/auth/token")
                .AddJsonBody($"{{\"email\":\"{email}\",\"password\":\"{password}\"}}")
                );
            if (response.IsSuccessful)
            {
                JsonNode? node = JsonNode.Parse(response.Content!);
                _bearerToken = node?["token"]?.GetValue<string>();
                //SaveLastUsed(savePassword);
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
                    return (true, "", fighters);
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

    public static async Task<(bool Success, string Message)> Submit(string body)
    {
        try
        {
            var request = new RestRequest("v1/tm/submit", Method.Post)
                .AddHeader("x-api-key", "@Lightspeed4Ever!")
                .AddJsonBody(body);

            //var response = await Client.PostAsync(request);
            var response = await Client.ExecuteAsync(request);

            if (response.IsSuccessful)
            {
                //JsonNode? node = JsonNode.Parse(response.Content!);
                //var responseMessage = node?["response"]?.GetValue<string>();
                return (true, "Tournament successfully submitted.");
            }
            else
                return (false, response.ErrorMessage ?? "Saber-sports submission failed");
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }
}
