/*
 * SecurityAdvisor.cs - Rebel Cleaner and Optimizer
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
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;

#endregion

namespace Rebel
{
    /// <summary>
    /// This class contains the utilities needed to download a graylist
    /// of applications from the Rebel Website and search the computer
    /// for these graylisted applications, by name or other means.
    /// </summary>
    public class SecurityAdvisor
    {
        private List<string> applications = new List<string>();

        public List<string> GetGraylistedApplications()
        {
            // Remove any applications recorded during a prior attempt.
            applications.Clear();

            // Check for applications in the 32 bit and 64 bit key locations.
            String key32 = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";
            String key64 = @"SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall";

            detectListedItemsInKey(key32);
            detectListedItemsInKey(key64);

            return applications;
        }

        public ListItem[] DownloadGraylist()
        {
            ListItem[] list = null;

            try
            {
                // Check the latest version of Reader on the Website.
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://www.getrebel.com/latest/graylist.json");
                request.Method = "GET";
                request.ContentType = "application/json";

                WebResponse response = request.GetResponse();

                DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(ListItem[]));
                using (Stream s = response.GetResponseStream())
                {
                    list = ser.ReadObject(s) as ListItem[];
                }
            }
            catch (WebException) { }
            catch (Exception exception)
            {
                EventLog.WriteEntry("Rebel", exception.ToString());
            }

            return list;
        }

        private void detectListedItemsInKey(string key)
        {
            try
            {
                RegistryKey uninstallKey = null;
                uninstallKey = Registry.LocalMachine.OpenSubKey(key);

                if (uninstallKey != null)
                {
                    foreach (string subkeyName in uninstallKey.GetSubKeyNames())
                    {
                        string name = uninstallKey.OpenSubKey(subkeyName).GetValue("DisplayName", "").ToString();
                        string systemComponent = uninstallKey.OpenSubKey(subkeyName).GetValue("SystemComponent", "").ToString();

                        // Ignore system components.
                        if ((systemComponent.Length == 0) || (systemComponent == "0"))
                        {
                            // Check if any part of this application's
                            // name appears in the graylist.
                            if (name.Length > 0)
                            {
                                // Loop through each graylisted term to determine
                                // if this application's name contains any of
                                // the graylisted terms.
                                foreach (ListItem item in DownloadGraylist())
                                {
                                    if (item.method == "contains")
                                    {
                                        if (name.ToLower().Contains(item.value))
                                            applications.Add(name);
                                    }
                                    else if (item.method == "equals")
                                    {
                                        if (name.ToLower().Equals(item.value))
                                            applications.Add(name);
                                    }
                                }
                            }
                        }
                    }
                    uninstallKey.Close();
                }
            }
            catch (Exception) { }
        }
    }

    [DataContract]
    public class ListItem
    {
        [DataMember(Name = "method")]
        public string method { get; set; }

        [DataMember(Name = "value")]
        public string value { get; set; }
    }
}
