using DAL.Entity;

namespace Services.Interface
{
    public interface IActivityLogService
    {
        bool LogActivity(AuditTrial audit);
        List<AuditTrial> GetUserActivityLog(string userName);
    }
}
