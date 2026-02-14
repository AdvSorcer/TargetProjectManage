# PDF 匯出功能說明

## 概述

本專案使用 **PuppeteerSharp** 來實作跨平台的 PDF 匯出功能，支援 Windows 和 Linux 環境。

## 技術架構

### 主要組件
- **PuppeteerSharp**: 跨平台 PDF 生成庫
- **Chrome/Chromium**: 用於 HTML 轉 PDF 的渲染引擎
- **IPdfService**: PDF 服務介面
- **PdfService**: PDF 服務實作

### 支援的頁面
1. 專案時程管理 (`/Schedules`)
2. 成本分析 (`/Analysis/CostAnalysis`)
3. 效益分析 (`/Analysis/BenefitAnalysis`)
4. 廠商提案管理 (`/Proposals`)

## 環境需求

### Windows 環境
- .NET 10.0 或更高版本
- 無需額外安裝，PuppeteerSharp 會自動下載 Chromium

### Linux 環境
- .NET 10.0 或更高版本
- 需要安裝 Chrome/Chromium 依賴套件（已在 Dockerfile 中配置）

### Docker 環境
- 使用提供的 Dockerfile 會自動安裝所需依賴
- 支援無頭模式運行

## 使用方式

### 匯出格式
每個支援的頁面都提供以下匯出選項：
- **CSV 格式**: 純文字資料匯出
- **Excel 格式**: 使用 ClosedXML 生成 .xlsx 檔案
- **PDF 格式**: 使用 PuppeteerSharp 生成 .pdf 檔案

### PDF 功能特色
- 支援中文字體（Microsoft JhengHei）
- 自動分頁和頁碼
- 專業的表格樣式
- 響應式設計
- 包含頁首和頁尾資訊

## 部署注意事項

### 生產環境
1. 確保伺服器有足夠的記憶體（建議至少 2GB）
2. 首次運行時會下載 Chromium，需要網路連線
3. 建議設定適當的逾時時間

### 效能優化
- PDF 服務使用單例模式，避免重複初始化
- 使用 SemaphoreSlim 確保執行緒安全
- 自動管理瀏覽器生命週期

## 故障排除

### 常見問題
1. **PDF 生成失敗**: 檢查記憶體使用量和網路連線
2. **中文字體顯示問題**: 確保系統支援 Microsoft JhengHei 字體
3. **Linux 環境錯誤**: 確認已安裝所有必要的系統依賴

### 日誌記錄
PDF 生成過程中的錯誤會記錄在應用程式日誌中，便於除錯。

## 技術優勢

### 跨平台兼容性
- 統一的 API 介面
- 自動處理平台差異
- 容器化部署支援

### 渲染品質
- 使用 Chrome 引擎，渲染效果優秀
- 支援現代 CSS 特性
- 高品質的 PDF 輸出

### 維護性
- 清晰的服務分層
- 易於擴展和修改
- 完整的錯誤處理機制
