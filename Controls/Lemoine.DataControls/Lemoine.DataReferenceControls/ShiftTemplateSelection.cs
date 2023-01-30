// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;

namespace Lemoine.DataReferenceControls
{
  /// <summary>
  /// Description of ShiftTemplateSelection.
  /// </summary>
  public partial class ShiftTemplateSelection : UserControl
  {
    #region Members
    string m_displayedProperty = "Display";
    private System.Windows.Forms.Button m_okButton = null;
    IList<IShiftTemplate> m_shiftTemplates = null;
    IShiftTemplate m_shiftTemplate = null;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (ShiftTemplateSelection).FullName);

    #region Getters / Setters
    /// <summary>
    /// Can several rows be selected ?
    /// </summary>
    [Category("Configuration"), Browsable(true), DefaultValue(false), Description("Allow multi-selection")]
    public bool MultiSelect {
      get { return (shiftTemplateListBox.SelectionMode != SelectionMode.One); }
      set
      {
        shiftTemplateListBox.SelectionMode =
          value ? SelectionMode.MultiExtended : SelectionMode.One;
      }
    }
    
    /// <summary>
    /// Is a null ShiftTemplate a valid value ?
    /// </summary>
    [Category("Configuration"), Browsable(true), DefaultValue(false), Description("Is a null MachineObservation valid ?")]
    public bool Nullable {
      get { return nullCheckBox.Visible; }
      set
      {
        if (value) {
          tableLayoutPanel1.RowStyles [1].Height = 30;
          nullCheckBox.Visible = true;
        }
        else {
          tableLayoutPanel1.RowStyles [1].Height = 0;
          nullCheckBox.Visible = false;
        }
      }
    }
    
    /// <summary>
    /// Property that is displayed
    /// </summary>
    [Category("Configuration"), Browsable(true), DefaultValue("Display"), Description("Property to display")]
    public string DisplayedProperty {
      get { return  m_displayedProperty; }
      set { m_displayedProperty = value; }
    }

    /// <summary>
    /// Return selected ShiftTemplates
    /// </summary>
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public IList<IShiftTemplate> SelectedShiftTemplates{
      get{
        IList<IShiftTemplate> mSTList = new List<IShiftTemplate>();
        foreach(object item in this.shiftTemplateListBox.SelectedItems){
          IShiftTemplate shiftTemplate = item as IShiftTemplate;
          if(item != null) {
            mSTList.Add(shiftTemplate);
          }
        }
        return mSTList;
      }
      set {
        if(value != null && value.Count >1){
          this.m_shiftTemplates = value;
        }
      }
    }
    
    /// <summary>
    /// Return selected ShiftTemplate
    /// </summary>
    public IShiftTemplate SelectedValue{
      get{
        IList<IShiftTemplate> shiftTemplates = this.SelectedShiftTemplates;
        if (shiftTemplates.Count >= 1 && !this.nullCheckBox.Checked) {
          return shiftTemplates[0] as IShiftTemplate;
        }
        else {
          return null;
        }
      }
      set {
        this.m_shiftTemplate = value;
      }
    }
    
    /// <summary>
    /// Set the OkButton from parent Dialog for avoid nullable value
    /// if nullable are not valid value
    /// </summary>
    public System.Windows.Forms.Button SetOkButton{
      set {
        this.m_okButton = value;
      }
    }
    #endregion // Getters / Setters

    #region Events
    /// <summary>
    /// Selection changed
    /// </summary>
    [Category("Behavior"), Description("Raised after a selection")]
    public event EventHandler AfterSelect;
    #endregion // Events
      
    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public ShiftTemplateSelection()
    {
      InitializeComponent();
      this.Nullable = false;
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Raise the AfterSelect event
    /// </summary>
    /// <param name="e"></param>
    protected virtual void OnAfterSelect (EventArgs e)
    {
      if (null != AfterSelect) {
        AfterSelect (this, e);
      }
    }
    
    /// <summary>
    /// Set Selected ShiftTemplate in listbox
    /// </summary>
    private void SetSelectedShiftTemplate(){
      if(this.m_shiftTemplate != null){
        int index = this.shiftTemplateListBox.Items.IndexOf(this.m_shiftTemplate);
        if(index >= 0){
          this.shiftTemplateListBox.SetSelected(index, true);
        }
      }
    }

    /// <summary>
    /// Set Selected ShiftTemplates in listbox
    /// </summary>
    private void SetSelectedShiftTemplates(){
      if(this.m_shiftTemplates != null){
        int index = -1;
        foreach(IShiftTemplate ShiftTemplate in this.m_shiftTemplates){
          index = this.shiftTemplateListBox.Items.IndexOf(ShiftTemplate);
          if(index >= 0){
            this.shiftTemplateListBox.SetSelected(index,true);
          }
        }
      }
    }
    #endregion // Methods
    
    void ShiftTemplateSelectionLoad(object sender, EventArgs e)
    {
      IList<IShiftTemplate> shiftTemplates;
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      
      if(daoFactory == null) //Designer Null Guard
{
        return;
      }

      using (IDAOSession session = daoFactory.OpenSession ()) {
        shiftTemplates = daoFactory.ShiftTemplateDAO.FindAll ();
      }

      shiftTemplateListBox.Items.Clear ();
      foreach (IShiftTemplate shiftTemplate in shiftTemplates) {
        shiftTemplateListBox.Items.Add (shiftTemplate);
      }
      shiftTemplateListBox.ValueMember = DisplayedProperty;
      
      this.SetSelectedShiftTemplate();
      this.SetSelectedShiftTemplates();
    }
    
    void NullCheckBoxCheckedChanged(object sender, EventArgs e)
    {
      if (nullCheckBox.Checked) {
        shiftTemplateListBox.Enabled = false;
      }
      else {
        shiftTemplateListBox.Enabled = true;
      }
      
      OnAfterSelect (new EventArgs ());
    }
    
    void ListBoxSelectedIndexChanged(object sender, EventArgs e)
    {
      if(shiftTemplateListBox.SelectedItems.Count >= 1){
        if(m_okButton != null) {
          m_okButton.Enabled = true;
        }
      }
      
      OnAfterSelect (new EventArgs ());
    }
  }
}
