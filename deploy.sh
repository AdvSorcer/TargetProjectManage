#!/bin/bash

# 專案部署腳本
echo "開始部署 TargetProjectManage 專案..."

# 建立資料目錄
echo "建立資料目錄..."
mkdir -p ./data

# 停止現有容器
echo "停止現有容器..."
docker-compose down

# 建置並啟動容器
echo "建置並啟動容器..."
docker-compose up --build -d

# 檢查容器狀態
echo "檢查容器狀態..."
docker-compose ps

echo "部署完成！"
echo "應用程式已啟動在 http://localhost:8080"
