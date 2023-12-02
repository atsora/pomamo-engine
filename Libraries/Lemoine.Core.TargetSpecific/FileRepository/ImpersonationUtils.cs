// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using Lemoine.Core.Log;

namespace Lemoine.Core.TargetSpecific.FileRepository
{
  /// <summary>
  /// Allowing the impersonation of the current process
  /// (necessary to access a shared folder for example)
  /// 
  /// Code from this page:
  /// http://stackoverflow.com/questions/3891260/impersonation-the-current-user-using-windowsimpersonationcontext-to-access-netwo
  ///
  /// Deprecated: use the code of Lemoine.Core.Security.Identity instead (in Lemoine.Core.TargetSpecific.dll)
  /// </summary>
  public static class ImpersonationUtils
  {
    static readonly ILog log = LogManager.GetLogger (typeof (ImpersonationUtils).FullName);

    private const int SW_HIDE = 0;
    private const int SW_SHOW = 5;
    private const int TOKEN_QUERY = 0x0008;
    private const int TOKEN_DUPLICATE = 0x0002;
    private const int TOKEN_ASSIGN_PRIMARY = 0x0001;
    private const int STARTF_USESHOWWINDOW = 0x00000001;
    private const int STARTF_FORCEONFEEDBACK = 0x00000040;
    private const int CREATE_UNICODE_ENVIRONMENT = 0x00000400;
    private const int TOKEN_IMPERSONATE = 0x0004;
    private const int TOKEN_QUERY_SOURCE = 0x0010;
    private const int TOKEN_ADJUST_PRIVILEGES = 0x0020;
    private const int TOKEN_ADJUST_GROUPS = 0x0040;
    private const int TOKEN_ADJUST_DEFAULT = 0x0080;
    private const int TOKEN_ADJUST_SESSIONID = 0x0100;
    private const int STANDARD_RIGHTS_REQUIRED = 0x000F0000;
    private const int TOKEN_ALL_ACCESS =
      STANDARD_RIGHTS_REQUIRED |
      TOKEN_ASSIGN_PRIMARY |
      TOKEN_DUPLICATE |
      TOKEN_IMPERSONATE |
      TOKEN_QUERY |
      TOKEN_QUERY_SOURCE |
      TOKEN_ADJUST_PRIVILEGES |
      TOKEN_ADJUST_GROUPS |
      TOKEN_ADJUST_DEFAULT |
      TOKEN_ADJUST_SESSIONID;

    [StructLayout (LayoutKind.Sequential)]
    private struct PROCESS_INFORMATION
    {
      public IntPtr hProcess;
      public IntPtr hThread;
      public int dwProcessId;
      public int dwThreadId;
    }

    [StructLayout (LayoutKind.Sequential)]
    private struct SECURITY_ATTRIBUTES
    {
      public int nLength;
      public IntPtr lpSecurityDescriptor;
      public bool bInheritHandle;
    }

    [StructLayout (LayoutKind.Sequential)]
    private struct STARTUPINFO
    {
      public int cb;
      public string lpReserved;
      public string lpDesktop;
      public string lpTitle;
      public int dwX;
      public int dwY;
      public int dwXSize;
      public int dwYSize;
      public int dwXCountChars;
      public int dwYCountChars;
      public int dwFillAttribute;
      public int dwFlags;
      public short wShowWindow;
      public short cbReserved2;
      public IntPtr lpReserved2;
      public IntPtr hStdInput;
      public IntPtr hStdOutput;
      public IntPtr hStdError;
    }

    private enum SECURITY_IMPERSONATION_LEVEL
    {
      SecurityAnonymous,
      SecurityIdentification,
      SecurityImpersonation,
      SecurityDelegation
    }

    private enum TOKEN_TYPE
    {
      TokenPrimary = 1,
      TokenImpersonation
    }

    [DllImport ("advapi32.dll", SetLastError = true)]
    private static extern bool CreateProcessAsUser (
      IntPtr hToken,
      string lpApplicationName,
      string lpCommandLine,
      ref SECURITY_ATTRIBUTES lpProcessAttributes,
      ref SECURITY_ATTRIBUTES lpThreadAttributes,
      bool bInheritHandles,
      int dwCreationFlags,
      IntPtr lpEnvironment,
      string lpCurrentDirectory,
      ref STARTUPINFO lpStartupInfo,
      out PROCESS_INFORMATION lpProcessInformation);

    [DllImport ("advapi32.dll", SetLastError = true)]
    private static extern bool DuplicateTokenEx (
      IntPtr hExistingToken,
      int dwDesiredAccess,
      ref SECURITY_ATTRIBUTES lpThreadAttributes,
      int ImpersonationLevel,
      int dwTokenType,
      ref IntPtr phNewToken);

    [DllImport ("advapi32.dll", SetLastError = true)]
    private static extern bool OpenProcessToken (
      IntPtr ProcessHandle,
      int DesiredAccess,
      ref IntPtr TokenHandle);

    [DllImport ("userenv.dll", SetLastError = true)]
    private static extern bool CreateEnvironmentBlock (
      ref IntPtr lpEnvironment,
      IntPtr hToken,
      bool bInherit);

    [DllImport ("userenv.dll", SetLastError = true)]
    private static extern bool DestroyEnvironmentBlock (
      IntPtr lpEnvironment);

    [DllImport ("kernel32.dll", SetLastError = true)]
    private static extern bool CloseHandle (
      IntPtr hObject);

#if NET45
    private static void LaunchProcessAsUser (string cmdLine, IntPtr token, IntPtr envBlock, int sessionId)
    {
      var pi = new PROCESS_INFORMATION ();
      var saProcess = new SECURITY_ATTRIBUTES ();
      var saThread = new SECURITY_ATTRIBUTES ();
      saProcess.nLength = Marshal.SizeOf (saProcess);
      saThread.nLength = Marshal.SizeOf (saThread);

      var si = new STARTUPINFO ();
      si.cb = Marshal.SizeOf (si);
      si.lpDesktop = @"WinSta0\Default";
      si.dwFlags = STARTF_USESHOWWINDOW | STARTF_FORCEONFEEDBACK;
      // si.wShowWindow = SW_SHOW;
      si.wShowWindow = SW_HIDE;

      if (!CreateProcessAsUser (
        token,
        null,
        cmdLine,
        ref saProcess,
        ref saThread,
        false,
        CREATE_UNICODE_ENVIRONMENT,
        envBlock,
        null,
        ref si,
        out pi)) {
        throw new Win32Exception (Marshal.GetLastWin32Error (), "CreateProcessAsUser failed");
      }
    }
#endif // NET45

    private static IDisposable Impersonate (IntPtr token)
    {
#if NET45
      var identity = new System.Security.Principal.WindowsIdentity (token);
      return identity.Impersonate ();
#else
      return new DummyDisposable ();
#endif // NET45
    }

    public static IntPtr GetPrimaryToken (Process process)
    {
#if !NET40
      if (!RuntimeInformation.IsOSPlatform (OSPlatform.Windows)) {
        log.Error ("GetPrimaryToken: not supported platform");
        throw new PlatformNotSupportedException ();
      }
#endif // !NET40

      var token = IntPtr.Zero;
      var primaryToken = IntPtr.Zero;

      if (OpenProcessToken (process.Handle, TOKEN_DUPLICATE, ref token)) {
        var sa = new SECURITY_ATTRIBUTES ();
        sa.nLength = Marshal.SizeOf (sa);

        if (!DuplicateTokenEx (
          token,
          TOKEN_ALL_ACCESS,
          ref sa,
          (int)SECURITY_IMPERSONATION_LEVEL.SecurityImpersonation,
          (int)TOKEN_TYPE.TokenPrimary,
          ref primaryToken)) {
          log.Error ($"GetPrimaryToken: DuplicateTokenEx failed with error {Marshal.GetLastWin32Error ()}");
          throw new Win32Exception (Marshal.GetLastWin32Error (), "DuplicateTokenEx failed");
        }

        CloseHandle (token);
      }
      else {
        log.Error ($"GetPrimaryToken: OpenProcessToken failed with error {Marshal.GetLastWin32Error ()}");
        throw new Win32Exception (Marshal.GetLastWin32Error (), "OpenProcessToken failed");
      }

      return primaryToken;
    }

#if NET45
    private static IntPtr GetEnvironmentBlock (IntPtr token)
    {
      var envBlock = IntPtr.Zero;
      if (!CreateEnvironmentBlock (ref envBlock, token, false)) {
        throw new Win32Exception (Marshal.GetLastWin32Error (), "CreateEnvironmentBlock failed");
      }
      return envBlock;
    }
#endif // NET45

    /// <summary>
    /// Launch a command line as the current user
    /// </summary>
    /// <param name="cmdLine"></param>
    public static void LaunchAsCurrentUser (string cmdLine)
    {
#if NET45
      if (!RuntimeInformation.IsOSPlatform (OSPlatform.Windows)) {
        log.Error ("LaunchAsCurrentUser: not supported platform");
        throw new PlatformNotSupportedException ();
      }

      var process = Process.GetProcessesByName ("explorer").FirstOrDefault ();
      if (process != null) {
        var token = GetPrimaryToken (process);
        if (token != IntPtr.Zero) {
          var envBlock = GetEnvironmentBlock (token);
          if (envBlock != IntPtr.Zero) {
            LaunchProcessAsUser (cmdLine, token, envBlock, process.SessionId);
            if (!DestroyEnvironmentBlock (envBlock)) {
              throw new Win32Exception (Marshal.GetLastWin32Error (), "DestroyEnvironmentBlock failed");
            }
          }

          CloseHandle (token);
        }
      }
#else
      log.Error ("LaunchAsCurrentUser: not supported platform/framework => do nothing");
      throw new NotSupportedException ("Not supported framework");
#endif // NET45
    }

    /// <summary>
    /// Temporary log as the current user
    /// 
    /// In case of error, do nothing, just log it
    /// </summary>
    /// <param name="active">if false, do nothing</param>
    /// <returns></returns>
    public static IDisposable ImpersonateCurrentUser (bool active)
    {
      if (active) {
        return ImpersonateCurrentUser ();
      }
      else {
        return new DummyDisposable ();
      }
    }

    /// <summary>
    /// Temporary log as the current user
    /// 
    /// In case of error, do nothing, just log it
    /// </summary>
    /// <returns></returns>
    public static IDisposable ImpersonateCurrentUser ()
    {
#if NET48 || NETCOREAPP
      if (!RuntimeInformation.IsOSPlatform (OSPlatform.Windows)) {
        log.Warn ("ImpersonateCurrentUser: not supported platform => return a dummy disposable");
        return new DummyDisposable ();
      }
#endif // !(NET48 || NETCOREAPP)

#if NET45
      try {
        var process = Process.GetProcessesByName ("explorer").FirstOrDefault ();
        if (process != null) {
          var token = GetPrimaryToken (process);
          if (token != IntPtr.Zero) {
            return Impersonate (token);
          }
          else {
            log.ErrorFormat ("ImpersonateCurrentUser: " +
                             "GetPrimaryToken for explorer.exe returned an empty token");
            throw new Exception ("No primary token");
          }
        }
        else {
          if (log.IsInfoEnabled) {
            log.InfoFormat ("ImpersonateCurrentUser: " +
                            "explorer.exe not found, it may be normal on a server where no desktop session is opened");
          }
          return new DummyDisposable ();
        }
      }
      catch (Exception ex) {
        log.ErrorFormat ("ImpersonateCurrentUser: " +
                         "exception {0} raised",
                         ex);
        return new DummyDisposable ();
      }
#else
      log.Error ("ImpersonateCurrentUser: not supported framework => return a dummy disposable");
      return new DummyDisposable ();
#endif // NET 45
    }
  }

  /// <summary>
  /// Dummy disposable
  /// </summary>
  public sealed class DummyDisposable : IDisposable
  {
#region IDisposable implementation
    /// <summary>
    /// IDisposable implementation
    /// </summary>
    public void Dispose ()
    {
    }
#endregion
  }
}
