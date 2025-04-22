using BlooTusk.Business.Interface;
using BlooTusk.Common;
using BlooTusk.Model.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System;

namespace BlooTusk.API.Controller
{

    [Route("api/[controller]")]
    [ApiController]

    public class RewardPointController: BaseAPIController
    {

        IRewardPointService rewardPointService;
            private readonly IWebHostEnvironment iwebhostingEnvironment;
           public RewardPointController(IRewardPointService _rewardPointService, IWebHostEnvironment _iwebhostingEnvironment)

            {
                rewardPointService = _rewardPointService;
                iwebhostingEnvironment = _iwebhostingEnvironment;
            }

            /// <summary>
            ///  AddEdit AuditorDetails
            /// </summary>
            /// <param name="model"></param>
            /// <returns></returns>
            [HttpPost("AddEditrewardPoint")]
            [Authorize]
            [ProducesResponseType(typeof(RewardPointModel), 200)]
            [ProducesResponseType(typeof(string), 404)]
            [ProducesResponseType(typeof(string), 400)]
            [ProducesResponseType(typeof(string), 500)]
            public IActionResult AddEditrewardPoint([FromBody] RewardPointModel rewardPointModel)
            {

                try
                {
                    var errorMessage = new ErrorResponseModel();
                    var result = rewardPointService.AddEditRewardPoint(rewardPointModel, ref errorMessage);
                    if (result.Equals("A") || result.Equals("U"))
                    {
                        return Ok(new APIResponse
                        {
                            ResponseStatusCode = GlobalConstants.OKStatus,
                            ResponseMessage = (rewardPointModel.RewardPonitId == 0) ? "rewardPoint added scucessfully" : "rewardPoint updated scucessfully",
                        }
                        );
                    }
                    else if (result.Equals("DR"))
                    {
                        return Ok(new APIResponse
                        {
                            ResponseStatusCode = GlobalConstants.DuplicateCategoryStatus,
                            ResponseMessage = GlobalConstants.DuplicateCategory,
                        });

                    }
                    else
                    {
                        return Ok(new APIResponse
                        {
                            ResponseStatusCode = GlobalConstants.ErrorStatus,
                            ResponseMessage = (rewardPointModel.RewardPonitId == 0) ? "Fail to add rewardPoint" : "Fail to update rewardPoint",
                        });
                    }
                }
                catch (Exception ex)
                {
                    return Ok(new APIResponse
                    {
                        ResponseStatusCode = GlobalConstants.ErrorStatus,
                        ResponseMessage = (rewardPointModel.RewardPonitId == 0) ? "Fail to add rewardPoint" : "Fail to update rewardPoint",
                        ErrorMessage = ex.ToString(),
                    });
                }

            }

            [HttpGet("GetrewardPointDDL")]
            [Authorize]
            [ProducesResponseType(typeof(SmsTemplateModel), 200)]
            [ProducesResponseType(typeof(string), 404)]
            [ProducesResponseType(typeof(string), 400)]
            [ProducesResponseType(typeof(string), 500)]
            public IActionResult GetrewardPointDDL()
                {
                    ErrorResponseModel errorResponseModel = null;
                    try
                    {
                        var rewardPointData = rewardPointService.GetRewardTypeDDL(ref errorResponseModel);
                        if (rewardPointData != null)
                        {
                            return Ok(new APIResponse
                            {
                                ResponseStatusCode = 200,
                                ResponseData = rewardPointData,
                            });
                        }
                        else
                        {
                            return Ok(new APIResponse
                            {
                                ResponseStatusCode = 212,//Data not found
                                ResponseMessage = "Data not found",
                                ResponseData = new RewardPointModel(),
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
                            ResponseData = new RewardPointModel(),
                        });
                    }
                }

            /// <summary>
            /// To get RewardPoint list  
            /// </summary>
            /// <returns></returns>
            [HttpGet("GetAllRewardPoint/{MerchantId}")]
            [Authorize]
            [ProducesResponseType(typeof(RewardPointModel), 200)]
            [ProducesResponseType(typeof(string), 404)]
            [ProducesResponseType(typeof(string), 400)]
            [ProducesResponseType(typeof(string), 500)]
            public IActionResult GetAllRewardPointList( int MerchantId)
            {
                ErrorResponseModel errorResponseModel = null;
                try
                {
                    var RewardPointData = rewardPointService.GetAllRewardPoint(MerchantId, ref errorResponseModel);
                    if (RewardPointData != null)
                    {
                        return Ok(new APIResponse
                        {
                            ResponseStatusCode = GlobalConstants.OKStatus,
                            ResponseData = RewardPointData,
                        });
                    }
                    else
                    {
                        return Ok(new APIResponse
                        {
                            ResponseStatusCode = GlobalConstants.NoDataStatus,//Data not found
                            ResponseMessage = "Data not found",
                            ResponseData = new List<RewardPointModel>(),
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
                        ResponseData = new List<RewardPointModel>(),
                    });
                }
            }

        /// <summary>
        /// To get RewardPoint by Id
        /// </summary>
        /// <param name="RewardPointId"></param>
        /// <returns></returns>
      
            [HttpGet("GetRewardPointById/{RewardPointId}")]
            [Authorize]
            [ProducesResponseType(typeof(RewardPointModel), 200)]
            [ProducesResponseType(typeof(string), 404)]
            [ProducesResponseType(typeof(string), 400)]
            [ProducesResponseType(typeof(string), 500)]
            public IActionResult GetRewardPointById(int RewardPointId)
            {
                ErrorResponseModel errorResponseModel = null;
                try
                {
                    if (RewardPointId <= 0)
                    {
                        return Ok(new APIResponse
                        {
                            ResponseStatusCode = 400,//Bad request
                            ResponseMessage = "Bad request",
                            ResponseData = new RewardPointModel(),
                        });
                    }
                    var RewardPointData = rewardPointService.GetRewardPointById(RewardPointId, ref errorResponseModel);

                    if (RewardPointData != null)
                    {
                        return Ok(new APIResponse
                        {
                            ResponseStatusCode = 200,
                            ResponseData = RewardPointData,
                        });
                    }
                    else
                    {
                        return Ok(new APIResponse
                        {
                            ResponseStatusCode = 212,//Data not found
                            ResponseMessage = "Data not found",
                            ResponseData = new RewardPointModel(),
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
                        ResponseData = new RewardPointModel(),
                    });
                }
            }

            /// <summary>
            /// Delete an CompanyDetails
            /// </summary>
            /// <param name="RewardPointId"></param>
            /// <returns></returns>
            [HttpDelete("DeleteRewardPoint")]
            [Authorize]
            [ProducesResponseType(typeof(RewardPointModel), 200)]
            [ProducesResponseType(typeof(string), 404)]
            [ProducesResponseType(typeof(string), 400)]
            [ProducesResponseType(typeof(string), 500)]
            public IActionResult DeleteRewardPoint(int RewardPointId)
            {
                ErrorResponseModel errorResponseModel = null;
                try
                {
                    bool result = rewardPointService.DeleteRewardPoint(RewardPointId, ref errorResponseModel);

                    if (result)
                    {
                        return Ok(new APIResponse
                        {
                            ResponseStatusCode = 200,
                            ResponseMessage = "RewardPoint successfully deleted",
                            ResponseData = new RewardPointModel(),
                        });
                    }
                    else
                    {
                        return Ok(new APIResponse
                        {
                            ResponseStatusCode = 213,//Fail to delete
                            ResponseMessage = "Fail to delete",
                            ErrorMessage = errorResponseModel.Message,
                            ResponseData = new RewardPointModel(),
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
                        ResponseData = new RewardPointModel(),
                    });
                }
            }

        }
    }

