using System;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Threading;

namespace GKeys
{
    /// <summary>
    /// Converting this to a number will return the zero based key pressed (ex: G5 would be 4)
    /// </summary>
    public enum GKey
    {
        G1 = 0,
        G2 = 1,
        G3 = 2,
        G4 = 3,
        G5 = 4,
        G6 = 5,
        G7 = 6,
        G8 = 7, 
        G9 = 8,
        G10 = 9,
        G11 = 10,
        G12 = 11,
        G13 = 12,
        G14 = 13,
        G15 = 14,
        G16 = 15, 
        G17 = 16,
        G18 = 17
    }

    public enum Mode
    {
        M1 = 0,
        M2 = 1,
        M3 = 2
    }

    public delegate void OnGKeyDownEventHandler(GKey whichKey);
    public delegate void OnGKeyUpEventHandler(GKey whichKey);
    public delegate void OnModeChangeEventHandler(Mode whichMode);

    /// <summary>
    /// Class for checking if the G-Keys of a G11 keyboard are pressed and much more
    /// </summary>
    public class GKeyHandler
    {
        [DllImportAttribute("Kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern bool ReadProcessMemory
        (
            IntPtr hProcess,
            IntPtr lpBaseAddress,
            byte[] lpBuffer,
            UInt32 nSize,
            ref UInt32 lpNumberOfBytesRead
        );

        [DllImport("kernel32.dll")]
        static extern bool VirtualProtectEx(IntPtr hProcess, IntPtr lpAddress,
           UIntPtr dwSize, uint flNewProtect, out uint lpflOldProtect);

        
        private IntPtr gHandle;
        private int[] gAddresses;
        private const int MODE_ADDRESS = 0x0012F520;

        private bool[] newGKeyState;
        private bool[] oldGKeyState;

        private int m_timerPeriod;

        private int mode;

        private Timer m_timer;

        /// <summary>
        /// Gets called when a G-Key is pressed
        /// </summary>
        public OnGKeyDownEventHandler OnGKeyDown;

        /// <summary>
        /// Gets called when a G-Key is released
        /// </summary>
        public OnGKeyUpEventHandler OnGKeyUp;

        /// <summary>
        /// Gets called when the mode (M1 M2 M3) changes
        /// </summary>
        public OnModeChangeEventHandler OnModeChange;

        /// <summary>
        /// Creates a new instance of GKeyHandler
        /// </summary>
        /// <param name="checkInterval">The time in milliseconds to update the key states. 
        /// A high value may be more inaccurate while very low values can cause the application to stop working.</param>
        public GKeyHandler(int checkInterval)
        {
            m_timerPeriod = checkInterval;
            gHandle = Process.GetProcessesByName("LGDCore")[0].Handle;
            if (gHandle == null)
            {
                Console.WriteLine("Process \"LGDCore.exe\" was not found. Exiting...");
                return;
            }

            //Offsets for version 1.03.166
            gAddresses = new int[] 
            {   
                0x3AABBC,
                0x3AABC8,
                0x3AABD4,
                0x3AABE0,
                0x3AABEC,
                0x3AABF8,
                0x3AAC04,
                0x3AAC10,
                0x3AAC1C,
                0x3AAC28,
                0x3AAC34,
                0x3AAC40,
                0x3AAC4C,
                0x3AAC58,
                0x3AAC64,
                0x3AAC70,
                0x3AAC7C,
                0x3AAC88
            };
            newGKeyState = new bool[18];
            oldGKeyState = new bool[18];

            mode = GetMode();

            m_timer = new Timer(new TimerCallback(Update), null, 0, checkInterval);
        }

        private void Update(object state)
        {
            if (OnGKeyUp != null || OnGKeyDown != null)
            {
                int tempMode = GetMode();
                if (tempMode == -1)
                {
                    if (Process.GetProcessesByName("LGDCore")[0] != null)
                    {
                        Console.WriteLine("An error occured and the memory could not be read");
                        //return;
                    }
                    else
                    {
                        Console.WriteLine("The process was closed. Exiting...");
                        m_timer.Change(Timeout.Infinite, Timeout.Infinite);
                        return;
                    }
                }
                if (tempMode != mode && OnModeChange != null)
                    OnModeChange((Mode)tempMode);
                mode = GetMode();
                Buffer.BlockCopy(newGKeyState, 0, oldGKeyState, 0, sizeof(bool) * 18);
                for (int i = 0; i < 18; i++)
                {
                    int j = ReadInt(gAddresses[i]);
                    if (j == -1)
                    {
                        if (Process.GetProcessesByName("LGDCore")[0] != null)
                        {
                            Console.WriteLine("An error occured and the memory could not be read");
                        }
                        else
                        {
                            Console.WriteLine("The process was closed. Exiting...");
                            m_timer.Change(Timeout.Infinite, Timeout.Infinite);
                            return;
                        }
                    }
                    newGKeyState[i] = Convert.ToBoolean(j);
                    if (oldGKeyState[i] != newGKeyState[i])
                    {
                        if (newGKeyState[i] == true)
                            if (OnGKeyDown != null)
                            {
                                OnGKeyDown((GKey)i);
                            }
                            else
                            {

                            }
                        else
                            if (OnGKeyUp != null)
                            {
                                OnGKeyUp((GKey)i);
                            }
                    }
                }
            }
        }


        /// <summary>
        /// Gets the zero-based mode currently active (M1/M2/M3)
        /// </summary>
        public int GetMode()
        {
            return ReadInt(MODE_ADDRESS);
        }

        /// <summary>
        /// Checks if a key is pressed
        /// </summary>
        /// <param name="index">Zero based index of the key you want to check (ex: G5 would be 4)</param>
        public bool IsKeyDown(int index)
        {
            if (newGKeyState != null)
                return newGKeyState[index];
            else
                return oldGKeyState[index];
        }

        private int ReadInt(int address)
        {
            IntPtr _address = new IntPtr(address);
            byte[] buffer = new byte[4];
            uint bytesRead = new uint();
            bool failed = false;

            if (!ReadProcessMemory(gHandle, _address, buffer, 4, ref bytesRead))
            {
                failed = true;
                if (Marshal.GetLastWin32Error() == 0x06)
                    gHandle = Process.GetProcessesByName("LGDCore")[0].Handle;
            }
            if(failed)
                if (!ReadProcessMemory(gHandle, _address, buffer, 4, ref bytesRead))
                {
                    Console.WriteLine("Failed twice, Error-Code: " + Marshal.GetLastWin32Error());
                    return -1;
                }
            
            return BitConverter.ToInt32(buffer, 0);
        }
    }
}
