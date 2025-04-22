using BlooTusk.Model.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlooTusk.Business.Interface
{
    public interface IUserService
    {
       // List<UserModel> GetSignupUserList(int merchantId,ref ErrorResponseModel errorResponseModel);

        // List<UserModel> GetUsersList(UserSearchModel userSearchModel);

        /// <summary>
        /// Method is used to add/edit merchant
        /// </summary>
        /// <param name="merchantModel"></param>
        /// <returns></returns>
        //bool AddUser(UserModel userModel, ref ErrorResponseModel errorResponseModel);

       // bool UpdateUserApprovalStatus(int merchantId,int userId, ref ErrorResponseModel errorResponseModel);
       // bool checkDuplicateUser(string phoneNumber);
        OTPModel SendOTP(OTPModel oTPModel, ref ErrorResponseModel errorResponseModel);

        bool AddCustomer(CustomerModel customerModel, ref ErrorResponseModel errorResponseModel);

        bool UpdateCustomerMapper(CustomerMerchantModel customerMerchantModel, ref ErrorResponseModel errorResponseModel);
        bool checkDuplicateCustomer(string phoneNumber, int MerchantId);
        List<CustomerSearchResultModel> GetCustomerList(CustomerSearchModel customerSearchModel);

        CustomerSearchResultModel GetCustomerRefferalList(CustomerSearchModel refModel);

        CustomerRewardPointResultModel GetCustomerRewardPointListList(int rewardPointId, string PhoneNumber);

        CustomerInfoModel GetCustomerByCOde(string custumerCode, ref ErrorResponseModel errorResponseModel);

        public int GetReferalCount(string phoneNumber);

        public int GetRewardPointSum(string phoneNumber);

        public List<object> GetRewardPointMerchantWiseCount(string phoneNumber);

        public int GetDiscountCouponCount(string PhoneNumber);

        public string GetReferalLink(int custcode);

        public bool UpdateCustomer(CustomerModel customerModel, ref ErrorResponseModel errorResponseModel);

        public refferalLinkModel GetReferalLinkList(string PhoneNumber);





    }
}
