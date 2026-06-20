# BEXIS2 .NET Core Migration Checklist

This document provides a comprehensive checklist for migrating BEXIS2 from .NET Framework 4.8 to .NET 6 for Linux compatibility.

## Phase 1: Infrastructure Migration (Weeks 1-3)

### Core Components Migration

#### Week 1: Configuration and Utilities
- [ ] **AppConfiguration.cs** - Replace with dependency injection pattern
  - [ ] Replace `HttpContext.Current` with `IHttpContextAccessor`
  - [ ] Replace `ConfigurationManager` with `IConfiguration`
  - [ ] Replace `HostingEnvironment.MapPath` with `IWebHostEnvironment`
  - [ ] Update path handling for cross-platform compatibility
  - [ ] Add constructor injection for dependencies
  - [ ] Test configuration loading from appsettings.json

- [ ] **Project Files Conversion**
  - [ ] Convert all 65+ projects from packages.config to PackageReference
  - [ ] Update from .NET Framework 4.8 to .NET 6.0
  - [ ] Replace legacy project format with SDK-style projects
  - [ ] Update NuGet package references to .NET 6 compatible versions
  - [ ] Remove Windows-specific build configurations

#### Week 2: Session and Context Management
- [ ] **Session Management**
  - [ ] Replace `System.Web.SessionState` with ASP.NET Core session services
  - [ ] Implement `ISessionManager` interface for type-safe session access
  - [ ] Create session extension methods for backward compatibility
  - [ ] Configure distributed session storage for scalability
  - [ ] Test session persistence and timeout behavior

- [ ] **HTTP Context Handling**
  - [ ] Replace all static `HttpContext.Current` usage
  - [ ] Implement `IHttpContextAccessor` injection throughout codebase
  - [ ] Update request/response handling patterns
  - [ ] Test cross-platform HTTP context behavior

#### Week 3: Configuration System
- [ ] **Web.config to appsettings.json Migration**
  - [ ] Convert 18+ Web.config files to appsettings.json format
  - [ ] Create environment-specific configuration files
  - [ ] Implement strongly-typed configuration classes
  - [ ] Set up configuration validation and binding
  - [ ] Test configuration loading in different environments

### Testing Infrastructure Updates
- [ ] Update unit test projects to .NET 6
- [ ] Replace MSTest/NUnit with xUnit if needed
- [ ] Update test configuration and mocking frameworks
- [ ] Create integration tests for configuration loading
- [ ] Set up test databases for Linux environment

## Phase 2: Application Framework Migration (Weeks 4-7)

### Week 4: MVC Framework Migration
- [ ] **ASP.NET MVC 5 to ASP.NET Core MVC**
  - [ ] Replace `System.Web.Mvc` with `Microsoft.AspNetCore.Mvc`
  - [ ] Update controller base classes and action filters
  - [ ] Migrate custom model binders and value providers
  - [ ] Update view engine configurations
  - [ ] Test routing and action execution

- [ ] **View Engine Updates**
  - [ ] Replace custom view engines with view location expanders
  - [ ] Update Razor view syntax for .NET Core
  - [ ] Migrate HTML helpers to tag helpers where applicable
  - [ ] Test view resolution and rendering

### Week 5: Web API Migration
- [ ] **ASP.NET Web API 2 to ASP.NET Core Web API**
  - [ ] Replace `System.Web.Http` with `Microsoft.AspNetCore.Mvc`
  - [ ] Update API controller base classes
  - [ ] Migrate custom message handlers to middleware
  - [ ] Update serialization configuration
  - [ ] Test API endpoints and data binding

### Week 6: Authentication and Authorization
- [ ] **Security Framework Migration**
  - [ ] Replace `System.Web.Security` with ASP.NET Core Identity
  - [ ] Migrate custom authentication providers
  - [ ] Update authorization policies and attributes
  - [ ] Configure cookie authentication for compatibility
  - [ ] Test user authentication and role management

### Week 7: Dependency Injection and IoC
- [ ] **IoC Container Migration**
  - [ ] Integrate existing IoC configuration with ASP.NET Core DI
  - [ ] Replace Unity/Ninject registrations with built-in DI
  - [ ] Update service lifetimes and scoping
  - [ ] Test dependency resolution and injection

## Phase 3: Application Lifecycle and Startup (Weeks 8-9)

### Week 8: Global.asax to Startup.cs Migration
- [ ] **Application Lifecycle Events**
  - [ ] Replace `Application_Start` with `Startup.ConfigureServices`
  - [ ] Replace `Application_End` with hosted service shutdown
  - [ ] Convert `Application_BeginRequest` to middleware
  - [ ] Convert `Application_EndRequest` to middleware
  - [ ] Convert `Application_Error` to exception handling middleware
  - [ ] Convert `Session_Start` to session initialization middleware

- [ ] **Middleware Pipeline Configuration**
  - [ ] Set up middleware pipeline in correct order
  - [ ] Configure CORS, authentication, and error handling
  - [ ] Add custom middleware for BExIS-specific functionality
  - [ ] Test middleware execution order and behavior

### Week 9: Bundling and Optimization
- [ ] **Asset Management Migration**
  - [ ] Replace `System.Web.Optimization` with WebOptimizer
  - [ ] Update CSS and JavaScript bundling configuration
  - [ ] Configure static file serving for wwwroot
  - [ ] Test asset loading and optimization

## Phase 4: Module Migration (Weeks 10-12)

### Week 10: Core Modules
- [ ] **Primary Module Migration**
  - [ ] Migrate DCM (Data Collection Module)
  - [ ] Migrate DIM (Data Information Module)
  - [ ] Migrate SAM (Security Administration Module)
  - [ ] Update module routing and area configurations
  - [ ] Test module functionality and integration

### Week 11: Secondary Modules
- [ ] **Additional Module Migration**
  - [ ] Migrate VIM (Visualization Module)
  - [ ] Migrate BAM (Business Administration Module)
  - [ ] Migrate DDM (Data Discovery Module)
  - [ ] Update inter-module communication patterns
  - [ ] Test module isolation and data sharing

### Week 12: Third-party Dependencies
- [ ] **External Library Updates**
  - [ ] Update Telerik UI controls for ASP.NET Core
  - [ ] Migrate jQuery and JavaScript dependencies
  - [ ] Update database drivers and ORM configurations
  - [ ] Test all third-party integrations

## Phase 5: Linux Deployment (Week 13)

### Linux Environment Setup
- [ ] **Docker Configuration**
  - [ ] Create Dockerfile for .NET 6 application
  - [ ] Configure Linux-compatible file paths
  - [ ] Set up database connections for Linux
  - [ ] Configure logging for containerized environment

- [ ] **Production Deployment**
  - [ ] Set up reverse proxy (nginx/Apache)
  - [ ] Configure SSL certificates and HTTPS
  - [ ] Set up monitoring and health checks
  - [ ] Configure backup and maintenance procedures

### Performance and Security Testing
- [ ] **Load Testing**
  - [ ] Test application performance under load
  - [ ] Compare performance with .NET Framework version
  - [ ] Optimize bottlenecks and memory usage
  - [ ] Test session management under concurrent users

- [ ] **Security Validation**
  - [ ] Perform security scanning and penetration testing
  - [ ] Validate authentication and authorization workflows
  - [ ] Test CORS and API security configurations
  - [ ] Verify data protection and privacy compliance

## Critical Success Factors

### Code Quality Gates
- [ ] All existing unit tests pass
- [ ] Integration tests validate core functionality
- [ ] Performance benchmarks meet requirements
- [ ] Security scans show no critical vulnerabilities
- [ ] All configuration settings are properly migrated

### Documentation Updates
- [ ] Update deployment documentation for Linux
- [ ] Create troubleshooting guides for common issues
- [ ] Update development environment setup instructions
- [ ] Document breaking changes and migration notes
- [ ] Create rollback procedures

### Training and Knowledge Transfer
- [ ] Train development team on .NET Core differences
- [ ] Create code review guidelines for .NET Core
- [ ] Document new development patterns and practices
- [ ] Establish monitoring and maintenance procedures

## Risk Mitigation

### High-Risk Areas Requiring Special Attention
1. **Custom HTTP Modules** - Need complete rewrite as middleware
2. **File System Operations** - Ensure cross-platform compatibility
3. **Database Connections** - Test connection strings and drivers
4. **Third-party Controls** - May require significant updates
5. **Custom Authentication** - Complex migration to ASP.NET Core Identity

### Rollback Plan
- [ ] Maintain parallel .NET Framework deployment
- [ ] Create automated rollback scripts
- [ ] Document rollback procedures and timelines
- [ ] Test rollback scenarios in staging environment

### Success Metrics
- [ ] Application starts successfully on Linux
- [ ] All core functionality works as expected
- [ ] Performance is equal or better than .NET Framework
- [ ] No data loss during migration
- [ ] User authentication and authorization work correctly
- [ ] All modules load and function properly

## Post-Migration Tasks

### Monitoring and Optimization
- [ ] Set up application performance monitoring
- [ ] Configure logging aggregation and analysis
- [ ] Monitor memory usage and garbage collection
- [ ] Track user experience and error rates

### Continuous Improvement
- [ ] Plan for .NET LTS upgrade schedule
- [ ] Optimize Docker images and deployment process
- [ ] Implement automated testing and deployment pipelines
- [ ] Plan for future feature development on .NET Core

---

**Estimated Timeline: 13 weeks**
**Resource Requirements: 2-3 senior developers**
**Risk Level: Medium-High (due to extensive Windows dependencies)**
**Success Probability: High (with proper planning and testing)**