// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Database.Persistent;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using System.Diagnostics;
using NHibernate.Criterion;
using System.Collections.Generic;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Implementation of <see cref="Lemoine.ModelDAO.ICncVariableDAO">ICncVariableDAO</see>
  /// </summary>
  public class CncVariableDAO
    : VersionableByMachineModuleNHibernateDAO<CncVariable, ICncVariable, int>
    , ICncVariableDAO
  {
    /// <summary>
    /// Constructor
    /// </summary>
    public CncVariableDAO()
      : base("MachineModule") {}

    /// <summary>
    /// Find the unique slot at the specified UTC date/time
    /// </summary>
    /// <param name="machineModule">not null</param>
    /// <param name="key">not null and not empty</param>
    /// <param name="dateTime">in UTC</param>
    /// <returns></returns>
    public virtual ICncVariable FindAt (IMachineModule machineModule, string key, DateTime dateTime)
    {
      Debug.Assert (null != machineModule);
      Debug.Assert (DateTimeKind.Utc == dateTime.Kind);
      Debug.Assert (!string.IsNullOrEmpty (key));

      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<CncVariable> ()
        .Add (Restrictions.Eq ("MachineModule", machineModule))
        .Add (Restrictions.Eq ("Key", key))
        // Note: new SimpleExpression ("DateTimeRange", dateTime, "@>") does not work because it compares object of different types
        // The returned error is: Type mismatch in NHibernate.Criterion.SimpleExpression: DateTimeRange expected type Lemoine.Model.UtcDateTimeRange, actual type System.DateTime
        .Add (new SimpleTypedExpression ("DateTimeRange", new Lemoine.NHibernateTypes.UTCDateTimeFullType (), dateTime, "@>"))
        .UniqueResult<ICncVariable> ();
    }

    /// <summary>
    /// Find all the slots in the specified UTC date/time range
    /// </summary>
    /// <param name="machineModule">not null</param>
    /// <param name="key">not null and not empty</param>
    /// <param name="range">in UTC</param>
    /// <returns></returns>
    public virtual IList<ICncVariable> FindOverlapsRange (IMachineModule machineModule, string key, UtcDateTimeRange range)
    { 
      Debug.Assert (null != machineModule);
      Debug.Assert (!string.IsNullOrEmpty (key));

      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<CncVariable> ()
        .Add (Restrictions.Eq ("MachineModule", machineModule))
        .Add (Restrictions.Eq ("Key", key))
        .Add (InUtcRange (range))
        .List<ICncVariable> ();
    }

    /// <summary>
    /// Range criterion with UTC date/times
    /// </summary>
    /// <param name="range"></param>
    /// <returns></returns>
    protected virtual AbstractCriterion InUtcRange (UtcDateTimeRange range)
    {
      return new SimpleExpression ("DateTimeRange", range, "&&");
    }
  }
}
