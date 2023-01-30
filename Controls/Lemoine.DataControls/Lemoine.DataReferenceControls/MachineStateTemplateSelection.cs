// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;

using Lemoine.I18N;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;

namespace Lemoine.DataReferenceControls
{
  /// <summary>
  /// Control to select a machine mode
  /// </summary>
  public partial class MachineStateTemplateSelection : UserControl
  {
    #region Members
    string m_displayedProperty = "Display";
    private System.Windows.Forms.Button m_okButton = null;
    IList<IMachineStateTemplate> m_machineStateTemplates = null;
    IMachineStateTemplate m_machineStateTemplate = null;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (MachineStateTemplateSelection).FullName);

    #region Getters / Setters
    /// <summary>
    /// Can several rows be selected ?
    /// </summary>
    [Category("Configuration"), Browsable(true), DefaultValue(false), Description("Allow multi-selection")]
    public bool MultiSelect {
      get { return (machineStateTemplateListBox.SelectionMode != SelectionMode.One); }
      set
      {
        machineStateTemplateListBox.SelectionMode =
          value ? SelectionMode.MultiExtended : SelectionMode.One;
      }
    }
    
    /// <summary>
    /// Is a null MachineStateTemplate a valid value ?
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
    /// Return selected MachineStateTemplates
    /// </summary>
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public IList<IMachineStateTemplate> SelectedMachineStateTemplates{
      get{
        IList<IMachineStateTemplate> mSTList = new List<IMachineStateTemplate>();
        foreach(object item in this.machineStateTemplateListBox.SelectedItems){
          IMachineStateTemplate machineStateTemplate = item as IMachineStateTemplate;
          if(item != null) {
            mSTList.Add(machineStateTemplate);
          }
        }
        return mSTList;
      }
      set {
        if(value != null && value.Count >1){
          this.m_machineStateTemplates = value;
        }
      }
    }
    
    /// <summary>
    /// Return selected MachineStateTemplate
    /// </summary>
    public IMachineStateTemplate SelectedValue{
      get{
        IList<IMachineStateTemplate> machineStateTemplates = this.SelectedMachineStateTemplates;
        if (machineStateTemplates.Count >= 1 && !this.nullCheckBox.Checked) {
          return machineStateTemplates[0] as IMachineStateTemplate;
        }
        else {
          return null;
        }
      }
      set {
        this.m_machineStateTemplate = value;
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
    public MachineStateTemplateSelection()
    {
      InitializeComponent();
      nullCheckBox.Text = PulseCatalog.GetString ("MachineStateTemplateNull");
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
    /// Set Selected MachineStateTemplate in listbox
    /// </summary>
    private void SetSelectedMachineStateTemplate(){
      if(this.m_machineStateTemplate != null){
        int index = this.machineStateTemplateListBox.Items.IndexOf(this.m_machineStateTemplate);
        if(index >= 0){
          this.machineStateTemplateListBox.SetSelected(index, true);
        }
      }
    }

    /// <summary>
    /// Set Selected MachineStateTemplates in listbox
    /// </summary>
    private void SetSelectedMachineStateTemplates(){
      if(this.m_machineStateTemplates != null){
        int index = -1;
        foreach(IMachineStateTemplate MachineStateTemplate in this.m_machineStateTemplates){
          index = this.machineStateTemplateListBox.Items.IndexOf(MachineStateTemplate);
          if(index >= 0){
            this.machineStateTemplateListBox.SetSelected(index,true);
          }
        }
      }
    }
    #endregion // Methods
    
    void MachineStateTemplateSelectionLoad(object sender, EventArgs e)
    {
      IList<IMachineStateTemplate> machineStateTemplates;
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      
      if(daoFactory == null) //Designer Null Guard
{
        return;
      }

      using (IDAOSession session = daoFactory.OpenSession ()) {
        machineStateTemplates = daoFactory.MachineStateTemplateDAO.FindAll ();
      }

      machineStateTemplateListBox.Items.Clear ();
      foreach (IMachineStateTemplate machineStateTemplate in machineStateTemplates) {
        machineStateTemplateListBox.Items.Add (machineStateTemplate);
      }
      machineStateTemplateListBox.ValueMember = DisplayedProperty;
      
      this.SetSelectedMachineStateTemplate();
      this.SetSelectedMachineStateTemplates();
    }
    
    void NullCheckBoxCheckedChanged(object sender, EventArgs e)
    {
      if (nullCheckBox.Checked) {
        machineStateTemplateListBox.Enabled = false;
      }
      else {
        machineStateTemplateListBox.Enabled = true;
      }
      
      OnAfterSelect (new EventArgs ());
    }
    
    void ListBoxSelectedIndexChanged(object sender, EventArgs e)
    {
      if(machineStateTemplateListBox.SelectedItems.Count >= 1){
        if(m_okButton != null) {
          m_okButton.Enabled = true;
        }
      }
      
      OnAfterSelect (new EventArgs ());
    }
  }
}
