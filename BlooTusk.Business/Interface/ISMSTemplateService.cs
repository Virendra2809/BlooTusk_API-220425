using BlooTusk.Model.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlooTusk.Business.Interface 
{
    public interface ISMSTemplateService
    {
        string AddEditSmsTemplate(SmsTemplateModel smsTemplateModel, ref ErrorResponseModel errorResponseModel);
        SmsTemplateModel GetSmsTemplateId(int templateId, ref ErrorResponseModel errorResponseModel);
        List<SmsTemplateModel> GetAllSmsTemplate(int merchantId, ref ErrorResponseModel errorResponseModel);
        List<MessageTypeModel> GetMessageTypeDDL(ref ErrorResponseModel errorResponseModel);
    }
}



