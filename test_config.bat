@echo off
echo Тестирование конфигурационных параметров...
echo.

echo 1. Запуск с пользовательским приглашением:
dotnet run -- --prompt "my-vfs> " --script "Scripts/test_success.txt"

echo.
echo 2. Запуск с указанием VFS пути:
dotnet run -- --vfs-path "config.vfs.csv" --prompt "config> " --script "Scripts/test_arguments.txt"

echo.
echo 3. Запуск с отключенным дебагом:
dotnet run -- --no-debug --script "Scripts/test_success.txt"

echo.
echo 4. Запуск скрипта с ошибкой (должен прерваться):
dotnet run -- --script "Scripts/test_error.txt"

echo.
echo Тестирование завершено!
pause