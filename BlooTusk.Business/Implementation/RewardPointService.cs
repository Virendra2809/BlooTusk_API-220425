using BlooTusk.Business.Interface;
using BlooTusk.Common;
using BlooTusk.Entity.BlooTuskModel;
using BlooTusk.Model.Models;
using System.Globalization;
using System.Net;

namespace BlooTusk.Business.Implementation
{
    public class RewardPointService : IRewardPointService
    {
        BlooTuskContext context;
        public RewardPointService(BlooTuskContext _context)
        {
            context = _context;
        }
        public string AddEditRewardPoint(RewardPointModel rewardPointMasters, ref ErrorResponseModel errorResponseModel)
        {
            string statusCode = "";
            bool result=false;
            Rewardpointmaster rewardPointmaster = new Rewardpointmaster(); 

            try
            {
                if (rewardPointMasters.RewardPonitId == 0)
                {

                    var isDuplicateRewardPoint = context.Rewardpointmasters.Where(x => (x.MerchantId) == rewardPointMasters.MerchantId && x.RewardTypeId == rewardPointMasters.RewardTypeID && (x.IssuedBy == 0 )).FirstOrDefault();

                    var isDuplicateTemplatemessage = context.Rewardpointmasters.Where(x => (x.MerchantId) == rewardPointMasters.MerchantId && x.RewardTypeId == rewardPointMasters.RewardTypeID && (x.IssuedBy == 1)).FirstOrDefault();

                    if ((isDuplicateRewardPoint == null && isDuplicateTemplatemessage != null && rewardPointMasters.IssuedBy != 1) || (isDuplicateRewardPoint != null && isDuplicateTemplatemessage == null&& rewardPointMasters.IssuedBy==0) || (isDuplicateRewardPoint == null && isDuplicateTemplatemessage == null))
                    { 

                        rewardPointmaster.RewardPoint = rewardPointMasters.RewardPoint;
                        rewardPointmaster.RewardTypeId = rewardPointMasters.RewardTypeID;
                        rewardPointmaster.Validity  = rewardPointMasters.Validity;
                        rewardPointmaster.IssuedBy = rewardPointMasters.IssuedBy; 
                        rewardPointmaster.RewardDate = rewardPointMasters.RewardDate;
                        rewardPointmaster.IsAdmin = rewardPointMasters.IsAdmin;
                        rewardPointmaster.Validity = rewardPointMasters.Validity;
                        rewardPointmaster.MerchantId = rewardPointMasters.MerchantId;
                        rewardPointmaster.ModifyBy = rewardPointMasters.ModifyBy;
                        rewardPointmaster.ModifiedDate = rewardPointMasters.ModifiedDate;
                        rewardPointmaster.RecStatus = 1;
                        rewardPointmaster.CreatedBy = 1;
                        rewardPointmaster.CreatedDate = DateTime.Now;
                        context.Add(rewardPointmaster);
                        context.SaveChanges();
                        result = true;
                        statusCode = "A";

                    }
                    else
                    {
                      //  errorResponseModel.Message = GlobalConstants.DuplicateRewardPoint;
                        result = false;
                        statusCode = "DR";

                    }
                }
                else
                {
                    var rewardPointEntity = context.Rewardpointmasters.FirstOrDefault(x => x.RewardPonitId == rewardPointMasters.RewardPonitId);
                                    
                    
                    if (rewardPointEntity != null)
                    {
                        rewardPointEntity.RewardPoint = rewardPointMasters.RewardPoint;
                        rewardPointEntity.RewardTypeId = rewardPointEntity.RewardTypeId;
                        rewardPointEntity.Validity = rewardPointMasters.Validity;
                        rewardPointEntity.IssuedBy = rewardPointMasters.IssuedBy;
                        rewardPointEntity.RewardDate = rewardPointMasters.RewardDate;
                        rewardPointEntity.IsAdmin = rewardPointMasters.IsAdmin;
                        rewardPointEntity.MerchantId = rewardPointMasters.MerchantId;
                        rewardPointEntity.ModifyBy = rewardPointMasters.ModifyBy;
                        rewardPointEntity.ModifiedDate = rewardPointMasters.ModifiedDate;
                        rewardPointEntity.RecStatus = 1;
                        rewardPointEntity.CreatedBy = rewardPointmaster.CreatedBy;
                        rewardPointEntity.CreatedDate = DateTime.Now;
                        //  rewardPointEntity.RecStatus = rewardPointMaster.RecStatus;
                        context.SaveChanges();
                        result = true;
                        statusCode = "U";
                    }
                    else
                    {
                        result = false;
                        statusCode = "NF";
                    }

                }
               
            }
            catch (Exception ex)
            {
                throw ;
            }
            
            return statusCode;
        }

        public bool DeleteRewardPoint(int rewardPointId, ref ErrorResponseModel errorResponseModel)
        {
            bool result = false;
            try
            {
                var rewardPointEntity = context.Rewardpointmasters.FirstOrDefault(x => x.RewardPonitId == rewardPointId);
                if (rewardPointEntity != null)
                {
                   // rewardPointEntity.RecStatus = "I";
                    context.SaveChanges();
                    // Message = "RewardPoint sucessfully dateled";
                    result = true;
                }
            }
            catch (Exception ex)
            {
                errorResponseModel.Message = ex.Message;
                result = false;
            }
            
           
            return result;
        }

        public List<RewardPointModel> GetAllRewardPoint(int MerchantId, ref ErrorResponseModel errorResponseModel)
        {
            errorResponseModel = new ErrorResponseModel();

            var rewardPointList = (from rewardPointMaster in context.Rewardpointmasters
                                   join rewardtypemaster_ in context.Rewardtypemasters on rewardPointMaster.RewardTypeId equals rewardtypemaster_.RewardTypeId
                                   where rewardPointMaster.MerchantId == MerchantId
                                   // && rewardPointMaster.RecStatus == "A"

                                   select new RewardPointModel
                                   {
                                       RewardPonitId = rewardPointMaster.RewardPonitId,
                                       RewardDate = rewardPointMaster.RewardDate,//('dd/MM/yyyy'),
                                       RewardPoint = rewardPointMaster.RewardPoint,
                                       RewardType = rewardtypemaster_.RewardType,
                                       Validity = rewardPointMaster.Validity,
                                       IssuedByName = rewardPointMaster.IssuedBy.Equals(0) ? "Merchant" : "BlooTusk Team",
                                     

                                   }).ToList();
            return rewardPointList;

        }

        public RewardPointModel GetRewardPointById(int rewardPointId, ref ErrorResponseModel errorResponseModel)
        {
            var rewardPointData = (from rewardPointMaster in context.Rewardpointmasters
                                   join merchantdata in context.Merchants on rewardPointMaster.MerchantId equals merchantdata.MerchantId
                                   where rewardPointMaster.RewardPonitId == rewardPointId
                                   //&& rewardPointMaster.RecStatus == "A"
                                   select new RewardPointModel
                                  {
                                      RewardPonitId = rewardPointMaster.RewardPonitId,
                                      RewardType = rewardPointMaster.RewardTypeId.Equals(0) ? "Merchant" : "Blootusk Team",
                                      RewardPoint = rewardPointMaster.RewardPoint,
                                      IssuedBy = rewardPointMaster.IssuedBy,
                                      RewardDate = rewardPointMaster.RewardDate,
                                      RewardTypeID = rewardPointMaster.RewardTypeId,
                                      MerchantId = merchantdata.MerchantId,
                                      organizationName = merchantdata.OrganizationName,
                                      phoneNumber = merchantdata.PhoneNumber,
                                      contactPersonName = merchantdata.ContactPersonName,
                                      email = merchantdata.Email,
                                      Validity = rewardPointMaster.Validity,
                                       //  RecStatus=rewardPointMaster.RecStatus,
                                   }
                               ).SingleOrDefault();

            if (rewardPointData == null)
            {
                errorResponseModel.StatusCode = HttpStatusCode.NotFound;
                errorResponseModel.Message = "No Data found";
            }

            return rewardPointData;
        }

        public List<RewardPointModel> GetRewardTypeDDL(ref ErrorResponseModel errorResponseModel)
        {

            errorResponseModel = new ErrorResponseModel();

            var rewardPointList = (from rewardPointMaster in context.Rewardtypemasters
                                       // where rewardPointMaster.RecStatus == "A"
                                   select new RewardPointModel
                                  {
                                       RewardTypeID = rewardPointMaster.RewardTypeId,
                                      RewardType = rewardPointMaster.RewardType, 
                                  }
                               ).Distinct().ToList();

            if (rewardPointList == null)
            {
                errorResponseModel.StatusCode = HttpStatusCode.NotFound;
                errorResponseModel.Message = "No Data found";
            }
            //var rewardPointList = new List<RewardPointModel>();
            //var rewardPointEntity = context.RewardPointmasters.Where(x => x.RecStatus == "A").ToList();

            //if (rewardPointEntity == null)
            //{
            //    errorResponseModel.StatusCode = HttpStatusCode.NotFound;
            //    errorResponseModel.Message = "No Data found";
            //}

            //rewardPointEntity.ForEach(item =>
            //{
            //    rewardPointList.Add(new RewardPointModel
            //    {
            //        RewardPointId = item.RewardPointId,
            //        RewardPointName = item.RewardPointName,
            //    });
            //});
            return rewardPointList;
        }

       
    
    }
}
