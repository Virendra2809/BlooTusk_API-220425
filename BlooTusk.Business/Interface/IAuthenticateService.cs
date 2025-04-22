using BlooTusk.Model.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlooTusk.Business.Interface
{
    public interface IAuthenticateService
    {
        /// <summary>
        /// This method is used to validate user credentials
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <param name="errorResponseModel"></param>
        /// <returns></returns>
        AdminAuthenticateModel AuthenticateAdminUser(LoginModel loginModel, ref ErrorResponseModel errorResponseModel);

        MerchantAuthenticateModel AuthenticateMerchantUser(LoginModel loginModel, ref ErrorResponseModel errorResponseModel);
        MerchantAuthenticateModel AuthenticateMerchantSystemUser(LoginModel loginModel, ref ErrorResponseModel errorResponseModel);

        CustomerAuthenticateModel AuthenticateCustomer(CustLoginModel loginModel, ref ErrorResponseModel errorResponseModel);

        OTPModel SendOTP(OTPModel oTPModel, ref ErrorResponseModel errorResponseModel);

    }
}
