using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.IO;
using System.Windows;
using Forms = System.Windows.Forms;

namespace DirectorySizeBrowser
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        void AppStartup(object sender, StartupEventArgs e)
        {
            DirectorySizeBrowser.MainWindow main = new DirectorySizeBrowser.MainWindow();
            main.Show();
        }
    }
}
