### Multi-stage Dockerfile: build .NET app then runtime that also includes Python OCR
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy repository into build context
COPY . .

# Switch to the webapp project directory
WORKDIR /src/edupro/edupro

# Restore and publish the edupro project
RUN dotnet restore
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Install Python and small utilities
RUN apt-get update && apt-get install -y --no-install-recommends \
    python3 python3-pip python3-venv curl procps ca-certificates \
    && rm -rf /var/lib/apt/lists/*

# Copy published .NET app
COPY --from=build /app/publish/ ./

# Copy OCR server and its requirements
COPY server_ocr.py ./
COPY requirements.txt ./

# Create a virtualenv and install Python deps there to avoid system package restrictions
RUN python3 -m venv /opt/venv \
    && /opt/venv/bin/python -m pip install --upgrade pip setuptools wheel \
    && /opt/venv/bin/pip install --no-cache-dir -r requirements.txt

# Ensure the virtualenv's bin is on PATH for runtime
ENV PATH="/opt/venv/bin:$PATH"

# Entrypoint that launches both OCR server and .NET app
# entrypoint is located at edupro/edupro/entrypoint.sh in this repo
COPY edupro/edupro/entrypoint.sh /entrypoint.sh
RUN chmod +x /entrypoint.sh

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080 8001

ENTRYPOINT ["/entrypoint.sh"]
