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
  public partial class MachineCategorySelection : UserControl
  {
    #region Members
    string m_displayedProperty = "Display";
    IList<IMachineCategory> m_MachineCategories = null;
    IMachineCategory m_machineCategory = null;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (MachineCategorySelection).FullName);

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
    /// Is a null MachineCategory a valid value ?
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
    /// Selected MachineCategories
    /// 
    /// The returned list is not null, but may be empty or contain only one null element
    /// </summary>
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public IList<IMachineCategory> SelectedMachineCategories {
      get
      {
        IList<IMachineCategory> list = new List<IMachineCategory> ();
        if (!nullCheckBox.Checked) {
          foreach (object item in listBox.SelectedItems) {
            IMachineCategory MachineCategory = item as IMachineCategory;
            list.Add (MachineCategory);
          }
        }
        return list;
      }
      set {
        if(value != null && value.Count >= 1){
          this.m_MachineCategories = value;
        }
      }
    }
    
    /// <summary>
    /// Selected MachineCategory
    /// Return the first selected (if multiselection) or null 
    /// </summary>
    public IMachineCategory SelectedMachineCategory {
      get {
        IList<IMachineCategory> machineCategories = this.SelectedMachineCategories;
        if (machineCategories.Count >= 1 && !this.nullCheckBox.Checked) {
          return machineCategories[0] as IMachineCategory;
        }
        else {
          return null;
        }
      }
      set {
        this.m_machineCategory = value;
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
    public MachineCategorySelection()
    {
      //
      // The InitializeComponent() call is required for Windows Forms designer support.
      //
      InitializeComponent();
      
      nullCheckBox.Text = PulseCatalog.GetString ("MachineCategoryNull");
      
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
    #endregion // Methods
    
    void MachineCategorySelectionLoad(object sender, EventArgs e)
    {
      IList<IMachineCategory> machineCategorys;
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      
      if (daoFactory == null) {
        // to allow use in designer
        return;
      }

      using (IDAOSession session = daoFactory.OpenSession ())
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction ("MachineCategorySelection.Load"))
      {
        machineCategorys = daoFactory.MachineCategoryDAO
          .FindAllSortedById ()
          .OrderBy (x => x)
          .ToList ();
      }
      
      listBox.Items.Clear ();
      foreach (IMachineCategory category in machineCategorys) {
        listBox.Items.Add (category);
      }
      listBox.ValueMember = DisplayedProperty;
      
      this.SetSelectedMachineCategory();
      this.SetSelectedMachineCategoryies();
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
    
    /// <summary>
    /// Set Selected MachineCategory in listbox
    /// </summary>
    private void SetSelectedMachineCategory(){
      if(this.m_machineCategory != null){
        int index = this.listBox.Items.IndexOf(this.m_machineCategory);
        if(index >= 0){
          this.listBox.SetSelected(index, true);
        }
      }
    }

    /// <summary>
    /// Set Selected MachineCategoryies in listbox
    /// </summary>
    private void SetSelectedMachineCategoryies(){
      if(this.m_MachineCategories != null){
        int index = -1;
        foreach(IMachineCategory MachineCategory in this.m_MachineCategories){
          index = this.listBox.Items.IndexOf(MachineCategory);
          if(index >= 0){
            this.listBox.SetSelected(index,true);
          }
        }
      }
    }
    
  }
}
