# 電子公文傳輸系統使用手冊

## 目錄
1. [快速開始](#快速開始)
2. [系統測試](#系統測試)
3. [實際使用案例](#實際使用案例)
4. [常見問題](#常見問題)
5. [技術細節](#技術細節)

---

## 快速開始

### 環境需求
- .NET 8.0 SDK 或更高版本
- 可執行 `dotnet` 命令的環境

### 安裝與編譯

```bash
# 克隆專案
git clone <repository-url>
cd 2025-12-15

# 編譯整個解決方案
dotnet build SecureDocumentTransfer.sln

# 編譯成功後，可執行檔位於：
# - Sender: SecureDocumentTransfer.Sender/bin/Debug/net8.0/
# - Receiver: SecureDocumentTransfer.Receiver/bin/Debug/net8.0/
```

---

## 系統測試

### 1. 執行單元測試

首先驗證 AES 加密/解密功能是否正常：

```bash
cd SecureDocumentTransfer.Tests
dotnet run
```

**預期輸出**：
```
✅ 測試通過！解密內容與原始內容完全一致
✅ 測試通過！解密檔案與原始檔案完全一致
```

### 2. 執行端對端傳輸測試

#### 步驟 1: 準備測試檔案

專案已包含測試檔案 `test_data/official_document.txt`，您也可以建立自己的測試檔案：

```bash
# 建立測試圖片或文件
echo "測試公文內容" > my_test_file.txt
```

#### 步驟 2: 開啟兩個終端機

**終端機 1 (接收端)**：
```bash
cd SecureDocumentTransfer.Receiver
dotnet run

# 系統提示時，直接按 Enter 使用預設值
# 監聽埠號: 8000 (預設)
# 儲存目錄: ./received (預設)
```

**終端機 2 (發送端)**：
```bash
cd SecureDocumentTransfer.Sender
dotnet run

# 輸入接收端資訊：
# IP 位址: 127.0.0.1 (本機測試)
# 埠號: 8000
# 檔案路徑: ../test_data/official_document.txt
```

#### 步驟 3: 驗證結果

檢查接收到的檔案：

```bash
cd SecureDocumentTransfer.Receiver/received
ls -lh
# 應該看到 recv_YYYYMMDD_HHMMSS_official_document.txt

# 比對內容
cat recv_*_official_document.txt
```

---

## 實際使用案例

### 案例 1: 跨電腦傳輸機密文件

**場景**：將機密文件從電腦 A 傳送到電腦 B

**步驟**：

1. **在電腦 B (接收端)**：
   - 啟動接收端程式
   - 記下電腦 B 的 IP 位址（例如：192.168.1.100）
   - 使用預設埠號 8000

2. **在電腦 A (發送端)**：
   - 啟動發送端程式
   - 輸入電腦 B 的 IP：192.168.1.100
   - 輸入埠號：8000
   - 選擇要傳送的檔案

3. **傳輸過程**：
   - 檔案在電腦 A 自動 AES 加密
   - 透過 TCP/IP 網路傳輸加密資料
   - 電腦 B 接收並自動解密
   - 檔案儲存到指定目錄

### 案例 2: 傳送圖片檔案

```bash
# 發送端
cd SecureDocumentTransfer.Sender
dotnet run

# 輸入資訊：
IP: 127.0.0.1
Port: 8000
檔案: /path/to/secret_diagram.jpg

# 接收端會自動儲存為：
# received/recv_20251215_123456_secret_diagram.jpg
```

### 案例 3: 傳送大型檔案

系統支援任意大小的檔案，AES 加密會自動處理：

```bash
# 傳送 100MB 的檔案
dotnet run
# 檔案路徑: /path/to/large_document.pdf
```

---

## 常見問題

### Q1: 連線失敗怎麼辦？

**A**: 檢查以下項目：
1. 確認接收端已啟動並在監聽
2. 確認 IP 位址和埠號正確
3. 檢查防火牆是否阻擋埠號 8000
4. 確認兩台電腦在同一網路或可互相連線

```bash
# 測試網路連通性
ping <接收端IP>

# 檢查埠號是否被佔用 (Linux/Mac)
netstat -an | grep 8000

# 檢查埠號是否被佔用 (Windows)
netstat -an | findstr 8000
```

### Q2: 如何更改預設埠號？

**A**: 啟動程式時，在提示時輸入您想要的埠號：

```bash
# 接收端
請輸入監聽埠號 (預設: 8000): 9000

# 發送端
請輸入接收端埠號 (預設: 8000): 9000
```

### Q3: 可以同時傳送多個檔案嗎？

**A**: 目前版本一次傳送一個檔案。若要傳送多個檔案：
1. 可以多次執行發送端程式
2. 或將檔案壓縮成 ZIP 後傳送

### Q4: 如何確保檔案完整性？

**A**: 可以比對檔案的 MD5 或 SHA256 雜湊值：

```bash
# 原始檔案
md5sum original_file.txt

# 接收檔案
md5sum received/recv_*_original_file.txt

# 兩者應該完全相同
```

### Q5: 加密金鑰可以修改嗎？

**A**: 可以，但需要修改原始碼：

編輯 `SecureDocumentTransfer.Common/AesEncryption.cs`：

```csharp
// 修改 Key 和 IV 的值（必須是 16 bytes）
private static readonly byte[] Key = new byte[16] 
{ 
    0x01, 0x02, ... // 修改為您的金鑰
};
```

⚠️ **重要**：發送端與接收端必須使用相同的金鑰！

---

## 技術細節

### AES 加密參數

| 參數 | 值 | 說明 |
|------|-----|------|
| 演算法 | AES | Advanced Encryption Standard |
| 金鑰長度 | 128 bits (16 bytes) | 符合 AES-128 標準 |
| 加密模式 | CBC | Cipher Block Chaining |
| 填充方式 | PKCS7 | 標準填充方式 |
| 初始向量 | 128 bits (16 bytes) | 固定 IV |

### 網路傳輸協定

傳輸封包格式：

```
+-------------------+-------------------+-------------------+-------------------+
| 檔案名稱長度      | 檔案大小          | 檔案名稱          | 加密資料          |
| (4 bytes)         | (8 bytes)         | (variable)        | (variable)        |
+-------------------+-------------------+-------------------+-------------------+
```

1. **檔案名稱長度** (4 bytes, Int32): 檔案名稱的位元組數
2. **檔案大小** (8 bytes, Int64): 加密後資料的總位元組數
3. **檔案名稱** (variable, UTF-8): 原始檔案名稱
4. **加密資料** (variable): AES 加密後的檔案內容

### 安全性考量

✅ **已實作**：
- AES-128 對稱式加密
- CBC 模式防止相同明文產生相同密文
- PKCS7 填充確保資料對齊
- TCP 協定保證資料完整性

⚠️ **待加強**：
- 金鑰管理（目前硬編碼）
- 金鑰交換機制（建議使用 RSA）
- 數位簽章驗證發送者身分
- TLS/SSL 保護 TCP 連線
- 訊息驗證碼 (MAC) 防止竄改

### 效能特性

- **加密速度**: AES-128 約 1GB/s (視 CPU 而定)
- **檔案大小限制**: 無限制（受限於可用記憶體）
- **網路傳輸**: 依網路頻寬，TCP 自動調整
- **記憶體使用**: 需將整個檔案載入記憶體

### 專案結構說明

```
SecureDocumentTransfer/
│
├── Common/                          # 共用函式庫
│   ├── AesEncryption.cs            # AES 加密/解密實作
│   └── NetworkProtocol.cs          # 網路協定常數定義
│
├── Sender/                          # 發送端應用程式
│   └── Program.cs                  # 主程式 (Client)
│
├── Receiver/                        # 接收端應用程式
│   └── Program.cs                  # 主程式 (Server)
│
└── Tests/                           # 測試程式
    └── Program.cs                  # 單元測試
```

---

## 延伸閱讀

- [AES 加密標準](https://en.wikipedia.org/wiki/Advanced_Encryption_Standard)
- [TCP/IP 網路協定](https://en.wikipedia.org/wiki/Transmission_Control_Protocol)
- [.NET Cryptography](https://learn.microsoft.com/en-us/dotnet/standard/security/cryptography-model)
- [Socket 程式設計](https://learn.microsoft.com/en-us/dotnet/api/system.net.sockets.socket)

---

**版本**: 1.0  
**更新日期**: 2025-12-15  
**聯絡方式**: 資訊安全應用專題小組
