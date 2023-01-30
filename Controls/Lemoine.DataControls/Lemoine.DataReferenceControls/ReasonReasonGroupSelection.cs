// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Windows.Forms;

using Lemoine.I18N;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;

namespace Lemoine.DataReferenceControls
{
  /// <summary>
  /// Control to select a reason or a reason group
  /// </summary>
  public partial class ReasonReasonGroupSelection : UserControl
  {
    #region Members
    string m_reasonDisplayedProperty = "Display";
    string m_reasonGroupDisplayedProperty = "Display";
    IReason m_reason = null;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (ReasonReasonGroupSelection).FullName);

    #region Getters / Setters
    /// <summary>
    /// Is a null reason or reason group a valid value ?
    /// </summary>
    [Category("Configuration"), Browsable(true), DefaultValue(true), Description("Is a null MachineObservation valid ?")]
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
    /// Property to use to display a reason
    /// 
    /// Default is Display
    /// </summary>
    [Category("Configuration"), Browsable(true), DefaultValue("Display"), Description("Property to use to display a reason")]
    public string ReasonDisplayedProperty {
      get { return  m_reasonDisplayedProperty; }
      set { m_reasonDisplayedProperty = value; }
    }
    
    /// <summary>
    /// Property to use to display a reason group
    /// 
    /// Default is Display
    /// </summary>
    [Category("Configuration"), Browsable(true), DefaultValue("Display"), Description("Property to use to display a reason group")]
    public string ReasonGroupDisplayedProperty {
      get { return  m_reasonGroupDisplayedProperty; }
      set { m_reasonGroupDisplayedProperty = value; }
    }
    
    /// <summary>
    /// Selected reason or reason group
    /// </summary>
    public object SelectedReasonReasonGroup {
      get
      {
        TreeNode selectedNode = treeView.SelectedNode;
        if (null == selectedNode) {
          return null;
        }
        else {
          return selectedNode.Tag;
        }
      }
      set {
        this.m_reason = (IReason)value;
      }
    }
    #endregion // Getters / Setters

    #region Events
    /// <summary>
    /// Selection changed
    /// </summary>
    [Category("Behavior"), Browsable(true), Description("Raised after a selection")]
    public event EventHandler AfterSelect;
    #endregion // Events
    
    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public ReasonReasonGroupSelection()
    {
      //
      // The InitializeComponent() call is required for Windows Forms designer support.
      //
      InitializeComponent();
      
      // Default properties
      nullCheckBox.Text = PulseCatalog.GetString ("ReasonReasonGroupNull");
      
      // Default events
    }
    #endregion // Constructors

    void ReasonReasonGroupSelectionLoad(object sender, EventArgs e)
    {
      
      if (ModelDAOHelper.DAOFactory == null) {
        // to allow use in designer
        return;
      }
      
      IList<IReasonGroup> reasonGroups;
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        reasonGroups =
          ModelDAOHelper.DAOFactory.ReasonGroupDAO
          .FindAllWithReasons ();
      }
      treeView.Nodes.Clear ();
      foreach (IReasonGroup reasonGroup in reasonGroups) {
        PropertyInfo reasonGroupDisplayedPropertyInfo =
          reasonGroup.GetType ().GetProperty (m_reasonGroupDisplayedProperty);
        TreeNode reasonGroupNode = new TreeNode ();
        reasonGroupNode.Text =
          (string) reasonGroupDisplayedPropertyInfo.GetValue (reasonGroup, null);
        reasonGroupNode.Tag = reasonGroup;
        treeView.Nodes.Add (reasonGroupNode);
        foreach (IReason reason in reasonGroup.Reasons) {
          PropertyInfo reasonDisplayedPropertyInfo =
            reason.GetType ().GetProperty (m_reasonDisplayedProperty);
          TreeNode reasonNode = new TreeNode ();
          reasonNode.Text =
            (string) reasonDisplayedPropertyInfo.GetValue (reason, null);
          reasonNode.Tag = reason;
          reasonNode.Name = (string) reasonDisplayedPropertyInfo.GetValue (reason, null);
          reasonGroupNode.Nodes.Add (reasonNode);
        }
      }
      
      this.SetSelectedNode();
    }
    
    /// <summary>
    /// Set Selected Reason
    /// </summary>
    void SetSelectedNode(){
      if(m_reason != null){
        //reasonNode.Name
        PropertyInfo reasonDisplayedPropertyInfo =
          m_reason.GetType ().GetProperty (m_reasonDisplayedProperty);
        TreeNode[] selectedTreeNode = this.treeView.Nodes.Find((string) reasonDisplayedPropertyInfo.GetValue (m_reason, null),true);
        if(selectedTreeNode[0] != null){
          this.treeView.SelectedNode = selectedTreeNode[0];
          this.treeView.HideSelection = false;
          if(!selectedTreeNode[0].Parent.IsExpanded){
            selectedTreeNode[0].Parent.Expand();
          }
        }
      }
    }
    
    void NullCheckBoxCheckedChanged(object sender, EventArgs e)
    {
      if (nullCheckBox.Checked) {
        treeView.Enabled = false;
      }
      else {
        treeView.Enabled = true;
      }
      
      this.AfterSelect (this, e);
    }
    
    void TreeViewAfterSelect(object sender, TreeViewEventArgs e)
    {
      this.AfterSelect (this, e);
    }
  }
}
