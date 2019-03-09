//Downloaded from
//Visual C# Kicks - http://www.vcskicks.com/

using System;
using System.Management; //Need to manually add to the References
using System.IO;

namespace HardwareID
{
    public class HDInfo
    {
        private static string _hdInfo = string.Empty;

        public static string GetUniqueId()
        {
            if (string.IsNullOrEmpty(_hdInfo))
            {
                var drive = "C";

                //Find first drive
                foreach (DriveInfo compDrive in DriveInfo.GetDrives())
                {
                    if (compDrive.IsReady)
                    {
                        drive = compDrive.RootDirectory.ToString();
                        break;
                    }
                }

                if (drive.EndsWith(":\\"))
                {
                    //C:\ -> C
                    drive = drive.Substring(0, drive.Length - 2);
                }

                string volumeSerial = GetVolumeSerial(drive);
                string cpuId = GetCpuId();

                //Mix them up and remove some useless 0's
                _hdInfo = cpuId.Substring(13) + volumeSerial.Substring(0, 3) + cpuId.Substring(4, 1);
            }
            return _hdInfo;
        }

        public static string GetVolumeSerial(string drive)
        {
            try
            {
                ManagementObject disk = new ManagementObject(@"win32_logicaldisk.deviceid=""" + drive + @":""");
                disk.Get();

                string volumeSerial = disk["VolumeSerialNumber"].ToString();
                disk.Dispose();
                return volumeSerial;
            }
            catch (Exception)
            {
                return "0A1B3C4D5E6F7G8HIT4";
            }
        }

        public static string GetCpuId()
        {
            try
            {
                string cpuInfo = "";
                ManagementClass managClass = new ManagementClass("win32_processor");
                ManagementObjectCollection managCollec = managClass.GetInstances();

                foreach (ManagementObject managObj in managCollec)
                {
                    if (cpuInfo == "")
                    {
                        //Get only the first CPU's ID
                        cpuInfo = managObj.Properties["processorID"].Value.ToString();
                        break;
                    }   
                }
                return cpuInfo;

            }
            catch (Exception)
            {
                return "HA9CD8B7K5UN2NLI";
            }
        }
    }
}