using System.Net;

namespace ChromeExtension.Model
{
    public class ApiResponse
    {
        public HttpStatusCode StatusCode { get; set; }
        public string Message { get; set; } = string.Empty;
        public bool IsSuccess { get; set; }
        public object Data { get; set; } = new();
    }
}
