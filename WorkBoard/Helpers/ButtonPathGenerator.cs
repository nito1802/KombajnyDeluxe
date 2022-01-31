using Common;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KombajnDoPracy.Helpers
{
    public static class ButtonPathGenerator
    {
        public static string GetMyDataPath()
        {
            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

            string result = KombajnCommon.MojeDanePath;

            if (!Directory.Exists(result)) Directory.CreateDirectory(result);

            return result;
        }

        internal static string GetAllDataPath()
        {
            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

            string result = KombajnCommon.AllDanePath;

            if (!Directory.Exists(result)) Directory.CreateDirectory(result);

            return result;
        }

        public static string GetNotesPath()
        {
            var polishFormat = new CultureInfo("pl-PL");
            string currentYear = DateTime.Now.Year.ToString();
            string currentMonth = DateTime.Now.ToString("MMMM", polishFormat);
            string currentDayFormat = DateTime.Now.ToString("dd_MM_yyyy");
            string directoryName = "Notatki";

            var result = Path.Combine(GetMyDataPath(), currentYear, currentMonth, currentDayFormat, directoryName);

            if (!Directory.Exists(result))
            {
                Directory.CreateDirectory(result);
            }

            return result;
        }

        public static void InitDailyNotes()
        {
            InitDailyTextFile();
            InitDailyJsonFile();
        }

        public static string GetAllDataNoteFullPath()
        {
            string result = Path.Combine(GetAllDataPath(), "notsyWazne.txt");

            if (!File.Exists(result)) File.Create(result);

            return result;
        }

        public static string GetDailyFileFullPath(string extension)
        {
            string dailyNotesPath = GetNotesPath();
            string currentDayFormat = DateTime.Now.ToString("dd_MM_yyyy");

            string txtFileName = $"{currentDayFormat}{extension}";

            string result = Path.Combine(dailyNotesPath, txtFileName);

            if (!File.Exists(result)) File.Create(result);

            return result;
        }

        public static void InitDailyTextFile()
        {
            string txtFileFullPath = GetDailyFileFullPath(".txt");

            if (!File.Exists(txtFileFullPath))
            {
                File.Create(txtFileFullPath);
            }
        }

        public static void InitDailyJsonFile()
        {
            string jsonFileFullPath = GetDailyFileFullPath(".json");

            if (!File.Exists(jsonFileFullPath))
            {
                File.Create(jsonFileFullPath);
            }
        }
    }
}
