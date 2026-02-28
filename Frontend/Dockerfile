# Build stage
FROM node:lts-alpine AS build

WORKDIR /app

COPY package*.json ./
RUN npm ci

# Declare build arg and expose it as an env var so Vite picks it up
ARG VITE_API_URL
ENV VITE_API_URL=$VITE_API_URL

COPY . .
RUN npm run build

# Runtime stage
FROM nginx:stable-alpine

COPY --from=build /app/dist /usr/share/nginx/html
COPY nginx.conf.template /etc/nginx/templates/default.conf.template

# Heroku assigns dynamic PORT - nginx will substitute $PORT from environment
CMD ["sh", "-c", "envsubst '${PORT}' < /etc/nginx/templates/default.conf.template > /etc/nginx/conf.d/default.conf && nginx -g 'daemon off;'"]
