# Bước 1: Dùng Node.js để build Angular
FROM node:20 AS build

WORKDIR /app

# Sao chép package.json và cài đặt dependencies
COPY package.json package-lock.json ./
RUN npm install --legacy-peer-deps
RUN npm install -g @angular/cli

# Sao chép toàn bộ source code
COPY . .

# Build ứng dụng Angular
RUN npx ng build --configuration=development

# Bước 2: Dùng Nginx để chạy ứng dụng
FROM nginx:alpine

# Xóa cấu hình mặc định của Nginx
RUN rm /etc/nginx/conf.d/default.conf

# Copy file cấu hình nginx.conf vào container
COPY nginx.conf /etc/nginx/conf.d/default.conf

# Copy thư mục build vào Nginx để phục vụ web
COPY --from=build /app/dist/browser/. /usr/share/nginx/html/

EXPOSE 80 443

CMD ["nginx", "-g", "daemon off;"]