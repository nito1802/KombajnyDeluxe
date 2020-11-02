﻿using KombajnDoPracy.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KombajnDoPracy.ViewModels
{
    public class SerializableButtonItemViewModel
    {
        private static List<ButtonFacade> InitLinkButton()
        {
            var res = new List<ButtonFacade>()
            {
                new ButtonFacade("https://www.gmail.com/", "Gmail", "", 4, true, 0, "sfg"),
                new ButtonFacade("Youtube", "https://www.youtube.com", "", 4, true, 0, "sfg", 7),
                new ButtonFacade("Gry Online", "https://www.gry-online.pl/", "", 4, true, 0, "sfg", 7),
                new ButtonFacade("CD-Action", "https://www.cdaction.pl/", "", 4, true, 0, "sfg", 7),
                new ButtonFacade("PPE", "https://www.ppe.pl/", "", 4, true, 0, "sfg", 7),
                new ButtonFacade("Evenant", "https://evenant.com/courses/", "", 4, true, 0, "sfg", 7),
                new ButtonFacade("Pluralsight", "https://app.pluralsight.com/library/", "", 4, true, 0, "sfg", 7),
                new ButtonFacade("Udemy", "https://www.udemy.com/", "", 4, true, 0, "sfg", 7),
                new ButtonFacade("Netflix", "https://www.netflix.com/browse", "", 4, true, 0, "sfg", 7),
                new ButtonFacade("HBO GO", "https://hbogo.pl/", "", 4, true, 0, "sfg", 7),
                new ButtonFacade("Ipla", "https://www.ipla.tv/wideo/serial/Miodowe-Lata/5007481", "", 4, true, 0, "sfg", 7),
                new ButtonFacade("Jeremy Either", "https://www.youtube.com/c/JeremyEthier/videos", "", 4, true, 0, "sfg", 7),
                new ButtonFacade("Athlean X", "https://www.youtube.com/c/athleanx/videos", "", 4, true, 0, "sfg", 7),
                new ButtonFacade("Precision Striking", "https://www.youtube.com/c/PrecisionBoxing/videos", "", 4, true, 0, "sfg", 7),
                new ButtonFacade("OSHEEN", "https://www.youtube.com/c/OSHEEN/videos", "", 4, true, 0, "sfg", 7),
                new ButtonFacade("Vories", "https://www.youtube.com/c/Voriesmusic/videos", "", 4, true, 0, "sfg", 7),











                //new LinkUrlModel("Youtube", "https://www.youtube.com"),
                //new LinkUrlModel("Gry Online", "https://www.gry-online.pl/"),
                //new LinkUrlModel("CD-Action", "https://www.cdaction.pl/"),
                //new LinkUrlModel("PPE", "https://www.ppe.pl/"),
                //new LinkUrlModel("Evenant", "https://evenant.com/courses/"),
                //new LinkUrlModel("Pluralsight", "https://app.pluralsight.com/library/"),
                //new LinkUrlModel("Udemy", "https://www.udemy.com/"),
                //new LinkUrlModel("Netflix", "https://www.netflix.com/browse"),
                ////new LinkUrlModel("HBO GO", "https://hbogo.pl/"),
                //new LinkUrlModel("Ipla", "https://www.ipla.tv/wideo/serial/Miodowe-Lata/5007481"),
                //new LinkUrlModel("Jeremy Either", "https://www.youtube.com/c/JeremyEthier/videos"),
                //new LinkUrlModel("Athlean X", "https://www.youtube.com/c/athleanx/videos"),
                //new LinkUrlModel("Precision Striking", "https://www.youtube.com/c/PrecisionBoxing/videos"),
                //new LinkUrlModel("OSHEEN", "https://www.youtube.com/c/OSHEEN/videos"),
                //new LinkUrlModel("Vories", "https://www.youtube.com/c/Voriesmusic/videos"),
            };
            return res;
        }

        [JsonIgnore]
        public static List<ButtonFacade> AlwaysThereButtons { get; set; }
        public static string SerializedButtonsStatePath { get; } = "SerializedButtonsState.json";
        public static string MojeDaneTag = "__MojeDane";
        public static string AllDaneTag = "__AllDane";
        public static string ScreenyTag = "__Screeny";
        public static string NotatkiTag = "__Notatki";

        public List<ButtonFacade> LeftButtons { get; set; }
        public List<ButtonFacade> MiddleButtons { get; set; }
        public List<ButtonFacade> RightButtons { get; set; }
        public List<ButtonFacade> LinkButtons { get; set; }

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
            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

            string mojeDanePath = Path.Combine(desktopPath, "MojeDane");
            string AllDanePath = Path.Combine(desktopPath, "All Dane");

            if (!Directory.Exists(mojeDanePath)) Directory.CreateDirectory(mojeDanePath);
            if (!Directory.Exists(AllDanePath)) Directory.CreateDirectory(AllDanePath);

            AlwaysThereButtons = new List<ButtonFacade>
            {
                new MyDataItem("Moje Dane", mojeDanePath, "Inne"){ GroupId = 2, CanDelete = false, TagName = MojeDaneTag},
                new ButtonFacade {Path = AllDanePath, Name = "All Dane", GroupId = 2, CanDelete = false, TagName = AllDaneTag },
                new MyDataItem("Moje Screeny", mojeDanePath, "Screeny"){ GroupId = 2, CanDelete = false, TagName = ScreenyTag },
                new MyDataNotes("Moje Notatki", mojeDanePath, "Notatki"){ GroupId = 2, CanDelete = false, TagName = NotatkiTag },
            };
        }

        private static void ValidateSpecialButton(List<ButtonFacade> buttons, string tagName, int alwaysThereIdx)
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

        private static List<ButtonFacade> ValidateGroupedButtonList(List<ButtonFacade> buttons, int groupIdx)
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

        private static void HandleSpecialButton(List<ButtonFacade> buttons, string tagName, int alwaysThereIdx)
        {
            var btnIdx = GetButtonIndex(buttons, MojeDaneTag);
            if (btnIdx != alwaysThereIdx)
            {
                var myBtn = buttons.First(a => a.TagName == tagName);
                buttons.Remove(myBtn);
                buttons.Insert(alwaysThereIdx, myBtn);
            }
        }
        private static int GetButtonIndex(List<ButtonFacade> buttons, string tagName)
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

                res.LeftButtons = new List<ButtonFacade>();
                res.MiddleButtons = new List<ButtonFacade>();
                res.RightButtons = new List<ButtonFacade>();
                res.LinkButtons = new List<ButtonFacade>();
            }

            res.LeftButtons = ValidateGroupedButtonList(res.LeftButtons, 1);
            res.MiddleButtons = ValidateGroupedButtonList(res.MiddleButtons, 2);
            res.RightButtons = ValidateGroupedButtonList(res.RightButtons, 3);
            res.LinkButtons = ValidateGroupedButtonList(res.LinkButtons, 4);

            //res.LinkButtons = InitLinkButton();

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

            res.LeftButtons = editedVm.LeftButtons.Where(b => !string.IsNullOrEmpty(b.Path)).Select(a => new ButtonFacade(a.Path, a.Name, a.Description, a.GroupId, a.CanDelete, a.ClickCounter, a.TagName)).ToList();
            res.MiddleButtons = editedVm.MiddleButtons.Where(b => !string.IsNullOrEmpty(b.Path)).Select(a => new ButtonFacade(a.Path, a.Name, a.Description, a.GroupId, a.CanDelete, a.ClickCounter, a.TagName)).ToList();
            res.RightButtons = editedVm.RightButtons.Where(b => !string.IsNullOrEmpty(b.Path)).Select(a => new ButtonFacade(a.Path, a.Name, a.Description, a.GroupId, a.CanDelete, a.ClickCounter, a.TagName)).ToList();
            res.LinkButtons = editedVm.UrlButtons.Where(b => !string.IsNullOrEmpty(b.Path)).Select(a => new ButtonFacade(a.Path, a.Name, a.Description, a.GroupId, a.CanDelete, a.ClickCounter, a.TagName)).ToList();

            res.LeftButtons = ValidateGroupedButtonList(res.LeftButtons, 1);
            res.MiddleButtons = ValidateGroupedButtonList(res.MiddleButtons, 2);
            res.RightButtons = ValidateGroupedButtonList(res.RightButtons, 3);
            res.LinkButtons = ValidateGroupedButtonList(res.LinkButtons, 4);

            return res;
        }
    }
}
