using System;
using System.Collections.Generic;

namespace ConfigManagement
{
    public class FileNode
    {
        public string Name { get; set; } = "";
        public string Path { get; set; } = "";
        public bool IsDirectory { get; set; }
        public string Content { get; set; } = "";
        public long Size { get; set; }
        public List<FileNode> Children { get; set; } = new List<FileNode>();
        public FileNode? Parent { get; set; }

        public FileNode() { }

        public FileNode(string name, string path, bool isDirectory, string content = "", long size = 0)
        {
            Name = name;
            Path = path;
            IsDirectory = isDirectory;
            Content = content;
            Size = size;
        }

        public void AddChild(FileNode child)
        {
            child.Parent = this;
            Children.Add(child);
        }

        public FileNode? FindChild(string name)
        {
            return Children.Find(node => node.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        public override string ToString()
        {
            return $"{Name} {(IsDirectory ? "/" : "")}";
        }
    }
}