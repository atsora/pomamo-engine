// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Database.Persistent;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using NHibernate;
using System.Diagnostics;
using NHibernate.Criterion;
using Lemoine.Collections;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Implementation of <see cref="Lemoine.ModelDAO.IStampingValueDAO">IStampingValueDAO</see>
  /// </summary>
  public class StampingValueDAO
    : VersionableNHibernateDAO<StampingValue, IStampingValue, int>
    , IStampingValueDAO
  {
    /// <summary>
    /// Find the stamping value that matches the sequence and the field
    /// </summary>
    /// <param name="sequence">not null</param>
    /// <param name="field">not null</param>
    /// <returns></returns>
    public IStampingValue Find (ISequence sequence, IField field)
    {
      Debug.Assert (null != sequence);
      Debug.Assert (null != field);

      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<StampingValue> ()
        .Add (Restrictions.Eq ("Sequence.Id", ((IDataWithId<int>)sequence).Id))
        .Add (Restrictions.Eq ("Field.Id", ((IDataWithId<int>)field).Id))
        .SetCacheable (true)
        .UniqueResult<IStampingValue> ();
    }
  }
}
