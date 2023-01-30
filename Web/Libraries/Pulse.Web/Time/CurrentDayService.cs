// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using System.Diagnostics;
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
  public class CurrentDayService
    : GenericCachedService<CurrentDayRequestDTO>
  {
    static readonly ILog log = LogManager.GetLogger(typeof (CurrentDayService).FullName);

    #region Getters / Setters
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public CurrentDayService ()
      : base(Lemoine.Core.Cache.CacheTimeOut.CurrentShort)
    {
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Response to GET request (no cache)
    /// </summary>
    /// <returns></returns>
    public override object GetWithoutCache (CurrentDayRequestDTO request)
    {
      CurrentDayResponseDTO response = new CurrentDayResponseDTO ();
      
      IDaySlot currentDaySlot;
      
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
        using (IDAOTransaction transaction = session.BeginTransaction ("Web.Time.CurrentDayService")) // Read-write because of the days
      {
        currentDaySlot = ModelDAOHelper.DAOFactory.DaySlotDAO.FindProcessedAt (DateTime.UtcNow);
        transaction.Commit ();
      }
      
      Debug.Assert (currentDaySlot.Day.HasValue);
      response.Day =  ConvertDTO.DayToIsoString (currentDaySlot.Day.Value);
      response.UtcDateTimeRange = currentDaySlot.DateTimeRange.ToString (dt => ConvertDTO.DateTimeUtcToIsoString (dt));
      response.LocalDateTimeRange = currentDaySlot.DateTimeRange.ToLocalTime ().ToString (dt => ConvertDTO.DateTimeLocalToIsoString (dt));
      return response;
    }
    #endregion // Methods
  }
}
