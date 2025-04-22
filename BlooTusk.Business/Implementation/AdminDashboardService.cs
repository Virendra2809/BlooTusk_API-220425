using BlooTusk.Business.Interface;
using BlooTusk.Common;
using BlooTusk.Entity.BlooTuskModel;
using BlooTusk.Model.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Globalization;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;


namespace BlooTusk.Business.Implementation
{
    public class AdminDashboardService : IAdminDashboardService
    {
        BlooTuskContext context;
        private ConfigurationModel configuration;
        private EmailSettings emailSettings;


        private readonly string imageDirectory = Directory.GetCurrentDirectory();

        public AdminDashboardService(BlooTuskContext _context, IOptions<ConfigurationModel> configuration, IOptions<EmailSettings> emailSettings)
        {
            context = _context;
            this.configuration = configuration.Value;
            this.emailSettings = emailSettings.Value;
        }

        public AdminDashboardModel AdminDashboardDetails(int MerchantId)
        {
            AdminDashboardModel adminDashboardModel = new AdminDashboardModel();

            DateTime currentDate = DateTime.Now;
            var firstDayOfMonth = new DateTime(currentDate.Year, currentDate.Month, 1);
            var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

            if (MerchantId == 0)
            {
                var dashboardCampaginData = (from Couponmaster_ in context.Couponmasters
                                                 //  where Couponmaster_.MerchantId == MerchantId
                                             select new
                                             {
                                                 campaignid = Couponmaster_.CouponId,
                                                 EndDate = Couponmaster_.EndDate,
                                             })
                    .ToList();


                adminDashboardModel.Merchants = context.Merchants
                                                    // .Where(c => c.CouponId == firstCampaignId)
                                                    .Count();

                adminDashboardModel.MerchantsPending = context.Merchants
                                     .Where(c => c.ApprovalStatus == "N")
                                    .Count();

                adminDashboardModel.MerchantsMtd = context.Merchants
                                                      .Where(merchant =>
                                                          merchant.CreatedDate >= firstDayOfMonth &&
                                                          merchant.CreatedDate <= lastDayOfMonth
                                                      )
                                                      .Count();

                adminDashboardModel.Campaign = dashboardCampaginData.Count;

                if (dashboardCampaginData.Count > 0)
                {
                    int firstCampaignId = dashboardCampaginData[0].campaignid;

                    //   adminDashboardModel.Coupon = context.Couponissuedetails
                    //                                       .Count(c => dashboardCampaginData.Select(d => d.campaignid)
                    //                                       .Contains(c.CouponId));



                    //adminDashboardModel.CouponRedeem = context.Redeemtions
                    //                                      // .Where(c =>  (c.RedeemId != null || c.RedeemId != 0))
                    //                                       .Count();

                    adminDashboardModel.CouponTransfer = context.Couponissuedetails
                                                            .Join(context.Couponmasters, c => c.CouponId, cm => cm.CouponId, (c, cm) => new { c, cm })
                                                            .Where(joined => joined.cm.CouponType == 1)// && joined.c.CouponId == firstCampaignId)
                                                            .Count();

                }

                // Total points from various transactions (SignUp, Referral, Redeem)
                var DashboardrewardSum = (from rewardpointtransaction_ in context.Rewardpointtransactions
                                          where
                                          //    rewardpointtransaction_.MerchantId == MerchantId && 
                                          (rewardpointtransaction_.TransactionType == 1 || rewardpointtransaction_.TransactionType == 2 || rewardpointtransaction_.TransactionType == 3)
                                          select new
                                          {
                                              RewardPoint = rewardpointtransaction_.Points,
                                              IsPositive = rewardpointtransaction_.TransactionType != 3
                                          }).ToList();

                // Summing up the points with consideration of transaction type
                adminDashboardModel.Points = Convert.ToInt32(DashboardrewardSum.Sum(item => item.IsPositive ? item.RewardPoint : -item.RewardPoint));



                var DashboardrewardSumPoint = (from rewardpointtransaction_ in context.Rewardpointtransactions
                                               where
                                               //rewardpointtransaction_.MerchantId == MerchantId
                                                      (rewardpointtransaction_.TransactionType == 1 || rewardpointtransaction_.TransactionType == 2 || rewardpointtransaction_.TransactionType == 3)
                                                     && rewardpointtransaction_.TransactionDate >= firstDayOfMonth
                                                     && rewardpointtransaction_.TransactionDate <= lastDayOfMonth
                                               select new
                                               {
                                                   RewardPoint = rewardpointtransaction_.Points,
                                                   IsPositive = rewardpointtransaction_.TransactionType != 3
                                               }).ToList();

                adminDashboardModel.PointsMtd = Convert.ToInt32(DashboardrewardSumPoint.Sum(item => item.IsPositive ? item.RewardPoint : -item.RewardPoint));


                // Points from SignUp transactions
                var DashboardSignUpPoint = (from rewardpointtransaction_ in context.Rewardpointtransactions
                                            where
                                            //rewardpointtransaction_.MerchantId == MerchantId && 
                                            (rewardpointtransaction_.TransactionType == 1)
                                            select new
                                            {
                                                RewardPoint = rewardpointtransaction_.Points,
                                            }).Sum(x => x.RewardPoint);

                adminDashboardModel.SignInPoint = DashboardSignUpPoint;


                // Points from Referral transactions
                var DashboardRefferalPoint = (from rewardpointtransaction_ in context.Rewardpointtransactions
                                              where
                                              //rewardpointtransaction_.MerchantId == MerchantId && 
                                              (rewardpointtransaction_.TransactionType == 2)
                                              select new
                                              {
                                                  RewardPoint = rewardpointtransaction_.Points,
                                              }).Sum(x => x.RewardPoint);

                adminDashboardModel.RefferalPoint = DashboardRefferalPoint;

                adminDashboardModel.ReddemPointsMtd = (from rewardpointtransaction_ in context.Rewardpointtransactions
                                                       where // rewardpointtransaction_.MerchantId == MerchantId &&
                                                              rewardpointtransaction_.TransactionType == 3
                                                             && rewardpointtransaction_.TransactionDate >= firstDayOfMonth
                                                             && rewardpointtransaction_.TransactionDate <= lastDayOfMonth
                                                       select new
                                                       {
                                                           RewardPoint = rewardpointtransaction_.Points,
                                                       }).Sum(x => x.RewardPoint);



                // Points from Redeem transactions
                //adminDashboardModel.ReddemPoints = (from rewardpointtransaction_ in context.Rewardpointtransactions
                //                                where 
                //                                //rewardpointtransaction_.MerchantId == MerchantId &&
                //                                (rewardpointtransaction_.TransactionType == 3)
                //                                select new
                //                                {
                //                                    RewardPoint = rewardpointtransaction_.Points,
                //                                }).Sum(x => x.RewardPoint);


                // adminDashboardModel.EarnPoint = DashboardSignUpPoint + DashboardRefferalPoint - adminDashboardModel.ReddemPoints;


                //adminDashboardModel.Users = context.Customers
                //                         //   .Where(customermerchant => customermerchant.MerchantId == MerchantId)
                //                            .Count();


                adminDashboardModel.Users = context.Customers
                                          .Join(context.Customermerchantmappers,
                                              customer => customer.CustomerId,
                                              customerMapper => customerMapper.CustomerId,
                                              (customer, customerMapper) => new { Customer = customer, CustomerMapper = customerMapper })
                                          // .Where(joinResult => joinResult.CustomerMapper.MerchantId == MerchantId)
                                          .Count();

                adminDashboardModel.UsersMtd = context.Customers
                                                .Where(customer =>
                                                       customer.CreatedDate >= firstDayOfMonth &&
                                                       customer.CreatedDate <= lastDayOfMonth
                                                   // && customer.MerchantId == MerchantId // Uncomment and adjust if necessary
                                                   ).Count();

                //adminDashboardModel.UserRefferal =  context.Customermerchantmappers
                //             .Where(customermerchant =>  customermerchant.ReferBy != 0)
                //             .Count();

                //var test =  from redemption in context.Redeemtions
                //   group redemption by redemption.RedeembyCustomerId into grouped
                //   select new
                //   {
                //       CustomerID = grouped.Key,
                //       TotalRedemptions = grouped.Count(),
                //   };

                //DateTime lastWeekStart = DateTime.Now.Date.AddDays(-7);  // Get the start date of last week
                //DateTime lastWeekEnd = DateTime.Now.Date;

                adminDashboardModel.UserWalkIn = context.Adminusergraphs.Sum(item => item.WalkInCount); //adminDashboardModel.Users - adminDashboardModel.UserRefferal;

                adminDashboardModel.UserRefferal = context.Adminusergraphs.Sum(item => item.ReferralCount); //adminDashboardModel.Users - adminDashboardModel.UserRefferal;

                adminDashboardModel.FrequentUser = context.Adminfrequentusergraphs.Sum(item => item.TotalFrequentUserCount);

                adminDashboardModel.FrequentUserVisit = context.Adminfrequentusergraphs.Sum(item => item.TotalVisitCount);

                adminDashboardModel.ReddemPoints = context.Adminpointgraphs.Sum(item => item.Redeem);

                adminDashboardModel.EarnPoint = context.Adminpointgraphs.Sum(item => item.Earned);

                adminDashboardModel.Coupon = context.Admincoupongraphs.Sum(item => item.TotalCouponCount);

                adminDashboardModel.CouponRedeem = context.Admincoupongraphs.Sum(item => item.RedeemCount);

            }
            else
            {
                adminDashboardModel.Merchants = 1;

                adminDashboardModel.MerchantsPending = context.Merchants
                                                     .Where(c => c.ApprovalStatus == "N" && c.MerchantId == MerchantId)
                                                     .Count();

                adminDashboardModel.MerchantsMtd = context.Merchants
                                                      .Where(merchant =>
                                                          merchant.CreatedDate >= firstDayOfMonth &&
                                                          merchant.CreatedDate <= lastDayOfMonth &&
                                                          merchant.MerchantId == MerchantId
                                                      )
                                                      .Count();
                adminDashboardModel.Campaign = context.Couponmasters
                    .Where(couponMaster => couponMaster.MerchantId == MerchantId)
                    .Select(couponMaster => new
                    {
                        campaignid = couponMaster.CouponId,
                        EndDate = couponMaster.EndDate
                    })
                    .ToList()
                    .Count;


                if (adminDashboardModel.Campaign > 0)
                {

                    adminDashboardModel.Coupon = context.Admincoupongraphs.Where(item => item.MerchantId == MerchantId)
                                            .Sum(item => item.TotalCouponCount);

                    adminDashboardModel.CouponRedeem = context.Admincoupongraphs.Where(item => item.MerchantId == MerchantId)
                                           .Sum(item => item.RedeemCount);

                    adminDashboardModel.CouponTransfer = context.Couponissuedetails
                                                        .Join(context.Couponmasters, c => c.CouponId, cm => cm.CouponId, (c, cm) => new { c, cm })
                                                        .Where(joined => joined.cm.CouponType == 1 && joined.cm.MerchantId == MerchantId)
                                                        .Count();
                }

                var DashboardrewardSum = (from rewardpointtransaction_ in context.Rewardpointtransactions
                                          where (rewardpointtransaction_.TransactionType == 1 || rewardpointtransaction_.TransactionType == 2 || rewardpointtransaction_.TransactionType == 3)
                                          && rewardpointtransaction_.MerchantId == MerchantId
                                          select new
                                          {
                                              RewardPoint = rewardpointtransaction_.Points,
                                              IsPositive = rewardpointtransaction_.TransactionType != 3
                                          }).ToList();

                adminDashboardModel.Points = Convert.ToInt32(DashboardrewardSum.Sum(item => item.IsPositive ? item.RewardPoint : -item.RewardPoint));

                var DashboardrewardSumPoint = (from rewardpointtransaction_ in context.Rewardpointtransactions
                                               where (rewardpointtransaction_.TransactionType == 1 || rewardpointtransaction_.TransactionType == 2 || rewardpointtransaction_.TransactionType == 3)
                                                  && rewardpointtransaction_.TransactionDate >= firstDayOfMonth
                                                  && rewardpointtransaction_.TransactionDate <= lastDayOfMonth
                                                  && rewardpointtransaction_.MerchantId == MerchantId
                                               select new
                                               {
                                                   RewardPoint = rewardpointtransaction_.Points,
                                                   IsPositive = rewardpointtransaction_.TransactionType != 3
                                               }).ToList();

                adminDashboardModel.PointsMtd = Convert.ToInt32(DashboardrewardSumPoint.Sum(item => item.IsPositive ? item.RewardPoint : -item.RewardPoint));

                var DashboardSignUpPoint = context.Rewardpointtransactions
                                          .Where(rewardpointtransaction_ => rewardpointtransaction_.TransactionType == 1 && rewardpointtransaction_.MerchantId == MerchantId)
                                          .Sum(x => x.Points);

                adminDashboardModel.SignInPoint = DashboardSignUpPoint;

                var DashboardRefferalPoint = context.Rewardpointtransactions
                                            .Where(rewardpointtransaction_ => rewardpointtransaction_.TransactionType == 2 && rewardpointtransaction_.MerchantId == MerchantId)
                                            .Sum(x => x.Points);

                adminDashboardModel.RefferalPoint = DashboardRefferalPoint;

                adminDashboardModel.ReddemPointsMtd = context.Rewardpointtransactions
                                                     .Where(rewardpointtransaction_ => rewardpointtransaction_.TransactionType == 3
                                                        && rewardpointtransaction_.TransactionDate >= firstDayOfMonth
                                                        && rewardpointtransaction_.TransactionDate <= lastDayOfMonth
                                                        && rewardpointtransaction_.MerchantId == MerchantId)
                                                     .Sum(x => x.Points);

                adminDashboardModel.ReddemPoints = context.Adminpointgraphs
                                            .Where(item => item.MerchantId == MerchantId)
                                            .Sum(item => item.Redeem);

                adminDashboardModel.EarnPoint = context.Adminpointgraphs
                                            .Where(item => item.MerchantId == MerchantId)
                                            .Sum(item => item.Earned);

                adminDashboardModel.Users = context.Customers
                                           .Join(context.Customermerchantmappers,
                                               customer => customer.CustomerId,
                                               customerMapper => customerMapper.CustomerId,
                                               (customer, customerMapper) => new { Customer = customer, CustomerMapper = customerMapper })
                                           .Where(joinResult => joinResult.CustomerMapper.MerchantId == MerchantId)
                                           .Count();


                adminDashboardModel.UsersMtd = context.Customers
                                                 .Join(context.Customermerchantmappers,
                                                     customer => customer.CustomerId,
                                                     customerMapper => customerMapper.CustomerId,
                                                     (customer, customerMapper) => new { Customer = customer, CustomerMapper = customerMapper })
                                                 .Where(joinResult =>
                                                     joinResult.Customer.CreatedDate >= firstDayOfMonth &&
                                                     joinResult.Customer.CreatedDate <= lastDayOfMonth &&
                                                     joinResult.CustomerMapper.MerchantId == MerchantId
                                                 )
                                                 .Count();


                adminDashboardModel.UserRefferal = context.Adminusergraphs
                                            .Where(item => item.MerchantId == MerchantId)
                                            .Sum(item => item.ReferralCount);

                adminDashboardModel.UserWalkIn = context.Adminusergraphs
                                            .Where(item => item.MerchantId == MerchantId)
                                            .Sum(item => item.WalkInCount);

                adminDashboardModel.FrequentUser = context.Adminfrequentusergraphs
                                            .Where(item => item.MerchantId == MerchantId)
                                            .Sum(item => item.TotalFrequentUserCount);

                // Get the count of distinct customers who have made multiple redemptions and are in FrequentUsers list
                adminDashboardModel.FrequentUserVisit = context.Adminfrequentusergraphs
                                            .Where(item => item.MerchantId == MerchantId)
                                            .Sum(item => item.TotalVisitCount
                                            );



            }

            return adminDashboardModel;
        }
        public MerchantStatementModel MerchantStatement(MerchantStatementRequest merchantUserModel)
        {
            MerchantStatementModel merchantStatementModel = new MerchantStatementModel();

            try
            {
              
                DateTime fromDates = DateTime.ParseExact(merchantUserModel.FromDate, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
                DateTime toDates = DateTime.ParseExact(merchantUserModel.ToDate, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);

               
                if(merchantUserModel.CustPhoneNo == "")
                {
                    
                    var rewardSum = (from rewardpointtransaction_ in context.Rewardpointtransactions

                                     where rewardpointtransaction_.TransactionDate < fromDates && rewardpointtransaction_.MerchantId == merchantUserModel.MerchantId
                                     && (rewardpointtransaction_.TransactionType == 1 || rewardpointtransaction_.TransactionType == 2 || rewardpointtransaction_.TransactionType == 3)
                                     select new
                                     {
                                         RewardPoint = rewardpointtransaction_.Points,
                                         IsPositive = rewardpointtransaction_.TransactionType != 3
                                     }).ToList();

                    merchantStatementModel.OpeningBalance = Convert.ToInt32(rewardSum.Sum(item => item.IsPositive ? item.RewardPoint : -item.RewardPoint));


                    //var MerchantTransactions = (from rt in context.Rewardpointtransactions
                    //                            join c in context.Customers on rt.CustomerId equals c.CustomerId
                    //                            join m in context.Merchants on rt.MerchantId equals m.MerchantId

                    //                            where rt.MerchantId == merchantUserModel.MerchantId &&
                    //                            rt.TransactionDate >= fromDates && rt.TransactionDate <= toDates

                    //                            select new MerchantTransaction
                    //                            {
                    //                                CustomerId = c.CustomerId,
                    //                                BalancePoints = rt.Points,
                    //                                Points = rt.Points * (rt.TransactionType == 3 ? -1 : 1),
                    //                                TransactionDate = rt.TransactionDate,
                    //                                Transaction = rt.TransactionType == 3
                    //                                   ?

                    //                            $"Collected - {(string.IsNullOrEmpty(c.Name) ? "User - " + CommonUtility.maskString(Encoding.UTF8.GetString(CommonUtility.Decrypt(c.PhoneNumber, configuration.SymmetricKey)), "PhoneNUmber") : (c.Name + (string.IsNullOrEmpty(c.Lastname) ? "" : " " + c.Lastname)))}"

                    //                             : $"Issued - {(string.IsNullOrEmpty(c.Name) ? "User - " + CommonUtility.maskString(Encoding.UTF8.GetString(CommonUtility.Decrypt(c.PhoneNumber, configuration.SymmetricKey)), "PhoneNUmber") : (c.Name + (string.IsNullOrEmpty(c.Lastname) ? "" : " " + c.Lastname)))}",
                    //                                BalPoint = rt.Points
                    //                            })
                    //                    .ToList();


                    var MerchantTransactions = (from rt in context.Rewardpointtransactions
                                                join c in context.Customers on rt.CustomerId equals c.CustomerId
                                                join m in context.Merchants on rt.MerchantId equals m.MerchantId
                                                join R in context.Redeemtions on c.CustomerId equals R.RedeembyCustomerId into redemptionGroup
                                                where rt.MerchantId == merchantUserModel.MerchantId &&
                                                      rt.TransactionDate >= fromDates && rt.TransactionDate <= toDates
                                                select new MerchantTransaction
                                                {
                                                    CustomerId = c.CustomerId,
                                                    BalancePoints = rt.Points,
                                                    Points = rt.Points * (rt.TransactionType == 3 ? -1 : 1),
                                                    TransactionDate = rt.TransactionDate,
                                                    Transaction = rt.TransactionType == 3
                                                        ? $"Collected - {(string.IsNullOrEmpty(c.Name) ? "User - " + CommonUtility.maskString(Encoding.UTF8.GetString(CommonUtility.Decrypt(c.PhoneNumber, configuration.SymmetricKey)), "PhoneNUmber") : (c.Name + (string.IsNullOrEmpty(c.Lastname) ? "" : " " + c.Lastname)))}"
                                                        : $"Issued - {(string.IsNullOrEmpty(c.Name) ? "User - " + CommonUtility.maskString(Encoding.UTF8.GetString(CommonUtility.Decrypt(c.PhoneNumber, configuration.SymmetricKey)), "PhoneNUmber") : (c.Name + (string.IsNullOrEmpty(c.Lastname) ? "" : " " + c.Lastname)))}",
                                                    BalPoint = rt.Points,
                                                    CouponsCount = redemptionGroup.Count() // Count of redemptions for each customer
                                                })
                            .ToList();

                    if (MerchantTransactions != null)
                    {
                        for (int i = 0; i < MerchantTransactions.Count - 1; i++)
                        {
                            MerchantTransactions[i + 1].BalPoint = MerchantTransactions[i].BalPoint + MerchantTransactions[i + 1].Points;
                        }

                        merchantStatementModel.MerchantTransactions = MerchantTransactions;
                    }
                }

                else
                {

                    var encryptedPhoneNumber = Convert.ToBase64String(CommonUtility.Encrypt(merchantUserModel.CustPhoneNo, configuration.SymmetricKey));


                    var rewardSum = (from rewardpointtransaction_ in context.Rewardpointtransactions
                                     join c in context.Customers on rewardpointtransaction_.CustomerId equals c.CustomerId

                                     where 
                                     c.PhoneNumber == encryptedPhoneNumber &&
                                     rewardpointtransaction_.TransactionDate < fromDates && rewardpointtransaction_.MerchantId == merchantUserModel.MerchantId
                                     && (rewardpointtransaction_.TransactionType == 1 || rewardpointtransaction_.TransactionType == 2 || rewardpointtransaction_.TransactionType == 3)
                                     select new
                                     {
                                         RewardPoint = rewardpointtransaction_.Points,
                                         IsPositive = rewardpointtransaction_.TransactionType != 3
                                     }).ToList();


                    merchantStatementModel.OpeningBalance = Convert.ToInt32(rewardSum.Sum(item => item.IsPositive ? item.RewardPoint : -item.RewardPoint));



                    var MerchantTransactions = (from rt in context.Rewardpointtransactions
                                                join c in context.Customers on rt.CustomerId equals c.CustomerId
                                                join m in context.Merchants on rt.MerchantId equals m.MerchantId
                                                join R in context.Redeemtions on c.CustomerId equals R.RedeembyCustomerId into redemptionGroup
                                                where rt.MerchantId == merchantUserModel.MerchantId && c.PhoneNumber == encryptedPhoneNumber &&
                                                rt.TransactionDate >= fromDates && rt.TransactionDate <= toDates

                                                select new MerchantTransaction
                                                {
                                                    CustomerId = c.CustomerId,
                                                    BalancePoints = rt.Points,
                                                    Points = rt.Points * (rt.TransactionType == 3 ? -1 : 1),
                                                    TransactionDate = rt.TransactionDate,
                                                    Transaction = rt.TransactionType == 3
                                                           ?

                                                           // $"Collected - {(string.IsNullOrEmpty(c.Name) ? "User - " + CommonUtility.maskString(Encoding.UTF8.GetString(CommonUtility.Decrypt(c.PhoneNumber, configuration.SymmetricKey)), "PhoneNumber") : (c.Name + (string.IsNullOrEmpty(c.Lastname) ? "" : " " + c.Lastname)))}"


                                                           //: $"Issued - {(string.IsNullOrEmpty(c.Name) ? "User - " + CommonUtility.maskString(Encoding.UTF8.GetString(CommonUtility.Decrypt(c.PhoneNumber, configuration.SymmetricKey)), "PhoneNUmber") : (c.Name + (string.IsNullOrEmpty(c.Lastname) ? "" : " " + c.Lastname)))}",

                                                           $"Collected - {(string.IsNullOrEmpty(c.Name) ? "User - " + CommonUtility.maskString(Encoding.UTF8.GetString(CommonUtility.Decrypt(c.PhoneNumber, configuration.SymmetricKey)), "PhoneNUmber") : (c.Name + (string.IsNullOrEmpty(c.Lastname) ? "" : " " + c.Lastname)))}"

                                                           : $"Issued - {(string.IsNullOrEmpty(c.Name) ? "User - " + CommonUtility.maskString(Encoding.UTF8.GetString(CommonUtility.Decrypt(c.PhoneNumber, configuration.SymmetricKey)), "PhoneNUmber") : (c.Name + (string.IsNullOrEmpty(c.Lastname) ? "" : " " + c.Lastname)))}",


                                                    BalPoint = rt.Points,
                                                    CouponsCount = redemptionGroup.Count(),
                                                })
                                        .ToList();


                    if (MerchantTransactions != null)
                    {
                        for (int i = 0; i < MerchantTransactions.Count - 1; i++)
                        {
                            MerchantTransactions[i + 1].BalPoint = MerchantTransactions[i].BalPoint + MerchantTransactions[i + 1].Points;
                        }

                        merchantStatementModel.MerchantTransactions = MerchantTransactions;
                    }
                }
                                
                
                

                // Set the values in your merchantStatementModel
                merchantStatementModel.MerchantName = (from merchant in context.Merchants
                                                       where merchant.MerchantId == merchantUserModel.MerchantId
                                                       select merchant.OrganizationName).FirstOrDefault();

                //}


            }
            catch (Exception ex)
            {

                throw;
            }
            return merchantStatementModel;
        }
        public CustomerStatementModel CustomerStatement(MerchantStatementRequest customerUserModel)
        {
            CustomerStatementModel customerStatementModel = new CustomerStatementModel();

            var encryptedPhoneNumber = Convert.ToBase64String(CommonUtility.Encrypt(customerUserModel.CustPhoneNo, configuration.SymmetricKey));
          
            DateTime fromDates = DateTime.ParseExact(customerUserModel.FromDate, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
            DateTime toDates = DateTime.ParseExact(customerUserModel.ToDate, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);


            customerStatementModel.CustomerName = context.Customers
                .Where(customer => customer.PhoneNumber == encryptedPhoneNumber)               
                .Select(customer => string.IsNullOrEmpty(customer.Name) ? "User - " + CommonUtility.maskString(Encoding.UTF8.GetString(CommonUtility.Decrypt(customer.PhoneNumber, configuration.SymmetricKey)), "PhoneNUmber") :  (customer.Name + (string.IsNullOrEmpty(customer.Lastname) ? "" : " " + customer.Lastname)))
                .FirstOrDefault();

            if (customerUserModel.MerchantId == 0)
            {

                var rewardSum = (from rewardpointtransaction_ in context.Rewardpointtransactions
                                 join Customer_ in context.Customers on rewardpointtransaction_.CustomerId equals Customer_.CustomerId
                                 where  rewardpointtransaction_.TransactionDate < fromDates && Customer_.PhoneNumber == encryptedPhoneNumber  && (rewardpointtransaction_.TransactionType == 1 || rewardpointtransaction_.TransactionType == 2 || rewardpointtransaction_.TransactionType == 3)
                                 select new
                                 {
                                     RewardPoint = rewardpointtransaction_.Points,
                                     IsPositive = rewardpointtransaction_.TransactionType != 3
                                 }).ToList();

                customerStatementModel.OpeningBalance = Convert.ToInt32(rewardSum.Sum(item => item.IsPositive ? item.RewardPoint : -item.RewardPoint));

         

                var customerTransactions = (from rt in context.Rewardpointtransactions
                                            join c in context.Customers on rt.CustomerId equals c.CustomerId
                                            join m in context.Merchants on rt.MerchantId equals m.MerchantId
                                            join R in context.Redeemtions on c.CustomerId equals R.RedeembyCustomerId into redemptionGroup
                                            where  c.PhoneNumber == encryptedPhoneNumber &&
                                            rt.TransactionDate >= fromDates && rt.TransactionDate <= toDates

                                            select new CustomerTransaction
                                            {
                                                CustomerId = c.CustomerId,
                                                BalancePoints = rt.Points,
                                                Points = rt.Points * (rt.TransactionType == 3 ? -1 : 1),
                                                TransactionDate = rt.TransactionDate,
                                                Transaction = (rt.TransactionType == 1 || rt.TransactionType == 2)
                                                  ? $"Earned - " + m.OrganizationName
                                                   : $"Redeemed - " + m.OrganizationName,
                                                BalPoint = rt.Points,
                                                CouponsCount = redemptionGroup.Count(),
                                            })
                                       .ToList();



                //var customerTransactions = (from rt in context.Rewardpointtransactions
                //                            join c in context.Customers on rt.CustomerId equals c.CustomerId
                //                            join m in context.Merchants on rt.MerchantId equals m.MerchantId
                //                         //   join R in context.Redeemtions on c.CustomerId equals R.RedeembyCustomerId into redemptionGroup
                //                            where c.PhoneNumber == encryptedPhoneNumber && rt.MerchantId == customerUserModel.MerchantId
                //                                && (rt.TransactionDate >= fromDates)
                //                                && (rt.TransactionDate <= toDates)
                //                            group new { rt, c, m } by new
                //                            {
                //                                c.CustomerId,
                //                                rt.TransactionDate,
                //                                m.OrganizationName
                //                            } into grouped
                //                            select new CustomerTransaction
                //                            {
                //                                CustomerId = grouped.Key.CustomerId,
                //                                TransactionDate = grouped.Key.TransactionDate,
                //                                BalPoint = grouped.Sum(g => g.rt.TransactionType == 1 || g.rt.TransactionType == 2 ? g.rt.Points : -g.rt.Points),
                //                                Points = grouped.Sum(g => g.rt.TransactionType == 1 || g.rt.TransactionType == 2 ? g.rt.Points : -g.rt.Points),
                //                                Transaction = (grouped.Max(g => g.rt.TransactionType) == 1 || grouped.Max(g => g.rt.TransactionType) == 2)
                //                                    ? $"Earned - " + grouped.Key.OrganizationName
                //                                    : $"Redeemed - " + grouped.Key.OrganizationName,
                //                          //      CouponsCount = grouped.SelectMany(g => g.redemptionGroup).Count() // Count of redemptions for each customer
                //                            }).ToList();


                var result = customerTransactions.ToList();




                if (customerTransactions != null && customerTransactions.Count > 0)
                {
                    customerTransactions[0].BalPoint = customerTransactions[0].BalPoint + customerStatementModel.OpeningBalance;

                    for (int i = 0; i < customerTransactions.Count - 1; i++)
                    {
                        // Calculate the new balance by adding the Points to the previous balance
                        customerTransactions[i + 1].BalPoint = customerTransactions[i].BalPoint + customerTransactions[i + 1].Points;
                    }

                    customerStatementModel.CustomerTransactions = customerTransactions;
                }

            }
            else
            {

                //  customerStatementModel.OpeningBalance = context.Rewardpointtransactions
                //.Join(
                //    context.Customers,
                //    r => r.CustomerId,
                //    m => m.CustomerId,
                //    (r, m) => new { Transaction = r, Customer = m }
                //)
                //.Where(joined => joined.Customer.PhoneNumber == encryptedPhoneNumber && joined.Transaction.MerchantId == customerUserModel.MerchantId &&
                //                 joined.Transaction.TransactionDate < fromDates &&
                //                (joined.Transaction.TransactionType == 1 || joined.Transaction.TransactionType == 2))
                //.Sum(joined =>
                //    joined.Transaction.TransactionType == 3 ? -joined.Transaction.Points : joined.Transaction.Points
                //);


                var rewardSum = (from rewardpointtransaction_ in context.Rewardpointtransactions
                                 join Customer_ in context.Customers on rewardpointtransaction_.CustomerId equals Customer_.CustomerId
                                 where rewardpointtransaction_.MerchantId == customerUserModel.MerchantId && rewardpointtransaction_.TransactionDate < fromDates && Customer_.PhoneNumber == encryptedPhoneNumber && (rewardpointtransaction_.TransactionType == 1 || rewardpointtransaction_.TransactionType == 2 || rewardpointtransaction_.TransactionType == 3)
                                 select new
                                 {
                                     RewardPoint = rewardpointtransaction_.Points,
                                     IsPositive = rewardpointtransaction_.TransactionType != 3
                                 }).ToList();

                customerStatementModel.OpeningBalance = Convert.ToInt32(rewardSum.Sum(item => item.IsPositive ? item.RewardPoint : -item.RewardPoint));


                var customerTransactions = (from rt in context.Rewardpointtransactions
                                            join c in context.Customers on rt.CustomerId equals c.CustomerId
                                            join m in context.Merchants on rt.MerchantId equals m.MerchantId
                                            join R in context.Redeemtions on c.CustomerId equals R.RedeembyCustomerId into redemptionGroup
                                            where c.PhoneNumber == encryptedPhoneNumber &&   rt.MerchantId == customerUserModel.MerchantId &&
                                            rt.TransactionDate >= fromDates && rt.TransactionDate <= toDates

                                            select new CustomerTransaction
                                            {
                                                CustomerId = c.CustomerId,
                                                BalancePoints = rt.Points,
                                                Points = rt.Points * (rt.TransactionType == 3 ? -1 : 1),
                                                TransactionDate = rt.TransactionDate,
                                                Transaction = (rt.TransactionType == 1 || rt.TransactionType == 2)
                                                  ? $"Earned - " + m.OrganizationName
                                                   : $"Redeemed - " + m.OrganizationName,
                                                BalPoint = rt.Points,
                                                CouponsCount = redemptionGroup.Count(),
                                            })
                                .ToList();


                //var customerTransactions = (from rt in context.Rewardpointtransactions
                //                            join c in context.Customers on rt.CustomerId equals c.CustomerId
                //                            join m in context.Merchants on rt.MerchantId equals m.MerchantId
                //                            join R in context.Redeemtions on c.CustomerId equals R.RedeembyCustomerId into redemptionGroup
                //                            where c.PhoneNumber == encryptedPhoneNumber && rt.MerchantId == customerUserModel.MerchantId
                //                             && (rt.TransactionDate >= fromDates)
                //                               && (rt.TransactionDate <= toDates)
                //                            group new { rt, c, m , redemptionGroup } by new
                //                            {
                //                                c.CustomerId,
                //                                rt.TransactionDate,
                //                                m.OrganizationName,                                                

                //                            } into grouped
                //                            select new CustomerTransaction
                //                            {

                //                                CustomerId = grouped.Key.CustomerId,
                //                                TransactionDate = grouped.Key.TransactionDate,
                //                                BalPoint = grouped.Sum(g => g.rt.TransactionType == 1 || g.rt.TransactionType == 2 ? g.rt.Points : -g.rt.Points),

                //                                Points = grouped.Sum(g => g.rt.TransactionType == 1 || g.rt.TransactionType == 2 ? g.rt.Points : -g.rt.Points),
                //                                Transaction = (grouped.Max(g => g.rt.TransactionType) == 1 || grouped.Max(g => g.rt.TransactionType) == 2)
                //                                                 ? $"Earned - " + grouped.Key.OrganizationName
                //                                                 : $"Redeemed - " + grouped.Key.OrganizationName,
                //                                CouponsCount = grouped.SelectMany(g => g.redemptionGroup).Count()  // Count of redemptions for each customer

                //                            }).ToList();

                var result = customerTransactions.ToList();



                if (customerTransactions != null && customerTransactions.Count > 0)
                {
                    customerTransactions[0].BalPoint = customerTransactions[0].BalPoint + customerStatementModel.OpeningBalance;

                    for (int i = 0; i < customerTransactions.Count - 1; i++)
                    {
                        // Calculate the new balance by adding the Points to the previous balance
                        customerTransactions[i + 1].BalPoint = customerTransactions[i].BalPoint + customerTransactions[i + 1].Points;
                    }

                    customerStatementModel.CustomerTransactions = customerTransactions;
                }

            }










            return customerStatementModel;
        }
        public List<MerchantDDLModel> GetMerchantDDL(ref ErrorResponseModel errorResponseModel)
        {  

                errorResponseModel = new ErrorResponseModel();

                var MerchantList = (from Merchant in context.Merchants
                              //   where Merchant.RecStatus == "A"
                                    select new MerchantDDLModel
                                    {
                                        MerchantId = Merchant.MerchantId,
                                        MerchantCode = Merchant.MerchantCode,
                                        OrganizationName = Merchant.OrganizationName
                                    }
                                   ).Distinct().ToList();

                if (MerchantList == null)
                {
                    errorResponseModel.StatusCode = HttpStatusCode.NotFound;
                    errorResponseModel.Message = "No Data found";
                }
               
                return MerchantList;
            

        }

        public AdminGraphModel AdminGrapgData(int merchantId)
        {
            AdminGraphModel adminGraphModel = new AdminGraphModel();


            if(merchantId == 0)
            {

                //adminGraphModel.AdminUserGraphs = context.Adminusergraphs
                //      .GroupBy(admingrf => new { admingrf.Month, admingrf.MonthText })
                //      .Select(group => new AdminUserGraphModel
                //      {
                //          Month = group.Key.Month,
                //          MonthText = group.Key.MonthText,
                //          RefferalUser = group.Sum(admingrf => Convert.ToInt32(admingrf.ReferralCount)),
                //          WalkInUser = group.Sum(admingrf => Convert.ToInt32(admingrf.WalkInCount)),
                //          // ... map other properties
                //      })
                //      .ToList();

                //adminGraphModel.AdminCouponsGraphs = context.Admincoupongraphs
                //    .GroupBy(admingrf => new { admingrf.Month, admingrf.MonthText })
                //    .Select(group => new AdminCouponsGraphModel
                //    {
                //        Month = group.Key.Month,
                //        MonthText = group.Key.MonthText,
                //        Couponsredeem = group.Sum(admingrf => Convert.ToInt32(admingrf.RedeemCount)),
                //        Coupons = group.Sum(admingrf => Convert.ToInt32(admingrf.TotalCouponCount)),
                //        // ... map other properties
                //    })
                //    .ToList();

                //adminGraphModel.AdminPointGraphs = context.Adminpointgraphs
                //    .GroupBy(admingrf => new { admingrf.Month, admingrf.MonthText })
                //    .Select(group => new AdminPointGraphModel
                //    {
                //        Month = group.Key.Month,
                //        MonthText = group.Key.MonthText,
                //        PointEarned = group.Sum(admingrf => Convert.ToInt32(admingrf.Earned)),
                //        Pointredeem = group.Sum(admingrf => Convert.ToInt32(admingrf.Redeem)),
                //        // ... map other properties
                //    })
                //    .ToList();




                var firstpointsmonth = context.Adminpointgraphs.Select(d => d.Month).FirstOrDefault();

                var next12Monthspoint = Enumerable.Range(0, 12)
                                 .Select(offset => (firstpointsmonth + offset - 1) % 12 + 1) // Adjust for months 1-12
                                 .ToList();

                var allpointMonths = next12Monthspoint
                    .Select(month => new { Month = month, MonthText = CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(Convert.ToInt32(month)) })
                    .ToArray();


                var pointresult = from month in allpointMonths
                                  join d in context.Adminpointgraphs
                                  on month.Month equals d.Month into gj
                                  from subdata in gj.DefaultIfEmpty()
                                  group new { subdata, month } by new { month.Month } into g
                                  select new AdminPointGraphModel
                                  {
                                      Month = g.Key.Month,
                                      MonthText = g.First().month.MonthText,
                                      PointEarned = g.Any() ? Convert.ToInt32(g.Sum(s => s.subdata?.Earned ?? 0)) : 0,
                                      Pointredeem = g.Any() ? Convert.ToInt32(g.Sum(s => s.subdata?.Redeem ?? 0)) : 0,
                                      // ... map other properties
                                  };

                adminGraphModel.AdminPointGraphs = pointresult.ToArray();

                var Earnpoint = adminGraphModel.AdminPointGraphs.Select(graph => graph.PointEarned).ToArray();
                var redeempoint = adminGraphModel.AdminPointGraphs.Select(graph => graph.Pointredeem).ToArray();
                var pointmonths = adminGraphModel.AdminPointGraphs.Select(graph => graph.MonthText).ToArray();

                var Earnstring = String.Join(", ", Earnpoint);
                var redeemstring = String.Join(", ", redeempoint);
                var pointmonthstring = String.Join(", ", pointmonths);

                string[] pointMonths = pointmonthstring.Split(',');
                string[] earncountdata = Earnstring.Split(',');
                string[] redeemcountdata = redeemstring.Split(',');


                for (int i = 0; i < pointMonths.Length; i++)
                {
                    pointMonths[i] = pointMonths[i].Trim(' ', '\'');
                    earncountdata[i] = earncountdata[i].Trim(' ', '\'');
                    redeemcountdata[i] = redeemcountdata[i].Trim(' ', '\'');
                }

                // Add single quotes to the first and last elements
                if (pointMonths.Length > 0)
                {
                    pointMonths[0] = $"'{pointMonths[0]}";
                    pointMonths[pointMonths.Length - 1] = $"{pointMonths[pointMonths.Length - 1]}'";
                }

                string concatepointreferraldata = string.Join(", ", earncountdata);
                string concatepointdata = string.Join(", ", redeemcountdata);
                string concatepointMonths = string.Join(", ", pointMonths);

                adminGraphModel.pointearndata = concatepointreferraldata;
                adminGraphModel.pointreddemdata = concatepointdata;
                adminGraphModel.pointmonthdata = concatepointMonths;



                var firstusermonth = context.Adminusergraphs.Select(d => d.Month).FirstOrDefault();

                var next12MonthsUser = Enumerable.Range(0, 12)
                                 .Select(offset => (firstusermonth + offset - 1) % 12 + 1) // Adjust for months 1-12
                                 .ToList();

                var allUserMonths = next12MonthsUser
                    .Select(month => new { Month = month, MonthText = CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(Convert.ToInt32(month)) })
                    .ToArray();

              
                var userresult = from month in allUserMonths
                             join d in context.Adminusergraphs
                             on month.Month equals d.Month into gj
                             from subdata in gj.DefaultIfEmpty()
                             group new { subdata, month } by new { month.Month } into g
                             select new AdminUserGraphModel
                             {
                                 Month = g.Key.Month,
                                 MonthText = g.First().month.MonthText,
                                 RefferalUser = g.Any() ? Convert.ToInt32(g.Sum(s => s.subdata?.ReferralCount ?? 0)) : 0,
                                 WalkInUser = g.Any() ? Convert.ToInt32(g.Sum(s => s.subdata?.WalkInCount ?? 0)) : 0,
                                 // ... map other properties
                             };


                adminGraphModel.AdminUserGraphs = userresult.ToArray();

                var reffrealuser = adminGraphModel.AdminUserGraphs.Select(graph => graph.RefferalUser).ToArray();
                var walkinuser= adminGraphModel.AdminUserGraphs.Select(graph => graph.WalkInUser).ToArray();
                var userMothnst = adminGraphModel.AdminUserGraphs.Select(graph => graph.MonthText).ToArray();

                var refferalstring = String.Join(", ", reffrealuser);
                var walkinstring = String.Join(", ", walkinuser);
                var usermonths = String.Join(", ", userMothnst);

                string[] userMonths = usermonths.Split(',');
                string[] walkincountdata = walkinstring.Split(',');
                string[] refferalcountdata = refferalstring.Split(',');


                for (int i = 0; i < userMonths.Length; i++)
                {
                    userMonths[i] = userMonths[i].Trim(' ', '\'');
                    walkincountdata[i] = walkincountdata[i].Trim(' ', '\'');
                    refferalcountdata[i] = refferalcountdata[i].Trim(' ', '\'');
                }

                // Add single quotes to the first and last elements
                if (userMonths.Length > 0)
                {
                    userMonths[0] = $"'{userMonths[0]}";
                    userMonths[userMonths.Length - 1] = $"{userMonths[userMonths.Length - 1]}'";
                }

                string concateuserreferraldata = string.Join(", ", refferalcountdata);
                string concatewalkinuserdata = string.Join(", ", walkincountdata);
                string concateuserMonths = string.Join(", ", userMonths);

                adminGraphModel.userrefferaldata = concateuserreferraldata;
                adminGraphModel.userwainindata = concatewalkinuserdata;
                adminGraphModel.usermonthdata = concateuserMonths;




                //----------------------------------- admin Coupon Graph -------------------------//
                var first = context.Admincoupongraphs.Select(d => d.Month).FirstOrDefault();

                var next12Months = Enumerable.Range(0, 12)
                                 .Select(offset => (first + offset -1) % 12 + 1) // Adjust for months 1-12
                                 .ToList();

                var allMonths = next12Months
                    .Select(month => new { Month = month, MonthText = CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(Convert.ToInt32(month)) })
                    .ToArray();

                var result = from month in allMonths
                             join d in context.Admincoupongraphs
                             on month.Month equals d.Month into gj
                             from subdata in gj.DefaultIfEmpty()
                             group new { subdata, month } by new { month.Month } into g
                             select new AdminCouponsGraphModel
                             {
                                 Month = g.Key.Month,
                                 MonthText = g.First().month.MonthText,
                                 Couponsredeem = g.Any() ? Convert.ToInt32(g.Sum(s => s.subdata?.RedeemCount ?? 0)) : 0,
                                 Coupons = g.Any() ? Convert.ToInt32(g.Sum(s => s.subdata?.TotalCouponCount ?? 0)) : 0,
                                 // ... map other properties
                             };


                adminGraphModel.AdminCouponsGraphs = result.ToArray();              

                var coupons = adminGraphModel.AdminCouponsGraphs.Select(graph => graph.Coupons).ToArray();
                var couponsredeem = adminGraphModel.AdminCouponsGraphs.Select(graph => graph.Couponsredeem).ToArray();
                var couponsMothnst = adminGraphModel.AdminCouponsGraphs.Select(graph => graph.MonthText).ToArray();

                var couponsstring = String.Join(", " ,coupons);
                var couponsredeemstring = String.Join(", ", couponsredeem);
                var months = String.Join(", ", couponsMothnst);
               
                string[] couponsMonths = months.Split(',');
                string[] couponcountdata = couponsstring.Split(',');
                string[] couponredeemcountdata = couponsredeemstring.Split(',');


                for (int i = 0; i < couponsMonths.Length; i++)
                {
                    couponsMonths[i] = couponsMonths[i].Trim(' ', '\'');
                    couponcountdata[i] = couponcountdata[i].Trim(' ','\'');
                    couponredeemcountdata[i] = couponredeemcountdata[i].Trim(' ', '\'');
                }

                // Add single quotes to the first and last elements
                if (couponsMonths.Length > 0)
                {
                    couponsMonths[0] = $"'{couponsMonths[0]}";
                   couponsMonths[couponsMonths.Length - 1] = $"{couponsMonths[couponsMonths.Length - 1]}'";
                }
                
                string concatecoupondata = string.Join(", ", couponcountdata);
                string concateredeemcoupondata = string.Join(", ", couponredeemcountdata);
                string concatenatedMonths = string.Join(", ", couponsMonths);

                adminGraphModel.coupondata = concatecoupondata;
                adminGraphModel.monthdata = concatenatedMonths;
                adminGraphModel.couponredeemdata = concateredeemcoupondata;



                //----------------------------------- end Coupon Graph -------------------------//



                var firstfrequentmonth = context.Adminfrequentusergraphs.Select(d => d.Mm).FirstOrDefault();

                var next12Monthsfrequent = Enumerable.Range(0, 12)
                                 .Select(offset => (firstfrequentmonth + offset - 1) % 12 + 1) // Adjust for months 1-12
                                 .ToList();

                var allfrequentMonths = next12Monthsfrequent
                    .Select(month => new { Month = month, MonthText = CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(Convert.ToInt32(month)) })
                    .ToArray();


                var frequentresult = from month in allfrequentMonths
                                     join d in context.Adminfrequentusergraphs
                                     on month.Month equals d.Mm into gj
                                     from subdata in gj.DefaultIfEmpty()
                                     group new { subdata, month } by new { month.Month } into g
                                     select new AdminFrequentGraphModel
                                     {
                                         Month = g.Key.Month,
                                         MonthText = g.First().month.MonthText,
                                         TotalvisitCount = g.Any() ? Convert.ToInt32(g.Sum(s => s.subdata?.TotalVisitCount ?? 0)) : 0,
                                         TotalFrequentCount = g.Any() ? Convert.ToInt32(g.Sum(s => s.subdata?.TotalFrequentUserCount ?? 0)) : 0,
                                         // ... map other properties
                                     };

                adminGraphModel.AdminFrequentGraphs = frequentresult.ToArray();

                var frequentvist = adminGraphModel.AdminFrequentGraphs.Select(graph => graph.TotalvisitCount).ToArray();
                var frequentuser = adminGraphModel.AdminFrequentGraphs.Select(graph => graph.TotalFrequentCount).ToArray();
                var frequentmonth = adminGraphModel.AdminFrequentGraphs.Select(graph => graph.MonthText).ToArray();

                var visittring = String.Join(", ", frequentvist);
                var frequentuserstring = String.Join(", ", frequentuser);
                var frequentmonthstring = String.Join(", ", frequentmonth);

                string[] frequentmonths = frequentmonthstring.Split(',');
                string[] frequentvists = visittring.Split(',');
                string[] frequentusers = frequentuserstring.Split(',');


                for (int i = 0; i < frequentmonths.Length; i++)
                {
                    frequentmonths[i] = frequentmonths[i].Trim(' ', '\'');
                    frequentvists[i] = frequentvists[i].Trim(' ', '\'');
                    frequentusers[i] = frequentusers[i].Trim(' ', '\'');
                }

                // Add single quotes to the first and last elements
                if (frequentmonths.Length > 0)
                {
                    frequentmonths[0] = $"'{frequentmonths[0]}";
                    frequentmonths[frequentmonths.Length - 1] = $"{frequentmonths[frequentmonths.Length - 1]}'";
                }

                string concatefrequenterndata = string.Join(", ", frequentvists);
                string concatefrequentuserdata = string.Join(", ", frequentusers);
                string concatefrequentMonths = string.Join(", ", frequentmonths);

                adminGraphModel.frequentvistdata = concatefrequenterndata;
                adminGraphModel.frequentuserdata = concatefrequentuserdata;
                adminGraphModel.frequentmonthdata = concatefrequentMonths;


            }

            else
            {

             

                var firstpointsmonth = context.Adminpointgraphs.Select(d => d.Month).FirstOrDefault();

                var next12Monthspoint = Enumerable.Range(0, 12)
                                 .Select(offset => (firstpointsmonth + offset - 1) % 12 + 1) // Adjust for months 1-12
                                 .ToList();

                var allpointMonths = next12Monthspoint
                    .Select(month => new { Month = month, MonthText = CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(Convert.ToInt32(month)) })
                    .ToArray();


                var pointresult = from month in allpointMonths
                                  join d in context.Adminpointgraphs.Where(d => d.MerchantId == merchantId)
                                  on month.Month equals d.Month into gj
                                  from subdata in gj.DefaultIfEmpty()
                                  group new { subdata, month } by new { month.Month } into g
                                  select new AdminPointGraphModel
                                  {
                                      Month = g.Key.Month,
                                      MonthText = g.First().month.MonthText,
                                      PointEarned = g.Any() ? Convert.ToInt32(g.Sum(s => s.subdata?.Earned ?? 0)) : 0,
                                      Pointredeem = g.Any() ? Convert.ToInt32(g.Sum(s => s.subdata?.Redeem ?? 0)) : 0,
                                      // ... map other properties
                                  };

                adminGraphModel.AdminPointGraphs = pointresult.ToArray();

                var Earnpoint = adminGraphModel.AdminPointGraphs.Select(graph => graph.PointEarned).ToArray();
                var redeempoint = adminGraphModel.AdminPointGraphs.Select(graph => graph.Pointredeem).ToArray();
                var pointmonths = adminGraphModel.AdminPointGraphs.Select(graph => graph.MonthText).ToArray();

                var Earnstring = String.Join(", ", Earnpoint);
                var redeemstring = String.Join(", ", redeempoint);
                var pointmonthstring = String.Join(", ", pointmonths);

                string[] pointMonths = pointmonthstring.Split(',');
                string[] earncountdata = Earnstring.Split(',');
                string[] redeemcountdata = redeemstring.Split(',');


                for (int i = 0; i < pointMonths.Length; i++)
                {
                    pointMonths[i] = pointMonths[i].Trim(' ', '\'');
                    earncountdata[i] = earncountdata[i].Trim(' ', '\'');
                    redeemcountdata[i] = redeemcountdata[i].Trim(' ', '\'');
                }

                // Add single quotes to the first and last elements
                if (pointMonths.Length > 0)
                {
                    pointMonths[0] = $"'{pointMonths[0]}";
                    pointMonths[pointMonths.Length - 1] = $"{pointMonths[pointMonths.Length - 1]}'";
                }

                string concatepointreferraldata = string.Join(", ", earncountdata);
                string concatepointdata = string.Join(", ", redeemcountdata);
                string concatepointMonths = string.Join(", ", pointMonths);

                adminGraphModel.pointearndata = concatepointreferraldata;
                adminGraphModel.pointreddemdata = concatepointdata;
                adminGraphModel.pointmonthdata = concatepointMonths;



                var firstusermonth = context.Adminusergraphs.Select(d => d.Month).FirstOrDefault();

                var next12MonthsUser = Enumerable.Range(0, 12)
                                 .Select(offset => (firstusermonth + offset - 1) % 12 + 1) // Adjust for months 1-12
                                 .ToList();

                var allUserMonths = next12MonthsUser
                    .Select(month => new { Month = month, MonthText = CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(Convert.ToInt32(month)) })
                    .ToArray();


                var userresult = from month in allUserMonths
                                 join d in context.Adminusergraphs.Where(d => d.MerchantId == merchantId)
                                 on month.Month equals d.Month into gj
                                 from subdata in gj.DefaultIfEmpty()
                                 group new { subdata, month } by new { month.Month } into g
                                 select new AdminUserGraphModel
                                 {
                                     Month = g.Key.Month,
                                     MonthText = g.First().month.MonthText,
                                     RefferalUser = g.Any() ? Convert.ToInt32(g.Sum(s => s.subdata?.ReferralCount ?? 0)) : 0,
                                     WalkInUser = g.Any() ? Convert.ToInt32(g.Sum(s => s.subdata?.WalkInCount ?? 0)) : 0,
                                     // ... map other properties
                                 };


                adminGraphModel.AdminUserGraphs = userresult.ToArray();

                var reffrealuser = adminGraphModel.AdminUserGraphs.Select(graph => graph.RefferalUser).ToArray();
                var walkinuser = adminGraphModel.AdminUserGraphs.Select(graph => graph.WalkInUser).ToArray();
                var userMothnst = adminGraphModel.AdminUserGraphs.Select(graph => graph.MonthText).ToArray();

                var refferalstring = String.Join(", ", reffrealuser);
                var walkinstring = String.Join(", ", walkinuser);
                var usermonths = String.Join(", ", userMothnst);

                string[] userMonths = usermonths.Split(',');
                string[] walkincountdata = walkinstring.Split(',');
                string[] refferalcountdata = refferalstring.Split(',');


                for (int i = 0; i < userMonths.Length; i++)
                {
                    userMonths[i] = userMonths[i].Trim(' ', '\'');
                    walkincountdata[i] = walkincountdata[i].Trim(' ', '\'');
                    refferalcountdata[i] = refferalcountdata[i].Trim(' ', '\'');
                }

                // Add single quotes to the first and last elements
                if (userMonths.Length > 0)
                {
                    userMonths[0] = $"'{userMonths[0]}";
                    userMonths[userMonths.Length - 1] = $"{userMonths[userMonths.Length - 1]}'";
                }

                string concateuserreferraldata = string.Join(", ", refferalcountdata);
                string concatewalkinuserdata = string.Join(", ", walkincountdata);
                string concateuserMonths = string.Join(", ", userMonths);

                adminGraphModel.userrefferaldata = concateuserreferraldata;
                adminGraphModel.userwainindata = concatewalkinuserdata;
                adminGraphModel.usermonthdata = concateuserMonths;




                //----------------------------------- admin Coupon Graph -------------------------//
                var first = context.Admincoupongraphs.Select(d => d.Month).FirstOrDefault();

                var next12Months = Enumerable.Range(0, 12)
                                 .Select(offset => (first + offset - 1) % 12 + 1) // Adjust for months 1-12
                                 .ToList();

                var allMonths = next12Months
                    .Select(month => new { Month = month, MonthText = CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(Convert.ToInt32(month)) })
                    .ToArray();

                var result = from month in allMonths
                             join d in context.Admincoupongraphs.Where(d => d.MerchantId == merchantId)
                             on month.Month equals d.Month into gj
                             from subdata in gj.DefaultIfEmpty()
                             group new { subdata, month } by new { month.Month } into g
                             select new AdminCouponsGraphModel
                             {
                                 Month = g.Key.Month,
                                 MonthText = g.First().month.MonthText,
                                 Couponsredeem = g.Any() ? Convert.ToInt32(g.Sum(s => s.subdata?.RedeemCount ?? 0)) : 0,
                                 Coupons = g.Any() ? Convert.ToInt32(g.Sum(s => s.subdata?.TotalCouponCount ?? 0)) : 0,
                                 // ... map other properties
                             };


                adminGraphModel.AdminCouponsGraphs = result.ToArray();

                var coupons = adminGraphModel.AdminCouponsGraphs.Select(graph => graph.Coupons).ToArray();
                var couponsredeem = adminGraphModel.AdminCouponsGraphs.Select(graph => graph.Couponsredeem).ToArray();
                var couponsMothnst = adminGraphModel.AdminCouponsGraphs.Select(graph => graph.MonthText).ToArray();

                var couponsstring = String.Join(", ", coupons);
                var couponsredeemstring = String.Join(", ", couponsredeem);
                var months = String.Join(", ", couponsMothnst);

                string[] couponsMonths = months.Split(',');
                string[] couponcountdata = couponsstring.Split(',');
                string[] couponredeemcountdata = couponsredeemstring.Split(',');


                for (int i = 0; i < couponsMonths.Length; i++)
                {
                    couponsMonths[i] = couponsMonths[i].Trim(' ', '\'');
                    couponcountdata[i] = couponcountdata[i].Trim(' ', '\'');
                    couponredeemcountdata[i] = couponredeemcountdata[i].Trim(' ', '\'');
                }

                // Add single quotes to the first and last elements
                if (couponsMonths.Length > 0)
                {
                    couponsMonths[0] = $"'{couponsMonths[0]}";
                    couponsMonths[couponsMonths.Length - 1] = $"{couponsMonths[couponsMonths.Length - 1]}'";
                }

                string concatecoupondata = string.Join(", ", couponcountdata);
                string concateredeemcoupondata = string.Join(", ", couponredeemcountdata);
                string concatenatedMonths = string.Join(", ", couponsMonths);

                adminGraphModel.coupondata = concatecoupondata;
                adminGraphModel.monthdata = concatenatedMonths;
                adminGraphModel.couponredeemdata = concateredeemcoupondata;

                //----------------------------------- end Coupon Graph -------------------------//


                var firstfrequentmonth = context.Adminfrequentusergraphs.Select(d => d.Mm).FirstOrDefault();

                var next12Monthsfrequent = Enumerable.Range(0, 12)
                                 .Select(offset => (firstfrequentmonth + offset - 1) % 12 + 1) // Adjust for months 1-12
                                 .ToList();

                var allfrequentMonths = next12Monthsfrequent
                    .Select(month => new { Month = month, MonthText = CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(Convert.ToInt32(month)) })
                    .ToArray();


                var frequentresult = from month in allfrequentMonths
                                     join d in context.Adminfrequentusergraphs.Where(d => d.MerchantId == merchantId)
                                     on month.Month equals d.Mm into gj
                                     from subdata in gj.DefaultIfEmpty()
                                     group new { subdata, month } by new { month.Month } into g
                                     select new AdminFrequentGraphModel
                                     {
                                         Month = g.Key.Month,
                                         MonthText = g.First().month.MonthText,
                                         TotalvisitCount = g.Any() ? Convert.ToInt32(g.Sum(s => s.subdata?.TotalVisitCount ?? 0)) : 0,
                                         TotalFrequentCount = g.Any() ? Convert.ToInt32(g.Sum(s => s.subdata?.TotalFrequentUserCount ?? 0)) : 0,
                                         // ... map other properties
                                     };

                adminGraphModel.AdminFrequentGraphs = frequentresult.ToArray();

                var frequentvist = adminGraphModel.AdminFrequentGraphs.Select(graph => graph.TotalvisitCount).ToArray();
                var frequentuser = adminGraphModel.AdminFrequentGraphs.Select(graph => graph.TotalFrequentCount).ToArray();
                var frequentmonth = adminGraphModel.AdminFrequentGraphs.Select(graph => graph.MonthText).ToArray();

                var visittring = String.Join(", ", frequentvist);
                var frequentuserstring = String.Join(", ", frequentuser);
                var frequentmonthstring = String.Join(", ", frequentmonth);

                string[] frequentmonths = frequentmonthstring.Split(',');
                string[] frequentvists = visittring.Split(',');
                string[] frequentusers = frequentuserstring.Split(',');


                for (int i = 0; i < frequentmonths.Length; i++)
                {
                    frequentmonths[i] = frequentmonths[i].Trim(' ', '\'');
                    frequentvists[i] = frequentvists[i].Trim(' ', '\'');
                    frequentusers[i] = frequentusers[i].Trim(' ', '\'');
                }

                // Add single quotes to the first and last elements
                if (frequentmonths.Length > 0)
                {
                    frequentmonths[0] = $"'{frequentmonths[0]}";
                    frequentmonths[frequentmonths.Length - 1] = $"{frequentmonths[frequentmonths.Length - 1]}'";
                }

                string concatefrequenterndata = string.Join(", ", frequentvists);
                string concatefrequentuserdata = string.Join(", ", frequentusers);
                string concatefrequentMonths = string.Join(", ", frequentmonths);

                adminGraphModel.frequentvistdata = concatefrequenterndata;
                adminGraphModel.frequentuserdata = concatefrequentuserdata;
                adminGraphModel.frequentmonthdata = concatefrequentMonths;

            }



            return adminGraphModel;


        }

      
    }
}

