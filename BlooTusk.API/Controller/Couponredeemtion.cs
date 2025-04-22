using BlooTusk.Business.Implementation;
using BlooTusk.Business.Interface;
using BlooTusk.Common;
using BlooTusk.Entity.BlooTuskModel;
using BlooTusk.Model.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace BlooTusk.API.Controller
{
    public class CouponredeemtionController : BaseAPIController
    {
        ICouponredeemtionService couponredeemtionService;
        IMerchantService merchantService;
        private readonly IWebHostEnvironment iwebhostingEnvironment;
        public CouponredeemtionController(ICouponredeemtionService _couponredeemtionService, IWebHostEnvironment _iwebhostingEnvironment)
        {
            couponredeemtionService = _couponredeemtionService;
            iwebhostingEnvironment = _iwebhostingEnvironment;
        }

        [HttpPost("AddCouponredeemtion")]
        [Authorize]
        [ProducesResponseType(typeof(CouponredeemtionModel), 200)]
        [ProducesResponseType(typeof(string), 404)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(string), 500)]
        public async Task<IActionResult> AddEditredeemtionAsync([FromBody] CouponredeemtionModel couponredeemtionModel)
        {
            try
            {
                var errorMessage = new ErrorResponseModel();
                CouponredeemtionResultModel couponredeemtionResultModel = new CouponredeemtionResultModel();

             
                couponredeemtionResultModel = await couponredeemtionService.RedeemCoupon(couponredeemtionModel,  errorMessage);

                if (couponredeemtionResultModel != null)
                    {
                        string ResponseMessage = "";

                       ResponseMessage = couponredeemtionResultModel.RewardCouponTitle;
                   

                    int StatusCode = GlobalConstants.OKStatus;
                  if(couponredeemtionResultModel.RewardCouponTitle == "Customer Inactive")
                    {
                        StatusCode = GlobalConstants.InactiveCustomer;
                    }
                    else
                    {
                        StatusCode = GlobalConstants.OKStatus;
                    }
                        return Ok(new APIResponse
                        {
                            ResponseData = couponredeemtionResultModel,
                            ResponseStatusCode = StatusCode,
                            ResponseMessage = ResponseMessage,
                        });
                    }
                    

                else 
                {

                    return Ok(new APIResponse
                    {
                        ResponseStatusCode = GlobalConstants.ErrorStatus,
                        ResponseMessage =  "Coupon Code not Valid" ,
                    });
               
                }

            }
            catch (Exception ex)
            {
                return Ok(new APIResponse
                {
                    ResponseStatusCode = GlobalConstants.ErrorStatus,
                    ResponseMessage = (couponredeemtionModel.CouponredeemtionId == 0) ? "Coupon Code not Valid" : "Coupon Code not Valid ",
                    ErrorMessage = ex.ToString(),
                });
            }

        }



      
    }
}
