using System;
using System.Runtime.InteropServices;

namespace GammaTools
{
    public class Gamma
    {
        [DllImport("gdi32.dll")]
        static extern bool SetDeviceGammaRamp(IntPtr hdc, ref RAMP lpRamp);

        // Import CreateDC and DeleteDC from gdi32.dll
        [DllImport("gdi32.dll", CharSet = CharSet.Auto)]
        static extern IntPtr CreateDC(string lpszDriver, string lpszDeviceName, string lpszOutput, IntPtr lpInitData);

        [DllImport("gdi32.dll")]
        static extern bool DeleteDC(IntPtr hdc);

        // Import EnumDisplayDevices from user32.dll
        [DllImport("user32.dll", CharSet = CharSet.Ansi)]
        static extern bool EnumDisplayDevices(string lpDevice, uint iDevNum, ref DISPLAY_DEVICE lpDisplayDevice,
            uint dwFlags);

        // Struct to hold display device information
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        struct DISPLAY_DEVICE
        {
            public int cb;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string DeviceName;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string DeviceString;

            public int StateFlags;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string DeviceID;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string DeviceKey;
        }

        // Struct to hold the gamma ramp values
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        struct RAMP
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
            public ushort[] Red;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
            public ushort[] Green;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
            public ushort[] Blue;
        }

        static void SetGammaForMonitor(string deviceName, float gamma)
        {
            RAMP ramp = new RAMP
            {
                Red = new ushort[256],
                Green = new ushort[256],
                Blue = new ushort[256]
            };

            for (int i = 0; i < 256; i++)
            {
                int val = (int)Math.Min(65535, Math.Max(0, Math.Pow(i / 256.0, Math.Pow(4, gamma)) * 65535 + 0.5));
                if (val > 65535) val = 65535;
                ramp.Red[i] = ramp.Green[i] = ramp.Blue[i] = (ushort)val;
            }

            IntPtr hDC = CreateDC(null, deviceName, null, IntPtr.Zero); // Get the device context for the monitor
            if (hDC != IntPtr.Zero)
            {
                SetDeviceGammaRamp(hDC, ref ramp);
                DeleteDC(hDC); // Release the device context
            }
            else
            {
                Console.WriteLine($"Failed to get device context for {deviceName}");
            }
        }

        static void SetGammaForSpecificMonitor(string targetDevice, float gamma)
        {
            DISPLAY_DEVICE d = new DISPLAY_DEVICE();
            d.cb = Marshal.SizeOf(d);

            uint devNum = 0;
            bool found = false;

            // Enumerate all display devices
            while (EnumDisplayDevices(null, devNum, ref d, 0))
            {
                if (d.DeviceName.Equals(targetDevice, StringComparison.OrdinalIgnoreCase) ||
                    d.DeviceString.Contains(targetDevice, StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine($"Adjusting Gamma for {d.DeviceName}: {d.DeviceString}");
                    SetGammaForMonitor(d.DeviceName, gamma);
                    found = true;
                    break;
                }

                devNum++;
            }

            if (!found)
            {
                Console.WriteLine($"No monitor found matching: {targetDevice}");
            }
        }
        public static void SetGammaForMonitorByIndex(int monitorIndex, float gamma)
        {
            DISPLAY_DEVICE d = new DISPLAY_DEVICE();
            d.cb = Marshal.SizeOf(d);

            uint devNum = 0;
            int currentMonitorIndex = 0;

            // Enumerate all display devices
            while (EnumDisplayDevices(null, devNum, ref d, 0))
            {
                if (currentMonitorIndex == monitorIndex)
                {
                    Console.WriteLine($"Monitor: {d.DeviceName}: {d.DeviceString} , Gamma: {0-gamma}");
                    SetGammaForMonitor(d.DeviceName, gamma);
                    return;
                }
                devNum++;
                currentMonitorIndex++;
            }

            Console.WriteLine($"No monitor found with index: {monitorIndex}");
        }
    }
}