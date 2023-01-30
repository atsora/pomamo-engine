// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Database.Persistent;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using NHibernate.Criterion;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Description of ComponentMachineAssociationDAO.
  /// </summary>
  public class ComponentMachineAssociationDAO
    : SaveOnlyByMachineNHibernateDAO<ComponentMachineAssociation, IComponentMachineAssociation, long>
    , IComponentMachineAssociationDAO
  {
    static readonly ILog log = LogManager.GetLogger(typeof (ComponentMachineAssociationDAO).FullName);
    
    /// <summary>
    /// Find all the component machine associations for a specified component
    /// </summary>
    /// <param name="component"></param>
    /// <returns></returns>
    public IList<IComponentMachineAssociation> FindAllWithComponent (IComponent component)
    {
      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<ComponentMachineAssociation> ()
        .Add (Restrictions.Eq ("Component", component))
        .List<IComponentMachineAssociation> ();
    }
  }
}
