using BlooTusk.Business.Implementation;
using BlooTusk.Business.Interface;
using BlooTusk.Common;
using BlooTusk.Entity.BlooTuskModel;
using BlooTusk.Model.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Text.Json;

namespace BlooTusk.API.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class MerchantUserController : BaseAPIController
    {
        public IMerchantUserService merchantUserService;
        public IAuditlogService auditLogService;
        public MerchantUserController(IMerchantUserService _merchantUserService, IAuditlogService _auditLogService)
        {
            this.merchantUserService = _merchantUserService;
            auditLogService = _auditLogService;
        }

        [HttpPost("AddMerchantSystemUser")]
        //[Authorize]
        [ProducesResponseType(typeof(MerchantModel), 200)]
        [ProducesResponseType(typeof(string), 404)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(string), 500)] 
        public IActionResult AddMerchantSystemUser([FromBody] MerchantUserModel merchantModel)
        {
            AuditlogModel auditlogModel = new AuditlogModel();
            auditlogModel.RequestedApiname = "AddMerchant";
            auditlogModel.InputData = JsonSerializer.Serialize<MerchantUserModel>(merchantModel);
            try
            {
                var errorMessage = new ErrorResponseModel();
                var IsDuplicatePhone = merchantUserService.IsDuplicatePhoneNumber(merchantModel.PhoneNumber, merchantModel.MerchantId, merchantModel.MerchantSystemUserId, true);
                if (IsDuplicatePhone)
                {
                    auditlogModel.RequestStatus = "Duplicate number";
                    auditlogModel.CreatedDate = DateTime.Now;
                    auditlogModel.ErrorMessage = "";
                  //  auditLogService.AddLog(auditlogModel);
                    return Ok(new APIResponse
                    {
                        ResponseStatusCode = GlobalConstants.DuplicatePhoneNUmberStatus,
                        ResponseMessage = GlobalConstants.DuplicatePhoneNumber,
                    });
                }
                var IsDuplicateEmail = merchantUserService.IsDuplicateEmail(merchantModel.Email, merchantModel.MerchantId, merchantModel.MerchantSystemUserId, true);
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
                else
                {
                    int userId = 0;
                    if (User != null && User.Identity != null && User.Identity.IsAuthenticated)
                    {
                        if (User != null && User.Identity != null && User.Identity.IsAuthenticated)
                        {
                            userId = Convert.ToInt32(((System.Security.Claims.ClaimsIdentity)User.Identity).FindFirst(System.Security.Claims.ClaimTypes.Name).Value);
                        }
                    }
                    Boolean Model = merchantUserService.AddMerchantUser(merchantModel, userId);
                    if (!Model)
                    {
                        auditlogModel.RequestStatus = "Failure";
                        auditlogModel.CreatedDate = DateTime.Now;
                        auditLogService.AddLog(auditlogModel);
                        return Ok(new APIResponse
                        {
                            ResponseStatusCode = GlobalConstants.BadRequestStatus,
                            ResponseMessage = "Fail to add merchant User",
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
                            ResponseMessage = GlobalConstants.MerchantUserCreateMessage,
                        });
                    }
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
        /// To get signup user list  
        /// </summary>
        ///  /// <param name="MerchantUserSearchModel"></param>
        /// <returns></returns>
        [HttpPost("GetMerchantUserList")]
        [Authorize]
        [ProducesResponseType(typeof(MerchantUserSearchModel), 200)]
        [ProducesResponseType(typeof(string), 404)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(string), 500)]
        public IActionResult GetMerchantUserList([FromBody] MerchantUserSearchModel merchantUserSearchModel)
        {
            ErrorResponseModel errorResponseModel = null;
            try
            {
                var merchantuserDataList = merchantUserService.GetMerchantUserList(merchantUserSearchModel);
                if (merchantuserDataList != null)
                {
                    return Ok(new APIResponse
                    {
                        ResponseStatusCode = GlobalConstants.OKStatus,
                        ResponseData = merchantuserDataList,
                    });
                }
                else
                {
                    return Ok(new APIResponse
                    {
                        ResponseStatusCode = GlobalConstants.NoDataStatus,//Data not found
                        ResponseMessage = "Data not found",
                        ResponseData = new List<MerchantUserSearchModel>(),
                    });
                }

            }
            catch (Exception ex)
            {
                //return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
                return Ok(new APIResponse
                {
                    ResponseStatusCode = GlobalConstants.ErrorStatus,//Data not found
                    ResponseMessage = "Error",
                    ErrorMessage = ex.Message,
                    ResponseData = new List<MerchantUserSearchModel>(),
                });
            }
        }



        /// <summary>
        /// To get signup user list  
        /// </summary>
        ///  /// <param name="MerchantUserSearchModel"></param>
        /// <returns></returns>
        [HttpPost("GetMerchantCouponList")]
        [Authorize]
        [ProducesResponseType(typeof(MerchantCouponListModel), 200)]
        [ProducesResponseType(typeof(string), 404)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(string), 500)]
        public IActionResult GetMerchantCouponList(int MerchantID)
        {
            ErrorResponseModel errorResponseModel = null;
            try
            {
                var merchantuserDataList = merchantUserService.GetMerchantCouponList(MerchantID);
                if (merchantuserDataList != null)
                {
                    return Ok(new APIResponse
                    {
                        ResponseStatusCode = GlobalConstants.OKStatus,
                        ResponseData = merchantuserDataList,
                    });
                }
                else
                {
                    return Ok(new APIResponse
                    {
                        ResponseStatusCode = GlobalConstants.NoDataStatus,//Data not found
                        ResponseMessage = "Data not found",
                        ResponseData = new List<UserModel>(),
                    });
                }

            }
            catch (Exception ex)
            {
                //return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
                return Ok(new APIResponse
                {
                    ResponseStatusCode = GlobalConstants.ErrorStatus,//Data not found
                    ResponseMessage = "Error",
                    ErrorMessage = ex.Message,
                    ResponseData = new List<UserModel>(),
                });
            }
        }

        // change the logic

        [HttpPost("EditMerchantUser")]
        [Authorize]
        [ProducesResponseType(typeof(MerchantUserModel), 200)]
        [ProducesResponseType(typeof(string), 404)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(string), 500)]
        public IActionResult EditMerchantUser([FromBody] MerchantUserModel merchantModel)
        {
            AuditlogModel auditlogModel = new AuditlogModel();
            auditlogModel.RequestedApiname = "EditMerchantUser";
            auditlogModel.InputData = JsonSerializer.Serialize<MerchantUserModel>(merchantModel);
            try
            {
                var IsDuplicatePhone = merchantUserService.IsDuplicatePhoneNumber(merchantModel.PhoneNumber,merchantModel.MerchantId, merchantModel.MerchantSystemUserId , false);
                if (IsDuplicatePhone)
                {
                    auditlogModel.RequestStatus = "Duplicate number";
                    auditlogModel.CreatedDate = DateTime.Now;
                    auditlogModel.ErrorMessage = "";
                    //  auditLogService.AddLog(auditlogModel);
                    return Ok(new APIResponse
                    {
                        ResponseStatusCode = GlobalConstants.DuplicatePhoneNUmberStatus,
                        ResponseMessage = GlobalConstants.DuplicatePhoneNumber,
                    });
                }
                var IsDuplicateEmail = merchantUserService.IsDuplicateEmail(merchantModel.Email, merchantModel.MerchantId, merchantModel.MerchantSystemUserId,false);
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


                else
                {
                    var errorMessage = new ErrorResponseModel();
                    Boolean Model = merchantUserService.EditMerchantUser(merchantModel, ref errorMessage);

                    if (!Model)
                    {
                        auditlogModel.RequestStatus = "Failure";
                        auditlogModel.CreatedDate = DateTime.Now;
                        auditLogService.AddLog(auditlogModel);
                        return Ok(new APIResponse
                        {
                            ResponseStatusCode = GlobalConstants.BadRequestStatus,
                            ResponseMessage = "Fail to edit merchant ",
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
                            ResponseMessage = GlobalConstants.MerchantUserUpdateMessage,
                        });
                    }
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
                    ResponseMessage = "Fail to update merchant user",
                    ErrorMessage = ex.Message,
                    ResponseData = new AdminAuthenticateModel()
                });
            }
        }

        [HttpGet("GetMerchantUserById/{merchantSystemUserId}")]
        [Authorize]
        [ProducesResponseType(typeof(MerchantUserModel), 200)]
        [ProducesResponseType(typeof(string), 404)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(string), 500)]
        public IActionResult GetMerchantUserById(int merchantSystemUserId)
        {
            AuditlogModel auditlogModel = new AuditlogModel();
            auditlogModel.RequestedApiname = "GetMerchantUserById";
            auditlogModel.InputData = "merchantSystemUserId : " + merchantSystemUserId;
            try
            {
                var errorMessage = new ErrorResponseModel();
                var merchantModel = merchantUserService.GetMerchantUserById(merchantSystemUserId, ref errorMessage);
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

    }

}
