using BlooTusk.Model.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlooTusk.Business.Interface
{
    public interface INoteMasterService
    {
        string AddEditNote(NotemasterModel noteMaster, ref ErrorResponseModel errorResponseModel);
        bool DeleteNote(int NotemasterID, ref ErrorResponseModel errorResponseModel);
        List<NotemasterModel> GetNoteById(int NotemasterID, ref ErrorResponseModel errorResponseModel);
       
    }
}
