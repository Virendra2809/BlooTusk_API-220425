using BlooTusk.Model.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlooTusk.Business.Interface
{
    public interface ICategoryService
    {
        /// <summary>
        /// Method is used to add/edit Category
        /// </summary> 
        /// <param name="categoryMaster"></param>
        /// <returns></returns>
        string AddEditCategory(CategoryModel categoryMaster, ref ErrorResponseModel errorResponseModel);

        /// <summary>
        /// Method is used to delete Category
        /// </summary>
        /// <param name="categoryId"></param>
        /// <returns></returns>
        bool DeleteCategory(int categoryId, ref ErrorResponseModel errorResponseModel);

        /// <summary>
        /// Method is used to get CompanyDetails by companyid
        /// </summary>
        /// <param name="companyId"></param>
        /// <returns></returns>
        CategoryModel GetCategoryById(int categoryId, ref ErrorResponseModel errorResponseModel);

        /// <summary>
        /// Method is used to get all CompanyDetails
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        List<CategoryModel> GetAllCategory(CategorySearchModel categorySearchModel, ref ErrorResponseModel errorResponseModel);
        List<CategoryModel> GetCategoryDDL(ref ErrorResponseModel errorResponseModel);
    }
}
