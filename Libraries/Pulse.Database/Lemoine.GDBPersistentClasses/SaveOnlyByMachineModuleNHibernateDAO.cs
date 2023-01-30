// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Threading.Tasks;
using Lemoine.Core.Log;
using Lemoine.Database.Persistent;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Generic NHiberate DAO for the read-only classes, on which no update is possible
  /// and for tables that are partitioned by machine module
  /// 
  /// This DAO allows save and update actions.
  /// </summary>
  public abstract class SaveOnlyByMachineModuleNHibernateDAO<T, I, ID>
    : GenericByMachineModuleNHibernateDAO<T, I, ID>
    where T: class, I
    where I: Lemoine.Model.IPartitionedByMachineModule
  {
    static readonly ILog log = LogManager.GetLogger(typeof (SaveOnlyByMachineModuleNHibernateDAO<T, I, ID>).FullName);

    /// <summary>
    /// MakePersistent implementation
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    public override I MakePersistent (I entity)
    {
      NHibernateHelper.GetCurrentSession ()
        .Save (entity);
      return entity;
    }

    /// <summary>
    /// MakePersistent implementation
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    public override async Task<I> MakePersistentAsync (I entity)
    {
      await NHibernateHelper.GetCurrentSession ()
        .SaveAsync (entity);
      return entity;
    }
  }
}
