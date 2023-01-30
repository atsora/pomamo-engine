// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;

namespace WizardMonitorMachine
{
  /// <summary>
  /// Utilitary class
  /// </summary>
  static public class Util
  {
    #region Methods
    /// <summary>
    /// Try to ping
    /// </summary>
    /// <param name="hostOrIp">host or ip</param>
    /// <returns>the error or empty if success</returns>
    public static string Ping(string hostOrIp)
    {
      // Get an IP from a host
      string result = "";
      if (!hostOrIp.Contains(".")) {
        try {
          hostOrIp = GetHostIp(hostOrIp);
        } catch (Exception e) {
          result = e.Message;
        }
      }
      
      if (String.IsNullOrEmpty(result)) {var pingSender = new Ping();

        // Use the default Ttl value which is 128, but change the fragmentation behavior
        var options = new PingOptions();
        options.DontFragment = true;
        
        // Create a buffer of 32 bytes of data to be transmitted.
        const string data = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
        PingReply reply = pingSender.Send(hostOrIp, 200, Encoding.ASCII.GetBytes(data), options);
        
        switch (reply.Status) {
          case IPStatus.Success:
            result = "";
            break;
          case IPStatus.DestinationNetworkUnreachable:
            result = "destination network unreachable";
            break;
          case IPStatus.DestinationHostUnreachable:
            result = "destination host unreachable";
            break;
          case IPStatus.DestinationProtocolUnreachable:
            //case IPStatus.DestinationProhibited: // They have the same value
            result = (reply.Address.IsIPv6LinkLocal ||
                      reply.Address.IsIPv6Multicast ||
                      reply.Address.IsIPv6SiteLocal) ?
              "destination prohibited" : "destination network unreachable";
            break;
          case IPStatus.DestinationPortUnreachable:
            result = "destination port unreachable";
            break;
          case IPStatus.NoResources:
            result = "no resources";
            break;
          case IPStatus.BadOption:
            result = "bad option";
            break;
          case IPStatus.HardwareError:
            result = "hardware error";
            break;
          case IPStatus.PacketTooBig:
            result = "packet too big";
            break;
          case IPStatus.TimedOut:
            result = "timed out";
            break;
          case IPStatus.BadRoute:
            result = "bad route";
            break;
          case IPStatus.TtlExpired:
            result = "ttl (time to live) expired";
            break;
          case IPStatus.TtlReassemblyTimeExceeded:
            result = "ttl (time to live) reassembly time exceeded";
            break;
          case IPStatus.ParameterProblem:
            result = "parameter problem";
            break;
          case IPStatus.SourceQuench:
            result = "source quench";
            break;
          case IPStatus.BadDestination:
            result = "bad destination";
            break;
          case IPStatus.DestinationUnreachable:
            result = "destination unreachable";
            break;
          case IPStatus.TimeExceeded:
            result = "time exceeded";
            break;
          case IPStatus.BadHeader:
            result = "bad header";
            break;
          case IPStatus.UnrecognizedNextHeader:
            result = "unrecognized next header";
            break;
          case IPStatus.IcmpError:
            result = "ICMP (Internet Control Message Protocol) error";
            break;
          case IPStatus.DestinationScopeMismatch:
            result = "destination scope mismatch";
            break;
          case IPStatus.Unknown:
            result = "unknown reason";
            break;
        }
      }
      
      return String.IsNullOrEmpty(result) ? "" :
        String.Format("Failed to ping '{0}': {1}.", hostOrIp, result);
    }
    
    static string GetHostIp(string host)
    {
      try {
        var hostEntry = Dns.GetHostEntry(host);
        if (hostEntry.AddressList.Length > 0) {
          return hostEntry.AddressList[0].ToString();
        }
      } catch (Exception) {}
      throw new Exception("unknown host");
    }
    #endregion // Methods
  }
}
