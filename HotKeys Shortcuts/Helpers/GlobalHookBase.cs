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
        private MouseHookListener MouseHook { get; }

        private List<ShortcutModel> Shortcuts { get; set; }

        public GlobalHookBase(List<ShortcutModel> shortcuts)
        {
            Shortcuts = shortcuts;
            KeyboardHook = new KeyboardHookListener(new GlobalHooker());
            MouseHook = new MouseHookListener(new GlobalHooker());

            KeyboardHook.KeyDown += KeyboardHook_KeyDown;
            MouseHook.MouseDoubleClick += MouseHook_MouseDoubleClick;

            KeyboardHook.Start();
            MouseHook.Start();
        }

        private void MouseHook_MouseDoubleClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            foreach (var item in Shortcuts)
            {
                if(item.IsMouseDoubleMiddleClick && e.Button == System.Windows.Forms.MouseButtons.Middle)
                {
                    item.actionOnClick();
                    break;
                }
            }
        }

        private void KeyboardHook_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
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
                        else if (item.SelectedModificator == ModificatorNumeration.Alt && e.Modifiers == System.Windows.Forms.Keys.Alt) //for now ctrl is always first selected modifiers
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
        }

        public void Dispose()
        {
            KeyboardHook.Enabled = false;
            KeyboardHook.Dispose();

            MouseHook.Enabled = false;
            MouseHook.Dispose();
        }
    }
}
