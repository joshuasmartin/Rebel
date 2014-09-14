using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Text;

namespace Rebel
{
    public class Advisor
    {
        public bool HasHighMemoryConsumption()
        {
            bool answer = false;

            try
            {
                ObjectQuery wql = new ObjectQuery("SELECT * FROM Win32_OperatingSystem");
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
    }
}
