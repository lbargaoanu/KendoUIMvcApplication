:: trebuie rulat din bin\debug (sau bin\release, dupa caz)
copy /Y x86\SQLite.Interop.dll "%VS120COMNTOOLS%"\..\Ide\
"%WINDIR%"\Microsoft.NET\Framework\v4.0.30319\ngen.exe Install EntityFramework.dll
"%PROGRAMFILES(X86)%\Microsoft SDKs\Windows\v8.1A\bin\NETFX 4.5.1 Tools\gacutil.exe" -i EntityFramework.dll