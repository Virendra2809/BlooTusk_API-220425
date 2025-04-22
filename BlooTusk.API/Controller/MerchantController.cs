using BlooTusk.Business.Implementation;
using BlooTusk.Business.Interface;
using BlooTusk.Common;
using BlooTusk.Entity.BlooTuskModel;
using BlooTusk.Model;
using BlooTusk.Model.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Reflection.Emit;
using System.Text;
using System.Text.Json;

namespace BlooTusk.API.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class MerchantController : BaseAPIController
    {
        public IMerchantService merchantService;
        public IAuditlogService auditLogService;
        public MerchantController(IMerchantService _merchantService, IAuditlogService _auditLogService)
        {
            this.merchantService = _merchantService;
            auditLogService = _auditLogService;
        }

        [HttpPost("AddMerchant")]
        //[Authorize]
        [ProducesResponseType(typeof(MerchantModel), 200)]
        [ProducesResponseType(typeof(string), 404)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(string), 500)]
        public IActionResult AddMerchant([FromBody] MerchantModel merchantModel)
        {
            AuditlogModel auditlogModel = new AuditlogModel();
            auditlogModel.RequestedApiname = "AddMerchant";
            auditlogModel.InputData = JsonSerializer.Serialize<MerchantModel>(merchantModel);
            try
            {
                var errorMessage = new ErrorResponseModel();
                var IsDuplicateEmail = merchantService.IsDuplicateEmail(merchantModel.Email);
                if (IsDuplicateEmail)
                {
                    auditlogModel.RequestStatus = "Duplicate Email";
                    auditlogModel.CreatedDate = DateTime.Now;
                    auditLogService.AddLog(auditlogModel);
                    return Ok(new APIResponse
                    {
                        ResponseStatusCode = GlobalConstants.DuplicateEmailStatus,
                        ResponseMessage = GlobalConstants.DuplicateEmail,
                    });
                }
                var IsDuplicatePhone = merchantService.IsDuplicatePhoneNumber(merchantModel.PhoneNumber);
                if (IsDuplicatePhone)
                {
                    auditlogModel.RequestStatus = "Duplicate number";
                    auditlogModel.CreatedDate = DateTime.Now;
                    auditLogService.AddLog(auditlogModel);
                    return Ok(new APIResponse
                    {
                        ResponseStatusCode = GlobalConstants.DuplicatePhoneNUmberStatus,
                        ResponseMessage = GlobalConstants.DuplicatePhoneNumber,
                    });
                }
                Boolean Model = merchantService.AddMerchant(merchantModel, ref errorMessage);
                if (!Model)
                {
                    auditlogModel.RequestStatus = "Failure";
                    auditlogModel.CreatedDate = DateTime.Now;
                    auditLogService.AddLog(auditlogModel);
                    return Ok(new APIResponse
                    {
                        ResponseStatusCode = GlobalConstants.BadRequestStatus,
                        ResponseMessage = "Fail to add merchand",
                        ErrorMessage = errorMessage.Message,
                    });
                }
                else
                {
                    auditlogModel.RequestStatus = "success";
                    auditlogModel.CreatedDate = DateTime.Now;
                    auditLogService.AddLog(auditlogModel);
                    return Ok(new APIResponse
                    {
                        ResponseStatusCode = GlobalConstants.OKStatus,
                        ResponseMessage = GlobalConstants.MerchantCreateMessage,
                    });
                }
            }
            catch (Exception ex)
            {
                auditlogModel.RequestStatus = "Failure";
                auditlogModel.CreatedDate = DateTime.Now;
                auditlogModel.ErrorMessage = ex.Message;
                auditLogService.AddLog(auditlogModel);
                return Ok(new APIResponse
                {
                    ResponseStatusCode = GlobalConstants.ErrorStatus,
                    ResponseMessage = "Fail to Login",
                    ErrorMessage = ex.Message,
                    ResponseData = new AdminAuthenticateModel()
                });
            }

        }

        [HttpPost("EditMerchant")]
        [Authorize]
        [ProducesResponseType(typeof(MerchantModel), 200)]
        [ProducesResponseType(typeof(string), 404)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(string), 500)]
        public IActionResult EditMerchant([FromBody] MerchantModel merchantModel)
        {
            AuditlogModel auditlogModel = new AuditlogModel();
            auditlogModel.RequestedApiname = "EditMerchant";
            auditlogModel.InputData = JsonSerializer.Serialize<MerchantModel>(merchantModel);
            try
            {
                var errorMessage = new ErrorResponseModel();
                Boolean Model = merchantService.EditMerchant(merchantModel, ref errorMessage);
                if (!Model)
                {
                    auditlogModel.RequestStatus = "Failure";
                    auditlogModel.CreatedDate = DateTime.Now;
                    auditLogService.AddLog(auditlogModel);
                    return Ok(new APIResponse
                    {
                        ResponseStatusCode = GlobalConstants.BadRequestStatus,
                        ResponseMessage = "Fail to edit merchand",
                        ErrorMessage = errorMessage.Message,
                    });
                }
                else
                {
                    auditlogModel.RequestStatus = "success";
                    auditlogModel.CreatedDate = DateTime.Now;
                    auditLogService.AddLog(auditlogModel);
                    return Ok(new APIResponse
                    {
                        ResponseStatusCode = GlobalConstants.OKStatus,
                        ResponseMessage = GlobalConstants.MerchantUpdateMessage,
                    });
                }
            }
            catch (Exception ex)
            {
                auditlogModel.RequestStatus = "Failure";
                auditlogModel.CreatedDate = DateTime.Now;
                auditlogModel.ErrorMessage = ex.Message;
                auditLogService.AddLog(auditlogModel);
                return Ok(new APIResponse
                {
                    ResponseStatusCode = GlobalConstants.ErrorStatus,
                    ResponseMessage = "Fail to Login",
                    ErrorMessage = ex.Message,
                    ResponseData = new AdminAuthenticateModel()
                });
            }

        }


        [HttpGet("DeleteMerchant/{merchantId}")]
        [Authorize]
        [ProducesResponseType(typeof(MerchantModel), 200)]
        [ProducesResponseType(typeof(string), 404)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(string), 500)]
        public IActionResult DeleteMerchant(int merchantId)
        {
            AuditlogModel auditlogModel = new AuditlogModel();
            auditlogModel.RequestedApiname = "DeleteMerchant";
            auditlogModel.InputData = "merchantId : "+merchantId;
            try
            {
                var errorMessage = new ErrorResponseModel();
                Boolean Model = merchantService.DeleteMerchant(merchantId, ref errorMessage);
                if (!Model)
                {
                    auditlogModel.RequestStatus = "Failure";
                    auditlogModel.CreatedDate = DateTime.Now;
                    auditLogService.AddLog(auditlogModel);
                    return Ok(new APIResponse
                    {
                        ResponseStatusCode = GlobalConstants.BadRequestStatus,
                        ResponseMessage = "Fail to delete merchand",
                        ErrorMessage = errorMessage.Message,
                    });
                }
                else
                {
                    auditlogModel.RequestStatus = "success";
                    auditlogModel.CreatedDate = DateTime.Now;
                    auditLogService.AddLog(auditlogModel);
                    return Ok(new APIResponse
                    {
                        ResponseStatusCode = GlobalConstants.OKStatus,
                        ResponseMessage = GlobalConstants.MerchantDeleteMessage,
                    });
                }
            }
            catch (Exception ex)
            {
                auditlogModel.RequestStatus = "Failure";
                auditlogModel.CreatedDate = DateTime.Now;
                auditlogModel.ErrorMessage = ex.Message;
                auditLogService.AddLog(auditlogModel);
                return Ok(new APIResponse
                {
                    ResponseStatusCode = GlobalConstants.ErrorStatus,
                    ResponseMessage = "Fail to Login",
                    ErrorMessage = ex.Message,
                    ResponseData = new AdminAuthenticateModel()
                });
            }

        }

        [HttpGet("GetMerchantById/{merchantId}")]
        [Authorize]
        [ProducesResponseType(typeof(MerchantModel), 200)]
        [ProducesResponseType(typeof(string), 404)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(string), 500)]
        public IActionResult GetMerchantById(int merchantId)
        {
            AuditlogModel auditlogModel = new AuditlogModel();
            auditlogModel.RequestedApiname = "GetMerchantById";
            auditlogModel.InputData = "merchantId : " + merchantId;
            try
            {
                var errorMessage = new ErrorResponseModel();
                var merchantModel = merchantService.GetMerchantById(merchantId, ref errorMessage);
                if (merchantModel == null)
                {
                    auditlogModel.RequestStatus = "No data found";
                    auditlogModel.CreatedDate = DateTime.Now;
                    auditLogService.AddLog(auditlogModel);
                    return Ok(new APIResponse
                    {
                        ResponseStatusCode = GlobalConstants.NoDataStatus,
                        ResponseMessage = "No data found",
                    });
                }
                else
                {
                    auditlogModel.RequestStatus = "success";
                    auditlogModel.CreatedDate = DateTime.Now;
                    auditLogService.AddLog(auditlogModel);
                    return Ok(new APIResponse
                    {
                        ResponseStatusCode = GlobalConstants.OKStatus,
                        ResponseData=merchantModel,
                    });
                }
            }
            catch (Exception ex)
            {
                auditlogModel.RequestStatus = "Failure";
                auditlogModel.CreatedDate = DateTime.Now;
                auditlogModel.ErrorMessage = ex.Message;
                auditLogService.AddLog(auditlogModel);
                return Ok(new APIResponse
                {
                    ResponseStatusCode = GlobalConstants.ErrorStatus,
                    ResponseMessage = "Fail to get merchant data",
                    ErrorMessage = ex.Message,
                    ResponseData = new AdminAuthenticateModel()
                });
            }

        }

        [HttpPost("GetPosDetail/{merchantId}")]
        [Authorize]
        [ProducesResponseType(typeof(MerchantModel), 200)]
        [ProducesResponseType(typeof(string), 404)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(string), 500)]

        public IActionResult GetPosDetail(int merchantId)
        {
            AuditlogModel auditlogModel = new AuditlogModel();
            auditlogModel.RequestedApiname = "GetPosDetail";
            auditlogModel.InputData = "merchantId : " + merchantId;
            try
            {
                var errorMessage = new ErrorResponseModel();
                var posModel = merchantService.GetPosDetails(merchantId, ref errorMessage);
                if (posModel == null)
                {
                    auditlogModel.RequestStatus = "No data found";
                    auditlogModel.CreatedDate = DateTime.Now;
                    auditLogService.AddLog(auditlogModel);
                    return Ok(new APIResponse
                    {
                        ResponseStatusCode = GlobalConstants.NoDataStatus,
                        ResponseMessage = "No data found",
                    });
                }
                else
                {
                    auditlogModel.RequestStatus = "success";
                    auditlogModel.CreatedDate = DateTime.Now;
                    auditLogService.AddLog(auditlogModel);
                    return Ok(new APIResponse
                    {
                        ResponseStatusCode = GlobalConstants.OKStatus,
                        ResponseData = posModel,
                    });
                }
            }
            catch (Exception ex)
            {
                auditlogModel.RequestStatus = "Failure";
                auditlogModel.CreatedDate = DateTime.Now;
                auditlogModel.ErrorMessage = ex.Message;
                auditLogService.AddLog(auditlogModel);
                return Ok(new APIResponse
                {
                    ResponseStatusCode = GlobalConstants.ErrorStatus,
                    ResponseMessage = "Fail to get merchant data",
                    ErrorMessage = ex.Message,
                    ResponseData = new AdminAuthenticateModel()
                });
            }

        }




        [HttpPost("GeProfile/{merchantId}")]
        [Authorize]
        [ProducesResponseType(typeof(MerchantModel), 200)]
        [ProducesResponseType(typeof(string), 404)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(string), 500)]
        public IActionResult GetProfile(int merchantId)
        {
            AuditlogModel auditlogModel = new AuditlogModel();
            auditlogModel.RequestedApiname = "GeProfile";
            auditlogModel.InputData = "merchantId : " + merchantId;
            try
            {
                var errorMessage = new ErrorResponseModel();
                var merchantModel = merchantService.GetProfile(merchantId, ref errorMessage);
                if (merchantModel == null)
                {
                    auditlogModel.RequestStatus = "No data found";
                    auditlogModel.CreatedDate = DateTime.Now;
                    auditLogService.AddLog(auditlogModel);
                    return Ok(new APIResponse
                    {
                        ResponseStatusCode = GlobalConstants.NoDataStatus,
                        ResponseMessage = "No data found",
                    });
                }
                else
                {
                    auditlogModel.RequestStatus = "success";
                    auditlogModel.CreatedDate = DateTime.Now;
                    auditLogService.AddLog(auditlogModel);
                    return Ok(new APIResponse
                    {
                        ResponseStatusCode = GlobalConstants.OKStatus,
                        ResponseData = merchantModel,
                    });
                }
            }
            catch (Exception ex)
            {
                auditlogModel.RequestStatus = "Failure";
                auditlogModel.CreatedDate = DateTime.Now;
                auditlogModel.ErrorMessage = ex.Message;
                auditLogService.AddLog(auditlogModel);
                return Ok(new APIResponse
                {
                    ResponseStatusCode = GlobalConstants.ErrorStatus,
                    ResponseMessage = "Fail to get merchant data",
                    ErrorMessage = ex.Message,
                    ResponseData = new AdminAuthenticateModel()
                });
            }

        }

        [HttpPost("UpdateProfile")]
        [Authorize]
        [ProducesResponseType(typeof(MerchantModel), 200)]
        [ProducesResponseType(typeof(string), 404)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(string), 500)]
        public IActionResult UpdateProfile([FromBody] ProfileModel profileModel)
        {
            AuditlogModel auditlogModel = new AuditlogModel();
            auditlogModel.RequestedApiname = "UpdateProfile";
            auditlogModel.InputData = JsonSerializer.Serialize<ProfileModel>(profileModel);
            try
            {
                var errorMessage = new ErrorResponseModel();
                Boolean Model = merchantService.UpdateProfile(profileModel, ref errorMessage);
                if (!Model)
                {
                    auditlogModel.RequestStatus = "Failure";
                    auditlogModel.CreatedDate = DateTime.Now;
                    auditLogService.AddLog(auditlogModel);
                    return Ok(new APIResponse
                    {
                        ResponseStatusCode = GlobalConstants.BadRequestStatus,
                        ResponseMessage = "Fail to edit merchand",
                        ErrorMessage = errorMessage.Message,
                    });
                }
                else
                {
                auditlogModel.RequestStatus = "success";
                auditlogModel.CreatedDate = DateTime.Now;
                auditLogService.AddLog(auditlogModel);
                return Ok(new APIResponse
                    {
                        ResponseStatusCode = GlobalConstants.OKStatus,
                        ResponseMessage = GlobalConstants.MerchantUpdateMessage,
                    });
                }
            }
            catch (Exception ex)
            {
                auditlogModel.RequestStatus = "Failure";
                auditlogModel.CreatedDate = DateTime.Now;
                auditlogModel.ErrorMessage = ex.Message;
                auditLogService.AddLog(auditlogModel);
                return Ok(new APIResponse
                {
                    ResponseStatusCode = GlobalConstants.ErrorStatus,
                    ResponseMessage = "Fail to Login",
                    ErrorMessage = ex.Message,
                    ResponseData = new AdminAuthenticateModel()
                });
            }

        }


        /// <summary>
        /// To get categoryDDL  
        /// </summary>
        /// <returns></returns>
        [HttpPost("GetAllMerchant")]
        [Authorize]
        [ProducesResponseType(typeof(MerchantModel), 200)]
        [ProducesResponseType(typeof(string), 404)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(string), 500)]
        public IActionResult GetAllMerchant(MerchantSerachModel merchantSerachModel)
        {
            AuditlogModel auditlogModel = new AuditlogModel();
            auditlogModel.RequestedApiname = "GetAllMerchant";
            auditlogModel.InputData = JsonSerializer.Serialize<MerchantSerachModel>(merchantSerachModel);
            ErrorResponseModel errorResponseModel = null;
            try
            {
                var merchantData = merchantService.GetAllMerchant(merchantSerachModel,ref errorResponseModel);
                if (merchantData != null)
                {
                    auditlogModel.RequestStatus = "success";
                    auditlogModel.CreatedDate = DateTime.Now;
                    auditLogService.AddLog(auditlogModel);
                    return Ok(new APIResponse
                    {
                        ResponseStatusCode = GlobalConstants.OKStatus, 
                        ResponseData = merchantData,
                    });
                }
                else
                {
                    auditlogModel.RequestStatus = "Data not found";
                    auditlogModel.CreatedDate = DateTime.Now;
                    auditLogService.AddLog(auditlogModel);
                    return Ok(new APIResponse
                    {
                        ResponseStatusCode = GlobalConstants.NoDataStatus,//Data not found
                        ResponseMessage = GlobalConstants.DataNotFound,
                        ResponseData = new List<MerchantModel>(),
                    });
                }

            }
            catch (Exception ex)
            {
                auditlogModel.RequestStatus = "Failure";
                auditlogModel.CreatedDate = DateTime.Now;
                auditlogModel.ErrorMessage = ex.Message;
                auditLogService.AddLog(auditlogModel);
                return Ok(new APIResponse
                {
                    ResponseStatusCode = GlobalConstants.ErrorStatus,//Data not found
                    ResponseMessage = "Error",
                    ErrorMessage = ex.Message,
                    ResponseData = new List<MerchantModel>(),
                });
            }
        }

        [HttpPost("SendOTP")]
        //[Authorize]
        [ProducesResponseType(typeof(OTPModel), 200)]
        [ProducesResponseType(typeof(string), 404)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(string), 500)]
        public IActionResult SendOTP([FromBody] OTPModel otpModel)
        {
            AuditlogModel auditlogModel = new AuditlogModel();
            auditlogModel.RequestedApiname = "SendOTP";
            auditlogModel.InputData = JsonSerializer.Serialize<OTPModel>(otpModel);
            try
            {
                var errorMessage = new ErrorResponseModel();

                var IsDuplicateEmail=merchantService.IsDuplicateEmail(otpModel.Email);
                if (IsDuplicateEmail)
                {
                    auditlogModel.RequestStatus = "Duplicate email";
                    auditlogModel.CreatedDate = DateTime.Now;
                    auditlogModel.RequestedApiname = "SendOTP";
                    auditLogService.AddLog(auditlogModel);
                    return Ok(new APIResponse
                    {
                        ResponseStatusCode = GlobalConstants.DuplicateEmailStatus,
                        ResponseMessage= GlobalConstants.DuplicateEmail,
                    });
                }

                var IsDuplicatePhone = merchantService.IsDuplicatePhoneNumber(otpModel.PhoneNumber);
                if (IsDuplicatePhone)
                {
                    auditlogModel.RequestStatus = "Duplicate number";
                    auditlogModel.ErrorMessage = "Duplicate number";
                    auditlogModel.CreatedDate = DateTime.Now;
                    auditLogService.AddLog(auditlogModel);
                    return Ok(new APIResponse
                    {
                        ResponseStatusCode = GlobalConstants.DuplicatePhoneNUmberStatus,
                        ResponseMessage = GlobalConstants.DuplicatePhoneNumber,
                    });
                }


                var Model = merchantService.SendOTP(otpModel, ref errorMessage);
                if (Model==null)
                {
                    auditlogModel.RequestStatus = "Failure";
                    auditlogModel.CreatedDate = DateTime.Now;
                    auditLogService.AddLog(auditlogModel);
                    return Ok(new APIResponse
                    {
                        ResponseStatusCode = StatusCodes.Status500InternalServerError,
                       
                        ResponseData=Model,
                    });
                }
                else
                {
                    auditlogModel.RequestStatus = "success";
                    auditlogModel.CreatedDate = DateTime.Now;
                    auditLogService.AddLog(auditlogModel);
                    return Ok(new APIResponse
                    {
                        ResponseStatusCode = GlobalConstants.OKStatus,
                        ResponseData = Model,
                    });
                }

            }
            catch (Exception ex)
            {
                auditlogModel.RequestStatus = "Failure";
                auditlogModel.CreatedDate = DateTime.Now;
                auditlogModel.ErrorMessage = ex.Message;
                auditLogService.AddLog(auditlogModel);
                return Ok(new APIResponse
                {
                    ResponseStatusCode =GlobalConstants.ErrorStatus,
                    ResponseMessage = "Fail to send otp",
                    ErrorMessage = ex.Message,
                    ResponseData = null,
                }) ;
            }

        }

        /// <summary>
        /// To get Country   DDL
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetCountryDDL")]
        [ProducesResponseType(typeof(CountryModel), 200)]
        [ProducesResponseType(typeof(string), 404)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(string), 500)]
        public IActionResult GetCountryDDL()
        {
            AuditlogModel auditlogModel = new AuditlogModel();
            auditlogModel.RequestedApiname = "GetCountryDDL";
            auditlogModel.InputData = "";

            ErrorResponseModel errorResponseModel = null;
            try
            {
                var countryData = merchantService.GetCountryDDL(ref errorResponseModel);
                if (countryData != null)
                {
                    auditlogModel.RequestStatus = "success";
                    auditlogModel.CreatedDate = DateTime.Now;
                    auditLogService.AddLog(auditlogModel);
                    return Ok(new APIResponse
                    {
                        ResponseStatusCode = GlobalConstants.OKStatus,
                        ResponseData = countryData,
                    });
                }
                else
                {
                    auditlogModel.RequestStatus = "Failure";
                    auditlogModel.CreatedDate = DateTime.Now;
                    auditLogService.AddLog(auditlogModel);
                    return Ok(new APIResponse
                    {
                        ResponseStatusCode = GlobalConstants.NoDataStatus,//Data not found
                        ResponseMessage = GlobalConstants.DataNotFound,
                        ResponseData = new CategoryModel(),
                    });
                }

            }
            catch (Exception ex)
            {
                auditlogModel.RequestStatus = "Failure";
                auditlogModel.CreatedDate = DateTime.Now;
                auditlogModel.ErrorMessage = ex.Message;
                auditLogService.AddLog(auditlogModel);
                
                return Ok(new APIResponse
                {
                    ResponseStatusCode = GlobalConstants.ErrorStatus,//Data not found
                    ResponseMessage = "Error",
                    ErrorMessage = ex.Message,
                    ResponseData = new CategoryModel(),
                });
            }
        }


        /// <summary>
        /// To get categoryDDL  
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetStateDDL")]
        [ProducesResponseType(typeof(StateModel), 200)]
        [ProducesResponseType(typeof(string), 404)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(string), 500)]
        public IActionResult GetCategoryDDL()
        {
            AuditlogModel auditlogModel = new AuditlogModel();
            auditlogModel.RequestedApiname = "GetStateDDL";
            auditlogModel.InputData = "";
            ErrorResponseModel errorResponseModel = null;
            try
            {
                var categoryData = merchantService.GetStateDDL(ref errorResponseModel);
                if (categoryData != null)
                {
                    auditlogModel.RequestStatus = "success";
                    auditlogModel.CreatedDate = DateTime.Now;
                    auditLogService.AddLog(auditlogModel);
                    return Ok(new APIResponse
                    {
                        ResponseStatusCode = GlobalConstants.OKStatus,
                        ResponseData = categoryData,
                    });
                }
                else
                {
                    auditlogModel.RequestStatus = "Data not found";
                    auditlogModel.CreatedDate = DateTime.Now;
                    auditLogService.AddLog(auditlogModel);
                    return Ok(new APIResponse
                    {
                        ResponseStatusCode = GlobalConstants.NoDataStatus,//Data not found
                        ResponseMessage = GlobalConstants.DataNotFound,
                        ResponseData = new CategoryModel(),
                    });
                }

            }
            catch (Exception ex)
            {
                auditlogModel.RequestStatus = "Failure";
                auditlogModel.CreatedDate = DateTime.Now;
                auditlogModel.ErrorMessage = ex.Message;
                auditLogService.AddLog(auditlogModel);
                return Ok(new APIResponse
                {
                    ResponseStatusCode = GlobalConstants.ErrorStatus,//Data not found
                    ResponseMessage = "Error",
                    ErrorMessage = ex.Message,
                    ResponseData = new CategoryModel(),
                });
            }
        }



        /// <summary>
        /// To get category by Id
        /// </summary>
        /// <param name="merchantId"></param>
        /// <returns></returns>
        [HttpGet("GetDashboardData/{merchantId}")]
        [Authorize]
        [ProducesResponseType(typeof(CategoryModel), 200)]
        [ProducesResponseType(typeof(string), 404)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(string), 500)]
        public IActionResult GetDashboardData(int merchantId)
        {
            AuditlogModel auditlogModel = new AuditlogModel();
            auditlogModel.RequestedApiname = "GetDashboardData";
            auditlogModel.InputData = "MerchantID : "+merchantId;
            ErrorResponseModel errorResponseModel = null;
            try
            {
                if (merchantId <= 0)
                {
                    auditlogModel.RequestStatus = "Bad request";
                    auditlogModel.CreatedDate = DateTime.Now;
                    auditLogService.AddLog(auditlogModel);
                    return Ok(new APIResponse
                    {
                        ResponseStatusCode = GlobalConstants.BadRequestStatus,//Bad request
                        ResponseMessage = "Bad request",
                        ResponseData = new DashboardDataModel(),
                    });
                }
                var dashboardData = merchantService.GetDashboardData(merchantId, ref errorResponseModel);

                if (dashboardData != null)
                {
                    auditlogModel.RequestStatus = "success";
                    auditlogModel.CreatedDate = DateTime.Now;
                    auditLogService.AddLog(auditlogModel);
                    return Ok(new APIResponse
                    {
                        ResponseStatusCode = GlobalConstants.OKStatus,
                        ResponseData = dashboardData,
                    });
                }
                else
                {
                    auditlogModel.RequestStatus = "Data not found";
                    auditlogModel.CreatedDate = DateTime.Now;
                    auditLogService.AddLog(auditlogModel);
                    return Ok(new APIResponse
                    {
                        ResponseStatusCode = GlobalConstants.NoDataStatus,//Data not found
                        ResponseMessage = "Data not found",
                        ResponseData = new CategoryModel(),
                    });
                }
            }
            catch (Exception ex)
            {
                auditlogModel.RequestStatus = "Failure";
                auditlogModel.CreatedDate = DateTime.Now;
                auditlogModel.ErrorMessage = ex.Message;
                auditLogService.AddLog(auditlogModel);
                return Ok(new APIResponse
                {
                    ResponseStatusCode = GlobalConstants.ErrorStatus,//Error
                    ResponseMessage = "Error",
                    ErrorMessage = ex.Message,
                    ResponseData = new CategoryModel(),
                });
            }
        }


        /// <summary>
        /// To get category by Id
        /// </summary>
        /// <param name="merchantId"></param>
        /// <returns></returns>
        [HttpGet("GetMarchantByCode/{merchantCode}")]
        //[Authorize]
        [ProducesResponseType(typeof(MerchantModel), 200)]
        [ProducesResponseType(typeof(string), 404)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(string), 500)]
        public IActionResult GetMarchantByCode(string merchantCode,bool isMerchant)
        {
            AuditlogModel auditlogModel = new AuditlogModel();
            auditlogModel.RequestedApiname = "GetMarchantByCode";
            auditlogModel.InputData = "merchantCode : "+ merchantCode;
            ErrorResponseModel errorResponseModel = null;
            try
            {  
                var merchantData = merchantService.GetMerchantByCOde(merchantCode, isMerchant, ref errorResponseModel);

                if (merchantData != null)
                {
                    auditlogModel.RequestStatus = "success";
                    auditlogModel.CreatedDate = DateTime.Now;
                    auditLogService.AddLog(auditlogModel);
                    return Ok(new APIResponse
                    {
                        ResponseStatusCode = GlobalConstants.OKStatus,
                        ResponseData = merchantData,
                    });
                }
                else
                {
                    auditlogModel.RequestStatus = "Data not found";
                    auditlogModel.CreatedDate = DateTime.Now;
                    auditLogService.AddLog(auditlogModel);
                    return Ok(new APIResponse
                    {
                        ResponseStatusCode = GlobalConstants.NoDataStatus,//Data not found
                        ResponseMessage = "Data not found",
                        ResponseData = new MerchantModel(),
                    });
                }
            }
            catch (Exception ex)
            {
                auditlogModel.RequestStatus = "Failure";
                auditlogModel.CreatedDate = DateTime.Now;
                auditlogModel.ErrorMessage = ex.Message;
                auditLogService.AddLog(auditlogModel);
                return Ok(new APIResponse
                {
                    ResponseStatusCode = GlobalConstants.ErrorStatus,//Error
                    ResponseMessage = "Error",
                    ErrorMessage = ex.Message,
                    ResponseData = new MerchantModel(),
                });
            }
        }

        [HttpPost("AddMerchantRemark")]
        [Authorize]
        [ProducesResponseType(typeof(RemarkModel), 200)]
        [ProducesResponseType(typeof(string), 404)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(string), 500)]
        public IActionResult AddMerchantRemark([FromBody] RemarkModel remarkModel)
        {
            AuditlogModel auditlogModel = new AuditlogModel();
            auditlogModel.RequestedApiname = "AddMerchantRemark";
            auditlogModel.InputData = JsonSerializer.Serialize<RemarkModel>(remarkModel);
            try
            {
                var errorMessage = new ErrorResponseModel();
                bool Model = merchantService.AddRemark(remarkModel);
                if (!Model)
                {
                    auditlogModel.RequestStatus = "Failure";
                    auditlogModel.CreatedDate = DateTime.Now;
                    auditLogService.AddLog(auditlogModel);
                    return Ok(new APIResponse
                    {
                        ResponseStatusCode = GlobalConstants.BadRequestStatus,
                        ResponseMessage = "Fail to add remark",
                        ErrorMessage = errorMessage.Message,
                    });
                }
                else
                {
                    auditlogModel.RequestStatus = "success";
                    auditlogModel.CreatedDate = DateTime.Now;
                    auditLogService.AddLog(auditlogModel);
                    return Ok(new APIResponse
                    {
                        ResponseStatusCode = GlobalConstants.OKStatus,
                        ResponseMessage = GlobalConstants.MerchantCreateMessage,
                    });
                }
            }
            catch (Exception ex)
            {
                auditlogModel.RequestStatus = "Failure";
                auditlogModel.CreatedDate = DateTime.Now;
                auditlogModel.ErrorMessage = ex.Message;
                auditLogService.AddLog(auditlogModel);
                return Ok(new APIResponse
                {
                    ResponseStatusCode = GlobalConstants.ErrorStatus,
                    ResponseMessage = "Fail to add remark",
                    ErrorMessage = ex.Message,
                    ResponseData = new AdminAuthenticateModel()
                });
            }

        }


        [HttpGet("GetRemarkHistory/{merchantId}")]
        [Authorize]
        [ProducesResponseType(typeof(RemarkModel), 200)]
        [ProducesResponseType(typeof(string), 404)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(string), 500)]
        public IActionResult GetRemarkHistory(int merchantId)
        {
            AuditlogModel auditlogModel = new AuditlogModel();
            auditlogModel.RequestedApiname = "GetRemarkHistory";
            auditlogModel.InputData = "merchantId : "+merchantId;
            try
            {
                var errorMessage = new ErrorResponseModel();
                var Model = merchantService.GetRemarkHistory(merchantId);
                if (Model==null)
                {
                    auditlogModel.RequestStatus = "No data found";
                    auditlogModel.CreatedDate = DateTime.Now;
                    auditLogService.AddLog(auditlogModel);
                    return Ok(new APIResponse
                    {
                        ResponseStatusCode = GlobalConstants.NoDataStatus,
                        ResponseMessage = "No data found",
                    });
                }
                else
                {
                    auditlogModel.RequestStatus = "success";
                    auditlogModel.CreatedDate = DateTime.Now;
                    auditLogService.AddLog(auditlogModel);
                    return Ok(new APIResponse
                    {
                        ResponseStatusCode = GlobalConstants.OKStatus,
                        ResponseData = Model,
                    });
                }
            }
            catch (Exception ex)
            {
                auditlogModel.RequestStatus = "Failure";
                auditlogModel.CreatedDate = DateTime.Now;
                auditlogModel.ErrorMessage = ex.Message;
                auditLogService.AddLog(auditlogModel);
                return Ok(new APIResponse
                {
                    ResponseStatusCode = GlobalConstants.ErrorStatus,
                    ResponseMessage = "Fail to get remark",
                    ErrorMessage = ex.Message,
                    ResponseData = null,
                });
            }

        }

        /// <summary>
        /// To get category by Id
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        [HttpPost("ForgetPasswordOTP/{username}")]
       // [Authorize]
        [ProducesResponseType(typeof(OTPModel), 200)]
        [ProducesResponseType(typeof(string), 404)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(string), 500)]
        public IActionResult ForgetPasswordOTP(string username)
        {
            AuditlogModel auditlogModel = new AuditlogModel();
            auditlogModel.RequestedApiname = "ForgetPasswordOTP";
            auditlogModel.InputData = "Username : " + username;
            ErrorResponseModel errorResponseModel = null;
            try
            {
                bool isEmailUsername = IsValidEmail(username);
                if (isEmailUsername)
                {
                    var isEmailPresent = merchantService.IsDuplicateEmailStaff(username);
                    if (!isEmailPresent)
                    {
                        auditlogModel.RequestStatus = "Email not exist";
                        auditlogModel.CreatedDate = DateTime.Now;
                        auditLogService.AddLog(auditlogModel);
                        return Ok(new APIResponse
                        {
                            ResponseStatusCode = GlobalConstants.EmailNotPresent,
                            ResponseMessage = GlobalConstants.EmailNotPresentMessage,
                        });
                    }
                }
                else
                {
                    var IsPresentPhone = merchantService.IsDuplicatePhoneNumberStaff(username);
                    if (!IsPresentPhone)
                    {
                        auditlogModel.RequestStatus = "Phone notexist";
                        auditlogModel.CreatedDate = DateTime.Now;
                        auditLogService.AddLog(auditlogModel);
                        return Ok(new APIResponse
                        {
                            ResponseStatusCode = GlobalConstants.PhoneNumberNotPresnt,
                            ResponseMessage = GlobalConstants.PhoneNumberNotPresntMessage,
                        });
                    }
                }

                
                var otpModel = merchantService.ForgetPasswordOTP(username);

                if (otpModel != null)
                {
                    auditlogModel.RequestStatus = "success";
                    auditlogModel.CreatedDate = DateTime.Now;
                    auditLogService.AddLog(auditlogModel);
                    return Ok(new APIResponse
                    {
                        ResponseStatusCode = GlobalConstants.OKStatus,
                        ResponseData = otpModel,
                    });
                }
                else
                {
                    auditlogModel.RequestStatus = "Data not found";
                    auditlogModel.CreatedDate = DateTime.Now;
                    auditLogService.AddLog(auditlogModel);
                    return Ok(new APIResponse
                    {
                        ResponseStatusCode = GlobalConstants.NoDataStatus,//Data not found
                        ResponseMessage = "Data not found",
                        ResponseData = new OTPModel(),
                    });
                }
            }
            catch (Exception ex)
            {
                auditlogModel.RequestStatus = "Failure";
                auditlogModel.CreatedDate = DateTime.Now;
                auditlogModel.ErrorMessage = ex.Message;
                auditLogService.AddLog(auditlogModel);
                return Ok(new APIResponse
                {
                    ResponseStatusCode = GlobalConstants.ErrorStatus,//Error
                    ResponseMessage = "Error",
                    ErrorMessage = ex.Message,
                    ResponseData = new OTPModel(),
                });
            }
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var mail = new System.Net.Mail.MailAddress(email);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// To get category by Id
        /// </summary>
        /// <param name="merchantId"></param>
        /// <returns></returns>
        [HttpPost("ResetPassword")]
        //[Authorize]
        [ProducesResponseType(typeof(APIResponse), 200)]
        [ProducesResponseType(typeof(string), 404)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(string), 500)]
        public IActionResult ResetPassword(ResetPasswordModel resetPasswordModel)
        {
            AuditlogModel auditlogModel = new AuditlogModel();
            auditlogModel.RequestedApiname = "ResetPassword";
            auditlogModel.InputData = JsonSerializer.Serialize<ResetPasswordModel>(resetPasswordModel);
            ErrorResponseModel errorResponseModel = null;
            try
            {
                bool result = merchantService.ResetPassword(resetPasswordModel);
                if (result)
                {
                    auditlogModel.RequestStatus = "success";
                    auditlogModel.CreatedDate = DateTime.Now;
                    auditLogService.AddLog(auditlogModel);
                    return Ok(new APIResponse
                    {
                        ResponseStatusCode = GlobalConstants.OKStatus,
                        ResponseMessage="Password reset sucessfully",
                    });
                }
                else
                {
                    auditlogModel.RequestStatus = "User not found";
                    auditlogModel.CreatedDate = DateTime.Now;
                    auditLogService.AddLog(auditlogModel);
                    return Ok(new APIResponse
                    {
                        ResponseStatusCode = GlobalConstants.NoDataStatus,//Data not found
                        ResponseMessage = "User not found",
                        ResponseData = new OTPModel(),
                    });
                }
            }
            catch (Exception ex)
            {
                auditlogModel.RequestStatus = "Failure";
                auditlogModel.CreatedDate = DateTime.Now;
                auditlogModel.ErrorMessage = ex.Message;
                auditLogService.AddLog(auditlogModel);
                return Ok(new APIResponse
                {
                    ResponseStatusCode = GlobalConstants.ErrorStatus,//Error
                    ResponseMessage = "Error",
                    ErrorMessage = ex.Message,
                    ResponseData = new OTPModel(),
                });
            }
        }

        /// <summary>
        /// To get category by Id
        /// </summary>
        /// <param name="merchantId"></param>
        /// <returns></returns>
        //[HttpPost("EncryptString")]
        ////[Authorize]
        //[ProducesResponseType(typeof(APIResponse), 200)]
        //[ProducesResponseType(typeof(string), 404)]
        //[ProducesResponseType(typeof(string), 400)]
        //[ProducesResponseType(typeof(string), 500)]
        //public IActionResult EncryptString(string data)
        //{
        //    ErrorResponseModel errorResponseModel = null;
        //    try
        //    {
        //        string encryptString = Convert.ToBase64String(CommonUtility.Encrypt("abcdefghijklmnop", data));
        //        return Ok(new APIResponse
        //        {
        //            ResponseStatusCode = GlobalConstants.ErrorStatus,//Error
        //            ResponseMessage = encryptString,
                   
        //            ResponseData = new OTPModel(),
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        return Ok(new APIResponse
        //        {
        //            ResponseStatusCode = GlobalConstants.ErrorStatus,//Error
        //            ResponseMessage = "Error",
        //            ErrorMessage = ex.Message,
        //            ResponseData = new OTPModel(),
        //        });
        //    }
        //}

    }
}
