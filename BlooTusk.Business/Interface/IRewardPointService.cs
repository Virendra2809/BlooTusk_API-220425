using BlooTusk.Model.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlooTusk.Business.Interface
{
    public interface IRewardPointService
    {
        /// <summary>
        /// Method is used to add/edit Category
        /// </summary>
        /// <param name="RewardPointMaster"></param>
        /// <returns></returns>
        string AddEditRewardPoint(RewardPointModel RewardPointMaster, ref ErrorResponseModel errorResponseModel);

        /// <summary>
        /// Method is used to delete Category
        /// </summary>
        /// <param name="RewardPointId"></param>
        /// <returns></returns>
        bool DeleteRewardPoint(int rewardPointId, ref ErrorResponseModel errorResponseModel);

        /// <summary>
        /// Method is used to get CompanyDetails by companyid 
        /// </summary>
        /// <param name="companyId"></param>
        /// <returns></returns>
        RewardPointModel GetRewardPointById(int RewardPointId, ref ErrorResponseModel errorResponseModel);

        /// <summary>
        /// Method is used to get all CompanyDetails
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        List<RewardPointModel> GetAllRewardPoint( int MerchantId, ref ErrorResponseModel errorResponseModel);
        List<RewardPointModel> GetRewardTypeDDL(ref ErrorResponseModel errorResponseModel);
    }
}
