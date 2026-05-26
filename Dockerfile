FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["work5_ASP.NET_Core_API/work5_ASP.NET_Core_API.csproj", "work5_ASP.NET_Core_API/"]
RUN dotnet restore "work5_ASP.NET_Core_API/work5_ASP.NET_Core_API.csproj"
COPY . .
WORKDIR "/src/work5_ASP.NET_Core_API"
RUN dotnet publish "work5_ASP.NET_Core_API.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
EXPOSE 8000
ENV ASPNETCORE_URLS=http://+:8000
ENV APP_ENV=docker
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "work5_ASP.NET_Core_API.dll"]