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
  public partial class MachineSubCategorySelection : UserControl
  {
    #region Members
    string m_displayedProperty = "Display";
    IList<IMachineSubCategory> m_machineSubCategories = null;
    IMachineSubCategory m_machineSubCategory = null;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (MachineSubCategorySelection).FullName);

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
    /// Is a null MachineSubCategory a valid value ?
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
    /// Selected MachineSubCategories
    /// 
    /// The returned list is not null, but may be empty or contain only one null element
    /// </summary>
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public IList<IMachineSubCategory> SelectedMachineSubCategories {
      get
      {
        IList<IMachineSubCategory> list = new List<IMachineSubCategory> ();
        if (!nullCheckBox.Checked) {
          foreach (object item in listBox.SelectedItems) {
            IMachineSubCategory MachineSubCategory = item as IMachineSubCategory;
            list.Add (MachineSubCategory);
          }
        }
        return list;
      }
      set {
        if(value != null && value.Count >= 1){
          this.m_machineSubCategories = value;
        }
      }
    }

    /// <summary>
    /// Selected MachineSubCategory
    /// Return the first selected (if multiselection) or null
    /// </summary>
    public IMachineSubCategory SelectedMachineSubCategory {
      get {
        IList<IMachineSubCategory> machineSubCategories = this.SelectedMachineSubCategories;
        if (machineSubCategories.Count >= 1 && !this.nullCheckBox.Checked) {
          return machineSubCategories[0] as IMachineSubCategory;
        }
        else {
          return null;
        }
      }
      set {
        this.m_machineSubCategory = value;
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
    public MachineSubCategorySelection()
    {
      //
      // The InitializeComponent() call is required for Windows Forms designer support.
      //
      InitializeComponent();
      
      nullCheckBox.Text = PulseCatalog.GetString ("MachineSubCategoryNull");
      
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
    /// Set Selected MachineSubCategory in listbox
    /// </summary>
    private void SetSelectedMachineSubCategory(){
      if(this.m_machineSubCategory != null){
        int index = this.listBox.Items.IndexOf(this.m_machineSubCategory);
        if(index >= 0){
          this.listBox.SetSelected(index, true);
        }
      }
    }

    /// <summary>
    /// Set Selected MachineSubCategories in listbox
    /// </summary>
    private void SetSelectedMachineSubCategories(){
      if(this.m_machineSubCategories != null){
        int index = -1;
        foreach(IMachineSubCategory MachineSubCategory in this.m_machineSubCategories){
          index = this.listBox.Items.IndexOf(MachineSubCategory);
          if(index >= 0){
            this.listBox.SetSelected(index,true);
          }
        }
      }
    }
    #endregion // Methods
    
    void MachineSubCategorySelectionLoad(object sender, EventArgs e)
    {
      IList<IMachineSubCategory> machineSubCategorys;
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      
      if (daoFactory == null) {
        // to allow use in designer
        return;        
      }
      
      using (IDAOSession session = daoFactory.OpenSession ())
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction ("MachineSubCategorySelection.Load"))
      {
        machineSubCategorys = daoFactory.MachineSubCategoryDAO
          .FindAllSortedById ()
          .OrderBy (x => x)
          .ToList ();
      }
      
      listBox.Items.Clear ();
      foreach (IMachineSubCategory subCategory in machineSubCategorys) {
        listBox.Items.Add (subCategory);
      }
      listBox.ValueMember = DisplayedProperty;
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
