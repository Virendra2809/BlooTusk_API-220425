using BlooTusk.Business.Implementation;
using BlooTusk.Business.Interface;
using BlooTusk.Common;
using BlooTusk.Model.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.ComponentModel.DataAnnotations;

namespace BlooTusk.API.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminDashbaordController : BaseAPIController
    {
        //  AdminDashbaordController

        IAdminDashboardService adminDashboardService;
        private readonly IWebHostEnvironment iwebhostingEnvironment;

        public AdminDashbaordController(IAdminDashboardService _adminDashboardService, IWebHostEnvironment _iwebhostingEnvironment)
        {
            adminDashboardService = _adminDashboardService;
            iwebhostingEnvironment = _iwebhostingEnvironment;
        }


        [HttpGet("AdminDashboard/{merchantId}")]
        [Authorize]
        [ProducesResponseType(typeof(AdminDashboardModel), 200)]

        [ProducesResponseType(typeof(string), 404)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(string), 500)]
        public IActionResult AdminDashboard(int merchantId)
        {
            AdminDashboardModel adminDashboardModel = new AdminDashboardModel();
            try
            {
                 adminDashboardModel = adminDashboardService.AdminDashboardDetails(merchantId);
                return Ok(adminDashboardModel);
            }
            catch (Exception ex)
            {
                return Ok(new APIResponse
                {
                    ResponseStatusCode = GlobalConstants.ErrorStatus,
                    ErrorMessage = ex.ToString(),
                });
            }
        }

        [HttpPost("Customerstatement")]
        [Authorize]
        [ProducesResponseType(typeof(MerchantStatementRequest), 200)]
        [ProducesResponseType(typeof(string), 404)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(string), 500)]
        public IActionResult Customerstatement([FromBody] MerchantStatementRequest merchantUserModel)
        {
            try
            {
                var errorMessage = new ErrorResponseModel();
                var result = adminDashboardService.CustomerStatement(merchantUserModel);
                return Ok(result);

            }
            catch (Exception ex)
            {
                return Ok(new APIResponse
                {
                    ResponseStatusCode = GlobalConstants.ErrorStatus,
                    ErrorMessage = ex.ToString(),
                });
            }
        }


        [HttpPost("MerchantStatement")]
        [Authorize]
        [ProducesResponseType(typeof(NudgeModel), 200)]
        [ProducesResponseType(typeof(string), 404)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(string), 500)]
        public IActionResult MerchantStatement([FromBody] MerchantStatementRequest merchantUserModel)
        {
            try
            {
                var result = adminDashboardService.MerchantStatement(merchantUserModel);
                return Ok(result);
            }
            catch (Exception ex)
            {
                // Handle 404 Not Found
                return NotFound(new APIResponse
                {
                    ResponseStatusCode = GlobalConstants.NoDataStatus,
                    ErrorMessage = ex.Message,
                });
            }           
            
        }

        //[HttpGet("AdminGraphData")]
        //[Authorize]
        //[ProducesResponseType(typeof(AdminGraphModel), 200)]
        //[ProducesResponseType(typeof(string), 404)]
        //[ProducesResponseType(typeof(string), 400)]
        //[ProducesResponseType(typeof(string), 500)]
        //public IActionResult AdminGrapgData(int merchantId)
        //{
        //    try
        //    {
        //        var result = adminDashboardService.AdminGrapgData(merchantId);
        //        return Ok(result);
        //    }
        //    catch (Exception ex)
        //    {
        //        // Handle 404 Not Found
        //        return NotFound(new APIResponse
        //        {
        //            ResponseStatusCode = GlobalConstants.NoDataStatus,
        //            ErrorMessage = ex.Message,
        //        });
        //    }

        //}


        [HttpGet("AdminGrapgData/{merchantId}")]
     //   [Authorize]
        [ProducesResponseType(typeof(AdminGraphModel), 200)]
        [ProducesResponseType(typeof(string), 404)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(string), 500)]
        public IActionResult AdminGrapgData(int merchantId)
        {
            AdminGraphModel adminDashboardModel = new AdminGraphModel();
            try
            {
                adminDashboardModel = adminDashboardService.AdminGrapgData(merchantId);
                return Ok(adminDashboardModel);
            }
            catch (Exception ex)
            {
                return Ok(new APIResponse
                {
                    ResponseStatusCode = GlobalConstants.ErrorStatus,
                    ErrorMessage = ex.ToString(),
                });
            }
        }


        [HttpGet("GetMerchantDDL")]
        [ProducesResponseType(typeof(MerchantDDLModel), 200)]
        [ProducesResponseType(typeof(string), 404)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(string), 500)]
        public IActionResult GetMerchantDDL()
        {
            ErrorResponseModel errorResponseModel = null;
            try
            {
                var categoryData = adminDashboardService.GetMerchantDDL(ref errorResponseModel);
                if (categoryData != null)
                {
                    return Ok(new APIResponse
                    {
                        ResponseStatusCode = 200,
                        ResponseData = categoryData,
                    });
                }
                else
                {
                    return Ok(new APIResponse
                    {
                        ResponseStatusCode = 212,//Data not found
                        ResponseMessage = "Data not found",
                        ResponseData = new CategoryModel(),
                    });
                }

            }
            catch (Exception ex)
            {
                //return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
                return Ok(new APIResponse
                {
                    ResponseStatusCode = StatusCodes.Status500InternalServerError,//Data not found
                    ResponseMessage = "Error",
                    ErrorMessage = ex.Message,
                    ResponseData = new CategoryModel(),
                });
            }
        }
    }
}
