// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

using Lemoine.I18N;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Collections;

using Lemoine.Core.Log;

namespace Lemoine.JobControls
{
  /// <summary>
  /// Panel with fields to manage properties of a Part.
  /// It enable a user to read, modify properties of a Part
  /// or to create a Part with basic information
  /// </summary>
  public partial class PartControl : UserControl, ITreeViewObserver
  {
    /// <summary>
    /// Accepted types for display mode
    /// </summary>
    public enum DisplayMode
    {
      /// <summary>
      /// Display UserControl to view or/and modify WorkOrder
      /// </summary>
      VIEW = 1,
      /// <summary>
      /// Display UserControl to fill information and create new WorkOrder
      /// </summary>
      CREATE = 2
    };

    #region Members
    IPart m_part;
    IComponentType[] m_componentTypeArray;
    DisplayMode m_displayMode;
    TreeNode m_node;
    ITreeViewObservable m_observable;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger (typeof (PartControl).FullName);

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public PartControl (DisplayMode displayMode)
    {
      InitializeComponent ();

      nameLbl.Text = PulseCatalog.GetString ("Name");
      codeLbl.Text = PulseCatalog.GetString ("Code");
      documentLinkLbl.Text = PulseCatalog.GetString ("DocumentLink");
      estimatedHoursLbl.Text = PulseCatalog.GetString ("EstimatedHours");
      typeLbl.Text = PulseCatalog.GetString ("Type");
      nameTextBox.Clear ();
      codeTextBox.Clear ();
      documentLinkTextBox.Clear ();
      estimatedHoursTextBox.Clear ();

      this.m_part = null;
      this.m_displayMode = displayMode;
      switch (displayMode) {
        case DisplayMode.VIEW:
          saveBtn.Text = PulseCatalog.GetString ("Save");
          saveBtn.Enabled = false;
          resetBtn.Text = PulseCatalog.GetString ("Reset");
          baseLayout.RowStyles[6].Height = 32;
          baseLayout.RowStyles[7].Height = 0;
          break;
        case DisplayMode.CREATE:
          createBtn.Text = PulseCatalog.GetString ("Create");
          cancelBtn.Text = PulseCatalog.GetString ("Cancel");
          baseLayout.RowStyles[6].Height = 0;
          baseLayout.RowStyles[7].Height = 32;
          break;
      }

      typeComboBox.Items.Clear ();
      using (IDAOSession daoSession = ModelDAOHelper.DAOFactory.OpenSession ()) {
        IList<IComponentType> partTypes = ModelDAOHelper.DAOFactory.ComponentTypeDAO.FindAllOrderByName ();
        m_componentTypeArray = new IComponentType[partTypes.Count];
        int i = 0;
        foreach (IComponentType partType in partTypes) {
          m_componentTypeArray[i++] = partType;
          typeComboBox.Items.Add ((partType.Display == null) ? "" : partType.Display);
        }
        if (partTypes.Count > 0) {
          typeComboBox.SelectedIndex = 0;
        }
      }
    }


    /// <summary>
    ///   Default constructor without argument
    /// </summary>
    public PartControl () : this (DisplayMode.VIEW)
    {
    }
    #endregion // Constructors

    #region inherited Methods
    /// <summary>
    /// Update state of this observer. In this case, PartControl will hide or not
    /// according type of selected node  in ITreeViewObservable
    /// </summary>
    /// <param name="observable"></param>
    public void UpdateInfo (ITreeViewObservable observable)
    {
      this.m_observable = observable;
      TreeNode selectedNode = observable.GetSelectedNode ();
      if (selectedNode != null) {
        if (selectedNode.Tag is Tuple<bool, IPart>) {
          m_node = selectedNode;
          using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
            observable.ReloadTreeNodes (m_node);
            IPart part = ((Tuple<bool, IPart>)m_node.Tag).Item2;
            LoadData (part);
          }
        }
      }
    }
    #endregion

    #region Button Click Methods
    /// <summary>
    /// Save all changes
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    void SaveBtnClick (object sender, EventArgs e)
    {
      if (!ValidateData ()) {
        return;
      }

      m_part.Name = nameTextBox.Text;
      m_part.Code = codeTextBox.Text;
      m_part.DocumentLink = documentLinkTextBox.Text;
      m_part.Type = m_componentTypeArray[typeComboBox.SelectedIndex];
      m_part.EstimatedHours = (estimatedHoursTextBox.Text.Trim ().Length == 0) ?
        (double?)null : double.Parse (estimatedHoursTextBox.Text.Trim ());

      // Archive state
      if (!checkBoxArchive.Checked) {
        m_part.ArchiveDateTime = null;
      }
      else if (!m_part.ArchiveDateTime.HasValue) {
        m_part.ArchiveDateTime = DateTime.Now;
        checkBoxArchive.Text = "(" + m_part.ArchiveDateTime.Value.ToString () + ")";
      }

      using (IDAOSession daoSession = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (IDAOTransaction transaction = daoSession.BeginTransaction ()) {
          ModelDAOHelper.DAOFactory.PartDAO.MakePersistent (m_part);
          transaction.Commit ();
        }
      }

      m_observable.ReloadTreeNodes (m_node);
      m_node.TreeView.SelectedNode = m_node;
      m_node.TreeView.Focus ();
      m_observable.NotifyObservers ();
      saveBtn.Enabled = false;
    }

    /// <summary>
    /// Discard change and put current values in fields
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    void ResetBtnClick (object sender, EventArgs e)
    {
      saveBtn.Enabled = false;
      if (m_part == null) {
        return;
      }

      nameTextBox.Text = m_part.Name;
      codeTextBox.Text = m_part.Code;
      documentLinkTextBox.Text = m_part.DocumentLink;
      estimatedHoursTextBox.Text = (m_part.EstimatedHours == null) ? "" : m_part.EstimatedHours.Value.ToString ();

      // Archive state
      if (m_part.ArchiveDateTime.HasValue) {
        checkBoxArchive.Text = "(" + m_part.ArchiveDateTime.Value.ToString () + ")";
        checkBoxArchive.Checked = true;
      }
      else {
        checkBoxArchive.Text = "";
        checkBoxArchive.Checked = false;
      }

      IComponentType componentType;
      for (int i = 0; i < m_componentTypeArray.Length; i++) {
        componentType = m_componentTypeArray[i];
        if (m_part.Type.Id == componentType.Id) {
          typeComboBox.SelectedIndex = i;
          break;
        }
      }
    }

    /// <summary>
    /// Create a Part
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    void CreateBtnClick (object sender, EventArgs e)
    {
      // If values entered is not correct, exit
      if (!ValidateData ()) {
        return;
      }

      IPart part = ModelDAOHelper.ModelFactory.CreatePart (m_componentTypeArray[typeComboBox.SelectedIndex]);
      part.Name = nameTextBox.Text;
      part.Code = codeTextBox.Text;
      part.DocumentLink = documentLinkTextBox.Text;
      part.EstimatedHours = (estimatedHoursTextBox.Text.Trim ().Length == 0) ?
        (double?)null : double.Parse (estimatedHoursTextBox.Text.Trim ());

      if (checkBoxArchive.Checked) {
        part.ArchiveDateTime = DateTime.Now;
        checkBoxArchive.Text = part.ArchiveDateTime.Value.ToString ();
      }

      if (this.FindForm () is CreateItemForm) {
        // created project must be attached to current selected node
        // if CreateFItemForm.Goal==BIND
        CreateItemForm createItemForm = this.FindForm () as CreateItemForm;
        IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
        if (createItemForm.Goal == CreateItemForm.GoalType.BIND) {
          TreeNode parentNode = createItemForm.OperationTreeView.TreeView.SelectedNode;
          using (IDAOSession daoSession = daoFactory.OpenSession ()) {
            createItemForm.OperationTreeView.ReloadTreeNodes (parentNode);
            IWorkOrder workOrder = ((Tuple<bool, IWorkOrder>)parentNode.Tag).Item2;
            ModelDAOHelper.DAOFactory.WorkOrderDAO.Lock (workOrder);
            using (IDAOTransaction transaction = daoSession.BeginTransaction ()) {
              part.AddWorkOrder (workOrder);
              daoFactory.PartDAO.MakePersistent (part);
              transaction.Commit ();
            }
            this.m_part = part;
          }
          createItemForm.OperationTreeView.BuildTreeNodes (parentNode);
          foreach (TreeNode childNode in parentNode.Nodes) {
            if (childNode.Name == part.ComponentId.ToString ()) {
              parentNode.TreeView.SelectedNode = childNode;
              break;
            }
            parentNode.TreeView.Focus ();
            createItemForm.OperationTreeView.NotifyObservers ();
          }
        }
        else if (createItemForm.Goal == CreateItemForm.GoalType.NEW) {
          using (IDAOSession daoSession = daoFactory.OpenSession ()) {
            using (IDAOTransaction transaction = daoSession.BeginTransaction ()) {
              daoFactory.PartDAO.MakePersistent (part);
              transaction.Commit ();
            }

            if (createItemForm.OperationTreeView.PartAtTheTop) {
              TreeNode treeNode = new TreeNode (part.Display, (int)TreeViewImageIndex.Part, (int)TreeViewImageIndex.Part);
              treeNode.Name = part.ComponentId.ToString ();
              treeNode.Tag = new Tuple<bool, IPart> (true, part);
              createItemForm.OperationTreeView.TreeView.Nodes.Add (treeNode);
              treeNode.TreeView.SelectedNode = treeNode;
              treeNode.TreeView.Focus ();
              createItemForm.OperationTreeView.NotifyObservers ();
            }
            else {
              OrphanedItemsTreeView orphanedItemsTreeView = createItemForm.OperationTreeView.OrphanedItemsTreeView;
              if (orphanedItemsTreeView != null) {
                TreeNode rootNode = orphanedItemsTreeView.TreeView.Nodes.Find ("Part", false)[0];
                TreeNode childNode = new TreeNode (part.Display, (int)TreeViewImageIndex.Part, (int)TreeViewImageIndex.Part);
                childNode.Name = part.ComponentId.ToString ();
                childNode.Tag = new Tuple<bool, IPart> (false, part);
                rootNode.Nodes.Add (childNode);
                rootNode.TreeView.SelectedNode = childNode;
                rootNode.TreeView.Focus ();
                orphanedItemsTreeView.NotifyObservers ();
              }
            }
          }
        }
      }
      this.FindForm ().Close ();
    }

    /// <summary>
    /// Close form
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    void CancelBtnClick (object sender, EventArgs e)
    {
      this.FindForm ().Close ();
    }

    void DocumentLinkBtnClick (object sender, EventArgs e)
    {
      if (openFileDialog.ShowDialog () == DialogResult.OK) {
        this.documentLinkTextBox.Text = openFileDialog.FileName;
      }
    }

    void DocumentLinkTextBoxKeyDown (object sender, KeyEventArgs e)
    {
      if ((e.KeyCode != Keys.Left) && (e.KeyCode != Keys.Right)) {
        e.Handled = true;
        e.SuppressKeyPress = true;
      }
    }
    #endregion

    #region Methods
    /// <summary>
    /// Use informations in Part to fill text in box
    /// </summary>
    public void LoadData (IPart part)
    {
      if (part == null) {
        return;
      }

      nameTextBox.Text = part.Name;
      codeTextBox.Text = part.Code;
      documentLinkTextBox.Text = part.DocumentLink;
      estimatedHoursTextBox.Text = (part.EstimatedHours == null) ? "" : part.EstimatedHours.Value.ToString ();

      // Archive state
      if (part.ArchiveDateTime.HasValue) {
        checkBoxArchive.Text = "(" + part.ArchiveDateTime.Value.ToString () + ")";
        checkBoxArchive.Checked = true;
      }
      else {
        checkBoxArchive.Text = "";
        checkBoxArchive.Checked = false;
      }

      for (int i = 0; i < m_componentTypeArray.Length; i++) {
        // Get index of current value
        if (part.Type.Id == m_componentTypeArray[i].Id) {
          typeComboBox.SelectedIndex = i;
        }
      }
      this.m_part = part;
      resetBtn.Enabled = true;
      saveBtn.Enabled = false;
      // Fill list of WorkOrder associated with this Part
      int row = 0;
      tableLayoutPanel.RowStyles.Clear ();
      tableLayoutPanel.ColumnStyles.Clear ();
      tableLayoutPanel.Controls.Clear ();
      tableLayoutPanel.RowCount = 1;
      tableLayoutPanel.AutoSize = true;
      Label titleLbl = new Label ();
      titleLbl.Text = PulseCatalog.GetString ("ListOfAssociatedWorkOrder");
      titleLbl.AutoSize = true;
      titleLbl.Padding = new Padding (titleLbl.Padding.Left, titleLbl.Padding.Top, titleLbl.Padding.Right, 5);
      titleLbl.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      titleLbl.Font = new Font (titleLbl.Font, titleLbl.Font.Style | FontStyle.Bold | FontStyle.Underline);
      tableLayoutPanel.Controls.Add (titleLbl, 0, row);
      tableLayoutPanel.ColumnStyles.Add (new ColumnStyle (SizeType.AutoSize));
      tableLayoutPanel.RowStyles.Add (new RowStyle (SizeType.AutoSize));
      row++;
      using (IDAOSession daoSession = ModelDAOHelper.DAOFactory.OpenSession ()) {
        ModelDAOHelper.DAOFactory.PartDAO.Lock (part);
        foreach (IWorkOrder workOrder in part.WorkOrders) {
          LinkLabel linkLabel = new LinkLabel ();
          if (workOrder.Display.Length == 0) {
            linkLabel.Text = "___";
          }
          else {
            linkLabel.Text = workOrder.Display;
          }
          linkLabel.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
          linkLabel.Height = 20;
          linkLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
          linkLabel.AutoSize = true;
          linkLabel.Tag = workOrder;
          linkLabel.LinkClicked += linkLabelClicked;
          tableLayoutPanel.Controls.Add (linkLabel, 0, row);
          tableLayoutPanel.RowStyles.Add (new RowStyle (SizeType.AutoSize));
          row++;
        }
      }
    }

    private void linkLabelClicked (object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
    {
      e.Link.Visited = true;
      IWorkOrder workOrder = (IWorkOrder)(((LinkLabel)sender).Tag);
      if (m_node.Level > 0) {
        m_observable.GiveFocusToAllNodeInstances (typeof (IWorkOrder), workOrder);
      }
      else {
        Form displayWorkOrderForm = new Form ();
        displayWorkOrderForm.Text = PulseCatalog.GetString ("WorkOrderCreation");
        WorkOrderControl workOrderControl = new WorkOrderControl (WorkOrderControl.DisplayMode.VIEW);
        workOrderControl.Location = new Point (0, 0);
        workOrderControl.LoadData (workOrder);
        displayWorkOrderForm.Controls.Add (workOrderControl);
        displayWorkOrderForm.Size =
          new Size ((int)(workOrderControl.Size.Width * 1.1),
                   (int)(workOrderControl.Size.Height * 1.1) + 50);
        displayWorkOrderForm.ShowDialog (this);
        using (IDAOSession daoSession = ModelDAOHelper.DAOFactory.OpenSession ()) {
          IWorkOrder reloadWorkOrder = ModelDAOHelper.DAOFactory.WorkOrderDAO.FindById (((Lemoine.Collections.IDataWithId)workOrder).Id);
          ((LinkLabel)sender).Tag = reloadWorkOrder;
        }
      }
    }

    /// <summary>
    /// Test if a change on field occurs and then enable or disable save button
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    void NameTextBoxTextChanged (object sender, EventArgs e)
    {
      if (m_part != null && m_displayMode == DisplayMode.VIEW) {
        saveBtn.Enabled = HasBeenChanged ();
      }
    }

    /// <summary>
    /// Get whether a field has been changed
    /// </summary>
    /// <returns>True if a fields has been changed, false otherwise</returns>
    bool HasBeenChanged ()
    {
      if (!nameTextBox.Text.Equals (m_part.Name ?? "") ||
        !codeTextBox.Text.Equals (m_part.Code ?? "") ||
        !documentLinkTextBox.Text.Equals (m_part.DocumentLink ?? "") ||
        m_componentTypeArray[typeComboBox.SelectedIndex].Id != m_part.Type.Id ||
        checkBoxArchive.Checked != m_part.ArchiveDateTime.HasValue) {
        return true;
      }

      String s = (m_part.EstimatedHours == null) ? "" : m_part.EstimatedHours.ToString ();
      if (!estimatedHoursTextBox.Text.Trim ().Equals (s)) {
        return true;
      }

      return false;
    }

    /// <summary>
    /// Tell whether input values are corrects. If not a messagebox
    /// is displayed to tell incorrects fields.
    /// </summary>
    /// <returns>true if all fields have corrects input, false otherwise.</returns>
    bool ValidateData ()
    {
      if ((nameTextBox.Text.Length == 0) && (codeTextBox.Text.Length == 0)) {
        MessageBox.Show (PulseCatalog.GetString ("FieldsNameAndCodeCanNotHaveBothNullValue"), "", MessageBoxButtons.OK, MessageBoxIcon.Error);
        return false;
      }

      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      switch (m_displayMode) {
        case DisplayMode.VIEW:
          using (IDAOSession daoSession = daoFactory.OpenSession ()) {
            if (daoFactory.ProjectDAO.IfExistsOtherWithSameName (nameTextBox.Text, m_part.ProjectId)) {
              MessageBox.Show (PulseCatalog.GetString ("ThereIsAnotherPartWithSameName") + "\n" + PulseCatalog.GetString ("PleaseChangeIt"), "", MessageBoxButtons.OK, MessageBoxIcon.Error);
              return false;
            }

            if (daoFactory.ProjectDAO.IfExistsOtherWithSameCode (codeTextBox.Text, m_part.ProjectId)) {
              MessageBox.Show (PulseCatalog.GetString ("ThereIsAnotherPartWithSameCode") + "\n" + PulseCatalog.GetString ("PleaseChangeIt"), "", MessageBoxButtons.OK, MessageBoxIcon.Error);
              return false;
            }

            if (daoFactory.ComponentDAO.IfExistsOtherWithSameName (nameTextBox.Text, m_part.ComponentId, m_part.ProjectId)) {
              MessageBox.Show (PulseCatalog.GetString ("ThereIsAnotherPartWithSameName") + "\n" + PulseCatalog.GetString ("PleaseChangeIt"), "", MessageBoxButtons.OK, MessageBoxIcon.Error);
              return false;
            }

            if (daoFactory.ComponentDAO.IfExistsOtherWithSameCode (codeTextBox.Text, m_part.ComponentId, m_part.ProjectId)) {
              MessageBox.Show (PulseCatalog.GetString ("ThereIsAnotherPartWithSameCode") + "\n" + PulseCatalog.GetString ("PleaseChangeIt"), "", MessageBoxButtons.OK, MessageBoxIcon.Error);
              return false;
            }
          }
          break;
        case DisplayMode.CREATE:
          using (IDAOSession daoSession = ModelDAOHelper.DAOFactory.OpenSession ()) {
            if (daoFactory.ProjectDAO.IfExistsWithSameName (nameTextBox.Text)) {
              MessageBox.Show (PulseCatalog.GetString ("ThereIsAnotherPartWithSameName") + "\n" + PulseCatalog.GetString ("PleaseChangeIt"), "", MessageBoxButtons.OK, MessageBoxIcon.Error);
              return false;
            }

            if (daoFactory.ProjectDAO.IfExistsWithSameCode (codeTextBox.Text)) {
              MessageBox.Show (PulseCatalog.GetString ("ThereIsAnotherPartWithSameCode") + "\n" + PulseCatalog.GetString ("PleaseChangeIt"), "", MessageBoxButtons.OK, MessageBoxIcon.Error);
              return false;
            }
          }
          break;
      }
      String msg = PulseCatalog.GetString ("FollowingFieldsHaveIncorrectValues");
      bool valid = true;
      if (typeComboBox.SelectedIndex == -1) {
        msg = msg + "\n\t" + typeLbl.Text;
        valid = false;
      }
      try {
        if (estimatedHoursTextBox.Text.Trim ().Length != 0) {
          double.Parse (estimatedHoursTextBox.Text.Trim ());
        }
      }
      catch (Exception) {
        msg = msg + "\n\t" + estimatedHoursLbl.Text;
        valid = false;
      }
      if (!valid) {
        MessageBox.Show (msg, "", MessageBoxButtons.OK, MessageBoxIcon.Error);
      }
      return valid;
    }
    #endregion // Methods
  }
}
