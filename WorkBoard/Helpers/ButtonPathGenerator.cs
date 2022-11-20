using Common;
using System;
using System.Globalization;
using System.IO;
using WorkBoard.Consts;

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

        /// <summary>
        /// Przypisz odpowiedni dzień dla notatek
        /// </summary>
        /// <returns></returns>
        public static string GetNotesPath()
        {
            var polishFormat = new CultureInfo("pl-PL");
            string currentYear = DateTime.Now.Year.ToString();
            string currentMonth = DateTime.Now.ToString("MMMM", polishFormat);
            string currentDayFormat = DateTime.Now.ToString(Consts.DateFormat);
            string directoryName = "Notatki";

            var result = Path.Combine(GetMyDataPath(), currentYear, currentMonth, currentDayFormat, directoryName);

            if (!Directory.Exists(result))
            {
                Directory.CreateDirectory(result);
            }

            return result;
        }

        /// <summary>
        /// Inicjalizuj notatki w danym miesiącu
        /// </summary>
        public static void InitNotesDirectoryInMonth()
        {
            var year = DateTime.Now.Year;
            var month = DateTime.Now.Month;
            var day = DateTime.Now.Day;

            int daysInMonth = DateTime.DaysInMonth(year, month);

            var polishFormat = new CultureInfo("pl-PL");
            string yearText = DateTime.Now.Year.ToString();
            string monthText = DateTime.Now.ToString("MMMM", polishFormat);

            var result = Path.Combine(GetMyDataPath(), yearText, monthText);
            for (int i = day; i <= daysInMonth; i++)
            {
                var date = $"{i}_{month}_{year}";
                CreateDailyFileFullPath(result, date, ".txt");
                CreateDailyFileFullPath(result, date, ".json");
            }
        }

        public static string GetAllDataNoteFullPath()
        {
            string result = Path.Combine(GetAllDataPath(), Consts.NotesImportantFileName);

            if (!File.Exists(result)) File.Create(result).Dispose();

            return result;
        }

        public static string GetDailyFileFullPath(string extension)
        {
            string dailyNotesPath = GetNotesPath();
            string currentDayFormat = DateTime.Now.ToString(Consts.DateFormat);

            string txtFileName = $"{currentDayFormat}{extension}";

            string result = Path.Combine(dailyNotesPath, txtFileName);

            if (!File.Exists(result)) File.Create(result).Dispose();

            return result;
        }

        public static string CreateDailyFileFullPath(string monthNotesPath, string date, string extension)
        {
            string txtFileName = $"{date}{extension}";

            string result = Path.Combine(monthNotesPath, date, Consts.NotatkiDirectoryName, txtFileName);

            if (!File.Exists(result))
            {
                var directory = Path.GetDirectoryName(result);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                File.Create(result).Dispose();
            }

            return result;
        }
    }
}
