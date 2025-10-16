using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConfigManagement
{
    public class VirtualFileSystem
    {
        public FileNode Root { get; private set; }
        public FileNode CurrentDirectory { get; set; }

        public VirtualFileSystem()
        {
            Root = new FileNode("", "/", true);
            CurrentDirectory = Root;
        }

        public void LoadFromCsv(string csvContent)
        {
            var lines = csvContent.Split('\n');
            Root = new FileNode("", "/", true);
            CurrentDirectory = Root;

            foreach (var line in lines)
            {
                var trimmedLine = line.Trim();
                if (string.IsNullOrEmpty(trimmedLine) || trimmedLine.StartsWith("Path,"))
                    continue;

                var parts = ParseCsvLine(trimmedLine);
                if (parts.Length >= 4)
                {
                    var path = parts[0];
                    var name = parts[1];
                    var type = parts[2];
                    var content = parts.Length > 3 ? parts[3] : "";
                    var size = parts.Length > 4 ? long.Parse(parts[4]) : 0;

                    AddFileToPath(path, name, type == "Directory", content, size);
                }
            }
        }

        private string[] ParseCsvLine(string line)
        {
            var result = new List<string>();
            var inQuotes = false;
            var current = new StringBuilder();

            foreach (char c in line)
            {
                if (c == '"')
                {
                    inQuotes = !inQuotes;
                }
                else if (c == ',' && !inQuotes)
                {
                    result.Add(current.ToString());
                    current.Clear();
                }
                else
                {
                    current.Append(c);
                }
            }

            result.Add(current.ToString());
            return result.ToArray();
        }

        private void AddFileToPath(string fullPath, string name, bool isDirectory, string content, long size)
        {
            var pathParts = fullPath.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            var current = Root;

            foreach (var part in pathParts)
            {
                var next = current.FindChild(part);
                if (next == null)
                {
                    next = new FileNode(part, GetPath(current, part), true);
                    current.AddChild(next);
                }
                current = next;
            }

            var existingNode = current.FindChild(name);
            if (existingNode == null)
            {
                var newNode = new FileNode(name, fullPath.TrimEnd('/') + "/" + name, isDirectory, content, size);
                current.AddChild(newNode);
            }
        }

        private string GetPath(FileNode parent, string name)
        {
            if (parent.Path == "/") 
                return $"/{name}";
            else 
                return $"{parent.Path}/{name}";
        }

        public FileNode? GetNodeByPath(string path)
        {
            if (path == "/") return Root;

            var pathParts = path.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            var current = path.StartsWith("/") ? Root : CurrentDirectory;

            foreach (var part in pathParts)
            {
                if (part == "..")
                {
                    current = current?.Parent ?? Root;
                }
                else if (part == ".")
                {
                    // remain in current directory
                }
                else
                {
                    current = current?.FindChild(part);
                    if (current == null) return null;
                }
            }

            return current;
        }

        public List<FileNode> GetDirectoryContents(string path = "")
        {
            var targetDir = string.IsNullOrEmpty(path) ? CurrentDirectory : GetNodeByPath(path);
            return targetDir?.Children ?? new List<FileNode>();
        }

        public bool ChangeDirectory(string path)
        {
            var target = GetNodeByPath(path);
            if (target != null && target.IsDirectory)
            {
                CurrentDirectory = target;
                return true;
            }
            return false;
        }

        public string GetCurrentPath()
        {
            return CurrentDirectory.Path;
        }

        public string ReadFileContent(string path)
        {
            var file = GetNodeByPath(path);
            if (file != null && !file.IsDirectory)
            {
                try
                {
                    if (!string.IsNullOrEmpty(file.Content))
                    {
                        var bytes = Convert.FromBase64String(file.Content);
                        return Encoding.UTF8.GetString(bytes);
                    }
                    return file.Content;
                }
                catch
                {
                    return file.Content;
                }
            }
            return "";
        }
    }
}