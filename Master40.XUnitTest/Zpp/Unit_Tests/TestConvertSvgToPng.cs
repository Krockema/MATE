using System.Diagnostics;
using System.IO;
using Master40.DB.Data.Helper;
using Xunit;

namespace Master40.XUnitTest.Zpp.Unit_Tests
{
    public class TestConvertSvgToPng
    {
        [Fact(Skip = "?!")]
        public void TestConvertSvgToPngImages()
        {
            if (Constants.IsWindows)
            {
                
                string sourcePath = "C:\\Users\\psc\\OneDrive\\diplomarbeit\\doc\\images";
                DirectoryInfo currentDirectory = new DirectoryInfo(sourcePath);
                foreach (string file in Directory.EnumerateFiles(currentDirectory.FullName, "*.*",
                    SearchOption.AllDirectories))
                {
                    FileInfo fileInfo = new FileInfo(file);

                    if (fileInfo.Extension.Equals(".svg"))
                    {
                        string fileNameWithoutExtension =
                            Path.GetFileNameWithoutExtension(fileInfo.Name);
                        string pngFileName = $"{sourcePath}\\{fileNameWithoutExtension}.png";
                        string command = $"\"C:\\Program Files\\Inkscape\\inkscape.exe\" " +
                                         $"{sourcePath}\\{fileInfo.Name} " +
                                         $"--export-png={pngFileName}";

                        Process cmd = new Process();
                        cmd.StartInfo.FileName = "cmd.exe";
                        cmd.StartInfo.RedirectStandardInput = true;
                        cmd.StartInfo.RedirectStandardOutput = true;
                        cmd.StartInfo.CreateNoWindow = true;
                        cmd.StartInfo.UseShellExecute = false;
                        cmd.Start();

                        cmd.StandardInput.WriteLine(command);
                        cmd.StandardInput.Flush();
                        cmd.StandardInput.Close();
                        cmd.WaitForExit();
                        cmd.StandardOutput.ReadToEnd();
                        // Console.WriteLine();

                        Assert.True(new FileInfo(pngFileName).Exists,
                            $"SVG was not converted to PNG: {command}");
                    }
                }
            }
            else
            {
                // Should only be done on thesis local machine
                Assert.True(true);
            }
        }
    }
}