using BlooTusk.Business.Interface;
using BlooTusk.Common;
using BlooTusk.Entity.BlooTuskModel;
using BlooTusk.Model.Models;
using Google.Protobuf.WellKnownTypes;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Net.Mail;
using System.Runtime.Intrinsics.Arm;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Twilio;
using Twilio.Base;
using Twilio.Rest.Api.V2010.Account;
using Twilio.TwiML.Voice;
using Twilio.Types;
using static Org.BouncyCastle.Bcpg.Attr.ImageAttrib;

namespace BlooTusk.Business.Implementation
{
    public class UserService : IUserService
    {
        BlooTuskContext context;

        private ConfigurationModel configuration;
        public UserService(BlooTuskContext _context, Microsoft.Extensions.Options.IOptions<ConfigurationModel> configuration)
        {
            context = _context;
            this.configuration = configuration.Value;
        }
        public OTPModel SendOTP(OTPModel oTPModel, ref ErrorResponseModel errorResponseModel)
        {
            bool success = false;
            Random generator = new Random();
            String randomNumber = generator.Next(1, 1000000).ToString("D6");
            string message = "\r\nHello User,\r\nThank you for choosing BlooTusk. Use this OTP " + randomNumber + " to complete your sign up procedures and verify your account on BlooTusk";
            bool smsStatus=SendSms(oTPModel.PhoneNumber, message);

         
            return new OTPModel
            {
                PhoneNumber = oTPModel.PhoneNumber,
                PhoneNumberOTP = randomNumber,
            };
        }

        private bool SendSms(string phonenumber, string verificationMessage)
        {
            string ToNumber = string.Empty;
            bool result = false;
            using (var web = new System.Net.WebClient())
            {
                try
                {
                    if(configuration.Bypassgetway == true)
                    {
                   
                    TwilioClient.Init(configuration.ACCOUNT_SID, configuration.AUTH_TOKEN);
                    //+918408845409
                    
                    if(configuration.MobileCountryCode != null)
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
                    throw ex;
                    result = false;
                }
            }
            return result;
        }
        public bool UpdateCustomer(CustomerModel customerModel, ref ErrorResponseModel errorResponseModel)
        {
            bool result = false;

            var customerToUpdate = context.Customers.FirstOrDefault(c => c.CustomerId == customerModel.CustomerID);
        
            if(customerModel.Name == "")
            {
                customerToUpdate.RecStatus = customerModel.RecStatus;
                context.SaveChanges();
                result = true;
            }
            else if (customerModel.Name != "")
            {

                var customerToUpdateWeb = context.Customers.FirstOrDefault(c => c.CustomerId == customerModel.CustomerID);

                customerToUpdateWeb.Name = customerModel.Name;
                customerToUpdateWeb.Lastname = customerModel.Lastname;  
               // customerToUpdateWeb.RecStatus = "A";
                context.SaveChanges();
                result = true;
            }           
            return result;
        }
        public bool AddCustomer(CustomerModel customerModel, ref ErrorResponseModel errorResponseModel)
        {

            bool result = false;
            //Get last usercode 
            string mobNo = null;
            var customerInfo = new Customer();

           string PhoneNo = Convert.ToBase64String(CommonUtility.Encrypt(customerModel.PhoneNumber, configuration.SymmetricKey));
            var isAlreadySignupCount = context.Customers.Count(customer => customer.PhoneNumber == PhoneNo);

            if (!customerModel.MerchantCode.Contains("M111"))
            {

                byte[] data = Convert.FromBase64String(customerModel.MerchantCode);
                customerModel.MerchantCode = Encoding.UTF8.GetString(data);
            }
            else
            {
                customerModel.MerchantCode = customerModel.MerchantCode;
            }

            var customerData = (from customer in context.Customers
                                orderby customer.CustomerId descending
                                select new UserModel
                                {
                                    UserCode = customer.CustomerCode,
                                }).FirstOrDefault();
            int customerCode = 0;
            if (customerData == null)
            {
                customerCode = 1100101;
            }
            else
            {
                var code = Convert.ToInt32(customerData.UserCode.Replace("C", "").Trim());
                customerCode = code + 1;
            }

            try
            {                

                string statusCode = "";

                if(isAlreadySignupCount == 0)
                {                  

                    customerInfo.Name = customerModel.Name;
                    customerInfo.Lastname = customerModel.Lastname;
                    customerInfo.PhoneNumber = Convert.ToBase64String(CommonUtility.Encrypt(customerModel.PhoneNumber, configuration.SymmetricKey));
                    customerInfo.IsPhoneNumberValidate = customerModel.IsPhoneNumberValidate;
                    customerInfo.CustomerCode = "C" + customerCode;
                    customerInfo.RecStatus = "A";
                    customerInfo.CreatedBy = null;
                    customerInfo.CreatedDate = DateTime.Now;
                    context.Customers.Add(customerInfo);
                    context.SaveChanges();
                }
                else
                {
                    var customerToUpdate = context.Customers.FirstOrDefault(c => c.PhoneNumber == PhoneNo);
                    customerInfo.CustomerId = customerToUpdate.CustomerId;
                }

                var CMMapper = new Customermerchantmapper();
                CMMapper.MerchantId = customerModel.MerchantID;
                CMMapper.CustomerId = customerInfo.CustomerId;
                CMMapper.ReferCode = customerModel.MerchantCode + "C" + customerCode;
                CMMapper.ReferBy = Convert.ToInt32(customerModel.ReferBy);
                CMMapper.StopMessage = false;
                CMMapper.ApprovlStatus = "A";
                context.Customermerchantmappers.Add(CMMapper);
                context.SaveChanges();

                var merchantInfo = context.Merchants
                                    .Where(merchant => merchant.MerchantId == CMMapper.MerchantId)
                                    .Join(context.Pos, merchant => merchant.MerchantId, pos => pos.MerchantId, (merchant, pos) => new
                                    {
                                        MerchantName = merchant.OrganizationName ,// + " " + pos.Posname,
                                        MerchantCode = merchant.MerchantCode
                                    })
                                    .FirstOrDefault();


                bool success = false;
                Random generator = new Random();
                String randomNumber = generator.Next(1, 1000000).ToString("D6");


                var SignupSucesssfulMessage = (from template_ in context.Smstemplates
                                             where template_.MerchantId == customerModel.MerchantID && template_.ModeTypeId == 1
                                             select new PosDetailsModel
                                             {
                                                 Posaddress = template_.MessageContent,
                                             }
                      ).FirstOrDefault();

                // string message = " \"Congratulations, " + customerInfo.Name + "! You're now part of " + merchantInfo.MerchantName + " exclusive community. Enjoy [Discount/Coupon Offer] on your next visit.Get ready to unlock exciting rewards! Stay tuned for your personalized referral link";

                string signupmessage = SignupSucesssfulMessage.Posaddress;

                string Signupresults = signupmessage;
                string Signuppattern = @"\[MerchantName\]";
                string Signupreplace = merchantInfo.MerchantName;
                signupmessage = Regex.Replace(Signupresults, Signuppattern, Signupreplace);


              // string custPhoneNumber = GetDecryptedString(customerInfo.PhoneNumber);

                SendSms(customerModel.PhoneNumber, signupmessage);

                var code = merchantInfo.MerchantCode + customerInfo.CustomerCode;
                var encryptedMerchantCode = Convert.ToBase64String(Encoding.UTF8.GetBytes(GetEncryptedString(code)));

                // string RefLink = configuration.MerchantSignUpURL + encryptedMerchantCode;

                string RefLink = configuration.MerchantSignUpURL + Convert.ToBase64String(Encoding.UTF8.GetBytes(Convert.ToBase64String(CommonUtility.Encrypt(CMMapper.UserMerchantMapperId.ToString(), configuration.SymmetricKey))));


                var Rewardmap = new Rewardpointmaster();
                Rewardmap.MerchantId = customerModel.MerchantID;

                var RewardPoint = context.Rewardpointmasters.Where(x => x.RewardTypeId == 1 && x.MerchantId == customerModel.MerchantID)
                     .Select(Rewardmap => new
                     {
                         RewardId = Rewardmap.RewardPonitId,
                        RewardPoints = Rewardmap.RewardPoint,
                     })
                    .FirstOrDefault();

                // var isAlreadySignup = context.Customers.Any(x => x.PhoneNumber == customerInfo.PhoneNumber);
                //var isAlreadySignupCount = context.Customers.Count(customer => customer.PhoneNumber == customerInfo.PhoneNumber);
             
                if (isAlreadySignupCount == 0)
                {
                    var rewardtransactionInfo = new Rewardpointtransaction();
                    rewardtransactionInfo.MerchantId = customerModel.MerchantID;
                    rewardtransactionInfo.RewardPointId = RewardPoint.RewardId;
                    rewardtransactionInfo.CustomerId = customerInfo.CustomerId;
                    rewardtransactionInfo.CreatedBy = customerModel.CreatedBy;
                    rewardtransactionInfo.Points = RewardPoint.RewardPoints;
                    rewardtransactionInfo.TransactionType = 1;
                    rewardtransactionInfo.Createddate = DateTime.Now;
                    rewardtransactionInfo.TransactionDate = DateTime.Now;
                    context.Rewardpointtransactions.Add(rewardtransactionInfo);
                    context.SaveChanges();
                }

                if (customerModel.ReferBy != 0)
                {
                    //var RefferalRewardPoint = context.Rewardpointmasters.Where(x =>  x.RewardTypeId == 2 && x.MerchantId == customerModel.MerchantID)
                    //                    .Select(Rewardmap => new
                    //                    {
                    //                        RefferalRewardId = Rewardmap.RewardPonitId,
                    //                        RefferalRewardPoints = Rewardmap.RewardPoint,
                    //                    })
                    //                   .FirstOrDefault();
                    //var RefferalrewardtransactionInfo = new Rewardpointtransaction();
                    //RefferalrewardtransactionInfo.MerchantId = customerModel.MerchantID;
                    //RefferalrewardtransactionInfo.Points = RefferalRewardPoint.RefferalRewardPoints;
                    //RefferalrewardtransactionInfo.TransactionType = 2;
                    //RefferalrewardtransactionInfo.RewardPointId = RefferalRewardPoint.RefferalRewardId;
                    //RefferalrewardtransactionInfo.CustomerId = customerModel.ReferBy;
                    //RefferalrewardtransactionInfo.CreatedBy = customerModel.CreatedBy;
                    //RefferalrewardtransactionInfo.Createddate = DateTime.Now;
                    //RefferalrewardtransactionInfo.TransactionDate = DateTime.Now;
                    //context.Rewardpointtransactions.Add(RefferalrewardtransactionInfo);
                    //context.SaveChanges();
                }
                //else
                //{
                //    errorResponseModel.Message = GlobalConstants.DuplicateCategory;
                //    result = false;
                //    statusCode = "DR";
                //}
                string Refmessage = "";

                if (customerModel.ReferBy != 0)
                {
                    bool smsStatuss = false;
                    string custrefPhoneNumber = "";

                    var RefferalRewardPoint   = (from customer_ in context.Customers
                                                  join Customermerchantmapper_ in context.Customermerchantmappers on customer_.CustomerId equals Customermerchantmapper_.ReferBy
                                                      where Customermerchantmapper_.ReferBy == customerModel.ReferBy
                                                      select new 
                                                      {
                                                          mobileno = customer_.PhoneNumber,
                                                      }
                             ).FirstOrDefault();
                   
                    //var MerchantRefferalRewardMrssage = (from template_ in context.Smstemplates
                    //                             where template_.MerchantId == customerModel.MerchantID && template_.ModeTypeId == 2 && template_.i
                    //                             select new 
                    //                             {
                    //                                 Messagecontent = template_.MessageContent,

                    //                             }
                    // ).FirstOrDefault();
                    // 0 record insert

                  //  var RefferalByMrssage = (from template_ in context.Smstemplates
                  //                               where template_.MessageTypeId == 3  && template_.MerchantId == customerModel.MerchantID
                  //                           select new PosDetailsModel
                  //                               {
                  //                                   Posname = template_.MessageContent,
                  //                               }
                  //  ).FirstOrDefault();

                  //  //Refmessage = "Great news! You've successfully referred your "+ customerModel.Name + " to BlooTusk.You've " +
                  //  //    "earned 200 Reward points. Thank you for spreading the word and sharing the benefits! Feel free" +
                  //  //    " to login to your account to check BlooTusk rewards. Happy earning and enjoy your rewards! - Team BlooTusk";

                  //  custrefPhoneNumber = GetDecryptedString(RefferalRewardPoint.mobileno);
                  //string  custrefPhoneNumbers = CommonUtility.maskString(Encoding.UTF8.GetString(CommonUtility.Decrypt(RefferalRewardPoint.mobileno, configuration.SymmetricKey)), "PhoneNUmber");
                             

                  //  //custrefPhoneNumber
                  //  string RefferalByMrssageresults = RefferalByMrssage.Posname;
                  //  string RefferalByMrssagepattern = @"\[User's Name\]";
                  //  string RefferalByMrssagereplace = customerModel.PhoneNumber;
                  //  RefferalByMrssage.Posname = Regex.Replace(RefferalByMrssageresults, RefferalByMrssagepattern, RefferalByMrssagereplace);

                  //  SendSms(custrefPhoneNumber, RefferalByMrssage.Posname);
                }

                // text for referal link

                var RefferalRewardMrssage = (from template_ in context.Smstemplates
                                             where template_.MerchantId == customerModel.MerchantID && template_.MessageTypeId == 2
                                               select new PosDetailsModel
                                               {
                                                        Posname = template_.MessageContent,
                                               }
                      ).FirstOrDefault();

                string results = RefferalRewardMrssage.Posname;
                string pattern = @"\[MerchantName\]";
                string replace = merchantInfo.MerchantName;
                RefferalRewardMrssage.Posname = Regex.Replace(results, pattern, replace);

                string results1 = RefferalRewardMrssage.Posname;
                string pattern1 = @"\[Referral Link\]";
                string replace1 = RefLink;
                RefferalRewardMrssage.Posname = Regex.Replace(results1, pattern1, replace1);

                string results2 = RefferalRewardMrssage.Posname;
                string pattern2 = @"\[Reward Points\]";
                string replace2 = Convert.ToString(RewardPoint.RewardPoints);
                RefferalRewardMrssage.Posname = Regex.Replace(results2, pattern2, replace2);


                string results3 = RefferalRewardMrssage.Posname;
                string pattern3 = @"\[UserName\]";
                string replace3 = customerModel.PhoneNumber;
                RefferalRewardMrssage.Posname = Regex.Replace(results3, pattern3, replace3);

                Console.WriteLine(RefferalRewardMrssage.Posname);

                SendSms(customerModel.PhoneNumber, RefferalRewardMrssage.Posname);

                result = true;

            }
            catch (Exception ex)
            {
                result = false;
                throw;
            }

            return result;
        }
        public bool checkDuplicateCustomer(string phoneNumber ,  int MerchantId)
        {
            string MobileNo = GetEncryptedString(phoneNumber);
            bool result = false;
            var duplicateUser = (from customer in context.Customers                                  
                                 join customerMerchant in context.Customermerchantmappers
                                 on customer.CustomerId equals customerMerchant.CustomerId
                                 where customerMerchant.MerchantId == MerchantId && customer.PhoneNumber == MobileNo
                                 select customer).FirstOrDefault();

            if (duplicateUser != null)
            {
                result = true;
            }

            return result;
        }
        public List<CustomerSearchResultModel> GetCustomerList(CustomerSearchModel customerSearchModel)
        {
           List<CustomerSearchResultModel> customerSearchResultModel = new List<CustomerSearchResultModel>();      

            var pageNumber = (customerSearchModel.PageNumber <= 0) ? 1 : customerSearchModel.PageNumber;
            var pageSize =10;
            var totalRecords = 0.0;
            var totalPages = 0.0;
            var skip = 0;

            if (!String.IsNullOrEmpty(customerSearchModel.Keyword))
            {
                totalRecords = (from customer_ in context.Customers
                                join cmmaaper in context.Customermerchantmappers on customer_.CustomerId equals cmmaaper.CustomerId
                                where cmmaaper.MerchantId == customerSearchModel.MerchantID && customer_.RecStatus == "A" && 
                                customer_.PhoneNumber == Convert.ToBase64String(CommonUtility.Encrypt(customerSearchModel.Keyword, configuration.SymmetricKey))
                                || customer_.Name == customerSearchModel.Keyword
                                select customer_).ToList().Count;
                 totalPages = Math.Ceiling((double)totalRecords / pageSize);
                 skip = (pageNumber - 1) * pageSize;
            }
            else
            {
                totalRecords = context.Customermerchantmappers.Count(x => x.MerchantId == customerSearchModel.MerchantID);
                totalPages = Math.Ceiling((double)totalRecords / pageSize);
                skip = (pageNumber - 1) * pageSize;
            }
            
            if (!String.IsNullOrEmpty(customerSearchModel.Keyword))
            {
                var customerDAta = (from customer_ in context.Customers
                                 //   join couponReddem_ in context.Couponredeemtions on customer_.CustomerId equals couponReddem_.RedeembyCustomerId
                                    join cmmaaper in context.Customermerchantmappers on customer_.CustomerId equals cmmaaper.CustomerId
                                    where cmmaaper.MerchantId == customerSearchModel.MerchantID && customer_.RecStatus == "A" && customer_.PhoneNumber == Convert.ToBase64String(CommonUtility.Encrypt(customerSearchModel.Keyword, configuration.SymmetricKey))
                                    select new CustomerSearchResultModel
                                    {
                                        CustomerID = customer_.CustomerId,
                                        Name = string.IsNullOrEmpty(customer_.Name) ? "User - " + CommonUtility.maskString(Encoding.UTF8.GetString(CommonUtility.Decrypt(customer_.PhoneNumber, configuration.SymmetricKey)), "PhoneNUmber") : (customer_.Name + (string.IsNullOrEmpty(customer_.Lastname) ? "" : " " + customer_.Lastname)),
                                        PhoneNumber = Encoding.UTF8.GetString(CommonUtility.Decrypt(customer_.PhoneNumber, configuration.SymmetricKey)),
                                        CustomerCode = customer_.CustomerCode,
                                        Trustscore = 0,
                                        isSelected = false,
                                        isVisibleMenu = false,
                                        Level = "",
                                         NoOfVisit = 0,
                                         RecStatus = customer_.RecStatus,
                                       // LastVisited = Convert.ToDateTime(couponReddem_.CreatedDate),                                 
                                         BlotuskPoint = 0,
                                        PageCount = totalPages

                                    }).ToList().Skip(skip).Take(pageSize);

                customerSearchResultModel = customerDAta.ToList();
            }
            else
            {
                var customerDAta = (from customer_ in context.Customers
                                 //  join couponReddem_ in context.Couponredeemtions on customer_.CustomerId equals couponReddem_.RedeembyCustomerId
                                    join cmmaaper in context.Customermerchantmappers on customer_.CustomerId equals cmmaaper.CustomerId
                                    where cmmaaper.MerchantId == customerSearchModel.MerchantID && customer_.RecStatus == "A"
                                    select new CustomerSearchResultModel
                                    {
                                        CustomerID = customer_.CustomerId,                                      
                                        Name = string.IsNullOrEmpty(customer_.Name) ? "User - " + CommonUtility.maskString(Encoding.UTF8.GetString(CommonUtility.Decrypt(customer_.PhoneNumber, configuration.SymmetricKey)), "PhoneNUmber") : (customer_.Name + (string.IsNullOrEmpty(customer_.Lastname) ? "" : " " + customer_.Lastname)),
                                        PhoneNumber = Encoding.UTF8.GetString(CommonUtility.Decrypt(customer_.PhoneNumber, configuration.SymmetricKey)),
                                        CustomerCode = customer_.CustomerCode,
                                        isSelected = false,
                                        isVisibleMenu = false,
                                        Trustscore = 0,
                                        RecStatus = customer_.RecStatus,
                                        Level = "",
                                        NoOfVisit = 0,
                                       // LastVisited = DateTime.Now()//Convert.ToDateTime(couponReddem_.CreatedDate),
                                        BlotuskPoint = 0,
                                        PageCount = totalPages,
                                    }).ToList().Skip(skip).Take(pageSize);
              
                customerSearchResultModel = customerDAta.ToList();

            }
            return customerSearchResultModel;
        }
        public CustomerSearchResultModel GetCustomerRefferalList(CustomerSearchModel refModel)
        {
            var customerList = new CustomerSearchResultModel();
            var pageNumber = (refModel.PageNumber <= 0) ? 1 : refModel.PageNumber;
            var pageSize = 10;
            var totalRecords = 0.0;
            var totalPages = 0.0;
            var skip = 0;

            // GetDecryptedString(refModel.PhoneNumber);

            string PhoneNumber = Convert.ToBase64String(CommonUtility.Encrypt(refModel.PhoneNumber, configuration.SymmetricKey));


            totalRecords = (from customer_ in context.Customers
                                 join mapper in context.Customermerchantmappers
                                 on customer_.CustomerId equals mapper.ReferBy
                                 where customer_.PhoneNumber == PhoneNumber
                                 select customer_).Count();


          //  totalRecords = context.Customermerchantmappers.Count(x => x.ReferBy == refModel.CustomerID);
                totalPages = Math.Ceiling((double)totalRecords / pageSize);
                skip = (pageNumber - 1) * pageSize;

            var customer = new List<CustomerModel>();

            if (refModel.Keyword != "")
            {


                //var customerDAta = (from C in context.Customers
                //                    join Cm in context.Customermerchantmappers on C.CustomerId equals Cm.ReferBy
                //                    join Cmm in context.Customers on Cm.CustomerId equals Cmm.CustomerId
                //                    join M in context.Merchants on Cm.MerchantId equals M.MerchantId
                //                    join Rt in context.Rewardpointtransactions on Cmm.CustomerId equals Rt.CustomerId
                //                    join Rm in context.Rewardpointmasters on Rt.RewardPointId equals Rm.RewardPonitId
                //                    where C.PhoneNumber == PhoneNumber && Cmm.Name.Contains(refModel.Keyword, StringComparison.OrdinalIgnoreCase)


                //                    group Rt.TransactionType by new
                //                    {

                //                        Cmm.CustomerId,
                //                        Cmm.Name,
                //                        Cmm.PhoneNumber,
                //                        Cmm.CustomerCode,
                //                        M.OrganizationName,

                //                        Cmm.CreatedDate

                //                    } into g
                //                    select new CustomerModel
                //                    {
                //                        CustomerID = g.Key.CustomerId,
                //                        Name = string.IsNullOrEmpty(g.Key.Name) ? "User - " + CommonUtility.maskString(Encoding.UTF8.GetString(CommonUtility.Decrypt(g.Key.PhoneNumber, configuration.SymmetricKey)), "PhoneNUmber") : g.Key.Name,
                //                        CustomerCode = g.Key.CustomerCode,
                //                        MerchantName = g.Key.OrganizationName,
                //                        CreatedDate = g.Key.CreatedDate,
                //                        RewardPoint = g.Sum!=3()
                //                    }).ToList();


                var customerData = (from C in context.Customers
                                    join Cm in context.Customermerchantmappers on C.CustomerId equals Cm.ReferBy
                                    join Cmm in context.Customers on Cm.CustomerId equals Cmm.CustomerId
                                    join M in context.Merchants on Cm.MerchantId equals M.MerchantId
                                    join Rt in context.Rewardpointtransactions on Cmm.CustomerId equals Rt.CustomerId
                                    join Rm in context.Rewardpointmasters on Rt.RewardPointId equals Rm.RewardPonitId
                                    where C.PhoneNumber == PhoneNumber && Cmm.Name.Contains(refModel.Keyword, StringComparison.OrdinalIgnoreCase)
                                    group new { Rm.RewardPoint, Rm.RewardTypeId } by new
                                    {
                                        Cmm.CustomerId,
                                        Cmm.Name,
                                       Cmm.Lastname,
                                        Cmm.PhoneNumber,
                                        Cmm.CustomerCode,
                                        M.OrganizationName,
                                        Cmm.CreatedDate
                                    } into g
                                    select new CustomerModel
                                    {
                                        CustomerID = g.Key.CustomerId,
                                        Name = string.IsNullOrEmpty(g.Key.Name) ? "User - " + CommonUtility.maskString(Encoding.UTF8.GetString(CommonUtility.Decrypt(g.Key.PhoneNumber, configuration.SymmetricKey)), "PhoneNumber") : (g.Key.Name + (string.IsNullOrEmpty(g.Key.Lastname) ? "" : " " + g.Key.Lastname)),
                                        CustomerCode = g.Key.CustomerCode,
                                        MerchantName = g.Key.OrganizationName,
                                        CreatedDate = g.Key.CreatedDate,
                                        RewardPoint = g.Sum(x => x.RewardTypeId == 3 ? -x.RewardPoint : x.RewardPoint)
                                    }).ToList();



                //var customerDAta = (from customer_ in context.Customers
                //                    // == refModel.Keyword
                //                    join cmmaaper in context.Customermerchantmappers on customer_.CustomerId equals cmmaaper.ReferBy
                //                    join merchant in context.Merchants on cmmaaper.MerchantId equals merchant.MerchantId
                //                    join rewardtransaction in context.Rewardpointtransactions on customer_.CustomerId equals rewardtransaction.CustomerId
                //                    join rewardmaster in context.Rewardpointmasters on rewardtransaction.RewardPointId equals rewardmaster.RewardPonitId
                //                    where customer_.PhoneNumber == PhoneNumber && customer_.Name.Contains(refModel.Keyword, StringComparison.OrdinalIgnoreCase)

                //                    select new CustomerModel
                //                    {
                //                        CustomerID = customer_.CustomerId,
                //                        ApprovalStatus = merchant.ApprovalStatus.Equals("N") ? "New" : merchant.ApprovalStatus.Equals("I") ? "InProgress" : merchant.ApprovalStatus.Equals("V") ? "Verified" : "Rejected",
                //                        Name = string.IsNullOrEmpty(customer_.Name) ? "User - " + CommonUtility.maskString(Encoding.UTF8.GetString(CommonUtility.Decrypt(customer_.PhoneNumber, configuration.SymmetricKey)), "PhoneNUmber") : customer_.Name,
                //                        CustomerCode = customer_.CustomerCode,
                //                        MerchantName = merchant.OrganizationName,
                //                        CreatedDate = customer_.CreatedDate,
                //                        RewardPoint = rewardmaster.RewardPoint

                //                    }).ToList();
                customerList.CustomerList = customerData.ToList();
                customerList.PageCount = totalPages;
            }
            else
            {

                //temp hide the code 
                //var  customerDAta = ( from C in context.Customers join
                //                            Cm in context.Customermerchantmappers  on C.CustomerId equals Cm.ReferBy
                //                              join Cmm in context.Customers  on Cm.CustomerId equals Cmm.CustomerId
                //                              join  M in context.Merchants on Cm.MerchantId equals M.MerchantId
                //                              join Rt in context.Rewardpointtransactions on Cmm.CustomerId equals Rt.CustomerId
                //                              join Rm in context.Rewardpointmasters on Rt.RewardPointId equals Rm.RewardPonitId

                //                      where C.PhoneNumber == PhoneNumber
                //                      select new CustomerModel
                //                      {
                //                          CustomerID = Cmm.CustomerId,
                //                         // Name = customer_.Name,
                //                          Name = string.IsNullOrEmpty(Cmm.Name) ? "User - " + CommonUtility.maskString(Encoding.UTF8.GetString(CommonUtility.Decrypt(Cmm.PhoneNumber, configuration.SymmetricKey)), "PhoneNUmber") : Cmm.Name,
                //                          CustomerCode = Cmm.CustomerCode,

                //                        MerchantName = M.OrganizationName,
                //                          CreatedDate = Cmm.CreatedDate,
                //                          RewardPoint = Rm.RewardPoint

                //                      }).ToList();


                var customerDAta = (from C in context.Customers
                                           join Cm in context.Customermerchantmappers on C.CustomerId equals Cm.ReferBy
                                           join Cmm in context.Customers on Cm.CustomerId equals Cmm.CustomerId
                                           join M in context.Merchants on Cm.MerchantId equals M.MerchantId
                                           join Rt in context.Rewardpointtransactions on Cmm.CustomerId equals Rt.CustomerId
                                           join Rm in context.Rewardpointmasters on Rt.RewardPointId equals Rm.RewardPonitId
                                           where C.PhoneNumber == PhoneNumber
                                           group new { Rm.RewardPoint, Rm.RewardTypeId } by new
                                           {
                                               Cmm.CustomerId,
                                               Cmm.Name,
                                               Cmm.Lastname,
                                               Cmm.PhoneNumber,
                                               Cmm.CustomerCode,
                                               M.OrganizationName,
                                               Cmm.CreatedDate
                                           } into g
                                           select new CustomerModel
                                           {
                                               CustomerID = g.Key.CustomerId,
                                               Name = string.IsNullOrEmpty(g.Key.Name) ? "User - " + CommonUtility.maskString(Encoding.UTF8.GetString(CommonUtility.Decrypt(g.Key.PhoneNumber, configuration.SymmetricKey)), "PhoneNUmber") : (g.Key.Name + (string.IsNullOrEmpty(g.Key.Lastname) ? "" : " " + g.Key.Lastname)),
                                               CustomerCode = g.Key.CustomerCode,
                                               MerchantName = g.Key.OrganizationName,
                                               CreatedDate = g.Key.CreatedDate,
                                               // RewardPoint = g.Sum()
                                               RewardPoint = g.Sum(x => x.RewardTypeId == 3 ? -x.RewardPoint : x.RewardPoint)

                                           }).ToList();

                customerList.CustomerList = customerDAta.ToList();
                customerList.PageCount = totalPages;
            }
            return customerList;
        }
        public CustomerRewardPointResultModel GetCustomerRewardPointListList(int rewardPointId, string PhoneNumber)
        {
            var customerRewardList = new CustomerRewardPointResultModel();
            //var pageNumber = (customerRewardSearch.PageNo <= 0) ? 1 : PageNo;
            var pageSize = 10;
            var totalRecords = 0.0;
            var totalPages = 0.0;
            var skip = 0;

           string encryptedPhoneNumber = Convert.ToBase64String(CommonUtility.Encrypt(PhoneNumber, configuration.SymmetricKey));


            var customer = new List<RewardpointtransactionModel>();
         
            //var merchant = context.Customermerchantmappers.Where(x => x.CustomerId == CustomerId)
            //       .Select(merchat => new
            //       {
            //           MerchantId = merchat.MerchantId
            //       }) .FirstOrDefault();

            totalRecords = (from customer_ in context.Customers
                            join mapper in context.Customermerchantmappers
                            on customer_.CustomerId equals mapper.CustomerId
                            where customer_.PhoneNumber == encryptedPhoneNumber
                            select customer_).Count();

            if (rewardPointId != 0)
            {

                var custid = (from customer_ in context.Customers
                              where customer_.PhoneNumber == encryptedPhoneNumber
                              select new
                              {
                                  CustID = customer_.CustomerId,
                              }).FirstOrDefault();

                var customerDAta = (from rewardPointtrans_ in context.Rewardpointtransactions
                                    join customer_ in context.Customers on rewardPointtrans_.CustomerId equals customer_.CustomerId
                                    join Merchant_ in context.Merchants on rewardPointtrans_.MerchantId equals Merchant_.MerchantId
                                   
                                    join rewardpoint_ in context.Rewardpointmasters on rewardPointtrans_.RewardPointId equals rewardpoint_.RewardPonitId
                                    join rewardtype_ in context.Rewardtypemasters on rewardpoint_.RewardTypeId equals rewardtype_.RewardTypeId  
                                   where rewardtype_.RewardTypeId == rewardPointId && customer_.PhoneNumber == encryptedPhoneNumber

                                    select new RewardpointtransactionModel
                                    {
                                        TransactionType = rewardtype_.RewardType,
                                        RewardPoint = rewardpoint_.RewardPoint,
                                        TransactionDate = rewardPointtrans_.TransactionDate,
                                        MerchantName = Merchant_.OrganizationName

                                    }).ToList().Skip(skip).Take(pageSize);

                customerRewardList.CustomerList = customerDAta.ToList();
            }
            else
            {

                var custid = (from customer_ in context.Customers
                              where customer_.PhoneNumber == encryptedPhoneNumber
                              select new
                              {
                                  CustID = customer_.CustomerId,
                              }).FirstOrDefault();

                var customerDAta = (from rewardPointtrans_ in context.Rewardpointtransactions
                                    join customer_ in context.Customers on rewardPointtrans_.CustomerId equals customer_.CustomerId
                                    join Merchant_ in context.Merchants on rewardPointtrans_.MerchantId equals Merchant_.MerchantId
                                    where customer_.PhoneNumber == encryptedPhoneNumber
                                    // where customer_.CustomerId == custid.CustID
                                    //&& rewardPointtrans_.MerchantId == merchant.MerchantId
                                    join rewardpoint_ in context.Rewardpointmasters on rewardPointtrans_.RewardPointId equals rewardpoint_.RewardPonitId
                                    join rewardtype_ in context.Rewardtypemasters on rewardpoint_.RewardTypeId equals rewardtype_.RewardTypeId
                                   
                                    select new RewardpointtransactionModel
                                    {

                                        TransactionType = rewardtype_.RewardType,
                                        RewardPoint = rewardpoint_.RewardPoint,
                                        TransactionDate = rewardPointtrans_.TransactionDate,
                                        MerchantName = Merchant_.OrganizationName

                                    }).ToList().Skip(skip).Take(pageSize);

                customerRewardList.CustomerList = customerDAta.ToList();
            }


            customerRewardList.PageCount = totalPages;


            return customerRewardList;
        }
        public CustomerInfoModel GetCustomerByCOde(string customercode, ref ErrorResponseModel errorResponseModel)
        {  

            var customerInfoModel = (from custmerMapper in context.Customermerchantmappers
                                     join Customer_ in context.Customers 
                                     on custmerMapper.CustomerId equals Customer_.CustomerId
                                     where custmerMapper.UserMerchantMapperId == Convert.ToInt32(customercode)
                                     select new CustomerInfoModel
                                     {
                                         CustomerID = Customer_.CustomerId,
                                         Name =  (string.IsNullOrEmpty(Customer_.Name) ? "User - " + CommonUtility.maskString(Encoding.UTF8.GetString(CommonUtility.Decrypt(Customer_.PhoneNumber, configuration.SymmetricKey)), "PhoneNUmber") : (Customer_.Name + (string.IsNullOrEmpty(Customer_.Lastname) ? "" : " " + Customer_.Lastname))),
                                         CustomerCode = Customer_.CustomerCode,                                   
                                         PhoneNumber = CommonUtility.maskString(Encoding.UTF8.GetString(CommonUtility.Decrypt(Customer_.PhoneNumber, configuration.SymmetricKey)), "PhoneNUmber"),
                                      }).SingleOrDefault();

            return customerInfoModel;
        }
        private string GetDecryptedString(string value)
        {
            return Encoding.UTF8.GetString(CommonUtility.Decrypt(value, configuration.SymmetricKey));
        }
        private string GetEncryptedString(string? value)
        {
            return Convert.ToBase64String(CommonUtility.Encrypt(value, configuration.SymmetricKey));
        }

        //public int GetReferalCount(string phoneNumber)
        //{

        //    int count =(from custmermapper in context.Customermerchantmappers 
        //                join customer_ in context.Customers  on custmermapper.ReferCode equals customer_.CustomerId
                      
        //                select custmermapper).Count();
        //    return count;
        //}

        public int GetReferalCount(string phoneNumber)
        {

            string testPhoneNumber = GetEncryptedString(phoneNumber);
            int count = context.Customermerchantmappers
                .Join(
                context.Customers,
                custmermapper => custmermapper.ReferBy,
                customer => customer.CustomerId,
                (custmermapper, customer) => customer)
                .Count(customer => customer.PhoneNumber == testPhoneNumber);
            return count;
        }


        public refferalLinkModel GetReferalLinkList(string PhoneNumber)
        {
            refferalLinkModel refferalLinkModel = new refferalLinkModel();

            string phoneNumber = GetEncryptedString(PhoneNumber);

            var CountOfRefferalLinks = (from cm in context.Customermerchantmappers
                                        join c in context.Customers on cm.CustomerId equals c.CustomerId
                                        join m in context.Merchants on cm.MerchantId equals m.MerchantId
                                        where c.PhoneNumber == phoneNumber
                                        select new refferallist
                                        {
                                            UserCustId = cm.UserMerchantMapperId,
                                            StopSms = cm.StopMessage,
                                            //    REfferalLink = configuration.MerchantSignUpURL +  Convert.ToBase64String(Encoding.UTF8.GetBytes(Convert.ToBase64String(CommonUtility.Encrypt(m.MerchantCode + c.CustomerCode, configuration.SymmetricKey)))),
                                            REfferalLink = configuration.MerchantSignUpURL + Convert.ToBase64String(Encoding.UTF8.GetBytes(Convert.ToBase64String(CommonUtility.Encrypt(cm.UserMerchantMapperId.ToString(), configuration.SymmetricKey)))),
                                            MerchantName = m.OrganizationName,
                                        }).Count();

            refferalLinkModel.CountOfRefferalLink = CountOfRefferalLinks;

            var referralList = (from cm in context.Customermerchantmappers
                                join c in context.Customers on cm.CustomerId equals c.CustomerId
                                join m in context.Merchants on cm.MerchantId equals m.MerchantId
                                where c.PhoneNumber == phoneNumber
                                select new refferallist
                                {
                                    UserCustId = cm.UserMerchantMapperId,
                                    StopSms = cm.StopMessage,
                                    //  REfferalLink = configuration.MerchantSignUpURL + Convert.ToBase64String(Encoding.UTF8.GetBytes(Convert.ToBase64String(CommonUtility.Encrypt(m.MerchantCode + c.CustomerCode, configuration.SymmetricKey)))),
                                    REfferalLink = configuration.MerchantSignUpURL + Convert.ToBase64String(Encoding.UTF8.GetBytes(Convert.ToBase64String(CommonUtility.Encrypt(cm.UserMerchantMapperId.ToString(), configuration.SymmetricKey)))),

                                    MerchantName = m.OrganizationName,
                                 
                                }).ToList();


            refferalLinkModel.referlist = referralList;

            return refferalLinkModel;
        }



        public string GetReferalLink(int custcode)
        {
                  var customerInfoModel = (from custmerchantmapper in context.Customermerchantmappers join
                                           Customer_ in context.Customers on custmerchantmapper.CustomerId equals Customer_.CustomerId
                                           join merchant_ in context.Merchants on custmerchantmapper.MerchantId equals merchant_.MerchantId
                                            where custmerchantmapper.CustomerId == custcode
                                    select new 
                                     {
                                       Customercode = Customer_.CustomerCode,
                                       MerchantCode = merchant_.MerchantCode
                                    }).SingleOrDefault();


              var code = customerInfoModel.MerchantCode + customerInfoModel.Customercode;

            var encryptedMerchantCode = Convert.ToBase64String(Encoding.UTF8.GetBytes(Convert.ToBase64String(CommonUtility.Encrypt(code, configuration.SymmetricKey))));
            string RefLink = configuration.MerchantSignUpURL + encryptedMerchantCode;

            return RefLink;

        }
        public int GetRewardPointSum(string PhoneNumber)
        {

            string testPhoneNumber = GetEncryptedString(PhoneNumber);  


         //   var testPhoneNumber = "yourPhoneNumber"; // Replace with the actual phone number
            var rewardSum = (from rewardpointtransaction_ in context.Rewardpointtransactions
                             join rewardpointmaster_ in context.Rewardpointmasters on rewardpointtransaction_.RewardPointId equals rewardpointmaster_.RewardPonitId
                             join Customer_ in context.Customers on rewardpointtransaction_.CustomerId equals Customer_.CustomerId
                             where Customer_.PhoneNumber == testPhoneNumber && (rewardpointmaster_.RewardTypeId == 1 || rewardpointmaster_.RewardTypeId == 2 || rewardpointmaster_.RewardTypeId == 3)
                             select new
                             { 
                                 RewardPoint = rewardpointmaster_.RewardPoint,
                                 IsPositive = rewardpointmaster_.RewardTypeId != 3
                             }).ToList();

            int totalRewardSum = Convert.ToInt32(rewardSum.Sum(item => item.IsPositive ? item.RewardPoint : -item.RewardPoint));


         //   int RewardPoints = Convert.ToInt32(rewardSum.Sum(x => x.RewardPoint) - Convert.ToInt32(rewardSumminus.Sum(x => x.RewardPoint)));
            return totalRewardSum;



        }
        public int GetDiscountCouponCount(string phoneNumber)
        {
            string testPhoneNumber = GetEncryptedString(phoneNumber);

            int countForPhoneNumber = context.Couponissuedetails
                  .Join(context.Customers,
                      coupon => coupon.CustomerId,
                      customer => customer.CustomerId,
                      (coupon, customer) => new
                      {
                          PhoneNumber = customer.PhoneNumber
                      })
                  .Where(c => c.PhoneNumber == testPhoneNumber)
                  .Count();
            return countForPhoneNumber;
        }
        public List<object> GetRewardPointMerchantWiseCount(string phoneNumber)
        {
            string testPhoneNumber = GetEncryptedString(phoneNumber);

            var rewardSumByMerchant = (from rewardpointtransaction in context.Rewardpointtransactions
                                       join rewardpointmaster in context.Rewardpointmasters on rewardpointtransaction.RewardPointId equals rewardpointmaster.RewardPonitId
                                       join customer in context.Customers on rewardpointtransaction.CustomerId equals customer.CustomerId
                                       join custMerchant in context.Customermerchantmappers on customer.CustomerId equals custMerchant.CustomerId
                                       where customer.PhoneNumber == testPhoneNumber 
                                       group new { custMerchant.MerchantId, rewardpointmaster.RewardPoint } by custMerchant.MerchantId into groupedResults
                                       select new
                                       {
                                           MerchantId = groupedResults.Key,
                                           TotalRewardPoints = groupedResults.Sum(x => x.RewardPoint)
                                       }).ToList();

            return rewardSumByMerchant.Cast<object>().ToList(); 
        }

        public bool UpdateCustomerMapper(CustomerMerchantModel customerMerchantModel, ref ErrorResponseModel errorResponseModel)
        {
            bool result = false;

            var customerToUpdate = (from mapper in context.Customermerchantmappers 
                                    where mapper.UserMerchantMapperId == customerMerchantModel.UserMerchantMapperId
                                  
                                    select mapper).FirstOrDefault();
            // var customerToUpdate = context.Customermerchantmappers(c => c.UserMerchantMapperId == customerMerchantModel.UserMerchantMapperId);

            customerToUpdate.StopMessage = customerMerchantModel.StopMessage;
           
            context.SaveChanges();
                result = true;
            
            return result;
        }
    }
}
