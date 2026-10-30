#nullable disable
using System;

namespace DAL.Entity;

public partial class LoginOtp
{
    public long Id { get; set; }
    public int UserId { get; set; }
    public string UserName { get; set; }
    public string Email { get; set; }
    public string CodeHash { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ConsumedAt { get; set; }
    public string CreatedByIp { get; set; }
    public virtual User User { get; set; }
}
