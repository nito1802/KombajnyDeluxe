using System;
using System.IO;

namespace WorkBoard.Helpers
{
    internal static class BoardDataPaths
    {
        public static string DirectoryPath { get; } = FindDirectory();

        private static string FindDirectory()
        {
            // A build launched from Visual Studio or dotnet run lives below the project.
            for (DirectoryInfo? directory = new DirectoryInfo(AppContext.BaseDirectory); directory != null; directory = directory.Parent)
            {
                if (File.Exists(Path.Combine(directory.FullName, "WorkBoard.csproj")))
                    return Path.Combine(directory.FullName, "WorkBoardData");
            }

            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), Consts.Consts.DataDirectoryName);
        }
    }
}
