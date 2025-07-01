# =============================
# 1. Build stage
# =============================
FROM node:20-alpine AS builder

WORKDIR /app

# Cài dependencies
COPY package.json pnpm-lock.yaml ./
RUN corepack enable && corepack prepare pnpm@latest --activate && pnpm install

# Copy toàn bộ source
COPY . .

# Build Next.js (tự động tạo thư mục out/ nếu cấu hình 'output: export')
RUN pnpm build

# =============================
# 2. Nginx: serve static site
# =============================
FROM nginx:alpine AS runner

# Copy thư mục out/ từ build stage
COPY --from=builder /app/out /usr/share/nginx/html

# Cấu hình Nginx custom
COPY nginx.conf /etc/nginx/conf.d/default.conf

EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
