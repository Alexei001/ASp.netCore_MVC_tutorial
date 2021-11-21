using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ASp.netCore_empty_tutorial.Controllers
{
    public class ErrorController : Controller
    {
        private readonly ILogger<ErrorController> _logger;

        public ErrorController(ILogger<ErrorController> logger)
        {
            _logger = logger;
        }
        [Route("/error/{statusCode}")]
        public IActionResult HttpStatusCodeHandler(int statusCode)
        {
            var statusCodeResult = HttpContext.Features.Get<IStatusCodeReExecuteFeature>();


            switch (statusCode)
            {
                case 404:
                    ViewBag.ErrorMessage = "Sorry! the page could not be found";
                    //logger 
                    _logger.LogWarning($"404 error ocurred. Path ={statusCodeResult.OriginalPath} and QueryStrings: {statusCodeResult.OriginalQueryString}");

                    break;
            }

            return View("NotFound");
        }
        [Route("Error")]
        [AllowAnonymous]
        public IActionResult Error()
        {
            var exceptionDetails = HttpContext.Features.Get<IExceptionHandlerPathFeature>();
            _logger.LogTrace($"Error stack trace: {exceptionDetails.Error.StackTrace}");
            _logger.LogError($"Error message: {exceptionDetails.Error.StackTrace}");
            _logger.LogInformation($"Error path: {exceptionDetails.Path}");
            return View("Error");
        }
    }
}
