using System;
using System.IO;
using System.Text.RegularExpressions;
using Xunit;
using Zpp.Test.WrappersForPrimitives;

namespace Zpp.Test
{
    public class ObjectCalisthenicsTest
    {
        [Fact]
        /**
         * Rule no. 2: Don't use else-keyword
         */
        public void testNoElse()
        {
            traverseAllCsharpFilesAndExecute((line, lineNumber, fileName) =>
            {
                Assert.False(line.GetLine().Contains("else"),
                    $"{fileName}:{lineNumber} contains an 'else'.");
            });
        }

        [Fact]
        /**
         * Rule no. 4: A line should not have more than two "." (access operator)
         */
        public void testNoMoreThanTwoPoints()
        {
            string[] exceptions =
            {
                "NLog"
            };

            var regex = new Regex("(?!using)(^.*\\..*\\..*\\..*$)");

            traverseAllCsharpFilesAndExecute((line, lineNumber, fileName) =>
            {
                // skip if line contains a string mention in exceptions
                foreach (var exception in exceptions)
                {
                    if (line.GetLine().Contains(exception))
                    {
                        return;
                    }
                }

                Assert.False(regex.IsMatch(line.GetLine()),
                    $"{fileName}:{lineNumber} contains more than two '.' (Access-Operators).");
            });
        }

        [Fact]
        /**
         * Rule no. 6: Keep Entites small (not more than 50 lines, 5 classes per package
         * --> with tolerance 100 lines, 10 classes per package)
         */
        public void testKeepEntitiesSmall()
        {
            int maxLines = 100;
            int maxClassesPerPackage = 10;
            
            traverseAllCsharpFilesAndExecute((lines, fileName) =>
            {
                Assert.False(lines.Count() > maxLines,
                    $"{fileName}: has more than {maxLines} lines.");
            });
        }

        private void traverseAllCsharpFilesAndExecute(Action<Lines, FileName> actionOnFile)
        {
            DirectoryInfo currentDirectory = new DirectoryInfo(Directory.GetCurrentDirectory());
            foreach (string file in Directory.EnumerateFiles(
                currentDirectory.Parent.Parent.Parent.FullName, "*.*", SearchOption.AllDirectories))
            {
                FileInfo fileInfo = new FileInfo(file);
                if (fileInfo.Extension.Equals(".cs"))
                {
                    var lines = File.ReadAllLines(file);
                    actionOnFile(new Lines(lines), new FileName(fileInfo.Name));
                }
            }
        }

        private void traverseAllCsharpFilesAndExecute(Action<Line, LineNumber, FileName> actionOnOneLine)
        {
            traverseAllCsharpFilesAndExecute((lines, fileName) =>
            {
                for (var i = 0; i < lines.GetLines().Count; i++)
                {
                    var currentLineNumber = new LineNumber(i);
                    var line = lines.GetLine(currentLineNumber);
                    // Process line
                    actionOnOneLine(line, currentLineNumber, fileName);
                }
            });
        }
    }
}