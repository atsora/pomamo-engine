// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;

using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using Lemoine.Core.Log;

namespace Lemoine.Info
{
  /// <summary>
  /// Get some information on the running computer
  /// </summary>
  public sealed class ComputerInfo
  {
#if NETSTANDARD
    static readonly string TOTAL_PHYSICAL_MEMORY_KEY = "TotalPhysicalMemory";
    static readonly int TOTAL_PHYSICAL_MEMORY_DEFAULT = 0; // 0: not set
#endif // NETSTANDARD

    static readonly Regex MEM_TOTAL_REGEX = new Regex ("^MemTotal: *(\\d+) *kB");

    static readonly ILog log = LogManager.GetLogger (typeof (ComputerInfo).FullName);

    ulong m_totalPhysicalMemory = 0; // 0: not known yet, -1: exception
    Exception m_totalPhysicalMemoryException = null;

    #region Getters / Setters
    /// <summary>
    /// Total physical memory
    /// </summary>
    /// <exception cref="Exception"></exception>
    public static ulong TotalPhysicalMemory
    {
      get {
        if (null != Instance.m_totalPhysicalMemoryException) {
          log.Error ($"TotalPhysicalMemory.get: not available because of previous exception", Instance.m_totalPhysicalMemoryException);
          throw new Exception ("Total physical memory is not available", Instance.m_totalPhysicalMemoryException);
        }
        if (0 == Instance.m_totalPhysicalMemory) {
          try {
            Instance.m_totalPhysicalMemory = GetTotalPhysicalMemory ();
          }
          catch (Exception ex) {
            log.Error ("TotalPhysicalMemory.get: exception", ex);
            Instance.m_totalPhysicalMemoryException = ex;
            throw;
          }
        }
        return Instance.m_totalPhysicalMemory;
      }
    }
    #endregion // Getters / Setters

    #region Network
    /// <summary>
    /// Get a list of all the possible name of the current computer
    /// </summary>
    /// <returns></returns>
    public static IEnumerable<string> GetNames ()
    {
      // 1. Add the content of the environment variable %PulseComputerName%
      string pulseComputerName = System.Environment.GetEnvironmentVariable ("PulseComputerName");
      if (!string.IsNullOrEmpty (pulseComputerName)) {
        if (log.IsDebugEnabled) {
          log.Debug ($"GetNames: add {pulseComputerName} from the environment variable %PulseComputerName%");
        }
        yield return pulseComputerName;
      }

      // 2. Add the NetBIOS name
      var machineName = "";
      try {
        machineName = System.Environment.MachineName;
      }
      catch (Exception ex) {
        log.Warn ($"GetNames: getting the NetBIOS name failed with error", ex);
      }
      if (!string.IsNullOrEmpty (machineName)) {
        if (log.IsDebugEnabled) {
          log.Debug ($"GetNames: return the NetBIOS name {machineName}");
        }
        yield return machineName;
      }

      // 3. Add the DNS name
      var hostName = "";
      var hostWithDomain = "";
      try {
        hostName = System.Net.Dns.GetHostName ();
      }
      catch (Exception ex) {
        log.Warn ($"GetNames: getting the DNS name failed with error", ex);
      }
      if (!string.IsNullOrEmpty (hostName)) {
        if (!string.Equals (machineName, hostName, StringComparison.InvariantCultureIgnoreCase)) {
          if (log.IsDebugEnabled) {
            log.Debug ($"GetNames: return the host name {hostName}");
          }
          yield return hostName;
        }
        else if (log.IsDebugEnabled) {
          log.Debug ($"GetNames: host name {hostName} matches the NetBIOS name {machineName}");
        }
        if (!hostName.Contains (".")) { // Another option adding the domain if available
          try {
            var domain = System.DirectoryServices.ActiveDirectory.Domain.GetComputerDomain ().Name;
            if (!string.IsNullOrEmpty (domain)) {
              hostWithDomain = $"{hostName}.{domain}";
            }
          }
          catch (Exception ex1) {
            log.Warn ($"GetNames: domain could not be read", ex1);
          }
          if (!string.IsNullOrEmpty (hostWithDomain)) {
            if (log.IsDebugEnabled) {
              log.Debug ($"GetNames: return hostName.domain {hostWithDomain}");
            }
            yield return hostWithDomain;
          }
        }
      }

      // 4. Full DNS name
      var fullDnsName = "";
      try {
        fullDnsName = System.Net.Dns.GetHostEntry ("").HostName;
      }
      catch (Exception ex) {
        log.Warn ($"GetNames: full dns name could not be retrieved", ex);
      }
      if (!string.IsNullOrEmpty (fullDnsName) && !string.Equals (hostWithDomain, fullDnsName, StringComparison.InvariantCultureIgnoreCase)) {
        if (log.IsDebugEnabled) {
          log.Debug ($"GetNames: return full DNS Name {fullDnsName}");
        }
        yield return fullDnsName;
      }
      else if (log.IsDebugEnabled) {
        log.Debug ($"GetNames: full DNS Name {fullDnsName} may match the host.domain {hostWithDomain}");
      }

      // 5. Add the IP addresses
      foreach (var ipAddress in GetIPAddresses ()) {
        yield return ipAddress;
      }
    }

    /// <summary>
    /// Get a list of all the possible IP Addresses of the current computer
    /// 
    /// Exclude the loopback IP addresses
    /// </summary>
    /// <returns></returns>
    public static IEnumerable<string> GetIPAddresses ()
    {
      IPAddress[] ipAddresses;
      try {
        ipAddresses = Dns.GetHostAddresses (Dns.GetHostName ());
      }
      catch (Exception ex) {
        log.Warn ($"GetIPAddresses: getting the IP addresses failed with error", ex);
        yield break;
      }

      foreach (IPAddress ipAddress in ipAddresses) {
        bool isLoopback = true;
        try {
          isLoopback = IPAddress.IsLoopback (ipAddress);
        }
        catch (Exception ex) {
          log.Error ($"GetIpAddress: isLoopback of {ipAddress} failed", ex);
        }
        if (!isLoopback) {
          if (log.IsDebugEnabled) {
            log.Debug ($"GetIPAddresses: return IP address {ipAddress.ToString ()}");
          }
          yield return ipAddress.ToString ();
        }
      }
    }

    /// <summary>
    /// Check if a specific address is a local address
    /// </summary>
    /// <param name="address"></param>
    /// <returns></returns>
    public static bool IsLocal (string address)
    {
      var ipAddresses = Lemoine.Net.NetworkAddress.GetIPAddresses (address);
      return ipAddresses.Any (x => x.Equals (IPAddress.IPv6Loopback) || x.Equals (IPAddress.Loopback));
    }
    #endregion // Network

    #region Memory
    [StructLayout (LayoutKind.Sequential, CharSet = CharSet.Auto)]
    class MEMORYSTATUSEX
    {
      public uint dwLength;
      public uint dwMemoryLoad;
      public ulong ullTotalPhys;
      public ulong ullAvailPhys;
      public ulong ullTotalPageFile;
      public ulong ullAvailPageFile;
      public ulong ullTotalVirtual;
      public ulong ullAvailVirtual;
      public ulong ullAvailExtendedVirtual;
      public MEMORYSTATUSEX ()
      {
        this.dwLength = (uint)Marshal.SizeOf (typeof (MEMORYSTATUSEX));
      }
    }

    [return: MarshalAs (UnmanagedType.Bool)]
    [DllImport ("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    static extern bool GlobalMemoryStatusEx ([In, Out] MEMORYSTATUSEX lpBuffer);

    /// <summary>
    /// Get the total physical memory in bytes
    /// </summary>
    /// <returns></returns>
    /// <exception cref="PlatformNotSupportedException">if the platform does not support it</exception>
    static ulong GetTotalPhysicalMemory ()
    {
#if !NETSTANDARD
      if (true) {
#else // NETSTANDARD
      if (RuntimeInformation.IsOSPlatform (OSPlatform.Windows)) {
#endif // NETSTANDARD
        MEMORYSTATUSEX memStatus = new MEMORYSTATUSEX ();
        if (GlobalMemoryStatusEx (memStatus)) {
          return memStatus.ullTotalPhys;
        }
        else {
          log.ErrorFormat ("GetTotalPhysicalMemory: " +
                           "GlobalMemoryStatusEx failed");
          throw new InvalidOperationException ("GlobalMemoryStatusEx");
        }
      }
#if NETSTANDARD
      else if (RuntimeInformation.IsOSPlatform (OSPlatform.Linux)) {
        var totalPhysicalMemoryConfig = Lemoine.Info.ConfigSet.LoadAndGet (TOTAL_PHYSICAL_MEMORY_KEY, TOTAL_PHYSICAL_MEMORY_DEFAULT);
        if (0 < totalPhysicalMemoryConfig) {
          if (log.IsDebugEnabled) {
            log.Debug ($"GetTotalPhysicalMemory: consider {totalPhysicalMemoryConfig} bytes from config");
          }
          return (ulong)totalPhysicalMemoryConfig;
        }
        else {
          // Try to use /proc/meminfo and parse it
          // For example $ more /proc/meminfo may return
          // MemTotal:           8049528 kB
          var meminfoPath = "/proc/meminfo";
          if (System.IO.File.Exists (meminfoPath)) {
            using (var reader = new StreamReader (meminfoPath)) {
              while (!reader.EndOfStream) {
                var line = reader.ReadLine ();
                if (line.StartsWith ("MemTotal:")) {
                  var match = MEM_TOTAL_REGEX.Match (line);
                  if (!match.Success) {
                    log.Error ($"GetTotalPhysicalMemory: wrong regex {MEM_TOTAL_REGEX} for line {line}");
                    throw new InvalidDataException ("Unexpected line in /proc/meminfo");
                  }
                  var group = match.Groups[1];
                  Debug.Assert (group.Success);
                  var v = group.Value;
                  var totalMemoryKb = int.Parse (v);
                  if (log.IsDebugEnabled) {
                    log.Debug ($"GetTotalPhysicalMemory: {totalMemoryKb} kB on Linux");
                  }
                  return (ulong)(1024 * (long)totalMemoryKb);
                }
              }
            }
            log.Error ($"GetTotalPhysicalMemory: MemTotal not found in {meminfoPath}");
            throw new InvalidDataException ("MemTotal not found in /proc/meminfo");
          }
          else {
            log.Error ($"GetTotalPhysicalMemory: not fully implemented on Linux, no config is set and {meminfoPath} does not exist");
            throw new PlatformNotSupportedException ();
          }
        }
      }
      else {
        log.Error ($"GetTotalPhysicalMemory: not implemented for this platform");
        throw new PlatformNotSupportedException ();
      }
#endif // NETSTANDARD
    }
    #endregion // Memory

    /// <summary>
    /// Private constructor (singleton class !)
    /// </summary>
    private ComputerInfo ()
    {
    }

    #region Instance
    static ComputerInfo Instance
    {
      get { return Nested.instance; }
    }

    class Nested
    {
      // Explicit static constructor to tell C# compiler
      // not to mark type as beforefieldinit
      static Nested ()
      {
      }

      internal static readonly ComputerInfo instance = new ComputerInfo ();
    }
    #endregion
  }
}
