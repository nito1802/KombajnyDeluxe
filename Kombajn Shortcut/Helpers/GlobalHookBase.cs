using System;
using Kombajn_Shortcut.Models;
using MouseKeyboardActivityMonitor;
using MouseKeyboardActivityMonitor.WinApi;
using System.Collections.Generic;

namespace Kombajn
{
    class GlobalHookBase : IDisposable
    {
        private KeyboardHookListener KeyboardHook { get; }
        private List<ShortcutModel> Shortcuts { get; set; }

        public GlobalHookBase(List<ShortcutModel> shortcuts)
        {
            Shortcuts = shortcuts;
            KeyboardHook = new KeyboardHookListener(new GlobalHooker());

            KeyboardHook.KeyDown += (s, e) =>
            {
                foreach (var item in Shortcuts)
                {
                    string keyCode = e.KeyCode.ToString();

                    if (item.KeyCode == keyCode || $"D{item.KeyCode}" == keyCode)
                    {
                        if (item.AllModifs)
                        {
                            if (e.Modifiers.HasFlag(System.Windows.Forms.Keys.Control) && e.Modifiers.HasFlag(System.Windows.Forms.Keys.Alt))
                            {
                                item.actionOnClick();
                                e.Handled = true;
                            }
                        }
                        else
                        {
                            if (item.SelectedModificator == ModificatorNumeration.Ctrl && e.Modifiers == System.Windows.Forms.Keys.Control) //for now ctrl is always first selected modifiers
                            {
                                if (item.actionOnClick != null)
                                {
                                    item.actionOnClick();
                                    e.Handled = true;

                                }

                            }
                            else if (item.AlternateSelectedModificator.ToString() == e.Modifiers.ToString()) //alt or shift
                            {
                                if (item.alternateActionOnClick != null)
                                {
                                    item.alternateActionOnClick();
                                    e.Handled = true;

                                }
                            }
                        }
                    }
                }
            };

            KeyboardHook.Start();
        }

        public void Dispose()
        {
            KeyboardHook.Enabled = false;
            KeyboardHook.Dispose();
        }
    }
}
