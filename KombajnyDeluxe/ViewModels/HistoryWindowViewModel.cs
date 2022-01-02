using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace KombajnDoPracy
{
    public class MyModel
    {
        public string Path { get; set; }
        public DateTime DateModified { get; set; }

        public MyModel(string path, DateTime dateModified)
        {
            Path = path;
            DateModified = dateModified;
        }

        public override string ToString()
        {
            return $"Path: {Path} DateModif: {DateModified}";
        }
    }

    public class DayDataModel
    {
        public long FilesAndFoldersCount { get; set; }
        public DateTime Date { get; set; }
        public string DayPath { get; set; }
        public string DataText { get; set; }
        public List<string> Items { get; set; }

        public DayDataModel(DateTime date, string dayPath, string dataText, long filesAndFoldersCount, List<string> items)
        {
            Date = date;
            DayPath = dayPath;
            DataText = dataText;
            FilesAndFoldersCount = filesAndFoldersCount;
            Items = items;
        }

        private ICommand enterLocation;

        public ICommand EnterLocation
        {
            get
            {
                if (enterLocation == null)
                {
                    enterLocation = new RelayCommand(
                        param =>
                        {
                            if (!Directory.Exists(DayPath)) return;
                            Process.Start(DayPath);
                            App.Current.Shutdown();
                        },
                        null
                    );
                }
                return enterLocation;
            }
        }

        public override string ToString()
        {
            return $"Day: {Date}, Count: {FilesAndFoldersCount}, Text: {DataText}";
        }
    }

    public class HistoryWindowViewModel
    {
        public string MojeDanePath { get; set; }

        public HistoryWindowViewModel(string mojeDanePath)
        {
            this.MojeDanePath = mojeDanePath;
        }

        private ICommand openOrRestoreWindowCommand;

        public ICommand OpenOrRestoreWindowCommand
        {
            get
            {
                if (openOrRestoreWindowCommand == null)
                {
                    openOrRestoreWindowCommand = new RelayCommand(
                        param =>
                        {
                            int argDays = !string.IsNullOrEmpty(param.ToString()) ? int.Parse(param.ToString()) : 0;
                            var polishFormat = new CultureInfo("pl-PL");

                            List<DayDataModel> filesFromDays = new List<DayDataModel>();

                            Stopwatch sw = new Stopwatch();
                            sw.Start();

                            for (int i = 0; i < argDays; i++)
                            {
                                string currentYear = DateTime.Now.AddDays(-i).Year.ToString();
                                string currentMonth = DateTime.Now.AddDays(-i).ToString("MMMM", polishFormat);
                                string currentDayFormat = DateTime.Now.AddDays(-i).ToString("dd_MM_yyyy");
                                string dataType = "Inne";

                                var dayPath = System.IO.Path.Combine(MojeDanePath, currentYear, currentMonth, currentDayFormat, dataType);

                                if (!Directory.Exists(dayPath))
                                {
                                    DayDataModel dayData = new DayDataModel(DateTime.Now.AddDays(-i), "Directory is not exist", "Directory is not exist", 0, null);
                                    filesFromDays.Add(dayData);
                                    continue;
                                }

                                var entries = Directory.GetFileSystemEntries(dayPath, "*", SearchOption.AllDirectories);

                                List<MyModel> processedFiles = new List<MyModel>();

                                string currentItem = null;

                                try
                                {
                                    for (int idx = 0; idx < entries.Length; idx++)
                                    {
                                        currentItem = entries[idx];

                                        var processedEntriesItem = new MyModel(currentItem, File.GetLastWriteTime(currentItem));

                                        processedFiles.Add(processedEntriesItem);
                                    }
                                }
                                catch (Exception ex)
                                {

                                }

                                var superProcessedItems = processedFiles
                                                        .OrderByDescending(b => b.DateModified)
                                                        .Select(c => c.Path)
                                                        .Select(d => d.Replace(dayPath + "\\", ""))
                                                        .ToList();

                                string text = string.Join(Environment.NewLine, entries);

                                DayDataModel res = new DayDataModel(DateTime.Now.AddDays(-i), dayPath, text, superProcessedItems.Count, superProcessedItems);
                                filesFromDays.Add(res);
                            }

                            sw.Stop();

                            DaysDataInne daysWindow = new DaysDataInne(filesFromDays);
                            daysWindow.Show();

                            int aaa = 2;
                            //Process.Start(Path);
                            //CloseApplication();
                        },
                        null
                    );
                }
                return openOrRestoreWindowCommand;
            }
        }
















        private ICommand historyOfScreensCommand;

        public ICommand HistoryOfScreensCommand
        {
            get
            {
                if (historyOfScreensCommand == null)
                {
                    historyOfScreensCommand = new RelayCommand(
                        param =>
                        {
                            string path = @"C:\Users\dante\source\repos myProject\KombajnyDeluxe\DisplayScreens\bin\Release\netcoreapp3.1\DisplayScreens.exe";

                            string searchScreenFrom = @"C:\Users\dante\Desktop\MojeDane";
                            int dateRange = int.Parse(param.ToString()); ;

                            Process process = new Process();
                            process.StartInfo.FileName = path;
                            process.StartInfo.Arguments = $"{dateRange} \"{searchScreenFrom}\"";
                            process.Start();
                            IntPtr handle = process.Handle;
                            WinApiHelper.SetForegroundWindow(handle);
                            App.Current.Shutdown();
                        },
                        null
                    );
                }
                return historyOfScreensCommand;
            }
        }







        static long GetDirectorySize(string p)
        {
            // 1.
            // Get array of all file names.
            string[] a = Directory.GetFiles(p, "*.*");

            // 2.
            // Calculate total bytes of all files in a loop.
            long b = 0;
            foreach (string name in a)
            {
                // 3.
                // Use FileInfo to get length of each file.
                FileInfo info = new FileInfo(name);
                b += info.Length;
            }
            // 4.
            // Return total size
            return b;
        }
    }
}
