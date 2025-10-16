using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace ConfigManagement
{
    internal static class Program
    {
        public static string VfsPath { get; private set; } = "default.vfs.csv";
        public static string CustomPrompt { get; private set; } = "$ ";
        public static string? ScriptPath { get; private set; }
        public static bool ShowDebugInfo { get; private set; } = true;

        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Парсинг аргументов командной строки
            ParseCommandLineArgs(args);

            // Отладочный вывод параметров
            if (ShowDebugInfo)
            {
                ShowDebugParameters();
            }

            Application.Run(new MainForm());
        }

        private static void ParseCommandLineArgs(string[] args)
        {
            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i].ToLower())
                {
                    case "--vfs-path":
                    case "-v":
                        if (i + 1 < args.Length) VfsPath = args[++i];
                        break;
                    case "--prompt":
                    case "-p":
                        if (i + 1 < args.Length) CustomPrompt = args[++i] + " ";
                        break;
                    case "--script":
                    case "-s":
                        if (i + 1 < args.Length) ScriptPath = args[++i];
                        break;
                    case "--no-debug":
                        ShowDebugInfo = false;
                        break;
                    case "--help":
                    case "-h":
                        ShowHelp();
                        Environment.Exit(0);
                        break;
                }
            }
        }

        private static void ShowDebugParameters()
        {
            Console.WriteLine("=== ДЕБАГ ИНФОРМАЦИЯ: Параметры запуска ===");
            Console.WriteLine($"VFS Path: {VfsPath}");
            Console.WriteLine($"Custom Prompt: {CustomPrompt}");
            Console.WriteLine($"Script Path: {ScriptPath ?? "не указан"}");
            Console.WriteLine($"Show Debug: {ShowDebugInfo}");
            Console.WriteLine("===========================================");
            Console.WriteLine();
        }

        private static void ShowHelp()
        {
            Console.WriteLine("Использование: ConfigManagement [ОПЦИИ]");
            Console.WriteLine();
            Console.WriteLine("Опции:");
            Console.WriteLine("  --vfs-path, -v ПУТЬ    Путь к файлу VFS");
            Console.WriteLine("  --prompt, -p ТЕКСТ     Пользовательское приглашение");
            Console.WriteLine("  --script, -s ПУТЬ      Путь к стартовому скрипту");
            Console.WriteLine("  --no-debug             Отключить отладочный вывод");
            Console.WriteLine("  --help, -h             Показать эту справку");
            Console.WriteLine();
            Console.WriteLine("Примеры:");
            Console.WriteLine("  ConfigManagement --vfs-path my.vfs --prompt \"my-shell> \"");
            Console.WriteLine("  ConfigManagement --script init.txt --no-debug");
        }
    }
}