using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using web_0799.Models;

namespace web_0799.Controllers
{
    public class ErrorController : Controller
    {
        [Route("Error")]
        public IActionResult Index()
        {
            var exceptionHandlerPathFeature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();
            var requestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;

            var model = new ErrorViewModel
            {
                RequestId = requestId
            };

            // Bạn có thể ghi log hoặc xử lý exception tại đây nếu cần
            var exception = exceptionHandlerPathFeature?.Error;

            // Log lỗi nếu cần: _logger.LogError(exception, "Unhandled exception");

            return View(model);
        }

        [Route("Error/{statusCode}")]
        public IActionResult HttpStatusCodeHandler(int statusCode)
        {
            var requestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
            var model = new ErrorViewModel
            {
                RequestId = requestId
            };

            // Có thể tùy chỉnh view cho từng mã lỗi tại đây
            switch (statusCode)
            {
                case 404:
                    return View("NotFound", model);
                case 500:
                    return View("ServerError", model);
                default:
                    return View("Index", model);
            }
        }
    }
}
