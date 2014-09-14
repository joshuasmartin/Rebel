using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.DirectoryServices;
using System.IO;
using System.Management;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.ServiceProcess;

namespace Rebel
{
    public class Cleaner
    {
        [DllImport("Shell32.dll")]
        static extern int SHEmptyRecycleBin(IntPtr hwnd, string pszRootPath, RecycleFlag dwFlags);

        [DllImport("dnsapi.dll", EntryPoint = "DnsFlushResolverCache")]
        private static extern UInt32 DnsFlushResolverCache();

        enum RecycleFlag : int
        {
            SHERB_NOCONFIRMATION = 0x00000001, // Provide no confirmation, when emptying.
            SHERB_NOPROGRESSUI = 0x00000001, // Show no progress tracking window during the emptying of the recycle bin.
            SHERB_NOSOUND = 0x00000004 // Produce no audio when the emptying of the recycle bin is complete.
        }

        /// <summary>
        /// Deletes the thumbnail cache files for each home directory
        /// under the system drive's Users directory.
        /// 
        /// Thumbnail caches can grow excessively large, and Windows
        /// will not automatically prune them.
        /// </summary>
        public void DeleteThumbnailCaches()
        {
            // Loop each user home directory under the system drive's Users folder.
            string pathToUsers = Path.Combine(Path.GetPathRoot(Environment.SystemDirectory), "Users");
            string[] homes = Directory.GetDirectories(pathToUsers);
            foreach (string home in homes)
            {
                // Determine if the thumbnail cache directory exists.
                string pathToHome = Path.Combine(pathToUsers, home);
                string pathToExplorer = Path.Combine(pathToHome, @"AppData\Local\Microsoft\Windows\Explorer");
                if (Directory.Exists(pathToExplorer))
                {
                    // Delete each thumbnail cache in the directory.
                    DirectoryInfo di = new DirectoryInfo(pathToExplorer);
                    FileInfo[] files = di.GetFiles("*.db");
                    foreach (FileInfo file in files)
                        try
                        {
                            file.Attributes = FileAttributes.Normal;
                            File.Delete(file.FullName);
                        }
                        catch { }
                }
            }
        }

        public void DeleteUserTemporaryFiles()
        {
            // Loop each user home directory under the system drive's Users folder.
            string pathToUsers = Path.Combine(Path.GetPathRoot(Environment.SystemDirectory), "Users");
            string[] homes = Directory.GetDirectories(pathToUsers);
            foreach (string home in homes)
            {
                // Determine if the temp directory exists.
                string pathToHome = Path.Combine(pathToUsers, home);
                string pathToTemp = Path.Combine(pathToHome, @"AppData\Local\Temp");
                if (Directory.Exists(pathToTemp))
                {
                    DirectoryInfo info = new DirectoryInfo(pathToTemp);

                    foreach (FileInfo file in info.GetFiles())
                    {
                        try
                        {
                            file.Delete();
                        }
                        catch { }
                    }
                    foreach (DirectoryInfo dir in info.GetDirectories())
                    {
                        try
                        {
                            dir.Delete(true);
                        }
                        catch { }
                    }
                }
            }
        }

        /// <summary>
        /// Deletes the files and directories contained within the Windows
        /// Prefetch directory. Prefetch is used by Windows to cache certain
        /// applications that are launched often. However, it may contain
        /// data for applications that have been uninstalled or used long ago.
        /// </summary>
        public void DeleteWindowsPrefetchFiles()
        {
            try
            {
                string path = Path.Combine(Environment.GetEnvironmentVariable("SystemRoot"), "Prefetch");
                DirectoryInfo info = new DirectoryInfo(path);

                foreach (FileInfo file in info.GetFiles())
                {
                    file.Delete();
                }
                foreach (DirectoryInfo dir in info.GetDirectories())
                {
                    dir.Delete(true);
                }
            }
            catch (Exception exception)
            {
                EventLog.WriteEntry("Rebel", exception.ToString(), EventLogEntryType.Error);
            }
        }

        public void DeleteWindowsTemporaryFiles()
        {
            string pathToTemp = Path.Combine(Path.GetDirectoryName(Environment.SystemDirectory), "Temp");

            DirectoryInfo tempDirectoryInfo = new DirectoryInfo(pathToTemp);

            foreach (FileInfo file in tempDirectoryInfo.GetFiles())
            {
                try
                {
                    file.Delete();
                }
                catch { }
            }
            foreach (DirectoryInfo dir in tempDirectoryInfo.GetDirectories())
            {
                try
                {
                    dir.Delete(true);
                }
                catch { }
            }
        }

        /// <summary>
        /// Deletes the files in the Windows Update download cache only after
        /// stopping both the Windows Update and Background Intelligent
        /// Transfer Service (BITS) services in order to prevent corruption.
        /// </summary>
        public void DeleteWindowsUpdateDownloadCache()
        {
            ServiceController serviceWindowsUpdate = new ServiceController("wuauserv");
            ServiceController serviceBits = new ServiceController("BITS");

            // Make sure Windows Update service is stopped.
            if (serviceWindowsUpdate.Status.Equals(ServiceControllerStatus.Running) ||
                serviceWindowsUpdate.Status.Equals(ServiceControllerStatus.StartPending))
            {
                serviceWindowsUpdate.Stop();
            }

            // Make sure Background Intelligent Transfer Service service is stopped.
            if (serviceBits.Status.Equals(ServiceControllerStatus.Running) ||
                serviceBits.Status.Equals(ServiceControllerStatus.StartPending))
            {
                serviceBits.Stop();
            }

            // Delete the files and directories in the Windows Update cache.
            try
            {
                string path = Path.Combine(Environment.GetEnvironmentVariable("SystemRoot"), @"SoftwareDistribution\Download");
                DirectoryInfo info = new DirectoryInfo(path);

                foreach (FileInfo file in info.GetFiles())
                {
                    file.Delete();
                }
                foreach (DirectoryInfo dir in info.GetDirectories())
                {
                    dir.Delete(true);
                }
            }
            catch (Exception exception)
            {
                EventLog.WriteEntry("Rebel", exception.ToString(), EventLogEntryType.Error);
            }
        }

        /// <summary>
        /// Deletes the files in the System Level Archived Error Reporting
        /// and Solutions logs.
        /// </summary>
        public void DeleteSystemArchivedErrorReportingLogs()
        {
            try
            {
                string path = Path.Combine(Environment.GetEnvironmentVariable("AllUsersProfile"), @"Microsoft\Windows\WER\ReportArchive");
                DirectoryInfo info = new DirectoryInfo(path);

                foreach (FileInfo file in info.GetFiles())
                {
                    file.Delete();
                }
                foreach (DirectoryInfo dir in info.GetDirectories())
                {
                    dir.Delete(true);
                }
            }
            catch (Exception exception)
            {
                EventLog.WriteEntry("Rebel", exception.ToString(), EventLogEntryType.Error);
            }
        }

        /// <summary>
        /// Empty the Recycle Bin using the Windows API.
        /// </summary>
        public void EmptyRecycleBin()
        {
            try
            {
                SHEmptyRecycleBin(IntPtr.Zero, null, RecycleFlag.SHERB_NOSOUND | RecycleFlag.SHERB_NOCONFIRMATION | RecycleFlag.SHERB_NOPROGRESSUI);
            }
            catch (Exception exception)
            {
                EventLog.WriteEntry("Rebel", exception.ToString(), EventLogEntryType.Error);
            }
        }

        /// <summary>
        /// Flush the DNS using the Windows API.
        /// </summary>
        public void FlushDnsCache()
        {
            try
            {
                UInt32 result = DnsFlushResolverCache();
            }
            catch (Exception exception)
            {
                EventLog.WriteEntry("Rebel", exception.ToString(), EventLogEntryType.Error);
            }
        }
    }
}
