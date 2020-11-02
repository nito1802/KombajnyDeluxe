using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace KombajnDoPracy
{
    public class ButtonFacade
    {
        public string Path { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int Idx { get; set; }
        public int GroupId { get; set; }
        public bool CanDelete { get; set; } = true;
        public bool IsEnabled { get; set; } = true; //jesli sciezka nie istnieje, wylaczam button
        public long ClickCounter { get; set; }
        public string TagName { get; set; }

        public ButtonFacade()
        { }

        public ButtonFacade(string path, string name, string description, int groupId, bool canDelete, long clickCounter, string tagName)
        {
            this.Path = path;
            this.Name = name;
            this.Description = description;
            this.GroupId = groupId;
            this.CanDelete = canDelete;
            this.ClickCounter = clickCounter;
            this.TagName = tagName;
        }

        public ButtonFacade(string name, string path,  string description, int groupId, bool canDelete, long clickCounter, string tagName, int ii)
        {
            this.Path = path;
            this.Name = name;
            this.Description = description;
            this.GroupId = groupId;
            this.CanDelete = canDelete;
            this.ClickCounter = clickCounter;
            this.TagName = tagName;
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
                            HelperFacade.OpenOrRestoreWindow(Path);
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
                            Process.Start(Path);
                            CloseApplication();
                        },
                        null
                    );
                }
                return openUrlCommand;
            }
        }

        //public override string ToString()
        //{
        //    return $"ButtonFacade Name: {Name} Path: {Path}";
        //}

        public static Action CloseApplication = null;
    }

    public class MyDataItem : ButtonFacade
    {
        public string MojeDanePath { get; set; }
        public string DataType { get; set; }

        public MyDataItem(string name, string mojeDanePath, string dataType)
        {
            this.Name = name;
            this.MojeDanePath = mojeDanePath;
            this.DataType = dataType;

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
                            Process.Start(Path);
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
            string currentDayFormat = DateTime.Now.ToString("dd_MM_yyyy");

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

    public class MyDataNotes : ButtonFacade
    {
        public string MojeDanePath { get; set; }
        public string DataType { get; set; }

        public MyDataNotes(string name, string mojeDanePath, string dataType)
        {
            this.Name = name;
            this.MojeDanePath = mojeDanePath;
            this.DataType = dataType;

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
                            Process.Start(Path);
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
            string currentDayFormat = DateTime.Now.ToString("dd_MM_yyyy");

            Path = System.IO.Path.Combine(MojeDanePath, currentYear, currentMonth, currentDayFormat, DataType);

            if (!Directory.Exists(MojeDanePath))
            {
                MessageBox.Show("Folder nie istineje!", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            if (!Directory.Exists(Path))
            {
                Directory.CreateDirectory(Path);
            }

            string txtFileName = $"{currentDayFormat}.txt";
            string txtFileFullPath = System.IO.Path.Combine(Path, txtFileName);

            if (!File.Exists(txtFileFullPath))
            {
                File.Create(txtFileFullPath);
            }

            string jsonFileName = $"{currentDayFormat}.json";
            string jsonFileFullPath = System.IO.Path.Combine(Path, jsonFileName);

            if (!File.Exists(jsonFileFullPath))
            {
                File.Create(jsonFileFullPath);
            }
        }
    }
}
