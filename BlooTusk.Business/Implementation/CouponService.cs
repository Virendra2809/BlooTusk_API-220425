using BlooTusk.Business.Interface;
using BlooTusk.Common;
using BlooTusk.Entity.BlooTuskModel;
using BlooTusk.Model.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MySqlX.XDevAPI.Common;
using System;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.Intrinsics.Arm;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Twilio;
using Twilio.Base;
using Twilio.Rest.Api.V2010.Account;
using Twilio.TwiML.Messaging;
using Twilio.TwiML.Voice;
using Twilio.Types;



namespace BlooTusk.Business.Implementation
{  
    public class CouponService : ICouponService
    {
        BlooTuskContext context;
        private ConfigurationModel configuration;
       
        public CouponService(BlooTuskContext _context, Microsoft.Extensions.Options.IOptions<ConfigurationModel> configuration)
        {
            this.configuration = configuration.Value;
            context = _context;
        }
        public string AddEditCoupon(CouponmasterModel CouponMasters, ref ErrorResponseModel errorResponseModel)
        {
            string statusCode = "";
            bool result = false;
            Couponmaster Couponmaster = new Couponmaster();
            try
            {
                if (CouponMasters.CouponId == 0)
                {
                    Couponissuemaster couponissuemaster = new Couponissuemaster();
                    Couponmaster.CouponTitle = CouponMasters.CouponTitle;
                    Couponmaster.CouponDiscerption = CouponMasters.CouponDiscerption;
                    Couponmaster.CouponCode = CouponGenerator.GenerateCouponCode();
                    Couponmaster.StartDate = CouponMasters.StartDate;
                    Couponmaster.EndDate = CouponMasters.EndDate;
                    Couponmaster.CouponType = CouponMasters.CouponType;
                    Couponmaster.DiscountType = CouponMasters.DiscountType;
                    Couponmaster.DiscountValue = CouponMasters.DiscountValue;
                    Couponmaster.NoOfCoupon = CouponMasters.NoOfCoupon;
                    Couponmaster.MerchantId = CouponMasters.MerchantId;
                    Couponmaster.Status = 1;
                    Couponmaster.RecStatus = CouponMasters.RecStatus;
                    Couponmaster.CreatedBy = CouponMasters.MerchantId;
                    Couponmaster.CreatedDate = DateTime.Now;
                    context.Add(Couponmaster);
                    context.SaveChanges();
                    result = true;

                    couponissuemaster.CouponId = Couponmaster.CouponId;
                    couponissuemaster.IssueDate = DateTime.Now;
                    couponissuemaster.IssuedbyId = (int)Couponmaster.MerchantId;//added to merchant id
                    couponissuemaster.IssuedQty = (int)CouponMasters.NoOfCoupon;
                    couponissuemaster.IssuedTo = CouponMasters.SelectedUser; //platinum,gold,silver
                    context.Add(couponissuemaster);
                    context.SaveChanges();
                    result = true;

                    string userTypes = CouponMasters.SelectedUser;
                    string[] userTypeArray = userTypes.Split(',');
                    string userGoldLevel = "";
                    string userplatinumLevel = "";
                    string usersilverLevel = "";

                    foreach (string type in userTypeArray)
                    {
                        string[] selectedusertype = type.Split('-');

                        switch (selectedusertype[0].ToLower())
                        {
                            case "gold":
                                //doto
                                userGoldLevel = "gold";
                                break;
                            case "platinum":
                                userplatinumLevel = "platinum";
                                //doto
                                break;

                            case "silver":
                                usersilverLevel = "silver";
                                break;
                        }
                    }

                    List<int>? CustomerIds = new List<int>();

                   
                    var levelpoint = (from levelmaster_ in context.Levelmasters
                                      where levelmaster_.RecStatus == 1
                                      select new
                                      {
                                          level = levelmaster_.Level,
                                          RewardPoint = levelmaster_.RequirePoint,
                                      }).Distinct().ToList();

                    var groupedCustomerList = (from customer in context.Customers
                                               join customermerchantmapper in context.Customermerchantmappers on customer.CustomerId equals customermerchantmapper.CustomerId
                                               join merchant in context.Merchants on customermerchantmapper.MerchantId equals merchant.MerchantId
                                               join rewardtransaction in context.Rewardpointtransactions on customer.CustomerId equals rewardtransaction.CustomerId into rewardTransactionsGroup
                                               from rewardtransaction in rewardTransactionsGroup.DefaultIfEmpty()
                                               where (rewardtransaction == null || (rewardtransaction.TransactionType == 1 || rewardtransaction.TransactionType == 2 || rewardtransaction.TransactionType == 3))
                                                   && merchant.MerchantId == CouponMasters.MerchantId
                                                   && customer.RecStatus == "A"
                                               group new { customer, rewardtransaction } by new { customer.CustomerId, customer.Name } into groupedData
                                               select new
                                               {
                                                   CustomerId = groupedData.Key.CustomerId,
                                                   RewardPoint = groupedData.Sum(item => item.rewardtransaction.TransactionType == 3 ? -item.rewardtransaction.Points : item.rewardtransaction.Points ?? 0),
                                               }).ToList();


                    if (userGoldLevel != "")
                    {
                        var customerGoldIds = groupedCustomerList.Where(c => c.RewardPoint < levelpoint[0].RewardPoint && c.RewardPoint >= levelpoint[1].RewardPoint).Select(c => c.CustomerId).ToList();
                        foreach (var customer in customerGoldIds)
                        {
                            CustomerIds.Add(customer);
                        }
                    }
                     if (userplatinumLevel != "")
                    {

                        var customerplatinumIds = groupedCustomerList.Where(c => c.RewardPoint >= levelpoint[0].RewardPoint)
                            .Select(c => c.CustomerId).ToList();

                        foreach (var customer in customerplatinumIds)
                        {
                            CustomerIds.Add(customer);
                        }
                    }
                     if (usersilverLevel != "")
                    {
                        var silverCustomerIds = groupedCustomerList.Where(c => c.RewardPoint < levelpoint[1].RewardPoint)
                                                 .Select(c => c.CustomerId ).ToList();
                       

                        foreach (var customer in silverCustomerIds)
                        {
                           CustomerIds.Add(customer);
                        }
                    }

                    List<Couponissuedetail> couponIssueDetailsList = new List<Couponissuedetail>();
                    if (CouponMasters.NoOfCoupon != 0)
                    {
                        for (int i = 0; i < CustomerIds.Count; i++)
                        {
                            Couponissuedetail couponissuedetail = new Couponissuedetail();
                            couponissuedetail.CouponIssueMasterId = couponissuemaster.CouponIssuemasterId;
                            couponissuedetail.CouponId = Couponmaster.CouponId;
                            couponissuedetail.CustomerId = CustomerIds[i];
                            couponissuedetail.CouponSerialNo = CouponGenerator.CouponCode();
                            couponissuedetail.RecStatus = 1;
                            couponissuedetail.UsedbyId = couponissuedetail.UsedbyId;
                            couponissuedetail.RedeemId = couponissuedetail.RedeemId;
                            context.Couponissuedetails.Add(couponissuedetail);
                            context.SaveChanges();

                        }
                        int? count = CouponMasters.NoOfCoupon - CustomerIds.Count;
                        for (int i = 0; i < count; i++)
                        {
                            Couponissuedetail couponissuedetail = new Couponissuedetail();
                            couponissuedetail.CouponIssueMasterId = couponissuemaster.CouponIssuemasterId;
                            couponissuedetail.CouponId = Couponmaster.CouponId;
                            couponissuedetail.CouponSerialNo = CouponGenerator.CouponCode();
                            couponissuedetail.UsedbyId = couponissuedetail.UsedbyId;
                            couponissuedetail.RedeemId = couponissuedetail.RedeemId;
                            context.Couponissuedetails.Add(couponissuedetail);
                            context.SaveChanges();

                            // couponIssueDetailsList.Add(couponissuedetail);
                        }

                        var merchant = context.Merchants
                                    .Where(merchant => merchant.MerchantId == CouponMasters.MerchantId)
                                    .Join(context.Pos, merchant => merchant.MerchantId, pos => pos.MerchantId, (merchant, pos) => new
                                    {
                                        MerchantName = merchant.OrganizationName + " " + pos.Posname,
                                        MerchantCode = merchant.MerchantCode
                                    })
                                    .FirstOrDefault();

                        var MerchantRefferal = context.Smstemplates
                                      .Where(template => template.MerchantId == CouponMasters.MerchantId && template.MessageTypeId == 5)
                                      .Select(template => new
                                      {
                                          messagecontent = template.MessageContent,
                                      })
                                      .FirstOrDefault();

                        string signupmessage = MerchantRefferal.messagecontent;                       

                        string Signupresults = signupmessage;
                        string Signuppattern = @"\[MerchantName\]";
                        string Signupreplace = merchant.MerchantName;
                        signupmessage = Regex.Replace(Signupresults, Signuppattern, Signupreplace);
                      
                        if (configuration.MessageLimit > CustomerIds.Count)
                        {
                            

                            var decryptedPhoneNumbers = (from c in context.Customers
                                               join cm in context.Customermerchantmappers on c.CustomerId equals cm.CustomerId
                                               where CustomerIds.Contains(c.CustomerId) && cm.StopMessage == false && cm.MerchantId == CouponMasters.MerchantId
                                               select Encoding.UTF8.GetString(CommonUtility.Decrypt(c.PhoneNumber, configuration.SymmetricKey)))
                             .ToList();


                            List<string> formattedPhoneNumbers = new List<string>();
                            foreach (var phoneNumber in decryptedPhoneNumbers)
                            {
                                SendSms(Convert.ToString(phoneNumber), signupmessage);

                                formattedPhoneNumbers.Add(Convert.ToString(phoneNumber));

                            }

                            string LogPhoneNumber = string.Join(", ", formattedPhoneNumbers);

                            string logDirectory = "Log_files/";

                            if (!Directory.Exists(logDirectory))
                            {
                                Directory.CreateDirectory(logDirectory);
                            }

                            string logFileName = "AddEditCoupon_" + DateTime.Now.ToString("yyyy-MM-dd-HHmmss");
                            string message = signupmessage + ", Count: " + decryptedPhoneNumbers.Count + ", Numbers: " + LogPhoneNumber;
                            string logFilePath = Path.Combine(logDirectory, logFileName + ".txt");
                            File.AppendAllText(logFilePath, $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}");

                            var serverPath = "Log_files/" + logFileName + ".txt";
                            var pathToSave = Path.Combine(Directory.GetCurrentDirectory(), serverPath);                            
                           

                            context.SaveChanges(); // Save changes after the loop
                            result = true;
                       
                        }
                        statusCode = "A";
                    }
                }
                else if (CouponMasters.CouponId != 0)
                {
                    var CouponEntity = (from couponmaster_ in context.Couponmasters
                                        where couponmaster_.CouponId == CouponMasters.CouponId
                                        select couponmaster_).FirstOrDefault();

                    var CouponIssuedMaster = (from Couponissuemasters_ in context.Couponissuemasters
                                              where Couponissuemasters_.CouponId == CouponMasters.CouponId
                                              select Couponissuemasters_).FirstOrDefault();
                        
                        
                        //context.Couponissuemasters.FirstOrDefault(x => x.CouponId == CouponMasters.CouponId);



                    if (CouponEntity != null)
                    {                        //coupon master updated
                      
                        List<int> CustomerIds = new List<int>();
                        string userTypes = CouponMasters.SelectedUser;
                        string[] userTypeArray = userTypes.Split(',');
                        string userGoldLevel = "";
                        string userplatinumLevel = "";
                        string usersilverLevel = "";

                        foreach (string type in userTypeArray)
                        {
                            string[] selectedusertype = type.Split('-');

                            switch (selectedusertype[0].ToLower())
                            {
                                case "gold":
                                    //doto
                                    userGoldLevel = "gold";
                                    break;
                                case "platinum":
                                    userplatinumLevel = "platinum";
                                    //doto
                                    break;

                                case "silver":
                                    usersilverLevel = "silver";
                                    break;
                            }
                        }

                        if (userGoldLevel != "")
                        {
                            var levelPointThreshold = context.Levelmasters
                              .Where(levelmaster_ => levelmaster_.RecStatus == 1 && levelmaster_.Level == userGoldLevel)
                              .Select(levelmaster_ => levelmaster_.RequirePoint)
                              .FirstOrDefault();

                            var customerGoldIds = context.Customers
                            .Join(context.Rewardpointtransactions, customer => customer.CustomerId, rewardtransaction => rewardtransaction.CustomerId, (customer, rewardtransaction) => new { customer, rewardtransaction })
                            .Join(context.Rewardpointmasters, combined => combined.rewardtransaction.RewardPointId, rewardmaster => rewardmaster.RewardPonitId, (combined, rewardmaster) => new { combined.customer, rewardmaster })
                            .Where(combined => combined.rewardmaster.RewardTypeId == 1 || combined.rewardmaster.RewardTypeId == 2)
                            .GroupBy(combined => combined.customer.CustomerId)
                            .Select(groupedData => new
                            {
                                RewardPoint = groupedData.Sum(item => item.rewardmaster.RewardPoint),
                                CustomerID = groupedData.Key
                            })
                            .Where(c => c.RewardPoint >= levelPointThreshold)
                            .Select(c => c.CustomerID)
                            .ToList();

                            foreach (var customer in customerGoldIds)
                            {
                                CustomerIds.Add(customer);
                            }
                        }
                        else if (userplatinumLevel != "")
                        {
                            var levelPointThreshold = context.Levelmasters
                             .Where(levelmaster_ => levelmaster_.RecStatus == 1 && levelmaster_.Level == userplatinumLevel)
                             .Select(levelmaster_ => levelmaster_.RequirePoint)
                             .FirstOrDefault();

                            var customerplatinumIds = context.Customers
                            .Join(context.Rewardpointtransactions, customer => customer.CustomerId, rewardtransaction => rewardtransaction.CustomerId, (customer, rewardtransaction) => new { customer, rewardtransaction })
                            .Join(context.Rewardpointmasters, combined => combined.rewardtransaction.RewardPointId, rewardmaster => rewardmaster.RewardPonitId, (combined, rewardmaster) => new { combined.customer, rewardmaster })
                            .Where(combined => combined.rewardmaster.RewardTypeId == 1 || combined.rewardmaster.RewardTypeId == 2)
                            .GroupBy(combined => combined.customer.CustomerId)
                            .Select(groupedData => new
                            {
                                RewardPoint = groupedData.Sum(item => item.rewardmaster.RewardPoint),
                                CustomerID = groupedData.Key
                            })
                            .Where(c => c.RewardPoint >= levelPointThreshold)
                            .Select(c => c.CustomerID)
                            .ToList();

                            foreach (var customer in customerplatinumIds)
                            {
                                CustomerIds.Add(customer);
                            }

                        }
                        else if (usersilverLevel != "")
                        {
                            var levelPointThreshold = context.Levelmasters
                             .Where(levelmaster_ => levelmaster_.RecStatus == 1 && levelmaster_.Level == usersilverLevel)
                             .Select(levelmaster_ => levelmaster_.RequirePoint)
                             .FirstOrDefault();

                            var customersilverIds = context.Customers
                            .Join(context.Rewardpointtransactions, customer => customer.CustomerId, rewardtransaction => rewardtransaction.CustomerId, (customer, rewardtransaction) => new { customer, rewardtransaction })
                            .Join(context.Rewardpointmasters, combined => combined.rewardtransaction.RewardPointId, rewardmaster => rewardmaster.RewardPonitId, (combined, rewardmaster) => new { combined.customer, rewardmaster })
                            .Where(combined => combined.rewardmaster.RewardTypeId == 1 || combined.rewardmaster.RewardTypeId == 2)
                            .GroupBy(combined => combined.customer.CustomerId)
                            .Select(groupedData => new
                            {
                                RewardPoint = groupedData.Sum(item => item.rewardmaster.RewardPoint),
                                CustomerID = groupedData.Key
                            })
                            .Where(c => c.RewardPoint >= levelPointThreshold)
                            .Select(c => c.CustomerID)
                            .ToList();

                            foreach (var customer in customersilverIds)
                            {
                                CustomerIds.Add(customer);
                            }
                        }

                        List<int> responseList = new List<int>();

                        var query = context.Couponissuedetails
                        .Where(c => responseList.Contains(c.CustomerId))
                        .ToList();

                        List<int> NewCustomerIds = responseList.Except(CustomerIds).ToList();

                        if (CouponEntity.NoOfCoupon < CouponMasters.NoOfCoupon)
                        {
                            int? count = ( CouponMasters.NoOfCoupon - CouponEntity.NoOfCoupon);

                            List<Couponissuedetail> couponIssueDetailsList = new List<Couponissuedetail>();
                            if (count != 0)
                            {
                                for (int i = 0; i < NewCustomerIds.Count; i++)
                                {
                                    Couponissuedetail couponissuedetail = new Couponissuedetail();
                                    couponissuedetail.CouponIssueMasterId = CouponIssuedMaster.CouponIssuemasterId;
                                    couponissuedetail.CouponId = CouponMasters.CouponId;
                                    couponissuedetail.CustomerId = NewCustomerIds[i];
                                    couponissuedetail.CouponSerialNo = CouponGenerator.CouponCode();
                                    couponissuedetail.UsedbyId = couponissuedetail.UsedbyId;
                                    couponissuedetail.RedeemId = couponissuedetail.RedeemId;
                                    context.Couponissuedetails.Add(couponissuedetail);
                                    context.SaveChanges();
                                }
                                if(NewCustomerIds.Count > 0)
                                {
                                    int? counts = count - NewCustomerIds.Count;
                                    for (int i = 0; i < counts; i++)
                                    {
                                        Couponissuedetail couponissuedetail = new Couponissuedetail();
                                        couponissuedetail.CouponIssueMasterId = CouponIssuedMaster.CouponIssuemasterId;
                                        couponissuedetail.CouponId = CouponMasters.CouponId;
                                        couponissuedetail.CouponSerialNo = CouponGenerator.CouponCode();
                                        couponissuedetail.UsedbyId = couponissuedetail.UsedbyId;
                                        couponissuedetail.RedeemId = couponissuedetail.RedeemId;
                                        context.Couponissuedetails.Add(couponissuedetail);
                                        context.SaveChanges();

                                        // couponIssueDetailsList.Add(couponissuedetail);
                                    }
                                }
                                else
                                {
                                   // int? counts = CouponMasters.NoOfCoupon - CouponEntity.NoOfCoupon;
                                    for (int i = 0; i < count; i++)
                                    {
                                        Couponissuedetail couponissuedetail = new Couponissuedetail();
                                        couponissuedetail.CouponIssueMasterId = CouponIssuedMaster.CouponIssuemasterId;
                                        couponissuedetail.CouponId = CouponMasters.CouponId;
                                        couponissuedetail.CouponSerialNo = CouponGenerator.CouponCode();
                                        couponissuedetail.UsedbyId = couponissuedetail.UsedbyId;
                                        couponissuedetail.RedeemId = couponissuedetail.RedeemId;
                                        context.Couponissuedetails.Add(couponissuedetail);
                                        context.SaveChanges();

                                        // couponIssueDetailsList.Add(couponissuedetail);
                                    }
                                }
                                context.SaveChanges(); // Save changes after the loop
                                result = true;
                                statusCode = "A";
                            }

                            
                        }


                        if (CouponEntity != null)
                        {
                            if(CouponMasters.NoOfCoupon != 0)
                            {
                                CouponEntity.NoOfCoupon = CouponMasters.NoOfCoupon;
                            }
                           
                            CouponEntity.EndDate = CouponMasters.EndDate;
                            CouponEntity.RecStatus = CouponMasters.RecStatus;
                            context.SaveChanges();
                        }

                        //coupon IssuedMAster Update

                        if (CouponIssuedMaster != null)
                        {
                            CouponIssuedMaster.IssuedQty = Convert.ToInt32(CouponMasters.NoOfCoupon);
                            CouponIssuedMaster.IssuedTo = CouponMasters.SelectedUser;
                            context.SaveChanges();
                        }
                    }
                    result = true;
                    statusCode = "U";
                }
            }
            catch (Exception ex)
            {
                throw;
            }

            return statusCode;
        }

        static void CreateTextFile(string filePath, string content)
        {
            // Use StreamWriter to create and write to the text file
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                // Write the content to the file
                writer.Write(content);
            }
        }
        public bool DeleteCoupon(int couponId, ref ErrorResponseModel errorResponseModel)
        {
            throw new NotImplementedException();
        }
        public CouponSearchModel GetAllCampaigns(CampainListSerach campainListSerach, ref ErrorResponseModel errorResponseModel)
        {
            var pageNumber = (campainListSerach.PageNumber <= 0) ? 1 : campainListSerach.PageNumber;
            var pageSize = 10;
            var totalRecords = 0.0;
            var totalPages = 0.0;
            var skip = 0;
            errorResponseModel = new ErrorResponseModel();

            skip = (pageNumber - 1) * pageSize;
            CouponSearchModel couponSearchModel = new CouponSearchModel();
            try
            {
                DateTime oneMonthAgo = DateTime.Now.Date.AddMonths(-1);//.ToShortDateString("yyyy/MM/dd");

                var campaign = (from Couponmaster_ in context.Couponmasters
                                where Couponmaster_.MerchantId == campainListSerach.MerchantId
                                select new
                                {
                                    EndDate = Couponmaster_.EndDate,
                                }).ToList();


                couponSearchModel.TotalCampagign = campaign.Count;
         
             //   couponSearchModel.ExpiredCampagign = couponSearchModel.TotalCampagign - couponSearchModel.LiveCampagign;

                couponSearchModel.ExpiredCampagign = campaign.Count(c => c.EndDate < DateTime.Today); 
                couponSearchModel.LiveCampagign = campaign.Count - couponSearchModel.ExpiredCampagign;

                var CouponList = (from cm in context.Couponmasters
                                  join cap in context.Couponissuemasters on cm.CouponId equals cap.CouponId
                                  join Merchant_ in context.Merchants on cm.MerchantId equals Merchant_.MerchantId
                                  join pos_ in context.Pos on Merchant_.MerchantId equals pos_.MerchantId
                                  join state_ in context.Statemasters on pos_.StateId equals state_.StateId
                                  where cm.MerchantId == campainListSerach.MerchantId                                  
                               
                                  select new CampagignListModel
                                  {
                                      CampagingID = cm.CouponId,
                                      NoOfCoupon = cm.NoOfCoupon,
                                      CouponTitle = cm.CouponTitle,
                                      CouponDiscerption = cm.CouponDiscerption,
                                      Status = cm.Status,
                                      StartDate = cm.StartDate,
                                      EndDate = cm.EndDate,
                                      SelectedUser = cap.IssuedTo,
                                      RecStatus = cm.RecStatus,
                                      State = state_.StateName,
                                      CouponType = cm.CouponType,
                                      DiscountType = cm.DiscountType,
                                      DiscountValue = cm.DiscountValue,
                                      CreatedDate = cm.CreatedDate,
                                      MerchantName = Merchant_.OrganizationName,
                                      isSelected = false,
                                      VisibleMenu = false,
                            
                                      SendCount = (from coupon_ in context.Couponmasters
                                                   join cid in context.Couponissuedetails on coupon_.CouponId equals cid.CouponId
                                                   where cid.CustomerId != 0 && cid.CouponId == cm.CouponId
                                                   select cid).Count(),

                                      RedeemCount = (from coupon_ in context.Couponmasters
                                                    join cid in context.Couponissuedetails on coupon_.CouponId equals cid.CouponId
                                                    join cr in context.Redeemtions on cid.CouponIssueDetailsId equals cr.CouponIssuedetailsId
                                                    where coupon_.CouponId == cm.CouponId
                                                    select cr).Count(),
                                      NudgedCoupon = (context.Nudgecoupons
                                            .Any(nudge => nudge.CouponId == cm.CouponId && nudge.CreatedDate >= oneMonthAgo))

                                  }).ToList();

                if (!String.IsNullOrEmpty(campainListSerach.Recent) || !String.IsNullOrEmpty(campainListSerach.Performance) || !String.IsNullOrEmpty(campainListSerach.Type))
                {
                    if (campainListSerach.Recent== "Recent")
                    {
                        var sortedCouponList = CouponList.OrderByDescending(coupon => coupon.CreatedDate)
                                   .Skip(skip)
                                   .Take(pageSize)
                                   .ToList();


                        CouponList = sortedCouponList;
                    }

                    else if (campainListSerach.Recent == "Alphabatical")
                    {
                            var sortedCouponList = CouponList.OrderBy(customer => customer.CouponTitle)
                                                             .Skip(skip)
                                                             .Take(pageSize)
                                                             .ToList();                           
                            CouponList = sortedCouponList;     
                    }

                    else if (!String.IsNullOrEmpty(campainListSerach.Performance))
                    {
                        CouponList = CouponList
                                      .OrderByDescending(campaign => campaign.RedeemCount)
                                      .Skip(skip)
                                      .Take(pageSize)
                                      .ToList();
                    }
                
                    else if (!String.IsNullOrEmpty(campainListSerach.Type))
                    {

                        switch (campainListSerach.Type)
                        {
                            case "Total":
                                CouponList = CouponList
                                    .Skip(skip)
                                    .Take(pageSize)
                                    .ToList();
                                break;

                            case "Live":
                                CouponList = CouponList
                                    .Where(c => c.EndDate >= DateTime.Now.Date)
                                    .Skip(skip)
                                    .Take(pageSize)
                                    .ToList();
                                break;

                            case "Expired":
                                CouponList = CouponList
                                    .Where(c => c.EndDate < DateTime.Now.Date)
                                    .Skip(skip)
                                    .ToList();
                                break;

                            case "SpacialOffer":
                                CouponList = CouponList
                                    .Where(c => c.EndDate >= DateTime.Today && c.SendCount < c.NoOfCoupon && c.RecStatus == "A")
                                    .Skip(skip)
                                    .Take(pageSize)
                                    .ToList();
                                break;

                            default:
                                // Handle the default case here if needed
                                break;
                        }

                        if (campainListSerach.CustomerId != 0)
                        {
                            var results = (from couponIssueDetail in context.Couponissuedetails
                                           where couponIssueDetail.CustomerId == campainListSerach.CustomerId
                                           select new
                                           {
                                               CouponId = couponIssueDetail.CouponId,
                                           }).ToList();

                            CouponList = CouponList.Where(c => c.EndDate >= DateTime.Now.Date && !results.Select(r => r.CouponId).Contains(c.CampagingID))
                                              .Skip(skip)
                                              .Take(pageSize)
                                              .ToList();
                        }
                    }
                    else
                    {
                        CouponList // Order by CreatedDate in descending order
                      .Skip(skip)
                      .Take(pageSize)
                      .ToList();

                    }
               
                    couponSearchModel.campagignList = CouponList.ToList();
                }
                else
                {
                    couponSearchModel.campagignList = CouponList.Skip(skip)
                      .Take(pageSize)
                      .ToList(); 
                }
            }
            catch (Exception)
            {
                throw;
            }
            return couponSearchModel;
        }



        public CouponmasterModel GetCouponById(int CouponId, ref ErrorResponseModel errorResponseModel)
        {
            var CouponData = (from CouponMaster in context.Couponmasters
                              select new CouponmasterModel
                              {
                                  CouponId = CouponMaster.CouponId,
                                  CouponTitle = CouponMaster.CouponTitle,
                                  //CouponCode = CouponMaster.CouponCode,
                                  RecStatus = CouponMaster.RecStatus,
                              }).SingleOrDefault();
            if (CouponData == null)
            {
                errorResponseModel.StatusCode = HttpStatusCode.NotFound;
                errorResponseModel.Message = "No Data found";
            }
            return CouponData;
        }

        public CouponQrModel GetCouponByCouponCode(string CouponCode, ref ErrorResponseModel errorResponseModel)
        {
            try
            {
                var CouponData = (from CouponMaster in context.Couponmasters
                                  join Couponissue in context.Couponissuedetails
                                  on CouponMaster.CouponId equals (int?)Couponissue.CouponId
                                  join merchant in context.Merchants on CouponMaster.MerchantId equals
                                  merchant.MerchantId
                                  join p in context.Pos on merchant.MerchantId equals p.MerchantId
                                  where Couponissue.CouponSerialNo == CouponCode
                                  select new CouponQrModel
                                  {
                                      CouponTitle = CouponMaster.CouponTitle,
                                      CouponCode = CouponCode,
                                      CouponDiscerption = CouponMaster.CouponDiscerption,
                                      MerchantName = merchant.OrganizationName,
                                      EndDate = CouponMaster.EndDate,
                                      Address = (p.Posname != null ? p.Posname + ", " : "") + merchant.City,
                                      // DiscountType = ( CouponMaster.DiscountValue + CouponMaster.DiscountType:)
                                      DiscountType = CouponMaster.DiscountValue + (CouponMaster.DiscountType == "percentage" ? "%" : "$"),

                  }).SingleOrDefault();

                if (CouponData == null)
                {
                    errorResponseModel.StatusCode = HttpStatusCode.NotFound;
                    errorResponseModel.Message = "No Data found";
                }

                return CouponData;
            }
            catch (Exception ex)
            {
                // Handle exceptions appropriately, log or rethrow as needed
                errorResponseModel.StatusCode = HttpStatusCode.InternalServerError;
                errorResponseModel.Message = "An error occurred while processing the request.";
                // Log the exception or handle it based on your application's logging strategy.
                // Log.Error(ex, "Error in GetCouponByCouponCode method");
                return null; // Or throw the exception if you want to propagate it further.
            }
        } 
        public IssuedToModel GetIssuedToDDL(IssuedSearchModel issuedSearchModel, ref ErrorResponseModel errorResponseModel)
        {
            IssuedToModel issuedToModel = new IssuedToModel();

            errorResponseModel = new ErrorResponseModel();

            //var groupedCustomerList = (from customer in context.Customers
            //                           join customermerchantmapper in context.Customermerchantmappers on customer.CustomerId equals customermerchantmapper.CustomerId
            //                           join merchant in context.Merchants on customermerchantmapper.MerchantId equals merchant.MerchantId
            //                           join rewardtransaction in context.Rewardpointtransactions on customer.CustomerId equals rewardtransaction.CustomerId into rewardTransactionsGroup
            //                           from rewardtransaction in rewardTransactionsGroup.DefaultIfEmpty()
            //                           join rewardmaster in context.Rewardpointmasters on rewardtransaction.RewardPointId equals rewardmaster.RewardPonitId into rewardMastersGroup
            //                           from rewardmaster in rewardMastersGroup.DefaultIfEmpty()
            //                           where merchant.MerchantId == issuedSearchModel.MerchantId && customer.RecStatus == "A"
            //                           group new { customer, rewardmaster } by new { customer.CustomerId, customer.Name } into groupedData                                   

            //                           select new
            //                           {
            //                               CustomerId = groupedData.Key.CustomerId,
            //                               RewardPoint = (groupedData.Where(item=>(item.rewardmaster.RewardTypeId==1 || item.rewardmaster.RewardTypeId == 2)).Sum(item=>item.rewardmaster.RewardPoint))-(groupedData.Where(item => (item.rewardmaster.RewardTypeId == 3)).Sum(item => item.rewardmaster.RewardPoint)),
            //                           }).ToList();


            var groupedCustomerList = (from customer in context.Customers
                                       join customermerchantmapper in context.Customermerchantmappers on customer.CustomerId equals customermerchantmapper.CustomerId
                                       join merchant in context.Merchants on customermerchantmapper.MerchantId equals merchant.MerchantId
                                       join rewardtransaction in context.Rewardpointtransactions on customer.CustomerId equals rewardtransaction.CustomerId into rewardTransactionsGroup
                                       from rewardtransaction in rewardTransactionsGroup.DefaultIfEmpty()
                                       //join rewardmaster in context.Rewardpointmasters on rewardtransaction.RewardPointId equals rewardmaster.RewardPonitId into rewardMastersGroup
                                       //from rewardmaster in rewardMastersGroup.DefaultIfEmpty()
                                       where 
                                       //
                                       merchant.MerchantId == issuedSearchModel.MerchantId &&
                                       customer.RecStatus == "A"
                                       group new { customer, rewardtransaction } by new { customer.CustomerId, customer.Name } into groupedData

                                       select new
                                       {
                                           CustomerId = groupedData.Key.CustomerId,
                                           RewardPoint = (groupedData.Where(item => (item.rewardtransaction.TransactionType == 1 || item.rewardtransaction.TransactionType == 2)).Sum(item => item.rewardtransaction.Points)) - (groupedData.Where(item => (item.rewardtransaction.TransactionType == 3)).Sum(item => item.rewardtransaction.Points)),
                                       }).ToList();


            var levelpoint = (from levelmaster_ in context.Levelmasters
                              where levelmaster_.RecStatus == 1
                              select new
                              {
                                  level = levelmaster_.Level,
                                  RewardPoint = levelmaster_.RequirePoint,
                              }).Distinct().ToList();

            issuedToModel.Platinumcustomercount = groupedCustomerList.Where(c => c.RewardPoint >= levelpoint[0].RewardPoint).ToList().Count;
            issuedToModel.Goldcustomercount = groupedCustomerList.Where(c => c.RewardPoint < levelpoint[0].RewardPoint && c.RewardPoint >= levelpoint[1].RewardPoint).ToList().Count;
            issuedToModel.Silvercustomercount = groupedCustomerList.Where(c => c.RewardPoint < levelpoint[1].RewardPoint).ToList().Count;



            if (issuedSearchModel.KeyWord != "" && issuedSearchModel.CouponId != 0)
            {
                var result = (from couponIssueDetail in context.Couponissuedetails
                              where couponIssueDetail.CouponId == issuedSearchModel.CouponId
                              select new
                              {
                                  IssuedCustomerId = couponIssueDetail.CustomerId,
                              });

                var excludedCustomerIds = result.Select(r => r.IssuedCustomerId).ToList();

                issuedToModel.Platinumcustomercount = groupedCustomerList
                    .Where(c => c.RewardPoint >= levelpoint[0].RewardPoint && !excludedCustomerIds.Contains(c.CustomerId))
                    .ToList()
                    .Count;

                issuedToModel.Goldcustomercount = groupedCustomerList
                    .Where(c => c.RewardPoint < levelpoint[0].RewardPoint && c.RewardPoint >= levelpoint[1].RewardPoint && !excludedCustomerIds.Contains(c.CustomerId))
                    .ToList()
                    .Count;

                issuedToModel.Silvercustomercount = groupedCustomerList
                    .Where(c => c.RewardPoint < levelpoint[1].RewardPoint && !excludedCustomerIds.Contains(c.CustomerId))
                    .ToList()
                    .Count;
            }

            return issuedToModel;
        }
        public CustomerCouponListModel GetCustomerCouponList(CustomerCouponList customerCouponList)
        {
          var  CustCouponList = new CustomerCouponListModel();

           // var merchantList = new MerchantSearchResultModel();
            try
            {
                var pageNumber = (customerCouponList.PageNumber <= 0) ? 1 : customerCouponList.PageNumber;
                var pageSize = 10;
                var totalRecords = 0.0;
                var totalPages = 0.0;
                var skip = 0;

                if (customerCouponList.phoneNumber != "")
                {
                    totalRecords = (from coupon_ in context.Couponmasters
                                                join couponissuemaster_ in context.Couponissuemasters on coupon_.CouponId equals couponissuemaster_.CouponId
                                                join couponissuedetail_ in context.Couponissuedetails on couponissuemaster_.CouponIssuemasterId equals couponissuedetail_.CouponIssueMasterId
                                                join customer_ in context.Customers on couponissuedetail_.CustomerId equals customer_.CustomerId
                                                where customer_.PhoneNumber == customerCouponList.phoneNumber
                                                select couponissuedetail_
                        ).Count();
                    totalPages = Math.Ceiling((double)totalRecords / pageSize);
                    skip = (pageNumber - 1) * pageSize;

                    var CouponData = (from coupon_ in context.Couponmasters
                                    join couponissuemaster_ in context.Couponissuemasters on coupon_.CouponId equals couponissuemaster_.CouponId
                                    join couponissuedetail_ in context.Couponissuedetails on couponissuemaster_.CouponIssuemasterId equals couponissuedetail_.CouponIssueMasterId
                                    join customer_ in context.Customers on couponissuedetail_.CustomerId equals customer_.CustomerId
                                    join merchant_ in context.Merchants on coupon_.MerchantId equals merchant_.MerchantId
                                 //   join merchantsystemuser_ in context.Merchantsystemusers on coupon_.MerchantId equals merchantsystemuser_.MerchantId
                                      join pos_ in context.Pos on merchant_.MerchantId equals pos_.MerchantId
                                    join state_ in context.Statemasters on pos_.StateId equals state_.StateId
                                    where customer_.PhoneNumber == customerCouponList.phoneNumber

                                        let redeemcoupon = (from R in context.Redeemtions
                                                            join Cid in context.Couponissuedetails on R.CouponIssuedetailsId equals Cid.CouponIssueDetailsId
                                                            where Cid.CouponIssueDetailsId == couponissuedetail_.CouponIssueDetailsId
                                                            select R).ToList()  // Use ToList to materialize the result

                                        select new CouponList
                                        {
                                        CouponId = coupon_.CouponId,
                                        MerchantDetail = merchant_.OrganizationName + " " + state_.StateName,
                                        //    ImageUrl = merchantsystemuser_.ImageUrl,
                                        CouponTitle = coupon_.CouponTitle,
                                        CouponDiscerption = coupon_.CouponDiscerption,
                                        Source = merchant_.OrganizationName,
                                        Address = (pos_.Posname != null ? pos_.Posname + ", " : "") + merchant_.City,


                                        Status = redeemcoupon.Any() ? 3 :
                                                 coupon_.EndDate > DateTime.Today ? 1 :
                                                 coupon_.EndDate < DateTime.Today ? 2 : 0,

                                        CouponType = coupon_.CouponType,
                                        DiscountType = coupon_.DiscountType,
                                        DiscountValue = coupon_.DiscountValue,
                                        StartDate = coupon_.StartDate,
                                        EndDate = coupon_.EndDate,
                                        CouponCode = couponissuedetail_.CouponSerialNo,
                                    })//.Skip(skip)
                //.Take(pageSize)
                .ToList();

                    CustCouponList.couponList = CouponData.ToList();
                }
 
                else if (customerCouponList.CustomerId != 0)
                {
                    //totalRecords = (from coupon_ in context.Couponmasters
                    //                            join couponissuemaster_ in context.Couponissuemasters on coupon_.CouponId equals couponissuemaster_.CouponId
                    //                            join couponissuedetail_ in context.Couponissuedetails on couponissuemaster_.CouponIssuemasterId equals couponissuedetail_.CouponIssueMasterId
                    //                            join customer_ in context.Customers on couponissuedetail_.CustomerId equals customer_.CustomerId
                    //                            where customer_.CustomerId == customerCouponList.CustomerId
                    //                            select couponissuedetail_ ).Count();


                    totalRecords = (from coupon_ in context.Couponmasters
                                  //  join couponissuemaster_ in context.Couponissuemasters on coupon_.CouponId equals couponissuemaster_.CouponId
                                    join couponissuedetail_ in context.Couponissuedetails on coupon_.CouponId equals couponissuedetail_.CouponId
                                    join customer_ in context.Customers on couponissuedetail_.CustomerId equals customer_.CustomerId
                                    where customer_.CustomerId == customerCouponList.CustomerId && coupon_.MerchantId == coupon_.MerchantId
                                    select couponissuedetail_  ).Count();

                    totalPages = Math.Ceiling((double)totalRecords / pageSize);
                    skip = (pageNumber - 1) * pageSize;

                    //    var CouponData = (from coupon_ in context.Couponmasters
                    //                join couponissuemaster_ in context.Couponissuemasters on coupon_.CouponId equals couponissuemaster_.CouponId
                    //                join couponissuedetail_ in context.Couponissuedetails on couponissuemaster_.CouponIssuemasterId equals couponissuedetail_.CouponIssueMasterId
                    //                join customer_ in context.Customers on couponissuedetail_.CustomerId equals customer_.CustomerId
                    //                join merchant_ in context.Merchants on coupon_.MerchantId equals merchant_.MerchantId
                    //                join pos_ in context.Pos on merchant_.MerchantId equals pos_.MerchantId
                    //                join state_ in context.Statemasters on pos_.StateId equals state_.StateId
                    //                where customer_.CustomerId == customerCouponList.CustomerId
                    //                let redeemcoupon = (from R in context.Redeemtions
                    //                                    join Cid in context.Couponissuedetails on R.CouponIssuedetailsId equals Cid.CouponIssueDetailsId
                    //                                    where Cid.CouponIssueDetailsId == couponissuedetail_.CouponIssueDetailsId
                    //                                    select R).ToList()
                    //                select new CouponList
                    //                {
                    //                    CouponTitle = coupon_.CouponTitle,
                    //                    CouponDiscerption = coupon_.CouponDiscerption,
                    //                    Status = redeemcoupon.Any() ? 3 :
                    //                                 coupon_.EndDate > DateTime.Today ? 1 :
                    //                                 coupon_.EndDate < DateTime.Today ? 2 : 0,
                    //                    MerchantDetail = merchant_.OrganizationName + "," + state_.StateName,
                    //                    CouponType = coupon_.CouponType,
                    //                    DiscountType = coupon_.DiscountType,
                    //                    DiscountValue = coupon_.DiscountValue,
                    //                    StartDate = coupon_.StartDate,
                    //                    EndDate = coupon_.EndDate,
                    //                    CouponCode = couponissuedetail_.CouponSerialNo,
                    //                    CouponId = coupon_.CouponId,

                    //                }).Skip(skip)
                    //.Take(pageSize)
                    //.ToList();


                    var CouponData = (from coupon_ in context.Couponmasters
                                     // join couponissuemaster_ in context.Couponissuemasters on coupon_.CouponId equals couponissuemaster_.CouponId
                                      join couponissuedetail_ in context.Couponissuedetails on coupon_.CouponId equals couponissuedetail_.CouponId
                                      join customer_ in context.Customers on couponissuedetail_.CustomerId equals customer_.CustomerId
                                      join merchant_ in context.Merchants on coupon_.MerchantId equals merchant_.MerchantId
                                      //   join merchantsystemuser_ in context.Merchantsystemusers on coupon_.MerchantId equals merchantsystemuser_.MerchantId
                                      join pos_ in context.Pos on merchant_.MerchantId equals pos_.MerchantId
                                      join state_ in context.Statemasters on pos_.StateId equals state_.StateId
                                      where customer_.CustomerId == customerCouponList.CustomerId && coupon_.MerchantId == customerCouponList.MerchantId

                                      let redeemcoupon = (from R in context.Redeemtions
                                                          join Cid in context.Couponissuedetails on R.CouponIssuedetailsId equals Cid.CouponIssueDetailsId
                                                          where Cid.CouponIssueDetailsId == couponissuedetail_.CouponIssueDetailsId
                                                          select R).ToList()  // Use ToList to materialize the result

                                      select new CouponList
                                      {
                                          CouponId = coupon_.CouponId,
                                          MerchantDetail = merchant_.OrganizationName + " " + state_.StateName,
                                          //    ImageUrl = merchantsystemuser_.ImageUrl,
                                          CouponTitle = coupon_.CouponTitle,
                                          CouponDiscerption = coupon_.CouponDiscerption,
                                          Source = merchant_.OrganizationName,

                                          Status = redeemcoupon.Any() ? 3 :
                                               coupon_.EndDate > DateTime.Today ? 1 :
                                               coupon_.EndDate < DateTime.Today ? 2 : 0,

                                          CouponType = coupon_.CouponType,
                                          DiscountType = coupon_.DiscountType,
                                          DiscountValue = coupon_.DiscountValue,
                                          StartDate = coupon_.StartDate,
                                          EndDate = coupon_.EndDate,
                                          CouponCode = couponissuedetail_.CouponSerialNo,
                                      }).ToList();

                    CustCouponList.couponList = CouponData.ToList();

                }


                // Assuming CustCouponList.couponList is a list of objects with properties 'couponType' and 'status'
                List<CouponList> filteredList = new List<CouponList>();

                if (customerCouponList.CouponFilter == 1 && CustCouponList.couponList != null)
                {
                    // Transferable: couponType = 1 and status != 3 (Expire) and
                    // Non-Transferable: couponType = 2 and status != 2 and 3
                    var transferableCoupons = CustCouponList.couponList.Where(coupon =>
                          coupon.CouponType == 1 && (coupon.Status == 3 && coupon.EndDate > DateTime.Today)
                       ).ToList();


                    var transferableCouponsstatus= CustCouponList.couponList
                        .Where(coupon => (coupon.CouponType == 1 && coupon.Status == 1))
                        .ToList();

                    

                    var nonTransferableCoupons = CustCouponList.couponList.Where(coupon =>
                        coupon.CouponType == 2 && coupon.Status == 1
                    ).ToList();

                    // Combine both lists if needed
                    filteredList.AddRange(transferableCoupons);
                    filteredList.AddRange(transferableCouponsstatus);
                    filteredList.AddRange(nonTransferableCoupons);
                }
                else if (customerCouponList.CouponFilter == 3)
                {
                    // Non-Transferable: couponType = 2 and status == 2
                    //filteredList = CustCouponList.couponList.Where(coupon =>
                    //    coupon.CouponType == 2 && coupon.Status == 3
                    //).ToList();

                    var transferableCoupons = CustCouponList.couponList.Where(coupon =>
                    coupon.CouponType == 1 && coupon.Status == 3 
                     ).ToList();

                    var nonTransferableCoupons = CustCouponList.couponList.Where(coupon =>
                        coupon.CouponType == 2 && coupon.Status == 3
                    ).ToList();                

                    // Combine both lists if needed
                    filteredList.AddRange(transferableCoupons);
                    filteredList.AddRange(nonTransferableCoupons);

                }
                else if (customerCouponList.CouponFilter == 2)
                {
                    // Transferable: couponType = 1 and status == 3
                    // Non-Transferable: couponType = 2 and status == 3
                    var transferableCoupons = CustCouponList.couponList.Where(coupon =>
                          coupon.CouponType == 1 && coupon.Status == 2 
                      ).ToList();

                    var nonTransferableCoupons = CustCouponList.couponList.Where(coupon =>
                        coupon.CouponType == 2 && coupon.Status == 2
                    ).ToList();

                    // Combine both lists if needed
                    filteredList.AddRange(transferableCoupons);
                    filteredList.AddRange(nonTransferableCoupons);
                }

                CustCouponList.couponList = filteredList; 
                // filteredList now contains the filtered coupons based on the conditions provided


            }
            catch (Exception)
            {
                throw;
            }
            return CustCouponList;
        }
        public List<CustomerCouponModel> GetCampaignsPerformance(CouponSearchPerformanceModel couponSearchPerformanceModel, ref ErrorResponseModel errorResponseModel)            
        {
            var pageNumber = (couponSearchPerformanceModel.PageNumber <= 0) ? 1 : couponSearchPerformanceModel.PageNumber;
            var pageSize = 10;
            var totalRecords = 0.0;
            var totalPages = 0.0;
            var skip = 0;

            skip = (pageNumber - 1) * pageSize;

            var customerDAta = (from coupon_ in context.Couponmasters
                                join merchant in context.Merchants on coupon_.MerchantId equals merchant.MerchantId
                                join pos_ in context.Pos on merchant.MerchantId equals pos_.MerchantId
                                join state_ in context.Statemasters on pos_.StateId equals state_.StateId
                                join couponissuemaster_ in context.Couponissuemasters on coupon_.CouponId equals couponissuemaster_.CouponId
                                join couponissuedetail_ in context.Couponissuedetails on couponissuemaster_.CouponIssuemasterId equals couponissuedetail_.CouponIssueMasterId
                                join coupomredeem_ in context.Redeemtions on couponissuedetail_.CouponIssueDetailsId equals coupomredeem_.CouponIssuedetailsId
                               join customer_ in context.Customers on coupomredeem_.RedeembyCustomerId equals customer_.CustomerId
                                where coupon_.CouponId == couponSearchPerformanceModel.CampaignsId
                                select new CustomerCouponModel
                                {
                                    CouponTitle = coupon_.CouponTitle,
                                    CouponDiscerption = coupon_.CouponDiscerption,
                                    Status = coupon_.Status,
                                    DiscountType = coupon_.DiscountType,
                                    DiscountValue = coupon_.DiscountValue,
                                    StartDate = coupon_.StartDate,
                                    EndDate = coupon_.EndDate,
                                    CouponCode = couponissuedetail_.CouponSerialNo,
                                    MerchantDetail = merchant.OrganizationName + state_.StateName,
                                    RedeemDate = coupomredeem_.CouponredeemtionDate,
                                    RedeemName =  string.IsNullOrEmpty(customer_.Name) ? "User - " + CommonUtility.maskString(Encoding.UTF8.GetString(CommonUtility.Decrypt(customer_.PhoneNumber, configuration.SymmetricKey)), "PhoneNUmber") : (customer_.Name + (string.IsNullOrEmpty(customer_.Lastname) ? "" : " " + customer_.Lastname)),

                                }).Skip(skip)
                .Take(pageSize)
                .ToList(); ; ;

            return customerDAta;


        }
        private bool SendSms(string phonenumber, string message)
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
                        //ToNumber =  phonenumber;
                        ToNumber = configuration.MobileCountryCode + phonenumber;
                        var message1 = MessageResource.Create(
                            new PhoneNumber(ToNumber),
                            from: new PhoneNumber(configuration.FromMobileNumber),
                            body: message
                        );
                        Console.WriteLine(message1.Sid);
                        Errorlog errorlog = new Errorlog();
                        errorlog.ErrorLog1 = "Send Sms to " + ToNumber + "  And SID ID" + message1.Sid;
                        errorlog.LogDate = DateTime.Now;
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
                }
            }
            return result;
        }
        public string NudgeCoupon(NudgeModel nudgeCouponModel, ref ErrorResponseModel errorResponseModel)
        {
            throw new NotImplementedException();
        }

        public MerchantWiseCustomerCount GetCountOfCustomerMerchantWise(MerchantCustList merchantCustList)
        {
           
            var pageNumber = (merchantCustList.PageNumber <= 0) ? 1 : merchantCustList.PageNumber;
            var pageSize = 10;
            var totalRecords = 0.0;
            var totalPages = 0.0;
            var skip = 0;

            int posId = (from m in context.Pos
                         where m.MerchantId == merchantCustList.MerchantId
                         select m.Posid).FirstOrDefault(); 


            MerchantWiseCustomerCount merchantWiseCustomerCount = new MerchantWiseCustomerCount();

            merchantWiseCustomerCount.NewCustomer = context.Customermerchantmappers
                      .Where(mapper => mapper.MerchantId == merchantCustList.MerchantId).Count();


            var test =
                from redemption in context.Redeemtions
                join pos in context.Pos on redemption.PosId equals pos.Posid
                join merchant in context.Merchants on pos.MerchantId equals merchant.MerchantId
                where merchant.MerchantId == merchantCustList.MerchantId
                group redemption by redemption.RedeembyCustomerId into grouped

                select new
                {
                    CustomerID = grouped.Key,
                    TotalRedemptions = grouped.Count(),
                };

         

            merchantWiseCustomerCount.CustomerVisit = (from customer_ in context.Customers
                                 join visit in test on customer_.CustomerId equals visit.CustomerID
                                 select customer_.CustomerId
                       ).Distinct().Count();

            merchantWiseCustomerCount.FrequentCustomer = ( from customer_ in context.Customers
                            join visit in test on customer_.CustomerId equals visit.CustomerID
                            where visit.TotalRedemptions > 1
                            select customer_.CustomerId
                        ).Distinct().Count();

          

            #region This code will be add in Percentage


            DateTime lastMonthStart = DateTime.Now.Date.AddDays(-30);  // Get the start date of last week
            DateTime lastMonthEnd = DateTime.Now.Date.AddDays(1);

            DateTime PriviousMonthAgoStart = DateTime.Now.Date.AddDays(-31 - 30);

            int totalnew = (from c in context.Customers
                            join cm in context.Customermerchantmappers on c.CustomerId equals cm.CustomerId
                            where c.CreatedDate >= lastMonthStart
                            && c.CreatedDate < lastMonthEnd && cm.MerchantId == merchantCustList.MerchantId
                            select cm).Count();


            int totalNewCustomers = context.Customermerchantmappers
             .Count(mapper => mapper.MerchantId == merchantCustList.MerchantId &&
                              mapper.Customer.CreatedDate.Value.Date >= lastMonthStart &&
                              mapper.Customer.CreatedDate.Value.Date < lastMonthEnd);


          


            int totalCustomerVisits = (from customer_ in context.Customers
                                       join visit in test on customer_.CustomerId equals visit.CustomerID
                                       where customer_.CreatedDate.Value.Date >= lastMonthStart && customer_.CreatedDate.Value.Date < lastMonthEnd
                                       select customer_.CustomerId
                                       ).Distinct().Count();

            int totalFrequentCustomers = (from customer_ in context.Customers
                                          join visit in test on customer_.CustomerId equals visit.CustomerID
                                          where customer_.CreatedDate.Value.Date >= lastMonthStart && customer_.CreatedDate.Value.Date < lastMonthEnd
                                              && visit.TotalRedemptions > 1
                                          select customer_.CustomerId
                                          ).Distinct().Count();
        

            //---------------------temp hide new percentage requirement------------------   

            int PreviousMonthCustomersCount = (from customer_ in context.Customers
                                               join visit in context.Customermerchantmappers on customer_.CustomerId equals visit.CustomerId
                                               where customer_.CreatedDate.Value.Date >= PriviousMonthAgoStart && customer_.CreatedDate.Value.Date < lastMonthStart && visit.MerchantId == merchantCustList.MerchantId
                                               select customer_.CustomerId
                                      ).Distinct().Count();


            int PreviousMonthCustomerVisits = (from customer_ in context.Customers
                                               join visit in test on customer_.CustomerId equals visit.CustomerID
                                               where customer_.CreatedDate.Value.Date >= PriviousMonthAgoStart && customer_.CreatedDate.Value.Date < lastMonthStart
                                               select customer_.CustomerId
                                      ).Distinct().Count();

            int PreviousMonthFrequentCustomers = (from customer_ in context.Customers
                                                  join visit in test on customer_.CustomerId equals visit.CustomerID
                                                  where customer_.CreatedDate.Value.Date >= PriviousMonthAgoStart && customer_.CreatedDate.Value.Date < lastMonthStart
                                                      && visit.TotalRedemptions > 1
                                                  select customer_.CustomerId
                                          ).Distinct().Count();


            merchantWiseCustomerCount.percentageNewCustomers =  totalNewCustomers > 0 && PreviousMonthCustomersCount > 0
                                                                ? (totalNewCustomers * 100 / (double)PreviousMonthCustomersCount)
                                                                : totalNewCustomers > 0 && PreviousMonthCustomersCount == 0
                                                                ? (totalNewCustomers * 100)
                                                                : totalNewCustomers == 0 && PreviousMonthCustomersCount > 0 
                                                                ? ((PreviousMonthCustomersCount * 100) - 1):0;


            merchantWiseCustomerCount.percentageCustomerVisits = totalCustomerVisits == 0 && PreviousMonthCustomerVisits > 0
                                                                 ? ((PreviousMonthCustomerVisits * 100) - 1)
                                                                 : totalCustomerVisits > 0 && PreviousMonthCustomerVisits > 0
                                                                 ? (totalCustomerVisits * 100 / (double)PreviousMonthCustomerVisits)
                                                                     : totalCustomerVisits > 0 && PreviousMonthCustomerVisits == 0
                                                                         ? (totalCustomerVisits * 100)
                                                                         : 0;


            merchantWiseCustomerCount.percentageFrequentCustomers = totalFrequentCustomers == 0 && PreviousMonthFrequentCustomers > 0
                                                                  ? ((PreviousMonthFrequentCustomers * 100) - 1)
                                                                  : totalFrequentCustomers > 0 && PreviousMonthFrequentCustomers > 0
                                                                  ? (totalFrequentCustomers * 100 / (double)PreviousMonthFrequentCustomers)
                                                                      : totalFrequentCustomers > 0 && PreviousMonthFrequentCustomers == 0
                                                                          ? (totalFrequentCustomers * 100)
                                                                          : 0;
           

            var refferal =  context.Adminusergraphs
                                        .Where(item => item.MerchantId == merchantCustList.MerchantId)
                                        .Sum(item => item.ReferralCount);

            merchantWiseCustomerCount.RefferalCustomer = Convert.ToInt32(refferal);

            var WalkInCustomer = context.Adminusergraphs
                                        .Where(item => item.MerchantId == merchantCustList.MerchantId)
                                        .Sum(item => item.WalkInCount);

            merchantWiseCustomerCount.WalkInCustomer = Convert.ToInt32(WalkInCustomer);

            #endregion

            if (merchantCustList.Keyword == "NewCustomer")
            {
                if (!String.IsNullOrEmpty(merchantCustList.Name))
                {
                    totalRecords = (from customer_ in context.Customers
                                    join cmmaaper in context.Customermerchantmappers on customer_.CustomerId equals cmmaaper.CustomerId
                                    where cmmaaper.MerchantId == merchantCustList.MerchantId  &&
                                    customer_.Name == merchantCustList.Name
                                    select customer_).ToList().Count;
                    totalPages = Math.Ceiling((double)totalRecords / pageSize);
                    skip = (pageNumber - 1) * pageSize;
                }
                else
                {
                    totalRecords = context.Customermerchantmappers.Count(x => x.MerchantId == merchantCustList.MerchantId);
                    totalPages = Math.Ceiling((double)totalRecords / pageSize);
                    skip = (pageNumber - 1) * pageSize;               }

               

                var CustomerData = (from customer_ in context.Customers
                                    join cmmaaper in context.Customermerchantmappers on customer_.CustomerId equals cmmaaper.CustomerId
                                    where cmmaaper.MerchantId == merchantCustList.MerchantId 
                                          //&& customer_.Name == merchantCustList.Name
                                    let rewardPointsSum = (from rewardpointtransaction_ in context.Rewardpointtransactions.AsEnumerable()
                                                           join rewardpointmaster_ in context.Rewardpointmasters on rewardpointtransaction_.RewardPointId equals rewardpointmaster_.RewardPonitId
                                                        //  join merchant_ in context.Merchants on rewardpointtransaction_.MerchantId equals merchant_.MerchantId
                                                           join Customer_ in context.Customers on rewardpointtransaction_.CustomerId equals Customer_.CustomerId
                                                           where Customer_.PhoneNumber == customer_.PhoneNumber &&  //rewardpointtransaction_.MerchantId == merchantCustList.MerchantId &&
                                                                 (rewardpointmaster_.RewardTypeId == 1 || rewardpointmaster_.RewardTypeId == 2 || rewardpointmaster_.RewardTypeId == 3)
                                                           select rewardpointmaster_.RewardTypeId != 3 ? rewardpointmaster_.RewardPoint : -rewardpointmaster_.RewardPoint)
                                                           .Sum()
                                    select new CustomerSearchResultModel
                                    {
                                        CustomerID = customer_.CustomerId,
                                        Name = string.IsNullOrEmpty(customer_.Name.Trim() ) ? "User - " + CommonUtility.maskString(Encoding.UTF8.GetString(CommonUtility.Decrypt(customer_.PhoneNumber, configuration.SymmetricKey)), "PhoneNUmber") : (customer_.Name.Trim() + (string.IsNullOrEmpty(customer_.Lastname.Trim()) ? "" : " " + customer_.Lastname.Trim())),
                                        PhoneNumber = Encoding.UTF8.GetString(CommonUtility.Decrypt(customer_.PhoneNumber, configuration.SymmetricKey)),
                                        CustomerCode = customer_.CustomerCode,
                                        Trustscore = context.Trustscores.Select(ts => ts.TrustScore1).FirstOrDefault(),
                                        isSelected = false,
                                        isVisibleMenu = false,
                                        Level = rewardPointsSum >= 1000 ? "Platinum" :
                                                rewardPointsSum >= 500 ? "Gold" :
                                                rewardPointsSum >= 0 ? "Silver" : "",
                                        RecStatus = customer_.RecStatus,

                                        NoOfVisit = (from redeem_ in context.Redeemtions
                                                     where redeem_.RedeembyCustomerId == customer_.CustomerId && redeem_.PosId == posId
                                                     select redeem_).Count(),

                                        LastVisited = (from redeem_ in context.Redeemtions
                                                       where redeem_.RedeembyCustomerId == customer_.CustomerId && redeem_.PosId == posId
                                                       orderby redeem_.CouponredeemtionDate descending // Order by redemption date in descending order
                                                       select redeem_.CouponredeemtionDate.ToString("yyyy-MM-dd"))
                                                    .FirstOrDefault() ?? "",

                                         BlotuskPoint = rewardPointsSum,
                                       PageCount = totalPages,
                                       CreatedDate = customer_.CreatedDate,
                                    })
                      .ToList();



                skip = (pageNumber - 1) * pageSize;

                if (!String.IsNullOrEmpty(merchantCustList.Name) || !String.IsNullOrEmpty(merchantCustList.Recent))
                {
                   

                    if (!String.IsNullOrEmpty(merchantCustList.Recent))
                    {
                        if(merchantCustList.Recent == "Recent")
                        {
                            var sortedCouponList = CustomerData.OrderByDescending(customer => customer.CreatedDate).ToList(); 
                                //.Skip(skip)
                                //.Take(pageSize)
                                //.ToList();

                            CustomerData = sortedCouponList;

                        }
                        else if(merchantCustList.Recent == "Alphabatical")
                        {
                            var sortedCouponList = CustomerData.OrderBy(customer => customer.Name).ToList();
                                  //.Skip(skip)
                                  //.Take(pageSize)
                                  //.ToList();

                            CustomerData  = sortedCouponList;
                        }

                      
                    }

                    else if (!String.IsNullOrEmpty(merchantCustList.Name))
                    {
                        var searchedCustomers = CustomerData
                         .Where(customer => !string.IsNullOrEmpty(merchantCustList.Name) && customer.Name.Contains(merchantCustList.Name))
                         .Skip(skip)
                         .Take(pageSize)
                         .ToList();

                        CustomerData = searchedCustomers;


                        // Now 'searchedCustomers' contains the list of customers with the specified name

                    }

                    else
                    {
                        skip = (pageNumber - 1) * pageSize;
                        CustomerData // Order by CreatedDate in descending order
                      .Skip(skip)
                      .Take(pageSize)
                      .ToList();
                    }
                    merchantWiseCustomerCount.CustomerList = CustomerData.Skip(skip)
                      .Take(pageSize)
                      .ToList(); ;
                }
                else
                {
                    merchantWiseCustomerCount.CustomerList = CustomerData.Skip(skip)
                      .Take(pageSize)
                      .ToList();
                }
            }
    
            else if (merchantCustList.Keyword == "CustomerVisit")
            {
                
                skip = (pageNumber - 1) * pageSize;

                var RedeemVisit =
                    from redemption in context.Redeemtions
                    join pos in context.Pos on redemption.PosId equals pos.Posid
                    join merchant in context.Merchants on pos.MerchantId equals merchant.MerchantId
                    where merchant.MerchantId == merchantCustList.MerchantId
                    group redemption by redemption.RedeembyCustomerId into grouped
                    select new
                    {
                        CustomerID = grouped.Key,
                        TotalRedemptions = grouped.Count(),
                    };

                var customerData =
                    from customer_ in context.Customers
                    join visit in RedeemVisit on customer_.CustomerId equals visit.CustomerID
                    // where visit.TotalRedemptions > 1

                    let rewardPointsSum = (from rewardpointtransaction_ in context.Rewardpointtransactions.AsEnumerable()
                                           join rewardpointmaster_ in context.Rewardpointmasters on rewardpointtransaction_.RewardPointId equals rewardpointmaster_.RewardPonitId
                                           join Customer_ in context.Customers on rewardpointtransaction_.CustomerId equals Customer_.CustomerId
                                           where Customer_.PhoneNumber == customer_.PhoneNumber && //rewardpointtransaction_.MerchantId == merchantCustList.MerchantId &&
                                                 (rewardpointmaster_.RewardTypeId == 1 || rewardpointmaster_.RewardTypeId == 2 || rewardpointmaster_.RewardTypeId == 3)
                                           select rewardpointmaster_.RewardTypeId != 3 ? rewardpointmaster_.RewardPoint : -rewardpointmaster_.RewardPoint)
                                                       .Sum()
                    select new CustomerSearchResultModel
                    {
                        CustomerID = customer_.CustomerId,
                        Name = string.IsNullOrEmpty(customer_.Name.Trim()) ? "User - " + CommonUtility.maskString(Encoding.UTF8.GetString(CommonUtility.Decrypt(customer_.PhoneNumber, configuration.SymmetricKey)), "PhoneNUmber") : (customer_.Name.Trim() + (string.IsNullOrEmpty(customer_.Lastname.Trim()) ? "" : " " + customer_.Lastname)),
                        PhoneNumber = Encoding.UTF8.GetString(CommonUtility.Decrypt(customer_.PhoneNumber, configuration.SymmetricKey)),
                        CustomerCode = customer_.CustomerCode,
                        Trustscore = context.Trustscores.Select(ts => ts.TrustScore1).FirstOrDefault(),
                        isSelected = false,
                        isVisibleMenu = false,
                        Level = rewardPointsSum >= 1000 ? "Platinum" :
                                                rewardPointsSum >= 500 ? "Gold" :
                                                rewardPointsSum >= 0 ? "Silver" : "",
                        NoOfVisit = (from redeem_ in context.Redeemtions
                                     where redeem_.RedeembyCustomerId == customer_.CustomerId && redeem_.PosId == posId
                                     select redeem_).Count(),

                        //LastVisited = (from redeem_ in context.Redeemtions
                        //               where redeem_.RedeembyCustomerId == customer_.CustomerId
                        //               select redeem_.CouponredeemtionDate.ToString("yyyy-MM-dd")).FirstOrDefault() ?? "",


                        LastVisited = (from redeem_ in context.Redeemtions
                                       where redeem_.RedeembyCustomerId == customer_.CustomerId && redeem_.PosId == posId
                                       orderby redeem_.CouponredeemtionDate descending // Order by redemption date in descending order
                                       select redeem_.CouponredeemtionDate.ToString("yyyy-MM-dd"))
                                                    .FirstOrDefault() ?? "",

                        RecStatus = customer_.RecStatus,
                        BlotuskPoint = rewardPointsSum,
                        PageCount = totalPages
                    };

                var CustomerData = customerData.ToList().Skip(skip).Take(pageSize);



                if (!String.IsNullOrEmpty(merchantCustList.Name) || !String.IsNullOrEmpty(merchantCustList.Recent))
                {

                    if (!String.IsNullOrEmpty(merchantCustList.Recent))
                    {
                        if (merchantCustList.Recent == "Recent")
                        {                          
                            var sortedCouponList = CustomerData.OrderByDescending(customer => customer.CreatedDate).ToList();
                            //.Skip(skip)
                            //.Take(pageSize)
                            //.ToList();

                            CustomerData = sortedCouponList;

                        }
                        else if (merchantCustList.Recent == "Alphabatical")
                        {
                            var sortedCouponList = CustomerData.OrderBy(customer => customer.Name)
                                  //.Skip(skip)
                                  //.Take(pageSize)
                                  .ToList();

                            CustomerData = sortedCouponList;
                        }


                    }

                  

                    else if (!String.IsNullOrEmpty(merchantCustList.Name))
                    {
                      
                        var searchedCustomers = CustomerData
                  .Where(customer => !string.IsNullOrEmpty(merchantCustList.Name) && customer.Name.Contains(merchantCustList.Name))
                  .Skip(skip)
                  .Take(pageSize)
                  .ToList();

                        CustomerData = searchedCustomers;
                    }

                    else
                    {
                        CustomerData // Order by CreatedDate in descending order
                      .Skip(skip)
                      .Take(pageSize)
                      .ToList();
                    }

                    merchantWiseCustomerCount.CustomerList = CustomerData.ToList();
                }
                else
                {
                    merchantWiseCustomerCount.CustomerList = CustomerData.ToList();
                }
            }

            else if (merchantCustList.Keyword == "FrequentCustomer")
            {          


                var totalRedemptionQuerys =
                                            from redemption in context.Redeemtions
                                            join pos in context.Pos on redemption.PosId equals pos.Posid
                                            join merchant in context.Merchants on pos.MerchantId equals merchant.MerchantId
                                            where merchant.MerchantId == merchantCustList.MerchantId
                                            group redemption by redemption.RedeembyCustomerId into grouped
                                            select new
                                            {
                                                CustomerID = grouped.Key,
                                                TotalRedemptions = grouped.Count(),
                                            };

                totalRecords = totalRedemptionQuerys.ToList().Count;
                totalPages = Math.Ceiling((double)totalRecords / pageSize);
                skip = (pageNumber - 1) * pageSize;

               var customerVisits =
                 from redemption in context.Redeemtions
                 join pos in context.Pos on redemption.PosId equals pos.Posid
                 join merchant in context.Merchants on pos.MerchantId equals merchant.MerchantId
                 where merchant.MerchantId == merchantCustList.MerchantId
                 group redemption by redemption.RedeembyCustomerId into grouped

                 select new
                 {
                     CustomerID = grouped.Key,
                     TotalRedemptions = grouped.Count(),
                 };

                var customerData =
                    from customer_ in context.Customers
                    join visit in customerVisits on customer_.CustomerId equals visit.CustomerID
                    where visit.TotalRedemptions > 1

                    let rewardPointsSum = (from rewardpointtransaction_ in context.Rewardpointtransactions.AsEnumerable()
                                           join rewardpointmaster_ in context.Rewardpointmasters on rewardpointtransaction_.RewardPointId equals rewardpointmaster_.RewardPonitId
                                           join Customer_ in context.Customers on rewardpointtransaction_.CustomerId equals Customer_.CustomerId
                                           where Customer_.PhoneNumber == customer_.PhoneNumber && //rewardpointtransaction_.MerchantId == merchantCustList.MerchantId &&
                                                 (rewardpointmaster_.RewardTypeId == 1 || rewardpointmaster_.RewardTypeId == 2 || rewardpointmaster_.RewardTypeId == 3)
                                           select rewardpointmaster_.RewardTypeId != 3 ? rewardpointmaster_.RewardPoint : -rewardpointmaster_.RewardPoint)
                                                    .Sum()
                    select new CustomerSearchResultModel
                    {
                        CustomerID = customer_.CustomerId,
                        Name = string.IsNullOrEmpty(customer_.Name.Trim()) ? "User - " + CommonUtility.maskString(Encoding.UTF8.GetString(CommonUtility.Decrypt(customer_.PhoneNumber, configuration.SymmetricKey)), "PhoneNUmber") : (customer_.Name.Trim() + (string.IsNullOrEmpty(customer_.Lastname.Trim()) ? "" : " " + customer_.Lastname.Trim()))    ,
                        PhoneNumber = Encoding.UTF8.GetString(CommonUtility.Decrypt(customer_.PhoneNumber, configuration.SymmetricKey)),
                        CustomerCode = customer_.CustomerCode,
                        Trustscore = context.Trustscores.Select(ts => ts.TrustScore1).FirstOrDefault() ?? 0,
                        isSelected = false,
                        isVisibleMenu = false,
                        Level = rewardPointsSum >= 1000 ? "Platinum" :
                                                rewardPointsSum >= 500 ? "Gold" :
                                                rewardPointsSum >= 0 ? "Silver" : "",
                        NoOfVisit = (from redeem_ in context.Redeemtions
                                     where redeem_.RedeembyCustomerId == customer_.CustomerId && redeem_.PosId == posId
                                     select redeem_).Count(),

                        LastVisited = (from redeem_ in context.Redeemtions
                                       where redeem_.RedeembyCustomerId == customer_.CustomerId && redeem_.PosId == posId
                                       orderby redeem_.CouponredeemtionDate descending // Order by redemption date in descending order
                                       select redeem_.CouponredeemtionDate.ToString("yyyy-MM-dd"))
                                                    .FirstOrDefault() ?? "",
                        BlotuskPoint = rewardPointsSum,
                        RecStatus = customer_.RecStatus,

                        PageCount = totalPages
                    };
                skip = (pageNumber - 1) * pageSize;
                var CustomerData = customerData.ToList().Skip(skip).Take(pageSize);


                if (!String.IsNullOrEmpty(merchantCustList.Name) || !String.IsNullOrEmpty(merchantCustList.Recent))
                {
                    if (!String.IsNullOrEmpty(merchantCustList.Recent))
                    {
                        if (merchantCustList.Recent == "Recent")
                        {
                           
                            var sortedCouponList = CustomerData.OrderByDescending(customer => customer.CreatedDate).ToList();
                            //.Skip(skip)
                            //.Take(pageSize)
                            //.ToList();

                            CustomerData = sortedCouponList;

                        }
                        else if (merchantCustList.Recent == "Alphabatical")
                        {
                            var sortedCouponList = CustomerData.OrderBy(customer => customer.Name)
                                  //.Skip(skip)
                                  //.Take(pageSize)
                                  .ToList();

                            CustomerData = sortedCouponList;
                        }


                    }
                    else if (!String.IsNullOrEmpty(merchantCustList.Name))
                    {
                      //  CustomerData
                      //.Where(customer => customer.Name.Contains(merchantCustList.Name))
                      //.Skip(skip)
                      //.Take(pageSize)
                      //.ToList();

                        var searchedCustomers = CustomerData
                  .Where(customer => !string.IsNullOrEmpty(merchantCustList.Name) && customer.Name.Contains(merchantCustList.Name))
                  .Skip(skip)
                  .Take(pageSize)
                  .ToList();


                        CustomerData = searchedCustomers;
                    }
                    else
                    {
                        CustomerData // Order by CreatedDate in descending order
                      .Skip(skip)
                      .Take(pageSize)
                      .ToList();

                    }
                    merchantWiseCustomerCount.CustomerList = CustomerData.ToList();
                }
                else
                {
                    merchantWiseCustomerCount.CustomerList = CustomerData.ToList();
                }
            }

            return merchantWiseCustomerCount;

        }    

        public int? GetVisit(int CustomerID)
        {
             
            var redemptionCount = (from redeem_ in context.Redeemtions
                                   where redeem_.RedeembyCustomerId == CustomerID
                                   select redeem_).Count();

            return redemptionCount;
        }
        public int GetBlootuskPoint(int CustomerID)
        {
            var rewardSum = (from rewardpointtransaction_ in context.Rewardpointtransactions
                             join rewardpointmaster_ in context.Rewardpointmasters on rewardpointtransaction_.RewardPointId equals rewardpointmaster_.RewardPonitId
                             join Customer_ in context.Customers on rewardpointtransaction_.CustomerId equals Customer_.CustomerId
                             where Customer_.CustomerId == CustomerID
                             select new
                             {
                                 rewardpointmaster_.RewardPoint
                             }).ToList();


            var rewardSumminus = (from rewardpointtransaction_ in context.Rewardpointtransactions
                                  join rewardpointmaster_ in context.Rewardpointmasters on rewardpointtransaction_.RewardPointId equals rewardpointmaster_.RewardPonitId
                                  join Customer_ in context.Customers on rewardpointtransaction_.CustomerId equals Customer_.CustomerId
                                  where rewardpointmaster_.RewardTypeId == 3 && Customer_.CustomerId == CustomerID
                                  select new
                                  {
                                      rewardpointmaster_.RewardPoint
                                  }).ToList();


            return Convert.ToInt32(rewardSum.Sum(x => x.RewardPoint) - rewardSumminus.Sum(x => x.RewardPoint));

        }

        public MerchantWiseDashboardCount GetCountOfDasboardMerchantWise(DashboardValueList dashboardValueList)
        {

            MerchantWiseDashboardCount merchantWiseDashboardCount = new MerchantWiseDashboardCount();

            merchantWiseDashboardCount.RewardIssued = context.Rewardpointmasters.Join(
                                                              context.Rewardpointtransactions,
                                                              rewardmaster_ => rewardmaster_.RewardPonitId,
                                                              rewardPointTransaction_ => rewardPointTransaction_.RewardPointId,
                                                              (rewardmaster_, rewardPointTransaction_) => new { rewardmaster_, rewardPointTransaction_ }
                                                          ).Where(joined =>
                                                              joined.rewardPointTransaction_.MerchantId == dashboardValueList.MerchantId &&
                                                              joined.rewardmaster_.RewardTypeId == 2
                                                          ).Select(joined => joined.rewardmaster_.RewardPoint).Sum();

            //        var rewardIssuedSum = context.Rewardpointmasters
            //.Join(
            //    context.Rewardpointtransactions,
            //    rewardmaster_ => rewardmaster_.RewardPonitId,
            //    rewardPointTransaction_ => rewardPointTransaction_.RewardPointId,
            //    (rewardmaster_, rewardPointTransaction_) => new { rewardmaster_, rewardPointTransaction_ }
            //)
            //.Where(joined =>
            //    joined.rewardPointTransaction_.MerchantId == dashboardValueList.MerchantId &&
            //    joined.rewardmaster_.RewardTypeId == 2
            //)
            //.Select(joined => joined.rewardmaster_.RewardPoint)
            //.Sum();

            //        merchantWiseDashboardCount.RewardIssued = rewardIssuedSum;



            merchantWiseDashboardCount.RewardRedeem = context.Rewardpointmasters.Join(
                                                              context.Rewardpointtransactions,
                                                              rewardmaster_ => rewardmaster_.RewardPonitId,
                                                              rewardPointTransaction_ => rewardPointTransaction_.RewardPointId,
                                                              (rewardmaster_, rewardPointTransaction_) => new { rewardmaster_, rewardPointTransaction_ }
                                                          ).Where(joined =>
                                                              joined.rewardPointTransaction_.MerchantId == dashboardValueList.MerchantId &&
                                                              joined.rewardmaster_.RewardTypeId == 3
                                                          ).Select(joined => joined.rewardmaster_.RewardPoint).Sum();


            var CampaginRedeem = (
                 from redemtion_ in context.Redeemtions.AsNoTracking()
                 join Couponissuedetails_ in context.Couponissuedetails on redemtion_.CouponIssuedetailsId equals Couponissuedetails_.CouponIssueDetailsId
                 join couponissuemaster_ in context.Couponissuemasters on Couponissuedetails_.CouponIssueMasterId equals couponissuemaster_.CouponIssuemasterId
                 join CouponMaster_ in context.Couponmasters on couponissuemaster_.CouponId equals CouponMaster_.CouponId
                 where CouponMaster_.MerchantId == dashboardValueList.MerchantId
                 select redemtion_
             );

            merchantWiseDashboardCount.CampaginRedeem = CampaginRedeem.Count();

            merchantWiseDashboardCount.CampaginIssued = context.Couponmasters
                        .Where(coupon => coupon.MerchantId == dashboardValueList.MerchantId)
                        .Join(context.Couponissuemasters, coupon => coupon.CouponId, issueMaster => issueMaster.CouponId, (coupon, issueMaster) => issueMaster)
                        .Join(context.Couponissuedetails, issueMaster => issueMaster.CouponIssuemasterId, issueDetail => issueDetail.CouponIssueMasterId, (issueMaster, issueDetail) => issueDetail)
                        .Count();

            return merchantWiseDashboardCount;
        }

        public string SpecialOffer(CouponSpacialModel CouponMasters, ref ErrorResponseModel errorResponseModel)
        {
            try
            {
                string statusCode = "";
                bool result = false;
                Couponmaster Couponmaster = new Couponmaster();
                Couponissuemaster couponissuemaster = new Couponissuemaster();

                if (CouponMasters.CouponId != 0)
                {
                    var CouponMastersUpdate = context.Couponmasters.FirstOrDefault(c => c.CouponId == CouponMasters.CouponId);

                    if (CouponMastersUpdate != null)
                    {
                        Couponmaster.NoOfCoupon = 1 + CouponMastersUpdate.NoOfCoupon;
                        Couponmaster.ModifyDate = DateTime.Now;
                        context.SaveChanges();
                    }
                    var couponissuemasterUpdate = context.Couponissuemasters.FirstOrDefault(c => c.CouponId == CouponMasters.CouponId);
                    if (couponissuemasterUpdate != null)
                    {
                        couponissuemaster.IssuedQty = couponissuemasterUpdate.IssuedQty + 1;
                        context.SaveChanges();
                    }

                    List<Couponissuedetail> couponIssueDetailsList = new List<Couponissuedetail>();
                   

                    var couponissuedetail = context.Couponissuedetails
                          .FirstOrDefault(cd => cd.CouponId == CouponMasters.CouponId && cd.CustomerId == 0);

                    if (couponissuedetail != null)
                    {
                        // Update the necessary fields
                        couponissuedetail.CustomerId = CouponMasters.CustomerId;
                        context.SaveChanges();

                        // sending SMS

                        var MerchantRefferal = context.Smstemplates
                                     .Where(template => template.MerchantId == CouponMasters.MerchantId && template.MessageTypeId == 5)
                                     .Select(template => new
                                     {
                                         messagecontent = template.MessageContent,
                                     })
                                     .FirstOrDefault();

                       

                        string signupmessage = MerchantRefferal.messagecontent;
                        var merchant = context.Merchants
                                  .Where(merchant => merchant.MerchantId == CouponMasters.MerchantId)
                                  .Join(context.Pos, merchant => merchant.MerchantId, pos => pos.MerchantId, (merchant, pos) => new
                                  {
                                      MerchantName = merchant.OrganizationName + " " + pos.Posname,
                                      MerchantCode = merchant.MerchantCode
                                  })
                                  .FirstOrDefault();

                        string Signupresults = signupmessage;
                        string Signuppattern = @"\[MerchantName\]";
                        string Signupreplace = merchant.MerchantName;
                        signupmessage = Regex.Replace(Signupresults, Signuppattern, Signupreplace);
                        //   string message = CouponMasters.CouponTitle + "  " + CouponMasters.CouponDiscerption;

                        if (configuration.MessageLimit > 1)
                        {
                           

                            var decryptedPhoneNumber = (from c in context.Customers
                                                        join cm in context.Customermerchantmappers on c.CustomerId equals cm.CustomerId
                                                        where c.CustomerId == CouponMasters.CustomerId && cm.StopMessage == false && cm.MerchantId == CouponMasters.MerchantId
                                                        select Encoding.UTF8.GetString(CommonUtility.Decrypt(c.PhoneNumber, configuration.SymmetricKey))).FirstOrDefault();
                           

                            if (decryptedPhoneNumber !=  null )
                            {
                                SendSms(decryptedPhoneNumber, signupmessage);
                            }

                            string logDirectory = "Log_files/";

                            if (!Directory.Exists(logDirectory))
                            {
                                Directory.CreateDirectory(logDirectory);
                            }

                            string logFileName = "SpecialOffer_" + DateTime.Now.ToString("yyyy-MM-dd-HHmmss");
                            string message = signupmessage + ", Count: " + 1 + ", Numbers: " + decryptedPhoneNumber;
                            string logFilePath = Path.Combine(logDirectory, logFileName + ".txt");
                            File.AppendAllText(logFilePath, $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}");

                            var serverPath = "Log_files/" + logFileName + ".txt";
                            var pathToSave = Path.Combine(Directory.GetCurrentDirectory(), serverPath);

                        }


                    }


                    result = true;
                    statusCode = "A";
                }


                return statusCode;
            }
            catch (Exception ex)
            {
                throw;
            }

        }
       

        public string IssuedCoupon(CouponSpacialModel CouponMasters, ref ErrorResponseModel errorResponseModel)
        {
            string statusCode = "";
            bool result = false;
            Couponmaster Couponmaster = new Couponmaster();
            Couponissuemaster couponissuemaster = new Couponissuemaster();
            try
            {
                if (CouponMasters.CouponId != 0)
                {
                    var CouponMastersUpdate = context.Couponmasters.FirstOrDefault(c => c.CouponId == CouponMasters.CouponId);
                  
                    var couponissuemasterUpdate = context.Couponissuemasters.FirstOrDefault(c => c.CouponId == CouponMasters.CouponId);                  

                    string userTypes = CouponMasters.SelectedUser;
                    string[] userTypeArray = userTypes.Split(',');
                    string userGoldLevel = "";
                    string userplatinumLevel = "";
                    string usersilverLevel = "";


                    foreach (string type in userTypeArray)
                    {
                        string[] selectedusertype = type.Split('-');

                        switch (selectedusertype[0].ToLower())
                        {
                            //gold-1,silver-2,platinum-3
                            case "gold":   
                               userGoldLevel = "gold";
                                break;
                            //doto

                            case "platinum":
                                userplatinumLevel = "platinum";
                                //doto
                                break;

                            case "silver":
                                usersilverLevel = "silver";
                                break;
                        }
                    }

                    List<int> CustomerIds = new List<int>();

                    var groupedCustomerList = (from customer in context.Customers
                                               join customermerchantmapper in context.Customermerchantmappers on customer.CustomerId equals customermerchantmapper.CustomerId
                                               join merchant in context.Merchants on customermerchantmapper.MerchantId equals merchant.MerchantId
                                               join rewardtransaction in context.Rewardpointtransactions on customer.CustomerId equals rewardtransaction.CustomerId into rewardTransactionsGroup
                                               from rewardtransaction in rewardTransactionsGroup.DefaultIfEmpty()
                                               join rewardmaster in context.Rewardpointmasters on rewardtransaction.RewardPointId equals rewardmaster.RewardPonitId into rewardMastersGroup
                                               from rewardmaster in rewardMastersGroup.DefaultIfEmpty()
                                               where 
                                          //this code will be hide by somnath sir Regarding globaly points
                                                 merchant.MerchantId == CouponMasters.MerchantId &&
                                               customer.RecStatus =="A"
                                               group new { customer, rewardmaster } by new { customer.CustomerId, customer.Name } into groupedData
                                               select new
                                               {
                                                   CustomerId = groupedData.Key.CustomerId,
                                                   RewardPoint = (groupedData.Where(item => (item.rewardmaster.RewardTypeId == 1 || item.rewardmaster.RewardTypeId == 2)).Sum(item => item.rewardmaster.RewardPoint)) - (groupedData.Where(item => (item.rewardmaster.RewardTypeId == 3)).Sum(item => item.rewardmaster.RewardPoint)),
                                               }).ToList();

                    var levelpoint = (from levelmaster_ in context.Levelmasters
                                      where levelmaster_.RecStatus == 1
                                      select new
                                      {
                                          level = levelmaster_.Level,
                                          RewardPoint = levelmaster_.RequirePoint,
                                      }).Distinct().ToList();


                    //this is code of alreday assign the coupons 

                    var results = (from couponIssueDetail in context.Couponissuedetails
                                   where couponIssueDetail.CouponId == CouponMasters.CouponId
                                   select new
                                   {
                                       IssuedCustomerId = couponIssueDetail.CustomerId,
                                   });
                    
                    var excludedCustomerIds = results.Select(r => r.IssuedCustomerId).ToList();

                    if (userGoldLevel != "")
                    {
                        var customerGoldIds = groupedCustomerList.Where(c => c.RewardPoint < levelpoint[0].RewardPoint 
                        && c.RewardPoint >= levelpoint[1].RewardPoint).Select(c => c.CustomerId).Except(excludedCustomerIds).ToList();
                        foreach (var customer in customerGoldIds)
                        {
                            CustomerIds.Add(customer);
                        }
                    }
                    if (userplatinumLevel != "")
                    {

                        var customerplatinumIds = groupedCustomerList.Where(c => c.RewardPoint >= levelpoint[0].RewardPoint)
                            .Select(c => c.CustomerId).Except(excludedCustomerIds).ToList();

                        foreach (var customer in customerplatinumIds)
                        {
                            CustomerIds.Add(customer);
                        }

                    }
                    if (usersilverLevel != "")
                    {
                        var silverCustomerIds = groupedCustomerList.Where(c => c.RewardPoint < levelpoint[1].RewardPoint)
                                                 .Select(c => c.CustomerId).Except(excludedCustomerIds).ToList();

                        foreach (var customer in silverCustomerIds)
                        {
                            CustomerIds.Add(customer);
                        }
                    }

                    CustomerIds = CustomerIds;//silver1

                    List<Couponissuedetail> couponIssueDetailsList = new List<Couponissuedetail>();

                    var CouponEntity = context.Couponmasters.FirstOrDefault(x => x.CouponId == CouponMasters.CouponId);

                    if ( CouponMasters.NoOfCoupon > CouponEntity.NoOfCoupon) //10
                    {
                        for (int i = 0; i < CustomerIds.Count; i++) // 5
                        {
                            Couponissuedetail couponissuedetail = new Couponissuedetail();
                            couponissuedetail.CouponIssueMasterId = couponissuemasterUpdate.CouponIssuemasterId;
                            couponissuedetail.CouponId = CouponMasters.CouponId;
                            couponissuedetail.CustomerId = CustomerIds[i];
                            couponissuedetail.CouponSerialNo = CouponGenerator.CouponCode();
                            couponissuedetail.UsedbyId = couponissuedetail.UsedbyId;
                            couponissuedetail.RedeemId = couponissuedetail.RedeemId;
                            context.Couponissuedetails.Add(couponissuedetail);
                            context.SaveChanges();
                          
                        }
                        int? count = CouponMasters.NoOfCoupon - CustomerIds.Count - CouponEntity.NoOfCoupon; //25 -1 - 20 = 4
                        for (int i = 0; i < count; i++)
                        {
                            Couponissuedetail couponissuedetail = new Couponissuedetail();
                            couponissuedetail.CouponIssueMasterId = couponissuemasterUpdate.CouponIssuemasterId;
                            couponissuedetail.CouponId = CouponMasters.CouponId;
                            couponissuedetail.CouponSerialNo = CouponGenerator.CouponCode();
                            couponissuedetail.UsedbyId = couponissuedetail.UsedbyId;
                            couponissuedetail.RedeemId = couponissuedetail.RedeemId;
                            context.Couponissuedetails.Add(couponissuedetail);
                            context.SaveChanges();
                            // couponIssueDetailsList.Add(couponissuedetail);
                        }

                        context.SaveChanges(); // Save changes after the loop

                        if (CouponEntity != null)
                        {
                            //coupon master updated
                            CouponEntity.NoOfCoupon = CouponMasters.NoOfCoupon;
                            context.SaveChanges();
                        }

                        //sending sms
                        var MerchantRefferal = context.Smstemplates
                                      .Where(template => template.MerchantId == CouponMasters.MerchantId && template.MessageTypeId == 5)
                                      .Select(template => new
                                      {
                                          messagecontent = template.MessageContent,
                                      })
                                      .FirstOrDefault();

                        string signupmessage = MerchantRefferal.messagecontent;

                        var merchant = context.Merchants
                                    .Where(merchant => merchant.MerchantId == CouponMasters.MerchantId)
                                    .Join(context.Pos, merchant => merchant.MerchantId, pos => pos.MerchantId, (merchant, pos) => new
                                    {
                                        MerchantName = merchant.OrganizationName + " " + pos.Posname,
                                        MerchantCode = merchant.MerchantCode
                                    })
                                    .FirstOrDefault();

                        string Signupresults = signupmessage;
                        string Signuppattern = @"\[MerchantName\]";
                        string Signupreplace = merchant.MerchantName;
                        signupmessage = Regex.Replace(Signupresults, Signuppattern, Signupreplace);
                        //   string message = CouponMasters.CouponTitle + "  " + CouponMasters.CouponDiscerption;

                        if (configuration.MessageLimit > CustomerIds.Count)
                        {                          


                            var decryptedPhoneNumbers = (from c in context.Customers
                                                         join cm in context.Customermerchantmappers on c.CustomerId equals cm.CustomerId
                                                         where CustomerIds.Contains(c.CustomerId) && cm.StopMessage == false && cm.MerchantId == CouponMasters.MerchantId
                                                         select Encoding.UTF8.GetString(CommonUtility.Decrypt(c.PhoneNumber, configuration.SymmetricKey)))
                           .ToList();


                            // var decryptedPhoneNumbers = context.Customers
                            //.Where(c => CustomerIds.Contains(c.CustomerId))
                            //.Select(c => Encoding.UTF8.GetString(CommonUtility.Decrypt(c.PhoneNumber, configuration.SymmetricKey)))
                            //.ToList();

                            List<string> formattedPhoneNumbers = new List<string>();
                           
                            foreach (var phoneNumber in decryptedPhoneNumbers)
                            {
                                SendSms(Convert.ToString(phoneNumber), signupmessage);

                                formattedPhoneNumbers.Add(Convert.ToString(phoneNumber));
                            }
                            string LogPhoneNumber = string.Join(", ", formattedPhoneNumbers);

                            string logDirectory = "Log_files/";

                            if (!Directory.Exists(logDirectory))
                            {
                                Directory.CreateDirectory(logDirectory);
                            }

                            string logFileName = "IssueCoupon_" + DateTime.Now.ToString("yyyy-MM-dd-HHmmss");
                            string message = signupmessage + ", Count: " + decryptedPhoneNumbers.Count + ", Numbers: " + LogPhoneNumber;
                            string logFilePath = Path.Combine(logDirectory, logFileName + ".txt");
                            File.AppendAllText(logFilePath, $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}");

                            var serverPath = "Log_files/" + logFileName + ".txt";
                            var pathToSave = Path.Combine(Directory.GetCurrentDirectory(), serverPath);
                        }

                        result = true;
                        statusCode = "A";
                    }

                    else
                    {
                        // couponissuedetail 

                        var CouponDetails = context.Couponissuedetails
                                            .Where(couponissuedetail_ => couponissuedetail_.CouponId == CouponMasters.CouponId
                                                                      && couponissuedetail_.CustomerId == 0)
                                            .ToList();

                        for (int i = 0; i < CustomerIds.Count; i++)//5
                        {
                            var couponissuedetail = CouponDetails.FirstOrDefault(c => c.CustomerId == 0 && c.CouponId == CouponMasters.CouponId);

                            if (couponissuedetail != null)
                            {
                                // Update the necessary fields
                                couponissuedetail.CustomerId = CustomerIds[i];
                                context.SaveChanges();

                                var merchant = context.Merchants.FirstOrDefault(c => c.MerchantId == CouponMasters.MerchantId);

                                //string message = "Exclusive offer alert! Check out the latest coupon from "
                                //+ merchant.OrganizationName + " Show this message at store and enjoy " + CouponMastersUpdate.DiscountValue + CouponMastersUpdate.DiscountType + " off your purchase.BlooTusk";


                                var MerchantRefferal = context.Smstemplates
                                       .Where(template => template.MerchantId == CouponMasters.MerchantId && template.MessageTypeId == 5)
                                       .Select(template => new
                                       {
                                           messagecontent = template.MessageContent,
                                       })
                                       .FirstOrDefault();

                                string signupmessage = MerchantRefferal.messagecontent;

                                string Signupresults = signupmessage;
                                string Signuppattern = @"\[MerchantName\]";
                                string Signupreplace = merchant.OrganizationName;
                                signupmessage = Regex.Replace(Signupresults, Signuppattern, Signupreplace);
                                //   string message = CouponMasters.CouponTitle + "  " + CouponMasters.CouponDiscerption;

                                if (configuration.MessageLimit > CustomerIds.Count)
                                {
                                   

                                    var decryptedPhoneNumbers = (from c in context.Customers
                                                                 join cm in context.Customermerchantmappers on c.CustomerId equals cm.CustomerId
                                                                 where CustomerIds.Contains(c.CustomerId) && cm.StopMessage == false && cm.MerchantId == CouponMasters.MerchantId
                                                                 select Encoding.UTF8.GetString(CommonUtility.Decrypt(c.PhoneNumber, configuration.SymmetricKey)))
                        .ToList();

                                    List<string> formattedPhoneNumbers = new List<string>();
                                    foreach (var phoneNumber in decryptedPhoneNumbers)
                                    {
                                        SendSms(Convert.ToString(phoneNumber), signupmessage);

                                        formattedPhoneNumbers.Add(Convert.ToString(phoneNumber));
                                    }
                                    string LogPhoneNumber = string.Join(", ", formattedPhoneNumbers);

                                    string logDirectory = "Log_files/";

                                    if (!Directory.Exists(logDirectory))
                                    {
                                        Directory.CreateDirectory(logDirectory);
                                    }

                                    string logFileName = "Issue_" + DateTime.Now.ToString("yyyy-MM-dd-HHmmss");
                                    string message = signupmessage + ", Count: " + decryptedPhoneNumbers.Count + ", Numbers: " + LogPhoneNumber;
                                    string logFilePath = Path.Combine(logDirectory, logFileName + ".txt");
                                    File.AppendAllText(logFilePath, $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}");

                                    var serverPath = "Log_files/" + logFileName + ".txt";
                                    var pathToSave = Path.Combine(Directory.GetCurrentDirectory(), serverPath);
                                }



                            }
                        }
                    }

                    if (CouponMastersUpdate != null)
                    {
                        Couponmaster.NoOfCoupon = CouponMasters.NoOfCoupon; //  + CouponMastersUpdate.NoOfCoupon;
                        Couponmaster.ModifyDate = DateTime.Now;
                        context.SaveChanges();

                    }

                    if (couponissuemasterUpdate != null)
                    {
                        couponissuemaster.IssuedQty = Convert.ToInt32(CouponMasters.NoOfCoupon);
                        context.SaveChanges();
                    }

                    statusCode = "A";
                }
                
            }
            catch (Exception ex)
            {
                throw;
            }

            return statusCode;
        
        
        }


       


        //public ValidateCouponSearchModel GetCustomerReddemData(ValidateCouponModel validateCouponModel)
        //{
        //    ValidateCouponSearchModel validateCouponSearch = new ValidateCouponSearchModel();

        //    if (validateCouponModel.CouponCode != "")
        //    {

        //        int customerId = context.Couponissuedetails
        //                      .Where(coupon => coupon.CouponSerialNo == validateCouponModel.CouponCode)
        //                      .Select(coupon => coupon.CustomerId)
        //                      .FirstOrDefault();


        //        var customer = from coupon in context.Couponmasters
        //                       join issueMaster in context.Couponissuemasters on coupon.CouponId equals issueMaster.CouponId
        //                       join issueDetail in context.Couponissuedetails on issueMaster.CouponIssuemasterId equals issueDetail.CouponIssueMasterId
        //                       join customer_ in context.Customers on issueDetail.CustomerId equals customer_.CustomerId
        //                       join custMerchnat_ in context.Customermerchantmappers on customer_.CustomerId equals custMerchnat_.CustomerId
        //                       join Merchant_ in context.Merchants on custMerchnat_.MerchantId equals Merchant_.MerchantId
        //                       where issueDetail.CouponSerialNo == validateCouponModel.CouponCode
        //                       select new ValidateCouponSearchModel
        //                       {
        //                           phoneNumber = customer_.PhoneNumber,
        //                           CouponCode = coupon.CouponCode,
        //                           RecLink = "",
        //                           MerchantId = coupon.MerchantId,
        //                           DiscountType = coupon.DiscountType,
        //                           DiscountValue = coupon.DiscountValue,
        //                           CouponDiscerption = coupon.CouponDiscerption,
        //                           CouponTitle = coupon.CouponTitle,
        //                           CustomerName = customer_.Name,
        //                           CustomerRefferalName = "",
        //                           MerchantName = Merchant_.OrganizationName,
        //                           CustomerId = customer_.CustomerId
        //                       };

        //        if(validateCouponModel.CustomerId != 0)
        //        {
        //            int customerIds = context.Couponissuedetails
        //                     .Where(coupon => coupon.CouponSerialNo == validateCouponModel.CouponCode)
        //                     .Select(coupon => coupon.CustomerId)
        //                     .FirstOrDefault();

        //            string ReffName = context.Customers.Where(c => c.CustomerId == customerIds)
        //                      .Select(c => string.IsNullOrEmpty(c.Name)
        //                          ? "User - " + CommonUtility.maskString(Encoding.UTF8.GetString(CommonUtility.Decrypt(c.PhoneNumber, configuration.SymmetricKey)), "PhoneNUmber")
        //                          : c.Name)
        //                      .FirstOrDefault();


        //            var customers = from coupon in context.Couponmasters
        //                            join issueMaster in context.Couponissuemasters on coupon.CouponId equals issueMaster.CouponId
        //                            join issueDetail in context.Couponissuedetails on issueMaster.CouponIssuemasterId equals issueDetail.CouponIssueMasterId
        //                            join customer_ in context.Customers on issueDetail.CustomerId equals customer_.CustomerId
        //                            join custMerchnat_ in context.Customermerchantmappers on customer_.CustomerId equals custMerchnat_.CustomerId
        //                            join Merchant_ in context.Merchants on custMerchnat_.MerchantId equals Merchant_.MerchantId
        //                            where issueDetail.CouponSerialNo == validateCouponModel.CouponCode
        //                            select new ValidateCouponSearchModel
        //                            {
        //                                phoneNumber = customer_.PhoneNumber,
        //                                CouponCode = coupon.CouponCode,
        //                                RecLink = "",
        //                                MerchantId = coupon.MerchantId,
        //                                DiscountType = coupon.DiscountType,
        //                                DiscountValue = coupon.DiscountValue,
        //                                CouponDiscerption = coupon.CouponDiscerption,
        //                                CouponTitle = coupon.CouponTitle,
        //                                CustomerName = string.IsNullOrEmpty(customer_.Name) ? "User - " + CommonUtility.maskString(Encoding.UTF8.GetString(CommonUtility.Decrypt(customer_.PhoneNumber, configuration.SymmetricKey)), "PhoneNUmber") : customer_.Name,
        //                                CustomerRefferalName = ReffName,
        //                                MerchantName = Merchant_.OrganizationName,
        //                                CustomerId = customer_.CustomerId,
        //                                RefCustomerId = customerIds,
        //                           };
        //        }

        //     return   validateCouponSearch;


        //    }
        //    else if(validateCouponModel.CustomerId != 0)
        //    {

        //    }

        //    return validateCouponSearch;


        //}

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




    }
}

