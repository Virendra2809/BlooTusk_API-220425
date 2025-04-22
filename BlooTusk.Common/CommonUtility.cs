using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net;
using BlooTusk.Model.Models;
using System.Security.Cryptography; 
using System.Text;
using System.Threading.Tasks;
using BlooTusk.Model.Models;
using Twilio.Types;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using BlooTusk.Entity.BlooTuskModel;

namespace BlooTusk.Common
{
    public class CommonUtility
    {
        private ConfigurationModel configuration;
        BlooTuskContext context;
        public static byte[] Encrypt(string plainText,string key)
        {
          
            byte[] inputBytes = Encoding.UTF8.GetBytes(plainText);
           
            // Create a new instance of the AesManaged class
            using (AesManaged aes = new AesManaged())
            {
                aes.Key = Encoding.UTF8.GetBytes("12$#@BLOO$^@TUSK");
                aes.IV = Encoding.UTF8.GetBytes("12$#@BLOO$^@TUSK");
                // Create an encryptor to perform the stream transform
                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
                // Create the streams used for encryption
                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        csEncrypt.Write(inputBytes, 0, inputBytes.Length);
                        csEncrypt.FlushFinalBlock();
                        // Return the encrypted bytes from the memory stream
                        return msEncrypt.ToArray();
                    }
                }
            }
        }

       public static byte[] Decrypt(byte[] inputBytes, string key)
        {
            // Create a new instance of the AesManaged class
            using (AesManaged aes = new AesManaged())
            {
                aes.Key = Encoding.UTF8.GetBytes("12$#@BLOO$^@TUSK");
                aes.IV = Encoding.UTF8.GetBytes("12$#@BLOO$^@TUSK");
                // Create a decryptor to perform the stream transform
                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
                // Create the streams used for decryption
                using (MemoryStream msDecrypt = new MemoryStream(inputBytes))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        byte[] outputBytes = new byte[inputBytes.Length];
                        int decryptedByteCount = csDecrypt.Read(outputBytes, 0, outputBytes.Length);
                        // Return the decrypted bytes from the memory stream
                        return outputBytes.Take(decryptedByteCount).ToArray();
                    }
                }
            }
        }
        public static byte[] Decrypt(string encryptedString, string key)
        {
            
            byte[] inputBytes = Convert.FromBase64String(encryptedString);
            // Create a new instance of the AesManaged class
            using (AesManaged aes = new AesManaged())
            {
                aes.Key = Encoding.UTF8.GetBytes("12$#@BLOO$^@TUSK");
                aes.IV = Encoding.UTF8.GetBytes("12$#@BLOO$^@TUSK");
                // Create a decryptor to perform the stream transform
                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
                // Create the streams used for decryption
                using (MemoryStream msDecrypt = new MemoryStream(inputBytes))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        byte[] outputBytes = new byte[inputBytes.Length];
                        int decryptedByteCount = csDecrypt.Read(outputBytes, 0, outputBytes.Length);
                        // Return the decrypted bytes from the memory stream
                        string decryptedString = Encoding.UTF8.GetString(outputBytes.Take(decryptedByteCount).ToArray());
                        return outputBytes.Take(decryptedByteCount).ToArray();
                    }
                }
            }
        }

        public static bool SendEmail(EmailModel emailModel)
        {
            bool result = false;
            try
            {
                EmailSettings emailSettings = emailModel.EmailSettings;
                string toEmail = string.IsNullOrEmpty(emailModel.ReceiverEmail) ? emailSettings.ToEmail : emailModel.ReceiverEmail;
                MailMessage mail = new MailMessage()
                {
                    From = new MailAddress(emailSettings.UsernameEmail, "BlooTusk")
                };
                mail.To.Add(new MailAddress(toEmail));
          //      mail.Bcc.Add(new MailAddress("support@blootusk.com"));
                mail.Subject = emailModel.Subject;
                mail.Body = emailModel.EmailBody;
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
                            result = true;
                        }
                        catch (SmtpException ex)
                        {

                            result = false;
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

        public static string maskString(string value, string type)
        {
            string result = string.Empty;
            string maskString=string.Empty;
            if (!string.IsNullOrEmpty(value)) {
                try
                {
                    if (type.Equals("Email"))
                    {
                        maskString = value.Substring(value.Length - 5);
                        result = value.Replace(maskString, "xxxxx");
                    }
                    else if (type.Equals("PhoneNUmber"))
                    {
                        maskString = value.Substring(0, 5);
                        result = value.Replace(maskString, "xxxxx");
                    }
                }
                catch (Exception)
                {

                    return result;
                }
               
            }
            return result;
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
                    Entity.BlooTuskModel.Errorlog errorlog = new Errorlog();
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
