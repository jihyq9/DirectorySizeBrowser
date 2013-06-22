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
using System.Threading;
using System.IO;

namespace OpenMix.DirectorySizeBrowser
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        DirectorySizer dirSizer;
        public static RoutedCommand BrowseUpCommand, BrowseDownCommand, RestartCommand, HideToolbarCommand, AboutCommand;

        public MainWindow()
        {
            BrowseUpCommand = new RoutedCommand();
            BrowseDownCommand = new RoutedCommand();
            RestartCommand = new RoutedCommand();
            HideToolbarCommand = new RoutedCommand();
            AboutCommand = new RoutedCommand();

            Loaded += MainWindow_Loaded;
            
            InitializeComponent();
        }

        private void MainWindow_Loaded(object obj, EventArgs args)
        {
            //StartBrowse(); //whether directorysizer created at startup or on user input
        }

        private void Restart_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            StartBrowse();
        }

        private void Restart_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void Close_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void Close_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void About_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            bool abtWindowOpened = false;
            foreach (Window subWindow in OwnedWindows)
            {
                if (subWindow.GetType() == typeof(AboutWindow))
                {
                    abtWindowOpened = true;
                    subWindow.Activate();
                    break;
                }
            }
            if (!abtWindowOpened)
            {
                AboutWindow abtWindow = new AboutWindow();
                abtWindow.Owner = this;
                abtWindow.Show();
            }
        }

        private void About_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void HideToolbar_Checked(object sender, RoutedEventArgs e)
        {
            Buttons.Visibility = Visibility.Collapsed;
        }

        private void HideToolbar_Unchecked(object sender, RoutedEventArgs e)
        {
            Buttons.Visibility = Visibility.Visible;
        }

        private void HideToolbar_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            HideToolbar.IsChecked = !HideToolbar.IsChecked;
        }

        private void HideToolbar_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void BrowseUp_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            DirectorySizer currentDir = mainDock.DataContext as DirectorySizer;
            mainDock.DataContext = currentDir.ParentDir;
        }

        private void BrowseUp_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            DirectorySizer currentDir = mainDock.DataContext as DirectorySizer;
            if (currentDir != null && currentDir.ParentDir != null)
                e.CanExecute = true;
            else
                e.CanExecute = false;
        }

        private void BrowseDown_Click(object sender, RoutedEventArgs e)
        {
            Button from = sender as Button;
            if (from != null)
            {
                DirectorySizer down = from.DataContext as DirectorySizer;
                if (down != null)
                    mainDock.DataContext = down;
            }
        }

        private void load_Button_Click(object sender, RoutedEventArgs e)
        {
            DirectorySizer arf = mainDock.DataContext as DirectorySizer;
            arf.FindSize(true);
            arf.Sort();
            arf.NotifyPropertyChanged("SubDirs");
            foreach (DirectorySizer subDir in arf.SubDirs) //force updates of data
            {
                subDir.NotifyPropertyChanged("Size");
                subDir.NotifyPropertyChanged("SizeRatio");
            }
            arf.NotifyPropertyChanged("Size");
            arf.NotifyPropertyChanged("SizeRatio");
        }

        private void StartBrowse()
        {
            DirectoryInfo chosenDir = DirectorySizer.ChooseDirectoryDialog();
            if (chosenDir != null)
            {
                dirSizer = new DirectorySizer(chosenDir, null, false, false); //create single tier DS
                mainDock.DataContext = dirSizer;
                dirSizer.CreateChildren(true, 0, null);
                dirSizer.Sort();
                dirSizer.NotifyPropertyChanged("SubDirs");
                //dirInfo.FindSize();
            }
        }
    }
}
