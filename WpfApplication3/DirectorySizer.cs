using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.ObjectModel;
using System.Windows.Forms;
using System.Threading.Tasks;

namespace DirectorySizeBrowser
{
    public class DirectorySizer : INotifyPropertyChanged, IComparable<DirectorySizer>
    {
        #region Properties
        public delegate void BranchIncrease(DirectorySizer src, long amount);
        
        public event BranchIncrease directoryAddedCircuit, childSizeCircuit;
        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<DirectorySizer> subDirs;
        public long subDirCount;
        public long thisSize;
        public long childSize;
        public long maxChildSize;
        public DirectoryInfo dirInfo;
        public DirectorySizer parentDir;
        private string dirPath;
        #endregion

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
        public float SizeRatio
        {
            get
            {
                long MaxChildSize = parentDir.RealMaxChildSize;
                if (parentDir == null || MaxChildSize == 0) //prevent errors
                {
                    return 0;
                }
                else
                {
                    float ratio = SizeLong * 100 / MaxChildSize;
                    return ratio;
                }
            }
        }
        public long[] ChildSizes
        {
            get
            {
                long[] childSizes = new long[subDirs.Count];
                for (int i = 0; i < subDirs.Count; ++i)
                    childSizes[i] = subDirs[i].SizeLong;

                return childSizes;
            }
        }
        public long RealMaxChildSize
        {
            get
            {
                return ChildSizes.Max();
            }
        }
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
        public long SizeLong { get { return thisSize + childSize; } }
#endregion

        /// <summary>
        /// Creates a new object representing sizes of nested directories
        /// </summary>
        /// <param name="path">string path of this root directory</param>
        /// <param name="parentDir">null if first (base), else the DirectorySizer for the directory containing this one</param>
        /// <param name="createSubDirs">whether to create subdirectories recursively or not</param>
        public DirectorySizer(DirectoryInfo pathDI, DirectorySizer parentDir, bool createSubDirs)
        {
            dirPath = pathDI.FullName;
            dirInfo = pathDI;

            if (parentDir != null) //single constructor since a this() constructor is called first
            {
                this.parentDir = parentDir;
                directoryAddedCircuit = parentDir.directoryAddedCircuit + parentDir.DirectoryAddedListener; //Extend chain of notifications--notify all parents
                childSizeCircuit = parentDir.childSizeCircuit + parentDir.ChildSizeListener;
            }
            if (directoryAddedCircuit != null)
               directoryAddedCircuit(null, 1);

            #region Create subdirs
            subDirs = new ObservableCollection<DirectorySizer>();
            if (createSubDirs)
            {
                try
                {
                    DirectoryInfo[] subDirDIs = pathDI.GetDirectories();
                    foreach (DirectoryInfo subDirDI in subDirDIs)
                    {
                        DirectorySizer newDir = new DirectorySizer(subDirDI, this, true);

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
            DirectorySizer srcDir = src as DirectorySizer;
            this.childSize += num;
            NotifyPropertyChanged("Size");
            NotifyPropertyChanged("SizeRatio");
        }

        public void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        public int CompareTo(DirectorySizer otherDir)//Implements IComparable<T>.  DirectorySizer compares based on Size
        {
            if (otherDir == null)
                return 1;

            return SizeLong.CompareTo(otherDir.SizeLong);
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
                FileInfo[] files = dirInfo.GetFiles();
                foreach (FileInfo file in files)
                {
                    thisSize += file.Length;
                }
                if (childSizeCircuit != null)
                    childSizeCircuit(this, thisSize);
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

        /// <summary>
        /// Create an Open Directory dialog and make a DirectorySizer based on the result
        /// </summary>
        /// <returns>a DirectorySizer on successful dialog; null otherwise</returns>
        public static DirectorySizer InitializeDirectorySizer()
        {
            DirectoryInfo dirInfo = new DirectoryInfo(@"C:\");
            FolderBrowserDialog chooseDir = new FolderBrowserDialog();
            chooseDir.Description = "Choose a folder to calculate folder sizes from...";
            if (chooseDir.ShowDialog() == DialogResult.OK) //ok, i.e. folder selected
            {
                dirInfo = new DirectoryInfo(chooseDir.SelectedPath);
            }
            else //return null if no folder selected
            {
                return null;
            }
            DirectorySizer newSizer = new DirectorySizer(dirInfo, null, true);

            return newSizer; //successful selection
        }

        public void Sort()
        {
            if (subDirs.Count == 0 || subDirs == null) //no work if no items!
            {
                return;
            }
            List<DirectorySizer> listSubDirs = subDirs.ToList();
            listSubDirs.Sort();
            listSubDirs.Reverse();
            subDirs = new ObservableCollection<DirectorySizer>(listSubDirs);
            foreach (DirectorySizer subDir in subDirs) //recursively sort
                subDir.Sort();
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

#region Test resources
    public class Bunks : ObservableCollection<Bunk> { }

    public class Bunk
    {
        private string a, b;

        public string A { get { return a; } set { a = value; } }
        public string B { get { return b; } set { b = value; } }

        public Bunk() { }
    }
#endregion
}
