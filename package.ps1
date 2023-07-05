param (
	[switch]$NoArchive,
	[string]$OutputDirectory = $PSScriptRoot
)

Set-Location "$PSScriptRoot"

$DistDir = "$OutputDirectory/dist"
if ($NoArchive)
{
	$ZipWorkDir = "$OutputDirectory"
}
else
{
	$ZipWorkDir = "$DistDir/tmp"
}
$ZipRootDir = "$ZipWorkDir/BepInEx"
$ZipInnerDir = "$ZipRootDir/plugins/SmoothCamera/"
$BuildDir = "build"
$LicenseFile = "LICENSE"
$AssemblyFile = "$BuildDir/SmoothCamera.dll"

New-Item "$ZipInnerDir" -ItemType Directory -Force
Copy-Item -Force -Path "$LicenseFile", "$AssemblyFile" -Destination "$ZipInnerDir"

if (!$NoArchive)
{
	$VERSION = (Select-String -Pattern '<Version>([0-9]+\.[0-9]+\.[0-9]+)</Version>' -Path SmoothCamera/SmoothCamera.csproj).Matches.Groups[1]
	$FILE_NAME = "$DistDir/SmoothCamera_v$VERSION.zip"
	Compress-Archive -Update -CompressionLevel Fastest -Path "$ZipRootDir" -DestinationPath "$FILE_NAME"
	Remove-Item -LiteralPath "$ZipWorkDir" -Force -Recurse
}
