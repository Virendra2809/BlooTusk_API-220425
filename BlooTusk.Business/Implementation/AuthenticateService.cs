using BlooTusk.Business.Interface;
using BlooTusk.Common;
using BlooTusk.Entity.BlooTuskModel;
using BlooTusk.Model.Models;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Twilio.Types;
using Twilio;
using Twilio.Rest.Api.V2010.Account;

namespace BlooTusk.Business.Implementation
{
    public class AuthenticateService : IAuthenticateService
    {
        BlooTuskContext context;
        private ConfigurationModel configuration;
        public AuthenticateService(BlooTuskContext _context,IOptions<ConfigurationModel> _configuration)
        {
            context = _context;
            this.configuration = _configuration.Value;
        }
        public AdminAuthenticateModel AuthenticateAdminUser(LoginModel loginModel, ref ErrorResponseModel errorResponseModel)
        {
            errorResponseModel = new ErrorResponseModel();
            var adminEntity = context.Systemusermasters.FirstOrDefault(x => x.Username == loginModel.Username && x.Password == loginModel.Password);

            if (adminEntity == null)
            {
                errorResponseModel.StatusCode = HttpStatusCode.OK;
                errorResponseModel.Message = "{\"message\": \"Invalid Username or password \"}";
                return null;
            }

            //To DO: Add multiple attempt logic
            // Update role dynamic logic
            return new AdminAuthenticateModel
            {
                AdminID = adminEntity.SystemUserId,
                Username = adminEntity.Username,
                RecStatus = adminEntity.RecStatus,
            };           
        }
        public MerchantAuthenticateModel AuthenticateMerchantUser(LoginModel loginModel, ref ErrorResponseModel errorResponseModel)
        {
            errorResponseModel = new ErrorResponseModel();
            string encryptedUsername = Convert.ToBase64String(CommonUtility.Encrypt(loginModel.Username, configuration.SymmetricKey));
            string encryptedPassword = Convert.ToBase64String(CommonUtility.Encrypt(loginModel.Password, configuration.SymmetricKey));
           
            var merchantEntity = context.Merchants.FirstOrDefault(x => (x.Email == encryptedUsername || x.PhoneNumber== encryptedUsername) 
            && x.Password == encryptedPassword);

            if (merchantEntity == null)
            {
                errorResponseModel.StatusCode = HttpStatusCode.OK;
                errorResponseModel.Message = "{\"message\": \"Invalid Username or password \"}";
              
                return null;
            }
            else
            {
                merchantEntity.DeviceId = loginModel.DeviceID;
                merchantEntity.DeviceOs = loginModel.DeviceOS;
                merchantEntity.Token = loginModel.FirebaseToken;
                context.SaveChanges();
            }

            //To DO: Add multiple attempt logic
            // Update role dynamic logic
            return new MerchantAuthenticateModel
            {
                MerchantID = merchantEntity.MerchantId,
                MerchantName = merchantEntity.OrganizationName,
                Email = merchantEntity.Email,
                PhoneNumber = merchantEntity.PhoneNumber,
                ApprovalStatus = merchantEntity.ApprovalStatus,
                RecStatus = merchantEntity.RecStatus,
            };
        }         
        public MerchantAuthenticateModel AuthenticateMerchantSystemUser(LoginModel loginModel, ref ErrorResponseModel errorResponseModel)
        {

            string loginEmail = loginModel.Email.ToLower();
            string encryptedEmail = "";
            errorResponseModel = new ErrorResponseModel();
            string encryptedPassword = Convert.ToBase64String(CommonUtility.Encrypt(loginModel.Password, configuration.SymmetricKey));
           

            if (loginModel.Email != "")
            {
                 encryptedEmail = Convert.ToBase64String(CommonUtility.Encrypt(loginEmail, configuration.SymmetricKey));
            }
            var merchantEntity = context.Merchantsystemusers.FirstOrDefault(x =>  x.UserId == loginModel.Username 
            && x.Password == encryptedPassword
            && x.RecStatus == "A"
                                || x.Email == encryptedEmail
                                && x.Password == encryptedPassword && x.RecStatus == "A") ;

            var merchantEntitys = (from merchantUser in context.Merchantsystemusers
                                 
                                  join pos in context.Pos on merchantUser.MerchantId equals pos.MerchantId
                                   join merchant in context.Merchants on pos.MerchantId equals merchant.MerchantId

                                  where merchantUser.UserId == loginModel.Username && merchantUser.RecStatus == "A"
                                  || merchantUser.Email == encryptedEmail && merchantUser.Password == encryptedPassword 
                                  select new
                                  {
                                      MerchantUser = merchantUser,
                                      Merchant = merchant,
                                      Pos = pos
                                  }).FirstOrDefault();

           if (merchantEntitys == null)
            {
                errorResponseModel.StatusCode = HttpStatusCode.OK;
                errorResponseModel.Message = "{\"message\": \"Invalid Username or password \"}";


                return null;
            }

            else
            {
                merchantEntity.DeviceId = loginModel.DeviceID;
                merchantEntity.DeviceOs = loginModel.DeviceOS;
                merchantEntity.Token = loginModel.FirebaseToken;
            
                context.SaveChanges();
            }

            string emailmerchant = GetDecryptedString(merchantEntity.Email);

            //To DO: Add multiple attempt logic
            // Update role dynamic logic
            return new MerchantAuthenticateModel
            {
                MerchantSystemUserID = merchantEntity.MerchantSystemUserId,
                ImageUrl = merchantEntity.ImageUrl,
                MerchantID = merchantEntity.MerchantId,
                UserID = merchantEntity.UserId,
                MerchantName = merchantEntity.Name,               
                PhoneNumber = merchantEntity.PhoneNumber,
                PosId = merchantEntitys.Pos.Posid,
                RecStatus = merchantEntity.RecStatus,
                Email = merchantEntity.Email,
                IsAdmin = merchantEntity.IsAdmin,
                ApprovalStatus = merchantEntity.Merchant.ApprovalStatus,
                Branch = merchantEntitys.Pos.Posname ?? "",
                City = merchantEntity.Merchant.City ?? "",
                MerchantURL = configuration.MerchantSignUpURL + Convert.ToBase64String(Encoding.UTF8.GetBytes(Convert.ToBase64String(CommonUtility.Encrypt(merchantEntity.MerchantId.ToString() + "M", configuration.SymmetricKey))))

            };
        }

        private string GetDecryptedString(string value)
        {
            return Encoding.UTF8.GetString(CommonUtility.Decrypt(value, configuration.SymmetricKey));
        }

        public CustomerAuthenticateModel AuthenticateCustomer(CustLoginModel loginModel, ref ErrorResponseModel errorResponseModel)
        {
            errorResponseModel = new ErrorResponseModel();

            string encryptedPhoneNumber = Convert.ToBase64String(CommonUtility.Encrypt(loginModel.PhoneNumber, configuration.SymmetricKey));

            var CustomerEntity = context.Customers.FirstOrDefault(x => (x.PhoneNumber == encryptedPhoneNumber));

            if (CustomerEntity == null)
            {
                errorResponseModel.StatusCode = HttpStatusCode.OK;
                errorResponseModel.Message = "{\"message\": \"Invalid  Mobile Number \"}";

                return null;
            }
            else
            {

                context.SaveChanges();
            }

            //To DO: Add multiple attempt logic
            // Update role dynamic logic
            return new CustomerAuthenticateModel
            {
                CustomerID = CustomerEntity.CustomerId,
                MobileNo = CustomerEntity.PhoneNumber,
                CustName = string.IsNullOrEmpty(CustomerEntity.Name) ? "User - " + Encoding.UTF8.GetString(CommonUtility.Decrypt(CustomerEntity.PhoneNumber, configuration.SymmetricKey)) : (CustomerEntity.Name + (string.IsNullOrEmpty(CustomerEntity.Lastname) ? "" : " " + CustomerEntity.Lastname)),
                RecStatus = CustomerEntity.RecStatus,
            };
        }
        public OTPModel SendOTP(OTPModel oTPModel, ref ErrorResponseModel errorResponseModel)
        {
            try
            {
                bool success = false;
                Random generator = new Random();
                String randomNumber = generator.Next(1, 1000000).ToString("D6");
                string message = "\r\nHello User,\r\nThank you for choosing BlooTusk. Use this OTP " + randomNumber + " to complete your sign up procedures and verify your account on BlooTusk";
                bool smsStatus = SendSms(oTPModel.PhoneNumber, message);


                return new OTPModel
                {
                    PhoneNumber = oTPModel.PhoneNumber,
                    PhoneNumberOTP = randomNumber,
                };
            }
            catch (Exception)
            {

                throw;
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
                    if(configuration.Bypassgetway == true)
                    {                    
                     TwilioClient.Init(configuration.ACCOUNT_SID, configuration.AUTH_TOKEN);
                    //+918408845409
                    // ToNumber =  phonenumber;
                    ToNumber = configuration.MobileCountryCode + phonenumber;
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
    }
}
