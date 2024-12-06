param(
    [string]$DomainName,
    [string]$DomainNetBIOSName,
    [string]$AdminUsername,
    [string]$AdminPassword,
    [string]$DomainControllerIP,
    [string]$DomainAdminUsername,
    [string]$DomainAdminPassword,
    [string]$UserName,
    [string]$UserPassword,
    [string]$GroupName
)

# Define the log file path
$logFile = "C:\MyLogFile.log"

# Function to write logs with timestamp and log level
function Write-Log {
    param(
        [string]$Message,
        [string]$Level = "INFO"
    )
    $timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    $logEntry = "$timestamp [$Level] - $Message"
    Add-Content -Path $logFile -Value $logEntry
}

# Start logging
Write-Log "Script execution started."

# Function to mask sensitive information
function Mask-String {
    param(
        [string]$InputString
    )
    if ([string]::IsNullOrEmpty($InputString)) {
        return ""
    }
    return '*' * $InputString.Length
}

# Set DNS server to Domain Controller IP on all active network interfaces
Write-Log "Retrieving active network adapters..."
try {
    $adapters = Get-NetAdapter | Where-Object { $_.Status -eq 'Up' }
    if ($adapters) {
        $adapterNames = ($adapters | ForEach-Object { $_.Name }) -join ', '
        Write-Log "Active adapters found: $adapterNames"
    } else {
        Write-Log "No active network adapters found." "ERROR"
     
    }
} catch {
    Write-Log ("Error retrieving network adapters: {0}" -f $_) "ERROR"
   
}

foreach ($adapter in $adapters) {
    Write-Log ("Setting DNS server to $DomainControllerIP for adapter $($adapter.Name)...")
    try {
        Set-DnsClientServerAddress -InterfaceIndex $adapter.InterfaceIndex -ServerAddresses $DomainControllerIP
        Write-Log ("DNS server set successfully for adapter $($adapter.Name).")
    } catch {
        Write-Log ("Failed to set DNS server for adapter $($adapter.Name): {0}" -f $_) "ERROR"
      
    }
}

# Allow ICMPv4-In traffic
Write-Log "Allowing ICMPv4-In traffic..."
try {
    New-NetFirewallRule -DisplayName "Allow ICMPv4-In" -Protocol ICMPv4 -Direction Inbound -Action Allow
    Write-Log "ICMPv4-In traffic allowed successfully."
} catch {
    Write-Log ("Failed to allow ICMPv4-In traffic: {0}" -f $_) "ERROR"

}

# Allow necessary ports for domain communication
$ports = @(53, 88, 135, 389, 445, 464)
Write-Log ("Allowing inbound traffic on the following ports: {0}..." -f ($ports -join ', '))
foreach ($port in $ports) {
    Write-Log ("Allowing TCP on port {0}..." -f $port)
    try {
        New-NetFirewallRule -DisplayName ("Allow TCP Port {0}" -f $port) -Direction Inbound -Protocol TCP -LocalPort $port -Action Allow
        Write-Log ("TCP port {0} allowed successfully." -f $port)
    } catch {
        Write-Log ("Failed to allow TCP port {0}: {1}" -f $port, $_) "ERROR"
       
    }

    Write-Log ("Allowing UDP on port {0}..." -f $port)
    try {
        New-NetFirewallRule -DisplayName ("Allow UDP Port {0}" -f $port) -Direction Inbound -Protocol UDP -LocalPort $port -Action Allow
        Write-Log ("UDP port {0} allowed successfully." -f $port)
    } catch {
        Write-Log ("Failed to allow UDP port {0}: {1}" -f $port, $_) "ERROR"
       
    }
}

# Pause for 10 minutes to wait for the Domain Controller to be ready
Write-Log "Pausing execution for 600 seconds (10 minutes)..."
Start-Sleep -Seconds 600
Write-Log "Resuming execution after pause."

# Join the domain
Write-Log "Preparing to join the domain: $DomainName..."
try {
    $securePassword = ConvertTo-SecureString $AdminPassword -AsPlainText -Force
    Write-Log "Password secured successfully."
    $credential = New-Object System.Management.Automation.PSCredential ("$DomainName\$DomainAdminUsername", $securePassword)
    Write-Log "Credential object created successfully."
} catch {
    Write-Log ("Error creating credential object: {0}" -f $_) "ERROR"
   
}

Write-Log ("Attempting to join the domain: {0}..." -f $DomainName)
try {
    Add-Computer -DomainName $DomainName -Credential $credential -ErrorAction Stop
    Write-Log ("Successfully joined the domain: {0}." -f $DomainName)
} catch {
    Write-Log ("Failed to join domain: {0}" -f $_) "ERROR"
    
}

# Install Active Directory Tools
Write-Log "Adding Windows capability: Rsat.ActiveDirectory.DS-LDS.Tools~~~~0.0.1.0..."
try {
    Add-WindowsCapability -Online -Name Rsat.ActiveDirectory.DS-LDS.Tools~~~~0.0.1.0 -ErrorAction Stop
    Write-Log "Windows capability added successfully."
} catch {
    Write-Log ("Failed to add Windows capability: {0}" -f $_) "ERROR"
    
}

# Import Active Directory Module
Write-Log "Importing ActiveDirectory module..."
try {
    Import-Module ActiveDirectory -ErrorAction Stop
    Write-Log "ActiveDirectory module imported successfully."
} catch {
    Write-Log ("Failed to import ActiveDirectory module: {0}" -f $_) "ERROR"
   
}

# Create credential object for domain admin
Write-Log "Creating credential object for domain admin..."
try {
    $domainSecurePassword = ConvertTo-SecureString $DomainAdminPassword -AsPlainText -Force
    $domainCredential = New-Object System.Management.Automation.PSCredential ("$DomainAdminUsername", $domainSecurePassword)
    Write-Log "Domain admin credential object created successfully."
} catch {
    Write-Log ("Error creating domain admin credential: {0}" -f $_) "ERROR"
   
}

# Create the user
Write-Log "Creating new AD user: $UserName..."
try {
    $secureUserPassword = ConvertTo-SecureString $UserPassword -AsPlainText -Force
    $UserParameters = @{
        Name                  = $UserName
        SamAccountName        = $UserName
        UserPrincipalName     = "$UserName@$DomainName"
        AccountPassword       = $secureUserPassword
        Enabled               = $true
        ChangePasswordAtLogon = $false
        PasswordNeverExpires  = $true
        Path                  = "CN=Users,DC=$((($DomainName -split '\.')[0])),DC=$((($DomainName -split '\.')[1]))"
    }
    New-ADUser @UserParameters -Credential $domainCredential
    Write-Log ("User '{0}' created successfully." -f $UserName)
} catch {
    Write-Log ("Error creating user '{0}': {1}" -f $UserName, $_) "ERROR"
   
}

# Create the group
Write-Log "Creating new AD group: $GroupName..."
try {
    $GroupParameters = @{
        Name           = $GroupName
        SamAccountName = $GroupName
        GroupScope     = 'Global'
        Path           = "CN=Users,DC=$((($DomainName -split '\.')[0])),DC=$((($DomainName -split '\.')[1]))"
    }
    New-ADGroup @GroupParameters -Credential $domainCredential
    Write-Log ("Group '{0}' created successfully." -f $GroupName)
} catch {
    Write-Log ("Error creating group '{0}': {1}" -f $GroupName, $_) "ERROR"
  
}

# Add the user to the group
Write-Log "Adding user '$UserName' to group '$GroupName'..."
try {
    Add-ADGroupMember -Identity $GroupName -Members $UserName -Credential $domainCredential
    Write-Log ("User '{0}' added to group '{1}' successfully." -f $UserName, $GroupName)
} catch {
    Write-Log ("Error adding user '{0}' to group '{1}': {2}" -f $UserName, $GroupName, $_) "ERROR"
   
}

# Set workstation to be managed by the user
$computerName = $env:COMPUTERNAME
Write-Log ("Setting AD computer '{0}' to be managed by user '{1}'..." -f $computerName, $UserName)
try {
    Set-ADComputer -Identity $computerName -ManagedBy $UserName -Credential $domainCredential
    Write-Log ("Computer '{0}' is now managed by '{1}'." -f $computerName, $UserName)
} catch {
    Write-Log ("Error setting computer '{0}' to be managed by '{1}': {2}" -f $computerName, $UserName, $_) "ERROR"
    
}

# Define the path to the installer and log file
$installerPath = "C:\Packages\Plugins\Microsoft.Compute.CustomScriptExtension\1.10.17\Downloads\0\ABRInstaller.msi"
$logPath = "C:\ABRInstall.log"

# Start the MSI installation with logging
Write-Log "Starting Admin By Request installation..."
try {
    $process = Start-Process -FilePath "msiexec.exe" `
                             -ArgumentList "/i `"$installerPath`" /qn /norestart /L*v `"$logPath`"" `
                             -Wait `
                             -PassThru
                             
    # Capture the exit code
    $exitCode = $process.ExitCode
    Write-Log "msiexec.exe exited with code $exitCode."
    
    if ($exitCode -eq 0) {
        Write-Log "Admin By Request installed successfully."
    } else {
        Write-Log "Admin By Request installation failed with exit code $exitCode." "ERROR"
    }
} catch {
    Write-Log ("Failed to start Admin By Request installation: {0}" -f $_) "ERROR"
}

# Configure automatic logon for user 'bettina'
Write-Log "Configuring automatic logon for user '$UserName'..."
try {
    $registryPath = "HKLM:\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Winlogon"
    Set-ItemProperty -Path $registryPath -Name "AutoAdminLogon" -Value "1" -Type String
    Set-ItemProperty -Path $registryPath -Name "DefaultUserName" -Value $UserName -Type String
    Set-ItemProperty -Path $registryPath -Name "DefaultDomainName" -Value $DomainNetBIOSName -Type String
    Set-ItemProperty -Path $registryPath -Name "DefaultPassword" -Value $UserPassword -Type String
    Write-Log "Automatic logon configured successfully for user '$UserName'."
} catch {
    Write-Log ("Failed to configure automatic logon for user '{0}': {1}" -f $UserName, $_) "ERROR"
   
}

# Define the name of the scheduled task
$TaskName = "ABR_RunElevatedPowerShell"
Write-Log ("Creating scheduled task '{0}'..." -f $TaskName)

# Define the path to LaunchElevated.ps1
$LaunchElevatedScript = "C:\Packages\Plugins\Microsoft.Compute.CustomScriptExtension\1.10.17\Downloads\0\LaunchElevated.ps1"

# Define the action: run PowerShell.exe with the LaunchElevated.ps1 script
$Action = New-ScheduledTaskAction -Execute "PowerShell.exe" `
    -Argument "-NoProfile -ExecutionPolicy Bypass -File `"$LaunchElevatedScript`""

# Define the trigger: at user logon
$Trigger = New-ScheduledTaskTrigger -AtLogOn -User $UserName

# Define the principal: run with highest privileges
$Principal = New-ScheduledTaskPrincipal -UserId $UserName -RunLevel Highest -LogonType Interactive

# Define settings: hidden, no task history, etc.
$Settings = New-ScheduledTaskSettingsSet -AllowStartIfOnBatteries -DontStopIfGoingOnBatteries -Hidden -StartWhenAvailable

# Register the scheduled task
try {
    Register-ScheduledTask -TaskName $TaskName `
        -Action $Action `
        -Trigger $Trigger `
        -Principal $Principal `
        -Settings $Settings

    Write-Log ("Scheduled Task '{0}' created successfully." -f $TaskName)
} catch {
    Write-Log ("Failed to create Scheduled Task '{0}': {1}" -f $TaskName, $_) "ERROR"
}

# Add user to Remote Desktop Users group
Write-Log ("Adding user '{0}' to 'Remote Desktop Users' group..." -f $UserName)
try {
    Add-LocalGroupMember -Group "Remote Desktop Users" -Member $UserName
    Write-Log ("User '{0}' added to 'Remote Desktop Users' group successfully." -f $UserName)
} catch {
    Write-Log ("Failed to add user '{0}' to 'Remote Desktop Users' group: {1}" -f $UserName, $_) "ERROR"
}

# Restart the computer
Write-Log "Restarting the computer..."
try {
    Restart-Computer -Force
    Write-Log "Computer restart initiated successfully."
} catch {
    Write-Log ("Failed to restart the computer: {0}" -f $_) "ERROR"
   
}
