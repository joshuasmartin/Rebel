/*
 * Advisor.cs - Rebel Cleaner and Optimizer
 * 
 * Copyright 2014 by Joshua Shane Martin <smokeygrowth.com>
 * 
 * This file is part of Rebel.
 * 
 * Rebel is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * 
 * Rebel is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with Rebel.  If not, see <http://www.gnu.org/licenses/>.
 */

#region Using Directives

using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Management;
using System.Net;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;

#endregion

namespace Rebel
{
    /// <summary>
    /// This class provides several simple utilities to determine the
    /// health and security of the system. This class also contains a
    /// method to connect to the Rebel Website and retrieve the latest
    /// version numbers of various user-space applications.
    /// </summary>
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
                    RegistryKey software64 = softwareKey.OpenSubKey("Wow6432Node");
                    if (software64 != null)
                        macromediaKey = software64.OpenSubKey("Macromedia");

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
                            if (int.Parse(version) < int.Parse(latestVersion.Name))
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
                    RegistryKey software64 = softwareKey.OpenSubKey("Wow6432Node");
                    if (software64 != null)
                        adobeKey = software64.OpenSubKey("Adobe");

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

        public bool HasOutdatedRebel()
        {
            bool outdated = false;

            try
            {
                // Get the latest version from the Website and the
                // currently installed from the running assembly.
                System.Version latestVersion = new System.Version(GetLatestVersion("rebel").Name);
                System.Version installedVersion = Assembly.GetEntryAssembly().GetName().Version;

                // Determine if the installed version older than the latest.
                if (installedVersion < latestVersion)
                    outdated = true;
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

    [DataContract]
    public class Version
    {
        [DataMember(Name = "name")]
        public string Name;
    }
}
