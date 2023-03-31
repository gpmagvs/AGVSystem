using Newtonsoft.Json;

namespace AGVSystem.HttpHelper
{
    public class Http
    {

        public class clsInternalError
        {
            public string ErrorCode { get; set; }
            public string ErrorMessage { get; set; } = string.Empty;
        }

        public static async Task<(HttpResponseMessage response, string content)> Get(string host, string request_url)
        {
            string jsonContent = "";
            HttpResponseMessage response = null;
            try
            {
                using HttpClient client = new HttpClient();
                client.BaseAddress = new Uri(host);
                response = await client.GetAsync(request_url);
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    jsonContent = await response.Content.ReadAsStringAsync();
                }
                return (response, jsonContent);
            }
            catch (Exception ex)
            {
                response = new HttpResponseMessage()
                {
                    StatusCode = System.Net.HttpStatusCode.InternalServerError
                };

                clsInternalError internalError = new clsInternalError()
                {
                    ErrorMessage = ex.Message,
                };

                return (response, JsonConvert.SerializeObject(internalError, Formatting.Indented));
            }
        }
    }
}
