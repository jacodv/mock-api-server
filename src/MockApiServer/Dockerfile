#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["MockApiServer/MockApiServer.csproj", "MockApiServer/"]
RUN dotnet restore "MockApiServer/MockApiServer.csproj"
COPY . .
WORKDIR "/src/MockApiServer"
RUN dotnet build "MockApiServer.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MockApiServer.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MockApiServer.dll"]