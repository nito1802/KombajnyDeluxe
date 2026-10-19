using KombajnDoPracy;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text.Json.Serialization;
using System.Windows;
using System.Windows.Input;

namespace WorkBoard.Models.SpecialButtons
{
    /// <summary>
    /// Model dla przycisków "Moje Dane" i "Moje Screeny"
    /// </summary>
    public class SpecialDataButton : NormalButton
    {
        public string MojeDanePath { get; set; }
        public string DataType { get; set; }

        public SpecialDataButton(string name, string mojeDanePath, string dataType)
        {
            Name = name;
            MojeDanePath = mojeDanePath;
            DataType = dataType;

            InitData();
        }

        private ICommand openOrRestoreWindowCommand;

        [JsonIgnore]
        public new ICommand OpenOrRestoreWindowCommand
        {
            get
            {
                if (openOrRestoreWindowCommand == null)
                {
                    openOrRestoreWindowCommand = new RelayCommand(
                        param =>
                        {
                            var processStartInfo = new ProcessStartInfo() { FileName = Path, UseShellExecute = true };
                            Process.Start(processStartInfo);
                            ClickCounter++;
                            CloseApplication();
                        },
                        null
                    );
                }
                return openOrRestoreWindowCommand;
            }
        }

        public new static Action CloseApplication = null;

        void InitData()
        {
            var polishFormat = new CultureInfo("pl-PL");
            string currentYear = DateTime.Now.Year.ToString();
            string currentMonth = DateTime.Now.ToString("MMMM", polishFormat);
            string currentDayFormat = DateTime.Now.ToString(Consts.Consts.DateFormat);

            Path = System.IO.Path.Combine(MojeDanePath, currentYear, currentMonth, currentDayFormat, DataType);

            if (!Directory.Exists(MojeDanePath))
            {
                MessageBox.Show("Folder nie istineje!", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            if (!Directory.Exists(Path))
            {
                Directory.CreateDirectory(Path);
            }
        }
    }
}
