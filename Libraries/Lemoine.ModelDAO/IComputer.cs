// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using Lemoine.Core.Log;
using System;
using System.Diagnostics;
using System.Linq;

namespace Lemoine.Model
{
  /// <summary>
  /// Model for table Computer
  /// </summary>
  public interface IComputer : IDataWithIdentifiers, /*IVersionable, */IDisplayable, ISelectionable, IDataWithVersion, ISerializableModel
  {
    // Note: IComputer does not inherit from IVersionable
    //       else the corresponding properties can't be used in a DataGridView binding

    /// <summary>
    /// ID
    /// </summary>
    int Id { get; }

    /// <summary>
    /// Computer name
    /// </summary>
    string Name { get; set; }

    /// <summary>
    /// Computer address
    /// </summary>
    string Address { get; set; }

    /// <summary>
    /// Is this computer the LCtr ?
    /// </summary>
    bool IsLctr { get; set; }

    /// <summary>
    /// Is this computer a LPst ?
    /// </summary>
    bool IsLpst { get; set; }

    /// <summary>
    /// Is this computer a Cnc ?
    /// </summary>
    bool IsCnc { get; set; }

    /// <summary>
    /// Is a web service is running on this computer ?
    /// </summary>
    bool IsWeb { get; set; }

    /// <summary>
    /// Is this computer the auto-reason server ?
    /// </summary>
    bool IsAutoReason { get; set; }

    /// <summary>
    /// Is this computer the alert server ?
    /// </summary>
    bool IsAlert { get; set; }

    /// <summary>
    /// Is this computer the synchronization server ?
    /// </summary>
    bool IsSynchronization { get; set; }

    /// <summary>
    /// Web service URL
    /// 
    /// If empty or null, this is automatically generated from the address
    /// </summary>
    string WebServiceUrl { get; set; }
  }

  /// <summary>
  /// Extension to IComputer interface
  /// </summary>
  public static class ComputerExtension
  {
    static readonly ILog log = LogManager.GetLogger (typeof (ComputerExtension).FullName);

    /// <summary>
    /// Check if the local computer is the specified computer
    /// </summary>
    /// <param name="computer">not null</param>
    /// <returns></returns>
    public static bool IsLocal (this IComputer computer)
    {
      Debug.Assert (null != computer);
      if (null == computer) {
        log.ErrorFormat ("IsLocal: " +
                         "null input computer");
        throw new ArgumentNullException ("computer");
      }

      var computerNames = Lemoine.Info.ComputerInfo.GetNames ();
      foreach (var computerName in computerNames) {
        if (string.Equals (computerName, computer.Name, StringComparison.InvariantCultureIgnoreCase)) {
          log.DebugFormat ("IsLocal: " +
                           "name {0} matches",
                           computerName);
          return true;
        }
        if (string.Equals (computerName, computer.Address, StringComparison.InvariantCultureIgnoreCase)) {
          log.DebugFormat ("IsLocal: address {0} matches", computerName);
          return true;
        }
      }
      var localIpAddresses = Lemoine.Info.ComputerInfo.GetIPAddresses ();
      if (localIpAddresses.Any (x => string.Equals (x, computer.Address, StringComparison.InvariantCultureIgnoreCase))) {
        log.DebugFormat ("IsLocal: " +
                         "ip matches computer address {0}",
                         computer.Address);
        return true;
      }
      var ipAddresses = Lemoine.Net.NetworkAddress
        .GetIPAddresses (computer.Address)
        .Select (x => x.ToString ().ToLowerInvariant ());
      if (ipAddresses.Intersect (localIpAddresses.Select (x => x.ToLowerInvariant ())).Any ()) {
        log.DebugFormat ("IsLocal: " +
                         "ip matches an ip from computer address {0}",
                         computer.Address);
        return true;
      }
      log.Info ("IsLocal: no match, return false");
      return false;
    }
  }

}
