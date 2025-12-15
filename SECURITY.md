# 安全性考量與改進建議 (Security Considerations)

## 目前實作的安全性特性

✅ **已實作**：
1. **AES-128 加密** - 使用 Advanced Encryption Standard 對稱式加密
2. **CBC 模式** - Cipher Block Chaining 防止相同明文產生相同密文
3. **PKCS7 填充** - 標準填充方式確保資料對齊
4. **TCP 可靠傳輸** - 確保資料完整性，防止封包遺失或亂序

## 已知的安全性限制

本系統為**教育與展示用途**設計，在實際生產環境部署前，必須解決以下安全性議題：

### 🔴 高風險項目（必須改進）

#### 1. 硬編碼的加密金鑰

**問題**：
- 金鑰與 IV 直接寫在原始碼中 (`AesEncryption.cs` 第 22-31 行)
- 使用可預測的序列模式 (0x01-0x10)
- 所有使用者共用相同金鑰
- 原始碼一旦外洩，所有加密資料即可被解密

**建議改進**：
```csharp
// 方案 1: 使用密碼學安全的隨機數產生器
using (var rng = RandomNumberGenerator.Create())
{
    byte[] key = new byte[16];
    rng.GetBytes(key);
}

// 方案 2: 使用密碼衍生函數 (PBKDF2)
using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 10000))
{
    byte[] key = pbkdf2.GetBytes(16);
}

// 方案 3: 整合密鑰管理服務
// - Azure Key Vault
// - AWS KMS
// - HashiCorp Vault
```

#### 2. 固定的初始向量 (IV)

**問題**：
- CBC 模式使用固定 IV 會讓加密變成確定性的
- 相同檔案每次加密結果相同，容易遭受選擇明文攻擊
- 違反 NIST 加密標準建議

**建議改進**：
```csharp
// 每次加密使用隨機 IV
using (var aes = Aes.Create())
{
    aes.GenerateIV(); // 自動產生隨機 IV
    byte[] iv = aes.IV;
    
    // 將 IV 附加在密文前面傳送
    // [IV (16 bytes)] [密文]
}
```

#### 3. 缺乏金鑰交換機制

**問題**：
- 發送端與接收端需預先約定金鑰
- 無法安全地在不安全的通道上交換金鑰
- 不支援動態金鑰更新

**建議改進**：
```csharp
// 方案 1: RSA 非對稱加密交換 AES 金鑰
// 1. 接收端產生 RSA 金鑰對，公鑰給發送端
// 2. 發送端產生隨機 AES 金鑰
// 3. 使用 RSA 公鑰加密 AES 金鑰
// 4. 接收端用 RSA 私鑰解密得到 AES 金鑰

// 方案 2: Diffie-Hellman 金鑰交換
// 兩端獨立產生私鑰，交換公鑰後計算共享金鑰
```

### 🟡 中風險項目（建議改進）

#### 4. 缺乏訊息驗證碼 (MAC)

**問題**：
- 無法驗證密文是否遭到竄改
- 攻擊者可能修改密文而不被發現
- 可能遭受 padding oracle 攻擊

**建議改進**：
```csharp
// 使用 HMAC 驗證訊息完整性
using (var hmac = new HMACSHA256(macKey))
{
    byte[] mac = hmac.ComputeHash(ciphertext);
    // 傳送格式: [密文] [MAC]
}

// 或使用 AES-GCM 模式（已內建認證）
aes.Mode = CipherMode.GCM; // .NET 6+ 支援
```

#### 5. 缺乏身分驗證

**問題**：
- 接收端無法驗證發送者身分
- 可能接收來自未授權來源的檔案
- 容易遭受中間人攻擊

**建議改進**：
```csharp
// 使用 RSA 數位簽章
using (var rsa = RSA.Create())
{
    byte[] signature = rsa.SignData(fileData, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
    // 接收端驗證簽章
    bool isValid = rsa.VerifyData(fileData, signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
}
```

#### 6. TCP 連線未加密

**問題**：
- 雖然資料本身已 AES 加密，但 TCP 連線元資料未保護
- IP 位址、埠號、傳輸時間等資訊可被監聽
- 可能遭受流量分析攻擊

**建議改進**：
```csharp
// 使用 TLS/SSL 加密 TCP 連線
using (var sslStream = new SslStream(networkStream))
{
    await sslStream.AuthenticateAsServerAsync(certificate);
    // 後續通訊透過 sslStream
}
```

#### 7. 大型檔案記憶體處理

**問題**：
- 將整個檔案載入記憶體可能導致 OutOfMemoryException
- 超過 2GB 的檔案會發生 int overflow
- 不適合處理大型檔案（如影片、資料庫備份）

**建議改進**：
```csharp
// 使用串流方式分塊處理
const int bufferSize = 8192;
byte[] buffer = new byte[bufferSize];
int bytesRead;

while ((bytesRead = await inputStream.ReadAsync(buffer, 0, bufferSize)) > 0)
{
    await cryptoStream.WriteAsync(buffer, 0, bytesRead);
}
```

### 🟢 低風險項目（未來擴充）

#### 8. 缺乏存取控制

**建議**：
- 實作使用者帳號密碼系統
- 基於角色的存取控制 (RBAC)
- 稽核日誌記錄所有傳輸活動

#### 9. 缺乏金鑰輪替機制

**建議**：
- 定期更換加密金鑰
- 實作金鑰版本管理
- 支援舊金鑰解密歷史資料

#### 10. 未實作速率限制

**建議**：
- 防止 DoS 攻擊
- 限制連線頻率
- 限制檔案大小

## 安全部署建議

如果要在實際環境中部署此系統，請遵循以下步驟：

### 第一階段：基礎安全強化

1. **更換加密金鑰**：
   ```bash
   # 使用 openssl 產生隨機金鑰
   openssl rand -hex 16
   ```

2. **啟用 TLS/SSL**：
   ```bash
   # 產生自簽憑證（測試用）
   openssl req -x509 -newkey rsa:4096 -keyout key.pem -out cert.pem -days 365
   ```

3. **設定防火牆規則**：
   ```bash
   # 只允許特定 IP 存取
   sudo ufw allow from 192.168.1.0/24 to any port 8000
   ```

### 第二階段：實作認證機制

1. 整合 RSA 數位簽章驗證發送者
2. 實作金鑰交換協定
3. 加入使用者認證系統

### 第三階段：稽核與監控

1. 記錄所有傳輸日誌
2. 異常行為偵測
3. 定期安全掃描

## 相關安全標準

- **NIST SP 800-38A** - AES 加密模式建議
- **FIPS 140-2** - 密碼模組安全需求
- **ISO/IEC 27001** - 資訊安全管理系統
- **OWASP Top 10** - Web 應用程式安全風險

## 結論

本系統成功展示了 AES 加密與 TCP 傳輸的基本原理，適合教育與學習用途。但在實際部署前，**必須**解決上述標註為高風險的安全性問題，並建議實作中風險項目的改進方案，以確保系統的安全性達到生產環境標準。

---

**重要提醒**: 請勿在未強化安全性的情況下，將此系統用於傳輸真正的機密資料！

**更新日期**: 2025-12-15  
**版本**: 1.0
