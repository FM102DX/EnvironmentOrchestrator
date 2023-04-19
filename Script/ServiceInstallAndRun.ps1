param([string]$ServiceExeFullPathParam='', [string]$ServiceNameParam='')

"Installing service..."
Read-Host -Prompt "Pressenter"

cls

[bool] $serviceExists = $false;
$service = Get-WmiObject -Class Win32_Service -Filter "Name='$($ServiceNameParam)'"
if (($service.Name).length  -ne 0) {$serviceExists=$true} else {$serviceExists=$false}
$serviceExists

# install service if not exists
if (-not $serviceExists)
{
     Start-Process  -FilePath $ServiceExeFullPathParam -ArgumentList  "install"
     
}

