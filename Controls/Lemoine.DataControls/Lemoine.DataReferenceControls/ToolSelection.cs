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
  public partial class ToolSelection : UserControl
  {
    #region Members
    string m_displayedProperty = "Display";
    IList<ITool> m_tools = null;
    ITool m_tool = null;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (ToolSelection).FullName);

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
    /// Is a null Tool a valid value ?
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
    /// Selected Tools
    /// 
    /// The returned list is not null, but may be empty or contain only one null element
    /// </summary>
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public IList<ITool> SelectedTools {
      get
      {
        IList<ITool> list = new List<ITool> ();
        if (!nullCheckBox.Checked) {
          foreach (object item in listBox.SelectedItems) {
            ITool Tool = item as ITool;
            list.Add (Tool);
          }
        }
        return list;
      }
      set {
        if(value != null && value.Count >= 1){
          this.m_tools = value;
        }
      }
    }

    /// <summary>
    /// Selected Tool
    /// Return the first selected (if multiselection) or null
    /// </summary>
    public ITool SelectedTool {
      get {
        IList<ITool> tools = this.SelectedTools;
        if (tools.Count >= 1 && !this.nullCheckBox.Checked) {
          return tools[0] as ITool;
        }
        else {
          return null;
        }
      }
      set {
        this.m_tool = value;
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
    public ToolSelection()
    {
      //
      // The InitializeComponent() call is required for Windows Forms designer support.
      //
      InitializeComponent();
      
      nullCheckBox.Text = PulseCatalog.GetString ("ToolNull");
      
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
    /// Set Selected Tool in listbox
    /// </summary>
    private void SetSelectedTool(){
      if(this.m_tool != null){
        int index = this.listBox.Items.IndexOf(this.m_tool);
        if(index >= 0){
          this.listBox.SetSelected(index, true);
        }
      }
    }

    /// <summary>
    /// Set Selected Tools in listbox
    /// </summary>
    private void SetSelectedTools(){
      if(this.m_tools != null){
        int index = -1;
        foreach(ITool Tool in this.m_tools){
          index = this.listBox.Items.IndexOf(Tool);
          if(index >= 0){
            this.listBox.SetSelected(index,true);
          }
        }
      }
    }
    #endregion // Methods
    
    void ToolSelectionLoad(object sender, EventArgs e)
    {
      IList<ITool> tools;
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      
      if (daoFactory == null) {
        // to allow use in designer
        return;
      }
      using (IDAOSession session = daoFactory.OpenSession ())
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction ("ToolSelection.Load"))
      {
        tools =
          daoFactory.ToolDAO.FindAll ();
      }
      
      listBox.Items.Clear ();
      foreach (ITool tool in tools) {
        listBox.Items.Add (tool);
      }
      listBox.ValueMember = DisplayedProperty;
      
      this.SetSelectedTool();
      this.SetSelectedTools();
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
