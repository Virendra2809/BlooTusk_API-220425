using BlooTusk.Entity.BlooTuskModel;
using BlooTusk.Model.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks; 

namespace BlooTusk.Business.Interface
{
    public interface ICouponService
    {        
        string AddEditCoupon(CouponmasterModel couponMaster, ref ErrorResponseModel errorResponseModel);

        string SpecialOffer(CouponSpacialModel CouponMasters, ref ErrorResponseModel errorResponseModel);

        string IssuedCoupon(CouponSpacialModel couponMaster, ref ErrorResponseModel errorResponseModel);

        bool DeleteCoupon(int couponId, ref ErrorResponseModel errorResponseModel);
        CouponmasterModel GetCouponById(int couponId, ref ErrorResponseModel errorResponseModel);
        CouponSearchModel GetAllCampaigns(CampainListSerach campainListSerach, ref ErrorResponseModel errorResponseModel);
        List<CustomerCouponModel> GetCampaignsPerformance(CouponSearchPerformanceModel couponSearchPerformanceModel , ref ErrorResponseModel errorResponseModel);
        IssuedToModel GetIssuedToDDL(IssuedSearchModel issuedSearchModel, ref ErrorResponseModel errorResponseModel);
        public CustomerCouponListModel GetCustomerCouponList(CustomerCouponList customerCouponList);
        string NudgeCoupon(NudgeModel nudgeCouponModel, ref ErrorResponseModel errorResponseModel);
        MerchantWiseCustomerCount GetCountOfCustomerMerchantWise(MerchantCustList merchantCustList);
        MerchantWiseDashboardCount GetCountOfDasboardMerchantWise(DashboardValueList dashboardValueList);
        CouponQrModel GetCouponByCouponCode(string CouponCode, ref ErrorResponseModel errorResponseModel);


        //  ValidateCouponSearchModel GetCustomerReddemData(ValidateCouponModel validateCouponSearchModel);
        //  public string ValidateCoupon(ValidateCouponModel validateCouponModel, ref ErrorResponseModel errorResponseModel);
    }
}
