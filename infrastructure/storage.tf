# Opret en unik suffix til storage account navnet
resource "random_string" "storage_suffix" {
  length  = 6
  special = false
  upper   = false
}

# Storage Account til scripts og ABR-installer
resource "azurerm_storage_account" "script_storage" {
  name                     = "scripts${random_string.storage_suffix.result}"
  resource_group_name      = azurerm_resource_group.staging_rg.name
  location                 = azurerm_resource_group.staging_rg.location
  account_tier             = "Standard"
  account_replication_type = "LRS"
}

resource "azurerm_storage_container" "scripts_container" {
  name                  = "scripts"
  storage_account_id    = azurerm_storage_account.script_storage.id
  container_access_type = "private"
}

# Upload Install-ADDSForest.ps1
resource "azurerm_storage_blob" "install_addsforest_ps1" {
  name                   = "Install-ADDSForest.ps1"
  storage_account_name   = azurerm_storage_account.script_storage.name
  storage_container_name = azurerm_storage_container.scripts_container.name
  type                   = "Block"
  source                 = "${path.module}/Install-ADDSForest.ps1"

  depends_on = [azurerm_storage_container.scripts_container]
}

# Upload SetupWorkstation.ps1
resource "azurerm_storage_blob" "setup_workstation_ps1" {
  name                   = "SetupWorkstation.ps1"
  storage_account_name   = azurerm_storage_account.script_storage.name
  storage_container_name = azurerm_storage_container.scripts_container.name
  type                   = "Block"
  source                 = "${path.module}/SetupWorkstation.ps1"

  depends_on = [azurerm_storage_container.scripts_container]
}

# Upload ABRInstaller.msi
resource "azurerm_storage_blob" "abr_installer_msi" {
  name                   = "ABRInstaller.msi"
  storage_account_name   = azurerm_storage_account.script_storage.name
  storage_container_name = azurerm_storage_container.scripts_container.name
  type                   = "Block"
  source                 = "${path.module}/ABRInstaller.msi"
}

# Upload HandleUAC.ps1
resource "azurerm_storage_blob" "handle_uac_ps1" {
  name                   = "HandleUAC.ps1"
  storage_account_name   = azurerm_storage_account.script_storage.name
  storage_container_name = azurerm_storage_container.scripts_container.name
  type                   = "Block"
  source                 = "${path.module}/HandleUAC.ps1"
}

# Upload LaunchElevated.ps1
resource "azurerm_storage_blob" "launch_elevated_ps1" {
  name                   = "LaunchElevated.ps1"
  storage_account_name   = azurerm_storage_account.script_storage.name
  storage_container_name = azurerm_storage_container.scripts_container.name
  type                   = "Block"
  source                 = "${path.module}/LaunchElevated.ps1"
}

data "azurerm_storage_account_sas" "scripts_sas" {
  connection_string = azurerm_storage_account.script_storage.primary_connection_string

  https_only = true
  start      = "2024-11-27T00:00:00Z"  # Sæt en startdato
  expiry     = "2025-11-29T00:00:00Z"  # Sæt en udløbsdato

  services {
    blob  = true
    file  = false
    queue = false
    table = false
  }

  resource_types {
    service   = true
    container = true
    object    = true
  }

  permissions {
    read    = true
    list    = true
    write   = false
    delete  = false
    update  = false
    add     = false
    create  = false
    process = false
    tag     = false
    filter  = false
  }
}


