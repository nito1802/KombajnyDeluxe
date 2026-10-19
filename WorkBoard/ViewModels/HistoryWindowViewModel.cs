using Avalonia;
using Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Input;
using WorkBoard.Consts;

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
                            var processStartInfo = new ProcessStartInfo() { FileName = DayPath, UseShellExecute = true };
                            Process.Start(processStartInfo);
                            Application.Current?.TryShutdown();
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

                            for (int i = 0; i < argDays; i++)
                            {
                                string currentYear = DateTime.Now.AddDays(-i).Year.ToString();
                                string currentMonth = DateTime.Now.AddDays(-i).ToString("MMMM", polishFormat);
                                string currentDayFormat = DateTime.Now.AddDays(-i).ToString(Consts.DateFormat);
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

                                try
                                {
                                    foreach (var entry in entries)
                                    {
                                        var processedEntriesItem = new MyModel(entry, File.GetLastWriteTime(entry));
                                        processedFiles.Add(processedEntriesItem);
                                    }
                                }
                                catch (Exception)
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

                            Views.DaysDataInne daysWindow = new Views.DaysDataInne(filesFromDays);
                            daysWindow.Show();
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
                            string searchScreenFrom = KombajnCommon.MojeDanePath;
                            int dateRange = int.Parse(param.ToString());

                            Process process = new Process();
                            process.StartInfo.FileName = AppsPath.DisplayScreensPath;
                            process.StartInfo.Arguments = $"{dateRange} \"{searchScreenFrom}\"";
                            process.StartInfo.UseShellExecute = true;
                            process.Start();
                            Application.Current?.TryShutdown();
                        },
                        null
                    );
                }
                return historyOfScreensCommand;
            }
        }
    }
}
