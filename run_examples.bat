@echo off
echo Примеры запуска эмулятора с разными параметрами...
echo.

echo 1. Базовый запуск:
dotnet run

echo.
echo 2. С пользовательским приглашением:
dotnet run -- --prompt "custom> "

echo.
echo 3. Со скриптом:
dotnet run -- --script "Scripts/test_success.txt"

echo.
echo 4. Со всеми параметрами:
dotnet run -- --vfs-path "my.vfs.csv" --prompt "shell> " --script "Scripts/test_arguments.txt"

echo.
pause