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
  /// Control to select a company
  /// </summary>
  public partial class CompanySelection : UserControl
  {
    static readonly ILog log = LogManager.GetLogger(typeof (CompanySelection).FullName);
    
    #region Members
    string m_displayedProperty = "SelectionText";
    IList<ICompany> m_companies = null;
    ICompany m_company = null;
    #endregion // Members

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
    /// Is a null Company a valid value ?
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
    [Category("Configuration"), Browsable(true), DefaultValue("SelectionText"), Description("Property to display")]
    public string DisplayedProperty {
      get { return  m_displayedProperty; }
      set { m_displayedProperty = value; }
    }

    /// <summary>
    /// Selected Companies
    /// 
    /// The returned list is not null, but may be empty or contain only one null element
    /// </summary>
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public IList<ICompany> SelectedCompanies {
      get
      {
        IList<ICompany> list = new List<ICompany> ();
        if (!nullCheckBox.Checked) {
          foreach (object item in listBox.SelectedItems) {
            ICompany Company = item as ICompany;
            list.Add (Company);
          }
        }
        return list;
      }
      set {
        if(value != null && value.Count >= 1){
          this.m_companies = value;
        }
      }
    }

    /// <summary>
    /// Selected Company
    /// Return the first selected (if multiselection) or null
    /// </summary>
    public ICompany SelectedCompany {
      get {
        IList<ICompany> companies = this.SelectedCompanies;
        if (companies.Count >= 1 && !this.nullCheckBox.Checked) {
          return companies[0] as ICompany;
        }
        else {
          return null;
        }
      }
      set {
        this.m_company = value;
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
    public CompanySelection()
    {
      //
      // The InitializeComponent() call is required for Windows Forms designer support.
      //
      InitializeComponent();
      
      nullCheckBox.Text = PulseCatalog.GetString ("CompanyNull");
      
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
    /// Set Selected Company in listbox
    /// </summary>
    private void SetSelectedCompany(){
      if(this.m_company != null){
        int index = this.listBox.Items.IndexOf(this.m_company);
        if(index >= 0){
          this.listBox.SetSelected(index, true);
        }
      }
    }

    /// <summary>
    /// Set Selected Companies in listbox
    /// </summary>
    private void SetSelectedCompanies(){
      if(this.m_companies != null){
        int index = -1;
        foreach(ICompany Company in this.m_companies){
          index = this.listBox.Items.IndexOf(Company);
          if(index >= 0){
            this.listBox.SetSelected(index,true);
          }
        }
      }
    }
    #endregion // Methods
    
    void CompanySelectionLoad(object sender, EventArgs e)
    {
      IList<ICompany> companys;
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      
      if (daoFactory == null) {
        // to allow use in designer
        return;
      }
      
      using (IDAOSession session = daoFactory.OpenSession ())
      {
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction ("CompanySelection.Load")) {
          companys = daoFactory.CompanyDAO
            .FindAllSortedById ()
            .OrderBy (x => x)
            .ToList ();
        }
      }
      
      listBox.Items.Clear ();
      foreach (ICompany company in companys) {
        listBox.Items.Add (company);
      }
      listBox.ValueMember = DisplayedProperty;
      
      this.SetSelectedCompany();
      this.SetSelectedCompanies();
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
