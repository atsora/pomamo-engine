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
  /// Implementation of <see cref="Lemoine.ModelDAO.IComponentIntermediateWorkPieceUpdateDAO">IComponentIntermediateWorkPieceUpdateDAO</see>
  /// </summary>
  public class ComponentIntermediateWorkPieceUpdateDAO
    : SaveOnlyNHibernateDAO<ComponentIntermediateWorkPieceUpdate, IComponentIntermediateWorkPieceUpdate, long>
    , IComponentIntermediateWorkPieceUpdateDAO
  {
    static readonly ILog log = LogManager.GetLogger(typeof (ComponentIntermediateWorkPieceUpdateDAO).FullName);
    
    /// <summary>
    /// Find all the ComponentIntermediateWorkPieceUpdate for a specified component
    /// </summary>
    /// <param name="component"></param>
    /// <returns></returns>
    public IList<IComponentIntermediateWorkPieceUpdate> FindAllWithComponent (IComponent component)
    {
      return NHibernateHelper.GetCurrentSession ().CreateCriteria<ComponentIntermediateWorkPieceUpdate> ()
        .Add (Restrictions.Eq ("Component", component))
        .List<IComponentIntermediateWorkPieceUpdate> ();
    }

    /// <summary>
    /// Find all the ComponentIntermediateWorkPieceUpdate for a specified IntermediateWorkPiece
    /// </summary>
    /// <param name="intermediateWorkPiece"></param>
    /// <returns></returns>
    public IList<IComponentIntermediateWorkPieceUpdate> FindAllWithIntermediateWorkPiece (IIntermediateWorkPiece intermediateWorkPiece)
    {
      return NHibernateHelper.GetCurrentSession ().CreateCriteria<ComponentIntermediateWorkPieceUpdate> ()
        .Add (Restrictions.Eq ("IntermediateWorkPiece", intermediateWorkPiece))
        .List<IComponentIntermediateWorkPieceUpdate> ();
    }
  }
}
