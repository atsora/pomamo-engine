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
  public partial class CadModelSelection : UserControl
  {
    #region Members
    string m_displayedProperty = "Display";
    IList<ICadModel> m_cadModels = null;
    ICadModel m_cadModel = null;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (CadModelSelection).FullName);

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
    /// Is a null CadModel a valid value ?
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
    /// Selected CadModels
    /// 
    /// The returned list is not null, but may be empty or contain only one null element
    /// </summary>
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public IList<ICadModel> SelectedCadModels {
      get
      {
        IList<ICadModel> list =
          new List<ICadModel> ();
        if (!nullCheckBox.Checked) {
          foreach (object item in listBox.SelectedItems) {
            ICadModel cadModel =
              item as ICadModel;
            list.Add (cadModel);
          }
        }
        return list;
      }
      set {
        if(value != null && value.Count >= 1){
          this.m_cadModels = value;
        }
      }
    }
    
    /// <summary>
    /// Selected CadModel
    /// Return the first selected (if multiselection) or null 
    /// </summary>
    public ICadModel SelectedCadModel {
      get {
        IList<ICadModel> cadModels = this.SelectedCadModels;
        if (cadModels.Count >= 1 && !this.nullCheckBox.Checked) {
          return cadModels[0] as ICadModel;
        }
        else {
          return null;
        }
      }
      set {
        this.m_cadModel = value;
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
    public CadModelSelection()
    {
      //
      // The InitializeComponent() call is required for Windows Forms designer support.
      //
      InitializeComponent();
      
      nullCheckBox.Text = PulseCatalog.GetString ("CadModelNull");
      
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
    /// Set Selected CadModel in listbox
    /// </summary>
    private void SetSelectedCadModel(){
      if(this.m_cadModel != null){
        int index = this.listBox.Items.IndexOf(this.m_cadModel);
        if(index >= 0){
          this.listBox.SetSelected(index, true);
        }
      }
    }
    
    /// <summary>
    /// Set Selected CadModels in listbox
    /// </summary>
    private void SetSelectedCadModels(){
      if(this.m_cadModels != null){
        int index = -1;
        foreach(ICadModel cadModel in this.m_cadModels){
          index = this.listBox.Items.IndexOf(cadModel);
          if(index >= 0){
            this.listBox.SetSelected(index,true);
          }
        }
      }
    }
    #endregion // Methods
    
    void CadModelSelectionLoad(object sender, EventArgs e)
    {
      IList<ICadModel> cadModels;
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      
      if (daoFactory == null) {
        // to allow use in designer
        return;
      }
      
      using (IDAOSession session = daoFactory.OpenSession ())
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction ("CadModelSelection.Load"))
      {
        cadModels =
          daoFactory.CadModelDAO.FindAll ();
      }
      
      listBox.Items.Clear ();
      foreach (ICadModel cadModel in cadModels) {
        listBox.Items.Add (cadModel);
      }
      listBox.ValueMember = DisplayedProperty;
      
      this.SetSelectedCadModel();
      this.SetSelectedCadModels();
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
