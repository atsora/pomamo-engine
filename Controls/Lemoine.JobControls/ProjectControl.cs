// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Drawing;
using System.Windows.Forms;

using Lemoine.I18N;
using Lemoine.Model;
using Lemoine.ModelDAO;

using Lemoine.Core.Log;

namespace Lemoine.JobControls
{
  /// <summary>
  /// Panel with fields to manage properties of a Project.
  /// It enable a user to read, modify properties of a Project
  /// or to create a Project with basic information
  /// </summary>
  public partial class ProjectControl : UserControl, ITreeViewObserver
  {
    /// <summary>
    /// Accepted types for display mode
    /// </summary>
    public enum DisplayMode
    {
      /// <summary>
      /// Display UserControl to view or/and modify Project
      /// </summary>
      VIEW = 1,
      /// <summary>
      /// Display UserControl to fill information and create a new Project
      /// </summary>
      CREATE = 2
    };

    #region Members
    IProject m_project;
    DisplayMode m_displayMode;
    TreeNode m_node;
    ITreeViewObservable m_observable;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger (typeof (ProjectControl).FullName);

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public ProjectControl (DisplayMode displayMode)
    {
      InitializeComponent ();

      nameLbl.Text = PulseCatalog.GetString ("Name");
      codeLbl.Text = PulseCatalog.GetString ("Code");
      documentLinkLbl.Text = PulseCatalog.GetString ("DocumentLink");
      nameTextBox.Clear ();
      codeTextBox.Clear ();
      documentLinkTextBox.Clear ();

      this.m_project = null;
      this.m_displayMode = displayMode;
      switch (displayMode) {
        case DisplayMode.VIEW:
          saveBtn.Text = PulseCatalog.GetString ("Save");
          saveBtn.Enabled = false;
          resetBtn.Text = PulseCatalog.GetString ("Reset");
          baseLayout.RowStyles[3].Height = 32;
          baseLayout.RowStyles[4].Height = 0;
          break;
        case DisplayMode.CREATE:
          createBtn.Text = PulseCatalog.GetString ("Create");
          cancelBtn.Text = PulseCatalog.GetString ("Cancel");
          baseLayout.RowStyles[3].Height = 0;
          baseLayout.RowStyles[4].Height = 32;
          break;
      }
    }

    /// <summary>
    /// Default constructor without argument
    /// </summary>
    public ProjectControl () : this (DisplayMode.VIEW)
    {
    }
    #endregion // Constructors

    #region inherited Methods
    /// <summary>
    /// Update state of this observer. In this case, ProjectControl
    /// will hide or not according type of selected node  in ITreeViewObservable
    /// </summary>
    /// <param name="observable"></param>
    public void UpdateInfo (ITreeViewObservable observable)
    {
      this.m_observable = observable;
      TreeNode selectedNode = observable.GetSelectedNode ();
      if (selectedNode != null) {
        if (selectedNode.Tag is Tuple<bool, IProject>) {
          m_node = selectedNode;
          observable.ReloadTreeNodes (m_node);
          IProject project = ((Tuple<bool, IProject>)m_node.Tag).Item2;
          LoadData (project);
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
      // If values entered are not correct, exit
      if (!ValidateData ()) {
        return;
      }

      m_project.Name = nameTextBox.Text;
      m_project.Code = codeTextBox.Text;
      m_project.DocumentLink = documentLinkTextBox.Text;

      // Archive state
      if (!checkBoxArchive.Checked) {
        m_project.ArchiveDateTime = null;
      }
      else if (!m_project.ArchiveDateTime.HasValue) {
        m_project.ArchiveDateTime = DateTime.Now;
        checkBoxArchive.Text = "(" + m_project.ArchiveDateTime.Value.ToString () + ")";
      }

      using (IDAOSession daoSession = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (IDAOTransaction transaction = daoSession.BeginTransaction ()) {
          ModelDAOHelper.DAOFactory.ProjectDAO.MakePersistent (m_project);
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
    /// Close form
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    void CancelBtnClick (object sender, EventArgs e)
    {
      this.FindForm ().Close ();
    }

    /// <summary>
    /// Discard change and put current values in fields
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    void ResetBtnClick (object sender, EventArgs e)
    {
      saveBtn.Enabled = false;
      if (m_project == null) {
        return;
      }

      nameTextBox.Text = m_project.Name;
      codeTextBox.Text = m_project.Code;
      documentLinkTextBox.Text = m_project.DocumentLink;

      // Archive state
      if (m_project.ArchiveDateTime.HasValue) {
        checkBoxArchive.Text = "(" + m_project.ArchiveDateTime.Value.ToString () + ")";
        checkBoxArchive.Checked = true;
      }
      else {
        checkBoxArchive.Text = "";
        checkBoxArchive.Checked = false;
      }
    }

    /// <summary>
    ///   Create a Project
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    void CreateBtnClick (object sender, EventArgs e)
    {
      // If values entered are not correct, exit
      if (!ValidateData ()) {
        return;
      }

      IProject project = ModelDAOHelper.ModelFactory.CreateProjectFromName (nameTextBox.Text);
      project.Code = codeTextBox.Text;
      project.DocumentLink = documentLinkTextBox.Text;

      if (checkBoxArchive.Checked) {
        project.ArchiveDateTime = DateTime.Now;
        checkBoxArchive.Text = project.ArchiveDateTime.Value.ToString ();
      }

      if (this.FindForm () is CreateItemForm) {
        // created project must be attached to current selected node
        // if CreateItemForm.Goal==BIND
        CreateItemForm createItemForm = this.FindForm () as CreateItemForm;
        IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
        if (createItemForm.Goal == CreateItemForm.GoalType.BIND) {
          TreeNode parentNode = createItemForm.OperationTreeView.TreeView.SelectedNode;
          using (IDAOSession daoSession = daoFactory.OpenSession ()) {
            IWorkOrder workOrder = ((Tuple<bool, IWorkOrder>)parentNode.Tag).Item2;
            ModelDAOHelper.DAOFactory.WorkOrderDAO.Lock (workOrder);
            project.AddWorkOrder (workOrder);
            using (IDAOTransaction transaction = daoSession.BeginTransaction ()) {
              daoFactory.ProjectDAO.MakePersistent (project);
              daoFactory.WorkOrderDAO.MakePersistent (workOrder);
              this.m_project = project;
              transaction.Commit ();
            }

            createItemForm.OperationTreeView.BuildTreeNodes (parentNode);
            foreach (TreeNode childNode in parentNode.Nodes) {
              if (childNode.Name == ((Lemoine.Collections.IDataWithId<int>)project).Id.ToString ()) {
                parentNode.TreeView.SelectedNode = childNode;
                break;
              }
            }
            parentNode.TreeView.Focus ();
            createItemForm.OperationTreeView.NotifyObservers ();
          }
        }
        else if (createItemForm.Goal == CreateItemForm.GoalType.NEW) {
          using (IDAOSession daoSession = daoFactory.OpenSession ()) {
            using (IDAOTransaction transaction = daoSession.BeginTransaction ()) {
              daoFactory.ProjectDAO.MakePersistent (project);
              transaction.Commit ();
            }
            OrphanedItemsTreeView orphanedItemsTreeView = createItemForm.OperationTreeView.OrphanedItemsTreeView;
            if (orphanedItemsTreeView != null) {
              TreeNode rootNode = orphanedItemsTreeView.TreeView.Nodes.Find ("Project", false)[0];
              TreeNode childNode = new TreeNode (project.Display, (int)TreeViewImageIndex.Project, (int)TreeViewImageIndex.Project);
              childNode.Name = ((Lemoine.Collections.IDataWithId<int>)project).Id.ToString ();
              childNode.Tag = new Tuple<bool, IProject> (false, project);
              rootNode.Nodes.Add (childNode);
              rootNode.TreeView.SelectedNode = childNode;
              rootNode.TreeView.Focus ();
              orphanedItemsTreeView.NotifyObservers ();
            }
          }
        }
      }

      this.FindForm ().Close ();
    }

    void DocumentLinkTextBoxKeyDown (object sender, KeyEventArgs e)
    {
      if (e.KeyCode != Keys.Left && e.KeyCode != Keys.Right) {
        e.Handled = true;
        e.SuppressKeyPress = true;
      }
    }

    void DocumentLinkBtnClick (object sender, EventArgs e)
    {
      if (openFileDialog.ShowDialog () == DialogResult.OK) {
        this.documentLinkTextBox.Text = openFileDialog.FileName;
      }
    }
    #endregion

    #region Methods
    /// <summary>
    /// Use informations in Project to fill text in box
    /// </summary>
    public void LoadData (IProject project)
    {
      if (project == null) {
        return;
      }

      nameTextBox.Text = project.Name;
      codeTextBox.Text = project.Code;
      documentLinkTextBox.Text = project.DocumentLink;

      // Archive state
      if (project.ArchiveDateTime.HasValue) {
        checkBoxArchive.Text = "(" + project.ArchiveDateTime.Value.ToString () + ")";
        checkBoxArchive.Checked = true;
      }
      else {
        checkBoxArchive.Text = "";
        checkBoxArchive.Checked = false;
      }

      saveBtn.Enabled = false;
      resetBtn.Enabled = true;

      // Fill list of WorkOrder associated with this Project
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
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      using (IDAOSession daoSession = daoFactory.OpenSession ()) {
        ModelDAOHelper.DAOFactory.ProjectDAO.Lock (project);
        this.m_project = project;
        NHibernate.NHibernateUtil.Initialize (this.m_project.WorkOrders);
        foreach (IWorkOrder workOrder in this.m_project.WorkOrders) {
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
    }

    /// <summary>
    /// Test if a change on field occurs and then enable or disable save button
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    void NameTextBoxTextChanged (object sender, EventArgs e)
    {
      if (m_project != null && m_displayMode == DisplayMode.VIEW) {
        if (HasBeenChanged ()) {
          saveBtn.Enabled = true;
        }
        else {
          saveBtn.Enabled = false;
        }
      }
    }

    /// <summary>
    /// Get whether a field has been changed
    /// </summary>
    /// <returns>True if a fields has been changed, false otherwise</returns>
    bool HasBeenChanged ()
    {
      return !nameTextBox.Text.Equals (m_project.Name ?? "") ||
        !codeTextBox.Text.Equals (m_project.Code ?? "") ||
        !documentLinkTextBox.Text.Equals (m_project.DocumentLink ?? "") ||
        checkBoxArchive.Checked != m_project.ArchiveDateTime.HasValue;
    }

    /// <summary>
    /// Tell whether input values are corrects. If not a messagebox
    /// is displayed to tell incorrects fields.
    /// </summary>
    /// <returns>true if all fields have corrects input, false otherwise.</returns>
    private bool ValidateData ()
    {
      if ((nameTextBox.Text.Length == 0) && (codeTextBox.Text.Length == 0)) {
        MessageBox.Show (PulseCatalog.GetString ("FieldsNameAndCodeCanNotHaveBothNullValue"), "", MessageBoxButtons.OK, MessageBoxIcon.Error);
        return false;
      }

      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      switch (m_displayMode) {
        case DisplayMode.VIEW:
          using (IDAOSession daoSession = daoFactory.OpenSession ()) {
            if (daoFactory.ProjectDAO.IfExistsOtherWithSameName (nameTextBox.Text, ((Lemoine.Collections.IDataWithId)m_project).Id)) {
              MessageBox.Show (PulseCatalog.GetString ("ThereIsAnotherProjectWithSameName") + "\n" + PulseCatalog.GetString ("PleaseChangeIt"), "", MessageBoxButtons.OK, MessageBoxIcon.Error);
              return false;
            }

            if (daoFactory.ProjectDAO.IfExistsOtherWithSameCode (codeTextBox.Text, ((Lemoine.Collections.IDataWithId)m_project).Id)) {
              MessageBox.Show (PulseCatalog.GetString ("ThereIsAnotherProjectWithSameCode") + "\n" + PulseCatalog.GetString ("PleaseChangeIt"), "", MessageBoxButtons.OK, MessageBoxIcon.Error);
              return false;
            }
          }

          break;
        case DisplayMode.CREATE:
          using (IDAOSession daoSession = ModelDAOHelper.DAOFactory.OpenSession ()) {
            if (daoFactory.ProjectDAO.IfExistsWithSameName (nameTextBox.Text)) {
              MessageBox.Show (PulseCatalog.GetString ("ThereIsAnotherProjectWithSameName") + "\n" + PulseCatalog.GetString ("PleaseChangeIt"), "", MessageBoxButtons.OK, MessageBoxIcon.Error);
              return false;
            }
            if (daoFactory.ProjectDAO.IfExistsWithSameCode (codeTextBox.Text)) {
              MessageBox.Show (PulseCatalog.GetString ("ThereIsAnotherProjectWithSameCode") + "\n" + PulseCatalog.GetString ("PleaseChangeIt"), "", MessageBoxButtons.OK, MessageBoxIcon.Error);
              return false;
            }
          }
          break;
      }
      return true;
    }
    #endregion // Methods
  }
}
