# BEXIS2 Core Development Instructions

Always reference these instructions first and fallback to search or bash commands only when you encounter unexpected information that does not match the info here.

BEXIS2 is a comprehensive data management platform built with .NET Framework 4.8, ASP.NET MVC, C#, and PostgreSQL. It features a modular architecture with 76+ C# projects and a modern SvelteKit frontend.

## Working Effectively

### Bootstrap and Build the Repository
- Install Visual Studio 2019+ or .NET Framework 4.8 SDK (Windows required for .NET Framework projects)
- Install Node.js 20+ and npm for Svelte frontend components
- Install PostgreSQL 12+ database server
- Clone and set up the BEXIS2 Workspace repository (separate dependency)

### Build Commands (NEVER CANCEL - Set 60+ minute timeouts)
**Svelte Frontend (Works on Linux/macOS/Windows):**
```bash
cd Console/BExIS.Web.Shell.Svelte
rm -rf node_modules package-lock.json  # If npm install fails
npm install                             # 2-3 minutes. May need retry due to rollup issues
npm run build                          # 13 seconds - builds frontend assets
npm run test:unit                      # Runs unit tests with Vitest
npm run lint                           # Lints TypeScript/Svelte code (has formatting issues)
npm run check                          # TypeScript checking (has known errors in docs processing)
```

**.NET Framework Projects (Windows Only):**
```bash
# Using Visual Studio or MSBuild
msbuild BExIS++.sln /p:Configuration=Debug    # 15-20 minutes. NEVER CANCEL. Set timeout to 60+ minutes
# Or using dotnet (limited compatibility)
dotnet restore BExIS++.sln                    # May fail on some projects - use NuGet Package Manager in VS
```

**Database Setup:**
- Install PostgreSQL server locally (port 5432)
- Create database: `bexis2test` (for development/testing)
- Default credentials: username=`postgres`, password=`1`
- Connection string format: `Server=localhost;Port=5432;Database=bexis2test;Userid=postgres;Password=1;Pooling=true;MinPoolSize=2;MaxPoolSize=100;ConnectionIdleLifetime=3600;`

### Running the Application
**NEVER run without proper database and workspace setup - the application will fail**

**Frontend Development (Independent):**
```bash
cd Console/BExIS.Web.Shell.Svelte
npm run dev                            # Starts SvelteKit dev server on port 5173
npm run preview                        # Preview production build
```

**Full Application (.NET + Database Required):**
- Set up Web.config from Web.config.sample with proper database connection
- Configure workspace path in Web.config
- Use IIS Express or Visual Studio F5 to run BExIS.Web.Shell project
- Application requires workspace files and database to start properly

## Validation

### ALWAYS Test Frontend Changes
- Run `npm run build` in Console/BExIS.Web.Shell.Svelte - NEVER CANCEL (13 seconds)
- Run `npm run test:unit` to execute unit tests
- Run `npm run lint` before committing - **NOTE**: Has known formatting issues to fix
- TypeScript checking (`npm run check`) has known errors in documentation processing code

### Manual Validation Scenarios
**After making frontend changes:**
1. Build Svelte project successfully
2. Run unit tests and ensure they pass
3. Test in browser that components render correctly
4. Verify no console errors in browser developer tools

**After making .NET changes:**
1. Build solution in Visual Studio without errors
2. Run relevant NUnit tests (if database available)
3. Start application and verify it loads without exceptions
4. Test basic navigation and module functionality

### Testing Infrastructure
- **Frontend**: Vitest unit tests, Playwright end-to-end tests, ESLint + Prettier
- **.NET**: NUnit unit tests, MSTest integration tests
- **Test command timing**: Frontend tests: 1-2 seconds, .NET tests: varies by scope

## Important File Locations

### Configuration Files
- `Console/BExIS.Web.Shell/Web.config.sample` - Main application configuration template
- `Console/BExIS.Web.Shell.Svelte/package.json` - Frontend dependencies and scripts
- `BExIS++.sln` - Main Visual Studio solution file (76 projects)
- `.nuget/NuGet.Config` - NuGet package source configuration

### Key Directories
- `Console/BExIS.Web.Shell/` - Main ASP.NET MVC web application
- `Console/BExIS.Web.Shell.Svelte/` - Modern SvelteKit frontend
- `Components/` - Core system components (AAA, DLM, DCM, IO, etc.)
- `Modules/` - Feature modules (RPM, SAM, DIM, etc.)
- `Libraries/` - External dependencies
- `database update scripts/` - SQL scripts for database migrations

### Project Architecture
**Components by Function:**
- `AAA` - Authentication, Authorization, Accounting
- `DLM` - Data Lifecycle Management
- `DCM` - Data Collection Management  
- `DDM` - Data Discovery Management
- `IO` - Input/Output operations
- `XML` - XML processing utilities
- `Utils` - Common utilities
- `Vaiona` - Core framework libraries

## Common Commands Reference

### Build Timing Expectations
- Svelte npm install: 2-3 minutes (may require retry)
- Svelte build: 13 seconds
- Svelte tests: 1-2 seconds
- .NET solution build: 15-20 minutes MINIMUM - NEVER CANCEL
- .NET test runs: 5-15 minutes depending on scope

### Frequent Issues and Solutions
**npm install fails with rollup error:**
```bash
rm -rf node_modules package-lock.json
npm install
```

**npm run lint fails with formatting issues:**
```bash
npm run format                          # Fix formatting before linting
```

**Visual Studio build fails:**
- Restore NuGet packages in Package Manager Console
- Check that .NET Framework 4.8 is installed
- Verify PostgreSQL connection string in config files

**Database connection issues:**
- Ensure PostgreSQL service is running
- Check connection string format and credentials
- Verify database `bexis2test` exists

### Repository Structure Quick Reference
```
/home/runner/work/Core/Core/
├── BExIS++.sln                    # Main solution file
├── Console/
│   ├── BExIS.Web.Shell/           # ASP.NET MVC app
│   └── BExIS.Web.Shell.Svelte/    # SvelteKit frontend
├── Components/                     # Core components
├── Modules/                        # Feature modules
├── Libraries/                      # External libs
└── database update scripts/        # SQL migrations
```

## Critical Warnings
- **NEVER CANCEL** build operations - .NET builds take 15-20+ minutes minimum
- **ALWAYS** set timeouts of 60+ minutes for build commands
- **Database required** - Application cannot run without PostgreSQL and workspace setup
- **Windows required** - .NET Framework projects require Windows development environment
- **Workspace dependency** - Requires separate BEXIS2 Workspace repository setup
- **Frontend can be developed independently** - Svelte project works without full backend setup