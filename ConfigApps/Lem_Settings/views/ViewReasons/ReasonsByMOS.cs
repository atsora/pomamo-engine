// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Model;

namespace ViewReasons
{
  /// <summary>
  /// Description of ReasonsByMOS.
  /// </summary>
  public class ReasonsByMOS: IComparable
  {
    #region Members
    int m_hashCode = 0;
    bool m_hashCodeComputed = false;
    #endregion // Members
    
    #region Getters / Setters
    /// <summary>
    /// One or several MOS sharing the same reasons
    /// </summary>
    public IList<IMachineObservationState> MOSS { get; private set; }
    
    /// <summary>
    /// Get the reasons associated to one or several MOS
    /// </summary>
    public List<OrderedReason> Reasons { get; private set; }
    #endregion // Getters / Setters

    #region Constructors
    public ReasonsByMOS()
    {
      MOSS = new List<IMachineObservationState>();
      Reasons = new List<OrderedReason>();
    }
    #endregion // Constructors
    
    #region Methods
    /// <summary>
    /// HashCode override
    /// </summary>
    /// <returns></returns>
    public override int GetHashCode()
    {
      if (!m_hashCodeComputed) {
        unchecked
        {
          m_hashCode = (int)20002061;
          foreach (var reason in Reasons) {
            m_hashCode = (m_hashCode * 16777619) ^ reason.GetHashCode();
          }
        }
        m_hashCodeComputed = true;
      }
      
      return m_hashCode;
    }
    
    /// <summary>
    /// Factorize both elements if possible
    /// Return true if success
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public bool Factorize(ReasonsByMOS other)
    {
      if (this.GetHashCode() == other.GetHashCode()) {
        foreach (var mos in other.MOSS) {
          this.MOSS.Add(mos);
        }

        return true;
      }
      return false;
    }
    
    /// <summary>
    /// Sort reasons
    /// </summary>
    public void Sort()
    {
      Reasons.Sort();
    }
    
    /// <summary>
    /// IComparable implementation
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public int CompareTo(object obj)
    {
      var otherReasonsByMos = obj as ReasonsByMOS;
      if (otherReasonsByMos == null) {
        return -1;
      }

      if (String.IsNullOrEmpty(this.MOSS[0].Name)) {
        return string.IsNullOrEmpty(otherReasonsByMos.MOSS[0].Name) ? 0 : -1;
      }

      return this.MOSS[0].Name.CompareTo(otherReasonsByMos.MOSS[0].Name);
    }
    
    public string GetWarning()
    {
      bool defaultDefinedWithNoConstraint = false;
      bool defaultDefined = false;
      bool overwriteRequired = false;
      bool selectionPossible = false;
      
      foreach (var reason in Reasons) {
        defaultDefined |= reason.IsDefault;
        defaultDefinedWithNoConstraint |= (reason.IsDefault && reason.MaxTime == null &&
                                           reason.MachineFilterExclude == null &&
                                           reason.MachineFilterInclude == null);
        overwriteRequired |= reason.OverwriteRequired;
        selectionPossible |= !reason.IsDefault;
      }
      
      if (!defaultDefined) {
        return "a default reason must be defined";
      }

      if (overwriteRequired && !selectionPossible) {
        return "selectable reasons must be defined";
      }

      if (!defaultDefinedWithNoConstraint) {
        return "a general default reason MIGHT be needed";
      }

      return "";
    }
    #endregion // Methods
  }
}
