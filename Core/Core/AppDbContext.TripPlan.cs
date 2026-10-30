using DAL.Entity;
using Microsoft.EntityFrameworkCore;

namespace DAL;

public partial class AppDbContext
{
    public virtual DbSet<TripPlan> TripPlans { get; set; }
    public virtual DbSet<TripPayment> TripPayments { get; set; }
}
