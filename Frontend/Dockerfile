# Build stage
FROM node:lts-alpine AS build

WORKDIR /app

COPY package*.json ./
RUN npm ci

# Set the API URL for Vite build
# For local development, override with --build-arg VITE_API_URL=http://localhost:5000
ARG VITE_API_URL=https://wellness-kit-api-fd220228cda9.herokuapp.com
ENV VITE_API_URL=$VITE_API_URL

COPY . .
RUN npm run build

# Runtime stage
FROM nginx:stable-alpine

COPY --from=build /app/dist /usr/share/nginx/html
COPY nginx.conf.template /etc/nginx/templates/default.conf.template

# Heroku assigns dynamic PORT - nginx will substitute $PORT from environment
CMD ["sh", "-c", "envsubst '${PORT}' < /etc/nginx/templates/default.conf.template > /etc/nginx/conf.d/default.conf && nginx -g 'daemon off;'"]
