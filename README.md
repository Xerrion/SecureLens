# SecureLens

**SecureLens** is a .NET-based solution that gathers, correlates, and analyzes data from **Active Directory** and **AdminByRequest** to provide comprehensive reports on user elevations, AD group usage, and application statistics. It offers both **online** and **cache** modes to accommodate different connectivity scenarios and data sources.

---

## Table of Contents
- [Features](#features)
- [Key Components](#key-components)
- [Getting Started](#getting-started)
  - [Prerequisites](#prerequisites)
  - [Installation](#installation)
  - [Configuration](#configuration)
- [Usage](#usage)
  - [Cache Mode](#cache-mode)
  - [Online Mode](#online-mode)
- [Generated HTML Report](#generated-html-report)
- [Contributing](#contributing)
- [License](#license)

---

## Features

1. **Active Directory Integration**  
   - Fetches live AD group memberships, or uses a cached dataset for offline scenarios.

2. **AdminByRequest Data Analysis**  
   - Fetches logs and inventory data (online) or parses JSON files (cache mode) to correlate user elevations, terminal usage, and application runs.

3. **Reporting & Visualization**  
   - Generates an HTML report summarizing:
     - Overall statistics of unique users, devices, and department usage.
     - Detailed application statistics (counts, vendors, preapproved flags).
     - Terminal usage and elevated sessions.
     - Unused AD groups per AdminByRequest setting.
   
4. **Customizable**  
   - Modular design to easily swap strategies (e.g., live queries vs. JSON-based) for both Active Directory and AdminByRequest data.

5. **Secure Key Handling**  
   - API keys are masked during input and cleared from memory after use in online mode.

---

## Key Components

| Component  | Description                                                                                                                             |
|------------|-----------------------------------------------------------------------------------------------------------------------------------------|
| **Application** | Contains analysis logic (calculators, results, analyzer class), services to interact with AD or AdminByRequest, and high-level application flow. |
| **Core**         | Provides shared interfaces and models (e.g., `ActiveDirectoryUser`, `AuditLogEntry`, `InventoryLogEntry`).                        |
| **Infrastructure** | Data repositories, strategies for AD/ABR (e.g., cache or online), logging utilities, and factory classes to instantiate these strategies. |
| **UI**           | Console-based user interface logic (prompts for mode selection, API key input, etc.).                                             |
| **Utilities**    | Helper classes for data sanitization, HTML report generation, JSON file loading, etc.                                             |
| `Program.cs`     | Entry point of the console application.                                                                                           |

---

## Getting Started

### Prerequisites

- [.NET 6 or later](https://dotnet.microsoft.com/download)  
- An Active Directory environment or cached data (if using **cache mode**).  
- Optional: [PowerShell 5.1 or later](https://learn.microsoft.com/powershell/scripting/install/installing-powershell) if you plan to query AD live on Windows.

### Installation

1. **Clone the repository**:
   ```bash
   git clone https://github.com/YourOrg/SecureLens.git
   cd SecureLens

2. **Open the solution (SecureLens.sln) in Visual Studio, VS Code, or any preferred .NET IDE.**

3. **Restore and build**:
```bash
dotnet restore
dotnet build
```

### Configuration
    config/adminbyrequestsettings.json

    Defines the AdminByRequest settings and the AD groups associated with each setting:
```markdown
{
  "AdminByRequestSettings": [
    {
      "Name": "Technology",
      "ActiveDirectoryGroups": [ "Technology" ]
    },
    {
      "Name": "Elevate Terminal Rights",
      "ActiveDirectoryGroups": [
        "Technology",
        "Servicedesk",
        "Tooling",
        "Cloud Developer",
        "Infrastructure",
        "Production Support",
        "Cloud Admin",
        "Content Technology",
        "Data Science",
        "Developers",
        "Access Management",
        "Ad & Sales",
        "Business Services",
        "Content Metadata",
        "Management",
        "Entertainment",
        "Economics",
        "Finance"
      ]
    },
    {
      "Name": "Global",
      "ActiveDirectoryGroups": [
        "Journalism",
        "Sport",
        "Graphical Designer",
        "Advertisement",
        "HR",
        "Legal",
        "Marketing"
      ]
    }
  ]
}
```

config/appsettings.json
Defines file paths for caching, known terminal applications, and the HTML report output path:

    {
      "CachePaths": {
        "GroupCache": "../../../../MockData/cached_adgroup_queries.json",
        "UserCache": "../../../../MockData/cached_admember_queries.json",
        "Inventory": "../../../../MockData/cached_inventory.json",
        "AuditLogs": "../../../../MockData/cached_auditlogs.json"
      },
      "ReportPath": "C:\\Users\\jeppe\\Desktop\\report.html",
      "KnownTerminalApps": [
        "cmd.exe",
        "PowerShell",
        "Windows Command Processor",
        "windows powershell ise",
        "windows subsystem for linux",
        "git for windows"
      ]
    }

Adjust these paths and values to match your local environment.

Usage

After building the project, run the SecureLens console application from the terminal:

cd SecureLens
dotnet run --project .\SecureLens.csproj

You will be prompted to select a mode:

    Cache Mode
        Loads local JSON files containing AD group/user data, inventory, and audit logs.
        Useful for testing, offline usage, or analyzing a static snapshot of data.

    Online Mode
        Makes API calls to fetch inventory and audit data from AdminByRequest.
        Queries AD in real-time (if configured for live AD lookups).
        Requires an API key to authenticate with AdminByRequest.

Cache Mode

When you choose cache mode, CacheModeHandler will:

    Load your local JSON files specified by CachePaths in appsettings.json.
    Build a list of CompletedUser objects correlating Audit and Inventory data.
    Optionally load AD user and group caches from JSON.
    Run the analyzers and generate the HTML report at ReportPath.

Online Mode

When you choose online mode, OnlineModeHandler will:

    Prompt you for an AdminByRequest API Key (masked during entry).
    Make live API requests to fetch Inventory (FetchInventoryDataAsync()) and Audit logs (FetchAuditLogsAsync()).
    Query Active Directory in real time (if running on Windows with AD modules).
    Run the analyzers and generate the HTML report at ReportPath.

Generated HTML Report

Regardless of the mode, SecureLens produces an HTML report providing:

    Overall Statistics
        Total unique users, total unique workstations, membership counts per AdminByRequest setting, etc.
    Application Statistics
        Summaries of run-as-admin usage per application, including vendor, preapproved flags, and counts by setting.
    Terminal Usage
        Detailed table of command-line apps used, the user’s department/title, and which AdminByRequest settings they are under.
    Unused AD Groups
        Lists which AD groups (defined in your settings) have zero recorded elevations.

You can find this report at the path specified in appsettings.json (e.g., ReportPath).

Contributing

    Fork the repository.
    Create your feature branch (git checkout -b feature/my-new-feature).
    Commit your changes (git commit -am 'Add some feature').
    Push the branch (git push origin feature/my-new-feature).
    Submit a Pull Request.

Suggestions, bug reports, and general feedback are always welcome via Issues.
