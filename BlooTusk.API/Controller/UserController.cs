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
using Twilio.TwiML.Voice;
using Twilio.Types;

namespace BlooTusk.API.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : BaseAPIController
    {
        BlooTuskContext context;
        public IUserService userService;

        public UserController(BlooTuskContext _context,  IUserService _userService)
        {
            this.userService = _userService;

        }

        [HttpPost("UserVerification")]
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
                //bool isDuplicateUser = userService.checkDuplicateCustomer(otpModel.PhoneNumber, );
                //if (isDuplicateUser)
                //{
                //    return Ok(new APIResponse
                //    {
                //        ResponseStatusCode = GlobalConstants.DuplicatePhoneNUmberStatus,
                //        ResponseMessage = GlobalConstants.DuplicatePhoneNumber,
                //    });

                //}

                var Model = userService.SendOTP(otpModel, ref errorMessage);
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


        /// <summary>
        /// To get category by Id
        /// </summary>
        /// <param name="merchantId"></param>
        /// <returns></returns>
        [HttpGet("GetCustomerByCOde/{customerCode}")]
        //[Authorize]
        [ProducesResponseType(typeof(CustomerInfoModel), 200)]
        [ProducesResponseType(typeof(string), 404)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(string), 500)]
        public IActionResult GetCustomerByCOde(string customerCode)
        {
            AuditlogModel auditlogModel = new AuditlogModel();
            auditlogModel.RequestedApiname = "GetMarchantByCode";
            auditlogModel.InputData = "customerCode : " + customerCode;
            ErrorResponseModel errorResponseModel = null;
            try
            {

                var merchantData = userService.GetCustomerByCOde(customerCode, ref errorResponseModel);

                if (merchantData != null)
                {
                    auditlogModel.RequestStatus = "success";
                    auditlogModel.CreatedDate = DateTime.Now;

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

                    return Ok(new APIResponse
                    {
                        ResponseStatusCode = GlobalConstants.NoDataStatus,//Data not found
                        ResponseMessage = "Data not found",
                        ResponseData = new CustomerInfoModel(),
                    });
                }
            }
            catch (Exception ex)
            {
                auditlogModel.RequestStatus = "Failure";
                auditlogModel.CreatedDate = DateTime.Now;
                auditlogModel.ErrorMessage = ex.Message;

                return Ok(new APIResponse
                {
                    ResponseStatusCode = GlobalConstants.ErrorStatus,//Error
                    ResponseMessage = "Error",
                    ErrorMessage = ex.Message,
                    ResponseData = new CustomerInfoModel(),
                });
            }
        }

        /// <summary>
        /// To get category by Id
        /// </summary>
        /// <param name="merchantId"></param>
        /// <returns></returns>
        [HttpGet("GetReferalCount/{phoneNumber}")]
        //[Authorize]
        [ProducesResponseType(typeof(CustomerInfoModel), 200)]
        [ProducesResponseType(typeof(string), 404)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(string), 500)]
        public IActionResult GetReferalCount(string phoneNumber)
        {
            AuditlogModel auditlogModel = new AuditlogModel();
            auditlogModel.RequestedApiname = "GetReferalCount";
            auditlogModel.InputData = "customerId : " + phoneNumber;
            ErrorResponseModel errorResponseModel = null;
            try
            {

                var merchantData = userService.GetReferalCount(phoneNumber);

                if (merchantData != null)
                {
                    auditlogModel.RequestStatus = "success";
                    auditlogModel.CreatedDate = DateTime.Now;

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

                return Ok(new APIResponse
                {
                    ResponseStatusCode = GlobalConstants.ErrorStatus,//Error
                    ResponseMessage = "Error",
                    ErrorMessage = ex.Message,
                    ResponseData = new MerchantModel(),
                });
            }
        }

        /// <summary>
        /// To get category by Id
        /// </summary>
        /// <param name="merchantId"></param>
        /// <returns></returns>
        [HttpGet("GetRewardPointCount/{phoneNumber}")]
        //[Authorize]
        [ProducesResponseType(typeof(CustomerInfoModel), 200)]
        [ProducesResponseType(typeof(string), 404)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(string), 500)]
        public IActionResult GetRewardPointCount(string phoneNumber)
        {
            AuditlogModel auditlogModel = new AuditlogModel();
            auditlogModel.RequestedApiname = "GetRewardPointCount";
         
            ErrorResponseModel errorResponseModel = null;
            try
            {
                var merchantData = userService.GetRewardPointSum(phoneNumber);

                if (merchantData != null)
                {
                    auditlogModel.RequestStatus = "success";
                    auditlogModel.CreatedDate = DateTime.Now;

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

                return Ok(new APIResponse
                {
                    ResponseStatusCode = GlobalConstants.ErrorStatus,//Error
                    ResponseMessage = "Error",
                    ErrorMessage = ex.Message,
                    ResponseData = new MerchantModel(),
                });
            }
        }

        [HttpGet("GetDiscountCouponCount/{PhoneNumber}")]
        //[Authorize]
        [ProducesResponseType(typeof(CustomerInfoModel), 200)]
        [ProducesResponseType(typeof(string), 404)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(string), 500)]
        public IActionResult GetDiscountCouponCount(string PhoneNumber)
        {
            AuditlogModel auditlogModel = new AuditlogModel();
            auditlogModel.RequestedApiname = "GetDiscountCouponCount";
            auditlogModel.InputData = "PhoneNumber : " + PhoneNumber;
            ErrorResponseModel errorResponseModel = null;
            try
            {

                var merchantData = userService.GetDiscountCouponCount(PhoneNumber);

                if (merchantData != null)
                {
                    auditlogModel.RequestStatus = "success";
                    auditlogModel.CreatedDate = DateTime.Now;

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

                return Ok(new APIResponse
                {
                    ResponseStatusCode = GlobalConstants.ErrorStatus,//Error
                    ResponseMessage = "Error",
                    ErrorMessage = ex.Message,
                    ResponseData = new MerchantModel(),
                });
            }
        }

        [HttpPost("GetCustomerListForRefferal")]
        [Authorize]
        [ProducesResponseType(typeof(CustomerSearchModel), 200)]
        [ProducesResponseType(typeof(string), 404)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(string), 500)]

        public IActionResult GetCustResultReffer([FromBody] CustomerSearchModel otpModel)// int CustId, int PageNo, string keyword)
        {

            ErrorResponseModel errorResponseModel = null;
            try
            {
                var userDataList = userService.GetCustomerRefferalList(otpModel);
                if (userDataList != null)
                {
                    return Ok(new APIResponse
                    {
                        ResponseStatusCode = GlobalConstants.OKStatus,
                        ResponseData = userDataList,
                    });
                }
                else
                {
                    return Ok(new APIResponse
                    {
                        ResponseStatusCode = GlobalConstants.NoDataStatus,//Data not found
                        ResponseMessage = "Data not found",
                        ResponseData = new List<CustomerSearchModel>(),
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
                    ResponseData = new List<CustomerSearchModel>(),
                });
            }
        }



        [HttpGet("GetRewardPointMerchantWiseCount/{PhoneNumber}")]
        //[Authorize]
        [ProducesResponseType(typeof(CustomerInfoModel), 200)]
        [ProducesResponseType(typeof(string), 404)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(string), 500)]
        public IActionResult GetRewardPointMerchantWiseCount(string PhoneNumber)
        {
            AuditlogModel auditlogModel = new AuditlogModel();
            auditlogModel.RequestedApiname = "GetRewardPointCount";

            ErrorResponseModel errorResponseModel = null;
            try
            {
                var merchantData = userService.GetRewardPointMerchantWiseCount(PhoneNumber);

                if (merchantData != null)
                {
                    auditlogModel.RequestStatus = "success";
                    auditlogModel.CreatedDate = DateTime.Now;
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

                return Ok(new APIResponse
                {
                    ResponseStatusCode = GlobalConstants.ErrorStatus,//Error
                    ResponseMessage = "Error",
                    ErrorMessage = ex.Message,
                    ResponseData = new MerchantModel(),
                });
            }
        }

        [HttpGet("GetCustomerListForReward")]
        [Authorize]
        [ProducesResponseType(typeof(CustomerRewardSearchModel), 200)]
        [ProducesResponseType(typeof(string), 404)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(string), 500)]

        public IActionResult GetCustRewardPoint(int rewardPointId, string PhoneNumber)
        {

            ErrorResponseModel errorResponseModel = null;
            try
            {

                var userDataList = userService.GetCustomerRewardPointListList(rewardPointId, PhoneNumber);
                if (userDataList != null)
                {
                    return Ok(new APIResponse
                    {
                        ResponseStatusCode = GlobalConstants.OKStatus,
                        ResponseData = userDataList,
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

        /// <summary>
        /// To get signup user list  
        /// </summary>
        ///  /// <param name="CustomerSearchModel"></param>
        /// <returns></returns>
        [HttpPost("GetCustomerList")]
        [Authorize]
        [ProducesResponseType(typeof(CustomerSearchModel), 200)]
        [ProducesResponseType(typeof(string), 404)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(string), 500)]
        public IActionResult GetCustomerList([FromBody] CustomerSearchModel customerSearchModel)
        {
            ErrorResponseModel errorResponseModel = null;
            try
            {
                var userDataList = userService.GetCustomerList(customerSearchModel);
                if (userDataList != null)
                {
                    return Ok(new APIResponse
                    {
                        ResponseStatusCode = GlobalConstants.OKStatus,
                        ResponseData = userDataList,
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


        [HttpPost("AddCustomer")]
        //[Authorize]
        [ProducesResponseType(typeof(UserModel), 200)]
        [ProducesResponseType(typeof(string), 404)]

        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(string), 500)]
        public IActionResult AddCustomer([FromBody] CustomerModel customerModel)
        {
            AuditlogModel auditlogModel = new AuditlogModel();
            try
            {

                var errorMessage = new ErrorResponseModel();
                //bool isDuplicateUser = userService.checkDuplicateCustomer(customerModel.PhoneNumber);
                //if (isDuplicateUser)
                //{
                //    return Ok(new APIResponse
                //    {
                //        ResponseStatusCode = GlobalConstants.DuplicatePhoneNUmberStatus,
                //        ResponseMessage = GlobalConstants.DuplicatePhoneNumber,
                //    });
                //}


                var IsDuplicatePhone = userService.checkDuplicateCustomer(customerModel.PhoneNumber, customerModel.MerchantID);
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
                else
                {
                    var errorMessages = new ErrorResponseModel();
                    bool result = userService.AddCustomer(customerModel, ref errorMessages);
                    if (!result)
                    {
                        return Ok(new APIResponse
                        {
                            ResponseStatusCode = GlobalConstants.BadRequestStatus,
                            ResponseMessage = "Fail to add merchand",
                            ErrorMessage = errorMessage.Message,
                        });
                    }
                    else if (result.Equals("DR"))
                    {
                        return Ok(new APIResponse
                        {
                            ResponseStatusCode = GlobalConstants.DuplicateCategoryStatus,
                            ResponseMessage = GlobalConstants.DuplicateCategory,
                        });

                    }

                    else
                    {
                        return Ok(new APIResponse
                        {
                            ResponseStatusCode = GlobalConstants.OKStatus,
                            ResponseMessage = "Customer added sucessfully",
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                return Ok(new APIResponse
                {
                    ResponseStatusCode = GlobalConstants.ErrorStatus,
                    ResponseMessage = "Fail to Login",
                    ErrorMessage = ex.Message,
                    ResponseData = new AdminAuthenticateModel()
                });
            }

        }



        [HttpPost("UpdateCustomer")]
        //[Authorize]
        [ProducesResponseType(typeof(UserModel), 200)]
        [ProducesResponseType(typeof(string), 404)]

        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(string), 500)]
        public IActionResult UpdateCustomer([FromBody] CustomerModel customerModel)
        {
            AuditlogModel auditlogModel = new AuditlogModel();
            try
            {

                var errorMessage = new ErrorResponseModel();
                bool result = userService.UpdateCustomer(customerModel, ref errorMessage);
                if (!result)
                {
                    return Ok(new APIResponse
                    {
                        ResponseStatusCode = GlobalConstants.BadRequestStatus,
                        ResponseMessage = "Fail to add Customer",
                        ErrorMessage = errorMessage.Message,
                    });
                }
                else if (result.Equals("DR"))
                {
                    return Ok(new APIResponse
                    {
                        ResponseStatusCode = GlobalConstants.DuplicateCategoryStatus,
                        ResponseMessage = GlobalConstants.DuplicateCategory,
                    });

                }

                else
                {
                    return Ok(new APIResponse
                    {
                        ResponseStatusCode = GlobalConstants.OKStatus,
                        ResponseMessage = "Customer Update sucessfully",
                    });
                }


            }
            catch (Exception ex)
            {
                return Ok(new APIResponse
                {
                    ResponseStatusCode = GlobalConstants.ErrorStatus,
                    ResponseMessage = "Fail to Login",
                    ErrorMessage = ex.Message,
                    ResponseData = new AdminAuthenticateModel()
                });
            }

        }


        [HttpGet("GetRefferalLink/{customerId}")]
        //[Authorize]
        [ProducesResponseType(typeof(CustomerInfoModel), 200)]
        [ProducesResponseType(typeof(string), 404)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(string), 500)]
        public IActionResult GetRefferalLink(int customerId)
        {
            AuditlogModel auditlogModel = new AuditlogModel();
            auditlogModel.RequestedApiname = "GetRewardPointCount";
            auditlogModel.InputData = "customerId : " + customerId;
            ErrorResponseModel errorResponseModel = null;
            try
            {

                var merchantData = userService.GetReferalLink(customerId);

                if (merchantData != null)
                {
                    auditlogModel.RequestStatus = "success";
                    auditlogModel.CreatedDate = DateTime.Now;

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

                return Ok(new APIResponse
                {
                    ResponseStatusCode = GlobalConstants.ErrorStatus,//Error
                    ResponseMessage = "Error",
                    ErrorMessage = ex.Message,
                    ResponseData = new MerchantModel(),
                });
            }
        }

        [HttpGet("GetRefferalLinkList/{PhoneNumber}")]
        //[Authorize]
        [ProducesResponseType(typeof(CustomerInfoModel), 200)]
        [ProducesResponseType(typeof(string), 404)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(string), 500)]
        public IActionResult GetRefferalLinkList(string PhoneNumber)
        {
            AuditlogModel auditlogModel = new AuditlogModel();
            auditlogModel.RequestedApiname = "GetRefferalLinkList";
            auditlogModel.InputData = "PhoneNumber : " + PhoneNumber;
            ErrorResponseModel errorResponseModel = null;
            try
            {

                var refferalLinks = userService.GetReferalLinkList(PhoneNumber);

                if (refferalLinks != null)
                {
                    auditlogModel.RequestStatus = "success";
                    auditlogModel.CreatedDate = DateTime.Now;

                    return Ok(new APIResponse
                    {
                        ResponseStatusCode = GlobalConstants.OKStatus,
                        ResponseData = refferalLinks,
                    });
                }
                else
                {
                    auditlogModel.RequestStatus = "Data not found";
                    auditlogModel.CreatedDate = DateTime.Now;

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

                return Ok(new APIResponse
                {
                    ResponseStatusCode = GlobalConstants.ErrorStatus,//Error
                    ResponseMessage = "Error",
                    ErrorMessage = ex.Message,
                    ResponseData = new MerchantModel(),
                });
            }
        }



        [HttpPost("UpdateCustomerMerchant")]
        [Authorize]
        [ProducesResponseType(typeof(CustomerMerchantModel), 200)]
        [ProducesResponseType(typeof(string), 404)]

        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(string), 500)]
        public IActionResult UpdateCustomerMerchant([FromBody] CustomerMerchantModel customerModel)
        {
            AuditlogModel auditlogModel = new AuditlogModel();
            try
            {

                var errorMessage = new ErrorResponseModel();
                
                
                    var errorMessages = new ErrorResponseModel();
                    bool result = userService.UpdateCustomerMapper(customerModel, ref errorMessages);
                    if (!result)
                    {
                        return Ok(new APIResponse
                        {
                            ResponseStatusCode = GlobalConstants.BadRequestStatus,
                            ResponseMessage = "Fail to Update CustomerMapper",
                            ErrorMessage = errorMessage.Message,
                        });
                    }
                   
                    else
                    {
                        return Ok(new APIResponse
                        {
                            ResponseStatusCode = GlobalConstants.OKStatus,
                            ResponseMessage = "CustomerMapper Update sucessfully",
                        });
                    }
                
            }
            catch (Exception ex)
            {
                return Ok(new APIResponse
                {
                    ResponseStatusCode = GlobalConstants.ErrorStatus,
                    ResponseMessage = "Fail to Login",
                    ErrorMessage = ex.Message,
                    ResponseData = new AdminAuthenticateModel()
                });
            }

        }


    }
}
