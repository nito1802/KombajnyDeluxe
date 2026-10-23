using KombajnDoPracy;
using System;
using System.Diagnostics;
using System.IO;
using Newtonsoft.Json;
using System.Windows;
using System.Windows.Input;
using WorkBoard.Enums;

namespace WorkBoard.Models
{
    /// <summary>
    /// Model dla normalnych przycisków folderowych i linkowych 
    /// </summary>
    public class NormalButton
    {
        public long LinkId { get; set; }
        public DateTime AddedAt { get; set; } = DateTime.Now;
        public DateTime? ModifiedAt { get; set; }
        public WorkBoard.Enums.LinkType LinkType { get; set; }
        public string Path { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int Idx { get; set; }
        public ButtonGroup GroupId { get; set; }
        public bool CanDelete { get; set; } = true;
        public bool IsEnabled { get; set; } = true; //jesli sciezka nie istnieje, wylaczam button
        public long ClickCounter { get; set; }
        public string TagName { get; set; }

        public NormalButton()
        { }

        public NormalButton(string path, string name, string description, ButtonGroup groupId, bool canDelete, long clickCounter, string tagName)
        {
            Path = path;
            Name = name;
            Description = description;
            GroupId = groupId;
            CanDelete = canDelete;
            ClickCounter = clickCounter;
            TagName = tagName;
        }

        private ICommand openOrRestoreWindowCommand;

        [JsonIgnore]
        public ICommand OpenOrRestoreWindowCommand
        {
            get
            {
                if (openOrRestoreWindowCommand == null)
                {
                    openOrRestoreWindowCommand = new RelayCommand(
                        param =>
                        {
                            if (!Directory.Exists(Path)) return;
                            ProcessHelper.OpenOrRestoreWindow(Path);
                            ClickCounter++;
                            CloseApplication();
                        },
                        null
                    );
                }
                return openOrRestoreWindowCommand;
            }
        }

        private ICommand openUrlCommand;

        [JsonIgnore]
        public ICommand OpenUrlCommand
        {
            get
            {
                if (openUrlCommand == null)
                {
                    openUrlCommand = new RelayCommand(
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
                return openUrlCommand;
            }
        }

        private ICommand copyPath;

        [JsonIgnore]
        public ICommand CopyPath
        {
            get
            {
                if (copyPath == null)
                {
                    copyPath = new RelayCommand(
                        param =>
                        {
                            Clipboard.SetText(Path);
                            ClickCounter++;
                            Copied?.Invoke(Name);
                        },
                        null
                    );
                }
                return copyPath;
            }
        }

        private ICommand copyNameWithPath;

        [JsonIgnore]
        public ICommand CopyNameWithPath
        {
            get
            {
                if (copyNameWithPath == null)
                {
                    copyNameWithPath = new RelayCommand(
                        param =>
                        {
                            Clipboard.SetText($"{Name} --> {Path}");
                            ClickCounter++;
                            Copied?.Invoke(Name);
                        },
                        null
                    );
                }
                return copyNameWithPath;
            }
        }

        public static Action<string>? Copied;
        public static Action CloseApplication = null;
    }
}
