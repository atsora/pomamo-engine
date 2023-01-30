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
  /// Control to select a Cnc Acquisition config
  /// </summary>
  public partial class CncAcquisitionSelection : UserControl
  {
    #region Members
    string m_displayedProperty = "Display";
    IList<ICncAcquisition> m_cncAcquisitions = null;
    ICncAcquisition m_cncAcquisition = null;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (CncAcquisitionSelection).FullName);

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
    /// Is a null CncAcquisition a valid value ?
    /// </summary>
    [Category("Configuration"), Browsable(true), DefaultValue(false), Description("Is a null Cnc Acquisition valid ?")]
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
    /// Selected CncAcquisitions
    /// 
    /// The returned list is not null, but may be empty or contain only one null element
    /// </summary>
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public IList<ICncAcquisition> SelectedCncAcquisitions {
      get
      {
        IList<ICncAcquisition> list = new List<ICncAcquisition> ();
        if (!nullCheckBox.Checked) {
          foreach (object item in listBox.SelectedItems) {
            ICncAcquisition CncAcquisition = item as ICncAcquisition;
            list.Add (CncAcquisition);
          }
        }
        return list;
      }
      set {
        if(value != null && value.Count >= 1){
          this.m_cncAcquisitions = value;
        }
      }
    }

    /// <summary>
    /// Selected CncAcquisition
    /// Return the first selected (if multiselection) or null
    /// </summary>
    public ICncAcquisition SelectedCncAcquisition {
      get {
        IList<ICncAcquisition> cncAcquisitions = this.SelectedCncAcquisitions;
        if (cncAcquisitions.Count >= 1 && !this.nullCheckBox.Checked) {
          return cncAcquisitions[0] as ICncAcquisition;
        }
        else {
          return null;
        }
      }
      set {
        this.m_cncAcquisition = value;
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
    public CncAcquisitionSelection()
    {
      //
      // The InitializeComponent() call is required for Windows Forms designer support.
      //
      InitializeComponent();
      
      nullCheckBox.Text = PulseCatalog.GetString ("CncAcquisitionNull");
      
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
    /// Set Selected CncAcquisition in listbox
    /// </summary>
    private void SetSelectedCncAcquisition(){
      if(this.m_cncAcquisition != null){
        int index = this.listBox.Items.IndexOf(this.m_cncAcquisition);
        if(index >= 0){
          this.listBox.SetSelected(index, true);
        }
      }
    }

    /// <summary>
    /// Set Selected CncAcquisitions in listbox
    /// </summary>
    private void SetSelectedCncAcquisitions(){
      if(this.m_cncAcquisitions != null){
        int index = -1;
        foreach(ICncAcquisition CncAcquisition in this.m_cncAcquisitions){
          index = this.listBox.Items.IndexOf(CncAcquisition);
          if(index >= 0){
            this.listBox.SetSelected(index,true);
          }
        }
      }
    }
    #endregion // Methods
    
    void CncAcquisitionSelectionLoad(object sender, EventArgs e)
    {
      IList<ICncAcquisition> cncAcquisitions;
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      
      if (daoFactory == null) {
        // to allow use in designer
        return;
      }
      
      using (IDAOSession session = daoFactory.OpenSession ())
      {
        cncAcquisitions = daoFactory.CncAcquisitionDAO.FindAll ();
      }
      
      listBox.Items.Clear ();
      foreach (ICncAcquisition cncAcquisition in cncAcquisitions) {
        listBox.Items.Add (cncAcquisition);
      }
      listBox.ValueMember = DisplayedProperty;
      
      this.SetSelectedCncAcquisition();
      this.SetSelectedCncAcquisitions();
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
