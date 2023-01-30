// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;

namespace Lemoine.Business.CncValue
{
  /// <summary>
  /// Specific implementation of <see cref="Lemoine.ModelDAO.ICncValueColorDAO">ICncValueColorDAO</see>
  /// for the red stack light
  /// </summary>
  public class RedStackLightDAO
    : CncValueColorDAO
    , ICncValueColorDAO
  {
    static readonly string MAX_GAP_KEY = "Business.CncValue.RedStackLight.MaxMergeGap";
    static readonly TimeSpan MAX_GAP_DEFAULT = TimeSpan.FromSeconds (0);

    readonly ILog log = LogManager.GetLogger (typeof (RedStackLightDAO).FullName);

    bool IsRedStackLight (ICncValue cncValue)
    {
      if (null == cncValue) {
        return false;
      }

      try {
        var stackLight = (StackLight)cncValue.Value;
        return stackLight.IsOnOrFlashingIfAcquired (StackLightColor.Red);
      }
      catch (Exception ex) {
        log.Error ("IsRedStackLight: exception", ex);
        log.ErrorFormat ("IsRedStackLight: {0} is not a valid stack light", cncValue.Value);
        return false;
      }
    }

    /// <summary>
    /// Create a ICncValueColor from a field and a cncValue
    /// </summary>
    /// <param name="field">not null</param>
    /// <param name="cncValue">not null</param>
    /// <returns></returns>
    protected override ICncValueColor CreateCncValueColor (IField field, ICncValue cncValue)
    {
      Debug.Assert (null != field);
      Debug.Assert (null != cncValue);

      return new CncValueColor (cncValue.MachineModule,
        field,
        "#FF0000", // Red
        cncValue.DateTimeRange,
        cncValue.DateTimeRange.Duration);
    }

    /// <summary>
    /// Find a ICncValue at a specific time
    /// </summary>
    /// <param name="machineModule">not null</param>
    /// <param name="field">not null</param>
    /// <param name="dateTime"></param>
    /// <returns></returns>
    protected override ICncValue FindAt (IMachineModule machineModule, IField field, DateTime dateTime)
    {
      Debug.Assert (null != machineModule);
      Debug.Assert (null != field);

      var cncValue = ModelDAOHelper.DAOFactory.CncValueDAO
        .FindAt (machineModule, field, dateTime);
      if (null == cncValue) {
        return null;
      }
      if (!IsRedStackLight (cncValue)) {
        return null;
      }
      return cncValue;
    }

    /// <summary>
    /// Find the ICncValue in a specific date/time range
    /// </summary>
    /// <param name="machineModule"></param>
    /// <param name="field"></param>
    /// <param name="range"></param>
    /// <returns></returns>
    protected override IEnumerable<ICncValue> FindByMachineFieldDateRange (IMachineModule machineModule, IField field, UtcDateTimeRange range)
    {
      Debug.Assert (null != machineModule);
      Debug.Assert (null != field);

      return ModelDAOHelper.DAOFactory.CncValueDAO
        .FindByMachineFieldDateRange (machineModule, field, range)
        .Where (v => IsRedStackLight (v));
    }

    /// <summary>
    /// Find the ICncValue with a specific end
    /// </summary>
    /// <param name="machineModule"></param>
    /// <param name="field"></param>
    /// <param name="dateTime"></param>
    /// <returns></returns>
    protected override ICncValue FindWithEnd (IMachineModule machineModule, IField field, DateTime dateTime)
    {
      Debug.Assert (null != machineModule);
      Debug.Assert (null != field);

      var cncValue = ModelDAOHelper.DAOFactory.CncValueDAO
        .FindWithEnd (machineModule, field, dateTime);
      if (null == cncValue) {
        return null;
      }
      if (!IsRedStackLight (cncValue)) {
        return null;
      }
      return cncValue;
    }

    /// <summary>
    /// Get the maximum gap
    /// </summary>
    /// <returns></returns>
    protected override TimeSpan GetMaxGap ()
    {
      return Lemoine.Info.ConfigSet.LoadAndGet<TimeSpan> (MAX_GAP_KEY,
                                                          MAX_GAP_DEFAULT);
    }
  }
}
