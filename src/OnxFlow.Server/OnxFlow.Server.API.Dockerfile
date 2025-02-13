# setup stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS setup-stage
WORKDIR /app

# solution level
COPY *.sln ./
COPY Directory.Build.props ./

# src level
COPY src/OnxFlow.Server.API/*.csproj src/OnxFlow.Server.API/
COPY src/Directory.Build.props src/
COPY src/Directory.Packages.props src/

# test level
COPY tests/OnxFlow.Server.API.Tests/*.csproj tests/OnxFlow.Server.API.Tests/
COPY tests/Directory.Build.props tests/
COPY tests/Directory.Packages.props tests/

RUN dotnet restore
COPY . .

# development stage
FROM setup-stage AS development-stage
ENV ASPNETCORE_ENVIRONMENT=Development
EXPOSE 3002
RUN apt-get update
RUN apt-get install -y unzip
RUN curl -sSL https://aka.ms/getvsdbgsh | /bin/sh /dev/stdin -v latest -l ~/vsdbg
CMD ["dotnet", "watch", "--project", "src/OnxFlow.Server.API/OnxFlow.Server.API.csproj"]

# build stage
FROM setup-stage AS build-stage
RUN dotnet publish -c Release -o dist

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS production-stage
WORKDIR /app
COPY --from=build-stage /app/dist ./
EXPOSE 8080
ENTRYPOINT ["dotnet", "OnxFlow.Server.API.dll"]

