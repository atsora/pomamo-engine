// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Windows.Forms;
using Lemoine.BaseControls;
using Lemoine.JobControls;
using Lemoine.I18N;
using Lemoine.Model;

using Lemoine.Core.Log;
using Lemoine.Business.Config;

namespace Lemoine.JobControls
{
  /// <summary>
  /// Operation selection dialog
  /// </summary>
  public partial class SelectOperationOrPathDialog : OKCancelDialog, ITreeViewObserver
  {
    #region Members
    OperationTreeView m_operationTreeView;
    InformationControl m_informationControl;
    IOperation m_selectedOperation = null;
    IPath m_selectedPath = null;
    bool m_onlyPathOK = false;
    bool m_onlyOperationOK = false;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger (typeof (SelectOperationOrPathDialog).FullName);

    #region Getters / Setters

    /// <summary>
    ///   OperationTreeView
    /// </summary>
    public OperationTreeView OperationTreeView
    {
      get { return m_operationTreeView; }
      private set { m_operationTreeView = value; }
    }

    /// <summary>
    ///   InformationControl
    /// </summary>
    public InformationControl InformationControl
    {
      get { return m_informationControl; }
      private set { m_informationControl = value; }
    }

    /// <summary>
    /// Selected operation
    /// </summary>
    public IOperation Operation
    {
      get { return m_selectedOperation; }
      private set { m_selectedOperation = value; }
    }

    /// <summary>
    /// Selected path
    /// </summary>
    public IPath Path
    {
      get { return m_selectedPath; }
      private set { m_selectedPath = value; }
    }

    /// <summary>
    /// Should only the selection of a path be ok ?
    /// </summary>
    public bool OnlyPathOK
    {
      get { return m_onlyPathOK; }
      set { m_onlyPathOK = value; }
    }

    /// <summary>
    /// Should only the selection of an operation be ok ?
    /// </summary>
    public bool OnlyOperationOK
    {
      get { return m_onlyOperationOK; }
      set { m_onlyOperationOK = value; }
    }

    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Use of a tree view for operation selection
    /// </summary>
    public SelectOperationOrPathDialog ()
    {
      //
      // The InitializeComponent() call is required for Windows Forms designer support.
      //
      InitializeComponent ();
      this.SuspendLayout ();
      this.Text = Lemoine.Info.ConfigSet.Get<bool> (ConfigKeys.GetDataStructureConfigKey (DataStructureConfigKey.SinglePath)) ?
        PulseCatalog.GetString ("OperationSelection") :
        PulseCatalog.GetString ("PathSelection"); // TODO i18n
      this.OperationTreeView = new OperationTreeView ();
      this.OperationTreeView.Location = new System.Drawing.Point (10, 10);
      this.OperationTreeView.Size = new System.Drawing.Size (350, 550);
      this.OperationTreeView.Anchor =
        AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
      this.OperationTreeView.TabIndex = 0;

      this.InformationControl = new InformationControl ();
      this.InformationControl.BackColor = System.Drawing.SystemColors.Control;
      this.InformationControl.Location = new System.Drawing.Point (390, 10);
      this.InformationControl.Size = new System.Drawing.Size (440, 550);
      this.InformationControl.Anchor =
        AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Bottom;
      this.InformationControl.TabIndex = 1;

      this.AutoScaleDimensions = new System.Drawing.SizeF (6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;

      this.Controls.Add (this.OperationTreeView);
      this.Controls.Add (this.InformationControl);

      //
      //this.InformationControl.AutoSizeMode = AutoSizeMode.GrowAndShrink;

      this.OperationTreeView.OrphanedItemsTreeView.AddObserver (this.InformationControl);
      foreach (ITreeViewObserver observer in this.InformationControl.AllObservers) {
        this.OperationTreeView.OrphanedItemsTreeView.AddObserver (observer);
      }
      this.OperationTreeView.OrphanedItemsTreeView.AddObserver (this);

      this.OperationTreeView.AddObserver (this.InformationControl);
      foreach (ITreeViewObserver observer in this.InformationControl.AllObservers) {
        this.OperationTreeView.AddObserver (observer);
      }
      this.OperationTreeView.AddObserver (this);

      this.okButton.Enabled = false;
      this.cancelButton.Enabled = true;

      this.InformationControl.UpdateInfo (this.OperationTreeView);
      this.ResumeLayout (false);
    }

    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Called on observable event on one of the tree views: update
    /// OK button status w.r.t. selected node and acceptable nodes
    /// </summary>
    /// <param name="observable"></param>
    public void UpdateInfo (ITreeViewObservable observable)
    {
      TreeNode selectedTreeNode = observable.GetSelectedNode ();

      if (selectedTreeNode != null) {
        Type selectedTreeNodeType = observable.GetItemType (selectedTreeNode);

        if ((selectedTreeNodeType != null)
            && (((selectedTreeNodeType.Equals (typeof (IPath))) && (!this.OnlyOperationOK))
                || (((selectedTreeNodeType.Equals (typeof (IOperation))) ||
                     (selectedTreeNodeType.Equals (typeof (ISimpleOperation)))) && (!this.OnlyPathOK)))) {
          this.okButton.Enabled = true;
        }
        else {
          this.okButton.Enabled = false;
        }
      }
      else {
        this.okButton.Enabled = false;
      }
    }

    /// <summary>
    /// Update selected operation/path if selected tree node has an operation/path tag
    /// </summary>
    void TrySetOperationOrPath (BaseOperationTreeView treeView, TreeNode treeNode)
    {
      if (treeNode != null) {
        Type treeNodeType = treeView.GetItemType (treeNode);
        if (treeNodeType.Equals (typeof (IOperation))) {
          m_selectedOperation = (IOperation)treeView.GetItem (treeNode);
        }
        else if (treeNodeType.Equals (typeof (ISimpleOperation))) {
          ISimpleOperation simpleOperation = (ISimpleOperation)treeView.GetItem (treeNode);
          m_selectedOperation = simpleOperation.Operation;
        }
        else if (treeNodeType.Equals (typeof (IPath))) {
          m_selectedPath = (IPath)treeView.GetItem (treeNode);
        }
      }
    }

    /// <summary>
    /// on OK button selection, update currently selected operation
    /// and return from form
    /// </summary>
    void OkButtonClick (object sender, EventArgs e)
    {
      IOperation old_selectedOperation = m_selectedOperation;
      m_selectedOperation = null;
      // first return selection in base treeview if any
      // otherwise check in orphaned items treeview
      TreeNode treeNode = m_operationTreeView.GetSelectedNode ();
      TrySetOperationOrPath (m_operationTreeView, treeNode);
      if (m_selectedOperation == null) {
        treeNode = m_operationTreeView.OrphanedItemsTreeView.GetSelectedNode ();
        TrySetOperationOrPath (m_operationTreeView, treeNode);
      }
      if (m_selectedOperation == null) {
        m_selectedOperation = old_selectedOperation;
      }

      this.DialogResult = DialogResult.OK;
    }

    /// <summary>
    /// cancel selection: selected operation/path remains the same
    /// </summary>
    void CancelButtonClick (object sender, EventArgs e)
    {
      this.DialogResult = DialogResult.Cancel;
    }
    #endregion // Methods
  }
}
