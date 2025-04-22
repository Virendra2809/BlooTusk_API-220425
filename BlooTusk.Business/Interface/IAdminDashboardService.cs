using BlooTusk.Model.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlooTusk.Business.Interface
{
    public interface IAdminDashboardService
    {
        AdminDashboardModel AdminDashboardDetails(int MerchantId);
        MerchantStatementModel MerchantStatement(MerchantStatementRequest merchantUserModel);
        CustomerStatementModel CustomerStatement(MerchantStatementRequest userModel);
        List<MerchantDDLModel> GetMerchantDDL(ref ErrorResponseModel errorResponseModel);
     
        AdminGraphModel AdminGrapgData(int merchantId);
    }
}
