#nullable disable
using System;
using System.Collections.Generic;

namespace DAL.Entity;

public partial class TripPlan
{
    public int TripPlanId { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public decimal TargetAmount { get; set; }
    public decimal CollectedAmount { get; set; }
    public int? CreatedByUserId { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public virtual User CreatedByUser { get; set; }
    public virtual ICollection<TripPayment> TripPayments { get; set; } = new List<TripPayment>();
}

public partial class TripPayment
{
    public long TripPaymentId { get; set; }
    public int TripPlanId { get; set; }
    public int UserId { get; set; }
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; }
    public string PaymentStatus { get; set; }
    public DateTime PaidAt { get; set; }
    public string ReferenceCode { get; set; }
    public virtual TripPlan TripPlan { get; set; }
    public virtual User User { get; set; }
}
