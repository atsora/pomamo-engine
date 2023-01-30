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
using System.Linq;

namespace Lemoine.DataReferenceControls
{
  /// <summary>
  /// Control to select a machine mode
  /// </summary>
  public partial class MachineFilterSelection : UserControl
  {
    #region Members
    string m_displayedProperty = "SelectionText";
    IList<IMachineFilter> m_machineFilters = null;
    IMachineFilter m_machineFilter = null;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (MachineFilterSelection).FullName);

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
    /// Is a null MachineFilter a valid value ?
    /// </summary>
    [Category("Configuration"), Browsable(true), DefaultValue(false), Description("Is a null MachineFilter valid ?")]
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
    /// Selected Machines
    /// 
    /// The returned list is not null, but may be empty or contain only one null element
    /// </summary>
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public IList<IMachineFilter> SelectedMachineFilters {
      get
      {
        IList<IMachineFilter> list = new List<IMachineFilter> ();
        if (!nullCheckBox.Checked) {
          foreach (object item in listBox.SelectedItems) {
            IMachineFilter MachineFilter = item as IMachineFilter;
            list.Add (MachineFilter);
          }
        }
        return list;
      }
      set {
        if (value != null && value.Count >= 1) {
          this.m_machineFilters = value;
        }
        else {
          this.m_machineFilters = null;
        }
        SetSelectedMachineFilters ();
      }
    }

    /// <summary>
    /// Selected MachineFilter
    /// Return the first selected (if multiselection) or null
    /// </summary>
    public IMachineFilter SelectedMachineFilter {
      get {
        IList<IMachineFilter> machineFilters = this.SelectedMachineFilters;
        if (machineFilters.Count >= 1 && !this.nullCheckBox.Checked) {
          return machineFilters[0] as IMachineFilter;
        }
        else {
          return null;
        }
      }
      set {
        this.m_machineFilter = value;
        SetSelectedMachineFilter ();
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
    public MachineFilterSelection()
    {
      //
      // The InitializeComponent() call is required for Windows Forms designer support.
      //
      InitializeComponent();
      
      nullCheckBox.Text = PulseCatalog.GetString ("MachineFilterNull");
      
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
    /// Set Selected MachineFilter in listbox
    /// </summary>
    private void SetSelectedMachineFilter()
    {
      nullCheckBox.Checked = (null == m_machineFilter);
      listBox.Enabled = !nullCheckBox.Checked;
      listBox.ClearSelected ();
      if (this.m_machineFilter != null) {
        int index = this.listBox.Items.IndexOf (this.m_machineFilter);
        if (index >= 0) {
          this.listBox.SetSelected (index, true);
        }
      }
    }

    /// <summary>
    /// Set Selected Machines in listbox
    /// </summary>
    private void SetSelectedMachineFilters()
    {
      nullCheckBox.Checked = (null == m_machineFilters) || !this.m_machineFilters.Any ();
      listBox.Enabled = !nullCheckBox.Checked;
      listBox.ClearSelected ();
      if (this.m_machineFilters != null) {
        int index = -1;
        foreach (IMachineFilter MachineFilter in this.m_machineFilters) {
          index = this.listBox.Items.IndexOf (MachineFilter);
          if (index >= 0) {
            this.listBox.SetSelected (index, true);
          }
        }
      }
    }
    #endregion // Methods
    
    void MachineFilterSelectionLoad(object sender, EventArgs e)
    {
      IList<IMachineFilter> machineFilters;
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      
      if (daoFactory == null) {
        // to allow use in designer
        return;
      }
      
      using (IDAOSession session = daoFactory.OpenSession ())
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction ("MachineFilterSelection.Load"))
      {
        machineFilters =
          daoFactory.MachineFilterDAO.FindAll ();
      }
      
      listBox.Items.Clear ();
      foreach (IMachineFilter machineFilter in machineFilters) {
        listBox.Items.Add (machineFilter);
      }
      listBox.ValueMember = DisplayedProperty;
      
      this.SetSelectedMachineFilter();
      this.SetSelectedMachineFilters();
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
