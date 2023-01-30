// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using Lemoine.Extensions.Web.Responses;
using Lemoine.Web;

namespace Pulse.Web.Cache
{
  /// <summary>
  /// Description of FlushCacheService
  /// </summary>
  public class ClearDaySlotCacheService
    : GenericNoCacheService<ClearDaySlotCacheRequestDTO>
  {
    static readonly ILog log = LogManager.GetLogger(typeof (ClearDaySlotCacheService).FullName);

    #region Constructors
    /// <summary>
    /// 
    /// </summary>
    public ClearDaySlotCacheService ()
    {
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Response to GET request (no cache)
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public override object GetWithoutCache(ClearDaySlotCacheRequestDTO request)
    {
      ModelDAOHelper.DAOFactory.DaySlotDAO.ClearCache ();

      return new OkDTO ("Day slot cache cleared");
    }
    #endregion // Methods
  }
}
