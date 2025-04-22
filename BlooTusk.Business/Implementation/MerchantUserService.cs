using BlooTusk.Business.Interface;
using BlooTusk.Common;
using BlooTusk.Entity.BlooTuskModel;
using BlooTusk.Model.Models;
using Google.Protobuf.WellKnownTypes;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Drawing;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.TwiML.Voice;
using Twilio.Types;
using static System.Net.Mime.MediaTypeNames;


namespace BlooTusk.Business.Implementation
{
    public class MerchantUserService : IMerchantUserService
    {
        BlooTuskContext context;
        private ConfigurationModel configuration;
        private EmailSettings emailSettings;


        private readonly string imageDirectory = Directory.GetCurrentDirectory();

        public MerchantUserService(BlooTuskContext _context, IOptions<ConfigurationModel> configuration, IOptions<EmailSettings> emailSettings)
        {
            context = _context;
            this.configuration = configuration.Value;
            this.emailSettings = emailSettings.Value;

        }
        public bool AddMerchantUser(MerchantUserModel merchantUserModel,int userID)
        {
            bool result = false;
            string userIDs = "";
            var merchantUserInfo = new Merchantsystemuser();
            var merchantData = (from merchantUser in context.Merchantsystemusers
                                where merchantUser.MerchantId== merchantUserModel.MerchantId
                                orderby merchantUser.MerchantSystemUserId descending
                                select merchantUser).FirstOrDefault();
            int mrchantCode = 0;
            if (merchantData == null)
            {
              //mrchantCode = 1110001;
              //Error message
            }
            else
            {
                var code = Convert.ToInt32(merchantData.UserId.Replace("M", "").Trim());
                mrchantCode = code + 1;
            }

            var merchantCode = (from merchant in context.Merchants
                                where merchant.MerchantId == merchantUserModel.MerchantId
                                orderby merchant.MerchantCode descending
                                select merchant).FirstOrDefault();

            if (merchantCode?.Merchantsystemusers?.Any(user => user.UserId != null) == true)
            {
                string? LastCode = merchantCode?.Merchantsystemusers?.FirstOrDefault()?.UserId;

                int results = Convert.ToInt32(LastCode.ToString().Substring(8, 2));
                 //01
                if (results <= 99)
                {
                    string NewR = LastCode.ToString().Substring(0, 8) + "" + (results < 9 ? ("0" + (results + 1)) : (results + 1));
                    userIDs = NewR;
                }
                else
                {
                    string message = "We can Not Allow the merchant User Greter tha 99";
                }
            }
            else
            {
                 userIDs = GenerateUniqueUserCode(merchantData.Merchant.MerchantCode);
            }

            if (merchantUserModel.Password == null)
            {
                merchantUserModel.Password = "12345678";
            }
            try
            {

                // Your base URL and image directory
                string baseUrl = "https://api.blootusk.com/";
                string imageDirectory = "user_images/";

                // Create the full image path on the server
                string imgPath = Path.Combine(imageDirectory, userIDs + ".jpg");
                // Check if the directory exists, if not, create it
                if (!Directory.Exists(imageDirectory))
                {
                    Directory.CreateDirectory(imageDirectory);
                }
                // Write the image bytes to the specified path
                File.WriteAllBytes(imgPath, merchantUserModel.UserImage);
                // Construct the URL for the saved image
                string imageUrl = baseUrl + imageDirectory + userIDs + ".jpg";


                //  merchantUserInfo.Image = merchantUserModel.Image;
                merchantUserInfo.MerchantId = merchantUserModel.MerchantId;
                merchantUserInfo.Email = Convert.ToBase64String(CommonUtility.Encrypt(merchantUserModel.Email.ToLower(), configuration.SymmetricKey));
                merchantUserInfo.Name = merchantUserModel.Name;

                if (merchantUserModel.UserImage != null && merchantUserModel.UserImage.All(b => b == 0))

                {
                    merchantUserInfo.ImageUrl = null;
                }
                else
                {
                    merchantUserInfo.ImageUrl = imageUrl;
                }

                merchantUserInfo.PhoneNumber = Convert.ToBase64String(CommonUtility.Encrypt(merchantUserModel.PhoneNumber, configuration.SymmetricKey));
                merchantUserInfo.Password = Convert.ToBase64String(CommonUtility.Encrypt(merchantUserModel.Password, configuration.SymmetricKey));
                merchantUserInfo.UserId = userIDs;
                merchantUserInfo.IsAdmin = (sbyte?)merchantUserModel.IsAdmin;
                merchantUserInfo.RecStatus = merchantUserModel.RecStatus;
                merchantUserInfo.CreatedBy = userID;
                merchantUserInfo.CreatedDate = DateTime.Now;
                context.Merchantsystemusers.Add(merchantUserInfo);
                context.SaveChanges();
                result = true;


                string message1 = "\"Congratulations, " + merchantUserInfo.Name + "! You're now part of Blootusk." +

                 "Hello " + merchantUserInfo.Name + "! Your UserName is " + merchantUserInfo.UserId + " ";

                string custPhoneNumber = GetDecryptedString(merchantUserInfo.PhoneNumber.Trim());
                //   bool smsStatus = SendSms(custPhoneNumber, message);
                bool smsStatus1 = SendSms(custPhoneNumber, message1);

            
            }
            catch (Exception ex)
            {
                result = false;               
            }
            return result;
        }
        private Dictionary<string, string> lastSavedUserCodes = new Dictionary<string, string>();
        private string GetDecryptedString(string value)
        {
            return Encoding.UTF8.GetString(CommonUtility.Decrypt(value, configuration.SymmetricKey));
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
                    string NewR = lastusercode.ToString().Substring(0, 10) + "" + (result < 10 ? ("0" + result + 1) : (result + 1).ToString());

                    newCodes = NewR;
                }
                else
                {
                    string message = "We can Not Allow the merchant USer Greter tha 99";
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

            if (merchantUserData.Count > 0)
            {
                int userId = 0;
                foreach (var item in merchantUserData)
                {
                    if (item.UserId.Substring(item.UserId.Length - 1).Equals("1"))
                    {
                        var numricString = item.UserId.Replace("M", "").Trim();
                        var code = Convert.ToInt32(numricString.Substring(0, numricString.Length - 3).Trim());
                        userId = code + 1;
                        result = "M" + userId + "001";
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
        public MerchantUserModel GetProfile(string userId)
        {
            var merchantData = (from merchantUser in context.Merchantsystemusers
                                where merchantUser.UserId == userId

                                select new MerchantUserModel
                                { 
                                    MerchantSystemUserId = merchantUser.MerchantSystemUserId,
                                    MerchantId = merchantUser.MerchantId,
                                    Name= merchantUser.Name,
                                    PhoneNumber= merchantUser.PhoneNumber,
                                    UserId = merchantUser.UserId,
                                
                                }).FirstOrDefault();
            return merchantData;
        }
        public bool UpdateProfile(MerchantUserModel userModel,int userId)
        {
            bool result = false;

            var merchantUserData = context.Merchantsystemusers.Where(x => x.UserId == userModel.UserId).FirstOrDefault();
            if (merchantUserData != null)
            {
                merchantUserData.PhoneNumber = userModel.PhoneNumber;
                merchantUserData.ModifyBy = userId;
                merchantUserData.ModifyDate = DateTime.Now;
                context.SaveChanges();

                var data = context.Merchants.FirstOrDefault(x => x.MerchantId == userModel.MerchantId);
                if (data != null)
                {
                    data.PhoneNumber = userModel.PhoneNumber;
                    data.Email = userModel.Email;
                    data.ModifyBy = userModel.MerchantId;
                    data.ModifyDate = DateTime.Now;
                    context.SaveChanges();
                }
                result = true;
            }
            return result;
        }
        public bool IsDuplicatePhoneNumber(string phoneNumber,int merchantId, int MerchantSystemUserId, bool merchantFlag)
        {
            bool result = false;
            if( merchantFlag == true)
            {
             var merchantData = context.Merchantsystemusers.FirstOrDefault(x =>
              x.MerchantId == merchantId &&  x.PhoneNumber == Convert.ToBase64String(CommonUtility.Encrypt(phoneNumber, configuration.SymmetricKey)));

                if (merchantData != null)
                {
                    result = true;
                }
            }
            else
            {
                var merchantData = context.Merchantsystemusers.FirstOrDefault(x =>
                                      x.MerchantId == merchantId
                                      && x.MerchantSystemUserId != MerchantSystemUserId
                                      && x.PhoneNumber == Convert.ToBase64String(CommonUtility.Encrypt(phoneNumber, configuration.SymmetricKey)));

                if (merchantData != null)
                {
                    result = true;
                }

             
            }

        
            return result;
        }
        public MerchantCouponListModel GetMerchantCouponList(int MerchantID)
        {
            var merchantCouponList = new MerchantCouponListModel();
            var CouponList = (from Couponmaster_ in context.Couponmasters
                              join CouponIssueMaster_ in context.Couponissuemasters on Couponmaster_.CouponId equals CouponIssueMaster_.CouponId
                              join couponIssueDetail_ in context.Couponissuedetails on CouponIssueMaster_.CouponIssuemasterId equals couponIssueDetail_.CouponIssueMasterId
                              join merchant_ in context.Merchants on Couponmaster_.MerchantId equals merchant_.MerchantId
                              join pos_ in context.Pos on merchant_.MerchantId equals pos_.MerchantId
                              where Couponmaster_.MerchantId == MerchantID
                              select new CouponListModel
                              {
                                  CouponTitle = Couponmaster_.CouponTitle,
                                  CouponDiscerption = Couponmaster_.CouponDiscerption,
                                  CreatedDate = Couponmaster_.CreatedDate,
                                  MerchantDetail = merchant_.OrganizationName + "," + pos_.Posaddress,
                                  DiscountType = Couponmaster_.DiscountType,
                                  DiscountValue = Couponmaster_.DiscountValue,
                                  Status = Couponmaster_.Status,
                                  CouponCode = couponIssueDetail_.CouponSerialNo,
                                 
                              }).ToList();

            merchantCouponList.BlooTuskRewardCount = (from rewardpointtransaction_ in context.Rewardpointtransactions
                         join rewardpointmaster_ in context.Rewardpointmasters on rewardpointtransaction_.RewardPointId equals rewardpointmaster_.RewardPonitId
                         where rewardpointtransaction_.MerchantId == MerchantID && rewardpointmaster_.RewardTypeId == 2
                                                      select rewardpointmaster_.RewardPoint).Sum();
           
           // merchantCouponList.BlooTuskRewardCount = 0;
            merchantCouponList.BlooTuskRedeemedCount = (from rewardpointtransaction_ in context.Rewardpointtransactions
                                                        join rewardpointmaster_ in context.Rewardpointmasters on rewardpointtransaction_.RewardPointId equals rewardpointmaster_.RewardPonitId
                                                        where rewardpointtransaction_.MerchantId == MerchantID && rewardpointmaster_.RewardTypeId == 3
                                                        select rewardpointmaster_.RewardPoint).Sum();


                    merchantCouponList.CouponsredeemCount = (
                     from CouponMaster_ in context.Couponmasters
                     join couponIssued_ in context.Couponissuemasters on CouponMaster_.CouponId equals couponIssued_.CouponId
                     join CouponissuedDetail_ in context.Couponissuedetails on couponIssued_.CouponIssuemasterId equals CouponissuedDetail_.CouponIssueMasterId
                     select CouponissuedDetail_
                 ).Count();


            merchantCouponList.CouponsSendCount = (
                     from CouponMaster_ in context.Couponmasters
                     join couponIssued_ in context.Couponissuemasters on CouponMaster_.CouponId equals couponIssued_.CouponId
                     join CouponissuedDetail_ in context.Couponissuedetails on couponIssued_.CouponIssuemasterId equals CouponissuedDetail_.CouponIssueMasterId
                     select CouponissuedDetail_
                 ).Count(); ;

            merchantCouponList.MerchantCouponList = CouponList;

            return merchantCouponList;
        }
        public MerchantUserSearchResultModel GetMerchantUserList(MerchantUserSearchModel merchantUserSearchModel)
        {
            var merchantUserList = new MerchantUserSearchResultModel();
            var pageNumber = (merchantUserSearchModel.PageNumber <= 0) ? 1 : merchantUserSearchModel.PageNumber;
            var pageSize = 10;
            var totalRecords = 0.0;
            var totalPages = 0.0; 
            var skip = 0;
            int? Usercode;
            totalRecords = (from merchantuser_ in context.Merchantsystemusers 
                                where merchantuser_.MerchantId == merchantUserSearchModel.MerchantID
                                select merchantuser_).ToList().Count;
                totalPages = Math.Ceiling((double)totalRecords / pageSize);
                skip = (pageNumber - 1) * pageSize;
           
            merchantUserList.TotalCount = Convert.ToInt32(totalRecords);
            Usercode = (from merchantuser_ in context.Merchantsystemusers
                        where merchantuser_.MerchantId == merchantUserSearchModel.MerchantID && merchantuser_.IsAdmin == 0                
                        select merchantuser_).ToList().Count;
            merchantUserList.UserCount = Usercode;
      
            skip = (pageNumber - 1) * pageSize;

            //comman call all the data 

            var query = context.Merchantsystemusers
                  .Where(merchantUser_ => merchantUser_.MerchantId == merchantUserSearchModel.MerchantID);

            if (!String.IsNullOrEmpty(merchantUserSearchModel.Recent))
            {
                if (merchantUserSearchModel.Recent == "Recent")
                {
                    query = query.OrderByDescending(merchantUser_ => merchantUser_.CreatedDate);
                }
                else if (merchantUserSearchModel.Recent == "Alphabatical")
                {
                     query = query.OrderBy(customer => customer.Name);     
                }
            }
            else if (!String.IsNullOrEmpty(merchantUserSearchModel.Keyword))
            {
                query = query.Where(merchantUser_ => merchantUser_.Name.Contains(merchantUserSearchModel.Keyword)
                        || merchantUser_.PhoneNumber == Convert.ToBase64String(CommonUtility.Encrypt(merchantUserSearchModel.Keyword, configuration.SymmetricKey)));
            }
            else if (!string.IsNullOrEmpty(merchantUserSearchModel.Role))
            {
                switch (merchantUserSearchModel.Role)
                {
                    case "Admin":
                        query = query.Where(merchantUser_ => merchantUser_.IsAdmin == 1);
                        break;
                    case "Staff":
                        query = query.Where(merchantUser_ => merchantUser_.IsAdmin == 0);
                        break;                        
                }
            }
            else
            {
                query = query;
            }

            var customerData = query.Select(merchantUser_ => new MerchantUserModel
                {
                    MerchantId = merchantUser_.MerchantId,
                    MerchantSystemUserId = merchantUser_.MerchantSystemUserId,
                    UserId = merchantUser_.UserId,
                    Name = merchantUser_.Name,
                    Email = merchantUser_.Email,
                    ImageUrl = merchantUser_.ImageUrl,
                    // Image = merchantUser_.Image,
                    Password = Encoding.UTF8.GetString(CommonUtility.Decrypt(merchantUser_.Password, configuration.SymmetricKey)),
                    PhoneNumber = Encoding.UTF8.GetString(CommonUtility.Decrypt(merchantUser_.PhoneNumber, configuration.SymmetricKey)),
                    RecStatus = merchantUser_.RecStatus,
                    IsAdmin = (ulong?)merchantUser_.IsAdmin,
                    isSelected = false,
                    VisibleMenu = false,
                }).Skip(skip).Take(pageSize).ToList();

            //merchantUserList.MerchantUserList = customerData;

            merchantUserList.MerchantUserList = customerData;

            merchantUserList.PageCount = customerData.Count; 

            //if (!String.IsNullOrEmpty(merchantUserSearchModel.Keyword ))
            //{
            //    var customerDAta = (from merchantUser_ in context.Merchantsystemusers

            //                        where merchantUser_.MerchantId == merchantUserSearchModel.MerchantID
            //                        && merchantUser_.Name == merchantUserSearchModel.Keyword
            //                        || merchantUser_.PhoneNumber == Convert.ToBase64String(CommonUtility.Encrypt(merchantUserSearchModel.Keyword, configuration.SymmetricKey))
            //                        select new MerchantUserModel
            //                        {
            //                            MerchantId = merchantUser_.MerchantId,
            //                            MerchantSystemUserId = merchantUser_.MerchantSystemUserId,
            //                            UserId = merchantUser_.UserId,
            //                            Name = merchantUser_.Name,                                    
            //                            Email = merchantUser_.Email,
            //                            Image = merchantUser_.Image,  
            //                           //PhoneNumber = CommonUtility.maskString(Encoding.UTF8.GetString(CommonUtility.Decrypt(merchantUser_.PhoneNumber, configuration.SymmetricKey)), "PhoneNUmber"),
            //                            PhoneNumber = Encoding.UTF8.GetString(CommonUtility.Decrypt(merchantUser_.PhoneNumber, configuration.SymmetricKey)),
            //                            RecStatus = merchantUser_.RecStatus,
            //                            IsAdmin = (ulong?)merchantUser_.IsAdmin,
            //                            isSelected = false,
            //                            VisibleMenu = false,
            //                            }).ToList().Skip(skip).Take(pageSize);

            //    merchantUserList.MerchantUserList = customerDAta.ToList();             
            //    merchantUserList.PageCount = totalPages;
                
            //}
            //else
            //{
            //    var customerDAta = (from merchantUser_ in context.Merchantsystemusers
            //                        where merchantUser_.MerchantId == merchantUserSearchModel.MerchantID
            //                        select new MerchantUserModel
            //                        {
            //                            MerchantId = merchantUser_.MerchantId,
            //                            MerchantSystemUserId = merchantUser_.MerchantSystemUserId,
            //                            UserId = merchantUser_.UserId,
            //                            Name = merchantUser_.Name,
            //                            Email = merchantUser_.Email,
            //                            Image = merchantUser_.Image,
            //                            PhoneNumber = CommonUtility.maskString(Encoding.UTF8.GetString(CommonUtility.Decrypt(merchantUser_.PhoneNumber, configuration.SymmetricKey)), "PhoneNUmber"),
            //                            RecStatus = merchantUser_.RecStatus,
            //                            IsAdmin = (ulong?)merchantUser_.IsAdmin,
            //                            isSelected=false,
            //                            VisibleMenu = false,                                       
            //                        }).ToList().Skip(skip).Take(pageSize);

            //    merchantUserList.MerchantUserList = customerDAta.ToList();
            //    merchantUserList.PageCount = totalPages;
            //}
            return merchantUserList;
        }
        public bool EditMerchantUser(MerchantUserModel merchantUserModel, ref ErrorResponseModel errorResponseModel)
        {
            bool result = false;
            var phoneNumber = "";
            var merchantUserData = context.Merchantsystemusers.FirstOrDefault(x => x.MerchantSystemUserId == merchantUserModel.MerchantSystemUserId);
           
            if (merchantUserData != null)
            {
                // Your base URL and image directory
                string baseUrl = "https://api.blootusk.com/";
                string imageDirectory = "user_images/";

                //if(merchantUserModel.UserImage != null )
                //{
                //    string inputString = merchantUserModel.ImageUrl;

                //    if (IsBase64String(inputString))
                //    {
                //        merchantUserModel.ImageUrl = Convert.ToString(merchantUserModel.UserImage);
                //        merchantUserModel.ImageUrl = "";
                //    }
                //    else if (IsImageUrl(inputString))
                //    {
                //        merchantUserModel.ImageUrl = merchantUserModel.ImageUrl;
                //        merchantUserModel.UserImage = new byte[0];
                //    }
                  
                //}

                //static bool IsBase64String(string s)
                //{
                //    try
                //    {
                //        Convert.FromBase64String(s);
                //        return true;
                //    }
                //    catch (FormatException)
                //    {
                //        return false;
                //    }
                //}

                //static bool IsImageUrl(string s)
                //{
                //    // Check if the string looks like a URL and ends with a common image extension
                //    Uri uriResult;
                //    if(Uri.TryCreate(s, UriKind.Absolute, out uriResult)
                //        && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps)
                //        && (s.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase)
                //            || s.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase)
                //            || s.EndsWith(".png", StringComparison.OrdinalIgnoreCase)
                //            || s.EndsWith(".gif", StringComparison.OrdinalIgnoreCase)
                //        // Add more image extensions as needed
                //        ))
                //    {
                //        return true;
                //    }

                //    return false;
                //}

                // Create the full image path on the server
                string imgPath = Path.Combine(imageDirectory, merchantUserData.UserId + ".jpg");
               
                // Check if the directory exists, if not, create it
                
                if (!Directory.Exists(imageDirectory))
                {
                    Directory.CreateDirectory(imageDirectory);
                }

                // Write the image bytes to the specified path

              
                    File.WriteAllBytes(imgPath, merchantUserModel.UserImage);
                   

                //using (FileStream fs = new FileStream(imgPath, FileMode.Create))
                //{
                //    fs.Write(merchantUserModel.UserImage, 0, merchantUserModel.UserImage.Length);
                //}


                // Construct the URL for the saved image
                string imageUrl = baseUrl + imageDirectory + merchantUserData.UserId + ".jpg";

                if (merchantUserModel.UserImage != null && merchantUserModel.UserImage.All(b => b == 0))
                {
                    if(merchantUserData.ImageUrl == null )
                    {
                        merchantUserData.ImageUrl = null;
                    }
                }
                else
                {
                    merchantUserData.ImageUrl = imageUrl;
                }
                merchantUserData.MerchantId = merchantUserModel.MerchantId;
                merchantUserData.Email = Convert.ToBase64String(CommonUtility.Encrypt(merchantUserModel.Email.ToLower(), configuration.SymmetricKey)); ;
                merchantUserData.Name = merchantUserModel.Name;
                merchantUserData.PhoneNumber = Convert.ToBase64String(CommonUtility.Encrypt(merchantUserModel.PhoneNumber, configuration.SymmetricKey));
                merchantUserData.Password = Convert.ToBase64String(CommonUtility.Encrypt(merchantUserModel.Password, configuration.SymmetricKey));
                merchantUserData.UserId = merchantUserData.UserId;
                merchantUserData.IsAdmin = (sbyte?)merchantUserModel.IsAdmin;
                merchantUserData.RecStatus = merchantUserModel.RecStatus;              
                merchantUserData.CreatedDate = DateTime.Now;
                context.SaveChanges();

                string merchantUserId = merchantUserData.UserId;

                // Splitting the merchant ID
                string prefix = merchantUserId.Substring(0, merchantUserId.Length - 2);
                string suffix = merchantUserId.Substring(merchantUserId.Length - 2);
                if (suffix == "01")
                {
                    var merchantData = context.Merchants.FirstOrDefault(x => x.MerchantId == merchantUserData.MerchantId);
                    merchantData.Email = merchantUserData.Email;
                    merchantData.PhoneNumber = Convert.ToBase64String(CommonUtility.Encrypt(merchantUserModel.PhoneNumber, configuration.SymmetricKey));
                    merchantData.Password = merchantUserData.Password;
                    merchantData.OrganizationName = merchantUserData.Name;
                    merchantData.RecStatus = merchantUserModel.RecStatus;
               
                    merchantData.ModifyDate = DateTime.Now;
                    context.SaveChanges();
                }
               

                result = true;
            }
            return result;
        }
        public MerchantUserModel GetMerchantUserById(int merchantSystemUserId, ref ErrorResponseModel errorResponseModel)
        {

                var marchantUserData = (from merchantUser in context.Merchantsystemusers
                                    where merchantUser.MerchantSystemUserId == merchantSystemUserId
                                    select new MerchantUserModel
                                    {
                                        MerchantSystemUserId =  merchantUser.MerchantSystemUserId,
                                        MerchantId = merchantUser.MerchantId,
                                        UserId = merchantUser.UserId,
                                        Name = merchantUser.Name,
                                        PhoneNumber = merchantUser.PhoneNumber,
                                        Password = merchantUser.Password,
                                        IsAdmin = (ulong?)merchantUser.IsAdmin,
                                        RecStatus = merchantUser.RecStatus,
                                        ImageUrl = merchantUser.ImageUrl,
                                        UserImage = merchantUser.Image

                                    }
                                 ).SingleOrDefault();


                return marchantUserData;
            }
        private bool SendSms(string phonenumber, string verificationMessage)
        {
            string ToNumber = string.Empty;
            bool result = false;
            using (var web = new System.Net.WebClient())
            {
                try
                {
                    //log file folder store string =datetime,phone number, message ,  Notepad append
                    //SMSLog Folder File NAme SMSLog.txt append record


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

        //public string GetEncryptedString(string value)
        //{
        //    try
        //    {
        //        return Convert.ToBase64String(CommonUtility.Encrypt(value, configuration.SymmetricKey));
        //    }
        //    catch (Exception ex)
        //    {

        //        throw ex;
        //    }         
        //} 
        public ValidateCouponModel ValidateCoupon(ValidateCouponModel validateCouponModel, ref ErrorResponseModel errorResponseModel)
        {
            ValidateCouponModel validateCoupon = new ValidateCouponModel();
            bool result = false;
            bool isCustomerInMerchant = false;
            string encryptedPhoneNumber = GetEncryptedString(validateCouponModel.phoneNumber);
            try
            {
                isCustomerInMerchant = context.Customers.Join(context.Customermerchantmappers,
                                             customer => customer.CustomerId,
                                             mapper => mapper.CustomerId,
                                             (customer, mapper) => new { Customer = customer, Mapper = mapper })
                                         .Any(result => result.Customer.PhoneNumber == encryptedPhoneNumber && result.Mapper.MerchantId == validateCouponModel.MerchantId);

                var encryptedstring = GetEncryptedString(validateCouponModel.phoneNumber);

                var customers = context.Customers
                     .FirstOrDefault(customer => customer.PhoneNumber == encryptedstring);

                if (isCustomerInMerchant == false)
                {
                    int AssignedCouponcustomerId = context.Couponissuedetails
                  .Where(coupon => coupon.CouponSerialNo == validateCouponModel.CouponCode)
                  .Select(coupon => coupon.CustomerId)
                  .FirstOrDefault();

                    string mobNo = null;
                    var customerInfo = new Customer();
                  
                    validateCouponModel.MerchantCode = context.Merchants
                        .Where(x => x.MerchantId == validateCouponModel.MerchantId)
                  .Select(x => x.MerchantCode)
                  .FirstOrDefault();

                   
                        validateCouponModel.MerchantCode = validateCouponModel.MerchantCode;
                    

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
                        string PhoneNo = Convert.ToBase64String(CommonUtility.Encrypt(validateCouponModel.phoneNumber, configuration.SymmetricKey));

                        var isAlreadySignupCount = context.Customers.Count(customer => customer.PhoneNumber == PhoneNo);

                        try
                        {
                            if (isAlreadySignupCount == 0)
                            {

                                customerInfo.Name = "";
                                customerInfo.PhoneNumber = Convert.ToBase64String(CommonUtility.Encrypt(validateCouponModel.phoneNumber, configuration.SymmetricKey));
                               // customerInfo.IsPhoneNumberValidate = customerModel.IsPhoneNumberValidate;
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

                                //here we will save Customermerchantmappers 
                                var CMMapper = new Customermerchantmapper();
                                CMMapper.MerchantId = Convert.ToInt32(validateCouponModel.MerchantId);
                                CMMapper.CustomerId = customerInfo.CustomerId;
                                CMMapper.ReferCode = validateCouponModel.MerchantCode + "C" + customerCode;
                                CMMapper.ReferBy = AssignedCouponcustomerId;
                                CMMapper.StopMessage = false;
                                CMMapper.ApprovlStatus = "A";
                                context.Customermerchantmappers.Add(CMMapper);
                                context.SaveChanges();

                              //  redeemcustId = customerInfo.CustomerId;
                                var merchantInfo = context.Merchants.Where(merchant => merchant.MerchantId == CMMapper.MerchantId)
                                  .Select(merchant => new
                                  {
                                      MerchantName = merchant.OrganizationName,
                                      MerchantCode = merchant.MerchantCode
                                  })
                                  .FirstOrDefault();
                                bool success = false;
                                Random generator = new Random();
                                String randomNumber = generator.Next(1, 1000000).ToString("D6");

                            var SignupSucesssfulMessage = (from template_ in context.Smstemplates
                                                           where template_.MerchantId == CMMapper.MerchantId && template_.ModeTypeId == 1
                                                           select new PosDetailsModel
                                                           {
                                                               Posaddress = template_.MessageContent,
                                                           }
                                      ).FirstOrDefault();

                           
                            string signupmessage = SignupSucesssfulMessage.Posaddress;

                            string Signupresults = signupmessage;
                            string Signuppattern = @"\[MerchantName\]";
                            string Signupreplace = merchantInfo.MerchantName;
                            signupmessage = Regex.Replace(Signupresults, Signuppattern, Signupreplace);

                            //  string custPhoneNumber = GetDecryptedString(customerInfo.PhoneNumber.Trim());

                            SendSms(validateCouponModel.phoneNumber, signupmessage);

                                var codes = merchantInfo.MerchantCode + customerInfo.CustomerCode;

                                var encryptedMerchantCode = GetEncryptedString(codes);

                               // string RefLink = configuration.MerchantSignUpURL + encryptedMerchantCode;

                            string RefLink = configuration.MerchantSignUpURL + Convert.ToBase64String(Encoding.UTF8.GetBytes(Convert.ToBase64String(CommonUtility.Encrypt(CMMapper.UserMerchantMapperId.ToString(), configuration.SymmetricKey))));


                            var Rewardmap = new Rewardpointmaster();
                                Rewardmap.MerchantId = validateCouponModel.MerchantId;


                            //  id no is already exit in any merchant


                            var RefferalRewardMrssage = (from template_ in context.Smstemplates
                                                         where template_.MerchantId == validateCouponModel.MerchantId && template_.MessageTypeId == 2
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

                            //string results2 = RefferalRewardMrssage.Posname;
                            //string pattern2 = @"\[Reward Points\]";
                            //string replace2 = Convert.ToString(RewardPoint.RewardPoints);
                            //RefferalRewardMrssage.Posname = Regex.Replace(results2, pattern2, replace2);


                            string results3 = RefferalRewardMrssage.Posname;
                            string pattern3 = @"\[UserName\]";
                            string replace3 = validateCouponModel.phoneNumber;
                            RefferalRewardMrssage.Posname = Regex.Replace(results3, pattern3, replace3);

                            Console.WriteLine(RefferalRewardMrssage.Posname);

                            SendSms(validateCouponModel.phoneNumber, RefferalRewardMrssage.Posname);



                            if (customers == null)
                            {
                                var RewardPoint = context.Rewardpointmasters.Where(x => x.RewardTypeId == 1 && x.MerchantId == validateCouponModel.MerchantId)
                                        .Select(Rewardmap => new
                                        {
                                            RewardId = Rewardmap.RewardPonitId,
                                            RewardPoints = Rewardmap.RewardPoint,

                                        })
                                       .FirstOrDefault();

                                var rewardtransactionInfo = new Rewardpointtransaction();
                                rewardtransactionInfo.MerchantId = validateCouponModel.MerchantId;
                                rewardtransactionInfo.RewardPointId = RewardPoint.RewardId;
                                rewardtransactionInfo.Points = RewardPoint.RewardPoints;
                                rewardtransactionInfo.CustomerId = customerInfo.CustomerId;
                                rewardtransactionInfo.CreatedBy = validateCouponModel.MerchantId;
                                rewardtransactionInfo.Createddate = DateTime.Now;
                                rewardtransactionInfo.TransactionDate = DateTime.Now;
                                rewardtransactionInfo.TransactionType = 1;
                                context.Rewardpointtransactions.Add(rewardtransactionInfo);
                                context.SaveChanges();

                                string Refmessage = "";

                                bool smsStatuss = false;

                                                 


                            }
                            //end                               

                            result = true;

                            validateCoupon.IsNewCustomer = true;

                        }
                            catch (Exception ex)
                            {
                                result = false;
                                throw;
                            }

                        }
                    validateCoupon.IsNewCustomer = true;
                }


                else
                {
                    var customer = (from cust in context.Customers
                                      where cust.PhoneNumber == encryptedPhoneNumber
                                      select cust).FirstOrDefault();

                    var custmerchant = context.Customermerchantmappers
                                       .Where(custmerchant => custmerchant.CustomerId == customer.CustomerId && custmerchant.MerchantId == validateCouponModel.MerchantId)
                    .FirstOrDefault();


                    if (custmerchant.ReferBy != 0)
                    {

                        var AlreadyGotRefferalPoints = (from rewardtrans in context.Rewardpointtransactions
                                                        where rewardtrans.CustomerId == custmerchant.ReferBy &&
                                                              rewardtrans.MerchantId == custmerchant.MerchantId && rewardtrans.ReffrealId == customer.CustomerId &&
                                                              rewardtrans.TransactionType == 2
                                                        select rewardtrans).FirstOrDefault();                      

                        if (AlreadyGotRefferalPoints == null)
                        {

                            var name = context.Customers.Where(c => c.CustomerId == custmerchant.ReferBy).FirstOrDefault();
                            validateCoupon.RefferalPoint = context.Rewardpointmasters
                              .Where(custmerchant => custmerchant.RewardTypeId == 2 && custmerchant.MerchantId == validateCouponModel.MerchantId)
                              .Select(reward => reward.RewardPoint)
                              .FirstOrDefault();

                            validateCoupon.phoneNumber = string.IsNullOrEmpty(customer.Name) ? "User - " + CommonUtility.maskString(Encoding.UTF8.GetString(CommonUtility.Decrypt(customer.PhoneNumber, configuration.SymmetricKey)), "PhoneNUmber") : (customer.Name + (string.IsNullOrEmpty(customer.Lastname) ? "" : " " + customer.Lastname));
                            validateCoupon.RefferBy = string.IsNullOrEmpty(name.Name) ? "User - " + CommonUtility.maskString(Encoding.UTF8.GetString(CommonUtility.Decrypt(name.PhoneNumber, configuration.SymmetricKey)), "PhoneNUmber") : (name.Name + (string.IsNullOrEmpty(name.Lastname) ? "" : " " + name.Lastname));
                            validateCoupon.IsNewCustomer = false;
                            validateCoupon.CouponCode = "";
                            validateCoupon.MerchantCode = validateCouponModel.MerchantCode;
                            validateCoupon.MerchantId = validateCouponModel.MerchantId;


                            var RefferalRewardPoint = context.Rewardpointmasters.Where(x => x.RewardTypeId == 2 && 
                            x.MerchantId == custmerchant.MerchantId)
                            .Select(Rewardmap => new
                            {
                                RefferalRewardId = Rewardmap.RewardPonitId,
                                              RefferalRewardPoints = Rewardmap.RewardPoint,
                                          })
                                         .FirstOrDefault();

                            var RefferalrewardtransactionInfo = new Rewardpointtransaction();
                            RefferalrewardtransactionInfo.MerchantId = custmerchant.MerchantId;
                            RefferalrewardtransactionInfo.Points = RefferalRewardPoint.RefferalRewardPoints;
                            RefferalrewardtransactionInfo.TransactionType = 2;
                            RefferalrewardtransactionInfo.RewardPointId = RefferalRewardPoint.RefferalRewardId;
                            RefferalrewardtransactionInfo.CustomerId = custmerchant.ReferBy;
                            RefferalrewardtransactionInfo.CreatedBy = custmerchant.MerchantId;
                            RefferalrewardtransactionInfo.Createddate = DateTime.Now;
                            RefferalrewardtransactionInfo.TransactionDate = DateTime.Now;
                            RefferalrewardtransactionInfo.ReffrealId = customer.CustomerId;
                            context.Rewardpointtransactions.Add(RefferalrewardtransactionInfo);
                            context.SaveChanges();

                            var RefferalByMrssage = (from template_ in context.Smstemplates
                                                     where template_.MessageTypeId == 3 && template_.MerchantId == validateCouponModel.MerchantId
                                                     select new PosDetailsModel
                                                     {
                                                         Posname = template_.MessageContent,
                                                     }
                            ).FirstOrDefault();

                            var RefferalReward = (from customer_ in context.Customers
                                                  join Customermerchantmapper_ in context.Customermerchantmappers on customer_.CustomerId equals Customermerchantmapper_.ReferBy
                                                  where Customermerchantmapper_.ReferBy == custmerchant.ReferBy
                                                  select new
                                                  {
                                                      mobileno = customer_.PhoneNumber,
                                                  }
                              ).FirstOrDefault();

                            string custrefPhoneNumber = GetDecryptedString(RefferalReward.mobileno);

                            //custrefPhoneNumber
                            string RefferalByMrssageresults = RefferalByMrssage.Posname;
                            string RefferalByMrssagepattern = @"\[User's Name\]";
                            string RefferalByMrssagereplace = string.IsNullOrEmpty(customer.Name) ? "User - " + CommonUtility.maskString(Encoding.UTF8.GetString(CommonUtility.Decrypt(customer.PhoneNumber, configuration.SymmetricKey)), "PhoneNUmber") : (customer.Name + (string.IsNullOrEmpty(customer.Lastname) ? "" : " " + customer.Lastname));
                            RefferalByMrssage.Posname = Regex.Replace(RefferalByMrssageresults, RefferalByMrssagepattern, RefferalByMrssagereplace);


                            string RefferalByMrssageresults2 = RefferalByMrssage.Posname;
                            string RefferalByRewardPoint = @"\[RewardPoint\]";
                            string RefferalByRewardPointreplace = validateCoupon.RefferalPoint.ToString();
                            RefferalByMrssage.Posname = Regex.Replace(RefferalByMrssageresults2, RefferalByRewardPoint, RefferalByRewardPointreplace);

                            SendSms(custrefPhoneNumber, RefferalByMrssage.Posname);
                        }

                        else
                        {
                            validateCoupon.phoneNumber = string.IsNullOrEmpty(customer.Name) ? "User - " + CommonUtility.maskString(Encoding.UTF8.GetString(CommonUtility.Decrypt(customer.PhoneNumber, configuration.SymmetricKey)), "PhoneNUmber") : (customer.Name + (string.IsNullOrEmpty(customer.Lastname) ? "" : " " + customer.Lastname));
                            validateCoupon.RefferBy = "";
                            validateCoupon.IsNewCustomer = false;
                            validateCoupon.CouponCode = "";
                            validateCoupon.MerchantCode = validateCouponModel.MerchantCode;
                            validateCoupon.MerchantId = validateCouponModel.MerchantId;
                            validateCoupon.RefferalPoint = 0;
                        }
                        
                    }


                    else
                    {
                        validateCoupon.phoneNumber = string.IsNullOrEmpty(customer.Name) ? "User - " + CommonUtility.maskString(Encoding.UTF8.GetString(CommonUtility.Decrypt(customer.PhoneNumber, configuration.SymmetricKey)), "PhoneNUmber") : (customer.Name + (string.IsNullOrEmpty(customer.Lastname) ? "" : " " + customer.Lastname));
                        validateCoupon.RefferBy = "";
                        validateCoupon.IsNewCustomer = false;
                        validateCoupon.CouponCode = "";
                        validateCoupon.MerchantCode = validateCouponModel.MerchantCode;
                        validateCoupon.MerchantId = validateCouponModel.MerchantId;
                        validateCoupon.RefferalPoint = 0;

                    }            
                }

                //private bool SendSms(string phonenumber, string verificationMessage)
                //{
                //    string ToNumber = string.Empty;
                //    bool result = false;
                //    using (var web = new System.Net.WebClient())
                //    {
                //        try
                //        {
                //            if (configuration.Bypassgetway == true)
                //            {

                //                TwilioClient.Init(configuration.ACCOUNT_SID, configuration.AUTH_TOKEN);
                //                //+918408845409

                //                if (configuration.MobileCountryCode != null)
                //                {
                //                    ToNumber = configuration.MobileCountryCode + phonenumber;
                //                }
                //                else
                //                {
                //                    ToNumber = phonenumber;
                //                }
                //                var message = MessageResource.Create(
                //                    new PhoneNumber(ToNumber),
                //                    from: new PhoneNumber(configuration.FromMobileNumber),
                //                    body: verificationMessage
                //                );
                //                Console.WriteLine(message.Sid);
                //                Errorlog errorlog = new Errorlog();
                //                errorlog.ErrorLog1 = "Send Sms to " + ToNumber + "  And SID ID" + message.Sid;
                //                context.Errorlogs.Add(errorlog);
                //                context.SaveChanges();
                //                result = true;
                //            }
                //            else
                //            {
                //                result = true;
                //            }
                //        }
                //        catch (Exception ex)
                //        {
                //            Errorlog errorlog = new Errorlog();
                //            errorlog.ErrorLog1 = "Error In sending sms to " + ToNumber + " Error Message is " + ex.Message;
                //            context.Errorlogs.Add(errorlog);
                //            context.SaveChanges();
                //            throw ex;
                //            result = false;
                //        }
                //    }
                //    return result;
                //}


                //  IsNewCustomer


                //if (validateCouponModel.CouponCode != null)
                //{
                //    //var isValidCoupon = context.Couponissuemasters
                //    //                .Join(
                //    //                    context.Couponissuedetails,
                //    //                    master => master.CouponIssuemasterId,
                //    //                    detail => detail.CouponIssueMasterId,
                //    //                    (master, detail) => new { master, detail }
                //    //                ).Any(entry => entry.detail.CouponSerialNo == validateCouponModel.CouponCode
                //    //                    && entry.detail.RedeemId == null || entry.detail.RedeemId == 0
                //    //                    && entry.detail.UsedbyId == null || entry.detail.UsedbyId == 0);

                //    var isValidCoupon = context.Couponissuemasters
                //                           .Join(
                //                               context.Couponissuedetails
                //                                   .Where(detail => detail.CouponSerialNo == validateCouponModel.CouponCode),
                //                               master => master.CouponIssuemasterId,
                //                               detail => detail.CouponIssueMasterId,
                //                               (master, detail) => new { master, detail }
                //                           )
                //                           .Any(entry =>
                //                               (entry.detail.RedeemId == null || entry.detail.RedeemId == 0) &&
                //                               (entry.detail.UsedbyId == null || entry.detail.UsedbyId == 0)
                //                           );




                //    result = isValidCoupon;
                //}
                //else
                //{
                //    result = false;
                //}

                return validateCoupon;
            }
            catch (Exception)
            {

                return validateCoupon = null;
            }
        }

        //        public ValidateCouponSearchModel ValidateCoupon(ValidateCouponModel validateCouponModel, ref ErrorResponseModel errorResponseModel)
        //        {
        //            ValidateCouponSearchModel validateCouponSearchModel = new ValidateCouponSearchModel();
        //            string result = "";

        //            if(validateCouponModel.phoneNumber != "" )
        //            {                
        //                bool isCustomerInMerchant = context.Customers.Join(context.Customermerchantmappers,
        //                                                customer => customer.CustomerId,
        //                                                mapper => mapper.CustomerId,
        //                                                (customer, mapper) => new { Customer = customer, Mapper = mapper })
        //                                            .Any(result => result.Customer.PhoneNumber == validateCouponModel.phoneNumber && result.Mapper.MerchantId == validateCouponModel.MerchantId);

        //                if (isCustomerInMerchant == false)
        //                {

        //                    var custdata = (from customer_ in context.Customers
        //                                    join couponIssued in context.Couponissuedetails
        //                                    on customer_.CustomerId equals couponIssued.CustomerId
        //                                    where couponIssued.CouponSerialNo == validateCouponModel.CouponCode
        //                                    select new
        //                                    {
        //                                        CustomerId = customer_.CustomerId,
        //                                        PhoneNumber = customer_.PhoneNumber,
        //                                        RefferalcustName = string.IsNullOrEmpty(customer_.Name)
        //                                           ? "User - " + CommonUtility.maskString(
        //                                               Encoding.UTF8.GetString(CommonUtility.Decrypt(customer_.PhoneNumber, configuration.SymmetricKey)),
        //                                               "PhoneNumber") : customer_.Name    }).ToList();

        //                        // here we need a customerid in coupon code wise

        //                        int customerId = context.Couponissuedetails
        //                                        .Where(coupon => coupon.CouponSerialNo == validateCouponModel.CouponCode)
        //                                        .Select(coupon => coupon.CustomerId)
        //                                        .FirstOrDefault();

        //                        var customerInfoModel = (from custmerchantmapper in context.Customermerchantmappers
        //                                                 join
        //                                                 Customer_ in context.Customers on custmerchantmapper.CustomerId equals Customer_.CustomerId
        //                                                 join merchant_ in context.Merchants on custmerchantmapper.MerchantId equals merchant_.MerchantId
        //                                                 where custmerchantmapper.CustomerId == customerId
        //                                                 select new
        //                                                 {
        //                                                     Customercode = Customer_.CustomerCode,
        //                                                     MerchantCode = merchant_.MerchantCode
        //                                                 }).SingleOrDefault();

        //                        var code = customerInfoModel.MerchantCode + customerInfoModel.Customercode;
        //                        var encryptedMerchantCode = Convert.ToBase64String(Encoding.UTF8.GetBytes(GetEncryptedString(code)));
        //                        string RefLink = configuration.MerchantSignUpURL + encryptedMerchantCode;

        //                    validateCouponSearchModel.CustomerId = customerId;
        //                    validateCouponSearchModel.RecLink = RefLink;


        //                }

        //                else
        //                {
        //                    int customerId = context.Couponissuedetails
        //                            .Where(coupon => coupon.CouponSerialNo == validateCouponModel.CouponCode)
        //                            .Select(coupon => coupon.CustomerId)
        //                            .FirstOrDefault();

        //                    string custphonenumber = context.Customers.Where(c => c.CustomerId == customerId)
        //                        .Select(x => x.PhoneNumber).FirstOrDefault();

        //                    int CustIdRef = 0;

        //                    if (custphonenumber != validateCouponModel.phoneNumber)
        //                    {
        //                         CustIdRef = context.Customers.Where(x => x.PhoneNumber == validateCouponModel.phoneNumber)
        //                              .Select(x => x.CustomerId).FirstOrDefault();
        //                    }
        //                    else
        //                    {
        //                        CustIdRef = customerId;
        //                    }

        //                    if(customerId != CustIdRef)
        //                    {

        //                        var customerref = context.Customers
        //                                        .Where(x => x.CustomerId == customerId)
        //                                        .Select(x => new
        //                                        {
        //                                            Name = x.Name,
        //                                            PhoneNumber = x.PhoneNumber
        //                                        })
        //                                        .FirstOrDefault();
        //                        string refName = "";
        //                        if (customerref != null)
        //                        {
        //                             refName = string.IsNullOrEmpty(customerref?.Name)
        //                           ? "User - " + CommonUtility.maskString(
        //                               Encoding.UTF8.GetString(CommonUtility.Decrypt(customerref.PhoneNumber, configuration.SymmetricKey)),
        //                               "PhoneNumber")
        //                           : customerref.Name;

        //                            validateCouponSearchModel.CustomerRefferalName = refName;
        //                        }
        //                        else
        //                        {

        //                        }
        //                        var customer = (from coupon in context.Couponmasters
        //                                       join issueMaster in context.Couponissuemasters on coupon.CouponId equals issueMaster.CouponId
        //                                       join issueDetail in context.Couponissuedetails on issueMaster.CouponIssuemasterId equals issueDetail.CouponIssueMasterId
        //                                       join customer_ in context.Customers on issueDetail.CustomerId equals customer_.CustomerId
        //                                       join custMerchnat_ in context.Customermerchantmappers on customer_.CustomerId equals custMerchnat_.CustomerId
        //                                        join Merchant_ in context.Merchants on coupon.MerchantId equals Merchant_.MerchantId
        //                                        where customer_.CustomerId == customerId && issueDetail.CouponSerialNo == validateCouponModel.CouponCode
        //                                        select new ValidateCouponSearchModel
        //                                       {
        //                                           phoneNumber = customer_.PhoneNumber,
        //                                           CouponCode = issueDetail.CouponSerialNo,
        //                                           RecLink = "",
        //                                           MerchantId = coupon.MerchantId,
        //                                           DiscountType = coupon.DiscountType,
        //                                           DiscountValue = coupon.DiscountValue,
        //                                           CouponDiscerption = coupon.CouponDiscerption,
        //                                           CouponTitle = coupon.CouponTitle,
        //                                           CustomerName = customer_.Name,
        //                                           MerchantName = Merchant_.OrganizationName,
        //                                           CustomerId = customer_.CustomerId,
        //                                           CustomerRefferalName = refName,

        //                                       }).SingleOrDefault();

        //                        validateCouponSearchModel = customer;
        //                    }
        //                    else
        //                    {

        ////                        select* from Couponmaster cm join
        ////Couponissuemaster cim on cm.CouponId = cim.CouponId
        ////join couponissuedetails  cid on cim.CouponIssuemasterId = cid.CouponIssuemasterId
        ////join customer c on cid.CustomerId = c.CustomerId
        ////join Customermerchantmapper cmm on c.CustomerId = cmm.CustomerId
        ////where c.CustomerId = 303 && cid.CouponSerialNo = 'A03TSKCP'

        //                        var customer = (from coupon in context.Couponmasters
        //                                       join issueMaster in context.Couponissuemasters on coupon.CouponId equals issueMaster.CouponId
        //                                       join issueDetail in context.Couponissuedetails on issueMaster.CouponIssuemasterId equals issueDetail.CouponIssueMasterId
        //                                       join customer_ in context.Customers on issueDetail.CustomerId equals customer_.CustomerId
        //                                       join custMerchnat_ in context.Customermerchantmappers on customer_.CustomerId equals custMerchnat_.CustomerId
        //                                       join Merchant_ in context.Merchants on coupon.MerchantId equals Merchant_.MerchantId
        //                                        where   issueDetail.CouponSerialNo == validateCouponModel.CouponCode
        //                                        && customer_.CustomerId == customerId
        //                                        select new ValidateCouponSearchModel
        //                                       {
        //                                           phoneNumber = customer_.PhoneNumber,
        //                                           CouponCode = coupon.CouponCode,
        //                                           RecLink = "",
        //                                           MerchantId = coupon.MerchantId,
        //                                           DiscountType = coupon.DiscountType,
        //                                           DiscountValue = coupon.DiscountValue,
        //                                           CouponDiscerption = coupon.CouponDiscerption,
        //                                           CouponTitle = coupon.CouponTitle,
        //                                           CustomerName = string.IsNullOrEmpty(customer_.Name) ? "User - " + CommonUtility.maskString(Encoding.UTF8.GetString(CommonUtility.Decrypt(customer_.PhoneNumber, configuration.SymmetricKey)), "PhoneNUmber") : customer_.Name,

        //                                            CustomerRefferalName = "",
        //                                           MerchantName = Merchant_.OrganizationName,
        //                                           CustomerId = customer_.CustomerId
        //                                       }).SingleOrDefault(); 

        //                        validateCouponSearchModel = customer;
        //                    }
        //                }
        //            }
        //            return validateCouponSearchModel;
        //        }

        private string GetEncryptedString(string? value)
        {
            return Convert.ToBase64String(CommonUtility.Encrypt(value, configuration.SymmetricKey));
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

                //20 customer

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

                //10 customer

                List<int> SMSSendCustomerIds = customerIds.Except(reddemcustomerIds).ToList();

                // List<int> SMSSendCustomerIds = IssueCouponList.Except(RedeemCouponList).ToList();

                string Discount = nudgeCouponModel.DiscountType + nudgeCouponModel.DiscountValue;
                if (nudgeCouponModel.DiscountType == "percentage")
                { 
                    Discount = nudgeCouponModel.DiscountType + "%";
                }
                else if (nudgeCouponModel.DiscountType == "dollar")
                {
                    Discount = nudgeCouponModel.DiscountType + "$";
                }                

                Nudgecoupon nudgecoupon = new Nudgecoupon();
                nudgecoupon.CouponId = Convert.ToInt32(nudgeCouponModel.CouponId);
                nudgecoupon.CreatedDate = DateTime.Now; ///datetime
                //CouponCount = smscustomerCount;
                context.Add(nudgecoupon);
                context.SaveChanges();

                int? merchantId = context.Couponmasters
                              .Where(c => c.CouponId == nudgeCouponModel.CouponId)
                              .Select(c => c.MerchantId)
                              .SingleOrDefault();

                var MerchantRefferal = context.Smstemplates
                                        .Where(template => template.MerchantId == merchantId && template.MessageTypeId == 5)
                                        .Select(template => new
                                        {
                                            messagecontent = template.MessageContent,
                                        })
                                        .FirstOrDefault();

                string signupmessage = MerchantRefferal.messagecontent;

                string Signupresults = signupmessage;
                string Signuppattern = @"\[MerchantName\]";
                string Signupreplace = nudgeCouponModel.MerchantName;
                signupmessage = Regex.Replace(Signupresults, Signuppattern, Signupreplace);

                //var decryptedPhoneNumbers = context.Customers
                //   .Where(c => SMSSendCustomerIds.Contains(c.CustomerId))
                //   .Select(c => Encoding.UTF8.GetString(CommonUtility.Decrypt(c.PhoneNumber, configuration.SymmetricKey)))
                //   .ToList();


                var decryptedPhoneNumbers = context.Customers
                                 .Where(c => SMSSendCustomerIds.Contains(c.CustomerId)) // Filter by initial CustomerIds
                                 .Where(c => !context.Customermerchantmappers // Check if StopSms is not true for the customer
                                     .Any(cm => cm.CustomerId == c.CustomerId && cm.StopMessage == true))
                                 .Select(c => Encoding.UTF8.GetString(CommonUtility.Decrypt(c.PhoneNumber, configuration.SymmetricKey)))
                                 .ToList();

                if (decryptedPhoneNumbers.Count < configuration.MessageLimit)
                {
                    //foreach (var phoneNumber in decryptedPhoneNumbers)
                    //{
                    //    SendSms(Convert.ToString(phoneNumber), signupmessage);
                    //}

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

                    string logFileName = "Nudge_" + DateTime.Now.ToString("yyyy-MM-dd-HHmmss");
                    string message = signupmessage + ", Count: " + decryptedPhoneNumbers.Count + ", Numbers: " + LogPhoneNumber;
                    string logFilePath = Path.Combine(logDirectory, logFileName + ".txt");
                    File.AppendAllText(logFilePath, $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}");

                    var serverPath = "Log_files/" + logFileName + ".txt";
                    var pathToSave = Path.Combine(Directory.GetCurrentDirectory(), serverPath);

                    result = "A";

                }
                else
                {
                    result = "D";
                }


            }
            catch (Exception)
            {
                result = "";
                throw;
            }
            return result;
        }    
        public bool IsDuplicateEmail(string email, int merchantId, int MerchantSystemUserId, bool merchantFlag)
        {
            bool result = false;
            if (merchantFlag == true )
            {
                string MerchantEmail = Convert.ToBase64String(CommonUtility.Encrypt(email.ToLower(), configuration.SymmetricKey));

                //var merchantData = context.Merchantsystemusers.FirstOrDefault(x =>
                // x.MerchantId == merchantId
                //&& x.Email == Convert.ToBase64String(CommonUtility.Encrypt(email, configuration.SymmetricKey)));
                var merchantData = context.Merchantsystemusers.FirstOrDefault(x =>
                                    x.MerchantId == merchantId
                                     && x.Email == MerchantEmail);

                if (merchantData != null)
                {
                    result = true;
                }
            
            }
            else
            {
                var merchantData = context.Merchantsystemusers.FirstOrDefault(x =>
                                      x.MerchantId == merchantId
                                      && x.MerchantSystemUserId != MerchantSystemUserId
                                      && x.Email == Convert.ToBase64String(CommonUtility.Encrypt(email.ToLower(), configuration.SymmetricKey)));

                if (merchantData != null)
                {
                    result = true;
                }

            }


            return result;
        }
    
    }
}

