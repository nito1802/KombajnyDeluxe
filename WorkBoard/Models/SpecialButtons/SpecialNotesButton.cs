using KombajnDoPracy;
using KombajnDoPracy.Helpers;
using System;
using System.Diagnostics;
using System.Text.Json.Serialization;
using System.Windows.Input;

namespace WorkBoard.Models.SpecialButtons
{
    /// <summary>
    /// Model dla przycisku "Moje Notatki"
    /// </summary>
    public class SpecialNotesButton : NormalButton
    {
        public string MojeDanePath { get; set; }
        public string DataType { get; set; }

        public SpecialNotesButton(string name, string mojeDanePath, string dataType)
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

        public void InitData()
        {
            ButtonPathGenerator.InitDailyNotes();
            Path = ButtonPathGenerator.GetNotesPath();
        }
    }
}
