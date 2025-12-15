using System.Security.Cryptography;

namespace SecureDocumentTransfer.Common;

/// <summary>
/// AES 加密/解密模組
/// 使用 128 位元金鑰進行對稱式加密，確保公文傳輸的機密性
/// </summary>
public static class AesEncryption
{
    // ⚠️ 安全警告 (Security Warning) ⚠️
    // 此金鑰與初始向量僅供教育與展示用途
    // 實際部署時必須：
    // 1. 使用密碼學安全的隨機數產生器生成金鑰
    // 2. 實作安全的金鑰管理與交換機制（建議使用 RSA 或 Diffie-Hellman）
    // 3. 每次傳輸使用不同的隨機 IV
    // 4. 將金鑰儲存在安全的密鑰保存庫中（如 Azure Key Vault）
    
    // 128 位元 (16 Bytes) 金鑰 - 發送端與接收端必須使用相同金鑰
    private static readonly byte[] Key = new byte[16] 
    { 
        0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08,
        0x09, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F, 0x10
    };

    // 128 位元 (16 Bytes) 初始向量 - 用於 CBC 模式
    private static readonly byte[] IV = new byte[16]
    {
        0x10, 0x0F, 0x0E, 0x0D, 0x0C, 0x0B, 0x0A, 0x09,
        0x08, 0x07, 0x06, 0x05, 0x04, 0x03, 0x02, 0x01
    };

    /// <summary>
    /// 加密檔案
    /// </summary>
    /// <param name="inputFilePath">原始檔案路徑</param>
    /// <param name="outputFilePath">加密後輸出路徑</param>
    public static void EncryptFile(string inputFilePath, string outputFilePath)
    {
        using var aes = Aes.Create();
        aes.Key = Key;
        aes.IV = IV;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;

        using var encryptor = aes.CreateEncryptor();
        using var inputStream = File.OpenRead(inputFilePath);
        using var outputStream = File.Create(outputFilePath);
        using var cryptoStream = new CryptoStream(outputStream, encryptor, CryptoStreamMode.Write);
        
        inputStream.CopyTo(cryptoStream);
    }

    /// <summary>
    /// 加密位元組陣列（適用於小型檔案，大型檔案建議使用串流方式）
    /// </summary>
    /// <param name="data">原始資料</param>
    /// <returns>加密後的資料</returns>
    public static byte[] Encrypt(byte[] data)
    {
        using var aes = Aes.Create();
        aes.Key = Key;
        aes.IV = IV;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;

        using var encryptor = aes.CreateEncryptor();
        using var memoryStream = new MemoryStream();
        using var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write);
        
        cryptoStream.Write(data, 0, data.Length);
        cryptoStream.FlushFinalBlock();
        
        return memoryStream.ToArray();
    }

    /// <summary>
    /// 解密檔案
    /// </summary>
    /// <param name="inputFilePath">加密檔案路徑</param>
    /// <param name="outputFilePath">解密後輸出路徑</param>
    public static void DecryptFile(string inputFilePath, string outputFilePath)
    {
        using var aes = Aes.Create();
        aes.Key = Key;
        aes.IV = IV;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;

        using var decryptor = aes.CreateDecryptor();
        using var inputStream = File.OpenRead(inputFilePath);
        using var outputStream = File.Create(outputFilePath);
        using var cryptoStream = new CryptoStream(inputStream, decryptor, CryptoStreamMode.Read);
        
        cryptoStream.CopyTo(outputStream);
    }

    /// <summary>
    /// 解密位元組陣列（適用於小型檔案，大型檔案建議使用串流方式）
    /// </summary>
    /// <param name="encryptedData">加密資料</param>
    /// <returns>解密後的資料</returns>
    public static byte[] Decrypt(byte[] encryptedData)
    {
        using var aes = Aes.Create();
        aes.Key = Key;
        aes.IV = IV;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;

        using var decryptor = aes.CreateDecryptor();
        using var memoryStream = new MemoryStream(encryptedData);
        using var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read);
        using var resultStream = new MemoryStream();
        
        cryptoStream.CopyTo(resultStream);
        
        return resultStream.ToArray();
    }
}
