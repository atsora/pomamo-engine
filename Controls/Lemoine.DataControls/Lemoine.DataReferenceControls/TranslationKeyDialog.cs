// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Drawing;
using System.Windows.Forms;

using Lemoine.BaseControls;
using Lemoine.Core.Log;

namespace Lemoine.DataReferenceControls
{
  /// <summary>
  /// Description of TranslationKeyDialog.
  /// </summary>
  public partial class TranslationKeyDialog : OKCancelDialog, IValueDialog<string>
  {
    #region Members
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (TranslationKeyDialog).FullName);

    #region Getters / Setters
    /// <summary>
    /// Is a null TranslationKey a valid value ?
    /// </summary>
    public bool Nullable {
      get { return translationKeySelection1.Nullable; }
      set { translationKeySelection1.Nullable = value; }
    }
    
    /// <summary>
    /// Selected TranslationKey or null if no TranslationKey is selected
    /// </summary>
    public string SelectedValue {
      get
      {
        return this.translationKeySelection1.SelectedTranslationKey;
      }
      set {
        this.translationKeySelection1.SelectedTranslationKey = value;
      }
    }
    
    /// <summary>
    /// No reason to be Implemented.
    /// </summary>
    public System.Collections.Generic.IList<string> SelectedValues {
      get {
        throw new NotImplementedException();
      }
      set {
        throw new NotImplementedException();
      }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public TranslationKeyDialog()
    {
      //
      // The InitializeComponent() call is required for Windows Forms designer support.
      //
      InitializeComponent();
      
      //
      // Add constructor code after the InitializeComponent() call.
      //
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
