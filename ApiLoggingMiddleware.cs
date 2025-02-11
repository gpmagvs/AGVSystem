using NLog;
using System.Text;

namespace AGVSystem
{
    public class ApiLoggingMiddleware
    {
        private readonly Logger _logger;

        private readonly RequestDelegate _next;

        private List<string> IngnorePath = new List<string>()
        {
            "/api/Map",
            "/AGVImages",
            "/api/system",
            "/api/Equipment",
            "/api/Alarm",
            "/api/WIP",
            "/api/system/website",
            "/api/TaskQuery",
            "/FrontEndDataHub",
        };
        private List<string> contentTypesToIgnore = new() { "image", "text/css", "text/html", "application/javascript", "application/zip", "application/x-zip-compressed", "font" };

        public ApiLoggingMiddleware(RequestDelegate next)
        {
            _logger = LogManager.GetCurrentClassLogger();
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // 攔截請求
            var request = await FormatRequest(context.Request);
            // 紀錄回應前的狀態
            var originalBodyStream = context.Response.Body;

            using var responseBody = new MemoryStream();
            context.Response.Body = responseBody;
            await _next(context);

            // 攔截回應
            var response = await FormatResponse(context.Response);

            //如果回應的Contetn-type 是圖片、html、javascript、字體等不需要紀錄的資料，直接回傳
            if (contentTypesToIgnore.Any(type => context.Response.ContentType?.Contains(type) == true))
            {
                await responseBody.CopyToAsync(originalBodyStream);
                return;
            }

            string? _requestPath = context.Request.Path.Value;

            if (IngnorePath.Any(ingnore => _requestPath.ToLower().Contains(ingnore.ToLower())))
            {
                await responseBody.CopyToAsync(originalBodyStream);
                return;
            }

            // 紀錄資訊
            _logger.Info("Request: \n{Request}", request);
            _logger.Info("Response: \n{Response}", response);

            // 將原始回應內容寫回
            await responseBody.CopyToAsync(originalBodyStream);
        }

        /// <summary>
        /// 格式化請求
        /// </summary>
        private async Task<string> FormatRequest(HttpRequest request)
        {
            request.EnableBuffering();
            var headers = FormatHeaders(request.Headers);
            var body = await new StreamReader(request.Body, Encoding.UTF8).ReadToEndAsync();
            request.Body.Position = 0;
            var ip = request.HttpContext.Connection.RemoteIpAddress?.ToString();

            return $"Method: {request.Method}\n" +
                   $"URL: {request.Scheme}://{request.Host}{request.Path}{request.QueryString}\n" +
                   $"Body: {body}\n" +
                   $"IP: {ip}";
        }

        /// <summary>
        /// 格式化回應
        /// </summary>
        private async Task<string> FormatResponse(HttpResponse response)
        {
            response.Body.Seek(0, SeekOrigin.Begin);
            var headers = FormatHeaders(response.Headers);
            var body = await new StreamReader(response.Body, Encoding.UTF8).ReadToEndAsync();
            response.Body.Seek(0, SeekOrigin.Begin);

            return $"Status code: {response.StatusCode}\n" +
                   $"Headers: \n{headers}" +
                   $"Body: {body}";
        }

        /// <summary>
        /// 格式化標頭
        /// </summary>
        private string FormatHeaders(IHeaderDictionary headers)
        {
            var formattedHeaders = new StringBuilder();
            foreach (var (key, value) in headers)
            {
                formattedHeaders.AppendLine($"\t{key}: {string.Join(",", value)}");
            }

            return formattedHeaders.ToString();
        }
    }
}
