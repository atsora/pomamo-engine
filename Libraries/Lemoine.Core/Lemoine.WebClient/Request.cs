// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using Lemoine.Core.Log;

namespace Lemoine.WebClient
{
  /// <summary>
  /// Set of pre-configured request
  /// </summary>
  public class Request
  {
    static readonly ILog log = LogManager.GetLogger (typeof (Request).FullName);

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public Request ()
    {
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Notify a configuration update
    /// </summary>
    /// <returns>success</returns>
    public static bool NotifyConfigUpdate ()
    {
      try {
        new Query ().Execute ("Config/NotifyUpdate");
      }
      catch (Exception ex) {
        log.Warn ("NotifyConfigUpdate: exception", ex);
        return false;
      }

      return true;
    }

    /// <summary>
    /// Request to clear a domain to the web service with the broadcast option
    /// </summary>
    /// <param name="domain"></param>
    /// <returns></returns>
    public static bool ClearDomain (string domain)
    {
      try {
        new Query ().Execute ("Cache/ClearDomain/" + domain + "?Broadcast=true");
      }
      catch (Exception ex) {
        log.Warn ("ClearDomain: exception", ex);
        return false;
      }

      return true;
    }
    #endregion // Methods
  }
}
