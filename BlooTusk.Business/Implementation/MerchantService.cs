using BlooTusk.Business.Interface;
using BlooTusk.Entity.BlooTuskModel;
using BlooTusk.Model.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Twilio.Types;
using Twilio;
using System.Net.Mail;
using System.Net;
using Twilio.Rest.Api.V2010.Account;
using Microsoft.Extensions.Options;
using Google.Protobuf.Collections;
using System.Security.Cryptography;
using System.Reflection.Emit;
using static Org.BouncyCastle.Bcpg.Attr.ImageAttrib;
using static System.Net.WebRequestMethods;
using System.Runtime.Intrinsics.X86;
using BlooTusk.Model;
using BlooTusk.Common; 
using Org.BouncyCastle.Crypto;
using Twilio.TwiML.Voice;
using Org.BouncyCastle.Bcpg;
using Microsoft.IdentityModel.Tokens;
using System.Web;
using Twilio.Http;
using System.Text.RegularExpressions;
using MySqlX.XDevAPI.Common;
using Google.Protobuf.WellKnownTypes;
using Twilio.TwiML.Messaging;
using Newtonsoft.Json.Linq;

namespace BlooTusk.Business.Implementation
{
    public class MerchantService : IMerchantService
    {
        BlooTuskContext context;
        private ConfigurationModel configuration;
        private EmailSettings emailSettings;
        public SMSTemplateService  Smstemplate;
       
        public MerchantService(BlooTuskContext _context, IOptions<ConfigurationModel> configuration, IOptions<EmailSettings> emailSettings)
        {
            context = _context;
            this.configuration = configuration.Value;
            this.emailSettings = emailSettings.Value;
        }
        public bool AddMerchant(MerchantModel merchantModel, ref ErrorResponseModel errorResponseModel)
        {
            bool result = false;
            var merchantInfo = new Merchant();
            string mrchantCode = GenerateMearchantCode();        
            try
            {
                merchantInfo.MerchantCode = mrchantCode;
                merchantInfo.OrganizationName = merchantModel.OrganizationName;
                merchantInfo.PhoneNumber = Convert.ToBase64String(CommonUtility.Encrypt(merchantModel.PhoneNumber, configuration.SymmetricKey));
                merchantInfo.Email = Convert.ToBase64String(CommonUtility.Encrypt(merchantModel.Email.ToLower(), configuration.SymmetricKey));
                merchantInfo.Password = Convert.ToBase64String(CommonUtility.Encrypt(merchantModel.Password, configuration.SymmetricKey));
                merchantInfo.ContactPersonName = merchantModel.ContactPersonName;
                merchantInfo.IsPhoneNumberValidate = merchantModel.IsPhoneNumberValidate;
                merchantInfo.IsEmailValidate = merchantModel.IsEmailValidate;
                merchantInfo.City = merchantModel.City;
                merchantInfo.RecStatus = "A";
                merchantInfo.ApprovalStatus = "N";
                merchantInfo.CreatedBy = merchantModel.CreatedBy;
                merchantInfo.CreatedDate = DateTime.Now;
                context.Merchants.Add(merchantInfo);
                context.SaveChanges();

                POSModel posInfo = merchantModel.POSInfo;
                var POSInfo = new Po();
                POSInfo.MerchantId = merchantInfo.MerchantId;
                POSInfo.CategoryId = posInfo.CategoryId;
                POSInfo.Posname = posInfo.Posname;
                POSInfo.Posaddress = posInfo.Posaddress;
                POSInfo.Zip = posInfo.Zip;
                POSInfo.StateId = posInfo.StateId;
                POSInfo.CountryId = posInfo.CountryId;
                POSInfo.RecStatus = "A";
                POSInfo.CreatedDate = DateTime.Now;
                context.Pos.Add(POSInfo);
                context.SaveChanges();

                //Get last admin recoard from merchant system user table

                //  string userID = GenerateMearchantUserID();
                string userID = GenerateUniqueUserCode(mrchantCode);
                var merchantUserInfo = new Merchantsystemuser();
                merchantUserInfo.MerchantId= merchantInfo.MerchantId;
                merchantUserInfo.Name= merchantModel.OrganizationName;
                merchantUserInfo.Email = Convert.ToBase64String(CommonUtility.Encrypt(merchantModel.Email.ToLower(), configuration.SymmetricKey));
                merchantUserInfo.PhoneNumber= Convert.ToBase64String(CommonUtility.Encrypt(merchantModel.PhoneNumber, configuration.SymmetricKey));
                merchantUserInfo.Password= Convert.ToBase64String(CommonUtility.Encrypt(merchantModel.Password, configuration.SymmetricKey));
                merchantUserInfo.UserId = userID;
                merchantUserInfo.RecStatus = "A";
                merchantUserInfo.IsAdmin = 1;
                merchantUserInfo.CreatedBy = merchantModel.CreatedBy;
                merchantUserInfo.CreatedDate=DateTime.Now;
                context.Merchantsystemusers.Add(merchantUserInfo);
                context.SaveChanges();
                AddTemplate(merchantInfo.MerchantId);
                AddReward(merchantInfo.MerchantId);

                string password = GetDecryptedString(merchantUserInfo.Password);

                //var RefferalRewardMrssage = (from template_ in context.Smstemplates
                //                             where template_.MerchantId == 0 && template_.ModeTypeId == 1
                //                             select new PosDetailsModel
                //                             {
                //                                 Posname = template_.MessageContent,
                //                             }
                //  ).FirstOrDefault();
                //string results = RefferalRewardMrssage.Posname;
                //string pattern = @"\[MerchantName\]";
                //string replace = merchantUserInfo.Name;
                //RefferalRewardMrssage.Posname = Regex.Replace(results, pattern, replace);

                //string message = " \"Congratulations, " + merchantUserInfo.Name + "! You're now part of Blootusk." +
                //    "Your Password is, " + password + " ";
                
                string message1 = " \"Congratulations, " + merchantUserInfo.Name + "! You're now part of Blootusk." +

                    "Hello " + merchantUserInfo.Name + "! Your UserName is " + merchantUserInfo.UserId + " ";

                string custPhoneNumber = GetDecryptedString(merchantUserInfo.PhoneNumber);
             //   bool smsStatus = SendSms(custPhoneNumber, message);
                bool smsStatus1 = SendSms(custPhoneNumber, message1);

                result = true;
            }
            catch (Exception ex)
            {
                result= false;
                errorResponseModel.Message = ex.Message;
            }
           return result;
        }

        public void AddTemplate(int MerchantId)
        {
            try
            {
                int ModeTypeId = 1;

                //var MerchantRefferal = (from template_ in context.Smstemplates
                //                        where template_.MerchantId == 0
                //                        select new
                //                        {
                //                            Messagecount = template_.MessageContent,
                //                        }
                //    ).FirstOrDefault();

                var MerchantRefferal = context.Smstemplates
                                  .Where(template => template.MerchantId == null)
                                  .Select(template => new
                                  {
                                      MerchantId = MerchantId,  // Assuming Merchant is the property representing the merchant
                                      MessageCount = template.MessageContent,
                                      ModeId = template.ModeTypeId,
                                     MessageTypeId= template.MessageTypeId,
                                     MessageContent = template.MessageContent,
                                     RecStatus= template.RecStatus,
                                     CreatedBy = template.CreatedBy,
                                     CreatedDate = template.CreatedDate,
                                  }).ToList();



                foreach (var merchantGroup in MerchantRefferal)
                {
                    
                        var newTemplate = new Smstemplate
                        {
                            MerchantId = MerchantId,  // Set the new merchant id
                            MessageContent = merchantGroup.MessageCount,
                            ModeTypeId = merchantGroup.ModeId,
                            MessageTypeId = merchantGroup.MessageTypeId,
                            RecStatus = merchantGroup.RecStatus,
                            CreatedBy = merchantGroup.CreatedBy,
                            CreatedDate = DateTime.Now,

                            // Set other properties as needed
                        };

                        context.Smstemplates.Add(newTemplate);
                    
                }

                context.SaveChanges();


                //var MerchantRefferalsignup = (from template_ in context.Smstemplates
                //                        where template_.MerchantId == 0 && template_.ModeTypeId==1
                //                        select new
                //                        {
                //                            Messagecount = template_.MessageContent,
                //                        }
                //  ).FirstOrDefault();

                //for (int i = ModeTypeId; i <= 4; i++)
                //{
                //    string MessageContent = "";

                //    if (i == 1)
                //    {  

                //        MessageContent = "Congratulations, [User's Name]! You've successfully signed up for BlooTusk. Get ready to unlock exciting rewards! Stay tuned for your personalized referral link.";

                //    }
                //    else if (i == 2)
                //    {
                //        ModeTypeId = 2;
                //        MessageContent = @"Hey [User], thanks for joining us at [Merchant Name]! Now, you can help your friends discover this awesome place too. Use the template below to share the love and spread the word: ""Hi [Friend's Name], I just discovered a great place called [Merchant Name]! Sign up using my referral link [Referral Link] and you'll get [Discount/Coupon Of er] on your first visit. This discount/coupon of ers [specific discount/coupon value] and is valid until [validity period]. Make sure to check the terms and conditions for any additional details. But that's not all – you'll also earn [Reward Points] BlooTusk reward points for each friend you refer. Join the BlooTusk community now! Let's both enjoy the benefits together!";
                //    }

                //    else if (MerchantRefferal == null && i ==3)
                //    {
                //        MessageContent = @"Great news! You've successfully referred your [friend's name] to BlooTusk.You've earned 200 Reward points. Thank you for spreading the word and sharing the benefits!";

                //        ModeTypeId = 0;
                //    }

                //    else if(MerchantRefferalsignup == null && i==4)
                //    {
                //         MessageContent = "Congratulations, [MerchantName]! You're now part of Blootusk. Your Password is, [Password] And UserName is [UserName]";
                //        ModeTypeId = 1;
                //    }

                //    Smstemplate smstemplate = new Smstemplate();
                //    smstemplate.MessageContent = MessageContent;
                //    smstemplate.MessageTypeId = 1;
                //    smstemplate.ModeTypeId = ModeTypeId;
                //    smstemplate.MerchantId = MerchantId;
                //    smstemplate.RecStatus = "A";
                //    smstemplate.CreatedBy = 1;

                //    smstemplate.CreatedDate = DateTime.Now;
                //    smstemplate.ModifyDate = DateTime.Now;

                //    context.Smstemplates.Add(smstemplate);
                //    context.SaveChanges();

                //}
            }
            catch (Exception ex)
            {

                throw;
            }

        }
        public void AddReward(int MerchantId)
        {
            try
            {
                int RewardTypeID = 1;
                int RewardPoint = 0;


                for (int i = RewardTypeID; i <= 2; i++)
                {
                    //signup =1
                    if (i == 1)
                    {
                         RewardPoint = 100;
                    }
                    else if (i == 2)
                    {
                        RewardPoint = 200;
                        RewardTypeID = 2;
                    }


                    Rewardpointmaster rewardPointmaster = new Rewardpointmaster();
                    rewardPointmaster.RewardPoint = RewardPoint;
                    rewardPointmaster.RewardTypeId = RewardTypeID;
                    rewardPointmaster.Validity = 3;
                    rewardPointmaster.IssuedBy =  1;
                    rewardPointmaster.RewardDate = DateTime.Now.ToShortDateString();
                    rewardPointmaster.IsAdmin = 1;
                    rewardPointmaster.MerchantId = MerchantId;
                    rewardPointmaster.RecStatus = 1;
                    rewardPointmaster.CreatedBy = 1;
                    rewardPointmaster.CreatedDate = DateTime.Now;
                    context.Add(rewardPointmaster);
                    context.SaveChanges();
                 
                    
                }
            }
            catch (Exception ex)
            {

                throw;
            }

        }       
                            
        public string GenerateUniqueUserCode(string merchantCode)
            {

            string newCodes = string.Empty;

            var lastusercode = (from Merchantsystemuser_ in context.Merchantsystemusers
                                join merchant_ in context.Merchants on Merchantsystemuser_.MerchantId equals merchant_.MerchantId
                                where merchant_.MerchantCode == merchantCode
                                orderby Merchantsystemuser_.UserId descending // Replace SomeProperty with the property you want to order by
                                select new
                                {
                                    Merchantsystemuser_.UserId
                                }).LastOrDefault();

            if (lastusercode != null)
                {
                // If no last saved user code, initialize it to the merchant code
            

                int result = Convert.ToInt32(lastusercode.ToString().Substring(10, 2));

                if (result <= 99)
                {
                    string NewR = lastusercode.ToString().Substring(0, 10) +""+( result < 10 ? ("0" + (result + 1)) : (result + 1).ToString());

                    newCodes = NewR;
                }
                else
                {
                    string message = "We can Not Allow the merchant User Greter tha 99";
                }
            }

              

            else
            {

                int result = 0;

                newCodes = merchantCode.ToString() + "0" + 1;

            }
            return newCodes;
            }

        
        private string GenerateMearchantUserID()
        {
            string result = string.Empty;
            var merchantUserData = (from merchantUser in context.Merchantsystemusers
                                where merchantUser.IsAdmin == 0
                                orderby merchantUser.MerchantSystemUserId descending
                                select merchantUser).ToList();

            if (merchantUserData.Count > 0 )
            {
                int userId = 0;
                foreach (var item in merchantUserData)
                {
                    if (item.UserId.Substring(item.UserId.Length - 1).Equals("1"))
                    {
                        var numricString = item.UserId.Replace("M", "").Trim();
                        var code = Convert.ToInt32(numricString.Substring(0,numricString.Length - 3).Trim());
                        userId = code + 1;
                        result = "M" + userId+"001";
                        break;
                    }
                    
                }
            }
            else
            {
                result = "M1001001";
            }

            return result;
        }
        private string GenerateMearchantCode()
        {
            string result = string.Empty;
            var merchantData = (from merchant in context.Merchants
                                orderby merchant.MerchantId descending
                                select merchant).FirstOrDefault();
            int mrchantCode = 0;
            if (merchantData == null)
            {
                result = "M"+1110001;
            }
            else
            {
                var code = Convert.ToInt32(merchantData.MerchantCode.Replace("M", "").Trim());
                mrchantCode = code + 1;
                result = "M" + mrchantCode;
            }
            return result;
        }
        public bool DeleteMerchant(int merchantId, ref ErrorResponseModel errorResponseModel)
        {
            bool result = false;
            var merchantData=context.Merchants.SingleOrDefault(x=>x.MerchantId == merchantId);
            if (merchantData != null)
            {
                merchantData.RecStatus = "I";
                context.SaveChanges();
                result= true;
            }
            return result;
        }

        public bool EditMerchant(MerchantModel merchantModel, ref ErrorResponseModel errorResponseModel)
        {
            bool result = false;
            var phoneNumber = "";
            var merchantData = context.Merchants.FirstOrDefault(x => x.MerchantId == merchantModel.MerchantId);
            var MerchantUserData = context.Merchantsystemusers.FirstOrDefault(x => x.MerchantId == merchantModel.MerchantId);
            var posData = context.Pos.SingleOrDefault(x => x.Posid == merchantModel.POSInfo.Posid);

            string ToEmail = Convert.ToBase64String(CommonUtility.Decrypt(MerchantUserData.Email, configuration.SymmetricKey));

            if (merchantData != null)
            {
                phoneNumber=merchantData.PhoneNumber;
                merchantData.OrganizationName =merchantModel.OrganizationName;
                merchantData.PhoneNumber = merchantModel.PhoneNumber;
                merchantData.Email = Convert.ToBase64String(CommonUtility.Encrypt(merchantModel.Email, configuration.SymmetricKey));
                merchantData.City = merchantModel.City;
                merchantData.ContactPersonName =merchantModel.ContactPersonName;
                merchantData.ApprovalStatus =merchantModel.ApprovalStatus;
                merchantData.RecStatus =merchantModel.RecStatus;
                merchantData.Password =merchantModel.Password;
                merchantData.ModifyBy =merchantModel.ModifyBy;
                merchantData.ModifyDate = merchantModel.ModifyDate;
                context.SaveChanges();

                if(posData != null)
                {
                    posData.Posname = merchantModel.POSInfo.Posname;
                    posData.CategoryId = merchantModel.POSInfo.CategoryId;
                    posData.Posaddress = merchantModel.POSInfo.Posaddress;
                    posData.Zip = merchantModel.POSInfo.Zip;
                    posData.StateId = merchantModel.POSInfo.StateId;
                    posData.CountryId = merchantModel.POSInfo.CountryId;
                    posData.Latitude= merchantModel.POSInfo.Latitude;
                    posData.Longitude= merchantModel.POSInfo.Longitude;
                    context.SaveChanges();
                }

                if(merchantModel.remark!=string.Empty)
                {
                   var remarkData=new Remarkhistory();
                    remarkData.Remark =merchantModel.remark;
                    remarkData.ApprovalStatus = merchantData.ApprovalStatus;
                    remarkData.MerchantId = merchantModel.MerchantId;
                    remarkData.RemarkDate = DateTime.Now;
                    context.Remarkhistories.Add(remarkData);
                    context.SaveChanges();
                }


                if(MerchantUserData != null)
                {
                    phoneNumber = merchantData.PhoneNumber;
                    MerchantUserData.Name = merchantModel.OrganizationName;
                    MerchantUserData.PhoneNumber = merchantModel.PhoneNumber;
                    MerchantUserData.Email = Convert.ToBase64String(CommonUtility.Encrypt(merchantModel.Email, configuration.SymmetricKey));
                    MerchantUserData.RecStatus = merchantModel.RecStatus;
                    MerchantUserData.Password = merchantModel.Password;
                    MerchantUserData.ModifyBy = merchantModel.ModifyBy;
                    MerchantUserData.ModifyDate = merchantModel.ModifyDate;
                    context.SaveChanges();
                }

                result = true;
            }


            string emailbodyMerchant = (from template in context.Smstemplates
                                        where template.MessageTypeId == 1 && template.ModeTypeId == 2 && template.MerchantId == null
                                        select template.MessageContent)
            .FirstOrDefault();


            string emailbodyMerchantVer = emailbodyMerchant
        .Replace("{CustomerName}", merchantModel.OrganizationName);   


            if (merchantModel.ApprovalStatus == "V")
            {
                EmailModel emailModel = new EmailModel();
                emailModel.ReceiverEmail = merchantModel.Email;
                emailModel.Subject = "BlooTusk Verification";
                emailModel.EmailBody = emailbodyMerchantVer;
                emailModel.EmailSettings = emailSettings;
                CommonUtility.SendEmail(emailModel);
            }
            return result;
        }



        public MerchantSearchResultModel GetAllMerchant(MerchantSerachModel merchantSerachModel, ref ErrorResponseModel errorResponseModel)
        {           

            var merchantList = new MerchantSearchResultModel();
            var pageNumber = (merchantSerachModel.PageNumber <= 0) ? 1 : merchantSerachModel.PageNumber;
            var pageSize = 10;
            var totalRecords = 0.0;
            var totalPages = 0.0;
            var skip = 0;

           var phonenoencrypt =   GetEncryptedString(merchantSerachModel.Keyword);

            totalRecords = (from merchant in context.Merchants
                            where ((string.IsNullOrEmpty(merchantSerachModel.Keyword) || merchant.OrganizationName.Contains(merchantSerachModel.Keyword, StringComparison.OrdinalIgnoreCase)) ||
                                           merchant.MerchantCode.Contains(merchantSerachModel.Keyword, StringComparison.OrdinalIgnoreCase) ||
                                           merchant.PhoneNumber.Contains(phonenoencrypt, StringComparison.OrdinalIgnoreCase) ||
                                           merchant.Email.Contains(phonenoencrypt, StringComparison.OrdinalIgnoreCase) ||
                                           merchant.ApprovalStatus.Contains(merchantSerachModel.Keyword, StringComparison.OrdinalIgnoreCase))

                            //  (merchant => merchant.MerchantCode.Contains(keyword, StringComparison.OrdinalIgnoreCase));
                            //&& merchant.RecStatus=="A"

                            select new MerchantModel
                            {
                                MerchantId = merchant.MerchantId,
                                MerchantCode = merchant.MerchantCode,
                                OrganizationName = merchant.OrganizationName,
                                PhoneNumber = CommonUtility.maskString(Encoding.UTF8.GetString(CommonUtility.Decrypt(merchant.PhoneNumber, configuration.SymmetricKey)), "PhoneNUmber"),
                                Email = CommonUtility.maskString(Encoding.UTF8.GetString(CommonUtility.Decrypt(Convert.FromBase64String(merchant.Email), configuration.SymmetricKey)), "Email"),
                                ApprovalStatus = merchant.ApprovalStatus.Equals("N") ? "New" : merchant.ApprovalStatus.Equals("I") ? "InProgress" : merchant.ApprovalStatus.Equals("V") ? "Verified" : "Rejected",
                                RecStatus = merchant.RecStatus,
                                GeneratedBy = merchant.CreatedBy.Equals(0) ? "Merchant" : "Admin",
                            }).ToList().Count;
            totalPages = Math.Ceiling((double)totalRecords / pageSize);
            skip = (pageNumber - 1) * pageSize;

            if (!String.IsNullOrEmpty(merchantSerachModel.Keyword))
            {

                var customerDAta = (from merchant in context.Merchants
                                    where ((string.IsNullOrEmpty(merchantSerachModel.Keyword) || merchant.OrganizationName.Contains(merchantSerachModel.Keyword, StringComparison.OrdinalIgnoreCase)) ||
                                           merchant.MerchantCode.Contains(merchantSerachModel.Keyword, StringComparison.OrdinalIgnoreCase) ||
                                           merchant.PhoneNumber.Contains(phonenoencrypt, StringComparison.OrdinalIgnoreCase) ||
                                           merchant.Email.Contains(phonenoencrypt, StringComparison.OrdinalIgnoreCase) ||
                                           merchant.ApprovalStatus.Contains(merchantSerachModel.Keyword, StringComparison.OrdinalIgnoreCase))

                                    select new MerchantModel
                                    {
                                        MerchantId = merchant.MerchantId,
                                        MerchantCode = merchant.MerchantCode,
                                        OrganizationName = merchant.OrganizationName,
                                        PhoneNumber = CommonUtility.maskString(Encoding.UTF8.GetString(CommonUtility.Decrypt(merchant.PhoneNumber, configuration.SymmetricKey)), "PhoneNUmber"),
                                        Email = CommonUtility.maskString(Encoding.UTF8.GetString(CommonUtility.Decrypt(Convert.FromBase64String(merchant.Email), configuration.SymmetricKey)), "Email"),
                                        ApprovalStatus = merchant.ApprovalStatus.Equals("N") ? "New" : merchant.ApprovalStatus.Equals("I") ? "InProgress" : merchant.ApprovalStatus.Equals("V") ? "Verified" : "Rejected",
                                        RecStatus = merchant.RecStatus,
                                        GeneratedBy = merchant.CreatedBy.Equals(0) ? "Merchant" : "Admin",

                                    }).ToList();//.Skip(skip).Take(pageSize);

                var customerData = customerDAta.OrderBy(i => i.CreatedDate);

                merchantList.MerchantList = customerData.ToList();
                merchantList.PageCount = totalPages;
            }
            else
            {
                var customerDAta = (from merchant in context.Merchants

                                        // && merchantUser_.Name == merchantUserSearchModel.name 
                                    select new MerchantModel
                                    {
                                        MerchantId = merchant.MerchantId,
                                        MerchantCode = merchant.MerchantCode,
                                        OrganizationName = merchant.OrganizationName,
                                        PhoneNumber = CommonUtility.maskString(Encoding.UTF8.GetString(CommonUtility.Decrypt(merchant.PhoneNumber, configuration.SymmetricKey)), "PhoneNUmber"),
                                        Email = CommonUtility.maskString(Encoding.UTF8.GetString(CommonUtility.Decrypt(Convert.FromBase64String(merchant.Email), configuration.SymmetricKey)), "Email"),
                                        ApprovalStatus = merchant.ApprovalStatus.Equals("N") ? "New" : merchant.ApprovalStatus.Equals("I") ? "InProgress" : merchant.ApprovalStatus.Equals("V") ? "Verified" : "Rejected",
                                        RecStatus = merchant.RecStatus,
                                        GeneratedBy = merchant.CreatedBy.Equals(0) ? "Merchant" : "Admin",
                                    }).ToList();//.Skip(skip).Take(pageSize);
                var customerData = customerDAta.OrderBy(i => i.CreatedDate);

                merchantList.PageCount = totalPages;
                merchantList.MerchantList = customerData.ToList();

            }


          

            return merchantList;

        }
         
        public MerchantModel GetMerchantById(int merchantId, ref ErrorResponseModel errorResponseModel)
        {
           var marchantData = (from
                               //Cmm in context.Customermerchantmappers
                               //join 
                               merchant in context.Merchants  
                               //on Cmm.MerchantId equals merchant.MerchantId
                               where merchant.MerchantId == merchantId
                                
                                select new MerchantModel 
                                {
                                    MerchantId = merchant.MerchantId,
                                    MerchantCode = merchant.MerchantCode,
                                    OrganizationName = merchant.OrganizationName,
                                    Email = merchant.Email,
                                    PhoneNumber = merchant.PhoneNumber,
                                    ContactPersonName = merchant.ContactPersonName,
                                    Password = merchant.Password,
                                    ApprovalStatus = merchant.ApprovalStatus,
                                    RecStatus = merchant.RecStatus,
                                    City = merchant.City,
                                    MerchantURL = configuration.MerchantSignUpURL + Convert.ToBase64String(Encoding.UTF8.GetBytes(Convert.ToBase64String(CommonUtility.Encrypt(merchant.MerchantId.ToString() + "M", configuration.SymmetricKey))))
                               }
                            ).FirstOrDefault();

            if ( marchantData != null )
            {
                var encryptedMerchantCode = GetEncryptedString(marchantData.MerchantCode);
                
                var posData = (from pos in context.Pos
                               where pos.MerchantId == merchantId
                               select new POSModel
                               {
                                   Posid = pos.Posid,
                                   Poscode = pos.Poscode,
                                   Posname = pos.Posname,
                                   Posaddress = pos.Posaddress,
                                   CategoryId = pos.CategoryId,
                                   StateId = pos.StateId,
                                   CountryId = pos.CountryId,
                                   Zip = pos.Zip,
                                   Latitude = pos.Latitude,
                                   Longitude = pos.Longitude,
                               }).SingleOrDefault();
                marchantData.POSInfo = posData;
  
            }

            var templateList = (from smsTemplate in context.Smstemplates
                                join type in context.Messagetypemasters
                                     on smsTemplate.MessageTypeId equals type.MessageTypeId
                                where smsTemplate.MerchantId == merchantId

                                select new SmsTemplateModel
                                {
                                    MessageType = type.MessageType,
                                    MessageContent = smsTemplate.MessageContent,
                                    RecStatus = smsTemplate.RecStatus,
                                    TemplateId = smsTemplate.TemplateId,

                                }).ToList();

             marchantData.smstemplateList = templateList;


     

            var rewardPointList = from rpm in context.Rewardpointmasters
                                  join rtm in context.Rewardtypemasters on rpm.RewardTypeId equals rtm.RewardTypeId
                                  where rpm.MerchantId == merchantId && (rpm.RewardTypeId == 1 || rpm.RewardTypeId == 2)
                                  select new RewardPointModel
                                  {
                                      RewardPonitId = rpm.RewardPonitId,
                                      RewardDate = rpm.RewardDate,
                                      RewardPoint = rpm.RewardPoint,
                                      RewardType = rtm.RewardType,
                                      Validity = rpm.Validity,
                                      IssuedByName = rpm.IssuedBy.Equals(0) ? "Merchant" : "BlooTusk Team"
                                  };

            marchantData.RewardPointlist = rewardPointList.ToList();

            return marchantData;
        }


        static string GetEightDigitHash(string input)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));

                // Take the first 4 bytes to ensure a 8-character representation
                byte[] truncatedHash = new byte[4];
                Array.Copy(hashBytes, truncatedHash, 4);

                // Convert to string representation
                string hashString = BitConverter.ToString(truncatedHash).Replace("-", "");

                // Take only the first 8 characters
                return hashString.Substring(0, 8);
            }
        }

        public PosDetailsModel GetPosDetail(int merchantID, ref ErrorResponseModel errorResponseModel)
        {
            var posData = (from pos in context.Pos 
                                join category in context.Categorymasters on pos.CategoryId equals category.CategoryId
                                join state in context.Statemasters on pos.StateId equals state.StateId
                                join country in context.Countrymasters on pos.CountryId equals country.CountryId
                                where pos.MerchantId == merchantID
                           select new PosDetailsModel
                           {
                                    
                                    Posname = pos.Posname,
                                    Posaddress = pos.Posaddress,
                                    Poscode = pos.Poscode,
                                    CategoryName = category.CategoryName,
                                    Zip = pos.Zip,
                                    StateName = state.StateName,
                                    CountryName = country.CountryName,
                                }
                             ).SingleOrDefault();
            return posData;

        }

        public ProfileModel GetProfile(int merchantId, ref ErrorResponseModel errorResponseModel)
        {
            var marchantData = (from merchant in context.Merchants
                                //join pos in context.Pos on merchant.MerchantId equals pos.MerchantId
                                //join category in context.Categorymasters on pos.CategoryId equals category.CategoryId
                                //join state in context.Statemasters on pos.StateId equals state.StateId
                                //join country in context.Countrymasters on pos.CountryId equals country.CountryId
                                where merchant.MerchantId == merchantId
                                select new ProfileModel
                                {
                                    MerchantId = merchant.MerchantId,
                                    MerchantCode = merchant.MerchantCode,
                                    OrganizationName = merchant.OrganizationName,
                                    Email = merchant.Email,
                                    PhoneNumber = merchant.PhoneNumber,
                                    ContactPersonName = merchant.ContactPersonName,
                                    //Posname=pos.Posname,
                                    //Posaddress=pos.Posaddress,
                                    //Poscode=pos.Poscode,
                                    //CategoryName= category.CategoryName,
                                    //Zip=pos.Zip,
                                    //StateName=state.StateName,
                                    //CountryName=country.CountryName,
                                }
                             ).SingleOrDefault();
            return marchantData;
        }

        public bool UpdateProfile(ProfileModel profileModel, ref ErrorResponseModel errorResponseModel)
        {
            bool result = false;

            var merchantData = context.Merchants.SingleOrDefault(x => x.MerchantId == profileModel.MerchantId);
            if (merchantData != null)
            {
                merchantData.PhoneNumber = profileModel.PhoneNumber;// Convert.ToBase64String(CommonUtility.Encrypt(profileModel.PhoneNumber, configuration.SymmetricKey));
                merchantData.ModifyBy = profileModel.modifyBy;
                merchantData.ModifyDate = profileModel.ModifyDate;
                context.SaveChanges();
                result = true;
            }

            return result;
        }
        public OTPModel SendOTP(OTPModel oTPModel, ref ErrorResponseModel errorResponseModel)
        {
            bool success = false;
            Random generator = new Random();
            String randomNumber = generator.Next(1, 1000000).ToString("D6");
            string message = "\r\nHello User,\r\nThank you for choosing BlooTusk. Use this OTP " + randomNumber + " to complete your sign up procedures and verify your account on BlooTusk";
            bool result=SendSms(oTPModel.PhoneNumber, message);

            String randomNumberForEmail = generator.Next(1, 1000000).ToString("D6");
            string emailMessage = "\r\nHello User,\r\nThank you for choosing BlooTusk. Use this OTP " + randomNumberForEmail + " to complete your sign up procedures and verify your account on BlooTusk \n\r Regards,\n Team BlooTusk";
            
            EmailModel emailModel = new EmailModel();
            emailModel.ReceiverEmail = oTPModel.Email;
            emailModel.Subject = "BlooTusk Otp Verification";
            emailModel.EmailBody = emailMessage;
            emailModel.EmailSettings = emailSettings;
            success =CommonUtility.SendEmail(emailModel);

            return new OTPModel { 
                PhoneNumber = oTPModel.PhoneNumber,
                Email = oTPModel.Email,
                PhoneNumberOTP=randomNumber,
                EmailOTP= randomNumberForEmail,
            };
        }
        private bool SendSms(string phonenumber,  string message)
        {
            string ToNumber = string.Empty;
            bool result=false;
            using (var web = new WebClient())
            {
                try
                {
                    if(configuration.Bypassgetway == true)
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
                    errorlog.ErrorLog1 = "Error In sending sms to "+ ToNumber + " Error Message is " + ex.Message;
                    context.Errorlogs.Add(errorlog);
                    context.SaveChanges();
                }
            }
            return result;
        }

        public bool SendEmail(string email, string subject, string message)
        {
            bool result=false;
            try
            {
                string toEmail = string.IsNullOrEmpty(email) ? emailSettings.ToEmail: email;
                MailMessage mail = new MailMessage()
                {
                    From = new MailAddress(emailSettings.UsernameEmail, "BlooTusk")
                };
                mail.To.Add(new MailAddress(toEmail));
                mail.Subject = subject;
                mail.Body = message;
                mail.IsBodyHtml = true;
                mail.Priority = MailPriority.High;

                try
                {
                    using (SmtpClient smtp = new SmtpClient(emailSettings.PrimaryDomain, emailSettings.PrimaryPort))
                    {
                        try
                        {
                            smtp.Credentials = new NetworkCredential(emailSettings.UsernameEmail, emailSettings.UsernamePassword);
                            smtp.EnableSsl = emailSettings.EnableSsl;
                            smtp.UseDefaultCredentials = emailSettings.UseDefaultCredentials;
                            //smtp.EnableSsl = false;
                            //smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                            //smtp.UseDefaultCredentials = false;
                            //await smtp.SendMailAsync(mail);
                            //await Task.Run(async () => await smtp.SendMailAsync(mail));
                            smtp.Send(mail);
                            result=true;
                        }
                        catch (SmtpException ex)
                        {

                            result=false;
                        }
                    }
                }
                catch (SmtpException ex)
                {
                    result = false;
                }
            }
            catch (SmtpException ex)
            {
                //do something here
                result = false;
                //await Task.FromResult(ex.InnerException.Message);
                //ex.Message;
                //this.LogError(ex);
            }
            return result;
        }
        public List<CountryModel> GetCountryDDL(ref ErrorResponseModel errorResponseModel)
        {

            errorResponseModel = new ErrorResponseModel();

            var CountryList = (from country in context.Countrymasters
                                select new CountryModel
                                {
                                    CountryId = country.CountryId,
                                    CountryName = country.CountryName,
                                }
                               ).Distinct().ToList();

            if (CountryList == null)
            {
                errorResponseModel.StatusCode = HttpStatusCode.NotFound;
                errorResponseModel.Message = "No Data found";
            }          
            return CountryList;
        }

        public List<StateModel> GetStateDDL(ref ErrorResponseModel errorResponseModel)
        {

            errorResponseModel = new ErrorResponseModel();

            var stateList = (from stateMaster in context.Statemasters
                               
                                select new StateModel
                                {
                                    StateId = stateMaster.StateId,
                                    StateName = stateMaster.StateName,
                                }
                               ).Distinct().ToList();

            if (stateList == null)
            {
                errorResponseModel.StatusCode = HttpStatusCode.NotFound;
                errorResponseModel.Message = "No Data found";
            }
            
            return stateList;
        }

        public DashboardDataModel GetDashboardData(int merchantId, ref ErrorResponseModel errorResponseModel)
        {
            var dashboardData = new DashboardDataModel();
            try
            {

               var userlist = (from customer_ in context.Customers
                               join cmmaaper in context.Customermerchantmappers on customer_.CustomerId equals cmmaaper.CustomerId
                               where cmmaaper.MerchantId == merchantId && customer_.RecStatus=="A"
                               select customer_).ToList();


               
                var tiedUsers = userlist.Count(X=>X.RecStatus=="A");

                dashboardData.SignUpUsers = 0;
                dashboardData.TiedUsers = tiedUsers;
                dashboardData.ClosedReffralUsers = 0;
                dashboardData.RepeatedUser = 0;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return dashboardData;        
        }

        public MerchantInfoModel GetMerchantByCOde(string merchantCode, bool isMerchant, ref ErrorResponseModel errorResponseModel)
        {
            string decryptedMerchantCode = string.Empty;
          
            //if(!merchantCode.Contains("M111"))
            //{              
            //   byte[] data = Convert.FromBase64String(merchantCode);
            //   decryptedMerchantCode = Encoding.UTF8.GetString(data);
            //}
            //else
            //{
            //   decryptedMerchantCode = merchantCode;
            //}          
            
            decryptedMerchantCode = merchantCode;
    
                var merchantInfoModel = isMerchant
                    ? (from merchant in context.Merchants

                       where merchant.MerchantId == Convert.ToInt32(decryptedMerchantCode)
                       select new MerchantInfoModel
                       {
                           MerchantID = merchant.MerchantId,
                           OrganizationName = merchant.OrganizationName,
                           MerchantCode = merchant.MerchantCode,
                       }).SingleOrDefault()
                    : (from CustMerMapper in context.Customermerchantmappers
                       join merchant in context.Merchants on CustMerMapper.MerchantId equals merchant.MerchantId
                       where CustMerMapper.UserMerchantMapperId == Convert.ToInt32(decryptedMerchantCode)
                       
                       select new MerchantInfoModel
                       {
                           MerchantID = merchant.MerchantId,
                           OrganizationName = merchant.OrganizationName,
                           MerchantCode = merchant.MerchantCode,
                       }).SingleOrDefault();

                return merchantInfoModel;
            }


            public bool AddRemark(RemarkModel remarkModel)
        {
            bool result=false;
            try
            {
                var remarkData = new Remarkhistory();
                remarkData.Remark = remarkModel.Remark;
                remarkData.MerchantId = remarkModel.MerchantID;
                remarkData.ApprovalStatus = remarkModel.ApprovalStatus;
                remarkData.RemarkDate = remarkModel.RemarkDate;
                context.Remarkhistories.Add(remarkData);
                context.SaveChanges();
                result = true;
            }
            catch (Exception)
            {
                result = false;
                throw;
            }
            return result;
        }

        public List<RemarkModel> GetRemarkHistory(int merchantId)
        {
            var remarkHistoryData = (from remarkHistory in context.Remarkhistories
                                     where remarkHistory.MerchantId == merchantId
                               select new RemarkModel
                               {
                                   MerchantID = remarkHistory.MerchantId,
                                   RemarkID = remarkHistory.RemarkHistoryId,
                                   RemarkDate =Convert.ToDateTime(remarkHistory.RemarkDate),
                                   Remark = remarkHistory.Remark,
                                   ApprovalStatus = remarkHistory.ApprovalStatus.Equals("N") ?"New": remarkHistory.ApprovalStatus.Equals("I") ? "InProgress" : remarkHistory.ApprovalStatus.Equals("V")? "Verified" : "Rejected",
                               }
                               ).Distinct().ToList();
            return remarkHistoryData;
        }

        public OTPModel ForgetPasswordOTP(string username)
        {
            Random generator = new Random();
            bool isEmailUsername = IsValidEmail(username);
            if (isEmailUsername)
            {
                String randomNumberForEmail = generator.Next(1, 1000000).ToString("D6");
                string emailBoday = "\r\nHello User,\r\n Your OTP for forgot password is " + randomNumberForEmail + " Use this otp to reset your password \r\n  Regards,\n Team BlooTusk";
                SendEmail(username, "BlooTusk Reset password OTP", emailBoday);
                return new OTPModel
                {
                    Email = username,
                    EmailOTP = randomNumberForEmail,
                };
            }
            else
            {
                String randomNumber = generator.Next(1, 1000000).ToString("D6");
                string message = "\r\nHello User,\r\n Your OTP for forgot password is " + randomNumber + " Use this otp to reset your password \r\n";
                SendSms(username, message);
                return new OTPModel
                {
                    PhoneNumber = username,
                    PhoneNumberOTP = randomNumber,
                };
            }
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var mail = new System.Net.Mail.MailAddress(email);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool ResetPassword(ResetPasswordModel resetPasswordModel)
        {
            bool result = false;

            try
            {              
                string encryptedUsername = Convert.ToBase64String(CommonUtility.Encrypt(resetPasswordModel.Username, configuration.SymmetricKey));
                var merchantData = new Merchant();
                var MerchantUserData = new Merchantsystemuser();
                var merchantDatauser = new Merchantsystemuser();

                if (resetPasswordModel.MerchantId != 0)
                {
                    //  merchantData = context.Merchants.SingleOrDefault(x => x.MerchantId == resetPasswordModel.MerchantId);
                    if (resetPasswordModel.Username != null)
                    {
                        MerchantUserData = context.Merchantsystemusers.SingleOrDefault(x => x.Email == encryptedUsername || x.PhoneNumber == encryptedUsername);
                    }
                    else
                    {
                        MerchantUserData = context.Merchantsystemusers.SingleOrDefault(x => x.MerchantId == resetPasswordModel.MerchantId);
                    }
                }
                else
                {
                    merchantData = context.Merchants.SingleOrDefault(x => x.Email == encryptedUsername || x.PhoneNumber == encryptedUsername);

                    MerchantUserData = context.Merchantsystemusers.SingleOrDefault(x => x.Email == encryptedUsername || x.PhoneNumber == encryptedUsername);
                }

                if (MerchantUserData.IsAdmin == 1 && MerchantUserData.CreatedBy == 0 || MerchantUserData.CreatedBy == null)
                {
               
                    if(resetPasswordModel.MerchantId != 0)
                    {
                        merchantData = context.Merchants.SingleOrDefault(x => x.MerchantId == resetPasswordModel.MerchantId);

                    }
                    else
                    {
                        merchantData = context.Merchants.SingleOrDefault(x => x.Email == encryptedUsername || x.PhoneNumber == encryptedUsername);
                    }

                    if (merchantData != null)
                    {
                        var password = Convert.ToBase64String(CommonUtility.Encrypt(resetPasswordModel.Password.Trim(), configuration.SymmetricKey));
                        merchantData.Password = password;
                        merchantData.ModifyDate = DateTime.Now;
                        context.SaveChanges();
                    }

                }
                if (MerchantUserData != null)
                {
                    var password = Convert.ToBase64String(CommonUtility.Encrypt(resetPasswordModel.Password.Trim(), configuration.SymmetricKey));

                    MerchantUserData.Password = password;
                    MerchantUserData.ModifyDate = DateTime.Now;
                    context.SaveChanges();
                    result = true;
                }
            }
            catch (Exception)
            {

                throw;
            }
            return result;
        }

        public bool IsDuplicateEmail(string email)
        {
            bool result = false;
            var merchantData = context.Merchants.FirstOrDefault(x=>x.Email == Convert.ToBase64String(CommonUtility.Encrypt(email, configuration.SymmetricKey)));
            if(merchantData != null)
            {
                result = true;
            }
            return result;
        }

        public bool IsDuplicateEmailStaff(string email)
        {
            bool result = false;
            var merchantData = context.Merchantsystemusers.FirstOrDefault(x => x.Email == Convert.ToBase64String(CommonUtility.Encrypt(email, configuration.SymmetricKey)));
            if (merchantData != null)
            {
                result = true;
            }
            return result;
        }

        public bool IsDuplicatePhoneNumberStaff(string phoneNumber)
        {

            bool result = false;
            var merchantData = context.Merchantsystemusers.FirstOrDefault(x => x.PhoneNumber == Convert.ToBase64String(CommonUtility.Encrypt(phoneNumber, configuration.SymmetricKey)));
            if (merchantData != null)
            {
                result = true;
            }
            return result;
        }

        public bool IsDuplicatePhoneNumber(string phoneNumber)
        {

            bool result = false;
            var merchantData = context.Merchants.FirstOrDefault(x => x.PhoneNumber == Convert.ToBase64String(CommonUtility.Encrypt(phoneNumber, configuration.SymmetricKey)));
            if (merchantData != null)
            {
                result = true;
            }
            return result;
        }

        private string GetDecryptedString(string value)
        {
            return Encoding.UTF8.GetString(CommonUtility.Decrypt(value, configuration.SymmetricKey));
        }

        public string GetEncryptedString(string value)
        {
            return Convert.ToBase64String(CommonUtility.Encrypt(value, configuration.SymmetricKey));
        }

        public PosDetailsModel GetPosDetails(int merchantId, ref ErrorResponseModel errorResponseModel)
        {
            throw new NotImplementedException();
        }

        public string NudgeCoupon(NudgeModel nudgeCouponModel, ref ErrorResponseModel errorResponseModel)
        {
            string result = "";
            try
            {

                List<int> IssueCouponList = new List<int>();

                List<int> RedeemCouponList = new List<int>();

                var customerIds = context.Couponissuedetails
                .Where(c => c.CouponId == nudgeCouponModel.CouponId)
                .Select(c => c.CustomerId)
                .ToList();


                var reddemcustomerIds = context.Redeemtions
                        .Join(
                            context.Couponissuedetails,
                            redemption => redemption.CouponIssuedetailsId,
                            issued => issued.CouponIssueDetailsId,
                            (redemption, issued) => new { Redemption = redemption, Issued = issued }
                        )
                        .Where(joined => joined.Issued.CouponId == nudgeCouponModel.CouponId)
                        .Select(joined => joined.Redemption.RedeembyCustomerId)
                        .ToList();



                List<int> SMSSendCustomerIds = customerIds.Except(reddemcustomerIds).ToList();

                // List<int> SMSSendCustomerIds = IssueCouponList.Except(RedeemCouponList).ToList();

                string Discount = nudgeCouponModel.DiscountType + nudgeCouponModel.DiscountValue;
                if(nudgeCouponModel.DiscountType == "percentage")
                {
                    Discount = nudgeCouponModel.DiscountType + "%";
                }
                else if(nudgeCouponModel.DiscountType == "dollar")
                {
                    Discount = nudgeCouponModel.DiscountType + "$";
                }

                string message = "Exclusive offer alert! Check out the latest coupon from "+ nudgeCouponModel.MerchantName + ". " +
                    "Show this message at store and enjoy " + Discount  + " off your purchase.BlooTusk";
                for (int i = 0; i < SMSSendCustomerIds.Count; i++)
                {
                    //var phoneNumbers = context.Customers
                    //    .Where(c => SMSSendCustomerIds.Contains(c.CustomerId))
                    //    .Select(c => c.PhoneNumber)
                    //    .ToList();

                    //for (int j = 0; j < phoneNumbers.Count; j++)
                    //{
                    //   var phoneno =  GetDecryptedString(phoneNumbers[j]);
                    //    SendSms(phoneno, message);
                    //}
                    var decryptedPhoneNumbers = context.Customers
                    .Where(c => SMSSendCustomerIds.Contains(c.CustomerId))
                    .Select(c => CommonUtility.Decrypt(c.PhoneNumber, configuration.SymmetricKey))
                    .ToList();

                    foreach (var phoneNumber in decryptedPhoneNumbers)
                    {
                        SendSms(Convert.ToString(phoneNumber), message);
                    }

                }
                result = "A";
            }
            catch (Exception)
            {
                result = "";
                throw;
            }
            return result;
        }


    

    }
}
