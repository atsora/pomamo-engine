// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Windows.Forms;

using Lemoine.BaseControls;
using Lemoine.I18N;
using Lemoine.Model;
using Lemoine.Core.Log;

namespace Lemoine.DataReferenceControls
{
  /// <summary>
  /// Description of CellDialog.
  /// </summary>
  public partial class CellDialog : OKCancelDialog, IValueDialog<ICell>
  {
    static readonly ILog log = LogManager.GetLogger(typeof (CellDialog).FullName);

    #region Getters / Setters
    /// <summary>
    /// Is a null Cell a valid value ?
    /// </summary>
    public bool Nullable {
      get { return cellSelection1.Nullable; }
      set { cellSelection1.Nullable = value; }
    }
    
    /// <summary>
    /// Property that is displayed
    /// </summary>
    public string DisplayedProperty {
      get { return  cellSelection1.DisplayedProperty; }
      set { cellSelection1.DisplayedProperty = value; }
    }
    
    /// <summary>
    /// Selected Cell or null if no Cell is selected
    /// 
    /// Setable Selection
    /// </summary>
    public ICell SelectedValue {
      get
      {
        return this.cellSelection1.SelectedCell;
      }
      set {
        this.cellSelection1.SelectedCell = value;
      }
    }
    
    /// <summary>
    /// Return Selected Cells or null
    /// 
    /// Setable Selection
    /// </summary>
    public IList<ICell> SelectedValues {
      get {
        return this.cellSelection1.SelectedCells;
      }
      set {
        this.cellSelection1.SelectedCells = value;
      }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public CellDialog()
    {
      //
      // The InitializeComponent() call is required for Windows Forms designer support.
      //
      InitializeComponent();
      
      //
      // Add constructor code after the InitializeComponent() call.
      //
      this.Text = PulseCatalog.GetString ("CellDialogTitle");
    }
    #endregion // Constructors

    #region Methods
    #endregion // Methods
    
    void OkButtonClick(object sender, EventArgs e)
    {
      this.DialogResult = DialogResult.OK;
    }
    
    void CancelButtonClick(object sender, EventArgs e)
    {
      this.DialogResult = DialogResult.Cancel;
    }
  }
}
