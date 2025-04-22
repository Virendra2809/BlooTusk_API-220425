using BlooTusk.Business.Interface;
using BlooTusk.Common;
using BlooTusk.Entity.BlooTuskModel;
using BlooTusk.Model.Models;
using System.Net;

namespace BlooTusk.Business.Implementation
{
    public class NoteMasterService : INoteMasterService
    {
        BlooTuskContext context;
        public NoteMasterService(BlooTuskContext _context)
        {
            context = _context;
        }
        public string AddEditNote(NotemasterModel notemaster, ref ErrorResponseModel errorResponseModel)
        {
            string statusCode = "";
            bool result=false;
            Notemaster Notemaster = new Notemaster();
            try
            {
                if (notemaster.NotemasterId == 0)
                {
                    var isDuplicateCategory = context.Notemasters.Where(x => (x.Note).ToLower() == notemaster.Note.ToLower()).FirstOrDefault();

                    if (isDuplicateCategory == null)
                    {
                        Notemaster.Note = notemaster.Note;
                        Notemaster.CustomerId = notemaster.CustomerId;
                        Notemaster.RecStatus = notemaster.RecStatus;
                        Notemaster.CreatedBy = notemaster.CreatedBy;
                        Notemaster.CreatedDate = DateTime.Now;
                        context.Add(Notemaster);
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
                    var noteEntity = context.Notemasters.FirstOrDefault(x => x.NotemasterId == notemaster.NotemasterId);
                    if (noteEntity != null)
                    {
                        Notemaster.Note = notemaster.Note;
                        Notemaster.CustomerId = notemaster.CustomerId;
                        Notemaster.RecStatus = notemaster.RecStatus;
                   Notemaster.ModifyDate = DateTime.Now;
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

        public bool DeleteNote(int NoteMasterID, ref ErrorResponseModel errorResponseModel)
        {
            bool result = false;
            try
            {
                var noteEntity = context.Notemasters.FirstOrDefault(x => x.NotemasterId == NoteMasterID);
                if (noteEntity != null)
                {
                    noteEntity.RecStatus = "I";
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
      
        public List<NotemasterModel> GetNoteById(int CustomerId, ref ErrorResponseModel errorResponseModel)
        {

            List<NotemasterModel> notemasterModels = new List<NotemasterModel>();
             notemasterModels = (from noteMaster in context.Notemasters
                            where noteMaster.CustomerId == CustomerId //&& categoryMaster.RecStatus == "A"
                            select new NotemasterModel
                            {
                                NotemasterId = noteMaster.NotemasterId,
                                Note = noteMaster.Note,
                                CustomerId = noteMaster.CustomerId,
                                RecStatus = noteMaster.RecStatus,
                            }
                               ).ToList();

            if (notemasterModels == null)
            {
                errorResponseModel.StatusCode = HttpStatusCode.NotFound;
                errorResponseModel.Message = "No Data found";
            }

            return notemasterModels;
        }

   
      
    }
}
