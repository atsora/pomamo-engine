// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using Lemoine.Web;
using Lemoine.Extensions.Web.Responses;

namespace Pulse.Web.Time
{
  /// <summary>
  /// Current Day service
  /// </summary>
  public class DayToDateTimeRangeService
    : GenericCachedService<DayToDateTimeRangeRequestDTO>
  {
    static readonly ILog log = LogManager.GetLogger(typeof (DayToDateTimeRangeService).FullName);

    #region Getters / Setters
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public DayToDateTimeRangeService ()
      : base(Lemoine.Core.Cache.CacheTimeOut.Config)
    {
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Response to GET request (no cache)
    /// </summary>
    /// <returns></returns>
    public override object GetWithoutCache (DayToDateTimeRangeRequestDTO request)
    {
      DateTime day = ConvertDTO.IsoStringToDay (request.Day).Value;

      var response = new DayToDateTimeRangeResponseDTO ();
      
      IDaySlot dayToDateTimeRangeSlot;
      
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
        using (IDAOTransaction transaction = session.BeginTransaction ("Web.Time.DayToDateTimeRangeService")) // Read-write because of the days
      {
        dayToDateTimeRangeSlot = ModelDAOHelper.DAOFactory.DaySlotDAO.FindProcessedByDay (day);
        transaction.Commit ();
      }
      
      response.UtcDateTimeRange = dayToDateTimeRangeSlot.DateTimeRange.ToString (dt => ConvertDTO.DateTimeUtcToIsoString (dt));
      response.LocalDateTimeRange = dayToDateTimeRangeSlot.DateTimeRange.ToLocalTime ().ToString (dt => ConvertDTO.DateTimeLocalToIsoString (dt));
      return response;
    }
    #endregion // Methods
  }
}
