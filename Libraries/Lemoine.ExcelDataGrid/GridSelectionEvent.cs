// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using Lemoine.Core.Log;
using System.Xml.Serialization;

namespace Lemoine.ExcelDataGrid
{
  /// <summary>
  /// Description of GridSelectionEvent.
  /// </summary>
  /// 
  [XmlType("GridSelectionEvent")]
  public class GridSelectionEvent
  {
    static readonly ILog log = LogManager.GetLogger(typeof (GridSelectionEvent).FullName);

    #region Members
    string m_regionName;
    int m_minRow;
    int m_maxRow;
    int m_minColumn;
    int m_maxColumn;
    #endregion // Members

    #region Constructors
    /// <summary>
    /// default constructor
    /// </summary>
    public GridSelectionEvent ()
    {
    }

    /// <summary>
    /// build a select region event from a cell selection
    /// </summary>
    public GridSelectionEvent(string regionName, 
                              int minRowIndex, int maxRowIndex,
                              int minColumnIndex, int maxColumnIndex)
    {
      this.RegionName = regionName;
      this.MinRow = minRowIndex;
      this.MaxRow = maxRowIndex;
      this.MinColumn = minColumnIndex;
      this.MaxColumn = maxColumnIndex;
    }
    #endregion // Constructors

    #region Getters/Setters
    
    /// <summary>
    /// name of the region (as it will appear in target Excel file)
    /// </summary>
    [XmlAttribute("RegionName")]
    public string RegionName {
      get { return m_regionName; }
      set { m_regionName = value; }
    }
    
    /// <summary>
    /// selected cells min row index
    /// </summary>
    [XmlAttribute("minRow")]
    public int MinRow {
      get { return m_minRow; }
      set { m_minRow = value; }
    }

    /// <summary>
    /// selected cells max row index
    /// </summary>
    [XmlAttribute("maxRow")]
    public int MaxRow {
      get { return m_maxRow; }
      set { m_maxRow = value; }
    }
    
    /// <summary>
    /// selected cells min column index
    /// </summary>
    [XmlAttribute("minColumn")]
    public int MinColumn {
      get { return m_minColumn; }
      set { m_minColumn = value; }
    }
    
    /// <summary>
    /// selected cells max column index
    /// </summary>
    [XmlAttribute("maxColumn")]
    public int MaxColumn {
      get { return m_maxColumn; }
      set { m_maxColumn = value; }
    }
    
    #endregion Getters/Setters
    
    #region Methods
    /// <summary>
    /// Convert to a readable string
    /// </summary>
    /// <returns></returns>
    public override string ToString() {
      return "[" + RegionName + "] (Row " + MinRow.ToString() + " : Col " + MinColumn.ToString() + ", Row " +
        MaxRow.ToString() + " : Col" + MaxColumn.ToString() + ")";
    }
    #endregion // Methods

  }
}
