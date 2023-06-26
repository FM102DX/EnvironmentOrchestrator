
<# 

This script is a part of Environment Orchestrator software, intended to run from special external program

#>

param(

[string] $transactionId='',
[string] $customWokingFolder='',
[string] $domesticCall='no',
[string] $externalCall='no'

)



#COMMON
[string] $scriptDir = $PSScriptRoot;
[string] $scriptFile=$MyInvocation.MyCommand;

#COMMON -- MENUMAKER
[string] $menuFileName='MenuDetail.txt';
[string] $menuFileFullPath=[IO.Path]::Combine($scriptDir,$menuFileName);


[string] $currentLogFileName = '';
[string] $logFileFullPath='';
[string] $logsDirectory=[IO.Path]::Combine($scriptDir,'Logs');
[int]    $howManyLogFilesToLeaveInLogsDirectory = 3

[bool] $canExit=$false;

$menuContent = Get-content $menuFileFullPath

function PerformCls()
{
   #  Clear-host
}
function ShowMenu() {
    ""
    ""
    "MAIN MENU"
    $menuContent
    "99-Exit"
}

<# 
if(($transactionId -eq ''))
{
    $tth = '10.10.10.10, 10.10.10.11';
    $tth_fact = (Get-Item WSMan: \localhost\Client\TrustedHosts).Value
    if ($tth -ne $tth_fact)
    {
        Set-Item WSMan: \localhost\Client\TrustedHosts -Value $tth -Force
    }
    Set-Item -Path  WSMan: \localhost\Service\AllowEncrypted -Value $true
}
 #>
function Pressenter ()
{
    Read-Host -Prompt 'Press enter...'
}


function Log ([string] $text)
{
    if ($script:currentLogFileName -eq '')
    {
        [string]$dtVar = Get-Date -Format "dd-MM-yyyy_HH-mm-ss"
        if($transactionId -eq '') {$tr=''} else {$tr="_$transactionId"}
        $script:currentLogFileName = "$($dtVar)$($tr).txt"
    }
    $Script:logFileFullPath = [IO.Path]::Combine($logsDirectory, $script:currentLogFileName)
    if ([System.IO.File]::Exists($Script:logFileFullPath)){}else{New-Item $Script:logFileFullPath | Out-Null}
    "[$(Get-Date -Format "dd.MM.yyyy-HH:mm:ss")] $text" | Add-Content $Script:logFileFullPath;
}

#COMMON FUNCTION
function CheckLogsDirectory()
{
    CreateDirectoryOnLocalHostIfNotExists -directoryName $logsDirectory
}

function CLearLogs()
{
    KeepOnlyNLastFilesInDirectory -directory $logsDirectory -itemsCol $howManyLogFilesToLeaveInLogsDirectory
}

function CreateDirectoryOnLocalHostIfNotExists([string] $directoryName)
{
    if (-not (Test-Path $directoryName -PathType Container)) {
        New-Item -ItemType Directory -Force -Path $directoryName
    } 
}

function KeepOnlyNLastFilesInDirectory([string] $directory, [int] $itemsCol)
{
    if (-not (Test-path -Path $directory)) {return};
    Get-ChildItem $directory | Sort-Object CreationTime | Select-Object -SkipLast $itemsCol | Remove-Item -Force -Recurse
}








#CORE CONTENT
function test01()
{
    write-host "this is 100000"
    pause
}



#MENU
function ExecMenuItem([string] $menuItem) {
    Log "Executing menu item_$($menuItem)_in operation script"
    
    $ex = $menuItem

    if ($ex -eq "00")     {    }
    #menubegin
    
    elseif ($ex -eq "100000") { test01 }
    elseif ($ex -eq "100001") { ClearLogs }

    elseif ($ex -eq "99") { $script:canExit = $true }
    else {
        "Wrong menu number, try again please"
    }
    #menuend
}

CheckLogsDirectory
CLearLogs

Log "Script started with params: transactionId=$transactionId, customWokingFolder=$customWokingFolder, domesticCall=$domesticCall, externalCall=$externalCall"

if ($script:transactionId -ne '') {
    Log "Gonna exec menu item, script:transactionId= $($script:transactionId)";
    ExecMenuItem -menuItem $script:transactionId;
    exit
}

DO {
    PerformCls
    ShowMenu
    $ex = read-host 'Please enter menu point number'
    ExecMenuItem -menuItem $ex
}
WHILE ($script:canExit -eq $false)