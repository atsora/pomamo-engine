// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using Lemoine.Database.Persistent;
using NHibernate;
using NHibernate.Criterion;
using System.Diagnostics;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Implementation of <see cref="Lemoine.ModelDAO.IComponentIntermediateWorkPieceDAO">IComponentIntermediateWorkPieceDAO</see>
  /// </summary>
  public class ComponentIntermediateWorkPieceDAO
    : VersionableNHibernateDAO<ComponentIntermediateWorkPiece, IComponentIntermediateWorkPiece, int>
    , IComponentIntermediateWorkPieceDAO
  {
    /// <summary>
    /// Find ComponentIwp by component and iwp
    /// </summary>
    /// <param name="component"></param>
    /// <param name="iwp"></param>
    /// <returns></returns>
    public IComponentIntermediateWorkPiece FindByComponentAndIwp(IComponent component, IIntermediateWorkPiece iwp)
    {
      return NHibernateHelper.GetCurrentSession ().CreateCriteria<ComponentIntermediateWorkPiece>()
        .Add(Restrictions.Eq("Component", component))
        .Add(Restrictions.Eq("IntermediateWorkPiece", iwp))
        .UniqueResult<IComponentIntermediateWorkPiece>();
    }

    /// <summary>
    /// <see cref="IComponentIntermediateWorkPieceDAO"/>
    /// </summary>
    /// <param name="component"></param>
    /// <param name="order"></param>
    /// <returns></returns>
    public IList<IComponentIntermediateWorkPiece> FindWithComponentOrder (IComponent component, int order)
    {
      Debug.Assert (null != component);

      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<ComponentIntermediateWorkPiece> ()
        .Add (Restrictions.Eq ("Component.Id", component.Id))
        .Add (Restrictions.Eq ("Order", order))
        .Fetch (SelectMode.Fetch, "IntermediateWorkPiece")
        .List<IComponentIntermediateWorkPiece> ();
    }
  }
}
