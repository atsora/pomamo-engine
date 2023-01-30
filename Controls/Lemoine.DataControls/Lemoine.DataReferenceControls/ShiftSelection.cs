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
  public partial class ShiftSelection : UserControl
  {
    #region Members
    string m_displayedProperty = "Display";
    IList<IShift> m_shifts = null;
    IShift m_shift = null;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (ShiftSelection).FullName);

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
    /// Is a null Xxx a valid value ?
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
    /// Selected Shifts
    /// 
    /// The returned list is not null, but may be empty or contain only one null element
    /// </summary>
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public IList<IShift> SelectedShifts {
      get
      {
        IList<IShift> list = new List<IShift> ();
        if (!nullCheckBox.Checked) {
          foreach (object item in listBox.SelectedItems) {
            IShift shift = item as IShift;
            list.Add (shift);
          }
        }
        return list;
      }
      set {
        if(value != null && value.Count >= 1){
          this.m_shifts = value;
        }
      }
    }

    /// <summary>
    /// Selected Shift
    /// Return the first selected (if multiselection) or null
    /// </summary>
    public IShift SelectedShift {
      get {
        IList<IShift> shifts = this.SelectedShifts;
        if (shifts.Count >= 1 && !this.nullCheckBox.Checked) {
          return shifts[0] as IShift;
        }
        else {
          return null;
        }
      }
      set {
        this.m_shift = value;
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
    public ShiftSelection()
    {
      //
      // The InitializeComponent() call is required for Windows Forms designer support.
      //
      InitializeComponent();
      
      nullCheckBox.Text = PulseCatalog.GetString ("ShiftNull");
      
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
    /// Set Selected Shift in listbox
    /// </summary>
    private void SetSelectedShift(){
      if(this.m_shift != null){
        int index = this.listBox.Items.IndexOf(this.m_shift);
        if(index >= 0){
          this.listBox.SetSelected(index, true);
        }
      }
    }

    /// <summary>
    /// Set Selected Shifts in listbox
    /// </summary>
    private void SetSelectedShifts(){
      if(this.m_shifts != null){
        int index = -1;
        foreach(IShift shift in this.m_shifts){
          index = this.listBox.Items.IndexOf(shift);
          if(index >= 0){
            this.listBox.SetSelected(index,true);
          }
        }
      }
    }
    #endregion // Methods
        
    void ShiftSelectionLoad(object sender, EventArgs e) {
      ShiftSelectionLoad();
    }
    
    void ShiftSelectionLoad()
    {
      IList<IShift> allShifts;
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      
      if (daoFactory == null) {
        // to allow use in designer
        return;
      }
      
      using (IDAOSession session = daoFactory.OpenSession ())
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction ("ShiftSelection.Load"))
      {
        allShifts = daoFactory.ShiftDAO
          .FindAll()
          .OrderBy (x => x)
          .ToList ();
      }
      
      listBox.Items.Clear ();
      foreach (IShift shift in allShifts) {
        listBox.Items.Add (shift);
      }
      listBox.ValueMember = DisplayedProperty;
      
      if (listBox.Items.Count > 0) {
        listBox.SelectedIndex = 0;
      } else {
        listBox.Enabled = false;
        this.nullCheckBox.Checked = true;
      }
      
      this.SetSelectedShift();
      this.SetSelectedShifts();
    }
    
    void NullCheckBoxCheckedChanged(object sender, EventArgs e)
    {
      if (nullCheckBox.Checked) {
        listBox.Enabled = false;
        listBox.SelectedIndex = -1;
      }
      else {
        if (listBox.Items.Count > 0) {
          listBox.SelectedIndex = 0;
          listBox.Enabled = true;
        } else {
          nullCheckBox.Checked = true;
        }
      }
      
      OnAfterSelect (new EventArgs ());
    }

    void ListBoxSelectedIndexChanged(object sender, EventArgs e)
    {
      OnAfterSelect (new EventArgs ());
    }
  }
}
