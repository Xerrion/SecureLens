# Resource Group
resource "azurerm_resource_group" "staging_rg" {
  name     = "staging-environment-rg"
  location = "westeurope"
}

resource "azurerm_virtual_network" "staging_vnet" {
  name                = "staging-vnet"
  address_space       = ["10.0.0.0/16"]
  location            = azurerm_resource_group.staging_rg.location
  resource_group_name = azurerm_resource_group.staging_rg.name

  dns_servers = []
  lifecycle {
    ignore_changes = [
      dns_servers,
    ]
  }
}

resource "azurerm_subnet" "staging_subnet" {
  name                 = "staging-subnet"
  resource_group_name  = azurerm_resource_group.staging_rg.name
  virtual_network_name = azurerm_virtual_network.staging_vnet.name
  address_prefixes     = ["10.0.1.0/24"]
}

# network.tf

# Public IP for Domain Controller VM
resource "azurerm_public_ip" "dc_public_ip" {
  name                = "dc-public-ip"
  location            = azurerm_resource_group.staging_rg.location
  resource_group_name = azurerm_resource_group.staging_rg.name
  allocation_method   = "Static"
  sku                 = "Basic"
}

# Public IP for Workstation VM
resource "azurerm_public_ip" "ws_public_ip" {
  name                = "ws-public-ip"
  location            = azurerm_resource_group.staging_rg.location
  resource_group_name = azurerm_resource_group.staging_rg.name
  allocation_method   = "Static"
  sku                 = "Basic"
}


# Network Security Group
resource "azurerm_network_security_group" "staging_nsg" {
  name                = "staging-nsg"
  location            = azurerm_resource_group.staging_rg.location
  resource_group_name = azurerm_resource_group.staging_rg.name

    security_rule {
    name                       = "Allow-VNet-Inbound"
    priority                   = 100
    direction                  = "Inbound"
    access                     = "Allow"
    protocol                   = "*"
    source_port_range          = "*"
    destination_port_range     = "*"
    source_address_prefix      = "VirtualNetwork"
    destination_address_prefix = "VirtualNetwork"
  }

  security_rule {
    name                       = "Allow-RDP"
    priority                   = 200
    direction                  = "Inbound"
    access                     = "Allow"
    protocol                   = "Tcp"
    source_port_range          = "*"
    destination_port_range     = "3389"
    source_address_prefix      = var.source_address
    destination_address_prefix = "*"
  }
}

# Network Interfaces
## Domain Controller NIC
resource "azurerm_network_interface" "dc_nic" {
  name                = "dc-nic"
  location            = azurerm_resource_group.staging_rg.location
  resource_group_name = azurerm_resource_group.staging_rg.name

  ip_configuration {
    name                          = "dc-ip-config"
    subnet_id                     = azurerm_subnet.staging_subnet.id
    private_ip_address_allocation = "Static"
    private_ip_address            = "10.0.1.4"
    public_ip_address_id          = azurerm_public_ip.dc_public_ip.id
  }
}

## Workstation NIC
resource "azurerm_network_interface" "ws_nic" {
  name                = "ws-nic"
  location            = azurerm_resource_group.staging_rg.location
  resource_group_name = azurerm_resource_group.staging_rg.name

  ip_configuration {
    name                          = "ws-ip-config"
    subnet_id                     = azurerm_subnet.staging_subnet.id
    private_ip_address_allocation = "Static"
    private_ip_address            = "10.0.1.5"
    public_ip_address_id          = azurerm_public_ip.ws_public_ip.id
  }
}


# Network Interface Security Group Association for Domain Controller NIC
resource "azurerm_network_interface_security_group_association" "dc_nic_nsg_assoc" {
  network_interface_id      = azurerm_network_interface.dc_nic.id
  network_security_group_id = azurerm_network_security_group.staging_nsg.id
}

# Network Interface Security Group Association for Workstation NIC
resource "azurerm_network_interface_security_group_association" "ws_nic_nsg_assoc" {
  network_interface_id      = azurerm_network_interface.ws_nic.id
  network_security_group_id = azurerm_network_security_group.staging_nsg.id
}

# Domain Controller VM
resource "azurerm_windows_virtual_machine" "dc_vm" {
  name                = "dc-vm"
  resource_group_name = azurerm_resource_group.staging_rg.name
  location            = azurerm_resource_group.staging_rg.location
  size                = "Standard_B2ms"
  admin_username      = var.admin_username
  admin_password      = var.admin_password

  network_interface_ids = [
    azurerm_network_interface.dc_nic.id,
  ]

  os_disk {
    caching              = "ReadWrite"
    storage_account_type = "Standard_LRS"
  }

  source_image_reference {
    publisher = "MicrosoftWindowsServer"
    offer     = "WindowsServer"
    sku       = "2019-Datacenter"
    version   = "latest"
  }
}

# Workstation VM
resource "azurerm_windows_virtual_machine" "ws_vm" {
  name                = "ws-vm"
  resource_group_name = azurerm_resource_group.staging_rg.name
  location            = azurerm_resource_group.staging_rg.location
  size                = "Standard_B2ms"
  admin_username      = var.admin_username
  admin_password      = var.admin_password

  network_interface_ids = [
    azurerm_network_interface.ws_nic.id,
  ]

  os_disk {
    caching              = "ReadWrite"
    storage_account_type = "Standard_LRS"
  }

  source_image_reference {
    publisher = "microsoftwindowsdesktop"
    offer     = "windows-10"
    sku       = "win10-22h2-pro"
    version   = "latest"
  }

}
