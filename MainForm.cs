using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace ConfigManagement
{
    public partial class MainForm : Form
    {
        private TextBox inputTextBox;
        private RichTextBox outputRichTextBox;
        private VirtualFileSystem vfs;
        private const string VFS_NAME = "ConfigVFS";
        private bool isExecutingScript = false;
        private string[] currentScriptLines;
        private int currentScriptLine = 0;

        public MainForm()
        {
            InitializeComponent();
            this.Text = $"{VFS_NAME} - Configuration Management System";
            
            // Initialize VFS
            vfs = Program.VFS ?? new VirtualFileSystem();
            
            ShowWelcomeMessage();
            
            // Запуск стартового скрипта если указан
            if (!string.IsNullOrEmpty(Program.ScriptPath))
            {
                ExecuteStartupScript();
            }
        }

        private void InitializeComponent()
        {
            // Настройка формы
            this.SuspendLayout();
            this.ClientSize = new Size(800, 450);
            this.MinimumSize = new Size(600, 300);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;

            // Панель вывода
            outputRichTextBox = new RichTextBox();
            outputRichTextBox.Location = new Point(12, 12);
            outputRichTextBox.Size = new Size(776, 350);
            outputRichTextBox.BackColor = Color.Black;
            outputRichTextBox.ForeColor = Color.LightGreen;
            outputRichTextBox.Font = new Font("Consolas", 10);
            outputRichTextBox.ReadOnly = true;
            outputRichTextBox.ScrollBars = RichTextBoxScrollBars.Vertical;
            outputRichTextBox.BorderStyle = BorderStyle.FixedSingle;

            // Поле ввода
            inputTextBox = new TextBox();
            inputTextBox.Location = new Point(12, 375);
            inputTextBox.Size = new Size(776, 30);
            inputTextBox.Font = new Font("Consolas", 10);
            inputTextBox.BackColor = Color.Black;
            inputTextBox.ForeColor = Color.White;
            inputTextBox.BorderStyle = BorderStyle.FixedSingle;
            inputTextBox.KeyPress += InputTextBox_KeyPress;

            // Добавление элементов на форму
            this.Controls.Add(outputRichTextBox);
            this.Controls.Add(inputTextBox);

            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private void InputTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                ProcessCommand(inputTextBox.Text.Trim());
                inputTextBox.Clear();
                e.Handled = true;
            }
        }

        private void ExecuteStartupScript()
        {
            try
            {
                if (File.Exists(Program.ScriptPath))
                {
                    AppendOutput($"=== Запуск стартового скрипта: {Program.ScriptPath} ===", Color.Magenta);
                    
                    var scriptContent = File.ReadAllLines(Program.ScriptPath);
                    currentScriptLines = scriptContent;
                    currentScriptLine = 0;
                    isExecutingScript = true;
                    
                    // Запускаем выполнение первой команды
                    ExecuteNextScriptCommand();
                }
                else
                {
                    AppendOutput($"ОШИБКА: Скрипт не найден: {Program.ScriptPath}", Color.Red);
                }
            }
            catch (Exception ex)
            {
                AppendOutput($"ОШИБКА загрузки скрипта: {ex.Message}", Color.Red);
            }
        }

        private void ExecuteNextScriptCommand()
        {
            if (!isExecutingScript || currentScriptLines == null) return;

            while (currentScriptLine < currentScriptLines.Length)
            {
                var line = currentScriptLines[currentScriptLine++].Trim();
                
                // Пропускаем пустые строки и комментарии
                if (string.IsNullOrEmpty(line) || line.StartsWith("#"))
                    continue;

                AppendOutput($"[СКРИПТ] Выполнение: {line}", Color.Gray);
                ProcessCommand(line);
                
                // Прерываем выполнение при ошибке
                if (outputRichTextBox.Text.Contains("ОШИБКА") || 
                    outputRichTextBox.Text.Contains("не найдена") ||
                    outputRichTextBox.Text.Contains("не найден"))
                {
                    AppendOutput($"=== ВЫПОЛНЕНИЕ СКРИПТА ПРЕРВАНО НА СТРОКЕ {currentScriptLine} ===", Color.Red);
                    isExecutingScript = false;
                    break;
                }

                // Даем время для отображения
                Application.DoEvents();
                System.Threading.Thread.Sleep(500);
                
                break; // Выполняем по одной команде за раз
            }

            // Если скрипт завершен
            if (currentScriptLine >= currentScriptLines.Length && isExecutingScript)
            {
                AppendOutput("=== ВЫПОЛНЕНИЕ СКРИПТА ЗАВЕРШЕНО УСПЕШНО ===", Color.Green);
                isExecutingScript = false;
            }
        }

        private void ProcessCommand(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                AppendOutput("", Color.White);
                return;
            }

            // Вывод введенной команды с пользовательским приглашением
            string prompt = !string.IsNullOrEmpty(Program.CustomPrompt) ? 
                Program.CustomPrompt : $"{vfs.GetCurrentPath()}$ ";
            AppendOutput($"{prompt}{input}", Color.Yellow);

            try
            {
                string[] parts = input.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                
                if (parts.Length == 0)
                    return;

                string command = parts[0].ToLower();
                string[] args = new string[parts.Length - 1];
                Array.Copy(parts, 1, args, 0, args.Length);

                switch (command)
                {
                    case "ls":
                        ExecuteLs(args);
                        break;
                    case "cd":
                        ExecuteCd(args);
                        break;
                    case "cat":
                        ExecuteCat(args);
                        break;
                    case "exit":
                        ExecuteExit(args);
                        break;
                    default:
                        AppendOutput($"Команда '{command}' не найдена", Color.Red);
                        // Прерываем скрипт при ошибке
                        if (isExecutingScript)
                        {
                            throw new Exception($"Неизвестная команда: {command}");
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                AppendOutput($"ОШИБКА выполнения: {ex.Message}", Color.Red);
                
                // Прерываем выполнение скрипта при ошибке
                if (isExecutingScript)
                {
                    isExecutingScript = false;
                    AppendOutput("=== ВЫПОЛНЕНИЕ СКРИПТА ПРЕРВАНО ИЗ-ЗА ОШИБКИ ===", Color.Red);
                }
            }

            AppendOutput("", Color.White);

            // Если выполняется скрипт, запускаем следующую команду
            if (isExecutingScript)
            {
                // Используем таймер для задержки между командами
                var timer = new System.Windows.Forms.Timer();
                timer.Interval = 1000;
                timer.Tick += (s, e) =>
                {
                    timer.Stop();
                    timer.Dispose();
                    ExecuteNextScriptCommand();
                };
                timer.Start();
            }
        }

        private void ExecuteLs(string[] args)
        {
            AppendOutput($"Команда: ls", Color.Cyan);
            AppendOutput($"Аргументы: {(args.Length > 0 ? string.Join(", ", args) : "нет")}", Color.Cyan);
            
            var path = args.Length > 0 ? args[0] : "";
            var contents = vfs.GetDirectoryContents(path);
            
            if (contents.Count == 0)
            {
                AppendOutput("Директория пуста", Color.White);
            }
            else
            {
                foreach (var item in contents)
                {
                    var display = item.IsDirectory ? $"{item.Name}/" : item.Name;
                    AppendOutput(display, Color.White);
                }
            }
            
            AppendOutput($"Всего: {contents.Count} элементов", Color.Gray);
        }

        private void ExecuteCd(string[] args)
        {
            AppendOutput($"Команда: cd", Color.Cyan);
            AppendOutput($"Аргументы: {(args.Length > 0 ? string.Join(", ", args) : "нет")}", Color.Cyan);
            
            if (args.Length == 0)
            {
                vfs.ChangeDirectory("/home/user");
                AppendOutput("Переход в домашнюю директорию", Color.Green);
            }
            else if (args.Length == 1)
            {
                if (vfs.ChangeDirectory(args[0]))
                {
                    AppendOutput($"Переход в: {args[0]}", Color.Green);
                }
                else
                {
                    AppendOutput($"cd: {args[0]}: Директория не найдена", Color.Red);
                    if (isExecutingScript)
                    {
                        throw new Exception($"Директория не найдена: {args[0]}");
                    }
                }
            }
            else
            {
                AppendOutput("cd: слишком много аргументов. Использование: cd [директория]", Color.Red);
            }
            
            AppendOutput($"Текущая директория: {vfs.GetCurrentPath()}", Color.LightBlue);
        }

        private void ExecuteCat(string[] args)
        {
            AppendOutput($"Команда: cat", Color.Cyan);
            AppendOutput($"Аргументы: {(args.Length > 0 ? string.Join(", ", args) : "нет")}", Color.Cyan);
            
            if (args.Length == 0)
            {
                AppendOutput("cat: не указан файл", Color.Red);
                return;
            }
            
            var content = vfs.ReadFileContent(args[0]);
            if (!string.IsNullOrEmpty(content))
            {
                AppendOutput($"Содержимое файла {args[0]}:", Color.White);
                AppendOutput(content, Color.White);
            }
            else
            {
                AppendOutput($"cat: {args[0]}: Файл не найден или пуст", Color.Red);
            }
        }

        private void ExecuteExit(string[] args)
        {
            AppendOutput($"Команда: exit", Color.Cyan);
            AppendOutput($"Аргументы: {(args.Length > 0 ? string.Join(", ", args) : "нет")}", Color.Cyan);
            
            if (args.Length > 0)
            {
                AppendOutput("Предупреждение: аргументы команды exit игнорируются", Color.Orange);
            }
            
            AppendOutput("Завершение работы системы управления конфигурациями...", Color.Yellow);
            Application.Exit();
        }

        private void AppendOutput(string text, Color color)
        {
            if (outputRichTextBox.InvokeRequired)
            {
                outputRichTextBox.Invoke(new Action<string, Color>(AppendOutput), text, color);
            }
            else
            {
                outputRichTextBox.SelectionStart = outputRichTextBox.TextLength;
                outputRichTextBox.SelectionColor = color;
                outputRichTextBox.AppendText(text + Environment.NewLine);
                outputRichTextBox.ScrollToCaret();
            }
        }

        private void ShowWelcomeMessage()
        {
            AppendOutput($"Добро пожаловать в {VFS_NAME} - система управления конфигурациями!", Color.LightBlue);
            AppendOutput("Версия: 1.0 (Этап 3: VFS)", Color.LightBlue);
            
            if (!string.IsNullOrEmpty(Program.ScriptPath))
            {
                AppendOutput($"Стартовый скрипт: {Program.ScriptPath}", Color.Cyan);
            }
            if (!string.IsNullOrEmpty(Program.VfsPath) && Program.VfsPath != "default.vfs.csv")
            {
                AppendOutput($"VFS файл: {Program.VfsPath}", Color.Cyan);
            }
            
            AppendOutput("", Color.White);
            AppendOutput("Доступные команды: ls, cd, cat, exit", Color.White);
            AppendOutput("", Color.White);
        }
    }
}