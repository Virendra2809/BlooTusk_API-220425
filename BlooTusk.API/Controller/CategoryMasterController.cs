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
    public class CategoryMasterController : BaseAPIController
    {

        ICategoryService categoryService; 
        private readonly IWebHostEnvironment iwebhostingEnvironment;

        public CategoryMasterController(ICategoryService _categoryService, IWebHostEnvironment _iwebhostingEnvironment)

        {
            categoryService = _categoryService;
            iwebhostingEnvironment = _iwebhostingEnvironment;
        }

        /// <summary>
        ///  AddEdit AuditorDetails
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("AddEditCategory")]
        [Authorize]
        [ProducesResponseType(typeof(CategoryModel), 200)]
        [ProducesResponseType(typeof(string), 404)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(string), 500)]
        public IActionResult AddEditCategory([FromBody] CategoryModel categoryModel)
        {

            try
            {
                var errorMessage = new ErrorResponseModel();
                var result = categoryService.AddEditCategory(categoryModel, ref errorMessage);
                if (result.Equals("A") || result.Equals("U"))
                {
                    return Ok(new APIResponse
                    {
                        ResponseStatusCode = GlobalConstants.OKStatus,
                        ResponseMessage = (categoryModel.CategoryId == 0) ? "Category added scucessfully" : "Category updated scucessfully",
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
                        ResponseMessage = (categoryModel.CategoryId == 0) ? "Fail to add category" : "Fail to update category",
                    });
                }
            }
            catch (Exception ex)
            {
                return Ok(new APIResponse
                {
                    ResponseStatusCode = GlobalConstants.ErrorStatus,
                    ResponseMessage = (categoryModel.CategoryId == 0) ? "Fail to add category" : "Fail to update category",
                    ErrorMessage = ex.ToString(),
                });
            }

        }


        /// <summary>
        /// To get categoryDDL  
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetCategoryDDL")]
        [ProducesResponseType(typeof(CategoryModel), 200)]
        [ProducesResponseType(typeof(string), 404)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(string), 500)]
        public IActionResult GetCategoryDDL()
        {
            ErrorResponseModel errorResponseModel = null;
            try
            {
                var categoryData = categoryService.GetCategoryDDL(ref errorResponseModel);
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
                        ResponseMessage="Data not found",
                        ResponseData = new CategoryModel(),
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
                    ResponseData = new CategoryModel(),
                }); 
            }
        }

        /// <summary>
        /// To get category list  
        /// </summary>
        /// <returns></returns>
        [HttpPost("GetAllCategory")]
        [Authorize]
        [ProducesResponseType(typeof(CategoryModel), 200)]
        [ProducesResponseType(typeof(string), 404)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(string), 500)]
        public IActionResult GetAllCategoryList([FromBody] CategorySearchModel categorySearchModel)
        {
            ErrorResponseModel errorResponseModel = null;
            try
            {
                var categoryData = categoryService.GetAllCategory(categorySearchModel,ref errorResponseModel);
                if (categoryData != null)
                {
                    return Ok(new APIResponse
                    {
                        ResponseStatusCode = GlobalConstants.OKStatus,
                        ResponseData = categoryData,
                    });
                }
                else
                {
                    return Ok(new APIResponse
                    {
                        ResponseStatusCode = GlobalConstants.NoDataStatus,//Data not found
                        ResponseMessage = "Data not found",
                        ResponseData = new List<CategoryModel>(),
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
                    ResponseData = new List<CategoryModel>(),
                });
            }
        }

        /// <summary>
        /// To get category by Id
        /// </summary>
        /// <param name="categoryId"></param>
        /// <returns></returns>
        [HttpGet("GetCategoryById/{categoryId}")]
        [Authorize]
        [ProducesResponseType(typeof(CategoryModel), 200)]
        [ProducesResponseType(typeof(string), 404)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(string), 500)]
        public IActionResult GetCategoryById(int categoryId)
        {
            ErrorResponseModel errorResponseModel = null;
            try
            {
                if (categoryId <= 0)
                {
                    return  Ok(new APIResponse
                    {
                        ResponseStatusCode = 400,//Bad request
                        ResponseMessage = "Bad request",
                        ResponseData = new CategoryModel(),
                    });
                }
                var categoryData = categoryService.GetCategoryById(categoryId, ref errorResponseModel);

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
                        ResponseData = new CategoryModel(),
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
                    ResponseData = new CategoryModel(),
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
        [ProducesResponseType(typeof(CategoryModel), 200)]
        [ProducesResponseType(typeof(string), 404)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(string), 500)]
        public IActionResult DeleteCategory(int categoryId)
        {
            ErrorResponseModel errorResponseModel = null;
            try
            {
                bool result = categoryService.DeleteCategory(categoryId, ref errorResponseModel);

                if (result)
                {
                    return Ok(new APIResponse
                    {
                        ResponseStatusCode = 200,
                        ResponseMessage = "Category successfully deleted",
                        ResponseData = new CategoryModel(),
                    });
                }
                else
                {
                    return Ok(new APIResponse
                    {
                        ResponseStatusCode = 213,//Fail to delete
                        ResponseMessage = "Fail to delete",
                        ErrorMessage= errorResponseModel.Message,
                        ResponseData = new CategoryModel(),
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
                    ResponseData = new CategoryModel(),
                });
            }
        }
    }
}
