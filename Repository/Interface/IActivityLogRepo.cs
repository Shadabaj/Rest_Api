using DAL;
using DAL.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Interface
{
    public interface IActivityLogRepo
    {

        bool ActivityLogFilter(AuditTrial audit);

        List<AuditTrial> GetUserActivityLog(string UserName);

    }
}
