// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Database.Persistent;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using NHibernate;
using NHibernate.Criterion;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Implementation of <see cref="Lemoine.ModelDAO.IMachineStateTemplateAssociationDAO">IMachineStateTemplateAssociationDAO</see>
  /// </summary>
  public class MachineStateTemplateAssociationDAO
    : SaveOnlyByMachineNHibernateDAO<MachineStateTemplateAssociation, IMachineStateTemplateAssociation, long>
    , IMachineStateTemplateAssociationDAO
  {
    /// <summary>
    /// Get all MachineStateTemplateAssociation for a specific machine within a period
    /// Valid segments have:
    /// - their beginning strictly inferior to the end of the period, AND
    /// - their end strictly superior to the beginning of the period
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <returns></returns>
    public IList<IMachineStateTemplateAssociation> FindByMachineAndPeriod(IMachine machine, DateTime start, DateTime end)
    {
      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<MachineStateTemplateAssociation> ()
        .Add (Restrictions.Eq ("ModificationMachine", machine))
        .Add (Restrictions.Eq ("ModificationStatusMachine", machine))
        .Add (Restrictions.Eq ("Machine", machine))
        .Add (Restrictions.IsNull ("ParentGlobal"))
        .Add (Restrictions.IsNull ("ParentMachine"))
        .Add (Restrictions.Lt ("Begin", end))
        .Add (Restrictions.Or (
          Restrictions.IsNull ("End"),
          Restrictions.Gt ("End", start)))
        .List<IMachineStateTemplateAssociation> ();
    }
  }
}
