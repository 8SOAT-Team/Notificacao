FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["Notificacao.Api/Notificacao.Api.csproj", "Notificacao.Api/"]
RUN dotnet restore "Notificacao.Api/Notificacao.Api.csproj"

COPY . .
WORKDIR "/src/Notificacao.Api"
RUN dotnet publish "Notificacao.Api.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:80
ENV ASPNETCORE_ENVIRONMENT=Development

EXPOSE 80

ENTRYPOINT ["dotnet", "Notificacao.Api.dll"]

