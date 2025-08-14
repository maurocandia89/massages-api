# ==========================================================
# Etapa de compilación (build)
# Usa la imagen del SDK para construir y publicar la aplicación
# ==========================================================
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copiamos solo el archivo del proyecto .csproj.
# Esto optimiza el caché de Docker.
COPY ["Message.API/Message.API.csproj", "Message.API/"]

# Restauramos las dependencias
RUN dotnet restore "Message.API/Message.API.csproj"

# Copiamos todo el resto del código fuente.
# Docker no tendrá que volver a restaurar las dependencias si solo cambia el código.
COPY . .

# Nos movemos al directorio del proyecto para la publicación
WORKDIR "/src/Message.API"

# Publicamos la aplicación en el directorio /app/out
# El --no-restore evita que se intenten restaurar dependencias de nuevo.
RUN dotnet publish -c Release -o /app/out --no-restore

# ==========================================================
# Etapa final (runtime)
# Usa la imagen de ASP.NET, que es más ligera, para ejecutar la aplicación.
# ==========================================================
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# Copiamos los archivos publicados desde la etapa de compilación
COPY --from=build /app/out .

# Configuramos la aplicación para que escuche en el puerto 8080 (el puerto por defecto de Railway).
ENV ASPNETCORE_URLS=http://0.0.0.0:8080
EXPOSE 8080

# Comando para iniciar la aplicación
ENTRYPOINT ["dotnet", "Message.API.dll"]
