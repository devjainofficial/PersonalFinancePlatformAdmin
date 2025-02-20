using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PersonalFinancePlatformAdmin.Components;
using PersonalFinancePlatformAdmin.Components.Account;
using PersonalFinancePlatformAdmin.Data;
using PersonalFinancePlatformAdmin.Shared.Helpers;
using System.Text;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents();

builder.Services.AddCascadingAuthenticationState();

builder.Services.AddScoped<IdentityUserAccessor>();

builder.Services.AddScoped<IdentityRedirectManager>();

builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("PO+3e2/FL7y0Q3dmhqnTFzUnM+DTO4GMB/yWUhABMiM=")),
        ValidateIssuer = false,
        ValidateAudience = false
    };
});

builder.Services.AddAuthorization();

string connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? 
                          throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
         options.UseSqlServer(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddIdentityCore<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddSignInManager()
                .AddDefaultTokenProviders();

builder.Services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();

builder.Services.AddScoped<UserManager<ApplicationUser>>();

builder.Services.AddScoped<SignInManager<ApplicationUser>>();

builder.Services.AddScoped<JwtTokenService>();

builder.Services.AddCors(options =>
    {
        options.AddPolicy
        ("AllowAngularApp", policy => policy.WithOrigins("http://localhost:4200")
                                            .AllowAnyHeader()
                                            .AllowAnyMethod()
        );
    }
);

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen();

WebApplication app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);

    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.UseCors("AllowAngularApp");

app.MapControllers();

app.UseStaticFiles();

app.UseAntiforgery();

app.MapRazorComponents<App>()
   .AddInteractiveServerRenderMode();

app.MapAdditionalIdentityEndpoints();

app.Run();

#region Application Structure

/// PersonalFinancePlatform /          # Root directory
//│
//├── / src /                        # Source code
//│   ├── / Application /            # Application layer (use cases, DTOs, commands)
//│   │   ├── Services /             # Application services (business logic)
//│   │   ├── Dtos /                 # Data transfer objects
//│   │   ├── Commands /             # Commands for CQRS
//│   │   ├── Queries /              # Queries for CQRS
//│   │   ├── Interfaces /           # Interfaces for services
//│   │   └── Validators /           # Validation logic
//│   │
//│   ├── / Domain /                 # Domain layer (core business logic)
//│   │   ├── Entities /             # Core business entities (User, Transaction, etc.)
//│   │   ├── ValueObjects /         # Value objects (e.g., Money, Address)
//│   │   ├── Aggregates /           # Aggregate roots (e.g., Budget, FinancialPlan)
//│   │   ├── Events /               # Domain events
//│   │   ├── Repositories /         # Repository interfaces
//│   │   └── Exceptions /           # Custom domain exceptions
//│   │
//│   ├── / Infrastructure /         # Infrastructure layer (database, external APIs)
//│   │   ├── Data /                 # EF Core DbContext, Migrations
//│   │   ├── EfCoreRepositories     # Concrete repository implementations
//│   │   ├── Identity /             # Authentication & authorization
//│   │   ├── Services/              # Infrastructure services (Email, Payment)
//│   │   ├── Configurations/        # Configuration files
//│   │   ├── Caching/               # Caching (Redis, Memory Cache)
//│   │   ├── Messaging/             # Message broker (e.g., RabbitMQ, Kafka)
//│   │   ├── Logging/               # Logging services
//│   │   └── ThirdParty/            # External integrations (Stripe, Plaid)
//│   │
//│   ├── /Presentation/             # Presentation layer (Blazor Server & API)
//│   │   ├── Blazor/                # Blazor Server components for admin
//│   │   ├── API/                   # Controllers & API endpoints
//│   │   │   ├── Controllers/       # API controllers
//│   │   │   ├── Middlewares/       # Middleware components (JWT, Exception handling)
//│   │   │   ├── Filters/           # API filters
//│   │   │   ├── Extensions/        # Dependency Injection (DI) extensions
//│   │   │   ├── Security/          # Security configurations (CORS, Auth)
//│   │   │   ├── Swagger/           # API documentation (Swagger setup)
//│   │   │   └── HealthChecks/      # Health check endpoints
//│   │   ├── Validators/            # Fluent Validation for API models
//│   │   └── Views/                 # Razor Views for Blazor
//│   │
//│   ├── /Shared/                   # Shared modules between layers
//│   │   ├── Extensions/            # Extension methods
//│   │   ├── Constants/             # Static constants
//│   │   ├── Enums /                # Enumerations
//│   │   ├── Helpers/               # Utility classes
//│   │   └── Responses/             # Standard API responses
//│   │
//│   ├── /Tests/                    # Unit & integration tests
//│   │   ├── Domain.Tests/          # Domain layer tests
//│   │   ├── Application.Tests/     # Application layer tests
//│   │   ├── Infrastructure.Tests/  # Infrastructure layer tests
//│   │   ├── Presentation.Tests/    # API and Blazor tests
//│   │   └── Integration.Tests/     # End-to-end integration tests
//│
//├── /docs/                         # Documentation
//├── /deploy/                       # Deployment configurations
//├── /scripts/                      # Deployment, automation, or database scripts
//├── .gitignore                     # Git ignore file
//├── README.md                      # Project documentation
//├── appsettings.json               # Application settings
//├── PersonalFinancePlatform.sln    # Solution file
//Explanation of Layers
//1️. Domain Layer (Core Business Logic)
//Contains business entities, value objects, aggregates, and domain events.
//No dependencies on Infrastructure or Application layers.
//Only interacts with repositories (Interfaces).
//2️. Application Layer (Use Cases)
//Contains Services, DTOs, Commands/Queries, and Validators.
//Calls Domain layer to execute business logic.
//No direct database operations.
//3️. Infrastructure Layer (Persistence, External APIs)
//Implements Repositories, Database context, Identity, and Caching.
//Handles third-party integrations (Payment, Messaging, etc.).
//4️. Presentation Layer (API & Blazor)
//Includes Blazor Server frontend for Admin.
//API for external client communication.
//Middleware for JWT authentication & error handling.
//5️. Shared Components
//Shared utilities, constants, responses, and extension methods used across layers.

#endregion