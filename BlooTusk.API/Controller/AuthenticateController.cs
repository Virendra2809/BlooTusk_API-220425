using BlooTusk.Model.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System;
using BlooTusk.Business.Interface;
using BlooTusk.Common;
using Microsoft.Extensions.Configuration;
using System.Text.Json;
using BlooTusk.Business.Implementation;
using BlooTusk.Entity.BlooTuskModel;

namespace BlooTusk.API.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticateController : BaseAPIController
    {
        BlooTuskContext context;

        public IAuthenticateService authenticateService;
        public IAuditlogService auditLogService;
        IConfiguration configuration;

        public AuthenticateController(IAuthenticateService _authenticateService, IAuditlogService _auditLogService, IConfiguration _configuration)
        {
            authenticateService = _authenticateService; 
            auditLogService = _auditLogService;
            configuration = _configuration;
        }


        [AllowAnonymous]
        [HttpPost("AdminLogin")]
        public IActionResult AdminLogin([FromBody] LoginModel model)
        {
            AuditlogModel auditlogModel = new AuditlogModel();
            auditlogModel.RequestedApiname = "AdminLogin";
            auditlogModel.InputData = JsonSerializer.Serialize<LoginModel>(model);
            try
            {
                ErrorResponseModel errorResponseModel = null;
                if (!ModelState.IsValid)
                {
                    var errorMessage = string.Join(",", ModelState.Values.ToList());
                    return BadRequest(new { message = errorMessage });
                }
                var authData = authenticateService.AuthenticateAdminUser(model, ref errorResponseModel);
                if (authData != null)
                {
                    var authClaims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, authData.AdminID.ToString()),
                       // new Claim(ClaimTypes.Role, authData.RoleName),
                    };

                    var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JWT:Secret"]));
                    var key = Encoding.ASCII.GetBytes(GlobalConstants.AuthKey);

                    var token = new JwtSecurityToken(
                    issuer: configuration["JWT:ValidIssuer"],
                    audience: configuration["JWT:ValidAudience"],
                    expires: DateTime.Now.AddDays(2),
                    claims: authClaims,
                    signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256Signature)
                    );

                    authData.Token = new JwtSecurityTokenHandler().WriteToken(token);

                    auditlogModel.RequestStatus = "Sucess";
                    auditlogModel.CreatedDate = DateTime.Now;
                    auditLogService.AddLog(auditlogModel);

                    return Ok(new APIResponse
                    {
                        ResponseStatusCode = 200,
                        ResponseMessage = "Admin Login Sucessfully",
                        ResponseData = authData
                    });

                }
                else
                {
                    auditlogModel.RequestStatus = "Success";
                    auditlogModel.CreatedDate = DateTime.Now;
                    auditLogService.AddLog(auditlogModel);
                    return Ok(new APIResponse
                    {
                        ResponseStatusCode = 212,
                        ResponseMessage = "Invalid username or password",
                        ResponseData = authData
                    });
                }


               // return ReturnErrorResponse(errorResponseModel);
            }
            catch (Exception ex)
            {
               
               
                auditlogModel.RequestStatus = "Failed";
                auditlogModel.ErrorMessage = ex.Message;
                auditlogModel.CreatedDate = DateTime.Now;
                auditLogService.AddLog(auditlogModel);

                return Ok(new APIResponse
                {
                    ResponseStatusCode = StatusCodes.Status500InternalServerError,
                    ResponseMessage = "Fail to Login",
                    ErrorMessage= ex.Message,
                    ResponseData = new AdminAuthenticateModel()
                });
               // return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [AllowAnonymous]
        [HttpPost("MerchantLogin")]
        public IActionResult MerchantLogin([FromBody] LoginModel model)
        {
            AuditlogModel auditlogModel = new AuditlogModel();
            auditlogModel.RequestedApiname = "MerchantLogin";
            auditlogModel.InputData = JsonSerializer.Serialize<LoginModel>(model);
            try
            {
                ErrorResponseModel errorResponseModel = null;
                if (!ModelState.IsValid)
                {
                    var errorMessage = string.Join(",", ModelState.Values.ToList());
                    return BadRequest(new { message = errorMessage });
                }
                var authData = authenticateService.AuthenticateMerchantSystemUser(model, ref errorResponseModel);
                if (authData != null)
                {
                    var authClaims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, authData.MerchantID.ToString()),
                       // new Claim(ClaimTypes.Role, authData.RoleName),
                    };

                    var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JWT:Secret"]));
                    var key = Encoding.ASCII.GetBytes(GlobalConstants.AuthKey);

                    var token = new JwtSecurityToken(
                    issuer: configuration["JWT:ValidIssuer"],
                    audience: configuration["JWT:ValidAudience"],
                    expires: DateTime.Now.AddDays(2),
                    claims: authClaims,
                    signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256Signature)
                    );

                    authData.Token = new JwtSecurityTokenHandler().WriteToken(token);

                    auditlogModel.RequestStatus = "Sucess";
                    auditlogModel.CreatedDate = DateTime.Now;
                    auditLogService.AddLog(auditlogModel);

                    if(authData.ApprovalStatus == "V")
                    {
                        return Ok(new APIResponse
                        {
                            ResponseStatusCode = 200,
                            ResponseMessage = "Merchant Login Sucessfully",
                            ResponseData = authData
                        });

                    }
                    else
                    {
                        return Ok(new APIResponse
                        {
                            ResponseStatusCode = 212,
                            ResponseMessage = "The account is pending approval from the blootusk admin",
                            ResponseData = null
                        });

                    }
                }
                else
                {

                    auditlogModel.RequestStatus = "Sucess";
                    auditlogModel.CreatedDate = DateTime.Now;
                    auditLogService.AddLog(auditlogModel);
                    return Ok(new APIResponse
                    {
                        ResponseStatusCode = 212,
                        ResponseMessage = "Invalid username or password",
                        ResponseData = authData
                    });
                }

                // return ReturnErrorResponse(errorResponseModel);
            }
            catch (Exception ex)
            {  
                auditlogModel.RequestStatus = "Failed";
                auditlogModel.ErrorMessage = ex.Message;
                auditlogModel.CreatedDate = DateTime.Now;
                auditLogService.AddLog(auditlogModel);
                return Ok(new APIResponse
                {
                    ResponseStatusCode = StatusCodes.Status500InternalServerError,
                    ResponseMessage = "Invalid username or password",
                    ErrorMessage = ex.Message,
                    ResponseData = new AdminAuthenticateModel()
                });
                // return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }


        [AllowAnonymous]
        [HttpPost("CustomerLogin")]
        public IActionResult CustomerLogin([FromBody] CustLoginModel model)
        {
            AuditlogModel auditlogModel = new AuditlogModel();
            auditlogModel.RequestedApiname = "CustomerLogin";
            auditlogModel.InputData = JsonSerializer.Serialize<CustLoginModel>(model);
            try
            {
                ErrorResponseModel errorResponseModel = null;
                if (!ModelState.IsValid)
                {
                    var errorMessage = string.Join(",", ModelState.Values.ToList());
                    return BadRequest(new { message = errorMessage });
                }
                var authData = authenticateService.AuthenticateCustomer(model, ref errorResponseModel);
                if (authData != null)
                {
                    var authClaims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, authData.CustomerID.ToString()),                      
                    };
                    var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JWT:Secret"]));
                    var key = Encoding.ASCII.GetBytes(GlobalConstants.AuthKey);
                    var token = new JwtSecurityToken(
                    issuer: configuration["JWT:ValidIssuer"],
                    audience: configuration["JWT:ValidAudience"],
                    expires: DateTime.Now.AddDays(2),
                    claims: authClaims,
                    signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256Signature)
                    );

                    authData.Token = new JwtSecurityTokenHandler().WriteToken(token);

                    auditlogModel.RequestStatus = "Sucess";
                    auditlogModel.CreatedDate = DateTime.Now;
                    auditLogService.AddLog(auditlogModel);

                    return Ok(new APIResponse
                    {
                        ResponseStatusCode = 200,
                        ResponseMessage = "Admin Login Sucessfully",
                        ResponseData = authData
                    });

                }
                else
                {
                    auditlogModel.RequestStatus = "Success";
                    auditlogModel.CreatedDate = DateTime.Now;
                    auditLogService.AddLog(auditlogModel);
                    return Ok(new APIResponse
                    {
                        ResponseStatusCode = 212,
                        ResponseMessage = "Invalid username or password",
                        ResponseData = authData
                    });
                }

                // return ReturnErrorResponse(errorResponseModel);
            }
            catch (Exception ex)
            {

                auditlogModel.RequestStatus = "Failed";
                auditlogModel.ErrorMessage = ex.Message;
                auditlogModel.CreatedDate = DateTime.Now;
                auditLogService.AddLog(auditlogModel);

                return Ok(new APIResponse
                {
                    ResponseStatusCode = StatusCodes.Status500InternalServerError,
                    ResponseMessage = "Fail to Login",
                    ErrorMessage = ex.Message,
                    ResponseData = new AdminAuthenticateModel()
                });
                // return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpPost("OTPVerification")]
        //[Authorize]
        [ProducesResponseType(typeof(OTPModel), 200)]
        [ProducesResponseType(typeof(string), 404)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(string), 500)]
        public IActionResult SendOTP([FromBody] OTPModel otpModel)
        {
            try
            {
                var errorMessage = new ErrorResponseModel();             

                var Model = authenticateService.SendOTP(otpModel, ref errorMessage);
                if (Model == null)
                {
                    return Ok(new APIResponse
                    {
                        ResponseStatusCode = GlobalConstants.ErrorStatus,
                        ResponseData = Model,
                    });
                }
                else
                {
                    return Ok(new APIResponse
                    {
                        ResponseStatusCode = GlobalConstants.OKStatus,
                        ResponseMessage = "Verification code sent to register mobile number",
                        ResponseData = Model,
                    });
                }

            }
            catch (Exception ex)
            {
                Errorlog errorlog = new Errorlog();
                errorlog.ErrorLog1 = "Error In sending sms to " + otpModel.PhoneNumber + " Error Message is " + ex.Message;
                context.Errorlogs.Add(errorlog);
                context.SaveChanges();
                return Ok(new APIResponse
                {
                    ResponseStatusCode = GlobalConstants.ErrorStatus,
                    ResponseMessage = "Fail to send verification code",
                    ErrorMessage = ex.Message,
                    ResponseData = null,
                });
            }

        }


    }
}
