Add-Type -AssemblyName System.Windows.Forms

# Optional: Function to log messages for debugging
function Write-Log {
    param (
        [string]$Message,
        [string]$Level = "INFO"
    )
    $timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    "$timestamp [$Level] - $Message" | Out-File -FilePath "C:\Users\bettina\HandleUAC.log" -Append
}

# Log start of helper script
Write-Log "HandleUAC.ps1 started."

# Wait sufficient time for the UAC prompt to appear
Start-Sleep -Seconds 2  # Adjust this delay as necessary

try {
    # Bring the UAC prompt to the foreground
    Add-Type @"
    using System;
    using System.Runtime.InteropServices;
    public class User32 {
        [DllImport("user32.dll")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);
    }
"@

    $uacWindow = Get-Process | Where-Object { $_.MainWindowTitle -like "*User Account Control*" }
    if ($uacWindow) {
        [User32]::SetForegroundWindow($uacWindow.MainWindowHandle)
        Write-Log "UAC window found and brought to foreground."
    } else {
        Write-Log "UAC window not found."
    }

    # Send keystrokes to handle the UAC prompt
    [System.Windows.Forms.SendKeys]::SendWait("{LEFT}")
    Write-Log "Sent '{LEFT}' key."
    Start-Sleep -Milliseconds 500  # Brief pause between keystrokes
    [System.Windows.Forms.SendKeys]::SendWait("{LEFT}")  # Press 'Left' key twice
    Write-Log "Sent second '{LEFT}' key."
    Start-Sleep -Milliseconds 500
    [System.Windows.Forms.SendKeys]::SendWait("{ENTER}") # Press 'Enter' to confirm
    Write-Log "Sent '{ENTER}' key."
} catch {
    Write-Log "Error sending keystrokes: $_" "ERROR"
}

# Log end of helper script
Write-Log "HandleUAC.ps1 completed."