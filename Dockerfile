# Usa una imagen base de Node.js para el proceso de construcción.
# alpine es una versión ligera de Linux, ideal para imágenes pequeñas.
FROM node:18-alpine AS builder

# Establece el directorio de trabajo dentro del contenedor.
WORKDIR /app

# Copia los archivos de manifiesto de dependencias.
# Esto nos permite instalar las dependencias antes de copiar el código completo.
COPY package*.json ./

# Instala las dependencias de producción.
# El flag "--only=production" asegura que solo se instalen las dependencias necesarias
# para que la aplicación funcione, reduciendo el tamaño final de la imagen.
RUN npm install --only=production

# --- SEGUNDA ETAPA (La imagen final) ---

# Usa una imagen base aún más ligera para la aplicación en producción.
# Esto reduce drásticamente el tamaño final de la imagen de Docker.
FROM node:18-alpine

# Establece el directorio de trabajo dentro del contenedor.
WORKDIR /app

# Copia las dependencias instaladas de la etapa "builder".
# Esto evita tener que instalarlas de nuevo en la imagen final.
COPY --from=builder /app/node_modules ./node_modules

# Copia todo el código de tu aplicación.
# Asegúrate de tener un archivo .dockerignore para excluir archivos innecesarios
# como node_modules (que ya se copió) y archivos de desarrollo.
COPY . .

# Expone el puerto en el que la API escuchará.
# Asumiendo que tu API escucha en el puerto 3000. Si es diferente, cámbialo.
EXPOSE 3000

# Define el comando para iniciar tu aplicación.
# Esto es lo que se ejecutará cuando se inicie el contenedor.
# "npm start" es el comando estándar para la mayoría de las aplicaciones de Node.js.
CMD ["npm", "start"]

