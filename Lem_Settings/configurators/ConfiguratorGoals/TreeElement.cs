// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Model;
using Lemoine.Core.Log;

namespace ConfiguratorGoals
{
  /// <summary>
  /// Decorator for an IDisplayable element, adding a value with a unit
  /// </summary>
  public class TreeElement: TreeNodeElement, IComparable
  {
    static readonly ILog log = LogManager.GetLogger(typeof (TreeElement).FullName);

    #region Getters / Setters
    /// <summary>
    /// Company
    /// </summary>
    public IDisplayable Company { get; private set; }
    
    /// <summary>
    /// Department
    /// </summary>
    public IDisplayable Department { get; private set; }
    
    /// <summary>
    /// Cell
    /// </summary>
    public IDisplayable Cell { get; private set; }
    
    /// <summary>
    /// Category
    /// </summary>
    public IDisplayable Category { get; private set; }
    
    /// <summary>
    /// SubCategory
    /// </summary>
    public IDisplayable SubCategory { get; private set; }
    
    string StrOrder {
      get {
        string str = "";
        str += Company == null ? "1-" : "0-" + Company.Display;
        str += Department == null ? "1-" : "0-" + Department.Display;
        str += Cell == null ? "1-" : "0-" + Cell.Display;
        str += Category == null ? "1-" : "0-" + Category.Display;
        str += SubCategory == null ? "1-" : "0-" + SubCategory.Display;
        str += Displayable.Display;
        return str;
      }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Default constructor
    /// </summary>
    /// <param name="machine"></param>
    public TreeElement(IMonitoredMachine machine) : base(machine)
    {
      Company = new TreeNodeElement(machine.Company);
      Department = new TreeNodeElement(machine.Department);
      Cell = new TreeNodeElement(machine.Cell);
      Category = new TreeNodeElement(machine.Category);
      SubCategory = new TreeNodeElement(machine.SubCategory);
    }
    #endregion // Constructors
    
    #region Methods
    /// <summary>
    /// Compare two TreeElement
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public int CompareTo(object obj)
    {
      var other = obj as TreeElement;
      if (other == null) {
        return -1;
      }

      return StrOrder.CompareTo(other.StrOrder);
    }
    #endregion // Methods
  }
}
