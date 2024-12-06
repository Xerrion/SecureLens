output "storage_account_name" {
  value = azurerm_storage_account.script_storage.name
}

output "resource_group_name" {
  value = azurerm_resource_group.staging_rg.name
}

output "scripts_sas_token" {
  value     = data.azurerm_storage_account_sas.scripts_sas.sas
  sensitive = true
}

output "storage_account_blob_endpoint" {
  value = azurerm_storage_account.script_storage.primary_blob_endpoint
}

output "dc_public_ip_address" {
  value = azurerm_public_ip.dc_public_ip.ip_address
}

output "ws_public_ip_address" {
  value = azurerm_public_ip.ws_public_ip.ip_address
}

output "dc_dns_address" {
  value = azurerm_network_interface.dc_nic.private_ip_address
}
