## Setup

```bash
git clone https://github.com/MarinoM0/f1-fantasy.git
cd f1-fantasy

cd backend/F1Fantasy.Api/F1Fantasy.Api
dotnet restore
dotnet user-secrets set "Jwt:Key" "local-development-secret-key-at-least-32-characters"

cd ..
dotnet build F1Fantasy.Api.slnx

cd ../../frontend/f1-fantasy-ui
npm install
```

## Run Backend

```bash
cd backend/F1Fantasy.Api
dotnet run --project F1Fantasy.Api/F1Fantasy.Api.csproj --launch-profile https
```

Backend Swagger:

```txt
https://localhost:7252/swagger
```

## Run Frontend

Open another terminal:

```bash
cd frontend/f1-fantasy-ui
npm start
```

Frontend:

```txt
http://localhost:4200
```

The backend automatically applies database migrations and seeds initial data on startup.



## Technical Features

- ASP.NET Core Web API backend with Angular frontend
- EF Core code-first database with SQL Server, migrations, and startup seeding
- JWT authentication with password hashing and protected API routes
- Layered backend architecture: controllers, services, DTOs, models, and data access
- Angular routing, reactive forms, typed API services, and JWT HTTP interceptor
- External F1 data integration through the Jolpica API
