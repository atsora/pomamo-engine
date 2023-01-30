// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Lemoine.Core.Log;

namespace Lemoine.Database.Persistent
{
  /// <summary>
  /// Generic NHiberate DAO for the read-only classes, on which no update is possible
  /// 
  /// This specific DAO is for abstract classes.
  /// </summary>
  public abstract class ReadOnlyNHibernateDAO<T, I, ID>
    : GenericNHibernateDAO<T, I, ID>
    where T: class, I
  {
    static readonly ILog log = LogManager.GetLogger(typeof (ReadOnlyNHibernateDAO<T, I, ID>).FullName);

    /// <summary>
    /// No MakePersistent implementation
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    public override I MakePersistent (I entity)
    {
      log.FatalFormat ("MakePersistent: " +
                       "An abstract class can be made persistent");
      Debug.Assert (false);
      throw new NotImplementedException ();
    }

    /// <summary>
    /// No MakePersistent implementation
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    public override async Task<I> MakePersistentAsync (I entity)
    {
      log.FatalFormat ("MakePersistent: " +
                       "An abstract class can be made persistent");
      Debug.Assert (false);
      return await System.Threading.Tasks.Task.FromException<I> (new NotImplementedException ());
    }
  }
}
