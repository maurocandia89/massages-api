# Usamos una imagen base con el SDK de .NET para compilar la aplicación.
# Esta es la primera etapa ("build") de un proceso multi-stage.
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copiamos el archivo de proyecto (csproj) para restaurar las dependencias.
# Esto mejora la eficiencia del caché de Docker, ya que si solo cambia el código, no tiene que volver a descargar las dependencias.
COPY ["Message.API/Message.API.csproj", "Message.API/"]
RUN dotnet restore "Message.API/Message.API.csproj"

# Copiamos todos los archivos del proyecto.
COPY . .
WORKDIR "/src/Message.API"

# Compilamos y publicamos la aplicación en modo "Release".
RUN dotnet publish "Message.API.csproj" -c Release -o /app/publish --no-restore

# Usamos una imagen base más ligera (solo con el runtime) para la aplicación final.
# Esto reduce significativamente el tamaño de la imagen final.
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# Copiamos los archivos publicados desde la etapa de "build" a la imagen final.
COPY --from=build /app/publish .

# Exponemos el puerto 8080. Por defecto, Railway usa este puerto.
# Asegúrate de que tu aplicación de ASP.NET Core escuche en esta dirección (http://0.0.0.0:8080).
ENV ASPNETCORE_URLS=http://0.0.0.0:8080
EXPOSE 8080

# Definimos el comando de inicio para la aplicación.
# "dotnet" es el ejecutable y "Message.API.dll" es el archivo que debe ejecutar.
ENTRYPOINT ["dotnet", "Message.API.dll"]
