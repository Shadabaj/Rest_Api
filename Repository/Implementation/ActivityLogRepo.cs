using DAL;
using DAL.Entity;
using Microsoft.EntityFrameworkCore;
using Repository.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Implementation
{
    public class ActivityLogRepo : Repository<AuditTrial>, IActivityLogRepo
    {
       

        public ActivityLogRepo(AppDbContext _Db):base(_Db) 
        {
           
        }

        public bool ActivityLogFilter(AuditTrial audit)
        {
          var transaction=  _db.Database.BeginTransaction();
            try
            {
                _db.Add(audit);
                _db.SaveChanges();
                transaction.Commit();
                return true;
            }
            catch (Exception ex) 
            { 
            
                transaction.Rollback();
                return false;
            
            }
        
        }

        public List<AuditTrial> GetUserActivityLog(string UserName)
        {
            return _db.AuditTrials.Where(x=>x.UserName == UserName).ToList();
        }
    }
}
