del "KoikatuPlugins.zip"
echo F|xcopy "README.md" "bin\README.md" /Y
for /d %%X in (bin) do "%PROGRAMFILES%\7-Zip\7z.exe" a -tzip -mx7 "KoikatuPlugins.zip" ".\%%X\*"
