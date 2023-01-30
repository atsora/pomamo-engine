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
  public partial class MachineObservationStateSelection : UserControl
  {
    #region Members
    string m_displayedProperty = "Display";
    IList<IMachineObservationState> m_machineObservationStates = null;
    IMachineObservationState m_machineObservationState = null;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (MachineObservationStateSelection).FullName);

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
    /// Is a null MachineObservationState a valid value ?
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
    /// Selected MachineObservationStates
    /// 
    /// The returned list is not null, but may be empty or contain only one null element
    /// </summary>
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public IList<IMachineObservationState> SelectedMachineObservationStates {
      get
      {
        IList<IMachineObservationState> list = new List<IMachineObservationState> ();
        if (!nullCheckBox.Checked) {
          foreach (object item in listBox.SelectedItems) {
            IMachineObservationState MachineObservationState = item as IMachineObservationState;
            list.Add (MachineObservationState);
          }
        }
        return list;
      }
      set {
        if(value != null && value.Count >= 1){
          this.m_machineObservationStates = value;
        }
      }
    }

    /// <summary>
    /// Selected MachineObservationState
    /// Return the first selected (if multiselection) or null
    /// </summary>
    public IMachineObservationState SelectedMachineObservationState {
      get {
        IList<IMachineObservationState> machineObservationStates = this.SelectedMachineObservationStates;
        if (machineObservationStates.Count >= 1 && !this.nullCheckBox.Checked) {
          return machineObservationStates[0] as IMachineObservationState;
        }
        else {
          return null;
        }
      }
      set {
        this.m_machineObservationState = value;
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
    public MachineObservationStateSelection()
    {
      //
      // The InitializeComponent() call is required for Windows Forms designer support.
      //
      InitializeComponent();
      
      nullCheckBox.Text = PulseCatalog.GetString ("MachineObservationStateNull");
      
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
    /// Set Selected MachineObservationState in listbox
    /// </summary>
    private void SetSelectedMachineObservationState(){
      if(this.m_machineObservationState != null){
        int index = this.listBox.Items.IndexOf(this.m_machineObservationState);
        if(index >= 0){
          this.listBox.SetSelected(index, true);
        }
      }
    }

    /// <summary>
    /// Set Selected MachineObservationStates in listbox
    /// </summary>
    private void SetSelectedMachineObservationStates(){
      if(this.m_machineObservationStates != null){
        int index = -1;
        foreach(IMachineObservationState MachineObservationState in this.m_machineObservationStates){
          index = this.listBox.Items.IndexOf(MachineObservationState);
          if(index >= 0){
            this.listBox.SetSelected(index,true);
          }
        }
      }
    }
    #endregion // Methods
    
    /// <summary>
    /// Reload the control
    /// </summary>
    public void Reload ()
    {
      MachineObservationStateSelectionLoad ();
    }
    
    void MachineObservationStateSelectionLoad(object sender, EventArgs e)
    {
      MachineObservationStateSelectionLoad();
    }
    
    void MachineObservationStateSelectionLoad ()
    {
      IList<IMachineObservationState> machineObservationStates;
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      
      if (daoFactory == null) {
        // to allow use in designer
        return;
      }
      
      using (IDAOSession session = daoFactory.OpenSession ())
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction ("MachineObservationStateSelection.Load"))
      {
        machineObservationStates =
          daoFactory.MachineObservationStateDAO.FindAll ();
      }
      
      listBox.Items.Clear ();
      foreach (IMachineObservationState machineObservationState in machineObservationStates) {
        listBox.Items.Add (machineObservationState);
      }
      listBox.ValueMember = DisplayedProperty;
      
      this.SetSelectedMachineObservationState();
      this.SetSelectedMachineObservationStates();
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
