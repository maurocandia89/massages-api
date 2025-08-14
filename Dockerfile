# ==========================================================
# Etapa de compilación (build)
# Usa la imagen del SDK para construir y publicar la aplicación.
# ==========================================================
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copiamos el archivo del proyecto .csproj al directorio de trabajo en el contenedor.
# Usamos la ruta correcta según la estructura de tu proyecto.
COPY ["massages-api/Message.API/Message.API.csproj", "massages-api/Message.API/"]

# Restauramos las dependencias.
RUN dotnet restore "massages-api/Message.API/Message.API.csproj"

# Copiamos todo el resto del código fuente del repositorio.
COPY . .

# Nos movemos al directorio del proyecto para la publicación.
WORKDIR "/src/massages-api/Message.API"

# Publicamos la aplicación en el directorio /app/out.
RUN dotnet publish -c Release -o /app/out --no-restore

# ==========================================================
# Etapa final (runtime)
# Usa la imagen de ASP.NET, que es más ligera, para ejecutar la aplicación.
# ==========================================================
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# Copiamos los archivos publicados desde la etapa de compilación.
COPY --from=build /app/out .

# Configuramos la aplicación para que escuche en el puerto 8080 (el puerto por defecto de Railway).
ENV ASPNETCORE_URLS=http://0.0.0.0:8080
EXPOSE 8080

# Comando para iniciar la aplicación.
ENTRYPOINT ["dotnet", "Message.API.dll"]
