using System;
using System.IO;
using Xunit;

namespace Zpp.Test
{
    public class ObjectCalisthenicsTest
    {
        [Fact]
        public void testNoElse()
        {
            // Assert.Equal("C:\\Users\\psc\\Documents\\git\\da_code\\Zpp\\bin", Directory.GetCurrentDirectory() + "");
            traverseAllCsharpFilesAndExecute((line, lineNumber, fileName) =>
            {
                Assert.False(line.Contains("else"), $"{fileName}:{lineNumber} contains an 'else'.");
            });
        }

        private void traverseAllCsharpFilesAndExecute(Action<string, int, string> actionOnOneLine)
        {
            DirectoryInfo currentDirectory = new DirectoryInfo(Directory.GetCurrentDirectory());
            foreach (string file in Directory.EnumerateFiles(
                currentDirectory.Parent.Parent.Parent.FullName, "*.*", SearchOption.AllDirectories))
            {
                FileInfo fileInfo = new FileInfo(file);
                if (fileInfo.Extension.Equals(".cs"))
                {
                    var lines = File.ReadAllLines(file);
                    for (var i = 0; i < lines.Length; i++) {
                        var line = lines[i];
                        // Process line
                        actionOnOneLine(line, i, fileInfo.Name);
                    }
                }
            }
        }
    }
}