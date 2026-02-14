@echo off
echo 開始部署 TargetProjectManage 專案...

REM 建立資料目錄
echo 建立資料目錄...
if not exist "data" mkdir data

REM 停止現有容器
echo 停止現有容器...
docker-compose down

REM 建置並啟動容器
echo 建置並啟動容器...
docker-compose up --build -d

REM 檢查容器狀態
echo 檢查容器狀態...
docker-compose ps

echo 部署完成！
echo 應用程式已啟動在 http://localhost:8080
pause
