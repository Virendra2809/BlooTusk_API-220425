using BlooTusk.Business.Interface;
using BlooTusk.Common;
using BlooTusk.Entity.BlooTuskModel;
using BlooTusk.Model.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace BlooTusk.Business.Implementation
{
    public class SMSTemplateService : ISMSTemplateService
    {
       
            BlooTuskContext context;
            public SMSTemplateService(BlooTuskContext _context) 
            {
                context = _context;
            }
            public string AddEditSmsTemplate(SmsTemplateModel smsTemplateModel, ref ErrorResponseModel errorResponseModel)
            {
                string statusCode = "";
                bool result = false;
                Smstemplate smstemplate = new Smstemplate();
            try
            {
                if (smsTemplateModel.TemplateId == 0)
                {

                    var isDuplicateTemplate = context.Smstemplates.Where(x => x.ModeTypeId == smsTemplateModel.ModeTypeId && x.MerchantId == smsTemplateModel.MerchantId  && (x.MessageTypeId == 1)).FirstOrDefault();
                   
                    var isDuplicateTemplatemessage = context.Smstemplates.Where(x => x.ModeTypeId == smsTemplateModel.ModeTypeId && x.MerchantId == smsTemplateModel.MerchantId && (x.MessageTypeId == 2)).FirstOrDefault();


                    if((isDuplicateTemplate != null && isDuplicateTemplatemessage == null && smsTemplateModel.MessageTypeId !=1) || (isDuplicateTemplate == null && isDuplicateTemplatemessage != null && smsTemplateModel.MessageTypeId != 2) || isDuplicateTemplate == null && isDuplicateTemplatemessage == null)

                    //if ((isDuplicateTemplate != null && isDuplicateTemplate.MessageTypeId != smsTemplateModel.MessageTypeId && isDuplicateTemplate.ModeTypeId!=smsTemplateModel.ModeTypeId) ||
                    //    (isDuplicateTemplatemessage != null && isDuplicateTemplatemessage.MessageTypeId == smsTemplateModel.MessageTypeId && isDuplicateTemplatemessage.ModeTypeId!= smsTemplateModel.ModeTypeId))
                    
                    {
                       

                 
                        smstemplate.MessageContent = smsTemplateModel.MessageContent;
                        smstemplate.MessageTypeId = smsTemplateModel.MessageTypeId;
                        smstemplate.ModeTypeId = smsTemplateModel.ModeTypeId;
                        smstemplate.MerchantId = smsTemplateModel.MerchantId;
                        smstemplate.RecStatus = smsTemplateModel.RecStatus;
                        smstemplate.CreatedBy = 1;
                        smstemplate.CreatedDate = DateTime.Now;
                        smstemplate.ModifyDate = DateTime.Now;
                        smsTemplateModel.ModifyBy = smsTemplateModel.ModifyBy;
                        context.Smstemplates.Add(smstemplate);

                        context.SaveChanges();
                        result = true;
                        statusCode = "A";
                    }
                    else
                    {
                        errorResponseModel.Message = GlobalConstants.DuplicateCategory;
                        result = false;
                        statusCode = "DR";

                    }
                }



                else
                {
                    var templateEntity = context.Smstemplates.FirstOrDefault(x => x.TemplateId == smsTemplateModel.TemplateId);
                    if (templateEntity != null)
                    {
                        templateEntity.MessageContent = smsTemplateModel.MessageContent;
                        templateEntity.MessageTypeId = smsTemplateModel.MessageTypeId;
                        templateEntity.MerchantId = smsTemplateModel.MerchantId;
                        templateEntity.RecStatus = smsTemplateModel.RecStatus;
                        templateEntity.ModifyDate = DateTime.Now;
                        templateEntity.ModifyBy = smsTemplateModel.ModifyBy;
                        templateEntity.CreatedBy = 1;
                        templateEntity.CreatedDate = DateTime.Now;

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
                throw;
            }

            return statusCode;
            }


        public List<SmsTemplateModel> GetAllSmsTemplate(int merchantId ,ref ErrorResponseModel errorResponseModel)
            {
                errorResponseModel = new ErrorResponseModel();

                var templateList = (from smsTemplate in context.Smstemplates join type in context.Messagetypemasters
                                    on smsTemplate.MessageTypeId equals type.MessageTypeId 
                                    where smsTemplate.MerchantId == merchantId
                                   
                                    select new SmsTemplateModel
                                    {
                                        MessageType = type.MessageType,
                                        MessageContent = smsTemplate.MessageContent,
                                        RecStatus = smsTemplate.RecStatus,
                                        TemplateId = smsTemplate.TemplateId,

                                    }).ToList();
                return templateList;

         }

        public SmsTemplateModel GetSmsTemplateId(int TemplateId, ref ErrorResponseModel errorResponseModel)
            {
                var templateData = (from smsTemplate in context.Smstemplates join merchantdata in context.Merchants
                                    on smsTemplate.MerchantId equals merchantdata.MerchantId
                                    where smsTemplate.TemplateId == TemplateId 
                                    select new SmsTemplateModel
                                    {
                                        MerchantId = smsTemplate.MerchantId,
                                        RecStatus =  smsTemplate.RecStatus,
                                        TemplateId = smsTemplate.TemplateId,
                                        MessageTypeId = smsTemplate.MessageTypeId,
                                        ModeTypeId = smsTemplate.ModeTypeId,
                                        MessageContent = smsTemplate.MessageContent,  
                                        organizationName = merchantdata.OrganizationName,
                                        phoneNumber = merchantdata.PhoneNumber,
                                        contactPersonName = merchantdata.ContactPersonName,
                                        email = merchantdata.Email,
                                        
                                    }
                                   ).SingleOrDefault();

                if (templateData == null)
                {
                    errorResponseModel.StatusCode = HttpStatusCode.NotFound;
                    errorResponseModel.Message = "No Data found";
                }

                return templateData;
            }


        public List<MessageTypeModel> GetMessageTypeDDL(ref ErrorResponseModel errorResponseModel)
        {
            errorResponseModel = new ErrorResponseModel();

            var MessageTypeList = (from messagetype in context.Messagetypemasters
                                where messagetype.RecStatus == "A"
                                select new MessageTypeModel
                                {
                                    MessageTypeId = messagetype.MessageTypeId,
                                    MessageType = messagetype.MessageType,
                                }
                               ).Distinct().ToList();

            if (MessageTypeList == null)
            {
                errorResponseModel.StatusCode = HttpStatusCode.NotFound;
                errorResponseModel.Message = "No Data found";
            }
            return MessageTypeList;
        }

    }

}
