set folder=Minecraft Adaptable Studio

powershell -Command "Get-ChildItem -Path %~dp0\%folder%\bin\Release -Exclude logs |  Compress-Archive -CompressionLevel Fastest -DestinationPath %~dp0\%folder%.zip -Force"
