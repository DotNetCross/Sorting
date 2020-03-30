$oldName = "Sorts."
$newName = ""
Get-ChildItem -Filter "*$oldName*" -Recurse | Rename-Item -NewName {$_.name -replace $oldName, $newName }
Get-ChildItem -Recurse -Include "*.sln","*.cs","*.xaml","*.xml","*.csproj","*.xproj","*.json","*.md","*.cmd","*.props","*.txt" |
 ForEach-Object { $a = $_.fullname; ( [System.IO.File]::ReadAllText($a) ) | % {
	 If ($_.Contains($oldName))
	 {
		 #$newContent = $_.Replace($oldName, $newName)
		 $newContent = $_
		 [System.IO.File]::WriteAllText($a, $newContent)
		 "Changed: " + $a
	 }
	}
}