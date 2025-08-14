# Primera etapa: Construir la aplicación
# Usamos una imagen con el SDK de .NET para compilar el proyecto.
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

# Copia todo el contenido del repositorio al directorio de trabajo en el contenedor.
# Esto asegura que todos los archivos del proyecto estén disponibles.
COPY . .

# Restaura las dependencias del proyecto.
# La ruta es relativa al WORKDIR (/app) y a la carpeta del proyecto.
RUN dotnet restore "Message.API/Message.API.csproj"

# Publica la aplicación en modo "Release".
RUN dotnet publish "Message.API/Message.API.csproj" -c Release -o /app/out

# Segunda etapa: Crear la imagen final de la aplicación
# Usamos una imagen más ligera que solo contiene el runtime de .NET.
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# Copia los archivos publicados desde la etapa 'build' a esta imagen final.
COPY --from=build /app/out .

# Expone el puerto 8080 y configura la URL de la aplicación.
# Railway usa el puerto 8080 por defecto.
ENV ASPNETCORE_URLS=http://0.0.0.0:8080
EXPOSE 8080

# Comando para iniciar la aplicación.
ENTRYPOINT ["dotnet", "Message.API.dll"]
