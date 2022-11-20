using KombajnDoPracy;
using KombajnDoPracy.Helpers;
using System;
using System.Diagnostics;
using System.Text.Json.Serialization;
using System.Windows.Input;

namespace WorkBoard.Models.ClickableButttons.Details
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
            this.Name = name;
            this.MojeDanePath = mojeDanePath;
            this.DataType = dataType;
            Path = ButtonPathGenerator.GetNotesPath();
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
    }
}
