using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.ObjectModel;
using System.Windows.Forms;

namespace DirectorySizeBrowser
{
    public class DirectorySizer : INotifyPropertyChanged
    {
        public delegate void BranchIncrease(DirectoryInfo src, long amount);
        
        public event BranchIncrease directoryAddedCircuit, childSizeCircuit;
        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<DirectorySizer> subDirs;
        public long subDirCount;
        public long thisSize;
        public long childSize;
        public DirectoryInfo dirInfo;
        public DirectorySizer parentDir;
        private string dirPath;

#region Accessors
        public long SubDirCount { get { return subDirCount; } }
        public string DirPath 
        { 
            get 
            {
                string path = dirPath.Substring(dirPath.LastIndexOf('\\') + 1);
                return path; 
            } 
        }
        public ObservableCollection<DirectorySizer> SubDirs { get { return subDirs; } }
        public string Size
        {
            get
            {
                double kb, mb, gb;
                double totalSize = (double)(thisSize + childSize);
                double outSize;
                string suffix;

                gb = Math.Pow(2, 30);
                mb = Math.Pow(2, 20);
                kb = Math.Pow(2, 10);

                if (totalSize >= gb)
                {
                    outSize = totalSize / gb;
                    suffix = "gb";
                }
                else if (totalSize >= mb)
                {
                    outSize = totalSize / mb;
                    suffix = "mb";
                }
                else if (totalSize >= kb)
                {
                    outSize = totalSize / kb;
                    suffix = "kb";
                }
                else
                {
                    outSize = totalSize;
                    suffix = "b";
                }
                outSize = Math.Round(outSize, 2);

                return outSize + " " + suffix;
            }
        }
#endregion

        /// <summary>
        /// Creates a new object representing sizes of nested directories
        /// </summary>
        /// <param name="path">string path of this root directory</param>
        /// <param name="parentDir">null if first (base), the DirectorySizer for the directory containing this one</param>
        public DirectorySizer(string path, DirectorySizer parentDir)
        {
            dirPath = path;

            if (parentDir != null) //single constructor since a this() constructor is called first
            {
                this.parentDir = parentDir;
                directoryAddedCircuit = parentDir.directoryAddedCircuit + parentDir.DirectoryAddedListener;
                childSizeCircuit = parentDir.childSizeCircuit + parentDir.ChildSizeListener;
            }
            if (directoryAddedCircuit != null)
               directoryAddedCircuit(null, 1);

            #region Create subdirs
            subDirs = new ObservableCollection<DirectorySizer>();
            try
            {
                string[] subDirPaths = Directory.GetDirectories(path);
                foreach (string subDirPath in subDirPaths)
                {
                    DirectorySizer newDir = new DirectorySizer(subDirPath, this);

                    subDirs.Add(newDir);
                    //childSize += newDir.thisSize + newDir.childSize;  //Removed since sizes calculated in another pass
                }
            }
            #endregion
            #region catch blocks
            catch (UnauthorizedAccessException unAuthExc)
            {
                Console.WriteLine(unAuthExc.Message);
            }
            catch (DirectoryNotFoundException dirNotFoundExcep)
            {
                Console.WriteLine(dirNotFoundExcep.Message);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            #endregion
        }

        public void DirectoryAddedListener(object src, long num)
        {
            ++this.subDirCount;
            NotifyPropertyChanged("SubDirCount");
        }

        public void ChildSizeListener(object src, long num)
        {
            this.childSize += num;
            NotifyPropertyChanged("Size");
        }

        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        public void FindSize() //separate from constructor in order to do disk i/o while the program is loaded
        {
            foreach (DirectorySizer subDir in subDirs)
            {
                subDir.FindSize();
            }
            
            thisSize = 0;
            try
            {
                FileInfo[] files = new DirectoryInfo(dirPath).GetFiles();
                foreach (FileInfo file in files)
                {
                    thisSize += file.Length;
                }
                if (childSizeCircuit != null)
                    childSizeCircuit(this.dirInfo, thisSize);
            }
            catch (UnauthorizedAccessException unAuthExc)
            {
                Console.WriteLine(unAuthExc.Message);
            }
            catch (FileNotFoundException fnfe)
            {
                Console.WriteLine(fnfe.Message);
            }
            catch (DirectoryNotFoundException dnfe)
            {
                Console.WriteLine(dnfe.Message);
            }
            return;
        }

        //Create an Open Directory dialog and make a DirectorySizer based on the result
        public static DirectorySizer InitializeDirectorySizer()
        {
            string path = @"C:\";
            FolderBrowserDialog chooseDir = new FolderBrowserDialog();
            chooseDir.Description = "Choose a folder to calculate foldersizes from...";
            if (chooseDir.ShowDialog() == DialogResult.OK) //ok i.e. folder selected
            {
                path = chooseDir.SelectedPath;
            }
            else //return null if no folder selected
            {
                return null;
            }
            //DirectoryInfo findDir = new DirectoryInfo(path);
            DirectorySizer newSizer = new DirectorySizer(path, null);

            return newSizer; //successful selection
        }

        #region Obsolete constructor
        /*
        public DirectorySizer(DirectoryInfo dir, DirectorySizer parentDir)
        {
            dirInfo = dir;
            dirPath = dir.ToString();

            if (parentDir != null) //single constructor since a this() constructor is called first
            {
                this.parentDir = parentDir;
                directoryAddedCircuit = parentDir.directoryAddedCircuit + parentDir.DirectoryAddedListener;
                childSizeCircuit = parentDir.childSizeCircuit + parentDir.ChildSizeListener;
            }
            if(directoryAddedCircuit != null)
                directoryAddedCircuit(dir, 1);

            #region Create subdirs
            subDirs = new ObservableCollection<DirectorySizer>();
            try
            {
                DirectoryInfo[] subDirInfos = dir.GetDirectories();
                foreach (DirectoryInfo subDir in subDirInfos)
                {
                    DirectorySizer newDir = new DirectorySizer(subDir, this);
                    newDir.directoryAddedCircuit = this.DirectoryAddedListener;

                    subDirs.Add(newDir);
                    //childSize += newDir.thisSize + newDir.childSize;
                }
            }
            #endregion
            #region catch blocks
            catch (UnauthorizedAccessException unAuthExc)
            {
                Console.WriteLine(unAuthExc.Message);
            }
            catch (DirectoryNotFoundException dirNotFoundExcep)
            {
                Console.WriteLine(dirNotFoundExcep.Message);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            #endregion
        }
        */
        #endregion
    }
}
