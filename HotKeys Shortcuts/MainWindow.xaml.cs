using Kombajn_Shortcut.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;

namespace Kombajn
{
   /// <summary>
   /// Interaction logic for MainWindow.xaml
   /// </summary>
   public partial class MainWindow : Window
   {
      public MainWindow()
      {
         InitializeComponent();
      }

      public MainWindow(List<ShortcutModel> shortcuts)
      {
         InitializeComponent();

         mainGrid.DataContext = this;
         dataGrid.ItemsSource = shortcuts;
      }

      private void btnQuit_Click(object sender, RoutedEventArgs e)
      {
         this.Close();
      }

      private void btnMinimize_Click(object sender, RoutedEventArgs e)
      {
         this.WindowState = WindowState.Minimized;
      }
   }
}
