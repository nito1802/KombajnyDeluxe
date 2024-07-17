using Common;
using System;
using System.Diagnostics;

namespace HotKeys_Shortcuts.Helpers
{
    public static class LanguageTeacherOcrProcessRunner
    {
        public static void RunImageUploader(string imagePath)
        {
            var processStartInfo = new ProcessStartInfo
            {
                FileName = AppsPath.LanguagesTeacherOcrPath,
                Arguments = $"\"{imagePath}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (var process = new Process { StartInfo = processStartInfo })
            {
                process.OutputDataReceived += (sender, args) => Console.WriteLine(args.Data);
                process.ErrorDataReceived += (sender, args) => Console.WriteLine(args.Data);

                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                process.WaitForExit();
            }
        }
    }
}