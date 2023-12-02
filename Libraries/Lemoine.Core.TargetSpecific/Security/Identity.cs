// Copyright (c) 2023 Atsora Solutions

#if !NET40

using Lemoine.Core.Log;
using Microsoft.Win32.SafeHandles;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Security.Principal;
using System.Threading.Tasks;

namespace Lemoine.Core.Security
{
  /// <summary>
  /// Identity
  /// </summary>
  public static class Identity
  {
    static readonly ILog log = LogManager.GetLogger (typeof (Identity).FullName);

    private const int TOKEN_QUERY = 0x0008;
    private const int TOKEN_DUPLICATE = 0x0002;
    private const int TOKEN_ASSIGN_PRIMARY = 0x0001;
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
    private struct SECURITY_ATTRIBUTES
    {
      public int nLength;
      public IntPtr lpSecurityDescriptor;
      public bool bInheritHandle;
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
    private static extern bool OpenProcessToken (
      IntPtr processHandle,
      int desiredAccess,
      out SafeAccessTokenHandle tokenHandle);

    [DllImport ("advapi32.dll", SetLastError = true)]
    private static extern bool DuplicateTokenEx (
      IntPtr hExistingToken,
      int dwDesiredAccess,
      ref SECURITY_ATTRIBUTES lpThreadAttributes,
      int ImpersonationLevel,
      int dwTokenType,
      out SafeAccessTokenHandle newTokenHandle);

    /// <summary>
    /// Run a function with the session user
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="func"></param>
    /// <returns></returns>
    /// <exception cref="PlatformNotSupportedException"></exception>
    /// <exception cref="Exception"></exception>
    /// <exception cref="Win32Exception"></exception>
    public static T RunImpersonatedAsExplorerUser<T> (Func<T> func)
    {
      if (!RuntimeInformation.IsOSPlatform (OSPlatform.Windows)) {
        log.Error ("RunImpersonatedAsExplorerUser: not supported platform");
        throw new PlatformNotSupportedException ();
      }

      try {
        var process = Process.GetProcessesByName ("explorer").FirstOrDefault ();
        if (process is null) {
          if (log.IsInfoEnabled) {
            log.Error ("RunImpersonatedAsExplorerUser: explorer not found, probably there is no open session");
            throw new Exception ("No open session"); // Create a specific exception for this
          }
        }
        if (!OpenProcessToken (process.Handle, TOKEN_DUPLICATE, out var token)) {
          log.Error ($"RunImpersonatedAsExplorerUser: OpenProcessToken failed with error {Marshal.GetLastWin32Error ()}");
          throw new Win32Exception (Marshal.GetLastWin32Error (), "OpenProcessToken failed");
        }
        using (token) {
          var sa = new SECURITY_ATTRIBUTES ();
          sa.nLength = Marshal.SizeOf (sa);

          if (!DuplicateTokenEx (
            token.DangerousGetHandle (),
            TOKEN_ALL_ACCESS,
            ref sa,
            (int)SECURITY_IMPERSONATION_LEVEL.SecurityImpersonation,
            (int)TOKEN_TYPE.TokenPrimary,
            out var primaryToken)) {
            log.Error ($"RunImpersonatedAsExplorerUser: DuplicateTokenEx failed with error {Marshal.GetLastWin32Error ()}");
            throw new Win32Exception (Marshal.GetLastWin32Error (), "DuplicateTokenEx failed");
          }
          using (primaryToken) {

            // Check the identity.
            if (log.IsDebugEnabled) {
              log.Debug ($"RunImpersonatedAsExplorerUser: before impersonation, user={WindowsIdentity.GetCurrent ().Name}");
            }
            return WindowsIdentity.RunImpersonated (primaryToken, func);

          }
        }
      }
      catch (Exception ex) {
        log.Error ($"RunImpersonatedAsExplorerUser: exception", ex);
        throw;
      }
    }

    /// <summary>
    /// Run a function with the session user
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="func"></param>
    /// <returns></returns>
    /// <exception cref="PlatformNotSupportedException"></exception>
    /// <exception cref="Exception"></exception>
    /// <exception cref="Win32Exception"></exception>
    public static void RunImpersonatedAsExplorerUser<T> (Action func)
    {
      if (!RuntimeInformation.IsOSPlatform (OSPlatform.Windows)) {
        log.Error ("RunImpersonatedAsExplorerUser: not supported platform");
        throw new PlatformNotSupportedException ();
      }

      try {
        var process = Process.GetProcessesByName ("explorer").FirstOrDefault ();
        if (process is null) {
          if (log.IsInfoEnabled) {
            log.Error ("RunImpersonatedAsExplorerUser: explorer not found, probably there is no open session");
            throw new Exception ("No open session"); // Create a specific exception for this
          }
        }
        if (!OpenProcessToken (process.Handle, TOKEN_DUPLICATE, out var token)) {
          log.Error ($"RunImpersonatedAsExplorerUser: OpenProcessToken failed with error {Marshal.GetLastWin32Error ()}");
          throw new Win32Exception (Marshal.GetLastWin32Error (), "OpenProcessToken failed");
        }
        using (token) {
          var sa = new SECURITY_ATTRIBUTES ();
          sa.nLength = Marshal.SizeOf (sa);

          if (!DuplicateTokenEx (
            token.DangerousGetHandle (),
            TOKEN_ALL_ACCESS,
            ref sa,
            (int)SECURITY_IMPERSONATION_LEVEL.SecurityImpersonation,
            (int)TOKEN_TYPE.TokenPrimary,
            out var primaryToken)) {
            log.Error ($"RunImpersonatedAsExplorerUser: DuplicateTokenEx failed with error {Marshal.GetLastWin32Error ()}");
            throw new Win32Exception (Marshal.GetLastWin32Error (), "DuplicateTokenEx failed");
          }
          using (primaryToken) {

            // Check the identity.
            if (log.IsDebugEnabled) {
              log.Debug ($"RunImpersonatedAsExplorerUser: before impersonation, user={WindowsIdentity.GetCurrent ().Name}");
            }
            WindowsIdentity.RunImpersonated (primaryToken, func);

          }
        }
      }
      catch (Exception ex) {
        log.Error ($"RunImpersonatedAsExplorerUser: exception", ex);
        throw;
      }
    }

#if NET6_0_OR_GREATER
    /// <summary>
    /// Run a function with the session user asynchronously
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="func"></param>
    /// <returns></returns>
    /// <exception cref="PlatformNotSupportedException"></exception>
    /// <exception cref="Exception"></exception>
    /// <exception cref="Win32Exception"></exception>
    public static async Task<T> RunImpersonatedAsExplorerUserAsync<T> (Func<Task<T>> func)
    {
      if (!RuntimeInformation.IsOSPlatform (OSPlatform.Windows)) {
        log.Error ("RunImpersonatedAsExplorerUserAsync: not supported platform");
        throw new PlatformNotSupportedException ();
      }

      try {
        var process = Process.GetProcessesByName ("explorer").FirstOrDefault ();
        if (process is null) {
          if (log.IsInfoEnabled) {
            log.Error ("RunImpersonatedAsExplorerUserAsync: explorer not found, probably there is no open session");
            throw new Exception ("No open session"); // Create a specific exception for this
          }
        }
        if (!OpenProcessToken (process.Handle, TOKEN_DUPLICATE, out var token)) {
          log.Error ($"RunImpersonatedAsExplorerUserAsync: OpenProcessToken failed with error {Marshal.GetLastWin32Error ()}");
          throw new Win32Exception (Marshal.GetLastWin32Error (), "OpenProcessToken failed");
        }
        using (token) {
          var sa = new SECURITY_ATTRIBUTES ();
          sa.nLength = Marshal.SizeOf (sa);

          if (!DuplicateTokenEx (
            token.DangerousGetHandle (),
            TOKEN_ALL_ACCESS,
            ref sa,
            (int)SECURITY_IMPERSONATION_LEVEL.SecurityImpersonation,
            (int)TOKEN_TYPE.TokenPrimary,
            out var primaryToken)) {
            log.Error ($"RunImpersonatedAsExplorerUserAsync: DuplicateTokenEx failed with error {Marshal.GetLastWin32Error ()}");
            throw new Win32Exception (Marshal.GetLastWin32Error (), "DuplicateTokenEx failed");
          }
          using (primaryToken) {

            // Check the identity.
            if (log.IsDebugEnabled) {
              log.Debug ($"RunImpersonatedAsExplorerUserAsync: before impersonation, user={WindowsIdentity.GetCurrent ().Name}");
            }
            return await WindowsIdentity.RunImpersonatedAsync<T> (primaryToken, func);

          }
        }
      }
      catch (Exception ex) {
        log.Error ($"RunImpersonatedAsExplorerUser: exception", ex);
        throw;
      }
    }

    /// <summary>
    /// Run a function with the session user asynchronously
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="func"></param>
    /// <returns></returns>
    /// <exception cref="PlatformNotSupportedException"></exception>
    /// <exception cref="Exception"></exception>
    /// <exception cref="Win32Exception"></exception>
    public static async Task RunImpersonatedAsExplorerUserAsync (Func<Task> func)
    {
      if (!RuntimeInformation.IsOSPlatform (OSPlatform.Windows)) {
        log.Error ("RunImpersonatedAsExplorerUserAsync: not supported platform");
        throw new PlatformNotSupportedException ();
      }

      try {
        var process = Process.GetProcessesByName ("explorer").FirstOrDefault ();
        if (process is null) {
          if (log.IsInfoEnabled) {
            log.Error ("RunImpersonatedAsExplorerUserAsync: explorer not found, probably there is no open session");
            throw new Exception ("No open session"); // Create a specific exception for this
          }
        }
        if (!OpenProcessToken (process.Handle, TOKEN_DUPLICATE, out var token)) {
          log.Error ($"RunImpersonatedAsExplorerUserAsync: OpenProcessToken failed with error {Marshal.GetLastWin32Error ()}");
          throw new Win32Exception (Marshal.GetLastWin32Error (), "OpenProcessToken failed");
        }
        using (token) {
          var sa = new SECURITY_ATTRIBUTES ();
          sa.nLength = Marshal.SizeOf (sa);

          if (!DuplicateTokenEx (
            token.DangerousGetHandle (),
            TOKEN_ALL_ACCESS,
            ref sa,
            (int)SECURITY_IMPERSONATION_LEVEL.SecurityImpersonation,
            (int)TOKEN_TYPE.TokenPrimary,
            out var primaryToken)) {
            log.Error ($"RunImpersonatedAsExplorerUserAsync: DuplicateTokenEx failed with error {Marshal.GetLastWin32Error ()}");
            throw new Win32Exception (Marshal.GetLastWin32Error (), "DuplicateTokenEx failed");
          }
          using (primaryToken) {

            // Check the identity.
            if (log.IsDebugEnabled) {
              log.Debug ($"RunImpersonatedAsExplorerUserAsync: before impersonation, user={WindowsIdentity.GetCurrent ().Name}");
            }
            await WindowsIdentity.RunImpersonatedAsync (primaryToken, func);
          }
        }
      }
      catch (Exception ex) {
        log.Error ($"RunImpersonatedAsExplorerUserAsync: exception", ex);
        throw;
      }
    }
#endif // !NET6_0_OR_GREATER

    /// <summary>
    /// Make the owner of a file run a function
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="func"></param>
    /// <returns></returns>
    /// <exception cref="PlatformNotSupportedException"></exception>
    /// <exception cref="Exception"></exception>
    /// <exception cref="Win32Exception"></exception>
    public static T RunImpersonatedAsFileOwner<T> (string path, Func<T> func)
    {
      if (!RuntimeInformation.IsOSPlatform (OSPlatform.Windows)) {
        log.Error ("RunImpersonatedAsFileOwner: not supported platform");
        throw new PlatformNotSupportedException ();
      }

      try {
        var fileInfo = new FileInfo (path);
        var sid = fileInfo.GetAccessControl ().GetOwner (typeof (System.Security.Principal.SecurityIdentifier));
        var ntAccount = sid.Translate (typeof (System.Security.Principal.NTAccount));
        if (log.IsDebugEnabled) {
          log.Debug ($"RunImpersonatedAsFileOwner: {path} => sid={sid} ntAccount={ntAccount}");
        }
        var splitAccountName = ntAccount.Value.Split (new char[] { '\\' }, 2);
        var (domain, name) = (splitAccountName[0], splitAccountName[1]);
        using (var windowsIdentity = new WindowsIdentity ($"{name}@{domain}")) { // This only works on domain users
          if (log.IsDebugEnabled) {
            log.Debug ($"RunImpersonatedAsFileOwner: before impersonation, user={WindowsIdentity.GetCurrent ().Name}");
          }
          using (var token = new SafeAccessTokenHandle (windowsIdentity.Token)) {
            return WindowsIdentity.RunImpersonated (token, func);
          }
        }
      }
      catch (Exception ex) {
        log.Warn ($"RunImpersonatedAsFileOwner: exception", ex);
        throw;
      }
    }

    /// <summary>
    /// Make the owner of a file run a function
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="func"></param>
    /// <returns></returns>
    /// <exception cref="PlatformNotSupportedException"></exception>
    /// <exception cref="Exception"></exception>
    /// <exception cref="Win32Exception"></exception>
    public static void RunImpersonatedAsFileOwner (string path, Action func)
    {
      if (!RuntimeInformation.IsOSPlatform (OSPlatform.Windows)) {
        log.Error ("RunImpersonatedAsFileOwner: not supported platform");
        throw new PlatformNotSupportedException ();
      }

      try {
        var fileInfo = new FileInfo (path);
        var sid = fileInfo.GetAccessControl ().GetOwner (typeof (System.Security.Principal.SecurityIdentifier));
        var ntAccount = sid.Translate (typeof (System.Security.Principal.NTAccount));
        if (log.IsDebugEnabled) {
          log.Debug ($"RunImpersonatedAsFileOwner: {path} => sid={sid} ntAccount={ntAccount}");
        }
        var splitAccountName = ntAccount.Value.Split (new char[] { '\\' }, 2);
        var (domain, name) = (splitAccountName[0], splitAccountName[1]);
        using (var windowsIdentity = new WindowsIdentity ($"{name}@{domain}")) { // This only works on domain users
          if (log.IsDebugEnabled) {
            log.Debug ($"RunImpersonatedAsFileOwner: before impersonation, user={WindowsIdentity.GetCurrent ().Name}");
          }
          using (var token = new SafeAccessTokenHandle (windowsIdentity.Token)) {
            WindowsIdentity.RunImpersonated (token, func);
          }
        }
      }
      catch (Exception ex) {
        log.Warn ($"RunImpersonatedAsFileOwner: exception", ex);
        throw;
      }
    }

#if NET6_0_OR_GREATER
    /// <summary>
    /// Make the owner of a file run a function asynchronously
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="func"></param>
    /// <returns></returns>
    /// <exception cref="PlatformNotSupportedException"></exception>
    /// <exception cref="Exception"></exception>
    /// <exception cref="Win32Exception"></exception>
    public static async Task<T> RunImpersonatedAsFileOwnerAsync<T> (string path, Func<Task<T>> func)
    {
      if (!RuntimeInformation.IsOSPlatform (OSPlatform.Windows)) {
        log.Error ("RunImpersonatedAsFileOwnerAsync: not supported platform");
        throw new PlatformNotSupportedException ();
      }

      try {
        var fileInfo = new FileInfo (path);
        var sid = fileInfo.GetAccessControl ().GetOwner (typeof (System.Security.Principal.SecurityIdentifier));
        var ntAccount = sid.Translate (typeof (System.Security.Principal.NTAccount));
        if (log.IsDebugEnabled) {
          log.Debug ($"RunImpersonatedAsFileOwnerAsync: {path} => sid={sid} ntAccount={ntAccount}");
        }
        var splitAccountName = ntAccount.Value.Split (new char[] { '\\' }, 2);
        var (domain, name) = (splitAccountName[0], splitAccountName[1]);
        using (var windowsIdentity = new WindowsIdentity ($"{name}@{domain}")) { // This only works on domain users
          if (log.IsDebugEnabled) {
            log.Debug ($"RunImpersonatedAsFileOwnerAsync: before impersonation, user={WindowsIdentity.GetCurrent ().Name}");
          }
          using (var token = new SafeAccessTokenHandle (windowsIdentity.Token)) {
            return await WindowsIdentity.RunImpersonatedAsync (token, func);
          }
        }
      }
      catch (Exception ex) {
        log.Warn ($"RunImpersonatedAsFileOwnerAsync: exception", ex);
        throw;
      }
    }

    /// <summary>
    /// Make the owner of a file run a function asynchronously
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="func"></param>
    /// <returns></returns>
    /// <exception cref="PlatformNotSupportedException"></exception>
    /// <exception cref="Exception"></exception>
    /// <exception cref="Win32Exception"></exception>
    public static async Task RunImpersonatedAsFileOwnerAsync (string path, Func<Task> func)
    {
      if (!RuntimeInformation.IsOSPlatform (OSPlatform.Windows)) {
        log.Error ("RunImpersonatedAsFileOwnerAsync: not supported platform");
        throw new PlatformNotSupportedException ();
      }

      try {
        var fileInfo = new FileInfo (path);
        var sid = fileInfo.GetAccessControl ().GetOwner (typeof (System.Security.Principal.SecurityIdentifier));
        var ntAccount = sid.Translate (typeof (System.Security.Principal.NTAccount));
        if (log.IsDebugEnabled) {
          log.Debug ($"RunImpersonatedAsFileOwnerAsync: {path} => sid={sid} ntAccount={ntAccount}");
        }
        var splitAccountName = ntAccount.Value.Split (new char[] { '\\' }, 2);
        var (domain, name) = (splitAccountName[0], splitAccountName[1]);
        using (var windowsIdentity = new WindowsIdentity ($"{name}@{domain}")) { // This only works on domain users
          if (log.IsDebugEnabled) {
            log.Debug ($"RunImpersonatedAsFileOwnerAsync: before impersonation, user={WindowsIdentity.GetCurrent ().Name}");
          }
          using (var token = new SafeAccessTokenHandle (windowsIdentity.Token)) {
            await WindowsIdentity.RunImpersonatedAsync (token, func);
          }
        }
      }
      catch (Exception ex) {
        log.Warn ($"RunImpersonatedAsFileOwnerAsync: exception", ex);
        throw;
      }
    }
#endif // NET6_0_OR_GREATER

  }
}

#endif // !NET40