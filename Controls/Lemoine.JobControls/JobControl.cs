// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Windows.Forms;

using Lemoine.I18N;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Collections;
using Lemoine.Core.Log;


namespace Lemoine.JobControls
{
  /// <summary>
  /// Panel with fields to manage properties of a Job.
  /// It enable a user to read, modify properties of a Job
  /// or to create a Job with basic information
  /// </summary>
  public partial class JobControl : UserControl, ITreeViewObserver
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
    IJob m_job;
    DisplayMode m_displayMode;
    TreeNode m_node;
    IWorkOrderStatus [] m_workOrderStatusArray;
    ITreeViewObservable m_observable;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (JobControl).FullName);

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public JobControl(DisplayMode displayMode)
    {
      InitializeComponent();
      
      nameLbl.Text = PulseCatalog.GetString("Name");
      codeLbl.Text = PulseCatalog.GetString("Code");
      documentLinkLbl.Text = PulseCatalog.GetString("DocumentLink");
      statusLbl.Text = PulseCatalog.GetString("Status");
      nameTextBox.Clear();
      codeTextBox.Clear();
      documentLinkTextBox.Clear();
      this.m_job = null;
      this.m_displayMode = displayMode;
      using (IDAOSession daoSession = ModelDAOHelper.DAOFactory.OpenSession ())
      {
        IList<IWorkOrderStatus> workOrderStatus = ModelDAOHelper.DAOFactory.WorkOrderStatusDAO.FindAllOrderByName();
        m_workOrderStatusArray = new IWorkOrderStatus[workOrderStatus.Count];
        int i = 0;
        int indexForWorkOrderNew = 0;
        statusComboBox.Items.Clear();
        foreach (IWorkOrderStatus aWorkOrderStatus in workOrderStatus) {
          m_workOrderStatusArray[i] = aWorkOrderStatus;
          statusComboBox.Items.Add((aWorkOrderStatus.Display == null)?"":aWorkOrderStatus.Display);
          if (PulseCatalog.GetString("New") != null) {
            if (PulseCatalog.GetString("New").Equals(aWorkOrderStatus.Display)) {
              indexForWorkOrderNew = i;
            }
          }
          i++;
        }
        if (workOrderStatus.Count > 0) {
          statusComboBox.SelectedIndex = indexForWorkOrderNew;
        }
      }
      switch (displayMode) {
        case DisplayMode.VIEW :
          saveBtn.Text = PulseCatalog.GetString("Save");
          saveBtn.Enabled = false;
          resetBtn.Text = PulseCatalog.GetString("Reset");
          resetBtn.Enabled = false;

          baseLayout.RowStyles[5].Height = 32;
          baseLayout.RowStyles[6].Height = 0;
          break;
        case DisplayMode.CREATE :
          cancelBtn.Text = PulseCatalog.GetString("Cancel");
          cancelBtn.Enabled = true;
          createBtn.Text = PulseCatalog.GetString("Create");
          createBtn.Enabled = true;
          baseLayout.RowStyles[5].Height = 0;
          baseLayout.RowStyles[6].Height = 32;
        break;
      }
    }

    /// <summary>
    ///   Default constructor without argument
    /// </summary>
    public JobControl() : this(DisplayMode.VIEW)
    {
    }
    #endregion // Constructors

    #region inherited Methods
    /// <summary>
    /// Update state of this observer. In this case, JobControl
    /// will hide or not according type of selected node  in ITreeViewObservable
    /// </summary>
    /// <param name="observable"></param>
    public void UpdateInfo (ITreeViewObservable observable)
    {
      this.m_observable = observable;
      TreeNode selectedNode = observable.GetSelectedNode();
      if (selectedNode != null) {
        if (selectedNode.Tag is Tuple<bool, IJob>) {
          m_node = selectedNode;
          observable.ReloadTreeNodes (m_node);
          LoadData (((Tuple<bool, IJob>)m_node.Tag).Item2);
        }
      }
    }
    #endregion
    
    
    #region Button Click Methods
    /// <summary>
    ///   Save all changes
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    void SaveBtnClick(object sender, EventArgs e)
    {
      if (!ValidateData()) {
        return;
      }

      m_job.Name = nameTextBox.Text;
      m_job.Code = codeTextBox.Text;
      m_job.DocumentLink = documentLinkTextBox.Text;
      m_job.Status = m_workOrderStatusArray[statusComboBox.SelectedIndex];

      // Archive state
      if (!checkBoxArchive.Checked) {
        m_job.ArchiveDateTime = null;
      } else if (!m_job.ArchiveDateTime.HasValue) {
        m_job.ArchiveDateTime = DateTime.Now;
        checkBoxArchive.Text = "(" + m_job.ArchiveDateTime.Value.ToString () + ")";
      }

      using (IDAOSession daoSession = ModelDAOHelper.DAOFactory.OpenSession ())
      {
        using (IDAOTransaction transaction = daoSession.BeginTransaction()) {
          ModelDAOHelper.DAOFactory.JobDAO.MakePersistent(m_job);
          transaction.Commit();
        }
      }

      m_node.Tag = new Tuple<bool, IJob> (((Tuple<bool, IJob>)m_node.Tag).Item1, m_job);
      m_node.Text = m_job.Display;
      m_node.TreeView.Sort(); // necessary here
      m_node.TreeView.SelectedNode = m_node;
      saveBtn.Enabled = false;
    }
    
    /// <summary>
    /// Discard change and put current values in fields
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    void ResetBtnClick(object sender, EventArgs e)
    {
      saveBtn.Enabled = false;
      if (m_job == null) {
        return;
      }

      nameTextBox.Text = m_job.Name;
      codeTextBox.Text = m_job.Code;
      documentLinkTextBox.Text = m_job.DocumentLink;
        
      // Archive state
      if (m_job.ArchiveDateTime.HasValue) {
        checkBoxArchive.Text = "(" + m_job.ArchiveDateTime.Value.ToString () + ")";
        checkBoxArchive.Checked = true;
      } else {
        checkBoxArchive.Text = "";
        checkBoxArchive.Checked = false;
      }

      using (IDAOSession daoSession = ModelDAOHelper.DAOFactory.OpenSession()) {
        IJob job = ModelDAOHelper.DAOFactory.JobDAO.FindById(m_job.ProjectId);
        IWorkOrderStatus status = job.Status;
        for (int i = 0; i < m_workOrderStatusArray.Length; i++) {
          if (status.Id == m_workOrderStatusArray[i].Id) {
            statusComboBox.SelectedIndex = i;
            break;
          }
        }
      }
    }
    
    /// <summary>
    /// Create a Job
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    void CreateBtnClick(object sender, EventArgs e)
    {
      // If values entered is not correct, exit
      if (!ValidateData()) {
        return;
      }

      IJob job = ModelDAOHelper.ModelFactory.CreateJobFromName (m_workOrderStatusArray[statusComboBox.SelectedIndex], nameTextBox.Text);
      job.Code = codeTextBox.Text;
      job.DocumentLink = documentLinkTextBox.Text;

      if (checkBoxArchive.Checked) {
        job.ArchiveDateTime = DateTime.Now;
        checkBoxArchive.Text = job.ArchiveDateTime.Value.ToString ();
      }

      using (IDAOSession daoSession = ModelDAOHelper.DAOFactory.OpenSession ())
      {
        using (IDAOTransaction transaction = daoSession.BeginTransaction ())
        {
          ModelDAOHelper.DAOFactory.JobDAO.MakePersistent(job);
          transaction.Commit();
        }
        if (this.FindForm() is CreateItemForm) {
          // created job must always be attached to operationTree
          // whatever value of CreateFItemForm.Goal
          CreateItemForm createItemForm = this.FindForm() as CreateItemForm;
          TreeNode treeNode = new TreeNode(job.Display,(int)TreeViewImageIndex.Job,(int)TreeViewImageIndex.Job);
          treeNode.Name = job.WorkOrderId.ToString();
          treeNode.Tag = new Tuple<bool, IJob> (false, job);
          createItemForm.OperationTreeView.TreeView.Nodes.Add(treeNode);
          createItemForm.OperationTreeView.TreeView.Focus();
          createItemForm.OperationTreeView.TreeView.SelectedNode = treeNode;
          createItemForm.OperationTreeView.NotifyObservers();
        }
      }
      this.FindForm().Close();
    }
    
    /// <summary>
    /// Close form
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    void CancelBtnClick(object sender, EventArgs e)
    {
      this.FindForm().Close();
    }

    void DocumentLinkTextBoxKeyDown(object sender, KeyEventArgs e)
    {
      if ((e.KeyCode != Keys.Left)&&(e.KeyCode != Keys.Right)) {
        e.Handled = true;
        e.SuppressKeyPress = true;
      }
    }
    
    void DocumentLinkBtnClick(object sender, EventArgs e)
    {
      if (openFileDialog.ShowDialog() == DialogResult.OK)
      {
        this.documentLinkTextBox.Text = openFileDialog.FileName;
      }
    }
    #endregion

    #region Methods
    /// <summary>
    /// Use informations in Job to fill text in box
    /// </summary>
    /// <param name="job">instance used to get data</param>
    public void LoadData(IJob job)
    {
      if (job == null) {
        return;
      }

      nameTextBox.Text = job.Name;
      codeTextBox.Text = job.Code;
      documentLinkTextBox.Text = job.DocumentLink;

      // Archive state
      if (job.ArchiveDateTime.HasValue) {
        checkBoxArchive.Text = "(" + job.ArchiveDateTime.Value.ToString () + ")";
        checkBoxArchive.Checked = true;
      } else {
        checkBoxArchive.Text = "";
        checkBoxArchive.Checked = false;
      }

      this.m_job = job;
      IWorkOrderStatus status;
      using (IDAOSession daoSession = ModelDAOHelper.DAOFactory.OpenSession ())
      {
        ModelDAOHelper.DAOFactory.JobDAO.Lock (job);
        status = job.Status;
      }
      for (int i = 0; i < m_workOrderStatusArray.Length; i++) {
        if (status.Id == m_workOrderStatusArray[i].Id) {
          statusComboBox.SelectedIndex = i;
          break;
        }
      }
      resetBtn.Enabled = true;
      saveBtn.Enabled = false;
    }

    
    /// <summary>
    /// Test if a change on field occurs and then enable or disable save button
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    void NameTextBoxTextChanged(object sender, EventArgs e)
    {
      if (m_job != null && m_displayMode == DisplayMode.VIEW) {
        saveBtn.Enabled = HasBeenChanged ();
      }
    }
    
    /// <summary>
    /// Get whether a field has been changed
    /// </summary>
    /// <returns>True if a fields has been changed, false otherwise</returns>
    bool HasBeenChanged()
    {
      if (!nameTextBox.Text.Equals(m_job.Name ?? "") ||
        !codeTextBox.Text.Equals (m_job.Code ?? "") ||
        !documentLinkTextBox.Text.Equals (m_job.DocumentLink ?? "") ||
        checkBoxArchive.Checked != m_job.ArchiveDateTime.HasValue) {
        return true;
      }

      IWorkOrderStatus status;
      using (IDAOSession daoSession = ModelDAOHelper.DAOFactory.OpenSession ())
      {
        ModelDAOHelper.DAOFactory.JobDAO.Lock (m_job);
        status = m_job.Status;
      }
      if (m_workOrderStatusArray[statusComboBox.SelectedIndex].Id != status.Id) {
        return true;
      }
      return false;
    }
    
    /// <summary>
    /// Tell whether input values are corrects. If not a messagebox
    /// is displayed to tell incorrects fields.
    /// </summary>
    /// <returns>true if all fields have corrects input, false otherwise.</returns>
    private bool ValidateData()
    {
      if ((nameTextBox.Text.Length == 0) && (codeTextBox.Text.Length == 0)) {
        MessageBox.Show (PulseCatalog.GetString("ThereIsAnotherJobWithSameName")+"\n"+PulseCatalog.GetString("PleaseChangeIt"), "",MessageBoxButtons.OK, MessageBoxIcon.Error);
        return false;
      }
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      switch (m_displayMode) {
        case DisplayMode.VIEW :
          using (IDAOSession daoSession = daoFactory.OpenSession ())
          {
            ModelDAOHelper.DAOFactory.JobDAO.Lock (m_job);
            if (daoFactory.ProjectDAO.IfExistsOtherWithSameName(nameTextBox.Text, m_job.ProjectId)) {
              MessageBox.Show (PulseCatalog.GetString("ThereIsAnotherJobWithSameName")+"\n"+PulseCatalog.GetString("PleaseChangeIt"), "",MessageBoxButtons.OK, MessageBoxIcon.Error);
              return false;
            }
            if (daoFactory.ProjectDAO.IfExistsOtherWithSameCode(codeTextBox.Text, m_job.ProjectId)) {
              MessageBox.Show (PulseCatalog.GetString("ThereIsAnotherJobWithSameCode")+"\n"+PulseCatalog.GetString("PleaseChangeIt"), "",MessageBoxButtons.OK, MessageBoxIcon.Error);
              return false;
            }
          }
          break;
        case DisplayMode.CREATE :
          using (IDAOSession daoSession = daoFactory.OpenSession ())
          {
            if (daoFactory.WorkOrderDAO.IfExistsWithSameName(nameTextBox.Text)) {
              MessageBox.Show (PulseCatalog.GetString("ThereIsAnotherJobWithSameName")+"\n"+PulseCatalog.GetString("PleaseChangeIt"), "",MessageBoxButtons.OK, MessageBoxIcon.Error);
              return false;
            }
            
            if (daoFactory.WorkOrderDAO.IfExistsWithSameCode(codeTextBox.Text)) {
              MessageBox.Show (PulseCatalog.GetString("ThereIsAnotherJobWithSameCode")+"\n"+PulseCatalog.GetString("PleaseChangeIt"), "",MessageBoxButtons.OK, MessageBoxIcon.Error);
              return false;
            }
            
            if (daoFactory.ProjectDAO.IfExistsWithSameName(nameTextBox.Text)) {
              MessageBox.Show (PulseCatalog.GetString("ThereIsAnotherJobWithSameName")+"\n"+PulseCatalog.GetString("PleaseChangeIt"), "",MessageBoxButtons.OK, MessageBoxIcon.Error);
              return false;
            }
            
            if (daoFactory.ProjectDAO.IfExistsWithSameCode(codeTextBox.Text)) {
              MessageBox.Show (PulseCatalog.GetString("ThereIsAnotherJobWithSameCode")+"\n"+PulseCatalog.GetString("PleaseChangeIt"), "",MessageBoxButtons.OK, MessageBoxIcon.Error);
              return false;
            }
          }
          break;
      }
      
      String msg = PulseCatalog.GetString("FollowingFieldsHaveIncorrectValues");
      bool valid = true;
      if (statusComboBox.SelectedIndex == -1) {
        msg = msg + "\n\t" + statusLbl.Text;
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
