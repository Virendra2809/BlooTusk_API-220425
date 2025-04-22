using BlooTusk.Business.Interface;
using BlooTusk.Common;
using BlooTusk.Entity.BlooTuskModel;
using BlooTusk.Model.Models;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlooTusk.Business.Implementation
{
    public class SignupRequestService : ISignupRequestService
    {
        BlooTuskContext context;
        private ConfigurationModel configuration;
        private EmailSettings emailSettings;
        public SignupRequestService(BlooTuskContext _context, IOptions<ConfigurationModel> configuration, IOptions<EmailSettings> _emailSettings)
        {
            context = _context;
            this.configuration = configuration.Value;
            this.emailSettings = _emailSettings.Value;
        }
        public bool AddSignupRequest(SignupRequestModel signupRequestModel, ref ErrorResponseModel errorResponseModel)
        {
           bool result = false;
            var signUpRequestEntity=context.Signuprequests.Where(x=>x.Email==signupRequestModel.Email).FirstOrDefault();
            if (signUpRequestEntity == null)
            {
                var newSignupRequest = new Signuprequest();
                newSignupRequest.Email = signupRequestModel.Email;
                newSignupRequest.Ipaddress = signupRequestModel.Ipaddress;
                newSignupRequest.CreatedDate= DateTime.Now;
                context.Signuprequests.Add(newSignupRequest);
                context.SaveChanges();
                result = true;

            }
            else
            {
                if (!String.IsNullOrEmpty(signupRequestModel.PhoneNumber) || signupRequestModel.PhoneNumber != null)
                {
                    signUpRequestEntity.PhoneNumber = signupRequestModel.PhoneNumber;
                    signUpRequestEntity.CategoryName = signupRequestModel.Category;
                    context.SaveChanges();
                }
                else
                {
                    signUpRequestEntity.CategoryName = signupRequestModel.Category;
                    context.SaveChanges();
                }

                string phoneNumber = (!String.IsNullOrEmpty(signupRequestModel.PhoneNumber) || signupRequestModel.PhoneNumber != null) ? signupRequestModel.PhoneNumber : "-";

                EmailModel emailModel = new EmailModel();

                emailModel.EmailSettings = emailSettings;
                emailModel.ReceiverEmail = emailSettings.FromEmail;
                emailModel.Subject = "Signup Request From Website";
                emailModel.EmailBody = "Hello, \n\t New Request from BlooTusk website\r\nemail : " + signupRequestModel.Email + "\r\nPhoneNumber : " + phoneNumber;
                CommonUtility.SendEmail(emailModel);
                result = true;
            }

            return result;
        }
    }
}
