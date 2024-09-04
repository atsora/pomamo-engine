// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Model;
using Lemoine.Core.Log;

namespace ConfiguratorGoals
{
  /// <summary>
  /// Decorator for a tree node element
  /// </summary>
  public class TreeNodeElement: IDisplayable//, IComparable
  {

    static readonly ILog log = LogManager.GetLogger(typeof (TreeNodeElement).FullName);

    #region Getters / Setters
    /// <summary>
    /// Displayable
    /// </summary>
    public IDisplayable Displayable { get; private set; }
    
    /// <summary>
    /// Value to display
    /// </summary>
    public double? Value { get; set; }
    
    /// <summary>
    /// Unit to display
    /// </summary>
    public static string Unit { get; set; }
    
    /// <summary>
    /// True if the element is displayed in blue
    /// </summary>
    public bool IsBlue { get; set; }
    
    /// <summary>
    /// Display property
    /// </summary>
    public string Display {
      get {
        return String.Format("{0} ({1} {2})",
                             Displayable == null ? "Not specified" : Displayable.Display,
                             Value.HasValue ? Value.Value.ToString() : "?", Unit);
      }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Default constructor
    /// </summary>
    /// <param name="displayable"></param>
    public TreeNodeElement (IDisplayable displayable)
    {
      Displayable = displayable;
      IsBlue = false;
    }
    #endregion // Constructors
    
    #region Methods
    public override bool Equals(object obj)
    {
      var other = obj as TreeNodeElement;
      return other != null && object.Equals(this.Displayable, other.Displayable);
    }

    public override int GetHashCode()
    {
      int hashCode = 0;
      unchecked {
        if (Displayable != null) {
          hashCode += 1000000007 * Displayable.GetHashCode();
        }
      }
      return hashCode;
    }

    public static bool operator ==(TreeNodeElement lhs, TreeNodeElement rhs) {
      if (ReferenceEquals(lhs, rhs)) {
        return true;
      }

      if (ReferenceEquals(lhs, null) || ReferenceEquals(rhs, null)) {
        return false;
      }

      return lhs.Equals(rhs);
    }

    public static bool operator !=(TreeNodeElement lhs, TreeNodeElement rhs) {
      return !(lhs == rhs);
    }
    #endregion // Methods
  }
}
