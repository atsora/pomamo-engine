// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Linq;
using Lemoine.Database.Persistent;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using NHibernate;
using NHibernate.Criterion;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Implementation of <see cref="Lemoine.ModelDAO.IMachineCncVariableDAO">IMachineCncVariableDAO</see>
  /// </summary>
  public class MachineCncVariableDAO
    : VersionableNHibernateDAO<MachineCncVariable, IMachineCncVariable, int>
    , IMachineCncVariableDAO
  {
    /// <summary>
    /// 
    /// Note: this is registered to be cacheable
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public IMachineCncVariable FindByKeyValue (IMachine machine, string key, object value)
    {
      var keyValueMatch = NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<MachineCncVariable> ()
        .Add (Restrictions.Eq ("CncVariableKey", key))
        .Add (Restrictions.Eq ("CncVariableValue", value))
        .SetCacheable (true)
        .List<IMachineCncVariable> ();
      return keyValueMatch
        .FirstOrDefault (c => c.MachineFilter.IsMatch (machine));
    }
  }
}
