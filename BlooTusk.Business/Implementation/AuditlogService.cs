using BlooTusk.Business.Interface;
using BlooTusk.Entity.BlooTuskModel;
using BlooTusk.Model.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlooTusk.Business.Implementation
{
    public class AuditlogService : IAuditlogService
    {
        BlooTuskContext context;
       
        public AuditlogService(BlooTuskContext _context)
        {
            context = _context;           
        }

        public void AddLog(AuditlogModel log)
        {
            try
            {
                Auditlog auditlog = new Auditlog();
                auditlog.RequestedApiname = log.RequestedApiname;
                auditlog.InputData=log.InputData;
                auditlog.RequestStatus = log.RequestStatus;
                auditlog.ErrorMessage = log.ErrorMessage;
                context.Auditlogs.Add(auditlog);
                context.SaveChanges();
            }
            catch (Exception ex)
            {

                throw;
            }

        }
    }
}
