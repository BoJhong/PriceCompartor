using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Text;

namespace PriceCompartor.Models.GeminiModels
{
    public class AIHttpClient
    {
        #region MyRegion
        protected static HttpClient _httpClient;
        protected static string[] ApiKeys = Environment.GetEnvironmentVariable("GEMINI_API_KEYS")?.Split(',')!;
        protected static int ApiKeyCycle;
        #endregion

        static AIHttpClient()
        {
            _httpClient = new HttpClient();
        }

        public static string GetApiKey()
        {
            ApiKeyCycle = ++ApiKeyCycle % ApiKeys.Length;
            return ApiKeys[ApiKeyCycle];
        }

        public async Task<ResponseData> PostAsync<RequestData, ResponseData>(RequestData data, string url) where RequestData : class, new() where ResponseData : class, new()
        {
            try
            {
                StringContent content = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");

                HttpResponseMessage response = await _httpClient.PostAsync(url, content);
                response.EnsureSuccessStatusCode();
                string responseBody = string.Empty;

                responseBody = await response.Content.ReadAsStringAsync();
                if (!string.IsNullOrWhiteSpace(responseBody))
                {
                    ResponseData? responseData = JsonConvert.DeserializeObject<ResponseData>(responseBody);

                    return SetResponse(responseData ?? null, true, "");
                }

                return SetResponse<ResponseData>(null, false, "网络请求失败！");

            }
            catch (Exception ex)
            {
                return SetResponse<ResponseData>(null, false, ex.Message);
            }
        }

        public async Task<ResponseData> PostAsync<ResponseData>(string url, string data) where ResponseData : class, new()
        {
            try
            {
                StringContent content = new StringContent(data, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await _httpClient.PostAsync(url, content);
                response.EnsureSuccessStatusCode();
                string responseBody = string.Empty;


                responseBody = await response.Content.ReadAsStringAsync();
                if (!string.IsNullOrWhiteSpace(responseBody))
                {
                    ResponseData? responseData = JsonConvert.DeserializeObject<ResponseData>(responseBody);


                    return SetResponse(responseData ?? null, true, "");
                }

                return SetResponse<ResponseData>(null, false, "网络请求失败！");

            }
            catch (Exception ex)
            {
                return SetResponse<ResponseData>(null, false, ex.Message);
            }
        }

        public async Task<ResponseData> GetAsync<ResponseData>(string url) where ResponseData : class, new()
        {
            try
            {

                HttpResponseMessage response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();
                string responseBody = string.Empty;

                responseBody = await response.Content.ReadAsStringAsync();
                if (!string.IsNullOrWhiteSpace(responseBody))
                {
                    ResponseData? responseData = JsonConvert.DeserializeObject<ResponseData>(responseBody);

                    return SetResponse(responseData ?? null, true, "");
                }

                return SetResponse<ResponseData>(null, false, "网络请求失败！");

            }
            catch (Exception ex)
            {
                return SetResponse<ResponseData>(null, false, ex.Message);
            }

        }

        public async Task<ResponseData> DeleteAsync<ResponseData>(string url) where ResponseData : class, new()
        {
            try
            {
                HttpResponseMessage response = await _httpClient.DeleteAsync(url);
                response.EnsureSuccessStatusCode();
                string responseBody = string.Empty;


                responseBody = await response.Content.ReadAsStringAsync();
                if (!string.IsNullOrWhiteSpace(responseBody))
                {
                    ResponseData responseData = JsonConvert.DeserializeObject<ResponseData>(responseBody)!;


                    return SetResponse(responseData ?? null, true, "")!;
                }

                return SetResponse<ResponseData>(null, false, "网络请求失败！");

            }
            catch (Exception ex)
            {
                return SetResponse<ResponseData>(null, false, ex.Message);
            }
        }

        private ResponseData SetResponse<ResponseData>(ResponseData? responseData, bool success, string errormsg) where ResponseData : class, new()
        {
            if (responseData == null)
            {
                responseData = new ResponseData();
                success = false;
            }

            var properties = responseData.GetType().GetProperties();

            foreach (var item in properties)
            {
                if (!item.CanRead || !item.CanWrite) continue;

                if (item.Name == "Success")
                {
                    item.SetValue(responseData, success);
                }
                if (item.Name == "ErrorMsg")
                {
                    item.SetValue(responseData, errormsg);
                }
            }

            return responseData;
        }

    }
}
