using KombajnDoPracy.Helpers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using WorkBoard.Enums;
using WorkBoard.Models;
using WorkBoard.Models.SpecialButtons;

namespace KombajnDoPracy.ViewModels
{
    public class SerializableButtonItemViewModel
    {
        [JsonIgnore]
        public static List<NormalButton> AlwaysThereButtons { get; set; }
        public static string SerializedButtonsStatePath { get; } = "SerializedButtonsState.json";
        public static string MojeDaneTag { get; } = "__MojeDane";
        public static string AllDaneTag { get; } = "__AllDane";
        public static string ScreenyTag { get; } = "__Screeny";
        public static string NotatkiTag { get; } = "__Notatki";

        public List<NormalButton> LeftButtons { get; set; }
        public List<NormalButton> MiddleButtons { get; set; }
        public List<NormalButton> RightButtons { get; set; }
        public List<NormalButton> LinkButtons { get; set; }

        public string LeftGroupName { get; set; }
        public string MiddleGroupName { get; set; }
        public string RightGroupName { get; set; }
        public long WholeClickCount { get; set; }

        static SerializableButtonItemViewModel()
        {
            InitAlwaysThereButtons();
        }

        public static void InitAlwaysThereButtons()
        {
            string mojeDanePath = ButtonPathGenerator.GetMyDataPath();
            string AllDanePath = ButtonPathGenerator.GetAllDataPath();

            AlwaysThereButtons = new List<NormalButton>
            {
                new SpecialDataButton("Moje Dane", mojeDanePath, "Inne"){ GroupId = ButtonGroup.MiddleButtons, CanDelete = false, TagName = MojeDaneTag},
                new NormalButton {Path = AllDanePath, Name = "All Dane", GroupId = ButtonGroup.MiddleButtons, CanDelete = false, TagName = AllDaneTag },
                new SpecialDataButton("Moje Screeny", mojeDanePath, "Screeny"){ GroupId = ButtonGroup.MiddleButtons, CanDelete = false, TagName = ScreenyTag },
                new SpecialNotesButton("Moje Notatki", mojeDanePath, "Notatki"){ GroupId = ButtonGroup.MiddleButtons, CanDelete = false, TagName = NotatkiTag },
            };
        }

        private static void ValidateSpecialButton(List<NormalButton> buttons, string tagName, int alwaysThereIdx)
        {
            var tagButtons = buttons.Where(a => a.TagName == tagName).ToList();

            long clickCount = 0;

            if(tagButtons.Any())
            {
                clickCount = tagButtons.Max(a => a.ClickCounter);
            }

            tagButtons.ForEach(a => buttons.Remove(a));
            var btnToInsert = AlwaysThereButtons[alwaysThereIdx];
            btnToInsert.ClickCounter = clickCount;

            buttons.Add(btnToInsert);
        }

        private static List<NormalButton> ValidateGroupedButtonList(List<NormalButton> buttons, int groupIdx)
        {
            buttons = buttons.OrderBy(a => a.Idx).ToList();

            if (groupIdx == 2) //tutaj mamy specjalne buttony, one musza byc pierwsze
            {
                buttons = buttons.OrderBy(a => a.Idx).ToList();

                ValidateSpecialButton(buttons, MojeDaneTag, 0);
                ValidateSpecialButton(buttons, AllDaneTag, 1);
                ValidateSpecialButton(buttons, ScreenyTag, 2);
                ValidateSpecialButton(buttons, NotatkiTag, 3);

                HandleSpecialButton(buttons, MojeDaneTag, 0);
                HandleSpecialButton(buttons, AllDaneTag, 1);
                HandleSpecialButton(buttons, ScreenyTag, 2);
                HandleSpecialButton(buttons, NotatkiTag, 3);
            }

            int startIdx = 1;
            foreach (var item in buttons)
            {
                item.Idx = startIdx++;

                //if (!Directory.Exists(item.Path))
                //{
                //    item.IsEnabled = false;
                //}
            }

            return buttons;
        }

        private static void HandleSpecialButton(List<NormalButton> buttons, string tagName, int alwaysThereIdx)
        {
            var btnIdx = GetButtonIndex(buttons, MojeDaneTag);
            if (btnIdx != alwaysThereIdx)
            {
                var myBtn = buttons.First(a => a.TagName == tagName);
                buttons.Remove(myBtn);
                buttons.Insert(alwaysThereIdx, myBtn);
            }
        }
        private static int GetButtonIndex(List<NormalButton> buttons, string tagName)
        {
            var btn = buttons.First(a => a.TagName == tagName);
            int idx = buttons.IndexOf(btn);

            return idx;
        }

        public static SerializableButtonItemViewModel Load()
        {
            var res = GetSerializedState();

            if(res == null)
            {
                res = new SerializableButtonItemViewModel();

                res.LeftGroupName = "LeftGroupName";
                res.MiddleGroupName = "MiddleGroupName";
                res.RightGroupName = "RightGroupName";

                res.LeftButtons = new List<NormalButton>();
                res.MiddleButtons = new List<NormalButton>();
                res.RightButtons = new List<NormalButton>();
                res.LinkButtons = new List<NormalButton>();
            }

            res.LeftButtons = ValidateGroupedButtonList(res.LeftButtons, 1);
            res.MiddleButtons = ValidateGroupedButtonList(res.MiddleButtons, 2);
            res.RightButtons = ValidateGroupedButtonList(res.RightButtons, 3);
            res.LinkButtons = ValidateGroupedButtonList(res.LinkButtons, 4);

            return res;
        }

        public static void Save(SerializableButtonItemViewModel state)
        {
            string myDocumentPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string applicationFolder = "KombajnDeluxe Data";
            var appDataPath = Path.Combine(myDocumentPath, applicationFolder);

            if (!Directory.Exists(appDataPath)) Directory.CreateDirectory(appDataPath);
            string serializedStatePath = Path.Combine(appDataPath, SerializedButtonsStatePath);

            var serializedContent = JsonConvert.SerializeObject(state);
            File.WriteAllText(serializedStatePath, serializedContent);
        }

        private static SerializableButtonItemViewModel GetSerializedState()
        {
            string myDocumentPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string applicationFolder = "KombajnDeluxe Data";
            var appDataPath = Path.Combine(myDocumentPath, applicationFolder);

            if (!Directory.Exists(appDataPath)) Directory.CreateDirectory(appDataPath);
            string serializedStatePath = Path.Combine(appDataPath, SerializedButtonsStatePath);

            if (!File.Exists(serializedStatePath)) return null;

            var serializedContent = File.ReadAllText(serializedStatePath);
            var res = JsonConvert.DeserializeObject<SerializableButtonItemViewModel>(serializedContent);

            return res;
        }

        public static SerializableButtonItemViewModel GetFromEditButtonViewModel(EditButtonViewModel editedVm)
        {
            SerializableButtonItemViewModel res = new SerializableButtonItemViewModel();

            res.LeftGroupName = editedVm.LeftGroupName;
            res.MiddleGroupName = editedVm.MiddleGroupName;
            res.RightGroupName = editedVm.RightGroupName;

            res.WholeClickCount = editedVm.WholeClickCount;

            res.LeftButtons = editedVm.LeftButtons.Where(b => !string.IsNullOrEmpty(b.Path)).Select(a => new NormalButton(a.Path, a.Name, a.Description, a.GroupId, a.CanDelete, a.ClickCounter, a.TagName)).ToList();
            res.MiddleButtons = editedVm.MiddleButtons.Where(b => !string.IsNullOrEmpty(b.Path)).Select(a => new NormalButton(a.Path, a.Name, a.Description, a.GroupId, a.CanDelete, a.ClickCounter, a.TagName)).ToList();
            res.RightButtons = editedVm.RightButtons.Where(b => !string.IsNullOrEmpty(b.Path)).Select(a => new NormalButton(a.Path, a.Name, a.Description, a.GroupId, a.CanDelete, a.ClickCounter, a.TagName)).ToList();
            res.LinkButtons = editedVm.UrlButtons.Where(b => !string.IsNullOrEmpty(b.Path)).Select(a => new NormalButton(a.Path, a.Name, a.Description, a.GroupId, a.CanDelete, a.ClickCounter, a.TagName)).ToList();

            res.LeftButtons = ValidateGroupedButtonList(res.LeftButtons, 1);
            res.MiddleButtons = ValidateGroupedButtonList(res.MiddleButtons, 2);
            res.RightButtons = ValidateGroupedButtonList(res.RightButtons, 3);
            res.LinkButtons = ValidateGroupedButtonList(res.LinkButtons, 4);

            return res;
        }
    }
}
