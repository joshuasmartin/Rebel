/****************************** Module Header ******************************\
* Copyright (c) Joshua Shane Martin.
* 
* This class provides simple utilities to determine the health of the system.
* 
* All other rights reserved.
\***************************************************************************/

#region Using Directives
using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Management;
using System.Net;
using System.Runtime.Serialization.Json;
#endregion

namespace Rebel
{
    public class Advisor
    {
        /// <summary>
        /// Determines via WMI if the system's free memory is less than
        /// 70% of the system's total memory.
        /// </summary>
        /// <returns>Whether or not the free memory is less than 70% of total memory</returns>
        public bool HasHighMemoryConsumption()
        {
            bool answer = false;

            try
            {
                ObjectQuery wql = new ObjectQuery("SELECT TotalVisibleMemorySize,FreePhysicalMemory FROM Win32_OperatingSystem");
                ManagementObjectSearcher searcher = new ManagementObjectSearcher(wql);
                ManagementObjectCollection results = searcher.Get();

                foreach (ManagementObject result in results)
                {
                    // Get the total and free values from the search and
                    // determine if the free memory is less than 70% of total memory.
                    long total = long.Parse(result["TotalVisibleMemorySize"].ToString());
                    long free = long.Parse(result["FreePhysicalMemory"].ToString());

                    if (((double)free / total) * 100.00 > 70)
                    {
                        return true;
                    }
                    break;
                }
            }
            catch (Exception exception)
            {
                EventLog.WriteEntry("Rebel", exception.ToString());
            }

            return answer;
        }

        /// <summary>
        /// Determines via WMI if the system's total memory is less than 2GB.
        /// </summary>
        /// <returns>Whether or not the total memory is less than 2GB</returns>
        public bool HasInsufficientMemory()
        {
            bool answer = false;

            try
            {
                ObjectQuery wql = new ObjectQuery("SELECT TotalVisibleMemorySize FROM Win32_OperatingSystem");
                ManagementObjectSearcher searcher = new ManagementObjectSearcher(wql);
                ManagementObjectCollection results = searcher.Get();

                foreach (ManagementObject result in results)
                {
                    // Determine if the total memory is less than 2GB.
                    long total = long.Parse(result["TotalVisibleMemorySize"].ToString());

                    // Check if total is less than approximately 1.8GB.
                    if (total < 1835008)
                    {
                        return true;
                    }
                    break;
                }
            }
            catch (Exception exception)
            {
                EventLog.WriteEntry("Rebel", exception.ToString());
            }

            return answer;
        }

        /// <summary>
        /// Determines if the system drive has less than 3GB
        /// of available free space.
        /// </summary>
        /// <returns>Whether or not the system drive has less than 3GB</returns>
        public bool HasHighDiskUsage()
        {
            bool answer = false;

            // 3GB in bytes.
            long low = 3221225472;

            try
            {
                foreach (DriveInfo drive in DriveInfo.GetDrives())
                {
                    if (drive.IsReady && drive.Name == Path.GetPathRoot(Environment.SystemDirectory))
                    {
                        return drive.TotalFreeSpace <= low;
                    }
                }
            }
            catch (Exception exception)
            {
                EventLog.WriteEntry("Rebel", exception.ToString());
            }

            return answer;
        }

        /// <summary>
        /// Determines if any outdated Java runtimes are present on the system
        /// by checking the registry for installed runtimes and if they are
        /// the same as the latest version provided by the Website.
        /// </summary>
        /// <returns>Whether or not outdated Java runtimes are present</returns>
        public bool HasOutdatedJava()
        {
            bool outdated = false;

            try
            {
                RegistryKey environmentsKey = Registry.LocalMachine.OpenSubKey("SOFTWARE\\JavaSoft\\Java Runtime Environment");
                
                if (environmentsKey != null)
                {
                    Version latestVersion = GetLatestVersion("java");

                    // Loop each subkey in the registry for the Java Runtime Environment key.
                    string[] versions = environmentsKey.GetSubKeyNames();
                    foreach (string version in versions)
                    {
                        // Keys can be named something like 1.6 or 1.7,
                        // so ignore those and stick to the keys named
                        // something like 1.7.0_67
                        if (version.Length > 3)
                        {
                            if (version != latestVersion.Name)
                                outdated = true;
                        }
                    }
                }
            }
            catch (WebException) { }
            catch (Exception exception)
            {
                EventLog.WriteEntry("Rebel", exception.ToString());
            }

            return outdated;
        }

        /// <summary>
        /// Determines if any outdated Flash Player instances are present on
        /// the system by checking the registry for the CurrentVersion key and
        /// determining if that version is the same as the latest version
        /// provided by the Website.
        /// </summary>
        /// <returns>Whether or not any outdated Flash Player instances are present</returns>
        public bool HasOutdatedFlash()
        {
            bool outdated = false;

            try
            {
                RegistryKey softwareKey = Registry.LocalMachine.OpenSubKey("Software");

                if (softwareKey != null)
                {
                    RegistryKey macromediaKey = null;

                    // Try to find 64 bit versions of Flash Player.
                    if (Detector.Is64BitOperatingSystem())
                    {
                        RegistryKey software64 = softwareKey.OpenSubKey("Wow6432Node");

                        if (software64 != null)
                            macromediaKey = software64.OpenSubKey("Macromedia");
                    }

                    // If no 64 bit version, try to find 32 bit version.
                    if (macromediaKey == null)
                        macromediaKey = softwareKey.OpenSubKey("Macromedia");

                    // If no 64 bit or 32 bit version can be found,
                    // then Flash Player is probably not installed.
                    if (macromediaKey != null)
                    {
                        RegistryKey flashPlayerKey = macromediaKey.OpenSubKey("FlashPlayer");

                        if (flashPlayerKey != null)
                        {
                            Version latestVersion = GetLatestVersion("flash");

                            string version = Convert.ToString(flashPlayerKey.GetValue("CurrentVersion"));
                            version = version.Replace(",", string.Empty);
                            version = version.Replace(".", string.Empty);

                            // Determine if the version found in the registry is
                            // the same as the latest version.
                            if (version != latestVersion.Name)
                                outdated = true;
                        }
                    }
                }
            }
            catch (WebException) { }
            catch (Exception exception)
            {
                EventLog.WriteEntry("Rebel", exception.ToString());
            }

            return outdated;
        }


        /// <summary>
        /// Determines if any outdated Adobe Reader instances are present on
        /// the system by checking the registry for installations and
        /// determining the Product Version of the file in the given
        /// installation's InstallPath registry key. Determine if that version
        /// is the same as the latest version provided by the Website.
        /// </summary>
        /// <returns>Whether or not any outdated Adobe Reader instances are present</returns>
        public bool HasOutdatedReader()
        {
            bool outdated = false;

            try
            {
                RegistryKey softwareKey = Registry.LocalMachine.OpenSubKey("Software");

                if (softwareKey != null)
                {
                    RegistryKey adobeKey = null;

                    // Try to find 64 bit versions of Adobe Reader.
                    if (Detector.Is64BitOperatingSystem())
                    {
                        RegistryKey software64 = softwareKey.OpenSubKey("Wow6432Node");

                        if (software64 != null)
                            adobeKey = software64.OpenSubKey("Adobe");
                    }

                    // If no 64 bit version, try to find 32 bit version.
                    if (adobeKey == null)
                        adobeKey = softwareKey.OpenSubKey("Adobe");

                    // If no 64 bit or 32 bit version can be found,
                    // then Adobe Reader is probably not installed.
                    if (adobeKey != null)
                    {
                        RegistryKey acrobatReaderKey = adobeKey.OpenSubKey("Acrobat Reader");

                        if (acrobatReaderKey != null)
                        {
                            Version latestVersion = GetLatestVersion("reader");

                            string[] acrobatReaderInstances = acrobatReaderKey.GetSubKeyNames();

                            // Each instance under SOFTWARE/Adobe/Acrobat Reader
                            // is a major version name like 11.0, so we need to look
                            // inside each instances' InstallPath key to find the
                            // executable and get the executable file's Product Version
                            // to determine the exact version of the instance installed.
                            foreach (string instance in acrobatReaderInstances)
                            {
                                RegistryKey instanceKey = acrobatReaderKey.OpenSubKey(instance);

                                // Find the install location of the instance and determine
                                // the file's Product Version.
                                string installPath = instanceKey.OpenSubKey("InstallPath").GetValue(null).ToString();
                                string version = FileVersionInfo.GetVersionInfo(installPath + @"\AcroRd32.exe").ProductVersion;
                                version = version.Replace(",", string.Empty);
                                version = version.Replace(".", string.Empty);

                                // Determine if the installed version is
                                // the same as the latest version.
                                if (version != latestVersion.Name)
                                    outdated = true;
                            }
                        }
                    }
                }
            }
            catch (WebException) { }
            catch (Exception exception)
            {
                EventLog.WriteEntry("Rebel", exception.ToString());
            }

            return outdated;
        }

        public Version GetLatestVersion(string application)
        {
            Version latestVersion = null;

            try
            {
                // Check the latest version of Reader on the Website.
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(string.Format("http://www.getrebel.com/latest/{0}.json", application));
                request.Method = "GET";
                request.ContentType = "application/json";

                WebResponse response = request.GetResponse();

                DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(Version));
                using (Stream s = response.GetResponseStream())
                {
                    latestVersion = ser.ReadObject(s) as Version;
                }
            }
            catch (WebException) { }
            catch (Exception exception)
            {
                EventLog.WriteEntry("Rebel", exception.ToString());
            }

            return latestVersion;
        }
    }

    [Serializable]
    public class Version
    {
        public string Name;
    }
}
