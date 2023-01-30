// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;
using Lemoine.Database.Persistent;
using Lemoine.Model;
using Lemoine.ModelDAO;
using NHibernate.Criterion;
using Lemoine.Core.Log;
using NHibernate;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Implementation of <see cref="Lemoine.ModelDAO.IMachineStateTemplateFlowDAO">IMachineStateTemplateFlowDAO</see>
  /// </summary>
  public class MachineStateTemplateFlowDAO
    : VersionableNHibernateDAO<MachineStateTemplateFlow, IMachineStateTemplateFlow, int>
    , IMachineStateTemplateFlowDAO
  {
    /// <summary>
    /// Implements <see cref="IMachineStateTemplateFlowDAO" />
    /// </summary>
    /// <param name="machineStateTemplate"></param>
    /// <returns></returns>
    public IEnumerable<IMachineStateTemplate> FindNext(IMachineStateTemplate machineStateTemplate)
    {
      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<MachineStateTemplateFlow> ()
        .Add (Restrictions.Eq ("From", machineStateTemplate))
        .List<IMachineStateTemplateFlow> ()
        .Select (flow => flow.To);
    }
  }
}
