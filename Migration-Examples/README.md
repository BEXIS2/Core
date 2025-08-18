# .NET Core Migration Examples

This directory contains concrete examples showing how to migrate Windows-dependent BEXIS2 components to .NET Core for Linux compatibility.

## Files Included

1. **AppConfiguration.NetCore.cs** - Refactored AppConfiguration class using dependency injection and IConfiguration
2. **Startup.cs** - Replacement for Global.asax.cs using ASP.NET Core startup patterns
3. **BExIS.Web.Shell.NetCore.csproj** - Example project file migration from .NET Framework to .NET 6
4. **SessionManagement.NetCore.cs** - Replacement patterns for System.Web.SessionState
5. **appsettings.json** - Configuration replacement for Web.config

## Key Migration Patterns

### 1. HttpContext.Current → IHttpContextAccessor
```csharp
// Old (.NET Framework)
HttpContext.Current.User

// New (.NET Core)
private readonly IHttpContextAccessor _httpContextAccessor;
_httpContextAccessor.HttpContext.User
```

### 2. HostingEnvironment.MapPath → IWebHostEnvironment
```csharp
// Old (.NET Framework)
HostingEnvironment.MapPath("~/Areas")

// New (.NET Core)
private readonly IWebHostEnvironment _environment;
Path.Combine(_environment.WebRootPath, "Areas")
```

### 3. ConfigurationManager → IConfiguration
```csharp
// Old (.NET Framework)
ConfigurationManager.AppSettings["key"]

// New (.NET Core)
private readonly IConfiguration _configuration;
_configuration["key"]
```

### 4. Global.asax → Startup.cs
Application lifecycle events are replaced with middleware pipeline configuration.

### 5. Web.config → appsettings.json
XML-based configuration is replaced with JSON-based configuration with strong typing support.