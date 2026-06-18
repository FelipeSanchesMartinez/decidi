namespace Decidi.Application.DTOs.Common;

public class ClientStatsDto
{
    public int ActiveProjects { get; set; }
    public int ProposalsToReview { get; set; }
    public int Conversations { get; set; }
    public decimal TotalSpentApprox { get; set; }
}

public class FreelancerStatsDto
{
    public decimal EarningsThisMonth { get; set; }
    public double AcceptanceRate { get; set; }
    public int PendingProposals { get; set; }
    public int ActiveContracts { get; set; }
    public double AverageRating { get; set; }
    public int TotalReviews { get; set; }
}
