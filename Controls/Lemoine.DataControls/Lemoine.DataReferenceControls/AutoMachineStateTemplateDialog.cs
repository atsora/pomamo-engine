// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.BaseControls;
using Lemoine.I18N;
using Lemoine.Model;
using Lemoine.Core.Log;

namespace Lemoine.DataReferenceControls
{
  /// <summary>
  /// Description of AutoMachineStateTemplateDialog.
  /// </summary>
  public partial class AutoMachineStateTemplateDialog : OKCancelDialog, IValueDialog<IAutoMachineStateTemplate>
  {
    #region Members
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (AutoMachineStateTemplateDialog).FullName);

    #region Getters / Setters
    /// <summary>
    /// Return selected AutoMachineStateTemplate
    /// </summary>
    public IAutoMachineStateTemplate SelectedValue {
      get {
        return this.autoMachineStateTemplateSelection.SelectedT;
      }
      set {
        this.autoMachineStateTemplateSelection.SelectedT = value;
      }
    }
    
    /// <summary>
    /// Return selected AutoMachineStateTemplates
    /// </summary>
    public IList<IAutoMachineStateTemplate> SelectedValues {
      get {
        return this.autoMachineStateTemplateSelection.SelectedTs;
      }
      set {
        this.autoMachineStateTemplateSelection.SelectedTs = value;
      }
    }
    
    /// <summary>
    /// <see cref="Lemoine.DataReferenceControls.GenericSelection{T}">MultiSelect</see> implementation
    /// </summary>
    public bool MultiSelect {
      get {
        return this.autoMachineStateTemplateSelection.MultiSelect;
      }
      set {
        this.autoMachineStateTemplateSelection.MultiSelect = value;
      }
    }
    
    /// <summary>
    /// <see cref="Lemoine.DataReferenceControls.GenericSelection{T}">Nullable</see> implementation
    /// </summary>
    public bool Nullable {
      get {
        return this.autoMachineStateTemplateSelection.Nullable;
      }
      set {
        this.autoMachineStateTemplateSelection.Nullable = value;
      }
    }
    
    /// <summary>
    /// <see cref="Lemoine.DataReferenceControls.GenericSelection{T}">DisplayedProperty</see> implementation
    /// </summary>
    public string DisplayedProperty {
      get {
        return this.autoMachineStateTemplateSelection.DisplayedProperty;
      }
      set {
        this.autoMachineStateTemplateSelection.DisplayedProperty = value;
      }
    }
    
    /// <summary>
    /// <see cref="Lemoine.DataReferenceControls.GenericSelection{T}">NoSelectionText</see> implementation
    /// </summary>
    public string NoSelectionText {
      get {
        return this.autoMachineStateTemplateSelection.NoSelectionText;
      }
      set {
        this.autoMachineStateTemplateSelection.NoSelectionText = value;
      }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public AutoMachineStateTemplateDialog()
    {
      //
      // The InitializeComponent() call is required for Windows Forms designer support.
      //
      InitializeComponent();
      
      this.Text = PulseCatalog.GetString("AutoMachineStateTemplateDialog");
    }
    #endregion // Constructors

    #region Methods
    #endregion // Methods
  }
}