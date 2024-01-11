using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace TerminatorCoreCSharp
{
    public class TerminatorCore
    {
        private static int _processId;
        public IntPtr _processHandle;
        private const int MAX_PATH = 260;
        private bool boAdjustPrivRet;
        public TerminatorCore(int processId)
        {
            if (!IsProcessElevated())
            {
                throw new Exception("[Constructor]Run as administrator!");
            }
            WinAPi.RtlAdjustPrivilege(20, true, false, out boAdjustPrivRet);
            Thread.Sleep(500);
            Init(processId);
        }

        public TerminatorCore(string processName)
        {
            if (processName == string.Empty)
            {
                throw new Exception("[Constructor]Process Name must not be empty!!");
            }
            if (!IsProcessElevated())
            {
                throw new Exception("[Constructor]Run as administrator!");
            }
            WinAPi.RtlAdjustPrivilege(20, true, false, out boAdjustPrivRet);
            Thread.Sleep(500);
            IntPtr handle = GetProcessHandle(processName);
            Init(handle);
        }
        private void Init(int processId)
        {
            _processId = processId;
            _processHandle = GetProcessHandle(_processId);
            if (_processHandle == IntPtr.Zero)
            {
                _processId = -1;
                throw new Exception("[Constructor]Failed to get process handle!");
            }
        }
        private void Init(IntPtr handle)
        {
            if (_processHandle == IntPtr.Zero)
            {
                _processHandle = handle;
            }
            else
            {
                _processId = -1;
                throw new Exception("[Constructor]Failed to get process handle!");
            }
        }
        public bool SuspendProcess()
        {
            if (_processHandle != IntPtr.Zero)
            {
                if (WinAPi.NtSuspendProcess(_processHandle) != 0)
                {
                    return false;
                }
            }
            else
            {
                throw new Exception("[SuspendProcess]Process handle equals null!");
            }
            return true;
        }
        public bool ResumeProcess()
        {
            if (_processHandle != IntPtr.Zero)
            {
                if (WinAPi.NtResumeProcess(_processHandle) != 0)
                {
                    return false;
                }
            }
            else
            {
                throw new Exception("[ResumeProcess]Process handle equals null!");
            }
            return true;
        }
        public void FreeTerminator()
        {
            WinAPi.CloseHandle(_processHandle);

            _processHandle = IntPtr.Zero;
            _processId = -1;
            WinAPi.RtlAdjustPrivilege(20, false, false, out bool boAdjustPrivRet);
        }
        private bool IsProcessElevated()
        {
            bool isElevated = false;
            IntPtr tokenHandle = IntPtr.Zero;

            try
            {
                if (!WinAPi.OpenProcessToken(WinAPi.GetCurrentProcess(), 0x0008, out tokenHandle))
                {
                    throw new ApplicationException("Failed to get Process Token");
                }


                if (!WinAPi.GetTokenInformation(tokenHandle, WinAPi.TokenInformationClass.TokenElevation, out WinAPi.TOKEN_ELEVATION elevation, (uint)Marshal.SizeOf(typeof(WinAPi.TOKEN_ELEVATION)), out uint returnLength))
                {
                    throw new ApplicationException("Failed to get Token Information");
                }

                isElevated = elevation.TokenIsElevated != 0;
            }
            finally
            {
                if (tokenHandle != IntPtr.Zero)
                {
                    WinAPi.CloseHandle(tokenHandle);
                }
            }

            return isElevated;
        }

        private IntPtr GetProcessHandle(int processId)
        {
            List<IntPtr> handleList = new List<IntPtr>();
            IntPtr hTarget = IntPtr.Zero;
            IntPtr hCurr = IntPtr.Zero;

            while (WinAPi.NtGetNextProcess(hCurr, 0x02000000, 0, 0, out hCurr) == 0)
            {
                if (processId == WinAPi.GetProcessId(hCurr))
                {
                    hTarget = hCurr;
                    break;
                }
                handleList.Add(hCurr);
            }

            for (int i = 0; i > handleList.Count; i++)
            {
                WinAPi.CloseHandle(handleList[i]);
            }
            return hTarget;
        }
        private IntPtr GetProcessHandle(string _processName)
        {
            List<IntPtr> handleList = new List<IntPtr>();
            IntPtr hTarget = IntPtr.Zero;
            IntPtr hCurr = IntPtr.Zero;
            StringBuilder processPath = new StringBuilder(MAX_PATH);
            while (WinAPi.NtGetNextProcess(hCurr, 0x02000000, 0, 0, out hCurr) == 0)
            {
                WinAPi.GetProcessImageFileName(hCurr, processPath, (uint)processPath.Capacity);
                string processName = processPath.ToString();
                int lastSlashIndex = processName.LastIndexOfAny(new char[] { '\\', '/' });
                if (lastSlashIndex != -1)
                {
                    processName = processName.Substring(lastSlashIndex + 1);
                }

                if (_processName == processName)
                {
                    hTarget = hCurr;
                    break;
                }
                handleList.Add(hCurr);
            }

            for (int i = 0; i > handleList.Count; i++)
            {
                WinAPi.CloseHandle(handleList[i]);
            }
            return hTarget;
        }
    }

    public static class WinAPi
    {
        public struct TOKEN_ELEVATION
        {
            public uint TokenIsElevated;
        }
        public enum TokenInformationClass
        {
            TokenElevation = 20
        }
        [DllImport("psapi.dll", CharSet = CharSet.Unicode)]
        public static extern uint GetProcessImageFileName(IntPtr hProcess, StringBuilder lpImageFileName, uint nSize);
        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern bool OpenProcessToken(IntPtr processHandle, uint desiredAccess, out IntPtr tokenHandle);
        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern bool GetTokenInformation(IntPtr tokenHandle, TokenInformationClass tokenInformationClass, out TOKEN_ELEVATION tokenInformation, uint tokenInformationLength, out uint returnLength);
        [DllImport("ntdll.dll")]
        public static extern int NtSuspendProcess(IntPtr ProcessHandle);
        [DllImport("ntdll.dll")]
        public static extern int NtResumeProcess(IntPtr ProcessHandle);
        [DllImport("kernel32.dll")]
        public static extern bool CloseHandle(IntPtr hObject);
        [DllImport("kernel32.dll")]
        public static extern IntPtr GetCurrentProcess();
        [DllImport("ntdll.dll")]
        public static extern int NtGetNextProcess(IntPtr ProcessHandle, uint DesiredAccess, uint HandleAttributes, uint Flags, out IntPtr NewProcessHandle);
        [DllImport("ntdll.dll")]
        public static extern int RtlAdjustPrivilege(uint Privilege, bool Enable, bool CurrentThread, out bool WasEnabled);
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern int GetProcessId(IntPtr hWnd);
    }
}
