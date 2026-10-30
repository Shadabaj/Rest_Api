using DAL.Entity;
using Microsoft.EntityFrameworkCore;

namespace DAL;

public partial class AppDbContext
{
    public virtual DbSet<LoginOtp> LoginOtps { get; set; }
}
