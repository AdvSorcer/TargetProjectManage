# TargetProjectManage 快速部署指南

## 系統需求

- Docker
- Docker Compose

## 一鍵部署

```bash
docker-compose up --build -d
```

就這麼簡單！應用程式會在 `http://localhost:8080` 運行。

## 基本維護命令

```bash
# 停止服務
docker-compose down

# 查看日誌
docker-compose logs -f

# 重新部署
docker-compose down && docker-compose up --build -d

# 備份資料庫
cp ./data/ProjectManage.db ./data/backup_$(date +%Y%m%d).db
```

## 注意事項

- SQLite 資料庫會自動保存在 `./data/` 目錄
- 預設使用 8080 埠，如需修改請編輯 `docker-compose.yml`
- 如果埠被占用，請先停止占用該埠的服務
