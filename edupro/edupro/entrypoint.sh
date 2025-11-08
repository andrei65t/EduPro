#!/bin/bash
set -e

# Start Python OCR server in background (port 8001)
python3 -m uvicorn server_ocr:app --host 0.0.0.0 --port 8001 &

# Wait a short moment for the OCR server to start (optional)
sleep 1

# Start .NET webapp in foreground
# The edupro project publishes to edupro.dll
exec dotnet edupro.dll
