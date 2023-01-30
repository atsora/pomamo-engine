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
  /// Description of ToolDialog.
  /// </summary>
  public partial class ToolDialog : OKCancelDialog, IValueDialog<ITool>
  {
    static readonly ILog log = LogManager.GetLogger(typeof (ToolDialog).FullName);

    #region members
   
    #endregion
    
    
    #region Getters / Setters
    /// <summary>
    /// Is a null Tool a valid value ?
    /// </summary>
    public bool Nullable {
      get { return toolSelection1.Nullable; }
      set { toolSelection1.Nullable = value; }
    }
    
    /// <summary>
    /// Property that is displayed
    /// </summary>
    public string DisplayedProperty {
      get { return  toolSelection1.DisplayedProperty; }
      set { toolSelection1.DisplayedProperty = value; }
    }
    
    /// <summary>
    /// Selected Tool or null if no Tool is selected
    /// </summary>
    public ITool SelectedValue {
      get
      {
        return this.toolSelection1.SelectedTool;
      }
      set {
        this.toolSelection1.SelectedTool = value;
      }
    }
    
    /// <summary>
    /// Selected Tools or null if no Tool is selected
    /// </summary>
    public IList<ITool> SelectedValues {
      get
      {
        return this.toolSelection1.SelectedTools;
      }
      set {
        this.toolSelection1.SelectedTools = value;
      }
    }

    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public ToolDialog()
    {
      //
      // The InitializeComponent() call is required for Windows Forms designer support.
      //
      InitializeComponent();
      
      this.Text = PulseCatalog.GetString ("ToolDialogTitle");
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
