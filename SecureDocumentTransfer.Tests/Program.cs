using System.Text;
using SecureDocumentTransfer.Common;

namespace SecureDocumentTransfer.Tests;

/// <summary>
/// 測試程式 - 驗證 AES 加密/解密功能
/// </summary>
class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("==============================================");
        Console.WriteLine("   電子公文傳輸系統 - 功能測試");
        Console.WriteLine("==============================================");
        Console.WriteLine();

        // 測試 1: 文字加密/解密
        TestTextEncryption();
        Console.WriteLine();

        // 測試 2: 檔案加密/解密
        TestFileEncryption();
        Console.WriteLine();

        Console.WriteLine("==============================================");
        Console.WriteLine("   所有測試完成");
        Console.WriteLine("==============================================");
    }

    static void TestTextEncryption()
    {
        Console.WriteLine("【測試 1】文字資料加密/解密");
        Console.WriteLine("------------------------------------------");

        string originalText = "這是一份極機密公文\n公文內容：資訊安全應用專題\n編號：2025-001";
        byte[] originalData = Encoding.UTF8.GetBytes(originalText);

        Console.WriteLine($"原始文字長度: {originalData.Length} bytes");
        Console.WriteLine($"原始內容:\n{originalText}");
        Console.WriteLine();

        // 加密
        byte[] encryptedData = AesEncryption.Encrypt(originalData);
        Console.WriteLine($"加密後長度: {encryptedData.Length} bytes");
        Console.WriteLine($"加密後內容 (前 32 bytes): {BitConverter.ToString(encryptedData.Take(32).ToArray())}");
        Console.WriteLine();

        // 解密
        byte[] decryptedData = AesEncryption.Decrypt(encryptedData);
        string decryptedText = Encoding.UTF8.GetString(decryptedData);

        Console.WriteLine($"解密後長度: {decryptedData.Length} bytes");
        Console.WriteLine($"解密內容:\n{decryptedText}");
        Console.WriteLine();

        // 驗證
        if (originalText == decryptedText)
        {
            Console.WriteLine("✅ 測試通過！解密內容與原始內容完全一致");
        }
        else
        {
            Console.WriteLine("❌ 測試失敗！解密內容與原始內容不一致");
        }
    }

    static void TestFileEncryption()
    {
        Console.WriteLine("【測試 2】檔案加密/解密");
        Console.WriteLine("------------------------------------------");

        string testDir = "/tmp/secure_doc_test";
        Directory.CreateDirectory(testDir);

        string originalFile = Path.Combine(testDir, "original.txt");
        string encryptedFile = Path.Combine(testDir, "encrypted.bin");
        string decryptedFile = Path.Combine(testDir, "decrypted.txt");

        // 建立測試檔案
        string fileContent = @"電子公文傳輸系統測試檔案
========================================
公文類型：極機密
發文單位：資訊安全部門
日期：2025-12-15

測試內容：
本檔案用於驗證 AES 加密傳輸系統的正確性。
系統採用 128 位元 AES-CBC 模式進行加密。
確保公文在網路傳輸過程中的機密性。";

        File.WriteAllText(originalFile, fileContent);
        Console.WriteLine($"建立測試檔案: {originalFile}");
        Console.WriteLine($"原始檔案大小: {new FileInfo(originalFile).Length} bytes");
        Console.WriteLine();

        // 加密檔案
        AesEncryption.EncryptFile(originalFile, encryptedFile);
        Console.WriteLine($"加密完成: {encryptedFile}");
        Console.WriteLine($"加密檔案大小: {new FileInfo(encryptedFile).Length} bytes");
        Console.WriteLine();

        // 解密檔案
        AesEncryption.DecryptFile(encryptedFile, decryptedFile);
        Console.WriteLine($"解密完成: {decryptedFile}");
        Console.WriteLine($"解密檔案大小: {new FileInfo(decryptedFile).Length} bytes");
        Console.WriteLine();

        // 比對內容
        string originalContent = File.ReadAllText(originalFile);
        string decryptedContent = File.ReadAllText(decryptedFile);

        Console.WriteLine("原始檔案內容:");
        Console.WriteLine("------------------------------------------");
        Console.WriteLine(originalContent);
        Console.WriteLine("------------------------------------------");
        Console.WriteLine();

        Console.WriteLine("解密檔案內容:");
        Console.WriteLine("------------------------------------------");
        Console.WriteLine(decryptedContent);
        Console.WriteLine("------------------------------------------");
        Console.WriteLine();

        if (originalContent == decryptedContent)
        {
            Console.WriteLine("✅ 測試通過！解密檔案與原始檔案完全一致");
        }
        else
        {
            Console.WriteLine("❌ 測試失敗！解密檔案與原始檔案不一致");
        }

        // 清理測試檔案
        try
        {
            Directory.Delete(testDir, true);
            Console.WriteLine();
            Console.WriteLine("測試檔案已清理");
        }
        catch { }
    }
}

