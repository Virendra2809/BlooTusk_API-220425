using BlooTusk.Business.Interface;
using BlooTusk.Common;
using BlooTusk.Model.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System;
using BlooTusk.Business.Implementation;
using BlooTusk.Entity.BlooTuskModel;
using MySqlX.XDevAPI.Common;

namespace BlooTusk.API.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class CouponMasterController : BaseAPIController
    {
        ICouponService couponService;
        IMerchantUserService merchantUserService;
        private readonly IWebHostEnvironment iwebhostingEnvironment;
        public CouponMasterController(ICouponService _couponService, IMerchantUserService _merchantUserService, IWebHostEnvironment _iwebhostingEnvironment)
        {
            couponService = _couponService;
            merchantUserService = _merchantUserService;
            iwebhostingEnvironment = _iwebhostingEnvironment;
        }

        /// <summary>
        ///  AddEdit AuditorDetails
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("AddEditCoupon")]
        [Authorize] 
        [ProducesResponseType(typeof(CouponmasterModel), 200)]
        [ProducesResponseType(typeof(string), 404)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(string), 500)]
        public IActionResult AddEditCoupon([FromBody] CouponmasterModel CouponModel)
        {
            try
            {
                var errorMessage = new ErrorResponseModel();
                var result = couponService.AddEditCoupon(CouponModel, ref errorMessage);
                if (result.Equals("A") || result.Equals("U"))
                {
                    return Ok(new APIResponse
                    {
                        ResponseStatusCode = GlobalConstants.OKStatus,
                        ResponseMessage = (CouponModel.CouponId == 0) ? "Campaign started successfully" : "Campaign updated successfully",
                    }
                    );
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
                        ResponseStatusCode = GlobalConstants.ErrorStatus,
                        ResponseMessage = (CouponModel.CouponId == 0) ? "Fail to start Campaign" : "Fail to update Campaign",
                    });
                }
            }
            catch (Exception ex)
            {
                return Ok(new APIResponse
                {
                    ResponseStatusCode = GlobalConstants.ErrorStatus,
                    ResponseMessage = (CouponModel.CouponId == 0) ? "Fail to add coupon" : "Fail to update coupon",
                    ErrorMessage = ex.ToString(),
                });
            }

        }

        /// <summary>
        /// To get Coupon list  
        /// </summary>
        /// <returns></returns>
        [HttpPost("GetAllCampaignsList")]
        [Authorize]
        [ProducesResponseType(typeof(CouponSearchModel), 200)]
        [ProducesResponseType(typeof(string), 404)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(string), 500)]
        public IActionResult GetAllCampaignsList(CampainListSerach campainListSerach)
        {
            ErrorResponseModel errorResponseModel = null;
            try
            {
                var CouponData = couponService.GetAllCampaigns(campainListSerach, ref errorResponseModel);
                if (CouponData != null)
                {
                    return Ok(new APIResponse
                    {
                        ResponseStatusCode = GlobalConstants.OKStatus,
                        ResponseData = CouponData,
                    });
                }
                else
                {
                    return Ok(new APIResponse
                    {
                        ResponseStatusCode = GlobalConstants.NoDataStatus,//Data not found
                        ResponseMessage = "Data not found",
                        ResponseData = new List<CouponSearchModel>(),
                    });
                }

            }
            catch (Exception ex)
            {
                return Ok(new APIResponse
                {
                    ResponseStatusCode = GlobalConstants.ErrorStatus,//Data not found
                    ResponseMessage = "Error",
                    ErrorMessage = ex.Message,
                    ResponseData = new List<CouponSearchModel>(),
                });
            }
        }

        [HttpPost("GetCampaignsPerformance")]
        [Authorize]
        [ProducesResponseType(typeof(CouponSearchModel), 200)]
        [ProducesResponseType(typeof(string), 404)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(string), 500)]
        public IActionResult GetCampaignsPerformance(CouponSearchPerformanceModel couponSearchPerformanceModel)
        {
            ErrorResponseModel errorResponseModel = null;
            try
            {
                var CouponData = couponService.GetCampaignsPerformance(couponSearchPerformanceModel, ref errorResponseModel);
                if (CouponData != null)
                {
                    return Ok(new APIResponse
                    {
                        ResponseStatusCode = GlobalConstants.OKStatus,
                        ResponseData = CouponData,
                    });
                }
                else
                {
                    return Ok(new APIResponse
                    {
                        ResponseStatusCode = GlobalConstants.NoDataStatus,//Data not found
                        ResponseMessage = "Data not found",
                        ResponseData = new List<CouponSearchModel>(),
                    });
                }

            }
            catch (Exception ex)
            {
                return Ok(new APIResponse
                {
                    ResponseStatusCode = GlobalConstants.ErrorStatus,//Data not found
                    ResponseMessage = "Error",
                    ErrorMessage = ex.Message,
                    ResponseData = new List<CouponSearchModel>(),
                });
            }
        }

        /// <summary>
        /// To get Coupon by Id
        /// </summary>
        /// <param name="CouponId"></param>
        /// <returns></returns>
        [HttpGet("GetCouponById/{couponId}")]
        [Authorize]
        [ProducesResponseType(typeof(CouponmasterModel), 200)]
        [ProducesResponseType(typeof(string), 404)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(string), 500)]
        public IActionResult GetCouponById(int couponId)
        {
            ErrorResponseModel errorResponseModel = null;
            try
            {
                if (couponId <= 0)
                {
                    return Ok(new APIResponse
                    {
                        ResponseStatusCode = 400,//Bad request
                        ResponseMessage = "Bad request",
                        ResponseData = new CouponmasterModel(),
                    });
                }
                var CouponData = couponService.GetCouponById(couponId, ref errorResponseModel);

                if (CouponData != null)
                {
                    return Ok(new APIResponse
                    {
                        ResponseStatusCode = 200,
                        ResponseData = CouponData,
                    });
                }
                else
                {
                    return Ok(new APIResponse
                    {
                        ResponseStatusCode = 212,//Data not found
                        ResponseMessage = "Data not found",
                        ResponseData = new CouponmasterModel(),
                    });
                }
            }
            catch (Exception ex)
            {
                return Ok(new APIResponse
                {
                    ResponseStatusCode = StatusCodes.Status500InternalServerError,//Error
                    ResponseMessage = "Error",
                    ErrorMessage = ex.Message,
                    ResponseData = new CouponmasterModel(),
                });
            }
        }

        [HttpPost("GetCustomerCouponList")]
        [Authorize]
        [ProducesResponseType(typeof(CustomerCouponModel), 200)]
        [ProducesResponseType(typeof(string), 404)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(string), 500)]

        public IActionResult GetCustomerCouponList(CustomerCouponList customerCouponList)
        {
            ErrorResponseModel errorResponseModel = null;
            try
            {
                var CouponData = couponService.GetCustomerCouponList(customerCouponList);
                if (CouponData != null)
                {
                    return Ok(new APIResponse
                    {
                        ResponseStatusCode = GlobalConstants.OKStatus,
                        ResponseData = CouponData,
                    });
                }
                else
                {
                    return Ok(new APIResponse
                    {
                        ResponseStatusCode = GlobalConstants.NoDataStatus,//Data not found
                        ResponseMessage = "Data not found",
                        ResponseData = new List<CustomerCouponModel>(),
                    });
                }

            }
            catch (Exception ex)
            {
                return Ok(new APIResponse
                {
                    ResponseStatusCode = GlobalConstants.ErrorStatus,//Data not found
                    ResponseMessage = "Error",
                    ErrorMessage = ex.Message,
                    ResponseData = new List<CustomerCouponModel>(),
                });
            }
        }

        /// <summary>
        /// To get categoryDDL  
        /// </summary>
        /// <returns></returns>
        [HttpPost("GetIssuedToDDL")]
        [ProducesResponseType(typeof(IssuedToModel), 200)]
        [ProducesResponseType(typeof(string), 404)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(string), 500)]
        public IActionResult GetIssuedToDDL(IssuedSearchModel issuedSearchModel)
        {
            ErrorResponseModel errorResponseModel = null;
            try
            {
                var issuedToDDLData = couponService.GetIssuedToDDL(issuedSearchModel, ref errorResponseModel);
                if (issuedToDDLData != null)
                {
                    return Ok(new APIResponse
                    {
                        ResponseStatusCode = 200,
                        ResponseData = issuedToDDLData,
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

        [HttpPost("NudgeCoupon")]
        [Authorize]
        [ProducesResponseType(typeof(NudgeModel), 200)]

        [ProducesResponseType(typeof(string), 404)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(string), 500)]
        public IActionResult NudgeCoupon([FromBody] NudgeModel NudgeCouponModel)
        {
            try
            {
                var errorMessage = new ErrorResponseModel();
                var result = merchantUserService.NudgeCoupon(NudgeCouponModel, ref errorMessage);
                if (result.Equals("A") || result.Equals("U"))
                {
                    return Ok(new APIResponse
                    {
                        ResponseStatusCode = GlobalConstants.OKStatus,
                        ResponseMessage = GlobalConstants.NudgeMessage //(NudgeCouponModel.CouponId == 0) ? "Nudge Send scucessfully" : "Nudge Send scucessfully",
                    }
                    );;
                }
                else if (result.Equals("D"))
                {
                    return Ok(new APIResponse
                    {
                        ResponseStatusCode = GlobalConstants.DuplicateCategoryStatus,
                        ResponseMessage = GlobalConstants.NudgeMessageLimitExtend,
                    });

                }
                else
                {
                    return Ok(new APIResponse
                    {
                        ResponseStatusCode = GlobalConstants.ErrorStatus,
                        ResponseMessage = (NudgeCouponModel.CouponId == 0) ? "Fail to add coupon" : "Fail to update coupon",
                    });
                }
            }
            catch (Exception ex)
            {
                return Ok(new APIResponse
                {
                    ResponseStatusCode = GlobalConstants.ErrorStatus,
                    ResponseMessage = (NudgeCouponModel.CouponId == 0) ? "Fail to add coupon" : "Fail to update coupon",
                    ErrorMessage = ex.ToString(),
                });
            }
        }

        [HttpPost("ValidateCoupon")]
        [Authorize]
        [ProducesResponseType(typeof(ValidateCouponModel), 200)]
        [ProducesResponseType(typeof(string), 404)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(string), 500)]
        public IActionResult ValidateCoupon([FromBody] ValidateCouponModel ValidateCoupon)
        {
            try
            {
                ValidateCouponModel validcoupon = new ValidateCouponModel();

                AuditlogModel auditlogModel = new AuditlogModel();
                var errorMessage = new ErrorResponseModel();

                validcoupon = merchantUserService.ValidateCoupon(ValidateCoupon, ref errorMessage);

                return Ok(new APIResponse
                {
                    ResponseStatusCode = GlobalConstants.OKStatus,
                    ResponseData = validcoupon
                });

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

        [HttpPost("GetCountOfCustomerMerchantWise")]
        [Authorize]
        [ProducesResponseType(typeof(MerchantCustList), 200)]
        [ProducesResponseType(typeof(string), 404)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(string), 500)]
        public IActionResult GetCountOfCustomerMerchantWise(MerchantCustList merchantCustList)
        {
            try
            {
                var errorMessage = new ErrorResponseModel();
                MerchantWiseCustomerCount merchantWiseCustomerCount = new MerchantWiseCustomerCount();

                merchantWiseCustomerCount = couponService.GetCountOfCustomerMerchantWise(merchantCustList);

                return Ok(new APIResponse
                {
                    ResponseStatusCode = GlobalConstants.OKStatus,
                    ResponseData = merchantWiseCustomerCount
                });
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


        [HttpPost("GetCountForDashBoardValue")]
        [Authorize]
        [ProducesResponseType(typeof(DashboardValueList), 200)]
        [ProducesResponseType(typeof(string), 404)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(string), 500)]
        public IActionResult GetCountOfDasboardMerchantWise(DashboardValueList DashboardValueList)
        {
            try
            {
                var errorMessage = new ErrorResponseModel();
                MerchantWiseDashboardCount merchantWiseDashboardCount = new MerchantWiseDashboardCount();

                merchantWiseDashboardCount = couponService.GetCountOfDasboardMerchantWise(DashboardValueList);


                return Ok(new APIResponse
                {
                    ResponseStatusCode = GlobalConstants.OKStatus,
                    ResponseData = merchantWiseDashboardCount
                });
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

        //get all info for coupon Reddem

        /// <summary>
        ///  AddEdit AuditorDetails
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        //[HttpPost("GetCustomerReddemData")]
        //[Authorize]
        //[ProducesResponseType(typeof(ValidateCouponSearchModel), 200)]
        //[ProducesResponseType(typeof(string), 404)]
        //[ProducesResponseType(typeof(string), 400)]
        //[ProducesResponseType(typeof(string), 500)]
        //public IActionResult GetCustomerReddemData([FromBody] ValidateCouponModel validateCouponSearchModel)
        //{
        //    try
        //    {
        //        var errorMessage = new ErrorResponseModel();
        //        var result = couponService.GetCustomerReddemData(validateCouponSearchModel);
        //        if (result != null)
        //        {
        //            return Ok(new APIResponse
        //            {
        //                ResponseStatusCode = GlobalConstants.OKStatus,
        //                ResponseData = result,
        //                ResponseMessage = "Success"
        //            });
        //        }

        //        else
        //        {
        //            return Ok(new APIResponse
        //            {
        //                ResponseStatusCode = GlobalConstants.ErrorStatus,
        //                ResponseMessage = "Failed"

        //            });
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return Ok(new APIResponse
        //        {
        //            ResponseStatusCode = GlobalConstants.ErrorStatus,
        //            ErrorMessage = ex.ToString(),
        //        });
        //    }

        //}


        [HttpPost("IssuedCoupon")]
        [Authorize]
        [ProducesResponseType(typeof(CouponSpacialModel), 200)]
        [ProducesResponseType(typeof(string), 404)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(string), 500)]
        public IActionResult IssuedCoupon([FromBody] CouponSpacialModel CouponModel)
        {
            try
            {
                var errorMessage = new ErrorResponseModel();
                var result = couponService.IssuedCoupon(CouponModel, ref errorMessage);
                if (result.Equals("A") || result.Equals("U"))
                {
                    return Ok(new APIResponse
                    {
                        ResponseStatusCode = GlobalConstants.OKStatus,
                        ResponseMessage = (CouponModel.CouponId == 0) ? "Coupon added scucessfully" : "Coupon updated scucessfully",
                    }
                    );
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
                        ResponseStatusCode = GlobalConstants.ErrorStatus,
                        ResponseMessage = (CouponModel.CouponId == 0) ? "Fail to add coupon" : "Fail to update coupon",
                    });
                }
            }
            catch (Exception ex)
            {
                return Ok(new APIResponse
                {
                    ResponseStatusCode = GlobalConstants.ErrorStatus,
                    ResponseMessage = (CouponModel.CouponId == 0) ? "Fail to add coupon" : "Fail to update coupon",
                    ErrorMessage = ex.ToString(),
                });
            }
        }

        [HttpPost("SpacialOffer")]
        [Authorize]
        [ProducesResponseType(typeof(CouponSpacialModel), 200)]
        [ProducesResponseType(typeof(string), 404)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(string), 500)]
        public IActionResult SpacialOffer([FromBody] CouponSpacialModel CouponModel)
        {
            try
            {
                var errorMessage = new ErrorResponseModel();
                var result = couponService.SpecialOffer(CouponModel, ref errorMessage);
                if (result.Equals("A") || result.Equals("U"))
                {
                    return Ok(new APIResponse
                    {
                        ResponseStatusCode = GlobalConstants.OKStatus,
                        ResponseMessage = (CouponModel.CouponId == 0) ? "Coupon added scucessfully" : "Coupon updated scucessfully",
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
                        ResponseStatusCode = GlobalConstants.ErrorStatus,
                        ResponseMessage = (CouponModel.CouponId == 0) ? "Fail to add coupon" : "Fail to update coupon",
                    });
                }
            }
            catch (Exception ex)
            {
                return Ok(new APIResponse
                {
                    ResponseStatusCode = GlobalConstants.ErrorStatus,
                    ResponseMessage = (CouponModel.CouponId == 0) ? "Fail to add coupon" : "Fail to update coupon",
                    ErrorMessage = ex.ToString(),
                });
            }
        }


        [HttpGet("GetCouponByCouponCode/{couponCode}")]
      
        [ProducesResponseType(typeof(MerchantModel), 200)]
        [ProducesResponseType(typeof(string), 404)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(string), 500)]
        public IActionResult GetCouponByCouponCode(string couponCode)
        {
            ErrorResponseModel errorResponseModel = null;
            try
            {
                var CouponData = couponService.GetCouponByCouponCode(couponCode, ref errorResponseModel);

                if (CouponData != null)
                {
                   
                    return Ok(new APIResponse
                    {
                        ResponseStatusCode = 200,
                        ResponseData = CouponData,
                    });
                }
                else
                {
                    return Ok(new APIResponse
                    {
                        ResponseStatusCode = 212,//Data not found
                        ResponseMessage = "Data not found",
                        ResponseData = new CouponmasterModel(),
                    });
                }
            }
            catch (Exception ex)
            {
                return Ok(new APIResponse
                {
                    ResponseStatusCode = StatusCodes.Status500InternalServerError,//Error
                    ResponseMessage = "Error",
                    ErrorMessage = ex.Message,
                    ResponseData = new CouponmasterModel(),
                });
            }
        }

    }
}