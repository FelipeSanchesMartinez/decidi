namespace Decidi.Application.Interfaces;

public interface IMarketplaceMailer
{
    Task ProposalReceivedAsync(string clientUserId, string projectTitle, string freelancerName, Guid projectId, string webBaseUrl);
    Task ProposalAcceptedAsync(string freelancerUserId, string projectTitle, Guid projectId, string webBaseUrl);
    Task ProposalRejectedAsync(string freelancerUserId, string projectTitle, string webBaseUrl);
    Task ProjectCompletedAsync(string toUserId, string projectTitle, Guid projectId, string webBaseUrl);
}
