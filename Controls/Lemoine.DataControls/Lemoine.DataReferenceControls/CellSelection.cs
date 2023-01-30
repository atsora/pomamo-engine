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
  public partial class CellSelection : UserControl
  {
    #region Members
    string m_displayedProperty = "SelectionText";
    IList<ICell> m_cells = null;
    ICell m_cell = null;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (CellSelection).FullName);

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
    /// Is a null Cell a valid value ?
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
    /// Selected Cells
    /// 
    /// The returned list is not null, but may be empty or contain only one null element
    /// </summary>
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public IList<ICell> SelectedCells {
      get
      {
        IList<ICell> list = new List<ICell> ();
        if (!nullCheckBox.Checked) {
          foreach (object item in listBox.SelectedItems) {
            ICell Cell = item as ICell;
            list.Add (Cell);
          }
        }
        return list;
      }
      set {
        if(value != null && value.Count >= 1){
          this.m_cells = value;
        }
      }
    }
    
    /// <summary>
    /// Selected Cell
    /// Return the first selected (if multiselection) or null
    /// </summary>
    public ICell SelectedCell {
      get {
        IList<ICell> cells = this.SelectedCells;
        if (cells.Count >= 1 && !this.nullCheckBox.Checked) {
          return cells[0] as ICell;
        }
        else {
          return null;
        }
      }
      set {
        this.m_cell = value;
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
    public CellSelection()
    {
      //
      // The InitializeComponent() call is required for Windows Forms designer support.
      //
      InitializeComponent();
      
      nullCheckBox.Text = PulseCatalog.GetString ("CellNull");
      
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
    /// Set Selected Cell in listbox
    /// </summary>
    private void SetSelectedCell(){
      if(this.m_cell != null){
        int index = this.listBox.Items.IndexOf(this.m_cell);
        if(index >= 0){
          this.listBox.SetSelected(index, true);
        }
      }
    }

    /// <summary>
    /// Set Selected Cells in listbox
    /// </summary>
    private void SetSelectedCells(){
      if(this.m_cells != null){
        int index = -1;
        foreach(ICell Cell in this.m_cells){
          index = this.listBox.Items.IndexOf(Cell);
          if(index >= 0){
            this.listBox.SetSelected(index,true);
          }
        }
      }
    }
    #endregion // Methods
    
    void CellSelectionLoad(object sender, EventArgs e)
    {
      IList<ICell> cells;
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      
      if (daoFactory == null) {
        // to allow use in designer
        return;
      }
      
      using (IDAOSession session = daoFactory.OpenSession ())
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction ("CellSelection.Load"))
      {
        cells = daoFactory.CellDAO.FindAllSortedById ()
          .OrderBy (x => x)
          .ToList ();
      }
      
      listBox.Items.Clear ();
      foreach (ICell cell in cells) {
        listBox.Items.Add (cell);
      }
      listBox.ValueMember = DisplayedProperty;
      
      this.SetSelectedCell();
      this.SetSelectedCells();
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
