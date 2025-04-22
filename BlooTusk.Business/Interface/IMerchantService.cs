using BlooTusk.Entity.BlooTuskModel;
using BlooTusk.Model;
using BlooTusk.Model.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlooTusk.Business.Interface
{
    public interface IMerchantService
    {
        /// <summary>
        /// Method is used to add/edit merchant
        /// </summary>
        /// <param name="merchantModel"></param>
        /// <returns></returns>
        bool AddMerchant(MerchantModel merchantModel, ref ErrorResponseModel errorResponseModel);

        bool EditMerchant(MerchantModel merchantModel, ref ErrorResponseModel errorResponseModel);
        public string NudgeCoupon(NudgeModel nudgeCouponModel, ref ErrorResponseModel errorResponseModel);

        /// <summary>
        /// Method is used to delete Category
        /// </summary>
        /// <param name="merchantId"></param>
        /// <returns></returns>
        bool DeleteMerchant(int merchantId, ref ErrorResponseModel errorResponseModel);

        /// <summary>
        /// Method is used to get CompanyDetails by companyid
        /// </summary>
        /// <param name="merchantId"></param>
        /// <returns></returns>
        MerchantModel GetMerchantById(int merchantId, ref ErrorResponseModel errorResponseModel);

        /// <summary>
        /// Method is used to get all CompanyDetails
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        MerchantSearchResultModel GetAllMerchant(MerchantSerachModel merchantSerachModel, ref ErrorResponseModel errorResponseModel);

        OTPModel SendOTP(OTPModel oTPModel, ref ErrorResponseModel errorResponseModel);

        List<CountryModel> GetCountryDDL(ref ErrorResponseModel errorResponseModel);

        List<StateModel> GetStateDDL(ref ErrorResponseModel errorResponseModel);
        DashboardDataModel GetDashboardData(int merchantId,ref ErrorResponseModel errorResponseModel);

        MerchantInfoModel GetMerchantByCOde(string merchantCode,bool isMerchant, ref ErrorResponseModel errorResponseModel);

        bool AddRemark(RemarkModel remarkModel);
        List<RemarkModel> GetRemarkHistory(int merchantId);

        OTPModel ForgetPasswordOTP(string username);

        bool ResetPassword(ResetPasswordModel resetPasswordModel);

        bool IsDuplicateEmail(string email);

        bool IsDuplicatePhoneNumber(string phoneNumber);

        public bool IsDuplicatePhoneNumberStaff(string phoneNumber);

        bool IsDuplicateEmailStaff(string email);

        bool UpdateProfile(ProfileModel profileModel, ref ErrorResponseModel errorResponseModel);

        ProfileModel GetProfile(int merchantId, ref ErrorResponseModel errorResponseModel);

        PosDetailsModel GetPosDetails(int merchantId, ref ErrorResponseModel errorResponseModel);


    }
}
