#nullable disable
using System;

namespace DAL.Entity;

public partial class RefreshToken
{
    public long Id { get; set; }

    public int UserId { get; set; }

    public string TokenHash { get; set; }

    public DateTime ExpiresAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? RevokedAt { get; set; }

    public string ReplacedByTokenHash { get; set; }

    public string CreatedByIp { get; set; }

    public virtual User User { get; set; }
}
