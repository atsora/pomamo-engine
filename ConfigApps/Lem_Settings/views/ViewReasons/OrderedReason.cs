// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Drawing;
using Lemoine.Model;

namespace ViewReasons
{
  /// <summary>
  /// Description of OrderedReason.
  /// </summary>
  public class OrderedReason: IComparable
  {
    #region Members
    int m_hashCode = 0;
    bool m_hashCodeComputed = false;
    #endregion // Members
    
    #region Getters / Setters
    /// <summary>
    /// True if default
    /// </summary>
    public bool IsDefault { get; private set; }
    
    /// <summary>
    /// True if the reason is inherited from a parent machine observation state
    /// </summary>
    public bool IsInherited { get; private set; }
    
    /// <summary>
    /// True if the overwrite is required (in case it is a default reason)
    /// </summary>
    public bool OverwriteRequired { get; private set; }
    
    /// <summary>
    /// True if details are required (in case it is NOT a default reason)
    /// </summary>
    public bool DetailsRequired { get; private set; }
    
    /// <summary>
    /// Maximum time during which a default reason can be assigned
    /// </summary>
    public TimeSpan? MaxTime { get; private set; }
    
    /// <summary>
    /// Filter to include only some machines
    /// </summary>
    public IMachineFilter MachineFilterInclude { get; private set; }
    
    /// <summary>
    /// Filter to exclude some machines
    /// </summary>
    public IMachineFilter MachineFilterExclude { get; private set; }
    
    /// <summary>
    /// Get a formated description of the reason
    /// </summary>
    public string Description { get; private set; }
    
    /// <summary>
    /// Color of the reason
    /// </summary>
    public Color Color { get; private set; }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public OrderedReason(IReasonSelection reason, bool inherited)
    {
      IsDefault = false;
      MachineFilterExclude = null;
      MachineFilterInclude = reason.MachineFilter;
      MaxTime = null;
      OverwriteRequired = false;
      DetailsRequired = reason.DetailsRequired;
      Color = ColorTranslator.FromHtml(reason.Reason.Color);
      Description = reason.Reason.Display;
      IsInherited = inherited;
    }
    
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public OrderedReason(IMachineModeDefaultReason reason, bool inherited)
    {
      IsDefault = true;
      MachineFilterExclude = reason.ExcludeMachineFilter;
      MachineFilterInclude = reason.IncludeMachineFilter;
      MaxTime = reason.MaximumDuration;
      OverwriteRequired = reason.OverwriteRequired;
      DetailsRequired = false;
      Color = ColorTranslator.FromHtml(reason.Reason.Color);
      Description = reason.Reason.Display;
      IsInherited = inherited;
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
          m_hashCode = (int)20002907;
          
          if (IsDefault) {
            m_hashCode *= 25457;
          }

          if (MachineFilterInclude != null) {
            m_hashCode *= 25469 * MachineFilterInclude.Id;
          }

          if (MachineFilterExclude != null) {
            m_hashCode *= 25999 * MachineFilterExclude.Id;
          }

          if (DetailsRequired) {
            m_hashCode *= 26437;
          }

          if (OverwriteRequired) {
            m_hashCode *= 27197;
          }

          m_hashCode ^= 30133 * MaxTime.GetHashCode();
          if (!string.IsNullOrEmpty(Description)) {
            m_hashCode ^= 34313 * Description.GetHashCode();
          }

          m_hashCode ^= 41233 * Color.GetHashCode();
          if (IsInherited) {
            m_hashCode ^= 100189;
          }
        }
        m_hashCodeComputed = true;
      }
      
      return m_hashCode;
    }
    
    /// <summary>
    /// IComparable implementation
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public int CompareTo(object obj)
    {
      var other = obj as OrderedReason;
      if (other == null) {
        return -1;
      }

      if (other.IsDefault != this.IsDefault) {
        return other.IsDefault ? 1 : -1;
      }

      return this.Description.CompareTo(other.Description);
    }
    #endregion // Methods
  }
}
