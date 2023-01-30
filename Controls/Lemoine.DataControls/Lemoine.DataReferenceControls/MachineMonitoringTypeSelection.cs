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
  /// Control to select a machine monitoring type
  /// </summary>
  public partial class MachineMonitoringTypeSelection : UserControl
  {
    #region Members
    string m_displayedProperty = "Display";
    IList<IMachineMonitoringType> m_machineMonitoringTypes = null;
    IMachineMonitoringType m_machineMonitoringType = null;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (MachineMonitoringTypeSelection).FullName);

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
    /// Is a null MachineMonitoringType a valid value ?
    /// </summary>
    [Category("Configuration"), Browsable(true), DefaultValue(false), Description("Is a null machine monitoring type valid ?")]
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
    /// Selected MachineMonitoringTypes
    /// 
    /// The returned list is not null, but may be empty or contain only one null element
    /// </summary>
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public IList<IMachineMonitoringType> SelectedMachineMonitoringTypes {
      get
      {
        IList<IMachineMonitoringType> list = new List<IMachineMonitoringType> ();
        if (!nullCheckBox.Checked) {
          foreach (object item in listBox.SelectedItems) {
            IMachineMonitoringType MachineMonitoringType = item as IMachineMonitoringType;
            list.Add (MachineMonitoringType);
          }
        }
        return list;
      }
      set {
        if(value != null && value.Count >= 1){
          this.m_machineMonitoringTypes = value;
        }
      }
    }

    /// <summary>
    /// Selected MachineMonitoringType
    /// Return the first selected (if multiselection) or null
    /// </summary>
    public IMachineMonitoringType SelectedMachineMonitoringType {
      get {
        IList<IMachineMonitoringType> machineMonitoringTypes = this.SelectedMachineMonitoringTypes;
        if (machineMonitoringTypes.Count >= 1 && !this.nullCheckBox.Checked) {
          return machineMonitoringTypes[0] as IMachineMonitoringType;
        }
        else {
          return null;
        }
      }
      set {
        this.m_machineMonitoringType = value;
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
    public MachineMonitoringTypeSelection()
    {
      //
      // The InitializeComponent() call is required for Windows Forms designer support.
      //
      InitializeComponent();
      
      nullCheckBox.Text = PulseCatalog.GetString ("MachineMonitoringTypeNull");
      
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
    /// Set Selected MachineMonitoringType in listbox
    /// </summary>
    private void SetSelectedMachineMonitoringType(){
      if(this.m_machineMonitoringType != null){
        int index = this.listBox.Items.IndexOf(this.m_machineMonitoringType);
        if(index >= 0){
          this.listBox.SetSelected(index, true);
        }
      }
    }

    /// <summary>
    /// Set Selected MachineMonitoringTypes in listbox
    /// </summary>
    private void SetSelectedMachineMonitoringTypes(){
      if(this.m_machineMonitoringTypes != null){
        int index = -1;
        foreach(IMachineMonitoringType MachineMonitoringType in this.m_machineMonitoringTypes){
          index = this.listBox.Items.IndexOf(MachineMonitoringType);
          if(index >= 0){
            this.listBox.SetSelected(index,true);
          }
        }
      }
    }
    #endregion // Methods
    
    void MachineMonitoringTypeSelectionLoad(object sender, EventArgs e)
    {
      IList<IMachineMonitoringType> machineMonitoringTypes;
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      
      if (daoFactory == null) {
        // to allow use in designer
        return;
      }

      using (IDAOSession session = daoFactory.OpenSession ())
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction ("MachineMonitoringTypeSelection.Load"))
      {
        machineMonitoringTypes =
          daoFactory.MachineMonitoringTypeDAO.FindAll ();
      }
      
      listBox.Items.Clear ();
      foreach (IMachineMonitoringType machineMonitoringType in machineMonitoringTypes) {
        listBox.Items.Add (machineMonitoringType);
      }
      listBox.ValueMember = DisplayedProperty;
      
      this.SetSelectedMachineMonitoringType();
      this.SetSelectedMachineMonitoringTypes();
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
