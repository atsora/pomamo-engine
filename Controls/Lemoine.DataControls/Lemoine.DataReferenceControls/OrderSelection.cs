// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Windows.Forms;
using Lemoine.I18N;
using Lemoine.Core.Log;

namespace Lemoine.DataReferenceControls
{
  /// <summary>
  /// Description of OrderSelection.
  /// </summary>
  public partial class OrderSelection : UserControl
  {
    #region Members
    private bool m_nullable;
    private Button m_okButton;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (OrderSelection).FullName);

    #region Getters / Setters
    /// <summary>
    /// Is a null DateTime a valid value ?
    /// </summary>
    public bool Nullable {
      get { return this.m_nullable; }
      set { this.m_nullable = value; }
    }
    
    /// <summary>
    /// Get the selected index 
    /// Can be NULL test UserSpecifiedIndex before
    /// </summary>
    public int SelectedIndex{
      get { return (int)this.indexNumericUpDown.Value; }
    }
    
    /// <summary>
    /// Did the user specify a index or not ?
    /// </summary>
    public bool UserSpecifiedIndex{
      get { return this.specificInsertCheckBox.Checked; }
    }
    
    /// <summary>
    /// Get/Set the Maximum selectable index 
    /// </summary>
    public int MaxIndex{
      set { this.indexNumericUpDown.Maximum = value; }
      get { return (int)this.indexNumericUpDown.Maximum;}
    }
    
    /// <summary>
    /// Get/Set the Minimum selectable index 
    /// </summary>
    public int MinIndex{
      set { this.indexNumericUpDown.Minimum = value; }
      get { return (int)this.indexNumericUpDown.Minimum;}
    }
    
    /// <summary>
    /// The the OkButton from parent Form
    /// Used if Nullable == false
    /// </summary>
    public Button OkButton{
      set { this.m_okButton = value; }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public OrderSelection()
    {
      //
      // The InitializeComponent() call is required for Windows Forms designer support.
      //
      InitializeComponent();
      this.defaultInsertCheckBox.Text = PulseCatalog.GetString ("OrderSelectionInsertDefaultText");
      this.specificInsertCheckBox.Text = PulseCatalog.GetString ("OrderSelectionSpecificInsertText");
    }

    #endregion // Constructors

    #region Methods

    #endregion // Methods
    
    void DefaultInsertCheckBoxCheckedChanged(object sender, EventArgs e)
    {
      if(this.defaultInsertCheckBox.Checked){
        this.specificInsertCheckBox.Checked = false;
        this.m_okButton.Enabled = true; 
      }
      else {
        if(!this.specificInsertCheckBox.Checked) {
          this.m_okButton.Enabled = false;
        }
      }
    }
    
    void SpecificInsertCheckBoxCheckedChanged(object sender, EventArgs e)
    {
      if(this.specificInsertCheckBox.Checked) {
        this.defaultInsertCheckBox.Checked = false;
      }
      else {
        if(!this.defaultInsertCheckBox.Checked) {
          this.m_okButton.Enabled = false;
        }
      }
    }
    
    void IndexNumericUpDownValueChanged(object sender, EventArgs e)
    {
      this.specificInsertCheckBox.Checked = true;
      this.defaultInsertCheckBox.Checked = false;
      this.m_okButton.Enabled = true; 
    }
      
    void OrderSelectionLoad(object sender, EventArgs e)
    {
      this.defaultInsertCheckBox.Checked = true;
    }
  }
}
