variable "admin_password" {
  description = "Admin password for VMs"
  type        = string
  sensitive   = true
}

variable "subscription_id" {
  description = "Azure Subscription ID"
  type        = string
}

variable "source_address" {
  description = "Source IP address"
  type        = string
}

variable "domain_admin_password" {
  description = "Password for the domain administrator"
  type        = string
  sensitive   = true
}

variable "domain_admin_username" {
  description = "Domain Admin username"
  type        = string
}

variable "safe_mode_password" {
  description = "Password for Safe Mode Administrator"
  type        = string
  sensitive   = true
}

variable "domain_name" {
  description = "Domain name for Active Directory"
  type        = string
}

variable "domain_netbios_name" {
  description = "NetBIOS name for the domain"
  type        = string
}

variable "admin_username" {
  description = "Admin username for VMs"
  type        = string
}

variable "user_name" {
  description = "Name of the user to create in Active Directory"
  type        = string
}

variable "user_password" {
  description = "Password for the new user"
  type        = string
  sensitive   = true
}

variable "group_name" {
  description = "Name of the group to create in Active Directory"
  type        = string
}
