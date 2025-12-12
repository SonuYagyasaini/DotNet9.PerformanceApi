#!/usr/bin/env bash
dotnet tool install --global dotnet-ef --version 9.0.0
dotnet ef migrations add InitialCreate --project src/Infrastructure/Infrastructure.csproj --startup-project src/Api/Api.csproj -o Migrations
dotnet ef database update --project src/Infrastructure/Infrastructure.csproj --startup-project src/Api/Api.csproj
