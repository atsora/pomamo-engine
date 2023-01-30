// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using Lemoine.Core.Log;
using Lemoine.Extensions.Web.Responses;
using Lemoine.Web;

namespace Pulse.Web.Time
{
  /// <summary>
  /// Current time service
  /// </summary>
  public class CurrentTimeService
    : GenericNoCacheService<CurrentTimeRequestDTO>
  {
    static readonly ILog log = LogManager.GetLogger(typeof (CurrentTimeService).FullName);

    #region Getters / Setters
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public CurrentTimeService ()
    {
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Get
    /// </summary>
    /// <returns></returns>
    public override object GetWithoutCache (CurrentTimeRequestDTO request)
    {
      CurrentTimeResponseDTO response = new CurrentTimeResponseDTO ();
      DateTime utcNow = DateTime.UtcNow;
      response.Utc = ConvertDTO.DateTimeUtcToIsoStringMs (utcNow);
      response.Local = ConvertDTO.DateTimeLocalToIsoStringMs (utcNow.ToLocalTime ());
      return response;
    }
    #endregion // Methods
  }
}
