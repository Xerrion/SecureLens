Start-Sleep 300
Add-Type -AssemblyName System.Windows.Forms

# Function to launch an elevated process
function Launch-ElevatedProcess {
    param (
        [string]$processPath,
        [string]$arguments = $null
    )

    if ($arguments) {
        return Start-Process powershell.exe -ArgumentList '-NoProfile','-ExecutionPolicy','Bypass','-Command','"Start-Sleep 15; exit"' -Verb RunAs -PassThru

    } else {
        return Start-Process powershell.exe -ArgumentList '-NoProfile','-ExecutionPolicy','Bypass','-Command','"Start-Sleep 15; exit"' -Verb RunAs -PassThru
    }
}

Start-Job -FilePath "C:\Packages\Plugins\Microsoft.Compute.CustomScriptExtension\1.10.17\Downloads\0\HandleUAC.ps1" | Out-Null

# Introduce a brief delay to ensure HandleUAC.ps1 starts before launching the elevated process
Start-Sleep -Seconds 1

# Launch the elevated process
$process = Launch-ElevatedProcess -processPath "powershell.exe"
