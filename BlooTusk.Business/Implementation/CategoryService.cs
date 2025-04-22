using BlooTusk.Business.Interface;
using BlooTusk.Common;
using BlooTusk.Entity.BlooTuskModel;
using BlooTusk.Model.Models;
using System.Net;

namespace BlooTusk.Business.Implementation
{
    public class CategoryService : ICategoryService
    {
        BlooTuskContext context;
        public CategoryService(BlooTuskContext _context)
        {
            context = _context;
        }
        public string AddEditCategory(CategoryModel categoryMaster, ref ErrorResponseModel errorResponseModel)
        {
            string statusCode = "";
            bool result=false; 
            Categorymaster categorymaster = new Categorymaster();

            try
            {
                if (categoryMaster.CategoryId == 0)
                {
                    var isDuplicateCategory = context.Categorymasters.Where(x => (x.CategoryName).ToLower() == categoryMaster.CategoryName.ToLower()).FirstOrDefault();

                    if (isDuplicateCategory == null)
                    {
                        categorymaster.CategoryName = categoryMaster.CategoryName;
                        categorymaster.RewardPoint = Convert.ToInt32(categoryMaster.RewardPoint);
                        categorymaster.RecStatus = "A";
                        categorymaster.CreatedBy = 1;
                        categorymaster.CreatedDate = DateTime.Now;
                        context.Add(categorymaster);
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
                    var categoryEntity = context.Categorymasters.FirstOrDefault(x => x.CategoryId == categoryMaster.CategoryId);
                    if (categoryEntity != null)
                    {
                        categoryEntity.CategoryName = categoryMaster.CategoryName;
                        categoryEntity.RewardPoint = Convert.ToInt32(categoryMaster.RewardPoint);
                        categoryEntity.ModifyBy = categorymaster.ModifyBy;
                        categoryEntity.ModifyDate = categorymaster.ModifyDate;
                        categoryEntity.RecStatus = categoryMaster.RecStatus;
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
                throw ;
            }
            
            return statusCode;
        }

        public bool DeleteCategory(int categoryId, ref ErrorResponseModel errorResponseModel)
        {
            bool result = false;
            try
            {
                var categoryEntity = context.Categorymasters.FirstOrDefault(x => x.CategoryId == categoryId);
                if (categoryEntity != null)
                {
                    categoryEntity.RecStatus = "I";
                    context.SaveChanges();
                    // Message = "Category sucessfully dateled";
                    result = true;
                }
            }
            catch (Exception ex)
            {
                errorResponseModel.Message = ex.Message;
                result = false;
            }
            
           
            return result;
        }

        //    merchant.MerchantCode.Contains(merchantSerachModel.Keyword, StringComparison.OrdinalIgnoreCase) ||

        public List<CategoryModel> GetAllCategory(CategorySearchModel categorySearchModel, ref ErrorResponseModel errorResponseModel)
        {
            errorResponseModel = new ErrorResponseModel();

            var categoryList = (from categoryMaster in context.Categorymasters
                            where (categorySearchModel.CategoryName == string.Empty || categoryMaster.CategoryName.Contains(categorySearchModel.CategoryName,StringComparison.OrdinalIgnoreCase) )&&// == categorySearchModel.CategoryName) &&
                                  (categorySearchModel.RewardPoint == 0 || categoryMaster.RewardPoint == categorySearchModel.RewardPoint)
                                 // && categoryMaster.RecStatus == "A"
                            select new CategoryModel
                            {
                                CategoryId = categoryMaster.CategoryId,
                                CategoryName = categoryMaster.CategoryName,
                                RewardPoint = categoryMaster.RewardPoint,
                                RecStatus = categoryMaster.RecStatus,

                            }).ToList();
            return categoryList;
           
        }

        public CategoryModel GetCategoryById(int categoryId, ref ErrorResponseModel errorResponseModel)
        {
            var categoryData = (from categoryMaster in context.Categorymasters
                                 where categoryMaster.CategoryId == categoryId //&& categoryMaster.RecStatus == "A"
                                  select new CategoryModel
                                  {
                                      CategoryId = categoryMaster.CategoryId,
                                      CategoryName = categoryMaster.CategoryName,
                                      
                                      RewardPoint = categoryMaster.RewardPoint,
                                      RecStatus=categoryMaster.RecStatus,
                                  }
                               ).SingleOrDefault();

            if (categoryData == null)
            {
                errorResponseModel.StatusCode = HttpStatusCode.NotFound;
                errorResponseModel.Message = "No Data found";
            }

            return categoryData;
        }

        public List<CategoryModel> GetCategoryDDL(ref ErrorResponseModel errorResponseModel)
        {

            errorResponseModel = new ErrorResponseModel();

            var categoryList = (from categoryMaster in context.Categorymasters
                                  where categoryMaster.RecStatus == "A"
                                  select new CategoryModel
                                  {
                                      CategoryId = categoryMaster.CategoryId,
                                      CategoryName = categoryMaster.CategoryName, 
                                  }
                               ).Distinct().ToList();

            if (categoryList == null)
            {
                errorResponseModel.StatusCode = HttpStatusCode.NotFound;
                errorResponseModel.Message = "No Data found";
            }
            //var categoryList = new List<CategoryModel>();
            //var categoryEntity = context.Categorymasters.Where(x => x.RecStatus == "A").ToList();

            //if (categoryEntity == null)
            //{
            //    errorResponseModel.StatusCode = HttpStatusCode.NotFound;
            //    errorResponseModel.Message = "No Data found";
            //}

            //categoryEntity.ForEach(item =>
            //{
            //    categoryList.Add(new CategoryModel
            //    {
            //        CategoryId = item.CategoryId,
            //        CategoryName = item.CategoryName,
            //    });
            //});
            return categoryList;
        }

      
    }
}
