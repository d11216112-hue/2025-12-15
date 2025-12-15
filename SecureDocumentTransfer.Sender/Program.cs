using System.Net.Sockets;
using System.Text;
using SecureDocumentTransfer.Common;

namespace SecureDocumentTransfer.Sender;

/// <summary>
/// 發送端應用程式
/// 負責選取檔案 → 執行 AES 加密 → 建立 TCP 連線 → 發送加密資料流
/// </summary>
class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("==============================================");
        Console.WriteLine("   電子公文傳輸系統 - 發送端 (Sender)");
        Console.WriteLine("   AES 加密 + TCP Socket 網路傳輸");
        Console.WriteLine("==============================================");
        Console.WriteLine();

        try
        {
            // 取得接收端 IP 位址
            Console.Write("請輸入接收端 IP 位址 (預設: 127.0.0.1): ");
            string? ipInput = Console.ReadLine();
            string ipAddress = string.IsNullOrWhiteSpace(ipInput) ? "127.0.0.1" : ipInput;

            // 取得接收端埠號
            Console.Write($"請輸入接收端埠號 (預設: {NetworkProtocol.DefaultPort}): ");
            string? portInput = Console.ReadLine();
            int port = string.IsNullOrWhiteSpace(portInput) 
                ? NetworkProtocol.DefaultPort 
                : int.Parse(portInput);

            // 取得要傳送的檔案路徑
            Console.WriteLine();
            Console.Write("請輸入要傳送的檔案完整路徑: ");
            string? filePath = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
            {
                Console.WriteLine("❌ 錯誤：檔案不存在！");
                return;
            }

            // 讀取檔案
            Console.WriteLine();
            Console.WriteLine($"📄 讀取檔案: {Path.GetFileName(filePath)}");
            byte[] fileData = await File.ReadAllBytesAsync(filePath);
            Console.WriteLine($"   檔案大小: {fileData.Length:N0} bytes");

            // AES 加密
            Console.WriteLine();
            Console.WriteLine("🔒 執行 AES 加密...");
            byte[] encryptedData = AesEncryption.Encrypt(fileData);
            Console.WriteLine($"   加密完成！加密後大小: {encryptedData.Length:N0} bytes");

            // 建立 TCP 連線並傳送
            Console.WriteLine();
            Console.WriteLine($"🌐 連線至 {ipAddress}:{port}...");
            
            using var client = new TcpClient();
            await client.ConnectAsync(ipAddress, port);
            Console.WriteLine("   ✅ 連線成功！");

            using var stream = client.GetStream();

            // 發送檔案名稱長度
            string fileName = Path.GetFileName(filePath);
            byte[] fileNameBytes = Encoding.UTF8.GetBytes(fileName);
            byte[] fileNameLength = BitConverter.GetBytes(fileNameBytes.Length);
            await stream.WriteAsync(fileNameLength, 0, fileNameLength.Length);

            // 發送檔案大小
            byte[] fileSize = BitConverter.GetBytes((long)encryptedData.Length);
            await stream.WriteAsync(fileSize, 0, fileSize.Length);

            // 發送檔案名稱
            await stream.WriteAsync(fileNameBytes, 0, fileNameBytes.Length);

            // 發送加密資料
            Console.WriteLine("📤 傳送加密資料中...");
            await stream.WriteAsync(encryptedData, 0, encryptedData.Length);
            
            Console.WriteLine();
            Console.WriteLine("✅ 檔案傳送成功！");
            Console.WriteLine($"   原始檔案: {fileName}");
            Console.WriteLine($"   原始大小: {fileData.Length:N0} bytes");
            Console.WriteLine($"   加密大小: {encryptedData.Length:N0} bytes");
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
}
