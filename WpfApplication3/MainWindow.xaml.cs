using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Timers;

namespace DirectorySizeBrowser
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        DirectorySizer dirInfo;
        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object obj, RoutedEventArgs args)
        {
            dirInfo = DirectorySizer.InitializeDirectorySizer();
            if (dirInfo != null)
            {
                mainDock.DataContext = dirInfo;
                //dirInfo.FindSize();
            }
        }

        private void restart_Button_Click(object sender, RoutedEventArgs e)
        {
            dirInfo = DirectorySizer.InitializeDirectorySizer();
            if (dirInfo != null)
            {
                mainDock.DataContext = dirInfo;
                //dirInfo.FindSize();
            }
        }

        private void browseDown_Button_Click(object sender, RoutedEventArgs e)
        {
            Button from = sender as Button;
            DirectorySizer down = from.DataContext as DirectorySizer;
            mainDock.DataContext = down;
        }

        private void up_Button_Click(object sender, RoutedEventArgs e)
        {
            DirectorySizer currentDir = mainDock.DataContext as DirectorySizer;
            if (currentDir.parentDir != null)
                mainDock.DataContext = currentDir.parentDir;
        }

        private void load_Button_Click(object sender, RoutedEventArgs e)
        {
            DirectorySizer arf = mainDock.DataContext as DirectorySizer;
            arf.FindSize();
        }
    }
}
