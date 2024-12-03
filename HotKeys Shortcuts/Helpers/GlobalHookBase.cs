using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Gma.System.MouseKeyHook;
using Kombajn_Shortcut.Models;

namespace Kombajn
{
    internal class GlobalHookBase : IDisposable
    {
        private IKeyboardMouseEvents GlobalHook { get; }
        private List<ShortcutModel> Shortcuts { get; set; }

        public GlobalHookBase(List<ShortcutModel> shortcuts)
        {
            Shortcuts = shortcuts;
            GlobalHook = Hook.GlobalEvents();

            GlobalHook.KeyDown += KeyboardHook_KeyDown;
            GlobalHook.MouseDoubleClick += MouseHook_MouseDoubleClick;
        }

        private void MouseHook_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            foreach (var item in Shortcuts)
            {
                if (item.IsMouseDoubleMiddleClick && e.Button == MouseButtons.Middle)
                {
                    item.actionOnClick?.Invoke();
                    break;
                }
            }
        }

        private void KeyboardHook_KeyDown(object sender, KeyEventArgs e)
        {
            foreach (var item in Shortcuts)
            {
                string keyCode = e.KeyCode.ToString();

                if (item.KeyCode == keyCode || $"D{item.KeyCode}" == keyCode)
                {
                    if (item.AllModifs)
                    {
                        if (e.Modifiers.HasFlag(Keys.Control) && e.Modifiers.HasFlag(Keys.Alt))
                        {
                            item.actionOnClick?.Invoke();
                            e.Handled = true;
                        }
                    }
                    else
                    {
                        if (item.SelectedModificator == ModificatorNumeration.Ctrl && e.Modifiers == Keys.Control)
                        {
                            item.actionOnClick?.Invoke();
                            e.Handled = true;
                        }
                        else if (item.SelectedModificator == ModificatorNumeration.Alt && e.Modifiers == Keys.Alt)
                        {
                            item.actionOnClick?.Invoke();
                            e.Handled = true;
                        }
                        else if (item.AlternateSelectedModificator.ToString() == e.Modifiers.ToString())
                        {
                            item.alternateActionOnClick?.Invoke();
                            e.Handled = true;
                        }
                    }
                }
            }
        }

        public void Dispose()
        {
            GlobalHook.KeyDown -= KeyboardHook_KeyDown;
            GlobalHook.MouseDoubleClick -= MouseHook_MouseDoubleClick;
            GlobalHook.Dispose();
        }
    }
}