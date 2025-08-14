# Etapa de build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /source

# Copiamos los archivos del proyecto
COPY . .

# Restauramos dependencias y compilamos
RUN dotnet restore Message.API.csproj
RUN dotnet publish Message.API.csproj -c Release -o /app/publish

# Etapa final: runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

# Expone el puerto en el que corre tu API
EXPOSE 5000

# Comando para ejecutar la API
ENTRYPOINT ["dotnet", "Message.API.dll"]
