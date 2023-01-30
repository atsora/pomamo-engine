// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Lemoine.BaseControls;
using Lemoine.Model;
using Lemoine.Core.Log;

namespace Lemoine.DataReferenceControls
{
  /// <summary>
  /// Description of StampingConfigDialog.
  /// </summary>
  public partial class StampingConfigDialog : OKCancelDialog, IValueDialog<IStampingConfigByName>
  {
    #region Members
    private bool m_nullable;
    private bool m_multiSelet;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger (typeof (StampingConfigDialog).FullName);

    #region Getters / Setters
    /// <summary>
    /// Is a null StampingConfig a valid value ?
    /// </summary>
    public bool Nullable
    {
      get { return this.m_nullable; }
      set { this.SetNullable (value); }
    }

    /// <summary>
    /// allow/disallow multi-selection
    /// </summary>
    public bool MultiSelect
    {
      get { return this.m_multiSelet; }
      set { this.SetMultiSelect (value); }
    }

    /// <summary>
    /// Property that is displayed
    /// </summary>
    public string DisplayedProperty
    {
      get { return stampingConfigSelection.DisplayedProperty; }
      set { stampingConfigSelection.DisplayedProperty = value; }
    }

    /// <summary>
    /// Return selected StampingConfig
    /// </summary>
    public IStampingConfigByName SelectedValue
    {
      get {
        return this.stampingConfigSelection.SelectedStampingConfig;
      }
      set {
        this.stampingConfigSelection.SelectedStampingConfig = value;
      }
    }

    /// <summary>
    /// Return selected StampingConfigs
    /// </summary>
    public IList<IStampingConfigByName> SelectedValues
    {
      get {
        return this.stampingConfigSelection.SelectedStampingConfigs;
      }
      set {
        this.stampingConfigSelection.SelectedStampingConfigs = value;
      }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public StampingConfigDialog ()
    {
      InitializeComponent ();
      this.Text = "Stamping config selection";
    }
    #endregion // Constructors

    #region Methods
    private void SetNullable (bool value)
    {
      m_nullable = value;
      UpdateOkButtonEnabled ();
    }

    private void SetMultiSelect (bool value)
    {
      m_multiSelet = value;
      this.stampingConfigSelection.MultiSelect = m_multiSelet;
      UpdateOkButtonEnabled ();
    }

    void OkButtonClick (object sender, EventArgs e)
    {
      this.DialogResult = DialogResult.OK;
    }

    void CancelButtonClick (object sender, EventArgs e)
    {
      this.DialogResult = DialogResult.Cancel;
    }
    #endregion // Methods

    void StampingConfigDialogLoad (object sender, EventArgs e)
    {
      this.stampingConfigSelection.MultiSelect = m_multiSelet;
      this.stampingConfigSelection.Nullable = this.Nullable;
    }

    void stampingConfigSelection_AfterSelect (object sender, EventArgs e)
    {
      UpdateOkButtonEnabled ();
    }

    void UpdateOkButtonEnabled ()
    {
      okButton.Enabled = stampingConfigSelection.Nullable
        || stampingConfigSelection.MultiSelect
        || (null != stampingConfigSelection.SelectedStampingConfig);
    }
  }
}
