del "KoikatuPlugins.zip"
echo F|xcopy "README.md" "bin\Release\README.md" /Y
for /d %%X in (bin\Release) do "%PROGRAMFILES%\7-Zip\7z.exe" a -tzip -mx7 "KoikatuPlugins.zip" ".\%%X\*"
