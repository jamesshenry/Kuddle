# codebase.ps1 - Generate codebase documentation for AI analysis
# This script creates a comprehensive text file containing the directory structure
# and all source code files from your project's source directory for AI processing.
#
# Customize the $sourceDirectory path to match your project's structure.

$repoRoot = git rev-parse --show-toplevel

Write-Host "Repository root: $repoRoot"

$sourceDirectory = Join-Path $repoRoot 'src'  # Change this to your source directory
Write-Host "Source directory: $sourceDirectory"

$outputDir = "$repoRoot/.ai/outputs"  # Change this to your preferred output location
if (-not (Test-Path $outputDir)) {
    New-Item -ItemType Directory -Path $outputDir -Force
}

$outputPath = Join-Path $outputDir 'codebase.txt'
Write-Host "Output path: $outputPath"

# Build directory tree
$directoryTree = Get-ChildItem -Directory -Path $sourceDirectory -Recurse -Exclude obj, bin | ForEach-Object {
    $indent = '  ' * ($_.FullName.Split('\').Length - $sourceDirectory.Split('\').Length)
    "$indent- $($_.Name)"
} | Out-String

$contextBlock = "$directoryTree`n# --- Start of Code Files ---`n`n"
Set-Content -Path $outputPath -Value $contextBlock

# Extension -> language mapping
$languageMap = @{
    '.cs'   = 'csharp'
    '.ps1'  = 'powershell'
    '.json' = 'json'
    '.xml'  = 'xml'
    '.yml'  = 'yaml'
    '.yaml' = 'yaml'
    '.md'   = 'markdown'
    '.sh'   = 'bash'
    '.ts'   = 'typescript'
    '.js'   = 'javascript'
}

# Grab all files except bin/obj
$allFiles = Get-ChildItem -Path $sourceDirectory -Recurse -File -Include *.cs, *.ps1, *.json, *.xml, *.yml, *.yaml, *.md, *.sh, *.ts, *.js

foreach ($file in $allFiles) {
    $relativePath = $file.FullName.Substring($PWD.Path.Length + 1)

    # Determine language based on extension (default: text)
    $ext = $file.Extension.ToLower()
    $lang = if ($languageMap.ContainsKey($ext)) { $languageMap[$ext] } else { 'text' }

    $filePathHeader = @"
// File: $relativePath
"@

    $codeBlockStart = @"
```$lang
"@
    $codeBlockEnd = "`n``````"

    $fileContent = Get-Content -Path $file.FullName | Out-String
    $formattedContent = $filePathHeader + $codeBlockStart + $fileContent + $codeBlockEnd
    Add-Content -Path $outputPath -Value $formattedContent
}