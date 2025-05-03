using System.ComponentModel;
using System.Diagnostics;
using Microsoft.Win32;

namespace RegistryChange
{
    public partial class Form1 : Form
    {
        RegistryKey currentKey = Registry.CurrentUser;
        Stack<RegistryKey> keyStack = new Stack<RegistryKey>();

        private static readonly string LogFilePath = "registry_editor.log";
        private static readonly string BackupFolder = "RegistryBackups";
        public Form1()
        {

            InitializeComponent();
            InitializeBackupFolder();
            InitializeLogging();

            label1.Text = ("\nТекущий раздел: " + currentKey.Name);
            ListSubKeys(currentKey);
            ListValues(currentKey);
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void ListSubKeys(RegistryKey key)
        {
            string[] subKeyNames = key.GetSubKeyNames();
            Console.WriteLine("\nПодразделы:");
            listBox1.Items.Clear();
            if (subKeyNames.Length == 0)
            {
                listBox1.Items.Add("Подразделы отсутствуют.");
                return;
            }

            foreach (string name in subKeyNames)
            {
                listBox1.Items.Add(name);
            }
        }

        private void ListValues(RegistryKey key)
        {
            string[] valueNames = key.GetValueNames();
            listBox2.Items.Clear();
            if (valueNames.Length == 0)
            {
                listBox2.Items.Add("Значения отсутствуют.");
                return;
            }

            foreach (string name in valueNames)
            {
                object value = key.GetValue(name);
                listBox2.Items.Add($"{name} = {value} ({value.GetType().Name})");
            }
        }

        private RegistryKey NavigateToSubKey(RegistryKey currentKey)
        {
            string subKeyName = listBox1.SelectedItem.ToString();

            RegistryKey subKey = currentKey.OpenSubKey(subKeyName, true);
            if (subKey == null)
            {
                listBox1.Items.Add("123");
                return currentKey;
            }

            return subKey;
        }

        private void CreateSubKey(RegistryKey currentKey)
        {

            string subKeyName = textBox2.Text.ToString();
            currentKey.CreateSubKey(subKeyName);
            ListSubKeys(currentKey);
        }

        private void DeleteSubKey(RegistryKey currentKey)
        {
            string subKeyName = listBox1.SelectedItem.ToString();

            currentKey.DeleteSubKey(subKeyName);
            ListSubKeys(currentKey);
        }

        private void DeleteValue(RegistryKey currentKey)
        {
            string valueName = listBox2.SelectedItem.ToString().Split(" ")[0];

            currentKey.DeleteValue(valueName);
            ListValues(currentKey);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            keyStack.Push(currentKey);
            currentKey = NavigateToSubKey(currentKey);
            label1.Text = ("\nТекущий раздел: " + currentKey.Name);
            ListSubKeys(currentKey);
            ListValues(currentKey);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (keyStack.Count > 0)
                currentKey = keyStack.Pop();

            label1.Text = ("\nТекущий раздел: " + currentKey.Name);
            ListSubKeys(currentKey);
            ListValues(currentKey);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            string valueName = listBox2.SelectedItem.ToString().Split(" ")[0];
            string value = textBox1.Text.ToString();

            currentKey.SetValue(valueName, value, RegistryValueKind.String);
            ListValues(currentKey);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            CreateSubKey(currentKey);
            ListSubKeys(currentKey);
        }

        private void button5_Click(object sender, EventArgs e)
        {
            DeleteSubKey(currentKey);
            ListSubKeys(currentKey);
        }

        private void button6_Click(object sender, EventArgs e)
        {
            DeleteValue(currentKey);
            ListValues(currentKey);
        }

        private void button7_Click(object sender, EventArgs e)
        {
            string valueName = textBox3.Text.ToString();
            string value = textBox4.Text.ToString();

            currentKey.SetValue(valueName, value, RegistryValueKind.String);
            ListValues(currentKey);

        }

        static void InitializeLogging()
        {
            if (!File.Exists(LogFilePath))
            {
                File.WriteAllText(LogFilePath, $"Лог редактора реестра - {DateTime.Now}\n\n");
            }
        }

        static void InitializeBackupFolder()
        {
            if (!Directory.Exists(BackupFolder))
            {
                Directory.CreateDirectory(BackupFolder);
            }
        }

        static void LogOperation(string message, bool isError = false)
        {
            string logEntry = $"[{DateTime.Now}] {(isError ? "ОШИБКА: " : "")}{message}\n";
            File.AppendAllText(LogFilePath, logEntry);
        }

        static void CreateBackup(RegistryKey key)
        {
            string backupFileName = $"{BackupFolder}\\backup_{DateTime.Now:yyyyMMdd_HHmmss}_{Guid.NewGuid().ToString().Substring(0, 8)}.reg";

            using (StreamWriter sw = new StreamWriter(backupFileName))
            {
                sw.WriteLine("Windows Registry Editor Version 5.00");
                sw.WriteLine();
                sw.WriteLine($"[{key.Name}]");

                foreach (string valueName in key.GetValueNames())
                {
                    object value = key.GetValue(valueName);
                    RegistryValueKind kind = key.GetValueKind(valueName);

                    string valueStr = value.ToString();
                    if (kind == RegistryValueKind.String || kind == RegistryValueKind.ExpandString)
                    {
                        valueStr = $"\"{valueStr}\"";
                    }
                    else if (kind == RegistryValueKind.DWord)
                    {
                        valueStr = $"dword:{((int)value).ToString("X8")}";
                    }
                    else if (kind == RegistryValueKind.QWord)
                    {
                        valueStr = $"hex(b):{BitConverter.ToString(BitConverter.GetBytes((long)value)).Replace("-", "")}";
                    }
                    else if (kind == RegistryValueKind.Binary)
                    {
                        byte[] bytes = (byte[])value;
                        valueStr = $"hex:{BitConverter.ToString(bytes).Replace("-", "")}";
                    }
                    else if (kind == RegistryValueKind.MultiString)
                    {
                        string[] strings = (string[])value;
                        valueStr = $"hex(7):{string.Join(",", strings.Select(s => BitConverter.ToString(System.Text.Encoding.Unicode.GetBytes(s + "\0")).Replace("-", "")))}";
                    }

                    string name = valueName == "" ? "@" : $"\"{valueName}\"";
                    sw.WriteLine($"{name}={valueStr}");
                }
            }

            Console.WriteLine($"Бэкап создан: {backupFileName}");
            LogOperation($"Создан бэкап раздела {key.Name} в файл {backupFileName}");
        }

        private void ShowBackups(RegistryKey currentKey, Stack<RegistryKey> keyStack)
        {
            var backupFiles = Directory.GetFiles(BackupFolder, "*.reg")
                                     .Select(f => new FileInfo(f))
                                     .OrderByDescending(f => f.LastWriteTime)
                                     .ToList();
            listBox3.Items.Clear();
            if (backupFiles.Count == 0)
            {
                listBox3.Items.Add("Бэкапы не найдены.");
            }

            for (int i = 0; i < backupFiles.Count; i++)
            {
                listBox3.Items.Add($"{i + 1}. {backupFiles[i].Name} ({backupFiles[i].LastWriteTime})");
            }
        }
        private RegistryKey RestoreFromBackup(RegistryKey currentKey, Stack<RegistryKey> keyStack)
        {
            var backupFiles = Directory.GetFiles(BackupFolder, "*.reg")
                     .Select(f => new FileInfo(f))
                     .OrderByDescending(f => f.LastWriteTime)
                     .ToList();

            if (int.TryParse(listBox3.SelectedItem.ToString().Split(". ")[0], out int choice) && choice > 0 && choice <= backupFiles.Count)
            {
                string backupPath = backupFiles[choice - 1].FullName;

                try
                {
                    var processInfo = new ProcessStartInfo("regedit.exe", "/s \"" + backupPath + "\"")
                    {
                        Verb = "runas",
                        UseShellExecute = true
                    };
                    Process.Start(processInfo).WaitForExit();

                    LogOperation($"Восстановлен раздел из бэкапа {backupPath}");

                    string keyPath = GetKeyPathFromBackup(backupPath);
                    return OpenKeyFromPath(keyPath, keyStack);
                }
                catch (Win32Exception ex)
                {
                    MessageBox.Show("Operation was cancelled by user or requires elevation: " + ex.Message);
                    return currentKey;
                }
            }
            else
            {
                Console.WriteLine("Неверный выбор.");
                return currentKey;
            }
        }

        static string GetKeyPathFromBackup(string backupPath)
        {
            string firstLine = File.ReadLines(backupPath).FirstOrDefault(line => line.StartsWith("["));
            if (firstLine != null)
            {
                return firstLine.Trim('[', ']');
            }
            return "HKEY_CURRENT_USER";
        }

        static RegistryKey OpenKeyFromPath(string path, Stack<RegistryKey> keyStack)
        {
            string[] parts = path.Split('\\');
            RegistryKey baseKey = GetBaseKey(parts[0]);

            keyStack.Clear();
            RegistryKey currentKey = baseKey;

            for (int i = 1; i < parts.Length; i++)
            {
                keyStack.Push(currentKey);
                currentKey = currentKey.OpenSubKey(parts[i], true);
                if (currentKey == null)
                {
                    Console.WriteLine($"Не удалось открыть раздел {parts[i]}");
                    return keyStack.Count > 0 ? keyStack.Pop() : baseKey;
                }
            }

            return currentKey;
        }

        static RegistryKey GetBaseKey(string rootName)
        {
            switch (rootName.ToUpper())
            {
                case "HKEY_CLASSES_ROOT": return Registry.ClassesRoot;
                case "HKEY_CURRENT_USER": return Registry.CurrentUser;
                case "HKEY_LOCAL_MACHINE": return Registry.LocalMachine;
                case "HKEY_USERS": return Registry.Users;
                case "HKEY_CURRENT_CONFIG": return Registry.CurrentConfig;
                default: return Registry.CurrentUser;
            }
        }

        private void button8_Click(object sender, EventArgs e)
        {
            ShowBackups(currentKey, keyStack);
        }

        private void button9_Click(object sender, EventArgs e)
        {
            CreateBackup(currentKey);
        }

        private void button10_Click(object sender, EventArgs e)
        {
            RestoreFromBackup(currentKey, keyStack);
            label3.Text = listBox3.SelectedItem.ToString().Split(". ")[0];
        }
    }
}
