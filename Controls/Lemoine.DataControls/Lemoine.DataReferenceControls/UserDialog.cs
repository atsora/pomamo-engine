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
  /// Description of UserDialog.
  /// </summary>
  public partial class UserDialog : OKCancelDialog, IValueDialog<IUser>
  {
    #region Members
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (UserDialog).FullName);

    #region Getters / Setters
    /// <summary>
    /// Return selected User
    /// </summary>
    public IUser SelectedValue {
      get {
        return this.userSelection1.SelectedT;
      }
      set {
        this.userSelection1.SelectedT = value;
      }
    }
    
    /// <summary>
    /// Return selected Users
    /// </summary>
    public IList<IUser> SelectedValues {
      get {
        return this.userSelection1.SelectedTs;
      }
      set {
        this.userSelection1.SelectedTs = value;
      }
    }
    
    /// <summary>
    /// <see cref="Lemoine.DataReferenceControls.GenericSelection{T}">MultiSelect</see> implementation
    /// </summary>
    public bool MultiSelect {
      get {
        return this.userSelection1.MultiSelect;
      }
      set {
        this.userSelection1.MultiSelect = value;
      }
    }
    
    /// <summary>
    /// <see cref="Lemoine.DataReferenceControls.GenericSelection{T}">Nullable</see> implementation
    /// </summary>
    public bool Nullable {
      get {
        return this.userSelection1.Nullable;
      }
      set {
        this.userSelection1.Nullable = value;
      }
    }
    
    /// <summary>
    /// <see cref="Lemoine.DataReferenceControls.GenericSelection{T}">DisplayedProperty</see> implementation
    /// </summary>
    public string DisplayedProperty {
      get {
        return this.userSelection1.DisplayedProperty;
      }
      set {
        this.userSelection1.DisplayedProperty = value;
      }
    }
    
    /// <summary>
    /// <see cref="Lemoine.DataReferenceControls.GenericSelection{T}">NoSelectionText</see> implementation
    /// </summary>
    public string NoSelectionText {
      get {
        return this.userSelection1.NoSelectionText;
      }
      set {
        this.userSelection1.NoSelectionText = value;
      }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public UserDialog()
    {
      //
      // The InitializeComponent() call is required for Windows Forms designer support.
      //
      InitializeComponent();
      
      this.Text = PulseCatalog.GetString("UserDialog");
    }
    #endregion // Constructors

    #region Methods
    #endregion // Methods
  }
}
