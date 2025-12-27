// Copyright (C) 2009-2023 Lemoine Automation Technologies
// Copyright (C) 2024 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;

namespace Lemoine.Model
{
  /// <summary>
  /// Model of table ReasonSelection
  /// 
  /// This table lists all the possible reasons
  /// for a given Machine Mode and Machine Observation State.
  /// 
  /// A specific column allows to list the reasons
  /// that can be effectively selected by the user. 
  /// </summary>
  public interface IReasonSelection : IDataWithVersion
  {
    // Note: IReasonSelection does not inherit from IVersionable
    //       else the corresponding properties can't be used in a DataGridView binding

    /// <summary>
    /// ID
    /// </summary>
    int Id { get; }

    /// <summary>
    /// Classification ID :
    /// ReasonId or "MST" + MachineStateTemplateId
    /// </summary>
    string ClassificationId { get; }

    /// <summary>
    /// Alternative text (if null or empty, not taken into account)
    /// 
    /// nullable
    /// </summary>
    string AlternativeText { get; }

    /// <summary>
    /// Alternative long text (if null or empty, not taken into account)
    /// 
    /// nullable
    /// </summary>
    string AlternativeLongText { get; }

    /// <summary>
    /// Alternative description (if null or empty, not taken into account)
    /// 
    /// nullable
    /// </summary>
    string AlternativeDescription { get; }

    /// <summary>
    /// Additional data
    /// 
    /// Nullable
    /// </summary>
    IDictionary<string, object> Data { get; }

    /// <summary>
    /// Reference to the Machine Mode (not null)
    /// </summary>
    IMachineMode MachineMode { get; }

    /// <summary>
    /// Reference to the Machine Observation State (not null)
    /// </summary>
    IMachineObservationState MachineObservationState { get; }

    /// <summary>
    /// Reference to the Reason (Reason or MachineStateTemplate not null)
    /// </summary>
    IReason Reason { get; set; }

    /// <summary>
    /// Machine state template (Reason or MachineStateTemplate not null)
    /// </summary>
    IMachineStateTemplate MachineStateTemplate { get; }

    /// <summary>
    /// Associated reason group (not null)
    /// </summary>
    IReasonGroup ReasonGroup { get;  }

    /// <summary>
    /// Return a recommended reason score
    /// </summary>
    double ReasonScore { get; }

    /// <summary>
    /// Can this reason be selected by the user ? 
    /// </summary>
    bool Selectable { get; set; }

    /// <summary>
    /// Additional details are not allowed. Default: false
    /// </summary>
    bool NoDetails { get; set;  }

    /// <summary>
    /// If True, when this reason is selected, the operator must also enter a free detailed entry
    /// </summary>
    bool DetailsRequired { get; set; }

    /// <summary>
    /// Associated MachineFilter
    /// 
    /// If null, all machines apply
    /// </summary>
    IMachineFilter MachineFilter { get; set; }

    /// <summary>
    /// Is the data dependent on time ?
    /// </summary>
    bool TimeDependent { get; }

    /// <summary>
    /// Are some additional dynamic data set or required?
    /// </summary>
    bool DynamicData { get; }
  }

  /// <summary>
  /// Extensions to IReasonSelection
  /// </summary>
  public static class ReasonSelectionExtension
  {
    /// <summary>
    /// Group reason selections with the same reason
    /// </summary>
    /// <param name="reasonSelections"></param>
    /// <returns></returns>
    public static IEnumerable<IReasonSelection> GroupSameReason (this IEnumerable<IReasonSelection> reasonSelections)
    {
      return reasonSelections
        .GroupBy (s => $"{s.ClassificationId}-{s.AlternativeText ?? ""}")
        .Select (g => g.OrderByDescending (a => a.ReasonScore).First ());
    }
  }

  /// <summary>
  /// Equality comparer that considers:
  /// <item>the reason</item>
  /// <item>the reason score</item>
  /// <item>the details required property</item>
  /// <item>the alternative text</item>
  /// </summary>
  public class ReasonSelectionReasonEqualityComparer
    : IEqualityComparer<IReasonSelection>
  {
    /// <summary>
    /// <see cref="IEqualityComparer{T}"/>
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    public bool Equals (IReasonSelection x, IReasonSelection y)
    {
      return (string.Equals (x.ClassificationId, y.ClassificationId, StringComparison.InvariantCultureIgnoreCase))
        && (x.ReasonScore == y.ReasonScore)
        && (x.NoDetails == y.NoDetails)
        && (x.DetailsRequired == y.DetailsRequired)
        && string.Equals (x.AlternativeText, y.AlternativeText, StringComparison.InvariantCultureIgnoreCase)
        && ((!x.DynamicData && !y.DynamicData) || !string.IsNullOrEmpty (x.AlternativeText));
    }

    /// <summary>
    /// <see cref="IEqualityComparer{T}"/>
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public int GetHashCode (IReasonSelection obj)
    {
      int hashCode = 0;
      unchecked {
        hashCode += 1000000007 * obj.ClassificationId.GetHashCode ();
        hashCode += 1000000009 * obj.ReasonScore.GetHashCode ();
        hashCode += 1000000011 * obj.NoDetails.GetHashCode ();
        hashCode += 1000000013 * obj.DetailsRequired.GetHashCode ();
        hashCode += 1000000015 * (obj.AlternativeText?.GetHashCode () ?? 0);
        if (obj.DynamicData) { 
          hashCode += 1000000021;
        }
      }
      return hashCode;
    }
  }
}
