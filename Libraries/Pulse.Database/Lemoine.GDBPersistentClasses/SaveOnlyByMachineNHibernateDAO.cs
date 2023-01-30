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
  /// and for tables that are partitioned by machine 
  /// 
  /// This DAO allows save and update actions.
  /// </summary>
  public abstract class SaveOnlyByMachineNHibernateDAO<T, I, ID>
    : GenericByMachineNHibernateDAO<T, I, ID>
    where T: class, I
    where I: Lemoine.Model.IPartitionedByMachine
  {
    // disable once StaticFieldInGenericType
    static readonly ILog log = LogManager.GetLogger(typeof (SaveOnlyByMachineNHibernateDAO<T, I, ID>).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    protected SaveOnlyByMachineNHibernateDAO ()
      : base ("Machine")
    { }
    
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
