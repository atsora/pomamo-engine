// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
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
  public partial class MonitoredMachineSelection : UserControl
  {
    #region Members
    string m_displayedProperty = "Display";
    IList<IMonitoredMachine> m_monitoredMachines = null;
    IMonitoredMachine m_monitoredMachine = null;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (MonitoredMachineSelection).FullName);

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
    /// Is a null MonitoredMachine a valid value ?
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
    /// Selected MonitoredMachines
    /// 
    /// The returned list is not null, but may be empty or contain only one null element
    /// </summary>
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public IList<IMonitoredMachine> SelectedMonitoredMachines {
      get
      {
        IList<IMonitoredMachine> list = new List<IMonitoredMachine> ();
        if (!nullCheckBox.Checked) {
          foreach (object item in listBox.SelectedItems) {
            IMonitoredMachine MonitoredMachine = item as IMonitoredMachine;
            list.Add (MonitoredMachine);
          }
        }
        return list;
      }
      set {
        if(value != null && value.Count >= 1){
          this.m_monitoredMachines = value;
        }
      }
    }

    /// <summary>
    /// Selected MonitoredMachine
    /// Return the first selected (if multiselection) or null
    /// </summary>
    public IMonitoredMachine SelectedMonitoredMachine {
      get {
        IList<IMonitoredMachine> monitoredMachines = this.SelectedMonitoredMachines;
        if (monitoredMachines.Count >= 1 && !this.nullCheckBox.Checked) {
          return monitoredMachines[0] as IMonitoredMachine;
        }
        else {
          return null;
        }
      }
      set {
        this.m_monitoredMachine = value;
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
    public MonitoredMachineSelection()
    {
      //
      // The InitializeComponent() call is required for Windows Forms designer support.
      //
      InitializeComponent();
      
      nullCheckBox.Text = PulseCatalog.GetString ("MonitoredMachineNull");
      
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
    /// Set Selected MonitoredMachine in listbox
    /// </summary>
    private void SetSelectedMonitoredMachine(){
      if(this.m_monitoredMachine != null){
    	int index = this.listBox.Items.IndexOf(this.m_monitoredMachine);
    	if(index >= 0){
    	  this.listBox.SetSelected(index, true);
    	}
      }
    }
    
    /// <summary>
    /// Set Selected MonitoredMachines in listbox
    /// </summary>
    private void SetSelectedMonitoredMachines(){
      if(this.m_monitoredMachines != null){
    	int index = -1;
    	foreach(IMonitoredMachine MonitoredMachine in this.m_monitoredMachines){
    	  index = this.listBox.Items.IndexOf(MonitoredMachine);
    	  if(index >= 0){
    		this.listBox.SetSelected(index,true);
    	  }
    	}
      }
    }
    #endregion // Methods
    
    void MonitoredMachineSelectionLoad(object sender, EventArgs e)
    {
      IList<IMonitoredMachine> monitoredMachines;
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      
      if (daoFactory == null) {
        // to allow use in designer
        return;
      }
      
      using (IDAOSession session = daoFactory.OpenSession ())
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction ("MonitoredMachineSelection.Load"))
      {
        monitoredMachines = daoFactory.MonitoredMachineDAO
          .FindAllOrderByName ()
          .OrderBy (x => x)
          .ToList ();
      }
      
      listBox.Items.Clear ();
      foreach (IMonitoredMachine monitoredMachine in monitoredMachines) {
        listBox.Items.Add (monitoredMachine);
      }
      listBox.ValueMember = DisplayedProperty;
      
      this.SetSelectedMonitoredMachine();
      this.SetSelectedMonitoredMachines();
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
