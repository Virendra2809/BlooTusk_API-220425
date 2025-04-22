using BlooTusk.Model.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlooTusk.Business.Interface
{
    public interface IAuditlogService
    {
        void AddLog(AuditlogModel log);
    }
}
