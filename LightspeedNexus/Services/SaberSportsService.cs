using RestSharp;
using System.Text.Json.Nodes;

namespace LightspeedNexus.Services;

public class SaberSportsService
{
    private readonly RestClient _client = new("https://services.saber-sport.com");
    private readonly RestClient _testClient = new("https://services-test.saber-sport.com");

#if DEBUG
    private RestClient Client => _testClient;
#else
    private  RestClient Client => _client;
#endif

    public async Task<(bool Success, string message, Fighter[] Fighters)> GetAllFighters()
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
                    Fighter[] fighters = [.. arr.Select(f => SaberSportsSerializer.DeserializeFighter(f)).Where(f => f is not null)!];
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

    public async Task<(bool Success, string Message)> Submit(string body)
    {
        try
        {
            var request = new RestRequest("v1/tm/submit", Method.Post)
                .AddHeader("x-api-key", "@Lightspeed4Ever!")
                .AddJsonBody(body);

            var response = await Client.ExecuteAsync(request);

            if (response.IsSuccessful)
                return (true, "Tournament successfully submitted.");
            else
                return (false, response.ErrorMessage ?? "Saber-sports submission failed");
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }
}
