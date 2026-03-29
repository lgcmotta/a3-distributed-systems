Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function New-HexValue {
    param(
        [Parameter(Mandatory = $true)]
        [int]$HexLength
    )

    if (Get-Command openssl -ErrorAction SilentlyContinue) {
        return (& openssl rand -hex ($HexLength / 2)).Trim()
    }

    $value = ''
    while ($value.Length -lt $HexLength) {
        $value += [Guid]::NewGuid().ToString('N')
    }

    return $value.Substring(0, $HexLength)
}

if (-not (Get-Command git -ErrorAction SilentlyContinue)) {
    throw 'git is required to run init.ps1'
}

try {
    $ScriptRoot = (& git rev-parse --show-toplevel).Trim()
}
catch {
    throw 'Unable to determine repository root with git rev-parse --show-toplevel'
}

if ([string]::IsNullOrWhiteSpace($ScriptRoot)) {
    throw 'git rev-parse --show-toplevel returned an empty repository root'
}

if (-not (Test-Path -LiteralPath $ScriptRoot -PathType Container)) {
    throw "Repository root does not exist: $ScriptRoot"
}

$TemplatePath = Join-Path $ScriptRoot 'src/AppHost/Realms/realm.template.json'
$OutputPath = Join-Path $ScriptRoot 'src/AppHost/Realms/realm.json'

if (-not (Test-Path -LiteralPath $TemplatePath -PathType Leaf)) {
    throw "Template file not found: $TemplatePath"
}

$WeatherApiClientSecret = New-HexValue -HexLength 48
$WeatherConsumerAClientSecret = New-HexValue -HexLength 48
$WeatherConsumerBClientSecret = New-HexValue -HexLength 48
$DevAdminPassword = New-HexValue -HexLength 8

$content = Get-Content -LiteralPath $TemplatePath -Raw

$placeholders = @(
    '__WEATHER_API_CLIENT_SECRET__',
    '__WEATHER_CONSUMER_A_CLIENT_SECRET__',
    '__WEATHER_CONSUMER_B_CLIENT_SECRET__',
    '__DEV_ADMIN_PASSWORD__'
)

foreach ($placeholder in $placeholders) {
    if (-not $content.Contains($placeholder)) {
        throw "Placeholder not found in template: $placeholder"
    }
}

$content = $content.Replace('__WEATHER_API_CLIENT_SECRET__', $WeatherApiClientSecret)
$content = $content.Replace('__WEATHER_CONSUMER_A_CLIENT_SECRET__', $WeatherConsumerAClientSecret)
$content = $content.Replace('__WEATHER_CONSUMER_B_CLIENT_SECRET__', $WeatherConsumerBClientSecret)
$content = $content.Replace('__DEV_ADMIN_PASSWORD__', $DevAdminPassword)

Set-Content -LiteralPath $OutputPath -Value $content -NoNewline

Write-Host "Generated realm file: $OutputPath"
Write-Host "weather_api secret: $WeatherApiClientSecret"
Write-Host "weather_consumer_a secret: $WeatherConsumerAClientSecret"
Write-Host "weather_consumer_b secret: $WeatherConsumerBClientSecret"
Write-Host "dev-admin password: $DevAdminPassword"