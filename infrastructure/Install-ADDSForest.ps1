param(
    [string]$DomainName,
    [string]$DomainNetBIOSName,
    [string]$SafeModePassword
)

# Installér Active Directory Domain Services
Install-WindowsFeature AD-Domain-Services -IncludeManagementTools

# Promover til Domain Controller
Install-ADDSForest `
  -DomainName $DomainName `
  -DomainNetBIOSName $DomainNetBIOSName `
  -SafeModeAdministratorPassword (ConvertTo-SecureString $SafeModePassword -AsPlainText -Force) `
  -InstallDns `
  -Force:$true
