# setup stage
FROM node:lts-alpine AS setup-stage
WORKDIR /app
COPY package*.json ./
RUN npm install
COPY . .

# development stage
FROM setup-stage AS development-stage
EXPOSE 3001
EXPOSE 9229
CMD ["npm", "run", "dev"]

# build stage
FROM setup-stage AS build-stage
ARG CI
ARG VITE_API_URL
ARG VITE_SENDING_EMAIL
ENV VITE_API_URL=$VITE_API_URL
ENV VITE_SENDING_EMAIL=$VITE_SENDING_EMAIL
ENV CI=$CI
RUN npm run build

# production stage
FROM nginx:stable-alpine AS production-stage
COPY --from=build-stage /app/dist /usr/share/nginx/html
RUN rm /etc/nginx/conf.d/default.conf
COPY nginx/nginx.conf /etc/nginx/conf.d
EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
