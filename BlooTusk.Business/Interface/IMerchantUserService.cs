using BlooTusk.Model.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlooTusk.Business.Interface
{
    public interface IMerchantUserService
    {
        bool AddMerchantUser(MerchantUserModel merchantUserModel, int userID);
        MerchantUserModel GetProfile(string userId);
        bool UpdateProfile(MerchantUserModel userModel, int userId);
        bool IsDuplicatePhoneNumber(string phoneNumber,int merchantId ,int MerchantSystemUserId, bool merchantFlag);
        bool IsDuplicateEmail(string email, int merchantId , int MerchantSystemUserId, bool merchantFlag);

      //  bool IsDuplicateEmail(string email , int )



        MerchantUserSearchResultModel GetMerchantUserList(MerchantUserSearchModel merchantUserSearchModel);
        bool EditMerchantUser(MerchantUserModel merchantUserModel, ref ErrorResponseModel errorResponseModel);
        public MerchantCouponListModel GetMerchantCouponList(int MerchantID);

        MerchantUserModel GetMerchantUserById(int merchantSystemUserId,ref ErrorResponseModel errorResponseModel);

        public ValidateCouponModel ValidateCoupon(ValidateCouponModel validateCouponModel, ref ErrorResponseModel errorResponseModel);
       
        string NudgeCoupon(NudgeModel nudgeCouponModel, ref ErrorResponseModel errorResponseModel);

    }
}
