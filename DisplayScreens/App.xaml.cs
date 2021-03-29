using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace DisplayScreens
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            int conditionDisplay = e.Args.Length == 2 ? int.Parse(e.Args[0]) : 24;
            string startupPath = e.Args.Length == 2 ? e.Args[1] : @"C:\Users\dante\OneDrive\Pulpit\MojeDane";

            MainWindow wnd = new MainWindow(conditionDisplay, startupPath);

            wnd.Show();
        }
    }
}
