FROM mcr.microsoft.com/dotnet/sdk:8.0
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY "EvilGiraf.csproj" .
RUN dotnet restore "EvilGiraf.csproj"
RUN dotnet tool install --global dotnet-ef
COPY . .
RUN dotnet build "EvilGiraf.csproj" -c $BUILD_CONFIGURATION -o /app/build
RUN dotnet publish "EvilGiraf.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false
