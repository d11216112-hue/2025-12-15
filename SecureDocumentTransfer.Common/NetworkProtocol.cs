namespace SecureDocumentTransfer.Common;

/// <summary>
/// 網路傳輸協定常數
/// </summary>
public static class NetworkProtocol
{
    /// <summary>
    /// 預設監聽埠號
    /// </summary>
    public const int DefaultPort = 8000;

    /// <summary>
    /// 檔案名稱最大長度
    /// </summary>
    public const int MaxFileNameLength = 256;

    /// <summary>
    /// 緩衝區大小 (8 KB)
    /// </summary>
    public const int BufferSize = 8192;

    /// <summary>
    /// 封包標頭：檔案名稱長度 (4 bytes) + 檔案大小 (8 bytes) + 檔案名稱 (variable)
    /// </summary>
    public static class Header
    {
        public const int FileNameLengthSize = 4;
        public const int FileSizeSize = 8;
    }
}
