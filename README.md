# 電子公文傳輸系統 (Secure Document Transfer System)

## 專題摘要 (Abstract)

本專題開發了一套具備資訊安全機制的電子公文傳輸系統。系統整合了 **AES (Advanced Encryption Standard)** 進階加密標準與 **TCP/IP** 網路協定，確保公文檔案在網路傳輸過程中即使遭竊取也無法被讀取，唯有持有對應金鑰的接收端才能解密還原，成功達成了電子公文「無紙化」與「高機密性」的雙重目標。

## 系統特色

✅ **AES 128 位元加密** - 採用國際公認的 AES 對稱式加密演算法  
✅ **TCP Socket 可靠傳輸** - 確保檔案傳輸的完整性與可靠性  
✅ **自動加解密** - 發送端自動加密，接收端自動解密  
✅ **檔案完整性保證** - 解密後的檔案與原始檔案完全一致  
✅ **跨網路傳輸** - 支援區域網路與廣域網路傳輸  

## 系統架構

```
┌─────────────┐                      ┌─────────────┐
│  發送端     │                      │  接收端     │
│  (Sender)   │                      │ (Receiver)  │
├─────────────┤                      ├─────────────┤
│ 1. 選取檔案 │                      │ 1. 監聽埠號 │
│ 2. AES加密  │──── TCP/IP 網路 ────▶│ 2. 接收資料 │
│ 3. TCP傳送  │    (加密資料流)      │ 3. AES解密  │
│             │                      │ 4. 儲存檔案 │
└─────────────┘                      └─────────────┘
```

## 核心技術

### 1. AES 對稱式加密 (AES Encryption)
- **金鑰長度**: 128 位元 (16 Bytes)
- **加密模式**: CBC (Cipher Block Chaining)
- **填充方式**: PKCS7
- **安全性**: 發送端與接收端必須使用相同的金鑰與初始向量 (IV)

### 2. TCP Socket 網路通訊
- **協定**: TCP/IP
- **架構**: 主從式 (Client-Server)
- **可靠性**: TCP 保證資料不遺失、不重複、順序正確
- **預設埠號**: 8000

## 系統需求

- **.NET 8.0** 或更高版本
- **作業系統**: Windows / Linux / macOS
- **網路**: 發送端與接收端需在同一網路或可互相連線

## 專案結構

```
SecureDocumentTransfer/
├── SecureDocumentTransfer.Common/      # 共用函式庫
│   ├── AesEncryption.cs                # AES 加密/解密模組
│   └── NetworkProtocol.cs              # 網路協定常數
├── SecureDocumentTransfer.Sender/      # 發送端應用程式
│   └── Program.cs                      # 發送端主程式
├── SecureDocumentTransfer.Receiver/    # 接收端應用程式
│   └── Program.cs                      # 接收端主程式
└── SecureDocumentTransfer.sln          # Visual Studio 解決方案
```

## 編譯方式

```bash
# 編譯整個解決方案
dotnet build SecureDocumentTransfer.sln

# 或分別編譯各專案
dotnet build SecureDocumentTransfer.Common/SecureDocumentTransfer.Common.csproj
dotnet build SecureDocumentTransfer.Sender/SecureDocumentTransfer.Sender.csproj
dotnet build SecureDocumentTransfer.Receiver/SecureDocumentTransfer.Receiver.csproj
```

## 使用方式

### 步驟 1: 啟動接收端

開啟第一個終端機，執行接收端程式：

```bash
cd SecureDocumentTransfer.Receiver
dotnet run
```

系統會提示：
```
請輸入監聽埠號 (預設: 8000): [直接按 Enter 使用預設值]
請輸入接收檔案儲存目錄 (預設: ./received): [直接按 Enter 使用預設值]

🎧 伺服器已啟動，監聽埠號: 8000
💾 儲存目錄: /path/to/received
⏳ 等待發送端連線...
```

### 步驟 2: 啟動發送端

開啟第二個終端機，執行發送端程式：

```bash
cd SecureDocumentTransfer.Sender
dotnet run
```

系統會提示輸入資訊：
```
請輸入接收端 IP 位址 (預設: 127.0.0.1): [輸入接收端 IP 或按 Enter]
請輸入接收端埠號 (預設: 8000): [按 Enter 使用預設值]
請輸入要傳送的檔案完整路徑: ../test_data/official_document.txt
```

### 步驟 3: 觀察傳輸結果

**發送端輸出**：
```
📄 讀取檔案: official_document.txt
   檔案大小: 256 bytes

🔒 執行 AES 加密...
   加密完成！加密後大小: 272 bytes

🌐 連線至 127.0.0.1:8000...
   ✅ 連線成功！

📤 傳送加密資料中...

✅ 檔案傳送成功！
   原始檔案: official_document.txt
   原始大小: 256 bytes
   加密大小: 272 bytes
```

**接收端輸出**：
```
📞 收到來自 127.0.0.1:xxxxx 的連線
📄 檔案名稱: official_document.txt
   加密大小: 272 bytes
📥 接收加密資料中...
   ✅ 接收完成！
🔓 執行 AES 解密...
   解密完成！原始大小: 256 bytes

✅ 檔案儲存成功！
   原始檔名: official_document.txt
   儲存路徑: /path/to/received/recv_20251215_041307_official_document.txt
   檔案大小: 256 bytes

⏳ 等待下一個連線...
```

### 步驟 4: 驗證檔案完整性

比對原始檔案與接收檔案：

```bash
# 查看原始檔案
cat test_data/official_document.txt

# 查看接收到的檔案
cat SecureDocumentTransfer.Receiver/received/recv_*_official_document.txt
```

兩個檔案內容應完全一致，證明加密傳輸系統運作正常。

## 測試案例

### 測試 1: 文字檔案傳輸
```bash
# 建立測試檔案
echo "機密公文內容" > test.txt

# 傳送測試
# 發送端輸入檔案路徑: test.txt
```

### 測試 2: 圖片檔案傳輸
```bash
# 準備圖片檔案 (例如 w.jpg)
# 發送端輸入檔案路徑: w.jpg
# 接收端會儲存為 recv_時間戳記_w.jpg
```

### 測試 3: 大型檔案傳輸
系統支援任意大小的檔案傳輸，AES 加密會自動分塊處理。

## 安全性說明

⚠️ **重要提醒**：

1. **金鑰管理**: 目前系統的 AES 金鑰與 IV 是硬編碼在程式中。在實際部署時，應使用安全的金鑰管理機制。

2. **金鑰交換**: 發送端與接收端必須預先約定相同的金鑰。未來可整合 RSA 非對稱加密進行金鑰交換。

3. **網路安全**: 雖然資料已加密，但 TCP 連線本身未加密。建議在可信任的網路環境中使用，或額外使用 VPN/TLS。

## 未來擴充方向

1. **數位簽章 (Digital Signature)**  
   加入 RSA 非對稱加密技術，讓接收端能驗證發送者的真實身分，防止公文遭到偽造。

2. **資料庫整合**  
   建立公文管理資料庫，記錄每一筆傳輸的送達時間與經手人員，完善公文的稽核軌跡。

3. **圖形化介面**  
   開發 Windows Forms 或 WPF 圖形介面，提供更友善的使用者體驗。

4. **多重接收端**  
   支援一次傳送給多個接收端，實現公文的群組傳送。

5. **傳輸進度顯示**  
   顯示大型檔案的傳輸進度百分比。

## 結論

本專題成功實作了一套具備 AES 高強度加密的電子公文傳輸系統。透過實際程式開發，驗證了以軟體加密取代實體封存的可行性。系統不僅解決了公文傳遞的時效性問題，更透過密碼學技術解決了網路傳輸最擔心的安全疑慮。

## 授權

本專題為教育用途開發，供學習與研究使用。

---

**開發團隊**: 資訊安全應用專題小組  
**開發時間**: 2025-12-15  
**技術棧**: C# / .NET 8.0 / AES Encryption / TCP Socket
