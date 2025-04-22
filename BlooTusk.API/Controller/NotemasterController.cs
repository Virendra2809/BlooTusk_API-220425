using BlooTusk.Business.Interface;
using BlooTusk.Common;
using BlooTusk.Model.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System;
using BlooTusk.Entity.BlooTuskModel;

namespace BlooTusk.API.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotemasterController : BaseAPIController
    {

        INoteMasterService noteService;
            private readonly IWebHostEnvironment iwebhostingEnvironment;

            public NotemasterController(INoteMasterService _noteService, IWebHostEnvironment _iwebhostingEnvironment)

            {
                noteService = _noteService;
                iwebhostingEnvironment = _iwebhostingEnvironment;
            }

           
            [HttpPost("AddEditNote")]
            [Authorize]
            [ProducesResponseType(typeof(NotemasterModel), 200)]
            [ProducesResponseType(typeof(string), 404)]
            [ProducesResponseType(typeof(string), 400)]
            [ProducesResponseType(typeof(string), 500)]
            public IActionResult AddEditNote([FromBody] NotemasterModel noteModel)
            {

                try
                {
                    var errorMessage = new ErrorResponseModel();
                    var result = noteService.AddEditNote(noteModel, ref errorMessage);
                    if (result.Equals("A") || result.Equals("U"))
                    {
                        return Ok(new APIResponse
                        {
                            ResponseStatusCode = GlobalConstants.OKStatus,
                            ResponseMessage = (noteModel.NotemasterId == 0) ? "Note added scucessfully" : "Note updated scucessfully",
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
                            ResponseMessage = (noteModel.NotemasterId == 0) ? "Fail to add note" : "Fail to update note",
                        });
                    }
                }
                catch (Exception ex)
                {
                    return Ok(new APIResponse
                    {
                        ResponseStatusCode = GlobalConstants.ErrorStatus,
                        ResponseMessage = (noteModel.NotemasterId == 0) ? "Fail to add note" : "Fail to update note",
                        ErrorMessage = ex.ToString(),
                    });
                }

            }


          
            /// <summary>
            /// To get category by Id
            /// </summary>
            /// <param name="categoryId"></param>
            /// <returns></returns>
            [HttpGet("GetNoteById/{CustomerId}")]
            [Authorize]
            [ProducesResponseType(typeof(NotemasterModel), 200)]
            [ProducesResponseType(typeof(string), 404)]
            [ProducesResponseType(typeof(string), 400)]
            [ProducesResponseType(typeof(string), 500)]
            public IActionResult GetCategoryById(int CustomerId)
            {
                ErrorResponseModel errorResponseModel = null;
            
                try
                {
                    if (CustomerId <= 0)
                    {
                        return Ok(new APIResponse
                        {
                            ResponseStatusCode = 400,//Bad request
                            ResponseMessage = "Bad request",
                            ResponseData = new NotemasterModel(),
                        });
                    }
                    var categoryData = noteService.GetNoteById(CustomerId, ref errorResponseModel);

                    if (categoryData != null)
                    {
                        return Ok(new APIResponse
                        {
                            ResponseStatusCode = 200,
                            ResponseData = categoryData,
                        });
                    }
                    else
                    {
                        return Ok(new APIResponse
                        {
                            ResponseStatusCode = 212,//Data not found
                            ResponseMessage = "Data not found",
                            ResponseData = new NotemasterModel(),
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
                        ResponseData = new NotemasterModel(),
                    });
                }
            }


            /// <summary>
            /// Delete an CompanyDetails
            /// </summary>
            /// <param name="categoryId"></param>
            /// <returns></returns>
            [HttpDelete("DeleteCategory")]
            [Authorize]
            [ProducesResponseType(typeof(NotemasterModel), 200)]
            [ProducesResponseType(typeof(string), 404)]
            [ProducesResponseType(typeof(string), 400)]
            [ProducesResponseType(typeof(string), 500)]
            public IActionResult DeleteCategory(int categoryId)
            {
                ErrorResponseModel errorResponseModel = null;
                try
                {
                    bool result = noteService.DeleteNote(categoryId, ref errorResponseModel);

                    if (result)
                    {
                        return Ok(new APIResponse
                        {
                            ResponseStatusCode = 200,
                            ResponseMessage = "Note successfully deleted",
                            ResponseData = new NotemasterModel(),
                        });
                    }
                    else
                    {
                        return Ok(new APIResponse
                        {
                            ResponseStatusCode = 213,//Fail to delete
                            ResponseMessage = "Fail to delete",
                            ErrorMessage = errorResponseModel.Message,
                            ResponseData = new NotemasterModel(),
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
                        ResponseData = new NotemasterModel(),
                    });
                }
            }
        }
    }
