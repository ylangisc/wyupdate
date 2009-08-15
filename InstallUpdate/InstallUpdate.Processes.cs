﻿using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;

namespace wyUpdate
{
    partial class InstallUpdate
    {
        public void RunProcessesCheck()
        {
            Thread.CurrentThread.IsBackground = true; //make them a daemon

            List<FileInfo> files = new List<FileInfo>(new DirectoryInfo(ProgramDirectory).GetFiles("*.exe", SearchOption.AllDirectories));
            //FileInfo[] files = ;

            RemoveSelfFromProcesses(files);

            //check for (and delete) a newer client if it exists
            DeleteClientInPath(ProgramDirectory, Path.Combine(TempDirectory, "base"));

            bool procNeedClosing = ProcessesNeedClosing(files);

            if (!procNeedClosing)
            {
                //no processes need closing, all done
                files = null;
            }

            Sender.BeginInvoke(SenderDelegate, new object[] { files, true });
        }

        private static void RemoveSelfFromProcesses(List<FileInfo> files)
        {
            for (int i = 0; i < files.Count; i++)
            {
                if (ProcessIsSelf(files[i].FullName))
                {
                    // remove self from the list of processes
                    files.RemoveAt(i);
                    return;
                }
            }
        }

        public static bool ProcessIsSelf(string processPath)
        {
            string self = Assembly.GetExecutingAssembly().Location;

#if DEBUG
            string vhostFile = self.Substring(0, self.Length - 3) + "vshost.exe"; //for debugging

            if (processPath.ToLower() == vhostFile.ToLower())
                return true;
#endif

            if (processPath.ToLower() == self.ToLower())
                return true;

            return false;
        }

        private static bool ProcessesNeedClosing(List<FileInfo> baseFiles)
        {
            Process[] aProcess = Process.GetProcesses();

            bool ProcNeedClosing = false;

            foreach (Process proc in aProcess)
            {
                foreach (FileInfo filename in baseFiles)
                {
                    try
                    {
                        //are one of the exe's in baseDir running?
                        if (proc.MainModule != null && proc.MainModule.FileName.ToLower() == filename.FullName.ToLower())
                        {
                            ProcNeedClosing = true;
                        }
                    }
                    catch { }
                }
            }

            return ProcNeedClosing;
        }
    }
}