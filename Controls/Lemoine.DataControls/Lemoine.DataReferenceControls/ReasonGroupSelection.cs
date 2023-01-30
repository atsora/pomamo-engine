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
  /// Control to select a reason group
  /// </summary>
  public partial class ReasonGroupSelection : UserControl
  {
    static readonly ILog log = LogManager.GetLogger(typeof (ReasonGroupSelection).FullName);
    
    #region Members
    IList<IReasonGroup> m_reasonGroups = null;
    IReasonGroup m_reasonGroup = null;
    #endregion

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
    /// Is a null ReasonGroup a valid value ?
    /// </summary>
    [Category("Configuration"), Browsable(true), DefaultValue(false), Description("Is a null reason group valid ?")]
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
      get { return  listBox.DisplayMember; }
      set { listBox.DisplayMember = value; }
    }

    /// <summary>
    /// Selected ReasonGroups
    /// 
    /// The returned list is not null, but may be empty or contain only one null element
    /// </summary>
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public IList<IReasonGroup> SelectedReasonGroups {
      get
      {
        IList<IReasonGroup> list = new List<IReasonGroup> ();
        if (!nullCheckBox.Checked) {
          foreach (object item in listBox.SelectedItems) {
            IReasonGroup ReasonGroup = item as IReasonGroup;
            list.Add (ReasonGroup);
          }
        }
        return list;
      }
      set {
        if(value != null && value.Count >= 1){
          this.m_reasonGroups = value;
        }
      }
    }

    /// <summary>
    /// Selected ReasonGroup
    /// Return the first selected (if multiselection) or null
    /// </summary>
    public IReasonGroup SelectedReasonGroup {
      get {
        IList<IReasonGroup> reasonGroups = this.SelectedReasonGroups;
        if (reasonGroups.Count >= 1 && !this.nullCheckBox.Checked) {
          return reasonGroups[0] as IReasonGroup;
        }
        else {
          return null;
        }
      }
      set {
        this.m_reasonGroup = value;
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
    public ReasonGroupSelection()
    {
      //
      // The InitializeComponent() call is required for Windows Forms designer support.
      //
      InitializeComponent();
      
      nullCheckBox.Text = PulseCatalog.GetString ("ReasonGroupNull");

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
    /// Set Selected ReasonGroup in listbox
    /// </summary>
    private void SetSelectedReasonGroup(){
      if(this.m_reasonGroup != null){
        int index = this.listBox.Items.IndexOf(this.m_reasonGroup);
        if(index >= 0){
          this.listBox.SetSelected(index, true);
        }
      }
    }

    /// <summary>
    /// Set Selected ReasonGroups in listbox
    /// </summary>
    private void SetSelectedReasonGroups(){
      if(this.m_reasonGroups != null){
        int index = -1;
        foreach(IReasonGroup ReasonGroup in this.m_reasonGroups){
          index = this.listBox.Items.IndexOf(ReasonGroup);
          if(index >= 0){
            this.listBox.SetSelected(index,true);
          }
        }
      }
    }
    #endregion // Methods
    
    void ReasonGroupSelectionLoad(object sender, EventArgs e)
    {
      log.DebugFormat ("ReasonGroupSelectionLoad /B");
      
      IList<IReasonGroup> reasonGroups;

      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      if (null == daoFactory) {
        log.ErrorFormat("ReasonGroupConfigLoad: " +
                        "no DAO factory is defined");
        return;
      }
      
      using (IDAOSession daoSession = daoFactory.OpenSession())
      {
        reasonGroups = daoFactory.ReasonGroupDAO.FindAllWithReasons ()
          .OrderBy (x => x)
          .ToList ();
      }
      
      listBox.Items.Clear ();
      foreach (IReasonGroup reasonGroup in reasonGroups) {
        listBox.Items.Add (reasonGroup);
      }
      listBox.DisplayMember = DisplayedProperty;
      
      this.SetSelectedReasonGroup();
      this.SetSelectedReasonGroups();
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
