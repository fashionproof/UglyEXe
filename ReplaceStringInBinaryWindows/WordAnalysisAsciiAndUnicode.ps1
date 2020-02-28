$FolderPath = "C:\Tools\NCReplace\AutoReplace\"
$originalExe = "nc.exe"
$exePath = "$($FolderPath)$($originalExe)"
$outExe = "out.exe"
$exeNewPath = "$($FolderPath)$($outExe)"

$AsciiOriginalAnaysisFileName = "Ugly_EXe_AsciiAnalysisOriginal.txt"
$AsciiOriginalAnaysisPath = "$($FolderPath)$($AsciiOriginalAnaysisFileName)"

$AsciiUglifiedAnaysisFileName = "Ugly_EXe_AsciiAnalysisUglified.txt"
$AsciiUglifiedAnaysisPath = "$($FolderPath)$($AsciiUglifiedAnaysisFileName)"


$UnicodeOriginalAnaysisFileName = "Ugly_EXe_UnicodeAnalysisOriginal.txt"
$UnicodeOriginalAnaysisPath = "$($FolderPath)$($UnicodeOriginalAnaysisFileName)"

$UnicodeUglifiedAnaysisFileName = "Ugly_EXe_UnicodAnalysisUglified.txt"
$UnicodeUglifiedAnaysisPath = "$($FolderPath)$($UnicodeUglifiedAnaysisFileName)"



write-host $exeName
write-host $exeNewPath
write-host $AsciiOriginalAnaysisPath
write-host $AsciiUglifiedAAnaysisPath
write-host $UnicodeOriginalAnaysisPath
write-host $UnicodeUglifiedAnaysisPath

$collectionWithItems = New-Object System.Collections.ArrayList


#unicode original
$stringListUnicode = cmd /c "C:\Tools\Excluded\strings64.exe -u $exePath" '2>&1' 
foreach ($string in $stringListUnicode)
{
    $stringProps = New-Object System.Object
    $stringProps | Add-Member -MemberType NoteProperty -Name "StringValue" -Value $string
    $stringProps | Add-Member -MemberType NoteProperty -Name "StringLength" -Value $string.Length
    $collectionWithItems.Add($stringProps) | Out-Null
}

$collectionWithItems | Sort-Object -Property StringLength -Descending | Out-File $UnicodeOriginalAnaysisPath

#unicode new
$collectionWithItems = New-Object System.Collections.ArrayList
$stringListUnicode = cmd /c "C:\Tools\Excluded\strings64.exe -u $exeNewPath" '2>&1' 
foreach ($string in $stringListUnicode)
{
   
    $stringProps = New-Object System.Object
    $stringProps | Add-Member -MemberType NoteProperty -Name "StringValue" -Value $string
    $stringProps | Add-Member -MemberType NoteProperty -Name "StringLength" -Value $string.Length
    $collectionWithItems.Add($stringProps) | Out-Null
}

$collectionWithItems | Sort-Object -Property StringLength -Descending | Out-File $UnicodeUglifiedAnaysisPath


#ascii original
$collectionWithItemsAscii = New-Object System.Collections.ArrayList
$stringListAscii = cmd /c "C:\Tools\Excluded\strings64.exe -a $exePath" '2>&1' 
foreach ($string in $stringListAscii)
{
    $stringProps = New-Object System.Object
    $stringProps | Add-Member -MemberType NoteProperty -Name "StringValue" -Value $string
    $stringProps | Add-Member -MemberType NoteProperty -Name "StringLength" -Value $string.Length
    $collectionWithItemsAscii.Add($stringProps) | Out-Null
}

$collectionWithItemsAscii | Sort-Object -Property StringLength -Descending | Out-File $AsciiOriginalAnaysisPath

#ascii new 
$collectionWithItemsAscii = New-Object System.Collections.ArrayList
$stringListAscii = cmd /c "C:\Tools\Excluded\strings64.exe -a $exeNewPath" '2>&1' 
foreach ($string in $stringListAscii)
{
   
    $stringProps = New-Object System.Object
    $stringProps | Add-Member -MemberType NoteProperty -Name "StringValue" -Value $string
    $stringProps | Add-Member -MemberType NoteProperty -Name "StringLength" -Value $string.Length
    $collectionWithItemsAscii.Add($stringProps) | Out-Null

}

$collectionWithItemsAscii | Sort-Object -Property StringLength -Descending | Out-File $AsciiUglifiedAnaysisPath