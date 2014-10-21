using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Text;

namespace Rebel
{
    public class SecurityAdvisor
    {
        public List<string> GetGraylistedApplications()
        {
            List<string> applications = new List<string>();

            try
            {
                RegistryKey uninstallKey = null;
                String key32 = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";
                String key64 = @"SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall";

                // Try to find the 64 bit version of the uninstall key.
                if (Detector.Is64BitOperatingSystem())
                {
                    uninstallKey = Registry.LocalMachine.OpenSubKey(key64);
                }
                else
                {
                    uninstallKey = Registry.LocalMachine.OpenSubKey(key32);
                }

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
                                foreach (string grayed in DownloadGraylist())
                                {
                                    if (name.ToLower().Contains(grayed))
                                        applications.Add(name);
                                }
                            }
                        }
                    }
                    uninstallKey.Close();
                }
            }
            catch (Exception)
            {

            }

            return applications;
        }

        public string[] DownloadGraylist()
        {
            string[] applications = null;

            try
            {
                // Check the latest version of Reader on the Website.
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://calm-harbor-2394.herokuapp.com/latest/graylist.json");
                request.Method = "GET";
                request.ContentType = "application/json";

                WebResponse response = request.GetResponse();

                DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(string[]));
                using (Stream s = response.GetResponseStream())
                {
                    applications = ser.ReadObject(s) as string[];
                }
            }
            catch (WebException) { }
            catch (Exception exception)
            {
                EventLog.WriteEntry("Rebel", exception.ToString());
            }

            return applications;
        }
    }
}
