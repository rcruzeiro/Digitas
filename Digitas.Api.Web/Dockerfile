FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Digitas.Api.Web/Digitas.Api.Web.csproj", "Digitas.Api.Web/"]
COPY ["Digitas.Core/Digitas.Core.csproj", "Digitas.Core/"]
RUN dotnet restore "Digitas.Api.Web/Digitas.Api.Web.csproj"
COPY . .
WORKDIR "/src/Digitas.Api.Web"
RUN dotnet build "Digitas.Api.Web.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Digitas.Api.Web.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Digitas.Api.Web.dll"]
