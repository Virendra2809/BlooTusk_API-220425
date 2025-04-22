using BlooTusk.Entity.BlooTuskModel;
using BlooTusk.Model.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlooTusk.Business.Interface
{
    public interface ICouponredeemtionService
    {
        // CouponredeemtionResultModel RedeemCoupon(CouponredeemtionModel couponredeemtion,  ref ErrorResponseModel errorResponseModel);
        
        Task<CouponredeemtionResultModel> RedeemCoupon(CouponredeemtionModel couponredeemtion,  ErrorResponseModel errorResponseModel);
       // Task RedeemCoupon(object couponredeemtion, ErrorResponseModel errorMessage);
        bool valideRedeemCoupon(CouponredeemtionModel couponredeemtion, ref ErrorResponseModel errorResponseModel); 

        //string IssueRedeemCoupon(CouponredeemtionModel couponredeemtion, ref ErrorResponseModel errorResponseModel);

    }
}
