// Copyright (C) 2009-2023 Lemoine Automation Technologies
// Copyright (C) 2025 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;

using System.Diagnostics;
using Lemoine.Core.Cache;
using Lemoine.Business;
using Lemoine.Core.Log;
using Lemoine.Model;
using Lemoine.ModelDAO;
using System.Threading.Tasks;

namespace Lemoine.Business.Operation
{
  /// <summary>
  /// Request class to get a factor on part production target
  /// </summary>
  public sealed class PartProductionTargetFactor
    : IRequest<double>
  {
    readonly IMachine m_machine;

    static readonly ILog log = LogManager.GetLogger (typeof (PartProductionTargetFactor).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="machine">not null</param>
    public PartProductionTargetFactor (IMachine machine)
    {
      Debug.Assert (null != machine);

      m_machine = machine;
    }

    #region IRequest implementation
    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns></returns>
    public double Get ()
    {
      double adjustment = 1.0;
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        IGoal goal = ModelDAOHelper.DAOFactory.GoalDAO
          .FindMatch (GoalTypeId.QuantityVsProductionCycleDuration, null, m_machine);
        if (null != goal) { // Note: goal is in % (between 1 and 100)
          if (log.IsInfoEnabled) {
            log.Info ($"GetAdjustment: goal is {goal.Value}%");
          }
          adjustment = goal.Value / 100.0;
        }
      }
      return adjustment;
    }


    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns></returns>
    public async Task<double> GetAsync ()
    {
      return await Task.FromResult (Get ());
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns></returns>
    public string GetCacheKey ()
    {
      return "Business.Operation.PartProductionTargetFactor." + m_machine.Id;
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns></returns>
    public TimeSpan GetCacheTimeout (double data)
    {
      return CacheTimeOut.Config.GetTimeSpan ();
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public bool IsCacheValid (CacheValue<double> data)
    {
      return true;
    }
    #endregion // IRequest implementation
  }
}
