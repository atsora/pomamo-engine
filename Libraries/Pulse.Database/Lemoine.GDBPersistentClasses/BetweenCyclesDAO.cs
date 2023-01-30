// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Diagnostics;
using Lemoine.Database.Persistent;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using NHibernate;
using NHibernate.Criterion;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Implementation of <see cref="Lemoine.ModelDAO.IBetweenCyclesDAO">IBetweenCyclesDAO</see>
  /// </summary>
  public class BetweenCyclesDAO
    : VersionableByMachineNHibernateDAO<BetweenCycles, IBetweenCycles, int>
    , IBetweenCyclesDAO
  {
    readonly ILog log = LogManager.GetLogger(typeof (BetweenCyclesDAO).FullName);
    
    /// <summary>
    /// Constructor
    /// </summary>
    public BetweenCyclesDAO ()
      : base ("Machine")
    { }
    
    /// <summary>
    /// MakePersistent implementation
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    public override IBetweenCycles MakePersistent (IBetweenCycles entity)
    {
      Debug.Assert (0 == entity.Id);

      if (0 != entity.Id) {
        log.FatalFormat ("MakePersistent: " +
                         "Id {0} is already known",
                         entity.Id);
        throw new NotSupportedException ("SaveOnly element with an Id");
      }
      
      // Check the associated cycles are not transient
      Debug.Assert (null != entity.PreviousCycle);
      Debug.Assert (null != entity.NextCycle);
      Debug.Assert (0 != entity.PreviousCycle.Id);
      Debug.Assert (0 != entity.NextCycle.Id);
      if (0 == entity.PreviousCycle.Id) { // Fallback, make it persistent
        log.FatalFormat ("MakePersistent: " +
                         "transient associated PreviousCycle " +
                         "=> make PreviousCycle {0} persistent first",
                         entity.PreviousCycle);
        ModelDAOHelper.DAOFactory.OperationCycleDAO.MakePersistent (entity.PreviousCycle);
      }
      if (0 == entity.NextCycle.Id) { // Fallback, make it persistent
        log.FatalFormat ("MakePersistent: " +
                         "transient associated NextCycle " +
                         "=> make NextCycle {0} persistent first",
                         entity.NextCycle);
        ModelDAOHelper.DAOFactory.OperationCycleDAO.MakePersistent (entity.NextCycle);
      }
      
      BetweenCycles betweenCycles = (BetweenCycles) entity;
      betweenCycles.UpdateOffsetDuration ();
      NHibernateHelper.GetCurrentSession ().Save (betweenCycles);
      return betweenCycles;
    }

    /// <summary>
    /// MakePersistent implementation
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    public override async Task<IBetweenCycles> MakePersistentAsync (IBetweenCycles entity)
    {
      Debug.Assert (0 == entity.Id);

      if (0 != entity.Id) {
        log.FatalFormat ("MakePersistent: " +
                         "Id {0} is already known",
                         entity.Id);
        throw new NotSupportedException ("SaveOnly element with an Id");
      }

      // Check the associated cycles are not transient
      Debug.Assert (null != entity.PreviousCycle);
      Debug.Assert (null != entity.NextCycle);
      Debug.Assert (0 != entity.PreviousCycle.Id);
      Debug.Assert (0 != entity.NextCycle.Id);
      if (0 == entity.PreviousCycle.Id) { // Fallback, make it persistent
        log.FatalFormat ("MakePersistent: " +
                         "transient associated PreviousCycle " +
                         "=> make PreviousCycle {0} persistent first",
                         entity.PreviousCycle);
        await ModelDAOHelper.DAOFactory.OperationCycleDAO.MakePersistentAsync (entity.PreviousCycle);
      }
      if (0 == entity.NextCycle.Id) { // Fallback, make it persistent
        log.FatalFormat ("MakePersistent: " +
                         "transient associated NextCycle " +
                         "=> make NextCycle {0} persistent first",
                         entity.NextCycle);
        await ModelDAOHelper.DAOFactory.OperationCycleDAO.MakePersistentAsync (entity.NextCycle);
      }

      BetweenCycles betweenCycles = (BetweenCycles)entity;
      betweenCycles.UpdateOffsetDuration ();
      await NHibernateHelper.GetCurrentSession ().SaveAsync (betweenCycles);
      return betweenCycles;
    }

    /// <summary>
    /// Update the offset duration of the specified entity directly in database
    /// </summary>
    /// <param name="entity"></param>
    public void UpdateOffsetDuration (IBetweenCycles entity)
    {
      Debug.Assert (0 != entity.Id);

      if (0 == entity.Id) {
        log.FatalFormat ("UpdateOffsetDuration: " +
                         "update the offset duration of an unsaved entity " +
                         "=> fallback");
        BetweenCycles betweenCycles = (BetweenCycles) entity;
        betweenCycles.UpdateOffsetDuration ();
        NHibernateHelper.GetCurrentSession ().SaveOrUpdate (betweenCycles);
      }
      else {
        BetweenCycles betweenCycles = (BetweenCycles) entity;
        betweenCycles.UpdateOffsetDuration ();
        NHibernateHelper.GetCurrentSession ().Update (betweenCycles);
      }
    }
    
    /// <summary>
    /// Find all the operation cycles, returned by ascending Date/Time
    /// </summary>
    /// <returns></returns>
    public override IList<IBetweenCycles> FindAll ()
    {
      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<BetweenCycles> ()
        .AddOrder (Order.Asc ("End"))
        .List<IBetweenCycles> ();
    }

    /// <summary>
    /// Find all the operation cycles, returned by ascending Date/Time
    /// </summary>
    /// <returns></returns>
    public override async Task<IList<IBetweenCycles>> FindAllAsync ()
    {
      return await NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<BetweenCycles> ()
        .AddOrder (Order.Asc ("End"))
        .ListAsync<IBetweenCycles> ();
    }

    /// <summary>
    /// Find the BetweenCycles item with the specified previous operation cycle
    /// </summary>
    /// <param name="previousCycle">not null</param>
    /// <returns></returns>
    public IBetweenCycles FindWithPreviousCycle (IOperationCycle previousCycle)
    {
      Debug.Assert (null != previousCycle);
      Debug.Assert (null != previousCycle.Machine);
      
      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<BetweenCycles> ()
        .Add (Restrictions.Eq ("Machine.Id", previousCycle.Machine.Id)) // To take profit of the partitioning
        .Add (Restrictions.Eq ("PreviousCycle", previousCycle))
        .UniqueResult<IBetweenCycles> ();
    }

    /// <summary>
    /// Find the BetweenCycles item with the specified next operation cycle
    /// </summary>
    /// <param name="nextCycle">not null</param>
    /// <returns></returns>
    public IBetweenCycles FindWithNextCycle (IOperationCycle nextCycle)
    {
      Debug.Assert (null != nextCycle);
      Debug.Assert (null != nextCycle.Machine);

      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<BetweenCycles> ()
        .Add (Restrictions.Eq ("Machine.Id", nextCycle.Machine.Id)) // To take profit of the partitioning
        .Add (Restrictions.Eq ("NextCycle", nextCycle))
        .UniqueResult<IBetweenCycles> ();
    }
  }
}
