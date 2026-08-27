# Welco Microservice Creation & Gateway Integration Workflow

## Goal
Standardized step-by-step workflow to scaffold a new microservice, implement features following the established CQRS + Clean Architecture patterns, integrate it with the Ocelot API Gateway, and set up CI/CD deployment. Reusable for all future microservices.

---

## Prerequisites Checklist

- [ ] Solution file `Welco.sln` exists at root
- [ ] `Welco.Shared` project exists with shared infrastructure (DbContext, Repository, Result pattern, Behaviors, Localization, OpenApi)
- [ ] `Welco.API` (Gateway) project exists with Ocelot configured
- [ ] Naming convention decided: `{ServiceName}.Service.API` (e.g., `Appointment.Service.API`)
- [ ] GitHub repository with Actions enabled

---

## Phase 1: Project Scaffolding

### Step 1.1: Create the ASP.NET Core Web API Project

```bash
dotnet new webapi -n "{ServiceName}.Service.API" -o "{ServiceName}.Service.API" --framework net10.0
```

### Step 1.2: Add Project to Solution

```bash
dotnet sln add "{ServiceName}.Service.API/{ServiceName}.Service.API.csproj"
```

### Step 1.3: Add Project Reference to Welco.Shared

```bash
cd "{ServiceName}.Service.API"
dotnet add reference "../Welco.Shared/Welco.Shared.csproj"
```

### Step 1.4: Add Required NuGet Packages

```bash
cd "{ServiceName}.Service.API"
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer --version 10.0.11
dotnet add package Microsoft.EntityFrameworkCore.Design --version 10.0.11
dotnet add package Scalar.AspNetCore --version 2.17.1
dotnet add package Microsoft.OpenApi --version 2.7.5
dotnet add package Microsoft.AspNetCore.OpenApi --version 10.0.11
```

### Step 1.5: Create Folder Structure

```
{ServiceName}.Service.API/
├── Controllers/
│   └── {ServiceName}Controller.cs
├── Features/
│   └── {Domain}/
│       ├── Commands/
│       │   └── {Action}/
│       │       ├── {Action}Command.cs
│       │       ├── {Action}CommandHandler.cs
│       │       └── {Action}CommandValidator.cs
│       └── Queries/
│           └── {Action}/
│               ├── {Action}Query.cs
│               ├── {Action}QueryHandler.cs
│               └── {Action}QueryValidator.cs
├── {ServiceName}Routes/
│   └── {ServiceName}ApiRoutes.cs
├── Infrastructure/
│   └── Services/
│       └── (service-specific infrastructure, e.g., JwtTokenService)
├── Program.cs
├── appsettings.json
├── appsettings.Development.json
├── appsettings.Test.json
├── appsettings.Production.json
├── Dockerfile
├── web.config
└── Properties/
    └── launchSettings.json
```

> **NOTE:** Routes folder is named `{ServiceName}Routes/` (e.g., `AuthRoutes/`) — NOT just `Routes/`.

### Step 1.6: Update `.csproj` (if AssemblyName override needed)

If the folder name contains typos or you want a clean assembly name, add to `.csproj`:

```xml
<PropertyGroup>
  <AssemblyName>{ServiceName}.Service.API</AssemblyName>
</PropertyGroup>
```

Example: `UserManamgent.Service.API` folder → `UserManagement.Service.API` assembly.

Also add the standard content copy rules:

```xml
<ItemGroup>
  <None Update="web.config">
    <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    <CopyToPublishDirectory>PreserveNewest</CopyToPublishDirectory>
  </None>
  <Content Update="appsettings*.json">
    <CopyToOutputDirectory>Always</CopyToOutputDirectory>
    <CopyToPublishDirectory>Always</CopyToPublishDirectory>
  </Content>
</ItemGroup>
```

---

## Phase 2: Configuration Files

### Step 2.1: Create `appsettings.json`

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore": "Warning",
      "Microsoft.Hosting.Lifetime": "Information"
    }
  },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "DatabaseConnection": "Server=db65360.public.databaseasp.net; Database=db65360; User Id=db65360; Password=Mo@123456; Encrypt=True; TrustServerCertificate=True; MultipleActiveResultSets=True;"
  }
}
```

### Step 2.2: Create `appsettings.Development.json`

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft.AspNetCore": "Information",
      "Microsoft.EntityFrameworkCore": "Information",
      "Microsoft.Hosting.Lifetime": "Information"
    }
  },
  "ConnectionStrings": {
    "DatabaseConnection": "Server=db65360.public.databaseasp.net; Database=db65360; User Id=db65360; Password=Mo@123456; Encrypt=True; TrustServerCertificate=True; MultipleActiveResultSets=True;"
  },
  "Identity": {
    "RequiredDigit": true,
    "RequiredLength": 6,
    "RequireLowercase": true,
    "RequiredUniqueChars": 1,
    "RequireUppercase": true,
    "MaxFailedAttempts": 5,
    "LockoutTimeSpanInDays": 1,
    "RequireNonAlphanumeric": false,
    "AllowedUserNameCharacters": "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+",
    "RequireUniqueEmail": true,
    "RequireConfirmedEmail": false
  },
  "JwtSettings": {
    "Issuer": "https://localhost:{HTTPS_PORT}/",
    "Audience": "https://localhost:{HTTPS_PORT}/",
    "ExpiryInMinutes": 60,
    "RefreshTokenExpiryDays": 30,
    "Secret": "V5B?*77+gzD_pk+2!%ORg<i)<D$DH+Xf.nECc?];2l;"
  },
  "EmailSettings": {
    "Email": "mohamed7saber10tech@gmail.com",
    "Name": "Welco Team",
    "Host": "smtp.gmail.com",
    "Port": 587,
    "Username": "mohamed7saber10tech@gmail.com",
    "Password": "cfnknpcuvkrycolr",
    "VerificationCodeExpiryMinutes": 10
  }
}
```

### Step 2.3: Create `appsettings.Test.json`

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore": "Warning",
      "Microsoft.Hosting.Lifetime": "Information"
    }
  },
  "ConnectionStrings": {
    "DatabaseConnection": "Server=db65360.public.databaseasp.net; Database=db65360; User Id=db65360; Password=Mo@123456; Encrypt=True; TrustServerCertificate=True; MultipleActiveResultSets=True;"
  },
  "Identity": {
    "RequiredDigit": true,
    "RequiredLength": 6,
    "RequireLowercase": true,
    "RequiredUniqueChars": 1,
    "RequireUppercase": true,
    "MaxFailedAttempts": 5,
    "LockoutTimeSpanInDays": 1,
    "RequireNonAlphanumeric": false,
    "AllowedUserNameCharacters": "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+",
    "RequireUniqueEmail": true,
    "RequireConfirmedEmail": false
  },
  "JwtSettings": {
    "Issuer": "https://{service-name}.runasp.net/",
    "Audience": "https://{service-name}.runasp.net/",
    "ExpiryInMinutes": 60,
    "RefreshTokenExpiryDays": 30,
    "Secret": "V5B?*77+gzD_pk+2!%ORg<i)<D$DH+Xf.nECc?];2l;"
  },
  "EmailSettings": {
    "Email": "mohamed7saber10tech@gmail.com",
    "Name": "Welco Team",
    "Host": "smtp.gmail.com",
    "Port": 587,
    "Username": "mohamed7saber10tech@gmail.com",
    "Password": "cfnknpcuvkrycolr",
    "VerificationCodeExpiryMinutes": 10
  }
}
```

### Step 2.4: Create `appsettings.Production.json`

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore": "Warning",
      "Microsoft.Hosting.Lifetime": "Information"
    }
  },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "DatabaseConnection": "Server=db65360.public.databaseasp.net; Database=db65360; User Id=db65360; Password=Mo@123456; Encrypt=True; TrustServerCertificate=True; MultipleActiveResultSets=True;"
  },
  "Identity": {
    "RequiredDigit": true,
    "RequiredLength": 6,
    "RequireLowercase": true,
    "RequiredUniqueChars": 1,
    "RequireUppercase": true,
    "MaxFailedAttempts": 5,
    "LockoutTimeSpanInDays": 1,
    "RequireNonAlphanumeric": false,
    "AllowedUserNameCharacters": "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+",
    "RequireUniqueEmail": true,
    "RequireConfirmedEmail": false
  },
  "JwtSettings": {
    "Issuer": "https://{service-name}.runasp.net/",
    "Audience": "https://{service-name}.runasp.net/",
    "ExpiryInMinutes": 60,
    "RefreshTokenExpiryDays": 30,
    "Secret": "V5B?*77+gzD_pk+2!%ORg<i)<D$DH+Xf.nECc?];2l;"
  },
  "EmailSettings": {
    "Email": "mohamed7saber10tech@gmail.com",
    "Name": "Welco Team",
    "Host": "smtp.gmail.com",
    "Port": 587,
    "Username": "mohamed7saber10tech@gmail.com",
    "Password": "cfnknpcuvkrycolr",
    "VerificationCodeExpiryMinutes": 10
  }
}
```

> **IMPORTANT:** Update `JwtSettings.Issuer` and `JwtSettings.Audience` per environment to match the service's actual URL.

### Step 2.5: Create `Properties/launchSettings.json`

```json
{
  "profiles": {
    "http": {
      "commandName": "Project",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      },
      "dotnetRunMessages": true,
      "launchBrowser": false,
      "applicationUrl": "http://localhost:{HTTP_PORT}"
    },
    "https": {
      "commandName": "Project",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      },
      "dotnetRunMessages": true,
      "launchBrowser": false,
      "applicationUrl": "https://localhost:{HTTPS_PORT};http://localhost:{HTTP_PORT}"
    },
    "IIS Express": {
      "commandName": "IISExpress",
      "launchBrowser": false,
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    }
  },
  "$schema": "https://json.schemastore.org/launchsettings.json",
  "iisSettings": {
    "windowsAuthentication": false,
    "anonymousAuthentication": true,
    "iisExpress": {
      "applicationUrl": "http://localhost:45726/",
      "sslPort": 44392
    }
  }
}
```

### Step 2.6: Create `Dockerfile`

```dockerfile
# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project files for caching layer
COPY ["{ServiceName}.Service.API/{ServiceName}.Service.API.csproj", "{ServiceName}.Service.API/"]
COPY ["Welco.Shared/Welco.Shared.csproj", "Welco.Shared/"]

# Restore dependencies
RUN dotnet restore "{ServiceName}.Service.API/{ServiceName}.Service.API.csproj"

# Copy source code and build
COPY . .
WORKDIR "/src/{ServiceName}.Service.API"
RUN dotnet publish "{ServiceName}.Service.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# Copy published artifacts from build stage
COPY --from=build /app/publish .

# Set default port for Render/Cloud environments
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "{ServiceName}.Service.API.dll"]
```

> **NOTE:** Dockerfiles don't currently exist on disk for any service. docker-compose.yml references them but they need to be created. The above template matches the pattern documented in docker-compose.yml.

### Step 2.7: Create `web.config` (for IIS/MonsterASP hosting)

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <location path="." inheritInChildApplications="false">
    <system.webServer>
      <handlers>
        <add name="aspnetcore" path="*" verb="*" modules="AspNetCoreModuleV2" resourceType="Unspecified" />
      </handlers>
      <aspNetCore processPath="dotnet" arguments=".\{ServiceName}.Service.API.dll" stdoutLogEnabled="false" stdoutLogFile=".\logs\stdout" hostingModel="inprocess" />
    </system.webServer>
  </location>
</configuration>
```

---

## Phase 3: Core Implementation (Program.cs)

### Step 3.1: Create `Program.cs`

Use the **Auth.Services.API** as reference. Key registrations:

```csharp
using System.Reflection;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using System.Text;
using Welco.Shared;
using Welco.Shared.Common.Behaviors;
using Welco.Shared.Common.Interfaces;
using Welco.Shared.Common.Middlewares;
using Welco.Shared.Common.Options;
using Welco.Shared.Localization;
using Welco.Shared.Localization.Interfaces;
using Welco.Shared.OpenApi;
using Welco.Shared.Persistance;
using Welco.Shared.Persistance.Seeding;

namespace {ServiceName}.Service.API
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

            var builder = WebApplication.CreateBuilder(new WebApplicationOptions
            {
                Args = args,
                EnvironmentName = environmentName,
                ContentRootPath = AppContext.BaseDirectory
            });

            var env = builder.Environment;

            // --- Configuration ---
            builder.Configuration.Sources.Clear();
            builder.Configuration
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true);

            if (env.IsDevelopment() || env.EnvironmentName == "Test")
            {
                var appAssembly = Assembly.Load(new AssemblyName(env.ApplicationName));
                if (appAssembly != null) builder.Configuration.AddUserSecrets(appAssembly, optional: true);
            }

            builder.Configuration.AddEnvironmentVariables().AddCommandLine(args);

            // --- Port Configuration ---
            var port = Environment.GetEnvironmentVariable("PORT")
                       ?? Environment.GetEnvironmentVariable("ASPNETCORE_HTTP_PORTS");
            if (!string.IsNullOrEmpty(port))
            {
                builder.WebHost.UseUrls($"http://*:{port}");
            }

            // --- Services ---
            builder.Services.AddControllers();
            builder.Services.AddJsonLocalization();
            builder.Services.AddWelcoSharedDependencies(builder.Configuration);
            builder.Services.AddWelcoIdentity(builder.Configuration);

            // --- JWT Settings ---
            builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection(JwtSettings.SectionName));
            var jwtSettings = builder.Configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>() ?? new JwtSettings();
            builder.Services.AddSingleton(jwtSettings);

            // --- Email Settings (if service sends emails) ---
            builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection(EmailSettings.SectionName));
            var emailSettings = builder.Configuration.GetSection(EmailSettings.SectionName).Get<EmailSettings>() ?? new EmailSettings();
            builder.Services.AddSingleton(emailSettings);

            // --- MediatR + FluentValidation ---
            builder.Services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssembly(typeof(Program).Assembly);
                cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
            });
            builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);

            // --- JWT Authentication ---
            var secretKey = !string.IsNullOrWhiteSpace(jwtSettings.Secret) && jwtSettings.Secret.Length >= 32
                ? jwtSettings.Secret
                : "V5B?*77+gzD_pk+2!%ORg<i)<D$DH+Xf.nECc?];2l;";

            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
                ValidateIssuer = !string.IsNullOrWhiteSpace(jwtSettings.Issuer),
                ValidIssuer = !string.IsNullOrWhiteSpace(jwtSettings.Issuer) ? jwtSettings.Issuer : null,
                ValidateAudience = !string.IsNullOrWhiteSpace(jwtSettings.Audience),
                ValidAudience = !string.IsNullOrWhiteSpace(jwtSettings.Audience) ? jwtSettings.Audience : null,
                RequireExpirationTime = true,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero,
            };
            builder.Services.AddSingleton(tokenValidationParameters);

            builder.Services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.SaveToken = true;
                options.TokenValidationParameters = tokenValidationParameters;
                options.Events = new JwtBearerEvents
                {
                    OnChallenge = context =>
                    {
                        context.HandleResponse();
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        context.Response.ContentType = "application/json";

                        var localizer = context.HttpContext.RequestServices.GetService<ILocalizationProvider>();
                        var localizedMessage = localizer?.GetLocalizedString(LocalizationKeys.ExceptionMessages.Unauthorized);

                        var result = System.Text.Json.JsonSerializer.Serialize(new
                        {
                            isSuccess = false,
                            statusCode = StatusCodes.Status401Unauthorized,
                            message = localizedMessage,
                            errors = new[] { localizedMessage },
                            data = (object?)null
                        });

                        return context.Response.WriteAsync(result);
                    }
                };
            });

            // --- OpenAPI + CORS ---
            builder.Services.AddConfiguredOpenApi();
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyHeader()
                          .AllowAnyMethod();
                });
            });

            var app = builder.Build();

            // --- Role Seeding ---
            using (var scope = app.Services.CreateScope())
            {
                try
                {
                    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
                    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
                    await RoleSeeder.SeedRolesAsync(roleManager, logger);
                }
                catch (Exception)
                {
                }
            }

            // --- Middleware Pipeline ---
            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedFor |
                                   Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedProto
            });

            app.UseCustomExceptionHandler();
            app.UseJsonLocalization();

            if (!app.Environment.IsEnvironment("Test") && !app.Environment.IsProduction())
            {
                app.UseHttpsRedirection();
            }
            app.UseCors("AllowAll");
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapGet("/", () => Results.Redirect("/scalar/v1"));
            app.MapOpenApi();
            app.MapScalarApiReference(options =>
            {
                options.WithTitle("{ServiceName} Microservice API")
                       .WithTheme(ScalarTheme.Moon);
            });
            app.MapControllers();

            app.Run();
        }
    }
}
```

**Critical middleware registrations (do NOT skip):**
- `AddJsonLocalization()` — required for localization to work
- `AddWelcoSharedDependencies()` — registers DB context, repositories, UnitOfWork, CurrentUserService, EmailService
- `AddWelcoIdentity()` — registers ASP.NET Identity with custom options
- `AddConfiguredOpenApi()` — registers OpenAPI with `AcceptLanguageHeaderTransformer`
- `UseForwardedHeaders()` — required for reverse proxy/Render/MonsterASP
- `UseCustomExceptionHandler()` — global exception → JSON handler
- `UseJsonLocalization()` — request localization middleware
- `RoleSeeder.SeedRolesAsync()` — seeds roles from `UserType` enum on startup

---

## Phase 4: Feature Implementation (CQRS Pattern)

### Step 4.1: Create Routes File

**File:** `{ServiceName}Routes/{ServiceName}ApiRoutes.cs`

```csharp
namespace {ServiceName}.Service.API.{ServiceName}Routes
{
    public static class {ServiceName}ApiRoutes
    {
        public const string Root = "api";
        public const string Version = "v1";
        public const string Base = Root + "/" + Version + "/{service-name}";

        public static class {Domain}
        {
            public const string GetAll = "";
            public const string GetById = "{id}";
            public const string Create = "";
            public const string Update = "{id}";
            public const string Delete = "{id}";
        }
    }
}
```

> **Convention:** Folder is `{ServiceName}Routes/` (e.g., `AuthRoutes/`).

### Step 4.2: Create Command (DTO)

**File:** `Features/{Domain}/Commands/{Action}/{Action}Command.cs`

```csharp
using MediatR;
using Welco.Shared.Results;

namespace {ServiceName}.Service.API.Features.{Domain}.Commands.{Action}
{
    public class {Action}Command : IRequest<Result<{ResponseDto}>>
    {
        // Command properties here
    }
}
```

### Step 4.3: Create Command Handler

**File:** `Features/{Domain}/Commands/{Action}/{Action}CommandHandler.cs`

```csharp
using MediatR;
using Welco.Shared.Common.Repositories.Interfaces.Base;
using Welco.Shared.Localization;
using Welco.Shared.Results;

namespace {ServiceName}.Service.API.Features.{Domain}.Commands.{Action}
{
    public class {Action}CommandHandler : IRequestHandler<{Action}Command, Result<{ResponseDto}>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public {Action}CommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<{ResponseDto}>> Handle({Action}Command request, CancellationToken cancellationToken)
        {
            // Business logic here
            // Use _unitOfWork.GetRepository<TEntity, TKey>() for data access
            // Return Result<{ResponseDto}>.Success(...) or Result<{ResponseDto}>.BadRequest(...)
        }
    }
}
```

### Step 4.4: Create Command Validator

**File:** `Features/{Domain}/Commands/{Action}/{Action}CommandValidator.cs`

```csharp
using FluentValidation;
using Welco.Shared.Localization;

namespace {ServiceName}.Service.API.Features.{Domain}.Commands.{Action}
{
    public class {Action}CommandValidator : AbstractValidator<{Action}Command>
    {
        public {Action}CommandValidator()
        {
            // RuleFor(x => x.Property)
            //     .NotEmpty().WithMessage(LocalizationKeys.{...});
        }
    }
}
```

### Step 4.5: Create Query (DTO)

**File:** `Features/{Domain}/Queries/{Action}/{Action}Query.cs`

```csharp
using MediatR;
using Welco.Shared.Results;

namespace {ServiceName}.Service.API.Features.{Domain}.Queries.{Action}
{
    public class {Action}Query : IRequest<Result<{ResponseDto}>>
    {
        // Query parameters here
    }
}
```

### Step 4.6: Create Query Handler

**File:** `Features/{Domain}/Queries/{Action}/{Action}QueryHandler.cs`

```csharp
using MediatR;
using Welco.Shared.Common.Repositories.Interfaces.Base;
using Welco.Shared.Localization;
using Welco.Shared.Results;

namespace {ServiceName}.Service.API.Features.{Domain}.Queries.{Action}
{
    public class {Action}QueryHandler : IRequestHandler<{Action}Query, Result<{ResponseDto}>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public {Action}QueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<{ResponseDto}>> Handle({Action}Query request, CancellationToken cancellationToken)
        {
            // Business logic here
        }
    }
}
```

### Step 4.7: Create Query Validator

**File:** `Features/{Domain}/Queries/{Action}/{Action}QueryValidator.cs`

```csharp
using FluentValidation;
using Welco.Shared.Localization;

namespace {ServiceName}.Service.API.Features.{Domain}.Queries.{Action}
{
    public class {Action}QueryValidator : AbstractValidator<{Action}Query>
    {
        public {Action}QueryValidator()
        {
            // Validation rules here
        }
    }
}
```

### Step 4.8: Create Controller

**File:** `Controllers/{ServiceName}Controller.cs`

```csharp
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Welco.Shared.Common.Attributes;
using Welco.Shared.Controllers;
using {ServiceName}.Service.API.{ServiceName}Routes;

namespace {ServiceName}.Service.API.Controllers
{
    [Route({ServiceName}ApiRoutes.Base)]
    public class {ServiceName}Controller : AppControllerBase
    {
        public {ServiceName}Controller(IMediator mediator) : base(mediator)
        {
        }

        [HttpGet]
        [RoleAuthorize]
        [Route({ServiceName}ApiRoutes.{Domain}.GetAll)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new GetAllQuery(), cancellationToken);
            return ToActionResult(result);
        }

        [HttpGet]
        [RoleAuthorize]
        [Route({ServiceName}ApiRoutes.{Domain}.GetById)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById([FromRoute] Guid id, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new GetByIdQuery { Id = id }, cancellationToken);
            return ToActionResult(result);
        }

        [HttpPost]
        [RoleAuthorize]
        [Route({ServiceName}ApiRoutes.{Domain}.Create)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] CreateCommand command, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(command, cancellationToken);
            return ToActionResult(result);
        }

        [HttpPut]
        [RoleAuthorize]
        [Route({ServiceName}ApiRoutes.{Domain}.Update)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdateCommand command, CancellationToken cancellationToken)
        {
            command.Id = id;
            var result = await _mediator.Send(command, cancellationToken);
            return ToActionResult(result);
        }

        [HttpDelete]
        [RoleAuthorize]
        [Route({ServiceName}ApiRoutes.{Domain}.Delete)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete([FromRoute] Guid id, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new DeleteCommand { Id = id }, cancellationToken);
            return ToActionResult(result);
        }
    }
}
```

---

## Phase 5: Gateway Integration (Ocelot)

### Step 5.1: Create Ocelot Route Files

Create **3 files** in `Welco.API/Ocelot/` (one per environment). The Gateway auto-discovers all `ocelot.*.{env}.json` files at startup — no manual registration needed.

#### `ocelot.{service-name}.Development.json`

```json
{
  "Routes": [
    {
      "DownstreamPathTemplate": "/api/v1/{service-name}/",
      "DownstreamScheme": "https",
      "DownstreamHostAndPorts": [
        {
          "Host": "localhost",
          "Port": {HTTPS_PORT}
        }
      ],
      "UpstreamPathTemplate": "/api/v1/{service-name}/",
      "UpstreamHttpMethod": [ "GET" ],
      "DangerousAcceptAnyServerCertificateValidator": true
    },
    {
      "DownstreamPathTemplate": "/api/v1/{service-name}/{id}",
      "DownstreamScheme": "https",
      "DownstreamHostAndPorts": [
        {
          "Host": "localhost",
          "Port": {HTTPS_PORT}
        }
      ],
      "UpstreamPathTemplate": "/api/v1/{service-name}/{id}",
      "UpstreamHttpMethod": [ "GET", "PUT", "DELETE" ],
      "DangerousAcceptAnyServerCertificateValidator": true
    },
    {
      "DownstreamPathTemplate": "/api/v1/{service-name}/",
      "DownstreamScheme": "https",
      "DownstreamHostAndPorts": [
        {
          "Host": "localhost",
          "Port": {HTTPS_PORT}
        }
      ],
      "UpstreamPathTemplate": "/api/v1/{service-name}/",
      "UpstreamHttpMethod": [ "POST" ],
      "DangerousAcceptAnyServerCertificateValidator": true
    },
    {
      "DownstreamPathTemplate": "/openapi/v1.json",
      "DownstreamScheme": "https",
      "DownstreamHostAndPorts": [
        {
          "Host": "localhost",
          "Port": {HTTPS_PORT}
        }
      ],
      "UpstreamPathTemplate": "/api/docs/{service-name}/openapi.json",
      "UpstreamHttpMethod": [ "GET" ],
      "DangerousAcceptAnyServerCertificateValidator": true
    },
    {
      "DownstreamPathTemplate": "/scalar/v1",
      "DownstreamScheme": "https",
      "DownstreamHostAndPorts": [
        {
          "Host": "localhost",
          "Port": {HTTPS_PORT}
        }
      ],
      "UpstreamPathTemplate": "/api/docs/{service-name}",
      "UpstreamHttpMethod": [ "GET" ],
      "DangerousAcceptAnyServerCertificateValidator": true
    }
  ]
}
```

#### `ocelot.{service-name}.Test.json`

```json
{
  "Routes": [
    {
      "DownstreamPathTemplate": "/api/v1/{service-name}/",
      "DownstreamScheme": "https",
      "DownstreamHostAndPorts": [
        {
          "Host": "{service-name}.runasp.net",
          "Port": 443
        }
      ],
      "UpstreamPathTemplate": "/api/v1/{service-name}/",
      "UpstreamHttpMethod": [ "GET" ],
      "DangerousAcceptAnyServerCertificateValidator": true
    },
    {
      "DownstreamPathTemplate": "/api/v1/{service-name}/{id}",
      "DownstreamScheme": "https",
      "DownstreamHostAndPorts": [
        {
          "Host": "{service-name}.runasp.net",
          "Port": 443
        }
      ],
      "UpstreamPathTemplate": "/api/v1/{service-name}/{id}",
      "UpstreamHttpMethod": [ "GET", "PUT", "DELETE" ],
      "DangerousAcceptAnyServerCertificateValidator": true
    },
    {
      "DownstreamPathTemplate": "/api/v1/{service-name}/",
      "DownstreamScheme": "https",
      "DownstreamHostAndPorts": [
        {
          "Host": "{service-name}.runasp.net",
          "Port": 443
        }
      ],
      "UpstreamPathTemplate": "/api/v1/{service-name}/",
      "UpstreamHttpMethod": [ "POST" ],
      "DangerousAcceptAnyServerCertificateValidator": true
    },
    {
      "DownstreamPathTemplate": "/openapi/v1.json",
      "DownstreamScheme": "https",
      "DownstreamHostAndPorts": [
        {
          "Host": "{service-name}.runasp.net",
          "Port": 443
        }
      ],
      "UpstreamPathTemplate": "/api/docs/{service-name}/openapi.json",
      "UpstreamHttpMethod": [ "GET" ],
      "DangerousAcceptAnyServerCertificateValidator": true
    },
    {
      "DownstreamPathTemplate": "/scalar/v1",
      "DownstreamScheme": "https",
      "DownstreamHostAndPorts": [
        {
          "Host": "{service-name}.runasp.net",
          "Port": 443
        }
      ],
      "UpstreamPathTemplate": "/api/docs/{service-name}",
      "UpstreamHttpMethod": [ "GET" ],
      "DangerousAcceptAnyServerCertificateValidator": true
    }
  ]
}
```

#### `ocelot.{service-name}.Production.json`

```json
{
  "Routes": [
    {
      "DownstreamPathTemplate": "/api/v1/{service-name}/",
      "DownstreamScheme": "https",
      "DownstreamHostAndPorts": [
        {
          "Host": "{service-name}.welco.internal",
          "Port": 443
        }
      ],
      "UpstreamPathTemplate": "/api/v1/{service-name}/",
      "UpstreamHttpMethod": [ "GET" ],
      "DangerousAcceptAnyServerCertificateValidator": true
    },
    {
      "DownstreamPathTemplate": "/api/v1/{service-name}/{id}",
      "DownstreamScheme": "https",
      "DownstreamHostAndPorts": [
        {
          "Host": "{service-name}.welco.internal",
          "Port": 443
        }
      ],
      "UpstreamPathTemplate": "/api/v1/{service-name}/{id}",
      "UpstreamHttpMethod": [ "GET", "PUT", "DELETE" ],
      "DangerousAcceptAnyServerCertificateValidator": true
    },
    {
      "DownstreamPathTemplate": "/api/v1/{service-name}/",
      "DownstreamScheme": "https",
      "DownstreamHostAndPorts": [
        {
          "Host": "{service-name}.welco.internal",
          "Port": 443
        }
      ],
      "UpstreamPathTemplate": "/api/v1/{service-name}/",
      "UpstreamHttpMethod": [ "POST" ],
      "DangerousAcceptAnyServerCertificateValidator": true
    },
    {
      "DownstreamPathTemplate": "/openapi/v1.json",
      "DownstreamScheme": "https",
      "DownstreamHostAndPorts": [
        {
          "Host": "{service-name}.welco.internal",
          "Port": 443
        }
      ],
      "UpstreamPathTemplate": "/api/docs/{service-name}/openapi.json",
      "UpstreamHttpMethod": [ "GET" ],
      "DangerousAcceptAnyServerCertificateValidator": true
    },
    {
      "DownstreamPathTemplate": "/scalar/v1",
      "DownstreamScheme": "https",
      "DownstreamHostAndPorts": [
        {
          "Host": "{service-name}.welco.internal",
          "Port": 443
        }
      ],
      "UpstreamPathTemplate": "/api/docs/{service-name}",
      "UpstreamHttpMethod": [ "GET" ],
      "DangerousAcceptAnyServerCertificateValidator": true
    }
  ]
}
```

---

## Phase 6: Docker Compose Integration

### Step 6.1: Add Service to `docker-compose.yml`

```yaml
services:
  gateway:
    build:
      context: .
      dockerfile: Welco.API/Dockerfile
    ports:
      - "8080:8080"
    environment:
      - ASPNETCORE_ENVIRONMENT=Test
      - ASPNETCORE_HTTP_PORTS=8080
    depends_on:
      - auth-service
      - {service-name}

  auth-service:
    build:
      context: .
      dockerfile: Auth.Services.API/Dockerfile
    environment:
      - ASPNETCORE_ENVIRONMENT=Test
      - ASPNETCORE_HTTP_PORTS=8080

  {service-name}:
    build:
      context: .
      dockerfile: {ServiceName}.Service.API/Dockerfile
    environment:
      - ASPNETCORE_ENVIRONMENT=Test
      - ASPNETCORE_HTTP_PORTS=8080
```

---

## Phase 7: Entity & Domain Model (if new entities needed)

### Step 7.1: Create Entity in Welco.Shared

**File:** `Welco.Shared/Domain/Models/{EntityName}.cs`

```csharp
using Welco.Shared.Common.Classes;

namespace Welco.Shared.Domain.Models
{
    public class {EntityName} : BaseEntity<Guid>
    {
        // Properties here

        // Factory method (recommended)
        public static {EntityName} Create(/* params */)
        {
            return new {EntityName}
            {
                Id = Guid.NewGuid(),
                // set properties
            };
        }
    }
}
```

> **IMPORTANT:** Entities go in `Welco.Shared.Domain.Models`, NOT in the service project. All services share the same `WelcoDbContext`.

### Step 7.2: Create EF Configuration

**File:** `Welco.Shared/Persistance/Configurations/{EntityName}Configuration.cs`

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Welco.Shared.Domain.Models;

namespace Welco.Shared.Persistance.Configurations
{
    public class {EntityName}Configuration : IEntityTypeConfiguration<{EntityName}>
    {
        public void Configure(EntityTypeBuilder<{EntityName}> builder)
        {
            builder.ToTable("{TableName}");
            builder.HasKey(e => e.Id);
            // Additional configuration
        }
    }
}
```

### Step 7.3: Add DbSet to WelcoDbContext

Add to `Welco.Shared/Persistance/WelcoDbContext.cs`:

```csharp
public DbSet<{EntityName}> {EntityNames} { get; set; } = null!;
```

And register the configuration in `OnModelCreating`:

```csharp
builder.ApplyConfiguration(new {EntityName}Configuration());
```

### Step 7.4: Update IWelcoDbContext

Add the new DbSet to `Welco.Shared/Common/Interfaces/IWelcoDbContext.cs`:

```csharp
DbSet<{EntityName}> {EntityNames} { get; }
```

### Step 7.5: Create Migration

```bash
cd "{ServiceName}.Service.API"
dotnet ef migrations add {MigrationName} --project ../Welco.Shared --startup-project .
```

---

## Phase 8: Localization Keys (if new messages needed)

### Step 8.1: Add Keys to `LocalizationKeys.cs`

Add a new nested class in `Welco.Shared/Localization/LocalizationKeys.cs`:

```csharp
public static class {ServiceName}
{
    public const string Prefix = "{service-name}:";

    // Commands
    public const string CreateSuccess = Prefix + "create_success";
    public const string UpdateSuccess = Prefix + "update_success";
    public const string DeleteSuccess = Prefix + "delete_success";

    // Errors
    public const string NotFound = Prefix + "not_found";
    public const string AlreadyExists = Prefix + "already_exists";
}
```

### Step 8.2: Add Translations

**File:** `Welco.Shared/Localization/Resources/messages.en.json`

```json
{
  "{ServiceName}": {
    "create_success": "{Entity} created successfully",
    "update_success": "{Entity} updated successfully",
    "delete_success": "{Entity} deleted successfully",
    "not_found": "{Entity} not found",
    "already_exists": "{Entity} already exists"
  }
}
```

**File:** `Welco.Shared/Localization/Resources/messages.ar.json`

```json
{
  "{ServiceName}": {
    "create_success": "تم إنشاء {Entity} بنجاح",
    "update_success": "تم تحديث {Entity} بنجاح",
    "delete_success": "تم حذف {Entity} بنجاح",
    "not_found": "لم يتم العثور على {Entity}",
    "already_exists": "{Entity} موجود بالفعل"
  }
}
```

> **NOTE:** JSON localization uses dot-notation flattening. Nested keys become `Parent.Child` at runtime.

---

## Phase 9: CI/CD Pipeline (GitHub Actions)

### Step 9.1: Create GitHub Actions Workflow

Create `.github/workflows/deploy-{service-name}.yml`:

```yaml
name: Deploy {ServiceName} Microservice to MonsterASP

on:
  push:
    branches:
      - main
      - master
    paths:
      - '{ServiceName}.Service.API/**'
      - 'Welco.Shared/**'
      - '.github/workflows/deploy-{service-name}.yml'
      - '.github/workflows/ftp-deploy.py'
  workflow_dispatch:

jobs:
  build-and-deploy:
    name: Build & Deploy {ServiceName} Service
    runs-on: ubuntu-latest
    timeout-minutes: 20

    steps:
      - name: Checkout Code
        uses: actions/checkout@v4

      - name: Setup .NET SDK
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '10.0.x'

      - name: Publish {ServiceName} Microservice
        run: |
          dotnet publish "{ServiceName}.Service.API/{ServiceName}.Service.API.csproj" \
            -c Release \
            -o ./publish/{service-name}

      - name: Deploy to MonsterASP via FTP
        env:
          FTP_SERVER: ${{ secrets.{SERVICE_NAME}_FTP_SERVER }}
          FTP_USERNAME: ${{ secrets.{SERVICE_NAME}_FTP_USERNAME }}
          FTP_PASSWORD: ${{ secrets.{SERVICE_NAME}_FTP_PASSWORD }}
          LOCAL_DIR: ./publish/{service-name}
          REMOTE_DIR: /wwwroot
        run: python .github/workflows/ftp-deploy.py
```

### Step 9.2: Add GitHub Secrets

Go to repository **Settings → Secrets and variables → Actions** and add:

| Secret Name | Description |
|-------------|-------------|
| `{SERVICE_NAME}_FTP_SERVER` | FTP server hostname (e.g., `monsterasp.net`) |
| `{SERVICE_NAME}_FTP_USERNAME` | FTP username |
| `{SERVICE_NAME}_FTP_PASSWORD` | FTP password |

> **Naming convention:** `{SERVICE_NAME}` is UPPER_SNAKE_CASE of the service name (e.g., `AUTH`, `USER_MANAGEMENT`, `APPOINTMENT`).

### Step 9.3: Add Gateway Ocelot File to Gateway Deployment Trigger

Update `.github/workflows/deploy-gateway.yml` paths to include new Ocelot files:

```yaml
    paths:
      - 'Welco.API/**'
      - 'Welco.Shared/**'
      - '.github/workflows/deploy-gateway.yml'
      - '.github/workflows/ftp-deploy.py'
```

> The gateway workflow already triggers on `Welco.API/**` which includes the Ocelot folder, so no change needed.

---

## Phase 10: Verification & Testing

### Step 10.1: Build the Solution

```bash
dotnet build Welco.sln
```

### Step 10.2: Run the Service Standalone

```bash
dotnet run --project "{ServiceName}.Service.API"
```

Verify: OpenAPI docs at `/scalar/v1`, health endpoint responds.

### Step 10.3: Run via Gateway

```bash
dotnet run --project "Welco.API"
```

Verify: Access service through gateway routes at `/api/v1/{service-name}/...`

### Step 10.4: Docker Build & Run

```bash
docker-compose build {service-name}
docker-compose up -d
```

### Step 10.5: Verify Integration

1. Gateway OpenAPI docs show the new service in the dropdown at `/scalar/v1`
2. All routes are accessible through the gateway
3. Authentication works (if `[RoleAuthorize]` is used)
4. Localization works (`Accept-Language: ar` header returns Arabic messages)
5. CI/CD workflow runs on push to main/master

---

## Quick Reference: Port Assignment

| Service | Dev HTTP | Dev HTTPS | Test URL | Prod URL |
|---------|----------|-----------|----------|----------|
| Gateway | 5293 | 7166 | welco-gateway.runasp.net | welco.innovation.com |
| Auth | 5066 | 7203 | welco-authservice.runasp.net | welco-authservice.runasp.net |
| UserManagement | 5067 | 7204 | welco-usermanagement.runasp.net | welco-usermanagement.runasp.net |
| {Next Service} | 5068 | 7205 | {service-name}.runasp.net | {service-name}.runasp.net |

---

## Quick Reference: GitHub Actions Secrets Per Service

| Service | Secrets Prefix | FTP Server Secret |
|---------|---------------|-------------------|
| Auth | `AUTH_FTP_*` | `AUTH_FTP_SERVER` |
| UserManagement | `USER_MGMT_FTP_*` | `USER_MGMT_FTP_SERVER` |
| {Next Service} | `{SERVICE_NAME}_FTP_*` | `{SERVICE_NAME}_FTP_SERVER` |

---

## Summary Checklist

- [ ] **Phase 1:** Project created, added to solution, NuGet packages installed, folder structure created
- [ ] **Phase 2:** Config files created (4x appsettings, launchSettings, Dockerfile, web.config)
- [ ] **Phase 3:** Program.cs implemented with ALL registrations (Localization, SharedDependencies, Identity, MediatR, JWT, OpenApi, CORS, ForwardedHeaders, ExceptionHandler, RoleSeeding)
- [ ] **Phase 4:** Features implemented (Commands/Queries with Handlers + Validators)
- [ ] **Phase 5:** Controller created with route constants using `{ServiceName}Routes/` folder
- [ ] **Phase 6:** Ocelot route files created for all 3 environments
- [ ] **Phase 7:** Docker compose updated
- [ ] **Phase 8:** Entities/migrations created in Welco.Shared (if needed)
- [ ] **Phase 9:** Localization keys added to LocalizationKeys.cs + messages.en.json + messages.ar.json (if needed)
- [ ] **Phase 10:** GitHub Actions workflow created with FTP deployment
- [ ] **Phase 11:** GitHub secrets added for FTP credentials
- [ ] **Phase 12:** Solution builds, service runs standalone, gateway integration verified, CI/CD works
