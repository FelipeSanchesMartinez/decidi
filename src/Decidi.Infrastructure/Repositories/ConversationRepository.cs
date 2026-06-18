using Decidi.Domain.Entities;
using Decidi.Domain.Interfaces;
using Decidi.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Decidi.Infrastructure.Repositories;

public class ConversationRepository(AppDbContext context) : Repository<Conversation>(context), IConversationRepository
{
    public async Task<Conversation?> GetConversationWithMessagesAsync(Guid conversationId, int messageCount = 50)
    {
        return await _dbSet
            .Include(c => c.Client)
            .Include(c => c.Freelancer)
            .Include(c => c.Project)
            .Include(c => c.Messages.OrderByDescending(m => m.CreatedAt).Take(messageCount))
                .ThenInclude(m => m.Sender)
            .FirstOrDefaultAsync(c => c.Id == conversationId);
    }

    public async Task<IEnumerable<Conversation>> GetUserConversationsAsync(string userId)
    {
        return await _dbSet
            .Where(c => c.ClientId == userId || c.FreelancerId == userId)
            .Include(c => c.Client)
            .Include(c => c.Freelancer)
            .Include(c => c.Project)
            .Include(c => c.Messages.OrderByDescending(m => m.CreatedAt).Take(1))
            .OrderByDescending(c => c.Messages.Max(m => (DateTime?)m.CreatedAt) ?? c.CreatedAt)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Conversation?> GetByProjectAndUsersAsync(Guid projectId, string clientId, string freelancerId)
    {
        return await _dbSet.FirstOrDefaultAsync(c =>
            c.ProjectId == projectId &&
            c.ClientId == clientId &&
            c.FreelancerId == freelancerId);
    }

    public async Task<Conversation?> GetWithProjectAsync(Guid conversationId)
    {
        return await _dbSet
            .Include(c => c.Project)
            .Include(c => c.Client)
            .Include(c => c.Freelancer)
            .FirstOrDefaultAsync(c => c.Id == conversationId);
    }
}
