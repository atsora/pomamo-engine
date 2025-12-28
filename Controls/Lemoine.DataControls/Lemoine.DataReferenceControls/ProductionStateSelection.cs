// Copyright (C) 2025 Atsora Solutions
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
  /// Control to select a production state
  /// </summary>
  public partial class ProductionStateSelection : UserControl
  {
    #region Members
    string m_displayedProperty = "Display";
    IList<IProductionState> m_productionStates = null;
    IProductionState m_productionState = null;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (ProductionStateSelection).FullName);

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
    /// Is a null ProductionState a valid value ?
    /// </summary>
    [Category("Configuration"), Browsable(true), DefaultValue(false), Description("Is a null ProductionState valid ?")]
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
    /// Selected ProductionStates
    /// 
    /// The returned list is not null, but may be empty or contain only one null element
    /// </summary>
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public IList<IProductionState> SelectedProductionStates {
      get
      {
        IList<IProductionState> list = new List<IProductionState> ();
        if (!nullCheckBox.Checked) {
          foreach (object item in listBox.SelectedItems) {
            IProductionState productionState = item as IProductionState;
            list.Add (productionState);
          }
        }
        return list;
      }
      set {
        if(value != null && value.Count >= 1){
          this.m_productionStates = value;
        }
      }
    }

    /// <summary>
    /// Selected ProductionState
    /// Return the first selected (if multiselection) or null
    /// </summary>
    public IProductionState SelectedProductionState {
      get {
        IList<IProductionState> productionStates = this.SelectedProductionStates;
        if (productionStates.Count >= 1 && !this.nullCheckBox.Checked) {
          return productionStates[0] as IProductionState;
        }
        else {
          return null;
        }
      }
      set {
        this.m_productionState = value;
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
    public ProductionStateSelection()
    {
      //
      // The InitializeComponent() call is required for Windows Forms designer support.
      //
      InitializeComponent();
      
      nullCheckBox.Text = PulseCatalog.GetString ("ProductionStateNull");
      
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
    /// Set Selected ProductionState in listbox
    /// </summary>
    private void SetSelectedProductionState(){
      if(this.m_productionState != null){
        int index = this.listBox.Items.IndexOf(this.m_productionState);
        if(index >= 0){
          this.listBox.SetSelected(index, true);
        }
      }
    }

    /// <summary>
    /// Set Selected ProductionStates in listbox
    /// </summary>
    private void SetSelectedProductionStates(){
      if(this.m_productionStates != null){
        int index = -1;
        foreach(IProductionState productionState in this.m_productionStates){
          index = this.listBox.Items.IndexOf(productionState);
          if(index >= 0){
            this.listBox.SetSelected(index,true);
          }
        }
      }
    }
    #endregion // Methods
    
    void ProductionStateSelectionLoad(object sender, EventArgs e)
    {
      IList<IProductionState> productionStates;
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      
      if (daoFactory == null) {
        // to allow use in designer
        return;
      }
      
      using (IDAOSession session = daoFactory.OpenSession ())
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction ("ProductionStateSelection.Load"))
      {
        productionStates = daoFactory.ProductionStateDAO
          .FindAll ()
          .OrderBy (x => x)
          .ToList ();
      }
      
      listBox.Items.Clear ();
      foreach (IProductionState productionState in productionStates) {
        listBox.Items.Add (productionState);
      }
      listBox.ValueMember = DisplayedProperty;
      
      this.SetSelectedProductionState();
      this.SetSelectedProductionStates();
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