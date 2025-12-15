# 快速開始指南 (Quick Start Guide)

## 5 分鐘快速開始

### 步驟 1: 編譯專案（1 分鐘）

```bash
# 在專案根目錄執行
dotnet build SecureDocumentTransfer.sln
```

### 步驟 2: 執行測試（1 分鐘）

```bash
# 驗證加密/解密功能
cd SecureDocumentTransfer.Tests
dotnet run
```

**預期結果**：
```
✅ 測試通過！解密內容與原始內容完全一致
✅ 測試通過！解密檔案與原始檔案完全一致
```

### 步驟 3: 啟動接收端（1 分鐘）

開啟**第一個終端機**：

```bash
cd SecureDocumentTransfer.Receiver
dotnet run

# 按 Enter 使用預設值
# Port: 8000
# 目錄: ./received
```

### 步驟 4: 啟動發送端（1 分鐘）

開啟**第二個終端機**：

```bash
cd SecureDocumentTransfer.Sender
dotnet run

# 輸入以下資訊：
# IP: 127.0.0.1
# Port: 8000
# 檔案: ../test_data/official_document.txt
```

### 步驟 5: 驗證結果（1 分鐘）

```bash
# 查看接收到的檔案
cd SecureDocumentTransfer.Receiver/received
ls -lh
cat recv_*_official_document.txt
```

---

## 一鍵測試腳本

如果您想自動執行測試，可以使用以下命令：

### Linux/macOS:
```bash
# 方法 1: 使用提供的測試腳本
chmod +x test_transfer.sh
./test_transfer.sh

# 方法 2: 手動分兩個終端機測試（建議）
# 終端機 1
cd SecureDocumentTransfer.Receiver && echo -e "\n./received" | dotnet run

# 終端機 2（等待 3 秒後執行）
cd SecureDocumentTransfer.Sender && echo -e "127.0.0.1\n8000\n../test_data/official_document.txt" | dotnet run
```

### Windows PowerShell:
```powershell
# 終端機 1
cd SecureDocumentTransfer.Receiver
dotnet run

# 終端機 2（在新視窗中）
cd SecureDocumentTransfer.Sender
dotnet run
# 然後手動輸入：127.0.0.1, 8000, ..\test_data\official_document.txt
```

---

## 常見問題快速解答

### Q: 連線失敗怎麼辦？
A: 確認接收端已啟動，且 IP 和 Port 正確

### Q: 找不到 dotnet 命令？
A: 請安裝 .NET 8.0 SDK：https://dotnet.microsoft.com/download

### Q: 可以傳送什麼類型的檔案？
A: 所有類型（.txt, .jpg, .pdf, .docx 等）

### Q: 檔案大小有限制嗎？
A: 建議 < 100MB（理論上無限制）

---

## 專案結構一覽

```
2025-12-15/
├── README.md                    ⭐ 從這裡開始
├── QUICK_START.md              ⭐ 本文件
├── USAGE_GUIDE.md              📖 詳細使用手冊
├── SECURITY.md                 🔒 安全性說明
├── PROJECT_SUMMARY.md          📊 專題總結
├── DEMONSTRATION.md            🎬 系統演示
│
├── SecureDocumentTransfer.Common/     # 加密模組
├── SecureDocumentTransfer.Sender/     # 發送端
├── SecureDocumentTransfer.Receiver/   # 接收端
└── SecureDocumentTransfer.Tests/      # 測試程式
```

---

## 下一步

✅ **已完成快速開始** → 閱讀 [USAGE_GUIDE.md](USAGE_GUIDE.md) 了解更多功能  
✅ **想了解技術細節** → 閱讀 [PROJECT_SUMMARY.md](PROJECT_SUMMARY.md)  
✅ **關心安全性議題** → 閱讀 [SECURITY.md](SECURITY.md)  
✅ **想看完整演示** → 閱讀 [DEMONSTRATION.md](DEMONSTRATION.md)  

---

**提示**: 建議先閱讀 README.md 了解系統架構，再使用本快速開始指南進行測試。

**版本**: 1.0  
**更新日期**: 2025-12-15
