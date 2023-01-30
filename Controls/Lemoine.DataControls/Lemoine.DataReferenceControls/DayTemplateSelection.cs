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
  /// Description of DayTemplateSelection.
  /// </summary>
  public partial class DayTemplateSelection : UserControl
  {
    #region Members
    string m_displayedProperty = "Display";
    private System.Windows.Forms.Button m_okButton = null;
    IList<IDayTemplate> m_dayTemplates = null;
    IDayTemplate m_dayTemplate = null;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (DayTemplateSelection).FullName);

    #region Getters / Setters
    /// <summary>
    /// Can several rows be selected ?
    /// </summary>
    [Category("Configuration"), Browsable(true), DefaultValue(false), Description("Allow multi-selection")]
    public bool MultiSelect {
      get { return (dayTemplateListBox.SelectionMode != SelectionMode.One); }
      set
      {
        dayTemplateListBox.SelectionMode =
          value ? SelectionMode.MultiExtended : SelectionMode.One;
      }
    }
    
    /// <summary>
    /// Is a null DayTemplate a valid value ?
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
    /// Return selected DayTemplates
    /// </summary>
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public IList<IDayTemplate> SelectedDayTemplates{
      get{
        IList<IDayTemplate> mSTList = new List<IDayTemplate>();
        foreach(object item in this.dayTemplateListBox.SelectedItems){
          IDayTemplate dayTemplate = item as IDayTemplate;
          if(item != null) {
            mSTList.Add(dayTemplate);
          }
        }
        return mSTList;
      }
      set {
        if(value != null && value.Count >1){
          this.m_dayTemplates = value;
        }
      }
    }
    
    /// <summary>
    /// Return selected DayTemplate
    /// </summary>
    public IDayTemplate SelectedValue{
      get{
        IList<IDayTemplate> dayTemplates = this.SelectedDayTemplates;
        if (dayTemplates.Count >= 1 && !this.nullCheckBox.Checked) {
          return dayTemplates[0] as IDayTemplate;
        }
        else {
          return null;
        }
      }
      set {
        this.m_dayTemplate = value;
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
    public DayTemplateSelection()
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
    /// Set Selected DayTemplate in listbox
    /// </summary>
    private void SetSelectedDayTemplate(){
      if(this.m_dayTemplate != null){
        int index = this.dayTemplateListBox.Items.IndexOf(this.m_dayTemplate);
        if(index >= 0){
          this.dayTemplateListBox.SetSelected(index, true);
        }
      }
    }

    /// <summary>
    /// Set Selected DayTemplates in listbox
    /// </summary>
    private void SetSelectedDayTemplates(){
      if(this.m_dayTemplates != null){
        int index = -1;
        foreach(IDayTemplate DayTemplate in this.m_dayTemplates){
          index = this.dayTemplateListBox.Items.IndexOf(DayTemplate);
          if(index >= 0){
            this.dayTemplateListBox.SetSelected(index,true);
          }
        }
      }
    }
    #endregion // Methods
    
    void DayTemplateSelectionLoad(object sender, EventArgs e)
    {
      IList<IDayTemplate> dayTemplates;
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      
      if(daoFactory == null) //Designer Null Guard
{
        return;
      }

      using (IDAOSession session = daoFactory.OpenSession ()) {
        dayTemplates = daoFactory.DayTemplateDAO.FindAll ();
      }

      dayTemplateListBox.Items.Clear ();
      foreach (IDayTemplate dayTemplate in dayTemplates) {
        dayTemplateListBox.Items.Add (dayTemplate);
      }
      dayTemplateListBox.ValueMember = DisplayedProperty;
      
      this.SetSelectedDayTemplate();
      this.SetSelectedDayTemplates();
    }
    
    void NullCheckBoxCheckedChanged(object sender, EventArgs e)
    {
      if (nullCheckBox.Checked) {
        dayTemplateListBox.Enabled = false;
      }
      else {
        dayTemplateListBox.Enabled = true;
      }
      
      OnAfterSelect (new EventArgs ());
    }
    
    void ListBoxSelectedIndexChanged(object sender, EventArgs e)
    {
      if(dayTemplateListBox.SelectedItems.Count >= 1){
        if(m_okButton != null) {
          m_okButton.Enabled = true;
        }
      }
      
      OnAfterSelect (new EventArgs ());
    }
  }
}
