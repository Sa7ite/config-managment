@echo off
echo Тестирование VFS функциональности...
echo.

echo 1. Минимальная VFS:
dotnet run -- --vfs-path "VFS/minimal_vfs.csv" --script "Scripts/test_vfs_basic.txt"

echo.
echo 2. Вложенная VFS (3+ уровня):
dotnet run -- --vfs-path "VFS/nested_vfs.csv" --script "Scripts/test_vfs_nested.txt"

echo.
echo 3. Комплексная VFS:
dotnet run -- --vfs-path "VFS/complex_vfs.csv" --script "Scripts/test_vfs_complex.txt"

echo.
echo 4. Обработка ошибок VFS:
dotnet run -- --vfs-path "VFS/minimal_vfs.csv" --script "Scripts/test_vfs_errors.txt"

echo.
echo Тестирование VFS завершено!
pause