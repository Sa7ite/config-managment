using System;
using System.Drawing;
using System.Windows.Forms;

namespace ConfigManagement
{
    public partial class MainForm : Form
    {
        private TextBox inputTextBox;
        private RichTextBox outputRichTextBox;
        private string currentDirectory = "/home/user";
        private const string VFS_NAME = "ConfigVFS";

        public MainForm()
        {
            InitializeComponent();
            this.Text = $"{VFS_NAME} - Configuration Management System";
            ShowWelcomeMessage();
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

        private void ProcessCommand(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                AppendOutput("", Color.White);
                return;
            }

            // Вывод введенной команды
            AppendOutput($"{currentDirectory}$ {input}", Color.Yellow);

            try
            {
                // Простой парсер - разделение по пробелам
                string[] parts = input.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                
                if (parts.Length == 0)
                    return;

                string command = parts[0].ToLower();
                string[] args = new string[parts.Length - 1];
                Array.Copy(parts, 1, args, 0, args.Length);

                // Обработка команд
                switch (command)
                {
                    case "ls":
                        ExecuteLs(args);
                        break;
                    case "cd":
                        ExecuteCd(args);
                        break;
                    case "exit":
                        ExecuteExit(args);
                        break;
                    default:
                        AppendOutput($"Команда '{command}' не найдена. Доступные команды: ls, cd, exit", Color.Red);
                        break;
                }
            }
            catch (Exception ex)
            {
                AppendOutput($"Ошибка выполнения: {ex.Message}", Color.Red);
            }

            // Пустая строка для разделения
            AppendOutput("", Color.White);
        }

        private void ExecuteLs(string[] args)
        {
            // Заглушка для команды ls
            AppendOutput($"Команда: ls", Color.Cyan);
            AppendOutput($"Аргументы: {(args.Length > 0 ? string.Join(", ", args) : "нет")}", Color.Cyan);
            AppendOutput("config.json  settings.yaml  scripts/  logs/", Color.White);
            AppendOutput("Всего: 4 элемента", Color.Gray);
        }

        private void ExecuteCd(string[] args)
        {
            // Заглушка для команды cd
            AppendOutput($"Команда: cd", Color.Cyan);
            AppendOutput($"Аргументы: {(args.Length > 0 ? string.Join(", ", args) : "нет")}", Color.Cyan);
            
            if (args.Length == 0)
            {
                currentDirectory = "/home/user";
                AppendOutput("Переход в домашнюю директорию", Color.Green);
            }
            else if (args.Length == 1)
            {
                if (args[0] == "..")
                {
                    if (currentDirectory != "/")
                    {
                        int lastSlash = currentDirectory.LastIndexOf('/');
                        currentDirectory = currentDirectory.Substring(0, lastSlash);
                        if (string.IsNullOrEmpty(currentDirectory)) currentDirectory = "/";
                    }
                    AppendOutput($"Переход в родительскую директорию", Color.Green);
                }
                else if (args[0] == "/")
                {
                    currentDirectory = "/";
                    AppendOutput("Переход в корневую директорию", Color.Green);
                }
                else
                {
                    string newDir = args[0].StartsWith("/") ? args[0] : $"{currentDirectory}/{args[0]}";
                    AppendOutput($"Попытка перехода в: {newDir}", Color.Green);
                    currentDirectory = newDir;
                }
            }
            else
            {
                AppendOutput("cd: слишком много аргументов. Использование: cd [директория]", Color.Red);
            }
            
            AppendOutput($"Текущая директория: {currentDirectory}", Color.LightBlue);
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
            AppendOutput("Версия: 1.0 (Этап 1: REPL)", Color.LightBlue);
            AppendOutput("", Color.White);
            AppendOutput("Доступные команды:", Color.White);
            AppendOutput("  ls [аргументы]    - вывод содержимого директории", Color.LightGreen);
            AppendOutput("  cd [директория]   - смена текущей директории", Color.LightGreen);
            AppendOutput("  exit              - завершение работы", Color.LightGreen);
            AppendOutput("", Color.White);
            AppendOutput("Примеры использования:", Color.Gray);
            AppendOutput("  ls -l             # вывод с аргументами", Color.Gray);
            AppendOutput("  cd scripts        # переход в папку scripts", Color.Gray);
            AppendOutput("  cd ..             # переход на уровень выше", Color.Gray);
            AppendOutput("", Color.White);
        }
    }
}