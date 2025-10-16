using System;
using System.IO;
using System.Text;

namespace ConfigManagement
{
    public static class VFSParser
    {
        public static VirtualFileSystem LoadFromFile(string filePath)
        {
            var vfs = new VirtualFileSystem();
            
            if (File.Exists(filePath))
            {
                var csvContent = File.ReadAllText(filePath);
                vfs.LoadFromCsv(csvContent);
            }
            else
            {
                CreateDefaultVFS(vfs);
            }

            return vfs;
        }

        private static void CreateDefaultVFS(VirtualFileSystem vfs)
        {
            var home = new FileNode("home", "/home", true);
            var user = new FileNode("user", "/home/user", true);
            
            var file1 = new FileNode("readme.txt", "/home/user/readme.txt", false, "VGhpcyBpcyBhIGRlZmF1bHQgZmlsZQ==", 25);
            var scripts = new FileNode("scripts", "/home/user/scripts", true);
            var script1 = new FileNode("start.sh", "/home/user/scripts/start.sh", false, "IyEvYmluL2Jhc2gKZWNobyAiSGVsbG8gVkZTIg==", 28);

            vfs.Root.AddChild(home);
            home.AddChild(user);
            user.AddChild(file1);
            user.AddChild(scripts);
            scripts.AddChild(script1);
        }
    }
}