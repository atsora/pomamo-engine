// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Collections;
using Lemoine.Core.Log;
using Lemoine.Model;

namespace Lemoine.Extensions.Business.Group.Impl
{
  /// <summary>
  /// Class to combine IPartProductionDataCurrentShift data
  /// considering they are run in a concurrent way.
  /// 
  /// Note that only the latest day/shift of the input data is considered here.
  /// </summary>
  public class PartProductionDataCurrentShiftConcurrent
    : IPartProductionDataCurrentShift
  {
    ILog log = LogManager.GetLogger (typeof (PartProductionDataCurrentShiftConcurrent).FullName);

    readonly IEnumerable<IPartProductionDataCurrentShift> m_items;
    readonly DateTime? m_day;
    readonly IShift m_shift;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="items">not empty</param>
    public PartProductionDataCurrentShiftConcurrent (IEnumerable<IPartProductionDataCurrentShift> items)
    {
      if (!items.Any ()) {
        m_items = items;
      }
      else {
        var lastItem = items
          .OrderByDescending (i => i.DateTime)
          .First ();
        m_day = lastItem.Day;
        m_shift = lastItem.Shift;
        m_items = items
          .Where (i => Comparison.EqualsNullable (i.Day, m_day, (a, b) => DateTime.Equals (a, b)))
          .Where (i => Comparison.EqualsNullable (i.Shift, m_shift, (a, b) => a.Id == b.Id));
      }
    }

    /// <summary>
    /// <see cref="IPartProductionDataCurrentShift"/>
    /// </summary>
    public DateTime DateTime
    {
      get
      {
        if (m_items.Any ()) {
          return m_items.Max (i => i.DateTime);
        }
        else {
          if (log.IsDebugEnabled) {
            log.DebugFormat ("DateTime.get: no item, return now");
          }
          return DateTime.UtcNow;
        }
      }
    }

    /// <summary>
    /// <see cref="IPartProductionDataCurrentShift"/>
    /// </summary>
    public double? NbPiecesCurrentShift
    {
      get
      {
        var withPieces = m_items
          .Where (i => i.NbPiecesCurrentShift.HasValue);
        if (withPieces.Any ()) {
          return withPieces
            .Sum (i => i.NbPiecesCurrentShift.Value);
        }
        else {
          return null;
        }
      }
    }

    /// <summary>
    /// <see cref="IPartProductionDataCurrentShift"/>
    /// </summary>
    public double? GoalCurrentShift
    {
      get
      {
        var withGoal = m_items
          .Where (i => i.GoalCurrentShift.HasValue);
        if (withGoal.Any ()) {
          return withGoal.Sum (i => i.GoalCurrentShift.Value);
        }
        else {
          return null;
        }
      }
    }

    /// <summary>
    /// <see cref="IPartProductionDataCurrentShift"/>
    /// </summary>
    public IComponent Component
    {
      get
      {
        if (!m_items.Any ()) {
          return null;
        }
        else { // Any
          var component = m_items.First ().Component;
          if (m_items.Skip (1).All (i => Comparison.EqualsNullable (i.Component, component, (a, b) => ((IDataWithId)a).Id == ((IDataWithId)b).Id))) {
            return component;
          }
          else {
            return null;
          }
        }
      }
    }

    /// <summary>
    /// <see cref="IPartProductionDataCurrentShift"/>
    /// </summary>
    public IOperation Operation
    {
      get
      {
        if (!m_items.Any ()) {
          return null;
        }
        else { // Any
          var operation = m_items.First ().Operation;
          if (m_items.Skip (1).All (i => Comparison.EqualsNullable (i.Operation, operation, (a, b) => ((IDataWithId)a).Id == ((IDataWithId)b).Id))) {
            return operation;
          }
          else {
            return null;
          }
        }
      }
    }

    /// <summary>
    /// <see cref="IPartProductionDataCurrentShift"/>
    /// </summary>
    public DateTime? Day
    {
      get
      {
        return m_day;
      }
    }

    /// <summary>
    /// <see cref="IPartProductionDataCurrentShift"/>
    /// </summary>
    public IShift Shift
    {
      get
      {
        return m_shift;
      }
    }

    /// <summary>
    /// <see cref="IPartProductionDataCurrentShift"/>
    /// </summary>
    public UtcDateTimeRange Range
    {
      get
      {
        if (!m_items.Any ()) {
          return new UtcDateTimeRange ();
        }
        else if (m_items.Any (i => i.Range.IsEmpty ())) {
          return new UtcDateTimeRange ();
        }
        else { // All are not empty
          var firstItem = m_items.First ();
          if (m_items.Skip (1)
            .All (i => LowerBound.Equals (i.Range.Lower, firstItem.Range.Lower))) { // Same lower for everyone
            return m_items.Skip (1)
              .Aggregate (firstItem.Range, (a, i) => new UtcDateTimeRange (a.Union (i.Range)));
          }
          else {
            return new UtcDateTimeRange ();
          }
        }
      }
    }

    /// <summary>
    /// <see cref="IPartProductionDataCurrentShift"/>
    /// </summary>
    public TimeSpan? CycleDurationTarget
    {
      get {
        if (!m_items.Any ()) {
          return null;
        }
        else if (m_items.Any (i => !i.CycleDurationTarget.HasValue)) {
          return null;
        }
        else { // All have a value
          var firstItem = m_items.First ();
          if (m_items.Skip (1).All (i => object.Equals (i.CycleDurationTarget, firstItem.CycleDurationTarget))) {
            return firstItem.CycleDurationTarget;
          }
          else {
            return null;
          }
        }
      }
    }
  }
}
