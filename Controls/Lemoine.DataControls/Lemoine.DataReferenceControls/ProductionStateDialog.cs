// Copyright (C) 2025 Atsora Solutions
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
  /// Description of ProductionStateDialog.
  /// </summary>
  public partial class ProductionStateDialog : OKCancelDialog, IValueDialog<IProductionState>
  {
    static readonly ILog log = LogManager.GetLogger(typeof (ProductionStateDialog).FullName);

    #region Getters / Setters
    /// <summary>
    /// Is a null ProductionState a valid value ?
    /// </summary>
    public bool Nullable {
      get { return productionStateSelection1.Nullable; }
      set { productionStateSelection1.Nullable = value; }
    }
    
    /// <summary>
    /// Property that is displayed
    /// </summary>
    public string DisplayedProperty {
      get { return  productionStateSelection1.DisplayedProperty; }
      set { productionStateSelection1.DisplayedProperty = value; }
    }

    /// <summary>
    /// Selected ProductionState or null if no ProductionState is selected
    /// 
    /// Setable selection
    /// </summary>
    public IProductionState SelectedValue {
      get
      {
        return this.productionStateSelection1.SelectedProductionState;
      }
      set {
        this.productionStateSelection1.SelectedProductionState = value;
      }
    }
    
    /// <summary>
    /// Selected ProductionStates or null if no ProductionState is selected
    /// 
    /// Setable selection
    /// </summary>
    public IList<IProductionState> SelectedValues {
      get
      {
        return this.productionStateSelection1.SelectedProductionStates;
      }
      set {
        this.productionStateSelection1.SelectedProductionStates = value;
      }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public ProductionStateDialog()
    {
      //
      // The InitializeComponent() call is required for Windows Forms designer support.
      //
      InitializeComponent();
      
      //
      // Add constructor code after the InitializeComponent() call.
      //
      this.Text = PulseCatalog.GetString ("ProductionStateDialogTitle");
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