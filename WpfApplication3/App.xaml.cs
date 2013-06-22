using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.IO;
using System.Windows;

namespace OpenMix.DirectorySizeBrowser
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {   
        void AppStartup(object sender, StartupEventArgs e)
        {
            OpenMix.DirectorySizeBrowser.MainWindow main = new OpenMix.DirectorySizeBrowser.MainWindow();
            main.Show();
        }
    }
}
