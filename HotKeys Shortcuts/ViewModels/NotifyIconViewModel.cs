using Kombajn_Shortcut.Helpers;
using Kombajn_Shortcut.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Kombajn
{
   public class NotifyIconViewModel
   {
      public List<ShortcutModel> Shortcuts { get; set; }

      public NotifyIconViewModel(List<ShortcutModel> shortcuts)
      {
         this.Shortcuts = shortcuts;
      }

      /// <summary>
      /// Shows a window, if none is already open.
      /// </summary>
      public ICommand ShowWindowCommand
      {
         get
         {
            return new DelegateCommand
            {
               CommandAction = () =>
               {
                  Application.Current.MainWindow = new MainWindow(Shortcuts);
                  Application.Current.MainWindow.Show();
               }
            };
         }
      }
      
      /// <summary>
      /// Shuts down the application.
      /// </summary>
      public ICommand ExitApplicationCommand
      {
         get
         {
            return new DelegateCommand { CommandAction = () => Application.Current.Shutdown() };
         }
      }
   }
}
