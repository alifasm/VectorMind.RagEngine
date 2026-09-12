FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files and restore dependencies
COPY ["VectorMind.RagEngine.Domain/VectorMind.RagEngine.Domain.csproj", "VectorMind.RagEngine.Domain/"]
COPY ["VectorMind.RagEngine.Infrastructure/VectorMind.RagEngine.Infrastructure.csproj", "VectorMind.RagEngine.Infrastructure/"]
COPY ["VectorMind.RagEngine.Api/VectorMind.RagEngine.Api.csproj", "VectorMind.RagEngine.Api/"]
RUN dotnet restore "VectorMind.RagEngine.Api/VectorMind.RagEngine.Api.csproj"

# Copy full source and build
COPY . .
WORKDIR "/src/VectorMind.RagEngine.Api"
RUN dotnet publish "VectorMind.RagEngine.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "VectorMind.RagEngine.Api.dll"]
