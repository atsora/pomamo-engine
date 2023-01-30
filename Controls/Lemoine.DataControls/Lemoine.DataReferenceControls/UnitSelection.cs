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
  public partial class UnitSelection : UserControl
  {
    #region Members
    string m_displayedProperty = "Display";
    IList<IUnit> m_units = null;
    IUnit m_unit = null;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (UnitSelection).FullName);

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
    /// Is a null Unit a valid value ?
    /// </summary>
    [Category("Configuration"), Browsable(true), DefaultValue(false), Description("Is a null unit valid ?")]
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
    /// Selected Units
    /// 
    /// The returned list is not null, but may be empty or contain only one null element
    /// </summary>
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public IList<IUnit> SelectedUnits {
      get
      {
        IList<IUnit> list = new List<IUnit> ();
        if (!nullCheckBox.Checked) {
          foreach (object item in listBox.SelectedItems) {
            IUnit Unit = item as IUnit;
            list.Add (Unit);
          }
        }
        return list;
      }
      set {
        if(value != null && value.Count >= 1){
          this.m_units = value;
        }
      }
    }

    /// <summary>
    /// Selected Unit
    /// Return the first selected (if multiselection) or null
    /// </summary>
    public IUnit SelectedUnit {
      get {
        IList<IUnit> units = this.SelectedUnits;
        if (units.Count >= 1 && !this.nullCheckBox.Checked) {
          return units[0] as IUnit;
        }
        else {
          return null;
        }
      }
      set {
        this.m_unit = value;
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
    public UnitSelection()
    {
      //
      // The InitializeComponent() call is required for Windows Forms designer support.
      //
      InitializeComponent();
      
      nullCheckBox.Text = PulseCatalog.GetString ("UnitNull");
      
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
    /// Set Selected Unit in listbox
    /// </summary>
    private void SetSelectedUnit(){
      if(this.m_unit != null){
        int index = this.listBox.Items.IndexOf(this.m_unit);
        if(index >= 0){
          this.listBox.SetSelected(index, true);
        }
      }
    }

    /// <summary>
    /// Set Selected Units in listbox
    /// </summary>
    private void SetSelectedUnits(){
      if(this.m_units != null){
        int index = -1;
        foreach(IUnit Unit in this.m_units){
          index = this.listBox.Items.IndexOf(Unit);
          if(index >= 0){
            this.listBox.SetSelected(index,true);
          }
        }
      }
    }
    #endregion // Methods
    
    void UnitSelectionLoad(object sender, EventArgs e)
    {
      IList<IUnit> units;
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      
      if (daoFactory == null) {
        // to allow use in designer
        return;
      }
      
      using (IDAOSession session = daoFactory.OpenSession ())
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction ("UnitSelection.Load"))
      {
        units =
          daoFactory.UnitDAO.FindAll ();
      }
      
      listBox.Items.Clear ();
      foreach (IUnit unit in units) {
        listBox.Items.Add (unit);
      }
      listBox.ValueMember = DisplayedProperty;
      
      this.SetSelectedUnit();
      this.SetSelectedUnits();
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
