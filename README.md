Library Management System API (LMS APIs)A RESTful Web API built with .NET / ASP.NET Core for managing a Library System. This application provides backend services and database models for managing authors, books, book categories, copies, publishers, branches, borrowers, users, and book loans.🚀 FeaturesAuthor Management: Endpoints to handle author details and operations.Book & Catalog Management: Endpoints for books, categories, publishers, and specific physical copies.User & Borrower Management: Interfaces for user and library borrower interactions.Loan System: Models and business logic for tracking book loans and due dates across library branches.Repository Pattern: Clean, maintainable architecture separating database context and business logic via service interfaces.Entity Framework Core Integration: Utilizes EF Core with SQL Server for database interactions.🛠️ Tech Stack & PrerequisitesTech StackFramework: .NET / ASP.NET Core Web APIORM: Entity Framework CoreDatabase: SQL ServerLanguage: C#Prerequisites.NET SDK 8.0+ (or matching .NET version)Visual Studio 2022 or VS CodeSQL Server (LocalDB or full instance)📂 Repository StructureLibraryManagementApi/
├── LMS APIs/
│   ├── Controllers/          # API Controllers (Author, Book, Users, etc.)
│   ├── Data/                 # DbContext (ApplicationDBContext.cs)
│   ├── Models/               # Entity Models (Author, Book, Loans, etc.)
│   ├── Repository/           # Service Interfaces & Implementations
│   ├── Program.cs            # Application startup and dependency injection
│   ├── appsettings.json      # Configuration settings & Connection strings
│   └── LMS APIs.csproj       # .NET Project file
└── LMS APIs.sln              # Visual Studio Solution
⚙️ Getting Started1. Clone the Repositorygit clone https://github.com/YOUR-USERNAME/LibraryManagementApi.git
cd LibraryManagementApi
2. Configure Database ConnectionOpen LMS APIs/appsettings.json and update the database connection string to point to your local SQL Server instance:{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER_NAME;Database=LibraryDb;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
3. Apply Database MigrationsRun the following command in the terminal to apply database migrations and initialize your database schema:dotnet ef database update --project "LMS APIs"
4. Run the ProjectStart the API locally using the .NET CLI:dotnet run --project "LMS APIs"
Alternatively, open LMS APIs.sln in Visual Studio and press F5.📑 API Endpoints SummaryFeature AreaControllerKey FunctionalitiesAuthorsAuthorControllerCreate, Read, Update, and Delete authorsBooksBookControllerManage books, categories, and inventoryUsersUsersControllerManage library users and authentication details📄 LicenseThis project is available under the MIT License.
