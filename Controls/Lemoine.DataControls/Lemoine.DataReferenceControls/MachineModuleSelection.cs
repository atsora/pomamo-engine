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
  public partial class MachineModuleSelection : UserControl
  {
    #region Members
    string m_displayedProperty = "Display";
    IList<IMachineModule> m_machineModules = null;
    IMachineModule m_machineModule = null;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (MachineModuleSelection).FullName);

    #region Getters / Setters
    /// <summary>
    /// Can several rows be selected ?
    /// </summary>
    [Category("Configuration"), Browsable(true), DefaultValue(false), Description("Allow multi-selection")]
    public bool MultiSelect {
      get { return (listBox.SelectionMode != SelectionMode.One); }
      set
      {
        listBox.SelectionMode =
          value ? SelectionMode.MultiExtended : SelectionMode.One;
      }
    }
    
    /// <summary>
    /// Is a null MachineModule a valid value ?
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
    /// Selected MachineModules
    /// 
    /// The returned list is not null, but may be empty or contain only one null element
    /// </summary>
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public IList<IMachineModule> SelectedMachineModules {
      get
      {
        IList<IMachineModule> list = new List<IMachineModule> ();
        if (!nullCheckBox.Checked) {
          foreach (object item in listBox.SelectedItems) {
            IMachineModule MachineModule = item as IMachineModule;
            list.Add (MachineModule);
          }
        }
        return list;
      }
      set {
        if(value != null && value.Count >= 1){
          this.m_machineModules = value;
        }
      }
    }

    /// <summary>
    /// Selected MachineModule
    /// Return the first selected (if multiselection) or null
    /// </summary>
    public IMachineModule SelectedMachineModule {
      get {
        IList<IMachineModule> machineModules = this.SelectedMachineModules;
        if (machineModules.Count >= 1 && !this.nullCheckBox.Checked) {
          return machineModules[0] as IMachineModule;
        }
        else {
          return null;
        }
      }
      set {
        this.m_machineModule = value;
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
    public MachineModuleSelection()
    {
      //
      // The InitializeComponent() call is required for Windows Forms designer support.
      //
      InitializeComponent();
      
      nullCheckBox.Text = PulseCatalog.GetString ("MachineModuleNull");
      
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
    /// Set Selected MachineModule in listbox
    /// </summary>
    private void SetSelectedMachineModule(){
      if(this.m_machineModule != null){
        int index = this.listBox.Items.IndexOf(this.m_machineModule);
        if(index >= 0){
          this.listBox.SetSelected(index, true);
        }
      }
    }

    /// <summary>
    /// Set Selected MachineModules in listbox
    /// </summary>
    private void SetSelectedMachineModules(){
      if(this.m_machineModules != null){
        int index = -1;
        foreach(IMachineModule MachineModule in this.m_machineModules){
          index = this.listBox.Items.IndexOf(MachineModule);
          if(index >= 0){
            this.listBox.SetSelected(index,true);
          }
        }
      }
    }
    #endregion // Methods
    
    void MachineModuleSelectionLoad(object sender, EventArgs e)
    {
      IList<IMachineModule> machineModules;
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      using (IDAOSession session = daoFactory.OpenSession ())
      {
        machineModules = daoFactory.MachineModuleDAO.FindAll ();
      }
      
      listBox.Items.Clear ();
      foreach (IMachineModule machineModule in machineModules) {
        listBox.Items.Add (machineModule);
      }
      listBox.ValueMember = DisplayedProperty;
      
      this.SetSelectedMachineModule();
      this.SetSelectedMachineModules();
    }
    
    void NullCheckBoxCheckedChanged(object sender, EventArgs e)
    {
      if (nullCheckBox.Checked) {
        listBox.Enabled = false;
      }
      else {
        listBox.Enabled = true;
      }
      
      OnAfterSelect (new EventArgs ());
    }
    
    void ListBoxSelectedIndexChanged(object sender, EventArgs e)
    {
      OnAfterSelect (new EventArgs ());
    }
  }
}
