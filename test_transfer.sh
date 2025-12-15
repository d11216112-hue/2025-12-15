#!/bin/bash

# Test script for Secure Document Transfer System

echo "=================================================="
echo "   電子公文傳輸系統測試腳本"
echo "=================================================="
echo ""

# 建立接收目錄
mkdir -p /home/runner/work/2025-12-15/2025-12-15/SecureDocumentTransfer.Receiver/received

# 啟動接收端 (背景執行)
echo "1. 啟動接收端..."
cd /home/runner/work/2025-12-15/2025-12-15/SecureDocumentTransfer.Receiver

# 使用 echo 提供輸入給接收端
(echo "" && echo "./received" && sleep 30) | dotnet run &
RECEIVER_PID=$!

echo "   接收端 PID: $RECEIVER_PID"
echo "   等待接收端啟動..."
sleep 3

# 執行發送端
echo ""
echo "2. 執行發送端..."
cd /home/runner/work/2025-12-15/2025-12-15/SecureDocumentTransfer.Sender

# 提供輸入：IP (127.0.0.1), Port (8000), 檔案路徑
echo "" | dotnet run <<EOF
127.0.0.1
8000
/home/runner/work/2025-12-15/2025-12-15/test_data/official_document.txt
EOF

echo ""
echo "3. 等待傳輸完成..."
sleep 2

# 終止接收端
echo ""
echo "4. 關閉接收端..."
kill $RECEIVER_PID 2>/dev/null

# 檢查接收的檔案
echo ""
echo "5. 驗證接收檔案..."
cd /home/runner/work/2025-12-15/2025-12-15/SecureDocumentTransfer.Receiver/received

if ls recv_*_official_document.txt 1> /dev/null 2>&1; then
    RECEIVED_FILE=$(ls recv_*_official_document.txt | head -1)
    echo "   ✅ 找到接收檔案: $RECEIVED_FILE"
    echo ""
    echo "   原始檔案內容:"
    echo "   ----------------------------------------"
    cat /home/runner/work/2025-12-15/2025-12-15/test_data/official_document.txt
    echo "   ----------------------------------------"
    echo ""
    echo "   接收檔案內容:"
    echo "   ----------------------------------------"
    cat "$RECEIVED_FILE"
    echo "   ----------------------------------------"
    echo ""
    
    # 比對檔案
    if diff /home/runner/work/2025-12-15/2025-12-15/test_data/official_document.txt "$RECEIVED_FILE" > /dev/null; then
        echo "   ✅ 檔案內容完全一致！傳輸成功！"
    else
        echo "   ❌ 檔案內容不一致！"
    fi
else
    echo "   ❌ 未找到接收檔案"
fi

echo ""
echo "=================================================="
echo "   測試完成"
echo "=================================================="
