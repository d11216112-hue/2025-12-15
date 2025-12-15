using System.Net;
using System.Net.Sockets;
using System.Text;
using SecureDocumentTransfer.Common;

namespace SecureDocumentTransfer.Receiver;

/// <summary>
/// 接收端應用程式
/// 負責監聽 Port → 接收資料流 → 執行 AES 解密 → 還原檔案 → 存入硬碟
/// </summary>
class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("==============================================");
        Console.WriteLine("   電子公文傳輸系統 - 接收端 (Receiver)");
        Console.WriteLine("   TCP Socket 監聽 + AES 解密");
        Console.WriteLine("==============================================");
        Console.WriteLine();

        try
        {
            // 取得監聽埠號
            Console.Write($"請輸入監聽埠號 (預設: {NetworkProtocol.DefaultPort}): ");
            string? portInput = Console.ReadLine();
            int port = string.IsNullOrWhiteSpace(portInput) 
                ? NetworkProtocol.DefaultPort 
                : int.Parse(portInput);

            // 取得儲存目錄
            Console.Write("請輸入接收檔案儲存目錄 (預設: ./received): ");
            string? savePathInput = Console.ReadLine();
            string savePath = string.IsNullOrWhiteSpace(savePathInput) 
                ? "./received" 
                : savePathInput;

            // 建立儲存目錄
            if (!Directory.Exists(savePath))
            {
                Directory.CreateDirectory(savePath);
                Console.WriteLine($"   📁 已建立目錄: {Path.GetFullPath(savePath)}");
            }

            // 啟動 TCP 伺服器
            var listener = new TcpListener(IPAddress.Any, port);
            listener.Start();

            Console.WriteLine();
            Console.WriteLine($"🎧 伺服器已啟動，監聽埠號: {port}");
            Console.WriteLine($"💾 儲存目錄: {Path.GetFullPath(savePath)}");
            Console.WriteLine("⏳ 等待發送端連線...");
            Console.WriteLine();

            while (true)
            {
                // 接受連線
                using var client = await listener.AcceptTcpClientAsync();
                var clientEndPoint = client.Client.RemoteEndPoint;
                Console.WriteLine($"📞 收到來自 {clientEndPoint} 的連線");

                using var stream = client.GetStream();

                try
                {
                    // 接收檔案名稱長度 (4 bytes)
                    byte[] fileNameLengthBuffer = new byte[4];
                    await ReadExactAsync(stream, fileNameLengthBuffer, 4);
                    int fileNameLength = BitConverter.ToInt32(fileNameLengthBuffer, 0);

                    // 接收檔案大小 (8 bytes)
                    byte[] fileSizeBuffer = new byte[8];
                    await ReadExactAsync(stream, fileSizeBuffer, 8);
                    long fileSize = BitConverter.ToInt64(fileSizeBuffer, 0);

                    // 接收檔案名稱
                    byte[] fileNameBuffer = new byte[fileNameLength];
                    await ReadExactAsync(stream, fileNameBuffer, fileNameLength);
                    string fileName = Encoding.UTF8.GetString(fileNameBuffer);

                    Console.WriteLine($"📄 檔案名稱: {fileName}");
                    Console.WriteLine($"   加密大小: {fileSize:N0} bytes");

                    // 接收加密資料
                    Console.WriteLine("📥 接收加密資料中...");
                    byte[] encryptedData = new byte[fileSize];
                    await ReadExactAsync(stream, encryptedData, (int)fileSize);
                    Console.WriteLine("   ✅ 接收完成！");

                    // AES 解密
                    Console.WriteLine("🔓 執行 AES 解密...");
                    byte[] decryptedData = AesEncryption.Decrypt(encryptedData);
                    Console.WriteLine($"   解密完成！原始大小: {decryptedData.Length:N0} bytes");

                    // 儲存檔案
                    string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                    string extension = Path.GetExtension(fileName);
                    string baseName = Path.GetFileNameWithoutExtension(fileName);
                    string outputFileName = $"recv_{timestamp}_{baseName}{extension}";
                    string outputPath = Path.Combine(savePath, outputFileName);

                    await File.WriteAllBytesAsync(outputPath, decryptedData);

                    Console.WriteLine();
                    Console.WriteLine("✅ 檔案儲存成功！");
                    Console.WriteLine($"   原始檔名: {fileName}");
                    Console.WriteLine($"   儲存路徑: {Path.GetFullPath(outputPath)}");
                    Console.WriteLine($"   檔案大小: {decryptedData.Length:N0} bytes");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ 處理檔案時發生錯誤: {ex.Message}");
                }

                Console.WriteLine();
                Console.WriteLine("⏳ 等待下一個連線...");
                Console.WriteLine();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine();
            Console.WriteLine($"❌ 發生錯誤: {ex.Message}");
            Console.WriteLine($"   詳細資訊: {ex.GetType().Name}");
        }

        Console.WriteLine();
        Console.WriteLine("按任意鍵結束...");
        Console.ReadKey();
    }

    /// <summary>
    /// 確保讀取指定長度的資料
    /// </summary>
    private static async Task ReadExactAsync(NetworkStream stream, byte[] buffer, int count)
    {
        int totalRead = 0;
        while (totalRead < count)
        {
            int bytesRead = await stream.ReadAsync(buffer, totalRead, count - totalRead);
            if (bytesRead == 0)
            {
                throw new EndOfStreamException("連線意外中斷");
            }
            totalRead += bytesRead;
        }
    }
}
