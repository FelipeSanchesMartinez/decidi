using Decidi.Domain.Entities;
using Decidi.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Decidi.Infrastructure.Data.Seed;

public static class DataSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        await context.Database.MigrateAsync();

        await SeedCategoriesAsync(context);
        await SeedSkillsAsync(context);
        await SeedUsersAsync(context, userManager);
        await SeedPlatformFeesAsync(context);
    }

    private static async Task SeedPlatformFeesAsync(AppDbContext context)
    {
        if (await context.PlatformFees.AnyAsync()) return;
        context.PlatformFees.Add(new PlatformFee
        {
            EffectiveFrom = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            ClientFee = 3.99m,
            FreelancerFee = 2.99m,
            CommissionPct = 12m,
            IsActive = true,
            Note = "Taxa inicial da plataforma."
        });
        await context.SaveChangesAsync();
    }

    private static async Task SeedCategoriesAsync(AppDbContext context)
    {
        if (await context.Categories.AnyAsync()) return;

        var categories = new List<Category>
        {
            new() { Name = "Desenvolvimento Web", Slug = "desenvolvimento-web", Description = "Sites, sistemas web e aplicações" },
            new() { Name = "Desenvolvimento Mobile", Slug = "desenvolvimento-mobile", Description = "Apps para iOS e Android" },
            new() { Name = "Design Gráfico", Slug = "design-grafico", Description = "Logos, identidade visual e materiais gráficos" },
            new() { Name = "Marketing Digital", Slug = "marketing-digital", Description = "SEO, mídias sociais e campanhas" },
            new() { Name = "Redação e Tradução", Slug = "redacao-traducao", Description = "Conteúdo, copywriting e tradução" },
            new() { Name = "Consultoria", Slug = "consultoria", Description = "Consultoria em TI, negócios e processos" },
            new() { Name = "Financeiro e Contabilidade", Slug = "financeiro-contabilidade", Description = "Serviços financeiros e contábeis" },
            new() { Name = "Suporte Técnico", Slug = "suporte-tecnico", Description = "Help desk e suporte de TI" },
            new() { Name = "Vídeo e Animação", Slug = "video-animacao", Description = "Edição de vídeo, motion graphics e animação" },
            new() { Name = "Engenharia de Software", Slug = "engenharia-software", Description = "Arquitetura, DevOps e engenharia" }
        };

        await context.Categories.AddRangeAsync(categories);
        await context.SaveChangesAsync();
    }

    private static async Task SeedSkillsAsync(AppDbContext context)
    {
        if (await context.Skills.AnyAsync()) return;

        var skills = new List<Skill>
        {
            // Backend
            new() { Name = "C#", Group = "Backend" },
            new() { Name = ".NET", Group = "Backend" },
            new() { Name = "ASP.NET Core", Group = "Backend" },
            new() { Name = "Node.js", Group = "Backend" },
            new() { Name = "Python", Group = "Backend" },
            new() { Name = "Java", Group = "Backend" },
            new() { Name = "PHP", Group = "Backend" },
            new() { Name = "Go", Group = "Backend" },
            new() { Name = "Ruby", Group = "Backend" },
            new() { Name = "Rust", Group = "Backend" },
            new() { Name = "Django", Group = "Backend" },
            new() { Name = "Flask", Group = "Backend" },
            new() { Name = "Spring Boot", Group = "Backend" },
            new() { Name = "Laravel", Group = "Backend" },
            new() { Name = "Express.js", Group = "Backend" },
            new() { Name = "NestJS", Group = "Backend" },
            new() { Name = "FastAPI", Group = "Backend" },
            new() { Name = "GraphQL", Group = "Backend" },
            new() { Name = "REST API", Group = "Backend" },
            new() { Name = "gRPC", Group = "Backend" },
            new() { Name = "Entity Framework", Group = "Backend" },
            new() { Name = "Dapper", Group = "Backend" },

            // Frontend
            new() { Name = "JavaScript", Group = "Frontend" },
            new() { Name = "TypeScript", Group = "Frontend" },
            new() { Name = "React", Group = "Frontend" },
            new() { Name = "Angular", Group = "Frontend" },
            new() { Name = "Vue.js", Group = "Frontend" },
            new() { Name = "HTML/CSS", Group = "Frontend" },
            new() { Name = "Blazor", Group = "Frontend" },
            new() { Name = "Next.js", Group = "Frontend" },
            new() { Name = "Tailwind CSS", Group = "Frontend" },
            new() { Name = "Svelte", Group = "Frontend" },
            new() { Name = "Nuxt.js", Group = "Frontend" },
            new() { Name = "SASS/SCSS", Group = "Frontend" },
            new() { Name = "Bootstrap", Group = "Frontend" },
            new() { Name = "Material UI", Group = "Frontend" },
            new() { Name = "jQuery", Group = "Frontend" },
            new() { Name = "Webpack", Group = "Frontend" },
            new() { Name = "Vite", Group = "Frontend" },
            new() { Name = "Storybook", Group = "Frontend" },
            new() { Name = "Redux", Group = "Frontend" },
            new() { Name = "Zustand", Group = "Frontend" },

            // Mobile
            new() { Name = "React Native", Group = "Mobile" },
            new() { Name = "Flutter", Group = "Mobile" },
            new() { Name = "Swift", Group = "Mobile" },
            new() { Name = "Kotlin", Group = "Mobile" },
            new() { Name = ".NET MAUI", Group = "Mobile" },
            new() { Name = "SwiftUI", Group = "Mobile" },
            new() { Name = "Jetpack Compose", Group = "Mobile" },
            new() { Name = "Xamarin", Group = "Mobile" },
            new() { Name = "Ionic", Group = "Mobile" },
            new() { Name = "Expo", Group = "Mobile" },
            new() { Name = "Firebase", Group = "Mobile" },
            new() { Name = "App Store Optimization", Group = "Mobile" },

            // Banco de Dados
            new() { Name = "SQL", Group = "Banco de Dados" },
            new() { Name = "PostgreSQL", Group = "Banco de Dados" },
            new() { Name = "MySQL", Group = "Banco de Dados" },
            new() { Name = "MongoDB", Group = "Banco de Dados" },
            new() { Name = "Redis", Group = "Banco de Dados" },
            new() { Name = "SQL Server", Group = "Banco de Dados" },
            new() { Name = "SQLite", Group = "Banco de Dados" },
            new() { Name = "Elasticsearch", Group = "Banco de Dados" },
            new() { Name = "DynamoDB", Group = "Banco de Dados" },
            new() { Name = "Cassandra", Group = "Banco de Dados" },
            new() { Name = "Neo4j", Group = "Banco de Dados" },
            new() { Name = "Oracle", Group = "Banco de Dados" },
            new() { Name = "MariaDB", Group = "Banco de Dados" },

            // DevOps & Cloud
            new() { Name = "Docker", Group = "DevOps & Cloud" },
            new() { Name = "Kubernetes", Group = "DevOps & Cloud" },
            new() { Name = "AWS", Group = "DevOps & Cloud" },
            new() { Name = "Azure", Group = "DevOps & Cloud" },
            new() { Name = "CI/CD", Group = "DevOps & Cloud" },
            new() { Name = "Linux", Group = "DevOps & Cloud" },
            new() { Name = "Google Cloud", Group = "DevOps & Cloud" },
            new() { Name = "Terraform", Group = "DevOps & Cloud" },
            new() { Name = "Ansible", Group = "DevOps & Cloud" },
            new() { Name = "Jenkins", Group = "DevOps & Cloud" },
            new() { Name = "GitHub Actions", Group = "DevOps & Cloud" },
            new() { Name = "GitLab CI", Group = "DevOps & Cloud" },
            new() { Name = "Nginx", Group = "DevOps & Cloud" },
            new() { Name = "Vercel", Group = "DevOps & Cloud" },
            new() { Name = "Netlify", Group = "DevOps & Cloud" },
            new() { Name = "Heroku", Group = "DevOps & Cloud" },
            new() { Name = "Monitoramento", Group = "DevOps & Cloud" },
            new() { Name = "Grafana", Group = "DevOps & Cloud" },

            // Design
            new() { Name = "UI/UX", Group = "Design" },
            new() { Name = "Figma", Group = "Design" },
            new() { Name = "Adobe Photoshop", Group = "Design" },
            new() { Name = "Adobe Illustrator", Group = "Design" },
            new() { Name = "Canva", Group = "Design" },
            new() { Name = "Prototipação", Group = "Design" },
            new() { Name = "Adobe XD", Group = "Design" },
            new() { Name = "Sketch", Group = "Design" },
            new() { Name = "Design System", Group = "Design" },
            new() { Name = "Wireframing", Group = "Design" },
            new() { Name = "User Research", Group = "Design" },
            new() { Name = "Design Responsivo", Group = "Design" },
            new() { Name = "Acessibilidade Web", Group = "Design" },
            new() { Name = "After Effects", Group = "Design" },
            new() { Name = "InDesign", Group = "Design" },
            new() { Name = "Identidade Visual", Group = "Design" },
            new() { Name = "Logo Design", Group = "Design" },

            // Marketing
            new() { Name = "SEO", Group = "Marketing" },
            new() { Name = "Google Ads", Group = "Marketing" },
            new() { Name = "Meta Ads", Group = "Marketing" },
            new() { Name = "E-mail Marketing", Group = "Marketing" },
            new() { Name = "Copywriting", Group = "Marketing" },
            new() { Name = "Analytics", Group = "Marketing" },
            new() { Name = "Social Media", Group = "Marketing" },
            new() { Name = "TikTok Ads", Group = "Marketing" },
            new() { Name = "LinkedIn Ads", Group = "Marketing" },
            new() { Name = "Google Analytics", Group = "Marketing" },
            new() { Name = "Marketing de Conteúdo", Group = "Marketing" },
            new() { Name = "Inbound Marketing", Group = "Marketing" },
            new() { Name = "Growth Hacking", Group = "Marketing" },
            new() { Name = "Automação de Marketing", Group = "Marketing" },
            new() { Name = "Branding", Group = "Marketing" },
            new() { Name = "CRM", Group = "Marketing" },
            new() { Name = "Funil de Vendas", Group = "Marketing" },

            // Redação & Conteúdo
            new() { Name = "Redação Publicitária", Group = "Redação & Conteúdo" },
            new() { Name = "Tradução PT-EN", Group = "Redação & Conteúdo" },
            new() { Name = "Tradução PT-ES", Group = "Redação & Conteúdo" },
            new() { Name = "Revisão de Textos", Group = "Redação & Conteúdo" },
            new() { Name = "Blog Posts", Group = "Redação & Conteúdo" },
            new() { Name = "UX Writing", Group = "Redação & Conteúdo" },
            new() { Name = "Storytelling", Group = "Redação & Conteúdo" },
            new() { Name = "Roteirização", Group = "Redação & Conteúdo" },
            new() { Name = "Legendagem", Group = "Redação & Conteúdo" },
            new() { Name = "Transcrição", Group = "Redação & Conteúdo" },

            // Vídeo & Animação
            new() { Name = "Premiere Pro", Group = "Vídeo & Animação" },
            new() { Name = "DaVinci Resolve", Group = "Vídeo & Animação" },
            new() { Name = "Motion Graphics", Group = "Vídeo & Animação" },
            new() { Name = "Animação 2D", Group = "Vídeo & Animação" },
            new() { Name = "Animação 3D", Group = "Vídeo & Animação" },
            new() { Name = "Blender", Group = "Vídeo & Animação" },
            new() { Name = "Cinema 4D", Group = "Vídeo & Animação" },
            new() { Name = "Edição de Vídeo", Group = "Vídeo & Animação" },
            new() { Name = "Color Grading", Group = "Vídeo & Animação" },
            new() { Name = "Lottie", Group = "Vídeo & Animação" },

            // Dados & IA
            new() { Name = "Machine Learning", Group = "Dados & IA" },
            new() { Name = "Deep Learning", Group = "Dados & IA" },
            new() { Name = "TensorFlow", Group = "Dados & IA" },
            new() { Name = "PyTorch", Group = "Dados & IA" },
            new() { Name = "Data Science", Group = "Dados & IA" },
            new() { Name = "Pandas", Group = "Dados & IA" },
            new() { Name = "NLP", Group = "Dados & IA" },
            new() { Name = "Computer Vision", Group = "Dados & IA" },
            new() { Name = "ChatGPT/OpenAI API", Group = "Dados & IA" },
            new() { Name = "LangChain", Group = "Dados & IA" },
            new() { Name = "ETL", Group = "Dados & IA" },
            new() { Name = "Data Visualization", Group = "Dados & IA" },

            // Segurança
            new() { Name = "Segurança da Informação", Group = "Segurança" },
            new() { Name = "Pentest", Group = "Segurança" },
            new() { Name = "OWASP", Group = "Segurança" },
            new() { Name = "Criptografia", Group = "Segurança" },
            new() { Name = "LGPD", Group = "Segurança" },
            new() { Name = "OAuth/JWT", Group = "Segurança" },

            // Gestão & Ferramentas
            new() { Name = "WordPress", Group = "Gestão & Ferramentas" },
            new() { Name = "Shopify", Group = "Gestão & Ferramentas" },
            new() { Name = "Power BI", Group = "Gestão & Ferramentas" },
            new() { Name = "Excel Avançado", Group = "Gestão & Ferramentas" },
            new() { Name = "Git", Group = "Gestão & Ferramentas" },
            new() { Name = "Jira", Group = "Gestão & Ferramentas" },
            new() { Name = "Scrum", Group = "Gestão & Ferramentas" },
            new() { Name = "Kanban", Group = "Gestão & Ferramentas" },
            new() { Name = "Notion", Group = "Gestão & Ferramentas" },
            new() { Name = "Trello", Group = "Gestão & Ferramentas" },
            new() { Name = "WooCommerce", Group = "Gestão & Ferramentas" },
            new() { Name = "Magento", Group = "Gestão & Ferramentas" },
            new() { Name = "HubSpot", Group = "Gestão & Ferramentas" },
            new() { Name = "Salesforce", Group = "Gestão & Ferramentas" },
            new() { Name = "SAP", Group = "Gestão & Ferramentas" },
            new() { Name = "ERP", Group = "Gestão & Ferramentas" },

            // Testes & Qualidade
            new() { Name = "Testes Unitários", Group = "Testes & Qualidade" },
            new() { Name = "Testes de Integração", Group = "Testes & Qualidade" },
            new() { Name = "Selenium", Group = "Testes & Qualidade" },
            new() { Name = "Cypress", Group = "Testes & Qualidade" },
            new() { Name = "Jest", Group = "Testes & Qualidade" },
            new() { Name = "Playwright", Group = "Testes & Qualidade" },
            new() { Name = "QA Manual", Group = "Testes & Qualidade" },
            new() { Name = "Postman/Insomnia", Group = "Testes & Qualidade" },
        };

        await context.Skills.AddRangeAsync(skills);
        await context.SaveChangesAsync();
    }

    private static async Task SeedUsersAsync(AppDbContext context, UserManager<ApplicationUser> userManager)
    {
        if (await userManager.FindByEmailAsync("cliente@decidi.com") is not null) return;

        var client = new ApplicationUser
        {
            UserName = "cliente@decidi.com",
            Email = "cliente@decidi.com",
            FullName = "Maria Silva",
            Role = UserRole.Client,
            EmailConfirmed = true
        };
        await userManager.CreateAsync(client, "Teste123!");

        var freelancer = new ApplicationUser
        {
            UserName = "freelancer@decidi.com",
            Email = "freelancer@decidi.com",
            FullName = "João Santos",
            Role = UserRole.Freelancer,
            EmailConfirmed = true
        };
        await userManager.CreateAsync(freelancer, "Teste123!");

        var skills = await context.Skills
            .Where(s => new[] { "C#", ".NET", "ASP.NET Core", "JavaScript", "React", "PostgreSQL", "Blazor", "Docker" }.Contains(s.Name))
            .ToListAsync();

        var profile = new FreelancerProfile
        {
            UserId = freelancer.Id,
            Title = "Desenvolvedor Full Stack .NET",
            Bio = "Desenvolvedor apaixonado por tecnologia com 5 anos de experiência em .NET, React e bancos de dados relacionais. Especializado em criar soluções web escaláveis e APIs robustas. Comprometido com código limpo, boas práticas e entregas dentro do prazo.",
            HourlyRate = 120,
            PortfolioUrl = "https://github.com/joaosantos",
            Skills = skills
        };

        await context.FreelancerProfiles.AddAsync(profile);
        await context.SaveChangesAsync();
    }
}
