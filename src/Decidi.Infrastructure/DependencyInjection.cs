using Decidi.Application.Interfaces;
using Decidi.Application.Services;
using Decidi.Domain.Entities;
using Decidi.Domain.Interfaces;
using Decidi.Infrastructure.Data;
using Decidi.Infrastructure.Repositories;
using Decidi.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Decidi.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 6;
                options.Password.RequireUppercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.User.RequireUniqueEmail = true;
            })
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();

        // Repositories
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IProjectRepository, ProjectRepository>();
        services.AddScoped<IProposalRepository, ProposalRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IConversationRepository, ConversationRepository>();
        services.AddScoped<IMilestoneRepository, MilestoneRepository>();
        services.AddScoped<IReviewRepository, ReviewRepository>();
        services.AddScoped<ISkillRepository, SkillRepository>();
        services.AddScoped<INotificationPreferencesRepository, NotificationPreferencesRepository>();
        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Services
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IProjectService, ProjectService>();
        services.AddScoped<IProposalService, ProposalService>();
        services.AddScoped<IChatService, ChatService>();
        services.AddScoped<IMilestoneService, MilestoneService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<IReviewService, ReviewService>();
        services.AddScoped<ISkillService, SkillService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddOptions<ResendOptions>()
            .Bind(configuration.GetSection(ResendOptions.SectionName));
        services.AddHttpClient<IEmailService, ResendEmailService>(client =>
        {
            client.BaseAddress = new Uri("https://api.resend.com/");
            client.Timeout = TimeSpan.FromSeconds(10);
        });
        services.AddSingleton<ISanitizer, InputSanitizer>();
        services.AddSingleton<IContactGuard, ContactGuard>();
        services.AddScoped<IPlatformFeeService, PlatformFeeService>();
        services.AddScoped<IMarketplaceMailer, MarketplaceMailer>();
        services.AddScoped<INotificationPreferencesService, NotificationPreferencesService>();
        services.AddMemoryCache();

        return services;
    }
}
