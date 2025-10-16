using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Web;

namespace HProxyApiClient
{
    // Các lớp để biểu diễn cấu trúc JSON trả về
    #region Response Models
    public class State
    {
        [JsonPropertyName("state_id")]
        public int StateId { get; set; }

        [JsonPropertyName("state_name")]
        public string StateName { get; set; }

        [JsonPropertyName("cities")]
        public List<string> Cities { get; set; }
    }

    public class Country
    {
        [JsonPropertyName("country_id")]
        public int CountryId { get; set; }

        [JsonPropertyName("country_name")]
        public string CountryName { get; set; }

        [JsonPropertyName("region")]
        public string Region { get; set; }

        [JsonPropertyName("states")]
        public List<State> States { get; set; }
    }

    public class LocationResponse
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("locations")]
        public List<Country> Locations { get; set; }
    }

    public class Route
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("id")]
        public int Id { get; set; }
    }

    public class ProtoTypesResponse
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("protoTypes")]
        public List<string> ProtoTypes { get; set; }
    }

    public class ProxyDetails
    {
        [JsonPropertyName("host")]
        public string Host { get; set; }

        [JsonPropertyName("port")]
        public int Port { get; set; }

        [JsonPropertyName("username")]
        public string Username { get; set; }

        [JsonPropertyName("password")]
        public string Password { get; set; }

        [JsonPropertyName("protoType")]
        public string ProtoType { get; set; }

        [JsonPropertyName("full_proxy")]
        public string FullProxy { get; set; }

        [JsonPropertyName("expires_in_minutes")]
        public int ExpiresInMinutes { get; set; }
    }

    public class NewProxyResponse
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("proxy")]
        public ProxyDetails Proxy { get; set; }
    }

    public class CurrentProxyResponse
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("proxy")]
        public string Proxy { get; set; }
    }
    #endregion

    public class HProxyAPI
    {
        private const string BaseUrl = "https://hproxy.xyz/public/api/v1";
        private readonly string _apiKey;
        private static readonly HttpClient HttpClient = new HttpClient();

        public HProxyAPI(string apiKey)
        {
            if (string.IsNullOrEmpty(apiKey))
            {
                throw new ArgumentNullException(nameof(apiKey), "API key là bắt buộc.");
            }
            _apiKey = apiKey;
        }

        private async Task<T> MakeRequestAsync<T>(string endpoint, Dictionary<string, string> parameters = null)
        {
            var uriBuilder = new UriBuilder($"{BaseUrl}/{endpoint}");
            var query = HttpUtility.ParseQueryString(uriBuilder.Query);
            query["api_key"] = _apiKey;

            if (parameters != null)
            {
                foreach (var param in parameters)
                {
                    query[param.Key] = param.Value;
                }
            }
            uriBuilder.Query = query.ToString();

            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, uriBuilder.ToString());
                request.Headers.Add("Accept", "application/json");

                HttpResponseMessage response = await HttpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    string errorContent = await response.Content.ReadAsStringAsync();
                    string errorMessage = $"Lỗi HTTP: {(int)response.StatusCode}. ";
                    if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                    {
                        errorMessage += "Yêu cầu không hợp lệ. Có thể thiếu tham số.";
                    }
                    else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        errorMessage += "API key không hợp lệ hoặc đã hết hạn.";
                    }
                    throw new HttpRequestException($"{errorMessage}\nNội dung: {errorContent}");
                }

                string jsonResponse = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<T>(jsonResponse);
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Lỗi yêu cầu HTTP: {ex.Message}");
                throw;
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"Lỗi giải mã JSON: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Đã xảy ra lỗi không mong muốn: {ex.Message}");
                throw;
            }
        }

        public Task<LocationResponse> GetAllLocationsAsync() => MakeRequestAsync<LocationResponse>("getAllLocations.php");
        public Task<List<Route>> GetRoutesAsync() => MakeRequestAsync<List<Route>>("getRoutes.php");
        public Task<ProtoTypesResponse> GetProtoTypesAsync() => MakeRequestAsync<ProtoTypesResponse>("getProtoTypes.php");
        public Task<CurrentProxyResponse> GetCurrentProxyAsync() => MakeRequestAsync<CurrentProxyResponse>("getCurrentProxy.php");

        public Task<NewProxyResponse> GetNewProxyAsync(string countryCode = null, string host = null, string state = null, string city = null, string protoType = null)
        {
            var parameters = new Dictionary<string, string>();
            if (!string.IsNullOrEmpty(countryCode)) parameters["country_code"] = countryCode;
            if (!string.IsNullOrEmpty(host)) parameters["host"] = host;
            if (!string.IsNullOrEmpty(state)) parameters["state"] = state;
            if (!string.IsNullOrEmpty(city)) parameters["city"] = city;
            if (!string.IsNullOrEmpty(protoType)) parameters["protoType"] = protoType;

            return MakeRequestAsync<NewProxyResponse>("getNewProxy.php", parameters);
        }
    }
}
