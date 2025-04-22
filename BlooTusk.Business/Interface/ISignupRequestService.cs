using BlooTusk.Model.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlooTusk.Business.Interface
{
    public interface ISignupRequestService
    {

        /// <summary>
        /// Method for add & update Signup Request
        /// </summary>
        /// <param name="signupRequestModel"></param>
        /// <returns> bool</returns>
        bool AddSignupRequest(SignupRequestModel signupRequestModel, ref ErrorResponseModel errorResponseModel);
    }
}
