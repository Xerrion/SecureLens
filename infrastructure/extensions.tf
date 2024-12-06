
resource "azurerm_virtual_machine_extension" "dc_extension" {
  name                 = "dc-extension"
  virtual_machine_id   = azurerm_windows_virtual_machine.dc_vm.id
  publisher            = "Microsoft.Compute"
  type                 = "CustomScriptExtension"
  type_handler_version = "1.10"

  settings = <<SETTINGS
    {
      "fileUris": ["${azurerm_storage_account.script_storage.primary_blob_endpoint}${azurerm_storage_container.scripts_container.name}/Install-ADDSForest.ps1?${data.azurerm_storage_account_sas.scripts_sas.sas}"],
      "commandToExecute": "powershell -ExecutionPolicy Unrestricted -Command \"& { .\\Install-ADDSForest.ps1 -DomainName '${var.domain_name}' -DomainNetBIOSName '${var.domain_netbios_name}' -SafeModePassword '${var.safe_mode_password}' }\""
    }
  SETTINGS

  depends_on = [
    azurerm_windows_virtual_machine.dc_vm,
    azurerm_storage_blob.install_addsforest_ps1
  ]
}

resource "azurerm_virtual_machine_extension" "ws_extension" {
  name                 = "ws-extension"
  virtual_machine_id   = azurerm_windows_virtual_machine.ws_vm.id
  publisher            = "Microsoft.Compute"
  type                 = "CustomScriptExtension"
  type_handler_version = "1.10"

  settings = <<SETTINGS
    {
      "fileUris": [
        "${azurerm_storage_account.script_storage.primary_blob_endpoint}${azurerm_storage_container.scripts_container.name}/SetupWorkstation.ps1?${data.azurerm_storage_account_sas.scripts_sas.sas}",
        "${azurerm_storage_account.script_storage.primary_blob_endpoint}${azurerm_storage_container.scripts_container.name}/ABRInstaller.msi?${data.azurerm_storage_account_sas.scripts_sas.sas}",
        "${azurerm_storage_account.script_storage.primary_blob_endpoint}${azurerm_storage_container.scripts_container.name}/LaunchElevated.ps1?${data.azurerm_storage_account_sas.scripts_sas.sas}",
        "${azurerm_storage_account.script_storage.primary_blob_endpoint}${azurerm_storage_container.scripts_container.name}/HandleUAC.ps1?${data.azurerm_storage_account_sas.scripts_sas.sas}"
      ],
      "commandToExecute": "powershell -ExecutionPolicy Unrestricted -Command \"& { .\\SetupWorkstation.ps1 -DomainName '${var.domain_name}' -DomainNetBIOSName '${var.domain_netbios_name}' -AdminUsername '${var.admin_username}' -AdminPassword '${var.admin_password}' -DomainControllerIP '${azurerm_network_interface.dc_nic.ip_configuration[0].private_ip_address}' -DomainAdminUsername '${var.domain_admin_username}' -DomainAdminPassword '${var.domain_admin_password}' -UserName '${var.user_name}' -UserPassword '${var.user_password}' -GroupName '${var.group_name}' }\""
    }
  SETTINGS

  depends_on = [
    azurerm_virtual_machine_extension.dc_extension,
    azurerm_storage_blob.setup_workstation_ps1,
    azurerm_storage_blob.abr_installer_msi
  ]
}
