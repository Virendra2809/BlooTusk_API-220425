using BlooTusk.Business.Interface;
using BlooTusk.Common;
using BlooTusk.Entity.BlooTuskModel;
using BlooTusk.Model.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Configuration;
using System;
using BlooTusk.Business.Implementation;
using System.Text.Json;

namespace BlooTusk.API.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class SignupRequestController : BaseAPIController
    {
        ISignupRequestService signupRequestService;
        public IAuditlogService auditLogService;
        public SignupRequestController(ISignupRequestService _signupRequestService, IAuditlogService _auditLogService )
        {
            this.signupRequestService = _signupRequestService;
            this.auditLogService = _auditLogService;
        }

        [HttpPost("AddSignupRequest")]
        //[Authorize]
        [ProducesResponseType(typeof(SignupRequestModel), 200)]
        [ProducesResponseType(typeof(string), 404)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(string), 500)]
        public IActionResult AddEditSignupRequest([FromBody] SignupRequestModel signupRequestModel)
        {
            AuditlogModel auditlogModel = new AuditlogModel();
            auditlogModel.RequestedApiname = "AddSignupRequest";
            auditlogModel.InputData = JsonSerializer.Serialize<SignupRequestModel>(signupRequestModel);
            try
            {
                var errorMessage = new ErrorResponseModel();
                bool Model = signupRequestService.AddSignupRequest(signupRequestModel, ref errorMessage);
                if (!Model)
                {
                    auditlogModel.RequestStatus = "Failed";
                    auditlogModel.CreatedDate = DateTime.Now;
                    auditLogService.AddLog(auditlogModel);
                    return Ok(new APIResponse
                    {
                        ResponseStatusCode = GlobalConstants.BadRequestStatus,
                        ResponseMessage = "Fail to add signup request",
                        ErrorMessage = errorMessage.Message,
                    });
                }
                else
                {
                    auditlogModel.RequestStatus = "Sucess";
                    auditlogModel.CreatedDate = DateTime.Now;
                    auditLogService.AddLog(auditlogModel);
                    return Ok(new APIResponse
                    {
                        ResponseStatusCode = GlobalConstants.OKStatus,
                        ResponseMessage = "Signup request add sucessfully",
                    });
                }
            }
            catch (Exception ex)
            {
                auditlogModel.RequestStatus = "Failed";
                auditlogModel.CreatedDate = DateTime.Now;
                auditlogModel.ErrorMessage = ex.Message;
                auditLogService.AddLog(auditlogModel);
                return Ok(new APIResponse
                {
                    ResponseStatusCode = GlobalConstants.ErrorStatus,
                    ResponseMessage = "Fail to add signup request",
                    ErrorMessage = ex.Message,
                    ResponseData = new AdminAuthenticateModel()
                });
            }

        }
    }
}
