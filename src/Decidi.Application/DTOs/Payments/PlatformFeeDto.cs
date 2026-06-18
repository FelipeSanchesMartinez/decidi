namespace Decidi.Application.DTOs.Payments;

public class PlatformFeeDto
{
    public Guid Id { get; set; }
    public DateTime EffectiveFrom { get; set; }
    public decimal ClientFee { get; set; }
    public decimal FreelancerFee { get; set; }
    public decimal CommissionPct { get; set; }
}
