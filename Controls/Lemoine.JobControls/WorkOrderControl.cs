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
  /// Panel with fields to manage properties of a WorkOrder.
  /// It enable a user to read, modify properties of a WorkOrder
  /// or to create a WorkOrder with basic information
  /// </summary>
  public partial class WorkOrderControl : UserControl, ITreeViewObserver
  {
    /// <summary>
    /// Accepted types for display mode
    /// </summary>
    public enum DisplayMode {
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
    IWorkOrder m_workOrder;
    DisplayMode m_displayMode;
    TreeNode m_node;
    IWorkOrderStatus [] m_workOrderStatusArray;
    ITreeViewObservable m_observable;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (WorkOrderControl).FullName);

    #region Getters / Setters
    #endregion // Getters / Setters
    
    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public WorkOrderControl(DisplayMode displayMode)
    {
      //
      // The InitializeComponent() call is required for Windows Forms designer support.
      //
      InitializeComponent();
      nameLbl.Text = PulseCatalog.GetString("Name");
      codeLbl.Text = PulseCatalog.GetString("Code");
      documentLinkLbl.Text = PulseCatalog.GetString("DocumentLink");
      statusLbl.Text = PulseCatalog.GetString("Status");
      nameTextBox.Clear();
      codeTextBox.Clear();
      documentLinkTextBox.Clear();
      this.m_workOrder = null;
      this.m_displayMode = displayMode;
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      using (IDAOSession daoSession = daoFactory.OpenSession ())
      {
        
        IList<IWorkOrderStatus> workOrderStatus = daoFactory.WorkOrderStatusDAO.FindAllOrderByName();
        m_workOrderStatusArray = new IWorkOrderStatus[workOrderStatus.Count];
        int i = 0;
        int indexForWorkOrderNew = 0;
        statusComboBox.Items.Clear();
        foreach (IWorkOrderStatus aWorkOrderStatus in workOrderStatus) {
          m_workOrderStatusArray[i] = aWorkOrderStatus;
          statusComboBox.Items.Add((aWorkOrderStatus.Display == null)?"":aWorkOrderStatus.Display);
          if (PulseCatalog.GetString("New").Equals(aWorkOrderStatus.Display)) {
            indexForWorkOrderNew = i;
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
          createBtn.Visible = false;
          cancelBtn.Visible = false;
          break;
        case DisplayMode.CREATE :
          createBtn.Text = PulseCatalog.GetString("Create");
          cancelBtn.Text = PulseCatalog.GetString("Cancel");
          saveBtn.Visible = false;
          resetBtn.Visible = false;
          break;
      }
    }
    /// <summary>
    ///   Default constructor without argument
    /// </summary>
    public WorkOrderControl() : this(DisplayMode.VIEW)
    {
    }
    #endregion // Constructors
    
    #region inherited Methods
    /// <summary>
    /// Update state of this observer. In this case, WorkOrderControl will hide or not
    /// according type of selected node  in ITreeViewObservable
    /// </summary>
    /// <param name="observable"></param>
    public void UpdateInfo (ITreeViewObservable observable)
    {
      this.m_observable = observable;
      TreeNode selectedNode = observable.GetSelectedNode();
      if (selectedNode != null) {
        if (selectedNode.Tag is Tuple<bool, IWorkOrder>) {
          m_node = selectedNode;
          using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
          {
            observable.ReloadTreeNodes (m_node);
            IWorkOrder workOrder = ((Tuple<bool, IWorkOrder>)m_node.Tag).Item2;
            LoadData (workOrder);
          }
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
    void SaveBtnClick(object sender, EventArgs e) {
      if(!ValidateData()) {
        return;
      }
      m_workOrder.Name = nameTextBox.Text;
      m_workOrder.Code = codeTextBox.Text;
      m_workOrder.DocumentLink = documentLinkTextBox.Text;
      m_workOrder.Status = m_workOrderStatusArray[statusComboBox.SelectedIndex];
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      using (IDAOSession daoSession = daoFactory.OpenSession ())
      {
        using (IDAOTransaction transaction = daoSession.BeginTransaction ())
        {
          daoFactory.WorkOrderDAO.MakePersistent(m_workOrder);
          transaction.Commit();
        }
        
        if (m_node != null) {
          m_node.Tag = new Tuple<bool, IWorkOrder> (((Tuple<bool, IWorkOrder>)m_node.Tag).Item1, m_workOrder);
          m_node.Text = m_workOrder.Display;
          m_node.TreeView.Sort();
          m_observable.ReloadTreeNodes(m_node);
          m_node.TreeView.SelectedNode = m_node;
          m_node.TreeView.Focus();
          m_observable.NotifyObservers();
        }
        
        saveBtn.Enabled = false;
      }
    }
    
    /// <summary>
    ///   Discard change and put current values in fields
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    void ResetBtnClick(object sender, EventArgs e) {
      if (m_workOrder != null) {
        nameTextBox.Text = m_workOrder.Name;
        codeTextBox.Text = m_workOrder.Code;
        documentLinkTextBox.Text = m_workOrder.DocumentLink;
        IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
        using (IDAOSession daoSession = daoFactory.OpenSession ()) {
          daoFactory.WorkOrderDAO.Lock(m_workOrder);
          NHibernate.NHibernateUtil.Initialize(m_workOrder.Status);
        }
        for (int i = 0; i < m_workOrderStatusArray.Length; i++) {
          if (m_workOrder.Status.Id == m_workOrderStatusArray[i].Id) {
            statusComboBox.SelectedIndex = i;
            break;
          }
        }
      }
      saveBtn.Enabled = false;
    }
    
    /// <summary>
    ///   Close form
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    void CancelBtnClick(object sender, EventArgs e) {
      this.FindForm().Close();
    }
    
    /// <summary>
    ///   Create a WorkOrder
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    void CreateBtnClick(object sender, EventArgs e) {
      //if values entered is not correct, exit
      if (!ValidateData()) {
        return;
      }
      IWorkOrder workOrder = ModelDAOHelper.ModelFactory
        .CreateWorkOrder (m_workOrderStatusArray[statusComboBox.SelectedIndex],
                          nameTextBox.Text);
      workOrder.Code = codeTextBox.Text;
      workOrder.DocumentLink = documentLinkTextBox.Text;
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      using (IDAOSession daoSession = daoFactory.OpenSession ()) {
        using (IDAOTransaction transaction = daoSession.BeginTransaction()) {
          daoFactory.WorkOrderDAO.MakePersistent(workOrder);
          transaction.Commit();
        }
      }
      
      if(this.FindForm() is CreateItemForm) {
        // created WorkOrder must always be attached to operationTree
        // whatever value of CreateFItemForm.Goal
        CreateItemForm createItemForm = this.FindForm() as CreateItemForm;
        TreeNode treeNode = new TreeNode(workOrder.Display,(int)TreeViewImageIndex.WorkOrder,(int)TreeViewImageIndex.WorkOrder);
        treeNode.Name = ((Lemoine.Collections.IDataWithId<int>)workOrder).Id.ToString();
        treeNode.Tag = new Tuple<bool, IWorkOrder> (false, workOrder);
        createItemForm.OperationTreeView.TreeView.Nodes.Add(treeNode);
        //TreeNode treeNode =  createItemForm.OperationTreeView.TreeView.Nodes
        // .Add(workOrder.Id.ToString(),workOrder.Display,(int)TreeViewBasicsOperations.ItemIndex.IDX_WORKORDER,(int)TreeViewBasicsOperations.ItemIndex.IDX_WORKORDER);
        //TreeViewBasicsOperations.SortTreeNodesWorkOrder(createItemForm.OperationTreeView.TreeView.Nodes);
        createItemForm.OperationTreeView.TreeView.Focus();
        createItemForm.OperationTreeView.TreeView.SelectedNode = treeNode;
        createItemForm.OperationTreeView.NotifyObservers();
      }
      this.FindForm().Close();
    }
    
    #endregion

    #region Methods
    
    /// <summary>
    /// Use informations in WorkOrder to fill text in box
    /// </summary>
    public void LoadData(IWorkOrder workOrder){
      if (workOrder != null) {
        this.m_workOrder = workOrder;
        nameTextBox.Text = workOrder.Name;
        codeTextBox.Text = workOrder.Code;
        documentLinkTextBox.Text = workOrder.DocumentLink;
        IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
        using (IDAOSession daoSession = daoFactory.OpenSession ())
        {
          ModelDAOHelper.DAOFactory.WorkOrderDAO.Lock (workOrder);
          this.m_workOrder = workOrder;
          NHibernate.NHibernateUtil.Initialize(this.m_workOrder.Status);
          for (int i = 0; i < m_workOrderStatusArray.Length; i++) {
            if (this.m_workOrder.Status.Id == m_workOrderStatusArray[i].Id) {
              statusComboBox.SelectedIndex = i;
              break;
            }
          }
        }

        resetBtn.Enabled = true;
        saveBtn.Enabled = false;
      }
    }

    
    /// <summary>
    ///   get whether a field has been changed
    /// </summary>
    /// <returns>True if a fields has been changed, false otherwise</returns>
    bool HasBeenChanged(){
      String s;
      s = (m_workOrder.Name==null)?"":m_workOrder.Name;
      if (!nameTextBox.Text.Equals(s)) {
        return true;
      }
      s = (m_workOrder.Code==null)?"":m_workOrder.Code;
      if (!codeTextBox.Text.Equals(s)) {
        return true;
      }
      s = (m_workOrder.DocumentLink==null)?"":m_workOrder.DocumentLink;
      if (!documentLinkTextBox.Text.Equals(s)) {
        return true;
      }
      if (m_workOrderStatusArray[statusComboBox.SelectedIndex].Id != m_workOrder.Status.Id) {
        return true;
      }
      return false;
    }
    
    /// <summary>
    ///   Test if a change on field occurs and then enable or disable
    ///   save button
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    void NameTextBoxTextChanged(object sender, EventArgs e) {
      if ((m_workOrder != null) && (m_displayMode == DisplayMode.VIEW)) {
        if (HasBeenChanged()) {
          saveBtn.Enabled = true;
        }
        else {
          saveBtn.Enabled = false;
        }
      }
    }
    
    
    /// <summary>
    ///   Tell whether input values are corrects. If not a messagebox
    ///   is displayed to tell incorrects fields.
    /// </summary>
    /// <returns>true if all fields have corrects input, false otherwise.</returns>
    private bool ValidateData() {
      if ((nameTextBox.Text.Length == 0) && (codeTextBox.Text.Length == 0)) {
        MessageBox.Show (PulseCatalog.GetString("FieldsNameAndCodeCanNotHaveBothNullValue"), "",MessageBoxButtons.OK, MessageBoxIcon.Error);
        return false;
      }
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      switch (m_displayMode) {
        case DisplayMode.VIEW :
          using(IDAOSession daoSession = daoFactory.OpenSession ())
          {
            if (daoFactory.WorkOrderDAO.IfExistsOtherWithSameName(nameTextBox.Text,((Lemoine.Collections.IDataWithId)m_workOrder).Id)) {
              MessageBox.Show (PulseCatalog.GetString("ThereIsAnotherWorkOrderWithSameName")+"\n"+PulseCatalog.GetString("PleaseChangeIt"), "",MessageBoxButtons.OK, MessageBoxIcon.Error);
              return false;
            }
            
            if (daoFactory.WorkOrderDAO.IfExistsOtherWithSameCode(codeTextBox.Text,((Lemoine.Collections.IDataWithId)m_workOrder).Id)) {
              MessageBox.Show (PulseCatalog.GetString("ThereIsAnotherWorkOrderWithSameCode")+"\n"+PulseCatalog.GetString("PleaseChangeIt"), "",MessageBoxButtons.OK, MessageBoxIcon.Error);
              return false;
            }
          }

          break;
        case DisplayMode.CREATE :
          using(IDAOSession daoSession = daoFactory.OpenSession ())
          {
            if (daoFactory.WorkOrderDAO.IfExistsWithSameName(nameTextBox.Text)) {
              MessageBox.Show (PulseCatalog.GetString("ThereIsAnotherWorkOrderWithSameName")+"\n"+PulseCatalog.GetString("PleaseChangeIt"), "",MessageBoxButtons.OK, MessageBoxIcon.Error);
              return false;
            }
            if (daoFactory.WorkOrderDAO.IfExistsWithSameCode(codeTextBox.Text)) {
              MessageBox.Show (PulseCatalog.GetString("ThereIsAnotherWorkOrderWithSameCode")+"\n"+PulseCatalog.GetString("PleaseChangeIt"), "",MessageBoxButtons.OK, MessageBoxIcon.Error);
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
        MessageBox.Show (msg, "",MessageBoxButtons.OK, MessageBoxIcon.Error);
      }
      return valid;
    }

    #endregion // Methods
    
    void DocumentLinkBtnClick(object sender, EventArgs e)
    {
      if (openFileDialog.ShowDialog() == DialogResult.OK)
      {
        this.documentLinkTextBox.Text = openFileDialog.FileName;
      }
    }
    
    void DocumentLinkTextBoxKeyDown(object sender, KeyEventArgs e)
    {
      if ((e.KeyCode != Keys.Left)&&(e.KeyCode != Keys.Right)) {
        e.Handled = true;
        e.SuppressKeyPress = true;
      }
    }

  }
}
