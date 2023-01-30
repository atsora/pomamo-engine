// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using Lemoine.Core.Log;
using System;
using System.Linq;
using System.Net;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Sockets;

namespace Lemoine.Net
{
  /// <summary>
  /// Utility class to get some properties on network addresses
  /// </summary>
  public static class NetworkAddress
  {
    static readonly ILog log = LogManager.GetLogger (typeof (NetworkAddress).FullName);

    /// <summary>
    /// Is the address an IP address v4 ?
    /// </summary>
    /// <param name="s"></param>
    /// <returns></returns>
    public static bool IsIPAddressV4 (string s)
    {
      if (IPAddress.TryParse (s, out var ipAddress)) {
        if (log.IsDebugEnabled) {
          log.DebugFormat ("IsIPAddressV4: family of {0} is {1}",
            s, ipAddress.AddressFamily);
        }
        return ipAddress.AddressFamily.Equals (System.Net.Sockets.AddressFamily.InterNetwork);
      }
      else {
        if (log.IsDebugEnabled) {
          log.DebugFormat ("IsIPAddressV4: {0} is not an IP address", s);
        }
        return false;
      }
    }

    /// <summary>
    /// Get all the IP Addresses (starting with the address itself, if it is a valid address)
    /// </summary>
    /// <param name="address"></param>
    /// <returns></returns>
    public static IEnumerable<IPAddress> GetIPAddressesV4Only (string address)
    {
      return GetIPAddresses (address, x => x.AddressFamily.Equals (System.Net.Sockets.AddressFamily.InterNetwork));
    }

    /// <summary>
    /// Get the IP Addresses
    /// (starting with the address itself, if it is a valid IP address)
    /// </summary>
    /// <param name="address"></param>
    /// <returns></returns>
    public static IEnumerable<IPAddress> GetIPAddresses (string address)
    {
      return GetIPAddresses (address, (x) => true);
    }

    /// <summary>
    /// Get the IP Addresses
    /// (starting with the address itself, if it is a valid IP address)
    /// </summary>
    /// <param name="address">not null or empty</param>
    /// <param name="filter"></param>
    /// <returns></returns>
    public static IEnumerable<IPAddress> GetIPAddresses (string address, Func<IPAddress, bool> filter)
    {
      Debug.Assert (!string.IsNullOrEmpty (address));

      // 1. Directly the address if it is an IP Address
      {
        if (IPAddress.TryParse (address, out var ipAddress)) {
          if (log.IsDebugEnabled) {
            log.DebugFormat ("GetIPAddresses: family of {0} is {1}",
              address, ipAddress.AddressFamily);
          }
          if (filter (ipAddress)) {
            yield return ipAddress;
#if NETSTANDARD
            if (ipAddress.AddressFamily.Equals (AddressFamily.InterNetwork)) {
              yield return ipAddress.MapToIPv6 ();
            }
            else if (ipAddress.AddressFamily.Equals (AddressFamily.InterNetworkV6)
              && ipAddress.IsIPv4MappedToIPv6) {
              yield return ipAddress.MapToIPv4 ();
            }
#endif // NETSTANDARD
          }
        }
      }

      // 2. Get all the IP Address
      {
        IPAddress[] ipAddresses;
        try {
          ipAddresses = Dns.GetHostAddresses (address);
        }
        catch (Exception ex) {
          log.Error ("GetIPAddresses: GetHostAddresses failed for " + address, ex);
          yield break;
        }
        foreach (var ipAddress in ipAddresses) {
          yield return ipAddress;
#if NETSTANDARD
          if (ipAddress.AddressFamily.Equals (AddressFamily.InterNetwork)) {
            yield return ipAddress.MapToIPv6 ();
          }
          else if (ipAddress.AddressFamily.Equals (AddressFamily.InterNetworkV6)
            && ipAddress.IsIPv4MappedToIPv6) {
            yield return ipAddress.MapToIPv4 ();
          }
#endif // NETSTANDARD
        }
      }
    }
  }
}
