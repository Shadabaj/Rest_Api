using DAL.Entity;
using Repository.Interface;
using Services.Interface;

namespace Services.Implementation
{
    public class ActivityLogService : IActivityLogService
    {
        private readonly IActivityLogRepo _activityLogRepo;

        public ActivityLogService(IActivityLogRepo activityLogRepo)
        {
            _activityLogRepo = activityLogRepo;
        }

        public bool LogActivity(AuditTrial audit)
            => _activityLogRepo.ActivityLogFilter(audit);

        public List<AuditTrial> GetUserActivityLog(string userName)
            => _activityLogRepo.GetUserActivityLog(userName);
    }
}
