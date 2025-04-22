using BlooTusk.Business.Interface;
using BlooTusk.Common;
using BlooTusk.Entity.BlooTuskModel;
using BlooTusk.Model.Models;
using Google.Protobuf.WellKnownTypes;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MySqlX.XDevAPI.Common;
using System.Text;
using System.Text.RegularExpressions;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;


namespace BlooTusk.Business.Implementation
{
    public class CouponredeemtionService : ICouponredeemtionService 
    {
        private  BlooTuskContext context;
        private  ConfigurationModel configuration;
        public CouponredeemtionService(BlooTuskContext _context, IOptions<ConfigurationModel> configuration)
        {
            context = _context ?? throw new ArgumentNullException(nameof(_context));
            this.configuration = configuration.Value;
        }
        public async Task<CouponredeemtionResultModel> RedeemCoupon(CouponredeemtionModel couponredeemtion, ErrorResponseModel errorResponseModel)
        {
            string MerchantPlace = "";
            string statusCode = "";
            bool result = false;
            int? queryResult = 0; 
            bool isCustomerInMerchant = false;
            Redeemtion couponredeemtion_ = new Redeemtion();
            CouponredeemtionResultModel couponredeemtionResultModel = new CouponredeemtionResultModel();
            string encryptedPhoneNumber = GetEncryptedString(couponredeemtion.phoneNumber);
            int redeemcustId = 0;


            var couponSerialNo = couponredeemtion?.CouponCode; // Null check for couponredeemtion

            if (couponredeemtion.phoneNumber != "")
            {
                isCustomerInMerchant = await context.Customers.Join(context.Customermerchantmappers,
                                              customer => customer.CustomerId,
                                              mapper => mapper.CustomerId,
                                              (customer, mapper) => new { Customer = customer, Mapper = mapper })
                                          .AnyAsync(result => result.Customer.PhoneNumber == encryptedPhoneNumber && result.Mapper.MerchantId == couponredeemtion.MerchantId);

                var firstOrDefaultCustomer = await (from customer_ in context.Customers
                                                    where customer_.PhoneNumber == encryptedPhoneNumber
                                                    select customer_
                                                //    {
                                                   //     customerid = customer_.CustomerId
                                                       
                                                    //}
            ).FirstOrDefaultAsync();

                couponredeemtion.RedeembyCustomerId = firstOrDefaultCustomer.CustomerId;


                int AssignedCouponcustomerId = await context.Couponissuedetails
                   .Where(coupon => coupon.CouponSerialNo == couponSerialNo)
                   .Select(coupon => coupon.CustomerId)
                   .FirstOrDefaultAsync();

                
                var ActiveRedeemCoupon = await (from C in context.Couponmasters
                                                join Cd in context.Couponissuedetails on C.CouponId equals Cd.CouponId
                                                join Ct in context.Customers on Cd.CustomerId equals Ct.CustomerId
                                                where C.RecStatus == "A" &&
                                                      C.MerchantId == couponredeemtion.MerchantId &&
                                                      Ct.RecStatus == "A" &&
                                                      Cd.CouponSerialNo == couponredeemtion.CouponCode &&
                                                      C.EndDate.Date >= DateTime.Now.Date
                                                 && C.StartDate.Date <= DateTime.Now.Date// its add because future date redeem
                                                select new
                                                {
                                                    couponIssuedID = Cd.CouponIssueDetailsId,
                                                }).FirstOrDefaultAsync();

                var CouponTyperesult = await (from C in context.Couponmasters
                                              join Ci in context.Couponissuedetails on C.CouponId equals Ci.CouponId
                                              where Ci.CouponSerialNo == couponredeemtion.CouponCode
                                              select new
                                              {
                                                  CouponType = C.CouponType
                                              }).FirstOrDefaultAsync();

                if (ActiveRedeemCoupon != null)
                {
                    if(CouponTyperesult.CouponType == 2)
                    {
                        var isRedemptionExists = await context.Redeemtions
                                             .AnyAsync(Rd => Rd.CouponIssuedetailsId == ActiveRedeemCoupon.couponIssuedID);

                        if (isRedemptionExists || ActiveRedeemCoupon == null)
                        {
                            ActiveRedeemCoupon = null;
                        }                      
                    }

                    else
                    {
                        //-----///
                        var isRedemptionTransferExists = await context.Redeemtions
                                .AnyAsync(Rd => Rd.CouponIssuedetailsId == ActiveRedeemCoupon.couponIssuedID && Rd.RedeembyCustomerId == firstOrDefaultCustomer.CustomerId);
                        //----//

                        if (isRedemptionTransferExists)
                        {
                            ActiveRedeemCoupon = null;
                        }
                        else
                        {
                            ActiveRedeemCoupon = ActiveRedeemCoupon;
                        }

                    }

                }
                if (ActiveRedeemCoupon != null)
                {
                    if (AssignedCouponcustomerId != couponredeemtion.RedeembyCustomerId)
                    {
                        //coupon transferable or not 
                       

                        if (CouponTyperesult != null)
                        {
                            queryResult = CouponTyperesult.CouponType;
                        }
                        if (queryResult == 2)
                        {
                            ActiveRedeemCoupon = null;
                        }
                        else
                        {
                            ActiveRedeemCoupon = ActiveRedeemCoupon;
                        }
                    }
        
                    #region this is code for new customer
                 
                    if (couponredeemtion.IsNewCustomer == true && couponredeemtion.CouponCode != "" && ActiveRedeemCoupon != null && queryResult == 1)
                    {

                        var RefferalRewardPoint = await context.Rewardpointmasters.Where(x => x.RewardTypeId == 2 && x.MerchantId == couponredeemtion.MerchantId)
                                                            .Select(Rewardmap => new
                                                            {
                                                                RefferalRewardId = Rewardmap.RewardPonitId,
                                                                RefferalRewardPoints = Rewardmap.RewardPoint,
                                                            })
                                                           .FirstOrDefaultAsync();

                        var RefferalrewardtransactionInfo = new Rewardpointtransaction();
                        RefferalrewardtransactionInfo.MerchantId = couponredeemtion.MerchantId;
                        RefferalrewardtransactionInfo.RewardPointId = RefferalRewardPoint.RefferalRewardId;
                        RefferalrewardtransactionInfo.CustomerId = AssignedCouponcustomerId;
                        RefferalrewardtransactionInfo.CreatedBy = couponredeemtion.MerchantId;
                        RefferalrewardtransactionInfo.Points = RefferalRewardPoint.RefferalRewardPoints;
                        RefferalrewardtransactionInfo.TransactionType = 2;
                        RefferalrewardtransactionInfo.Createddate = DateTime.Now;
                        RefferalrewardtransactionInfo.TransactionDate = DateTime.Now;
                        RefferalrewardtransactionInfo.ReffrealId = couponredeemtion.RedeembyCustomerId;
                        context.Rewardpointtransactions.Add(RefferalrewardtransactionInfo);
                        context.SaveChanges();

                        var RefferalRewardPoints = await (from customer_ in context.Customers
                                                          join Customermerchantmapper_ in context.Customermerchantmappers on customer_.CustomerId equals Customermerchantmapper_.ReferBy
                                                          where Customermerchantmapper_.ReferBy == AssignedCouponcustomerId
                                                          select new
                                                          {
                                                              mobileno = customer_.PhoneNumber,
                                                          }
                                                 ).FirstOrDefaultAsync();
  

                        var RefferalByMrssage = (from template_ in context.Smstemplates
                                                 where template_.MessageTypeId == 3 && template_.MerchantId == couponredeemtion.MerchantId
                                                 select new PosDetailsModel
                                                 {
                                                     Posname = template_.MessageContent,
                                                 }
                        ).FirstOrDefault();

                        string custrefPhoneNumber = string.Empty;
                        //if anyone first time customer , customer geting refferalpoint and add entry in customermerchantmapper for a referal
                        // witch is customer using any one coupon 

                        var GetCustomerCode = await (from Customer_ in context.Customers
                                                     where Customer_.CustomerId == couponredeemtion.RedeembyCustomerId
                                                     select new
                                                     {
                                                         customerCode = Customer_.CustomerCode,
                                                     }
                        ).FirstOrDefaultAsync();


                        var custmapperEntity = (from customerMapper_ in context.Customermerchantmappers
                                           where customerMapper_.CustomerId == couponredeemtion.RedeembyCustomerId && customerMapper_.MerchantId == couponredeemtion.MerchantId
                                                      select customerMapper_).FirstOrDefault();

                  
                        custmapperEntity.ReferBy = AssignedCouponcustomerId;
                        custmapperEntity.ApprovlStatus = "A";

                        context.SaveChanges();

                        #region Temp Hide

                        //try
                        //{

                        if(RefferalRewardPoints.mobileno != null)
                        {
                            custrefPhoneNumber = Encoding.UTF8.GetString(CommonUtility.Decrypt(RefferalRewardPoints.mobileno.Trim(), configuration.SymmetricKey));
                    
                          var  RefferalPoint = context.Rewardpointmasters
                                    .Where(custmerchant => custmerchant.RewardTypeId == 2  && custmerchant.MerchantId == couponredeemtion.MerchantId)
                                    .Select(reward => reward.RewardPoint)
                                    .FirstOrDefault();

                            string RefferalByMrssageresults = RefferalByMrssage.Posname;
                            string RefferalByMrssagepattern = @"\[User's Name\]";
                            string RefferalByMrssagereplace = string.IsNullOrEmpty(firstOrDefaultCustomer.Name) ? "User - " + CommonUtility.maskString(Encoding.UTF8.GetString(CommonUtility.Decrypt(firstOrDefaultCustomer.PhoneNumber, configuration.SymmetricKey)), "PhoneNUmber") : (firstOrDefaultCustomer.Name + (string.IsNullOrEmpty(firstOrDefaultCustomer.Lastname) ? "" : " " + firstOrDefaultCustomer.Lastname));
                            RefferalByMrssage.Posname = Regex.Replace(RefferalByMrssageresults, RefferalByMrssagepattern, RefferalByMrssagereplace);


                            string RefferalByMrssageresults2 = RefferalByMrssage.Posname;
                            string RefferalByRewardPoint = @"\[RewardPoint\]";
                            string RefferalByRewardPointreplace = RefferalPoint.ToString();
                            RefferalByMrssage.Posname = Regex.Replace(RefferalByMrssageresults2, RefferalByRewardPoint, RefferalByRewardPointreplace);


                            SendSms(custrefPhoneNumber, RefferalByMrssage.Posname);
                        }

                       
                        //custrefPhoneNumber
                       
                        #endregion

                    }
                    #endregion

                    if (AssignedCouponcustomerId == couponredeemtion.RedeembyCustomerId || queryResult == 1)
                    {

                        var Redeemcode = RedeemCouponCodeAsync(couponredeemtion, ActiveRedeemCoupon.couponIssuedID);
                    }
                }
                bool redemreward = false;

                if (couponredeemtion.RedeemRewardPoints != 0)
                {
                    if (couponredeemtion.CouponCode != "")
                    {
                        if (ActiveRedeemCoupon != null)
                        {
                            redemreward = true;
                        }
                        else
                        {
                            redemreward = false;
                        }
                    }
                    else
                    {
                             var status = await (from customer_ in context.Customers
                                                                 where customer_.PhoneNumber == encryptedPhoneNumber
                                                                 select new
                                                                 {
                                                                     recStatus = customer_.RecStatus,
                                                                 }).FirstOrDefaultAsync();
                        if (status.recStatus != "A")
                        {
                            redemreward = false;
                        }
                        else
                        {
                            redemreward = true;
                        }
                    }

             

                    if (redemreward == true)
                    {
                        int RewardTypeID = 3;
                        int RewardPoint = Convert.ToInt32(couponredeemtion.RedeemRewardPoints);
                        Rewardpointmaster rewardPointmaster = new Rewardpointmaster();
                        rewardPointmaster.RewardPoint = RewardPoint;
                        rewardPointmaster.RewardTypeId = RewardTypeID;
                        rewardPointmaster.Validity = 3;
                        rewardPointmaster.IssuedBy = 1;
                        rewardPointmaster.RewardDate = DateTime.Now.ToShortDateString();
                        rewardPointmaster.IsAdmin = 1;
                        rewardPointmaster.MerchantId = couponredeemtion.MerchantId;
                        rewardPointmaster.RecStatus = 1;
                        rewardPointmaster.CreatedBy = 1;
                        rewardPointmaster.CreatedDate = DateTime.Now;
                        context.Add(rewardPointmaster);
                        context.SaveChanges();

                        var rewardtransactionInfos = new Rewardpointtransaction();
                        rewardtransactionInfos.MerchantId = couponredeemtion.MerchantId;
                        rewardtransactionInfos.RewardPointId = rewardPointmaster.RewardPonitId;
                        rewardtransactionInfos.CustomerId = couponredeemtion.RedeembyCustomerId;
                        rewardtransactionInfos.CreatedBy = couponredeemtion.MerchantId;
                        rewardtransactionInfos.Points = RewardPoint;
                        rewardtransactionInfos.TransactionType = 3;
                        rewardtransactionInfos.Createddate = DateTime.Now;
                        rewardtransactionInfos.TransactionDate = DateTime.Now;
                        context.Rewardpointtransactions.Add(rewardtransactionInfos);
                        context.SaveChanges();
                    }

                    statusCode = "A";
                    result = true;
                }

                #region  This code using fo a front  response

                string testPhoneNumber = GetEncryptedString(couponredeemtion.phoneNumber);
                var rewardSum = (from rewardpointtransaction_ in context.Rewardpointtransactions
                                 join rewardpointmaster_ in context.Rewardpointmasters on rewardpointtransaction_.RewardPointId equals rewardpointmaster_.RewardPonitId
                                 join Customer_ in context.Customers on rewardpointtransaction_.CustomerId equals Customer_.CustomerId
                                 where Customer_.PhoneNumber == testPhoneNumber && (rewardpointmaster_.RewardTypeId == 1 || rewardpointmaster_.RewardTypeId == 2 || rewardpointmaster_.RewardTypeId == 3)
                                 select new
                                 {
                                     RewardPoint = rewardpointmaster_.RewardPoint,
                                     IsPositive = rewardpointmaster_.RewardTypeId != 3
                                 }).ToList();

                // coupon code = value , reward point = value 
                if (couponredeemtion.CouponCode != "" && couponredeemtion.RedeemRewardPoints != 0)
                {

                    var query =  from couponmaster_ in context.Couponmasters
                                join Couponissuedetail_ in context.Couponissuedetails on couponmaster_.CouponId equals Couponissuedetail_.CouponId
                                join merchant in context.Merchants on couponmaster_.MerchantId equals merchant.MerchantId
                                join Customer_ in context.Customers on Couponissuedetail_.CustomerId equals Customer_.CustomerId
                                // Add other joins and conditions if needed
                                where Couponissuedetail_.CouponSerialNo == couponredeemtion.CouponCode && merchant.MerchantId == couponredeemtion.MerchantId
                                select new CouponredeemtionResultModel
                                {
                                    CouponIssueDetailId = Couponissuedetail_.CouponIssueDetailsId,
                                    DiscountValue = couponmaster_.DiscountValue,
                                    DiscountType = couponmaster_.DiscountType,
                                    CouponTitle = couponmaster_.CouponTitle,
                                    CouponDiscerption = couponmaster_.CouponDiscerption,
                                    CustomerPhoneNumber = "User - " + CommonUtility.maskString(Encoding.UTF8.GetString(CommonUtility.Decrypt(Customer_.PhoneNumber, configuration.SymmetricKey)), "PhoneNUmber"),
                                    CustomerName = string.IsNullOrEmpty(Customer_.Name) ? "User - " + CommonUtility.maskString(Encoding.UTF8.GetString(CommonUtility.Decrypt(Customer_.PhoneNumber, configuration.SymmetricKey)), "PhoneNUmber") : (Customer_.Name + (string.IsNullOrEmpty(Customer_.Lastname) ? "" : " " + Customer_.Lastname)),
                                    // Add other properties based on the common logic
                                    MerchantName = merchant.OrganizationName,
                                    RewardCouponTitle = "",
                                    RewardPoint = couponredeemtion.RedeemRewardPoints,
                                };

                    var couponReddemResponse = await query.FirstOrDefaultAsync(); 



                    if (AssignedCouponcustomerId != couponredeemtion.RedeembyCustomerId)
                    {
                       


                        couponReddemResponse.RefferName =  GetRefferNameAsync(couponredeemtion.RedeembyCustomerId);

                        couponReddemResponse.RewardCouponTitle = ActiveRedeemCoupon != null ? "Both" : "Coupon Not Valid";

                        couponredeemtionResultModel = couponReddemResponse;
                    }
                    else if (AssignedCouponcustomerId == couponredeemtion.RedeembyCustomerId)
                    {
                        couponReddemResponse.RefferName = "";
                        couponReddemResponse.RewardCouponTitle = ActiveRedeemCoupon != null ? "Both" : "Coupon Not Valid";
                        couponredeemtionResultModel = couponReddemResponse;
                    }

                    int totalRewardSum = Convert.ToInt32(rewardSum.Sum(item => item.IsPositive ? item.RewardPoint : -item.RewardPoint));

                    string balRewardPOint = totalRewardSum.ToString(); // Convert.ToString(totalRewardSum - couponredeemtion.RedeemRewardPoints);

                    decimal redeemRewardPoints = Convert.ToDecimal(couponredeemtion.RedeemRewardPoints);
                    decimal ActuaclRedeemPoints = redeemRewardPoints / 100;

                    string Reward = couponredeemtion.RedeemRewardPoints + " Points ($" + ActuaclRedeemPoints + ")";


                    if(couponReddemResponse.RewardCouponTitle == "Both")
                    {
                        string PointRedeemSMS = "[RewardPoints] redeemed at [Merchant Place] and your new balance is [Bal Point] points. Login to https://crm.blootusk.com/#/logincustomer to check your points.";

                        string ReplacedMessage = PointRedeemSMS
                       .Replace("[Merchant Place]", couponReddemResponse.MerchantName)
                       .Replace("[Bal Point]", balRewardPOint)
                       .Replace("[RewardPoints]", Reward);

                        SendSms(couponredeemtion.phoneNumber, ReplacedMessage);
                    }
        
                }

           
                // coupon code = value , reward point = 0
                else if (couponredeemtion.CouponCode != "" && couponredeemtion.RedeemRewardPoints == 0)
                {
                    var couponReddemResponse = await (from couponmaster_ in context.Couponmasters
                                                      join Couponissuedetail_ in context.Couponissuedetails on couponmaster_.CouponId equals Couponissuedetail_.CouponId
                                                      join merchant in context.Merchants on couponmaster_.MerchantId equals merchant.MerchantId
                                                      join Customer_ in context.Customers on Couponissuedetail_.CustomerId equals Customer_.CustomerId
                                                      // Add other joins and conditions if needed
                                                      where Couponissuedetail_.CouponSerialNo == couponredeemtion.CouponCode && merchant.MerchantId == couponredeemtion.MerchantId
                                                      select new CouponredeemtionResultModel
                                                      {
                                                          CouponIssueDetailId = Couponissuedetail_.CouponIssueDetailsId,
                                                          DiscountValue = couponmaster_.DiscountValue,
                                                          DiscountType = couponmaster_.DiscountType,
                                                          CouponTitle = couponmaster_.CouponTitle,
                                                          CouponDiscerption = couponmaster_.CouponDiscerption,
                                                          CustomerPhoneNumber = "User - " + CommonUtility.maskString(Encoding.UTF8.GetString(CommonUtility.Decrypt(Customer_.PhoneNumber, configuration.SymmetricKey)), "PhoneNUmber"),
                                                          CustomerName = string.IsNullOrEmpty(Customer_.Name) ? "User - " + CommonUtility.maskString(Encoding.UTF8.GetString(CommonUtility.Decrypt(Customer_.PhoneNumber, configuration.SymmetricKey)), "PhoneNUmber") : (Customer_.Name + (string.IsNullOrEmpty(Customer_.Lastname) ? "" : " " + Customer_.Lastname)),
                                                          // Add other properties based on the common logic
                                                          MerchantName = merchant.OrganizationName,
                                                          RewardCouponTitle = "",
                                                          RewardPoint = 0,
                                                      }).FirstOrDefaultAsync();

                    // Apply additional logic if isCustomerInMerchant is true
                    //  var couponReddemResponse = query.FirstOrDefault();
                    if (AssignedCouponcustomerId != couponredeemtion.RedeembyCustomerId)
                    {

                        //couponReddemResponse.RefferName = (from customer_ in context.Customers
                        //                                   where customer_.CustomerId == couponredeemtion.RedeembyCustomerId
                        //                                   select new
                        //                                   {
                        //                                       name = string.IsNullOrEmpty(customer_.Name) ? "User - " + CommonUtility.maskString(Encoding.UTF8.GetString(CommonUtility.Decrypt(customer_.PhoneNumber, configuration.SymmetricKey)), "PhoneNUmber") : customer_.Name,

                        //                                   }).FirstOrDefault()?.name;


                        couponReddemResponse.RefferName =  GetRefferNameAsync(couponredeemtion.RedeembyCustomerId);


                        couponReddemResponse.RewardCouponTitle = ActiveRedeemCoupon != null ? "Coupon Redeem Successfully" : "Coupon Not Valid";

                        couponredeemtionResultModel = couponReddemResponse;
                    }
                    else if (AssignedCouponcustomerId == couponredeemtion.RedeembyCustomerId)
                    {
                        couponReddemResponse.RefferName = "";
                        couponReddemResponse.RewardCouponTitle = ActiveRedeemCoupon != null ? "Coupon Redeem Successfully" : "Coupon Not Valid";
                        couponredeemtionResultModel = couponReddemResponse;
                    }

                    MerchantPlace = couponReddemResponse.MerchantName;
                }

                //coupon code = "" , rewardpoint = value
                else if (couponredeemtion.CouponCode == "" && couponredeemtion.RedeemRewardPoints != 0)
                {

                    MerchantPlace = (from m in context.Merchants
                                     where m.MerchantId == couponredeemtion.MerchantId
                                     select m.OrganizationName).FirstOrDefault();

                    int totalRewardSum = Convert.ToInt32(rewardSum.Sum(item => item.IsPositive ? item.RewardPoint : -item.RewardPoint));

                    string balRewardPOint = totalRewardSum.ToString(); //Convert.ToString(totalRewardSum - couponredeemtion.RedeemRewardPoints);

                    decimal redeemRewardPoints = Convert.ToDecimal(couponredeemtion.RedeemRewardPoints);
                    decimal ActuaclRedeemPoints = redeemRewardPoints / 100;

                    string Reward = couponredeemtion.RedeemRewardPoints + " Points ("+"$" + ActuaclRedeemPoints + ")";             

                    var redeemcustIds = (from customer_ in context.Customers
                                         join cusmermerchant in context.Customermerchantmappers on customer_.CustomerId equals cusmermerchant.CustomerId
                                         join Merchant_ in context.Merchants on cusmermerchant.MerchantId equals Merchant_.MerchantId
                                         where customer_.PhoneNumber == encryptedPhoneNumber && Merchant_.MerchantId == couponredeemtion.MerchantId
                                         select new CouponredeemtionResultModel
                                         {
                                             DiscountValue = 0,
                                             DiscountType = "",
                                             RefferName = "",
                                             MerchantName = Merchant_.OrganizationName,
                                             CouponTitle = "",
                                             CouponDiscerption = "",
                                             RewardCouponTitle = "",
                                             RewardPoint = couponredeemtion.RedeemRewardPoints,
                                             CustomerName = string.IsNullOrEmpty(customer_.Name) ? "User - " + CommonUtility.maskString(Encoding.UTF8.GetString(CommonUtility.Decrypt(customer_.PhoneNumber, configuration.SymmetricKey)), "PhoneNUmber") : (customer_.Name + (string.IsNullOrEmpty(customer_.Lastname) ? "" : " " + customer_.Lastname)),
                                         }
                            ).FirstOrDefault();

                    var couponReddemResponse = redeemcustIds;

                    if (AssignedCouponcustomerId != couponredeemtion.RedeembyCustomerId)
                    {
                        couponReddemResponse.RefferName =  GetRefferNameAsync(couponredeemtion.RedeembyCustomerId);

                        couponReddemResponse.RewardCouponTitle = redemreward == true ? "Reward Point Redeemed" : "Customer Inactive";

                        couponredeemtionResultModel = couponReddemResponse;
                    }
                    else if (AssignedCouponcustomerId == couponredeemtion.RedeembyCustomerId)
                    {
                        couponReddemResponse.RefferName = "";
                        couponReddemResponse.RewardCouponTitle = redemreward == true ? "Reward Point Redeemed" : "Customer Inactive";
                        couponredeemtionResultModel = couponReddemResponse;
                    }


                    if(couponReddemResponse.RewardCouponTitle == "Reward Point Redeemed")
                    {
                        string PointRedeemSMS = "[RewardPoints] redeemed at [Merchant Place] and your new balance is [Bal Point] points. Login to https://crm.blootusk.com/#/logincustomer to check your points.";

                        string ReplacedMessage = PointRedeemSMS
                            .Replace("[Merchant Place]", MerchantPlace)
                            .Replace("[Bal Point]", balRewardPOint)
                            .Replace("[RewardPoints]", Reward);

                        SendSms(couponredeemtion.phoneNumber, ReplacedMessage);
                    }
             


                    redeemcustIds.RewardCouponTitle = couponReddemResponse.RewardCouponTitle;
                    couponredeemtionResultModel = redeemcustIds;
                }
                #endregion

            }
        

            return couponredeemtionResultModel;

        }
        private async Task<string> RedeemCouponCodeAsync(CouponredeemtionModel couponredeemtion, int couponIssuedID)
        {
            Redeemtion couponredeemtion_ = new Redeemtion();
           
            try
            {
                if (couponredeemtion.CouponCode != null)
                {           

                    var CouponIssueDetailsUpdate = (from Cid in context.Couponissuedetails
                                                    where Cid.CouponSerialNo == couponredeemtion.CouponCode
                                                    select Cid).FirstOrDefault();

                    couponredeemtion_.CouponredeemtionDate = DateTime.Now;
                    couponredeemtion_.PosId = couponredeemtion.PosId;
                    couponredeemtion_.CouponIssuedetailsId = CouponIssueDetailsUpdate.CouponIssueDetailsId;
                    couponredeemtion_.RedeembyCustomerId = couponredeemtion.RedeembyCustomerId;
                    couponredeemtion_.RecStatus = 1;

                    if (couponredeemtion.RedeemRewardPoints != 0)
                    {
                        couponredeemtion_.Points = couponredeemtion.RedeemRewardPoints;
                    }
                    else
                    {
                        couponredeemtion_.Points = 0;
                    }

                    couponredeemtion_.CreatedDate = Convert.ToString(DateTime.Now);
                    couponredeemtion_.CreatedBy = couponredeemtion.CreatedBy;
                    context.Add(couponredeemtion_);
                    context.SaveChanges();

                    var CouponIssuedDetail_ =  context.Couponissuedetails.SingleOrDefault(x => x.CouponIssueDetailsId == CouponIssueDetailsUpdate.CouponIssueDetailsId);

                    if (CouponIssuedDetail_ != null)
                    {
                        CouponIssuedDetail_.CouponIssueDetailsId = couponIssuedID;
                        CouponIssuedDetail_.RedeemId = couponredeemtion.CouponredeemtionId;
                        CouponIssuedDetail_.UsedbyId = couponredeemtion.RedeembyCustomerId;
                         context.SaveChanges();
                    }

                    return "A";
                    // Rest of your code...
                }
                else
                {
                    // Handle the case when couponredeemtion.CouponCode is null.
                }


                // CouponIssueDetailsId
                // Reward point transaction Entry Saved

                return "A";
            }
            catch (Exception ex)
            {
                // Handle exceptions or log errors
                return "E";
            }
        }
        public string GetRefferNameAsync(int redeemByCustomerId)
        {
            string name = "";
            var customerNameInfo =  (from customer_ in context.Customers
                                          where customer_.CustomerId == redeemByCustomerId
                                          select new
                                          {
                                              Name = string.IsNullOrEmpty(customer_.Name)
                                                  ? "User - " + CommonUtility.maskString(Encoding.UTF8.GetString(CommonUtility.Decrypt(customer_.PhoneNumber, configuration.SymmetricKey)), "PhoneNUmber")
                                                  : (customer_.Name + (string.IsNullOrEmpty(customer_.Lastname) ? "" : " " + customer_.Lastname))
                                          })
                                          .FirstOrDefault();
            name = customerNameInfo.Name;

            return name;
        }
        public string GetEncryptedString(string value)
        {
            try
            {

                return Convert.ToBase64String(CommonUtility.Encrypt(value, configuration.SymmetricKey));
            }
            catch (Exception ex)
            {

                throw ex;
            }

        }
        private bool SendSms(string phonenumber, string verificationMessage)
        {
            string ToNumber = string.Empty;
            bool result = false;
            using (var web = new System.Net.WebClient())
            {
                try
                {
                    if (configuration.Bypassgetway == true)
                    {

                        TwilioClient.Init(configuration.ACCOUNT_SID, configuration.AUTH_TOKEN);
                        //+918408845409

                        if (configuration.MobileCountryCode != null)
                        {
                            ToNumber = configuration.MobileCountryCode + phonenumber;
                        }
                        else
                        {
                            ToNumber = phonenumber;
                        }
                        var message = MessageResource.Create(
                            new PhoneNumber(ToNumber),
                            from: new PhoneNumber(configuration.FromMobileNumber),
                            body: verificationMessage
                        );
                        Console.WriteLine(message.Sid);
                        Errorlog errorlog = new Errorlog();
                        errorlog.ErrorLog1 = "Send Sms to " + ToNumber + "  And SID ID" + message.Sid;
                        context.Errorlogs.Add(errorlog);
                        context.SaveChanges();
                        result = true;
                    }
                    else
                    {
                        result = true;
                    }
                }
                catch (Exception ex)
                {
                    Errorlog errorlog = new Errorlog();
                    errorlog.ErrorLog1 = "Error In sending sms to " + ToNumber + " Error Message is " + ex.Message;
                    context.Errorlogs.Add(errorlog);
                    context.SaveChanges();
                    throw ex;
                    result = false;
                }
            }
            return result;
        }
        private string GetDecryptedString(string value)
        {
            return Encoding.UTF8.GetString(CommonUtility.Decrypt(value, configuration.SymmetricKey));
        }
        public bool valideRedeemCoupon(CouponredeemtionModel couponredeemtion, ref ErrorResponseModel errorResponseModel)
        {
           bool result = false;
            var redemtion = 0;
            CouponredeemtionResultModel couponredeemtionResultModel = new CouponredeemtionResultModel();


            string encryptedPhoneNumber = GetEncryptedString(couponredeemtion.phoneNumber);
             


                //here we will check coupon is active , not redeemed and not expired
                var ActiveRedeemCoupon = (from C in context.Couponmasters
                                          join Cd in context.Couponissuedetails on C.CouponId equals Cd.CouponId
                                          where
                                          C.RecStatus == "A" && Cd.UsedbyId != 0
                                          && C.MerchantId == couponredeemtion.MerchantId
                                          && Cd.CouponSerialNo == couponredeemtion.CouponCode 
                                          && C.EndDate > DateTime.Today
                                          select new
                                          {
                                              couponIssuedID = Cd.CouponIssueDetailsId,// because we need to update status
                                          
                                          }).FirstOrDefault();


            redemtion = (from Cr in context.Redeemtions
                         where Cr.CouponIssuedetailsId == ActiveRedeemCoupon.couponIssuedID
                         select Cr).Count();

            //its transferable
            var queryResult = (from C in context.Couponmasters
                               join Ci in context.Couponissuedetails on C.CouponId equals Ci.CouponId
                               where Ci.CouponSerialNo == couponredeemtion.CouponCode
                               select new
                               {
                                   CouponType = C.CouponType
                               }).FirstOrDefault();

            //  redemtion

          


         

            return result;
          
        }

    }

}
