# ── Stage 1: Restore ─────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY MyFirstApi.csproj .
RUN dotnet restore -r linux-musl-x64

COPY . .

# ── Stage 2: Publish — langsung dari restore, skip build terpisah ─
FROM build AS publish
RUN dotnet publish -c Release \
    -r linux-musl-x64 \
    --self-contained true \
    -p:PublishSingleFile=false \
    -o /app/publish

# ── Stage 3: Runtime — Alpine image ──────────────────────────────
FROM alpine:3.19 AS final
WORKDIR /app

RUN apk add --no-cache \
    libstdc++ \
    libgcc \
    icu-libs

COPY --from=publish /app/publish .

RUN addgroup -S appgroup && adduser -S appuser -G appgroup
RUN mkdir -p /data && chown appuser:appgroup /data
USER appuser

ENV DOTNET_CONTENTROOT=/app
ENV ASPNETCORE_URLS=http://0.0.0.0:5000
ENV DATA_PATH=/data

EXPOSE 5000
ENTRYPOINT ["./MyFirstApi"]