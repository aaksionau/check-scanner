# Build context is the repo root (this project references sibling projects
# under src/) -- invoke as `docker build -f src/CheckScanner.Web/Dockerfile .` from
# the repo root.

# --- assets: Tailwind CSS (standalone CLI, no Node.js) + vendored Alpine.js ---
FROM debian:bookworm-slim AS assets
WORKDIR /assets
RUN apt-get update && apt-get install -y --no-install-recommends curl ca-certificates \
    && rm -rf /var/lib/apt/lists/*

# Pinned versions -- bump deliberately, not via a floating "latest" tag.
ARG TAILWIND_VERSION=v4.3.3
ARG ALPINE_JS_VERSION=3.16.3

RUN curl -sSLo tailwindcss "https://github.com/tailwindlabs/tailwindcss/releases/download/${TAILWIND_VERSION}/tailwindcss-linux-x64" \
    && chmod +x tailwindcss
RUN curl -sSLo alpine.min.js "https://cdn.jsdelivr.net/npm/alpinejs@${ALPINE_JS_VERSION}/dist/cdn.min.js"

COPY src/CheckScanner.Web/Styles/ Styles/
COPY src/CheckScanner.Web/Components/ Components/
RUN ./tailwindcss -i Styles/tailwind.css -o app.css --minify

# --- build: dotnet publish (picks up the generated assets into wwwroot before publish) ---
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY check-scanner.slnx .
COPY src/CheckScanner.Domain/CheckScanner.Domain.csproj src/CheckScanner.Domain/
COPY src/CheckScanner.Application/CheckScanner.Application.csproj src/CheckScanner.Application/
COPY src/CheckScanner.Infrastructure/CheckScanner.Infrastructure.csproj src/CheckScanner.Infrastructure/
COPY src/CheckScanner.Web/CheckScanner.Web.csproj src/CheckScanner.Web/
RUN dotnet restore src/CheckScanner.Web/CheckScanner.Web.csproj

COPY src/ src/
COPY --from=assets /assets/app.css src/CheckScanner.Web/wwwroot/css/app.css
COPY --from=assets /assets/alpine.min.js src/CheckScanner.Web/wwwroot/js/alpine.min.js
RUN dotnet publish src/CheckScanner.Web/CheckScanner.Web.csproj -c Release -o /app/publish

# --- final: aspnet runtime only ---
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "CheckScanner.Web.dll"]
