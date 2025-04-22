using BlooTusk.Model.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace BlooTusk.API.Controller
{

    [Route("api/[controller]")]
    [ApiController]
    public class BaseAPIController : ControllerBase
    {
        [ApiExplorerSettings(IgnoreApi = true)]
        [NonAction]
        public IActionResult ReturnErrorResponse(ErrorResponseModel model, string customErrorMessage = null)
        {
            if (model != null)
            {
                var errorMessage = string.IsNullOrEmpty(customErrorMessage) ? model.Message : customErrorMessage;
                switch (model.StatusCode)
                {
                    case HttpStatusCode.BadGateway:
                        return BadRequest(errorMessage);
                    case HttpStatusCode.ServiceUnavailable:
                        return StatusCode(StatusCodes.Status503ServiceUnavailable, "Service  not available for this user");
                    case HttpStatusCode.NotFound:
                        return StatusCode(StatusCodes.Status404NotFound, "Record Not found");
                    default:
                        return BadRequest(errorMessage);
                }
            }
            else
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Something went wrong!");
            }

        }
    }
}
