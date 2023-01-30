// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Runtime.InteropServices;
using BOOL = System.Boolean;
using DWORD = System.UInt32;
using LPWSTR = System.String;
using NET_API_STATUS = System.UInt32;
namespace Lemoine.Net
{
  /// <summary>
  /// Utility class to enable login to a network share
  /// </summary>
  public class UNCAccess
  {
    [StructLayout (LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    internal struct USE_INFO_2
    {
      internal LPWSTR ui2_local;
      internal LPWSTR ui2_remote;
      internal LPWSTR ui2_password;
      internal DWORD ui2_status;
      internal DWORD ui2_asg_type;
      internal DWORD ui2_refcount;
      internal DWORD ui2_usecount;
      internal LPWSTR ui2_username;
      internal LPWSTR ui2_domainname;
    }
    [DllImport ("NetApi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    internal static extern NET_API_STATUS NetUseAdd (
    LPWSTR UncServerName,
    DWORD Level,
    ref USE_INFO_2 Buf,
    out DWORD ParmError);
    [DllImport ("NetApi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    internal static extern NET_API_STATUS NetUseDel (
    LPWSTR UncServerName,
    LPWSTR UseName,
    DWORD ForceCond);
    private string m_UNCPath;
    private string m_user;
    private string m_password;
    private string m_domain;
    private int m_lastError;

    /// <summary>
    /// Description of the constructor
    /// </summary>
    public UNCAccess ()
    {
    }
    /// <summary>
    /// Description of the constructor
    /// </summary>
    /// <param name="uncPath"></param>
    /// <param name="user"></param>
    /// <param name="domain"></param>
    /// <param name="password"></param>
    public UNCAccess (string uncPath, string user, string domain, string password)
    {
      Login (uncPath, user, domain, password);
    }
    /// <summary>
    /// returns last error
    /// </summary>
    /// <returns></returns>
    public int LastError
    {
      get { return m_lastError; }
    }

    /// <summary>
    /// Login to a unc network share
    /// </summary>
    /// <param name="uncPath"></param>
    /// <param name="user"></param>
    /// <param name="domain"></param>
    /// <param name="password"></param>
    /// <returns></returns>
    public int Login (string uncPath, string user, string domain, string password)
    {
      m_UNCPath = uncPath;
      m_user = user;
      m_password = password;
      m_domain = domain;
      return NetUseWithCredentials ();
    }

    /// <summary>
    /// connect the unc network share
    /// </summary>
    /// <returns></returns>
    private int NetUseWithCredentials ()
    {
      uint returncode;
      try {
        USE_INFO_2 useInfo = new USE_INFO_2 ();

        useInfo.ui2_remote = m_UNCPath;
        useInfo.ui2_username = m_user;
        useInfo.ui2_domainname = m_domain;
        useInfo.ui2_password = m_password;
        useInfo.ui2_asg_type = 0;
        useInfo.ui2_usecount = 1;
        uint paramErrorIndex;
        returncode = NetUseAdd (null, 2, ref useInfo, out paramErrorIndex);
        m_lastError = (int)returncode;
        return (int)returncode;
      }
      catch {
        m_lastError = Marshal.GetLastWin32Error ();
        return -1;
      }
    }
    /// <summary>
    /// close the unc network share
    /// </summary>
    /// <returns></returns>
    public bool NetUseDelete ()
    {
      uint returncode;
      try {
        returncode = NetUseDel (null, m_UNCPath, 2);
        m_lastError = (int)returncode;
        return (returncode == 0);
      }
      catch {
        m_lastError = Marshal.GetLastWin32Error ();
        return false;
      }
    }
  }
}