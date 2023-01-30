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
  /// Control to select a computer
  /// </summary>
  public partial class ComputerSelection : UserControl
  {
    #region Members
    string m_displayedProperty = "Display";
    IList<IComputer> m_computers = null;
    IComputer m_computer = null;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (ComputerSelection).FullName);

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
    /// Is a null Computer a valid value ?
    /// </summary>
    [Category("Configuration"), Browsable(true), DefaultValue(false), Description("Is a null Computer valid ?")]
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
    /// Selected Computers
    /// 
    /// The returned list is not null, but may be empty or contain only one null element
    /// </summary>
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public IList<IComputer> SelectedComputers {
      get
      {
        IList<IComputer> list = new List<IComputer> ();
        if (!nullCheckBox.Checked) {
          foreach (object item in listBox.SelectedItems) {
            IComputer Computer = item as IComputer;
            list.Add (Computer);
          }
        }
        return list;
      }
      set {
        if(value != null && value.Count >= 1){
          this.m_computers = value;
        }
      }
    }

    /// <summary>
    /// Selected Computer
    /// Return the first selected (if multiselection) or null
    /// </summary>
    public IComputer SelectedComputer {
      get {
        IList<IComputer> computers = this.SelectedComputers;
        if (computers.Count >= 1 && !this.nullCheckBox.Checked) {
          return computers[0] as IComputer;
        }
        else {
          return null;
        }
      }
      set {
        this.m_computer = value;
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
    public ComputerSelection()
    {
      //
      // The InitializeComponent() call is required for Windows Forms designer support.
      //
      InitializeComponent();
      
      nullCheckBox.Text = PulseCatalog.GetString ("ComputerNull");
      
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
    /// Set Selected Computer in listbox
    /// </summary>
    private void SetSelectedComputer(){
      if(this.m_computer != null){
        int index = this.listBox.Items.IndexOf(this.m_computer);
        if(index >= 0){
          this.listBox.SetSelected(index, true);
        }
      }
    }

    /// <summary>
    /// Set Selected Computers in listbox
    /// </summary>
    private void SetSelectedComputers(){
      if(this.m_computers != null){
        int index = -1;
        foreach(IComputer computer in this.m_computers){
          index = this.listBox.Items.IndexOf(computer);
          if(index >= 0){
            this.listBox.SetSelected(index,true);
          }
        }
      }
    }
    #endregion // Methods
    
    void ComputerSelectionLoad(object sender, EventArgs e)
    {
      IList<IComputer> computers;
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      
      if (daoFactory == null) {
        // to allow use in designer
        return;
      }
      
      using (IDAOSession session = daoFactory.OpenSession ())
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction ("ComputerSelection.Load"))
      {
        computers =
          daoFactory.ComputerDAO.FindAll ();
      }
      
      listBox.Items.Clear ();
      foreach (IComputer computer in computers) {
        listBox.Items.Add (computer);
      }
      listBox.ValueMember = DisplayedProperty;
      
      this.SetSelectedComputer();
      this.SetSelectedComputers();
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
