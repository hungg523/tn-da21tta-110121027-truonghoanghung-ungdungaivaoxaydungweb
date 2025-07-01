# Stage 1: Build dependencies
FROM python:3.11-slim as builder

RUN apt-get update && apt-get install -y --no-install-recommends \
    gcc \
    libgomp1 \
    && rm -rf /var/lib/apt/lists/*

WORKDIR /install

COPY requirements.txt .
RUN pip install --upgrade pip && \
    pip install --prefix=/install --no-cache-dir -r requirements.txt

# Stage 2: Runtime + Nginx
FROM python:3.11-slim

# Cài đặt ODBC driver, dependencies và nginx
RUN apt-get update && apt-get install -y --no-install-recommends \
    libgomp1 \
    unixodbc \
    unixodbc-dev \
    freetds-dev \
    freetds-bin \
    tdsodbc \
    nginx \
    && rm -rf /var/lib/apt/lists/*

# Cấu hình FreeTDS
RUN echo "[FreeTDS]" > /etc/odbcinst.ini && \
    echo "Description = FreeTDS Driver" >> /etc/odbcinst.ini && \
    echo "Driver = /usr/lib/x86_64-linux-gnu/odbc/libtdsodbc.so" >> /etc/odbcinst.ini && \
    echo "Setup = /usr/lib/x86_64-linux-gnu/odbc/libtdsS.so" >> /etc/odbcinst.ini && \
    echo "UsageCount = 1" >> /etc/odbcinst.ini

WORKDIR /app

COPY --from=builder /install /usr/local
COPY . .

EXPOSE 8000

CMD ["uvicorn", "api.main:app", "--host", "0.0.0.0", "--port", "8000"]
