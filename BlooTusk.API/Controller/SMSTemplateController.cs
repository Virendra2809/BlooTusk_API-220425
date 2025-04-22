using BlooTusk.Business.Interface;
using BlooTusk.Model.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System;
using BlooTusk.Common;
using System.Diagnostics.Eventing.Reader;

namespace BlooTusk.API.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class SMSTemplateController : BaseAPIController
    {
       
        ISMSTemplateService smstemplateService;
        private readonly IWebHostEnvironment iwebhostingEnvironment;

        public SMSTemplateController(ISMSTemplateService _smstemplateService, IWebHostEnvironment _iwebhostingEnvironment)

        {
            smstemplateService = _smstemplateService;
            iwebhostingEnvironment = _iwebhostingEnvironment;
        }

       
        [HttpPost("AddSMSTemplate")]
        [Authorize]
        [ProducesResponseType(typeof(SmsTemplateModel), 200)]
        [ProducesResponseType(typeof(string), 404)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(string), 500)]
        public IActionResult AddSMSTemplate([FromBody] SmsTemplateModel smsTemplateModel)
        {
            try
            {
                var errorMessage = new ErrorResponseModel();
                var result = smstemplateService.AddEditSmsTemplate(smsTemplateModel, ref  errorMessage);
                if (result.Equals("A") || result.Equals("U"))
                {
                    return Ok(new APIResponse
                    {
                        ResponseStatusCode = GlobalConstants.OKStatus,
                        ResponseMessage = (smsTemplateModel.TemplateId == 0) ? "Template added scucessfully" : "Template updated scucessfully",
                    }
                    );
                }
                else if (result.Equals("DR"))
                {
                    return Ok(new APIResponse
                    {
                        ResponseStatusCode = GlobalConstants.DuplicateTemplateStatus,
                        ResponseMessage = GlobalConstants.DuplicateTemplate,
                    });

                }
                else
                {
                    return Ok(new APIResponse
                    {
                        ResponseStatusCode = GlobalConstants.ErrorStatus,
                        ResponseMessage = (smsTemplateModel.TemplateId == 0) ? "Fail to add Template" : "Fail to update Template",
                    });
                }
            }
            catch (Exception ex)
            {
                return Ok(new APIResponse
                {
                    ResponseStatusCode = GlobalConstants.ErrorStatus,
                    ResponseMessage = (smsTemplateModel.TemplateId == 0) ? "Fail to add template" : "Fail to update template",
                    ErrorMessage = ex.ToString(),
                });
            }
        }


        
        [HttpGet("GetMessageTypeDDL")]
        [ProducesResponseType(typeof(MessageTypeModel), 200)]
        [ProducesResponseType(typeof(string), 404)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(string), 500)]
        public IActionResult GetMessageTypeDDL()
        {
            ErrorResponseModel errorResponseModel = null;
            try
            {
                var templateData = smstemplateService.GetMessageTypeDDL(ref errorResponseModel);
                if (templateData != null)
                {
                    return Ok(new APIResponse
                    {
                        ResponseStatusCode = 200,
                        ResponseData = templateData,
                    });
                }
                else
                {
                    return Ok(new APIResponse
                    {
                        ResponseStatusCode = 212,//Data not found
                        ResponseMessage = "Data not found",
                        ResponseData = new SmsTemplateModel(),
                    });
                }

            }
            catch (Exception ex)
            {
                //return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
                return Ok(new APIResponse
                {
                    ResponseStatusCode = StatusCodes.Status500InternalServerError,//Data not found
                    ResponseMessage = "Error",
                    ErrorMessage = ex.Message,
                    ResponseData = new SmsTemplateModel(),
                });
            }
        }

         
        [HttpGet("GetAllSmsTemplate/{MerchantId}")]
        [Authorize]
        [ProducesResponseType(typeof(SmsTemplateModel), 200)]
        [ProducesResponseType(typeof(string), 404)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(string), 500)]
        public IActionResult GetAllSmsTemplate(int MerchantId)
        {
            ErrorResponseModel errorResponseModel = null;
            try
            {
                var templateData = smstemplateService.GetAllSmsTemplate(MerchantId, ref errorResponseModel);
                if (templateData != null)
                {
                    return Ok(new APIResponse
                    {
                        ResponseStatusCode = GlobalConstants.OKStatus,
                        ResponseData = templateData,
                    });
                }
                else
                {
                    return Ok(new APIResponse
                    {
                        ResponseStatusCode = GlobalConstants.NoDataStatus,//Data not found
                        ResponseMessage = "Data not found",
                        ResponseData = new List<SmsTemplateModel>(),
                    });
                }

            }
            catch (Exception ex)
            {
                //return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
                return Ok(new APIResponse
                {
                    ResponseStatusCode = GlobalConstants.ErrorStatus,//Data not found
                    ResponseMessage = "Error",
                    ErrorMessage = ex.Message,
                    ResponseData = new List<SmsTemplateModel>(),
                });
            }
        }

        [HttpGet("GetSmsTemplateById/{templateId}")]
        [Authorize]
        [ProducesResponseType(typeof(SmsTemplateModel), 200)]
        [ProducesResponseType(typeof(string), 404)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(string), 500)]
        public IActionResult GetSmsTemplateId(int templateId)
        {
            ErrorResponseModel errorResponseModel = null;
            try
            {
                if (templateId <= 0)
                {
                    return Ok(new APIResponse
                    {
                        ResponseStatusCode = 400,//Bad request
                        ResponseMessage = "Bad request",
                        ResponseData = new SmsTemplateModel(),
                    });
                }
                var templateData = smstemplateService.GetSmsTemplateId(templateId, ref errorResponseModel);

                if (templateData != null)
                {
                    return Ok(new APIResponse
                    {
                        ResponseStatusCode = 200,
                        ResponseData = templateData,
                    });
                }
                else
                {
                    return Ok(new APIResponse
                    {
                        ResponseStatusCode = 212,//Data not found
                        ResponseMessage = "Data not found",
                        ResponseData = new SmsTemplateModel(),
                    });
                }
            }
            catch (Exception ex)
            {
                return Ok(new APIResponse
                {
                    ResponseStatusCode = StatusCodes.Status500InternalServerError,//Error
                    ResponseMessage = "Error",
                    ErrorMessage = ex.Message,
                    ResponseData = new SmsTemplateModel(),
                });
            }
        }

    }
}
