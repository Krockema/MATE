using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Zpp.Test.WrappersForPrimitives;
using Xunit;

namespace Zpp.Test
{
    /**
     * Tests testable rules that ensures a good structure e.g. ObjectCalisthenics
     */
    public class StructureTest
    {
        const string
            skipThisTestClass =
                "ObjectCalisthenicsTest is currenctly disabled."; // TODO: change to null to enable this class

        [Fact]
        public void testInterfacesUseInterfacesAsParametersAndResturns()
        {
            
            traverseAllCsharpFilesAndExecute((line, lineNumber, fileName) =>
            {
                String[] fileIgnoreList = new String[]
                {
                    ""
                };
                if (fileName.GetValue().StartsWith("I") && !fileIgnoreList.Contains(fileName.GetValue()))
                {
                    var regexReturnType = new Regex("(^.+[A-Z]+[0-9]*\\($)");
                    Match match = regexReturnType.Match(line.GetValue());
    
                        Assert.True(match.Value.StartsWith("I"),
                            $"{fileName}:{lineNumber} return type is not an interface.");
                }
                // TODO assert parameters are also interfaces
                
            });
        }
        
        [Fact(Skip = skipThisTestClass)]
        /**
         * Rule no. 2: Don't use else-keyword
         */
        public void testNoElse()
        {
            traverseAllCsharpFilesAndExecute((line, lineNumber, fileName) =>
            {
                Assert.False(line.GetValue().Contains("else"),
                    $"{fileName}:{lineNumber} contains an 'else'.");
            });
        }

        [Fact(Skip = skipThisTestClass)]
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
                    if (line.GetValue().Contains(exception))
                    {
                        return;
                    }
                }

                Assert.False(regex.IsMatch(line.GetValue()),
                    $"{fileName}:{lineNumber} contains more than two '.' (Access-Operators).");
            });
        }

        [Fact(Skip = skipThisTestClass)]
        /**
         * Rule no. 6: Keep Entites small (not more than x lines, x classes per package
         * --> with some tolerance)
         */
        public void testKeepEntitiesSmall()
        {
            int maxLines = 100;
            int maxClassesPerPackage = 7;

            // check no more than maxClassesPerPackage
            traverseAllCsharpDirectoriesAndExecute((directoryInfo) =>
            {
                int fileCountInDirectory = 0;
                foreach (var file in directoryInfo.GetFiles())
                {
                    if (file.Extension.Equals(".cs"))
                    {
                        fileCountInDirectory++;
                    }
                }


                Assert.False(fileCountInDirectory > maxClassesPerPackage,
                    $"{directoryInfo.Name}: has more than {maxClassesPerPackage} classes.");
            });

            // check maxLines in every csharpFile
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

        private void traverseAllCsharpFilesAndExecute(
            Action<Line, LineNumber, FileName> actionOnOneLine)
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

        private void traverseAllCsharpDirectoriesAndExecute(Action<DirectoryInfo> actionOnDirectory)
        {
            DirectoryInfo currentDirectory = new DirectoryInfo(Directory.GetCurrentDirectory());
            foreach (string directory in Directory.EnumerateDirectories(
                currentDirectory.Parent.Parent.Parent.FullName, "*.*", SearchOption.AllDirectories))
            {
                DirectoryInfo directoryInfo = new DirectoryInfo(directory);
                actionOnDirectory(directoryInfo);
            }
        }
    }
}