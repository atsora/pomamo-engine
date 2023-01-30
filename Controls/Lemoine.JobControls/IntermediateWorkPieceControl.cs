// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Diagnostics;
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
  /// Panel with fields to manage properties of an IntermediateWorkPiece.
  /// It enable a user to read, modify properties of an IntermediateWorkPiece
  /// or to create an IntermediateWorkPiece with basic information
  /// </summary>
  public partial class IntermediateWorkPieceControl : UserControl, ITreeViewObserver
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
    IIntermediateWorkPiece m_intermediateWorkPiece;
    DisplayMode m_displayMode;
    TreeNode m_node;
    ITreeViewObservable m_observable;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (IntermediateWorkPieceControl).FullName);

    #region Getters / Setters
    #endregion // Getters / Setters

    
    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public IntermediateWorkPieceControl(DisplayMode displayMode)
    {
      //
      // The InitializeComponent() call is required for Windows Forms designer support.
      //
      InitializeComponent();
      nameLbl.Text = PulseCatalog.GetString("Name");
      codeLbl.Text = PulseCatalog.GetString("Code");
      documentLinkLbl.Text = PulseCatalog.GetString("DocumentLink");
      weightLbl.Text = PulseCatalog.GetString("Weight");
      operationQuantityLbl.Text = PulseCatalog.GetString("OperationQuantity");
      nameTextBox.Clear();
      codeTextBox.Clear();
      documentLinkTextBox.Clear();
      weightTextBox.Clear();
      operationQuantityNumericUpDown.Value = operationQuantityNumericUpDown.Minimum;

      m_intermediateWorkPiece = null;
      this.m_displayMode = displayMode;
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
    public IntermediateWorkPieceControl() : this(DisplayMode.VIEW)
    {
    }
    
    #endregion // Constructors

    
    #region inherited Methods
    
    /// <summary>
    ///   Update state of this observer. In this case, IntermediateWorkPieceControl
    ///   will hide or not according type of selected node  in ITreeViewObservable
    /// </summary>
    /// <param name="observable"></param>
    public void UpdateInfo (ITreeViewObservable observable)
    {
      this.m_observable = observable;
      TreeNode selectedNode = observable.GetSelectedNode();
      if (selectedNode != null) {
        if (selectedNode.Tag is Tuple<bool, IIntermediateWorkPiece>) {
          m_node = selectedNode;
          observable.ReloadTreeNodes (m_node);
          IIntermediateWorkPiece intermediateWorkPiece = ((Tuple<bool, IIntermediateWorkPiece>)m_node.Tag).Item2;
          LoadData (intermediateWorkPiece);
        }
      }
    }
    
    #endregion

    
    #region Methods
    
    /// <summary>
    /// Use informations in IntermediateWorkPiece to fill text in box
    /// </summary>
    public void LoadData(IIntermediateWorkPiece intermediateWorkPiece){
      if (intermediateWorkPiece != null) {
        nameTextBox.Text = intermediateWorkPiece.Name;
        codeTextBox.Text = intermediateWorkPiece.Code;
        documentLinkTextBox.Text = intermediateWorkPiece.DocumentLink;
        weightTextBox.Text = (intermediateWorkPiece.Weight == null)? "": intermediateWorkPiece.Weight.Value.ToString();
        operationQuantityNumericUpDown.Value = intermediateWorkPiece.OperationQuantity;
        resetBtn.Enabled = true;
        saveBtn.Enabled = false;
        
        // Fill list of Component or Part associated with this IntermediateWorkPiece
        int row = 0;
        tableLayoutPanel.RowStyles.Clear();
        tableLayoutPanel.ColumnStyles.Clear();
        tableLayoutPanel.Controls.Clear();
        tableLayoutPanel.RowCount = 1;
        tableLayoutPanel.AutoSize = true;
        Label titleLbl = new Label();
        bool isComponent = true;
        if (m_node.Parent.Tag is Tuple<bool, IComponent>) {
          titleLbl.Text = PulseCatalog.GetString("ListOfAssociatedComponent");
          isComponent = true;
        }
        else if (m_node.Parent.Tag is Tuple<bool, IPart>) {
          titleLbl.Text = PulseCatalog.GetString("ListOfAssociatedPart");
          isComponent = false;
        }
        
        titleLbl.AutoSize = true;
        titleLbl.Padding = new Padding(titleLbl.Padding.Left,titleLbl.Padding.Top,titleLbl.Padding.Right,5);
        titleLbl.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        titleLbl.Font = new Font(titleLbl.Font, titleLbl.Font.Style | FontStyle.Bold | FontStyle.Underline);
        tableLayoutPanel.Controls.Add(titleLbl,0,row);
        tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        row++;
        IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
        using (IDAOSession daoSession = daoFactory.OpenSession ())
        {
          ModelDAOHelper.DAOFactory.IntermediateWorkPieceDAO.Lock (intermediateWorkPiece);
          this.m_intermediateWorkPiece = intermediateWorkPiece;
          NHibernate.NHibernateUtil.Initialize(this.m_intermediateWorkPiece.ComponentIntermediateWorkPieces);
          foreach (Lemoine.Model.IComponentIntermediateWorkPiece componentIntermediateWorkPiece in this.m_intermediateWorkPiece.ComponentIntermediateWorkPieces) {
            Lemoine.Model.IComponent component = componentIntermediateWorkPiece.Component;
            LinkLabel linkLabel = new LinkLabel();
            linkLabel.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
            linkLabel.Height = 20;
            linkLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            linkLabel.AutoSize = true;
            String display = "";
            if (isComponent) {
              display = component.Display;
              linkLabel.Tag = component;
            }
            else {
              IPart part = component.Part;
              display = part.Display;
              linkLabel.Tag = part;
            }
            if (display.Length == 0) {
              linkLabel.Text = "___";
            }
            else {
              linkLabel.Text = display;
            }
            linkLabel.LinkClicked += linkLabelClicked;
            tableLayoutPanel.Controls.Add(linkLabel,0,row);
            tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            row++;
          }
        }
      }
    }
    
    
    
    private void linkLabelClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
    {
      e.Link.Visited = true;
      if (((LinkLabel)sender).Tag is Lemoine.Model.IComponent) {
        Lemoine.Model.IComponent component = (Lemoine.Model.IComponent)(((LinkLabel)sender).Tag);
        m_observable.GiveFocusToAllNodeInstances(typeof(Lemoine.Model.IComponent), component);
      }
      else if (((LinkLabel)sender).Tag is IPart){
        IPart part = (IPart)(((LinkLabel)sender).Tag);
        m_observable.GiveFocusToAllNodeInstances(typeof(IPart), part);
      }
    }

    
    /// <summary>
    ///   Tell whether input values are corrects. If not a messagebox
    ///   is displayed to tell incorrects fields.
    /// </summary>
    /// <returns>true if all fields have corrects input, false otherwise.</returns>
    bool ValidateData(){
      String msg = PulseCatalog.GetString("FollowingFieldsHaveIncorrectValues");
      bool valid = true;

      try {
        if (weightTextBox.Text.Trim().Length != 0) {
          double.Parse(weightTextBox.Text.Trim());
        }
      }
      catch (Exception) {
        msg = msg + "\n\t"+ weightLbl.Text;
        valid = false;
      }
      if (!valid) {
        MessageBox.Show (msg, "",MessageBoxButtons.OK, MessageBoxIcon.Error);
      }
      return valid;
    }
    
    /// <summary>
    ///   Test if a change on field occurs and then enable or disable
    ///   save button
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    void NameTextBoxTextChanged(object sender, EventArgs e) {
      if ((m_intermediateWorkPiece !=  null) && (m_displayMode == DisplayMode.VIEW)) {
        if (HasBeenChanged()) {
          saveBtn.Enabled = true;
        }
        else {
          saveBtn.Enabled = false;
        }
      }
    }
    
    /// <summary>
    ///   Get whether a field has been changed
    /// </summary>
    /// <returns>True if a fields has been changed, false otherwise</returns>
    bool HasBeenChanged(){
      String s;
      s = (m_intermediateWorkPiece.Name==null)?"":m_intermediateWorkPiece.Name;
      if (!nameTextBox.Text.Equals(s)) {
        return true;
      }
      s = (m_intermediateWorkPiece.Code==null)?"":m_intermediateWorkPiece.Code;
      if (!codeTextBox.Text.Equals(s)) {
        return true;
      }
      s = (m_intermediateWorkPiece.DocumentLink==null)?"":m_intermediateWorkPiece.DocumentLink;
      if (!documentLinkTextBox.Text.Equals(s)) {
        return true;
      }
      s = (m_intermediateWorkPiece.Weight==null)?"":m_intermediateWorkPiece.Weight.ToString();
      if (!weightTextBox.Text.Trim().Equals(s)) {
        return true;
      }

      if (m_intermediateWorkPiece.OperationQuantity != Convert.ToInt32(operationQuantityNumericUpDown.Value)) {
        return true;
      }
      
      return false;
    }

    #endregion // Methods

    
    #region Button Click Methods
    
    /// <summary>
    ///   Save all changes
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    void SaveBtnClick(object sender, EventArgs e) {
      if (!ValidateData()) {//If entered values are not correct
        return ;
      }
      m_intermediateWorkPiece.Name = nameTextBox.Text;
      m_intermediateWorkPiece.Code = codeTextBox.Text;
      m_intermediateWorkPiece.DocumentLink = documentLinkTextBox.Text;
      var oldOperationQuantity = m_intermediateWorkPiece.OperationQuantity;
      m_intermediateWorkPiece.OperationQuantity = int.Parse(operationQuantityNumericUpDown.Text);
      if (weightTextBox.Text.Trim().Length == 0) {
        m_intermediateWorkPiece.Weight = null;
      }
      else {
        m_intermediateWorkPiece.Weight = double.Parse(weightTextBox.Text.Trim());
      }
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      using (IDAOSession daoSession = daoFactory.OpenSession ()) {
        using (IDAOTransaction transaction = daoSession.BeginTransaction ()) {
          daoFactory.IntermediateWorkPieceDAO.MakePersistent(m_intermediateWorkPiece);
          transaction.Commit();
        }
      }
      if (oldOperationQuantity != m_intermediateWorkPiece.OperationQuantity) {
        if (!Lemoine.WebClient.Request.ClearDomain ("operationinformationchange")) {
          log.ErrorFormat ("SaveBtnClick: clear domain failed");
        }
      }
      m_observable.ReloadTreeNodes(m_node);
      m_node.TreeView.SelectedNode = m_node;
      m_node.TreeView.Focus();
      m_observable.NotifyObservers();
      saveBtn.Enabled = false;
    }
    
    
    /// <summary>
    ///   Discard change and put current values in fields
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    void ResetBtnClick(object sender, EventArgs e) {
      if (m_intermediateWorkPiece != null) {
        nameTextBox.Text = m_intermediateWorkPiece.Name;
        codeTextBox.Text = m_intermediateWorkPiece.Code;
        documentLinkTextBox.Text = m_intermediateWorkPiece.DocumentLink;
        weightTextBox.Text = (m_intermediateWorkPiece.Weight == null)? "" : m_intermediateWorkPiece.Weight.Value.ToString();
        operationQuantityNumericUpDown.Value = m_intermediateWorkPiece.OperationQuantity;
      }
      saveBtn.Enabled = false;
    }
    
    
    /// <summary>
    ///   Create an IntermediateWorkPiece
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    void CreateBtnClick(object sender, EventArgs e) {
      //if values entered is not correct, exit
      if (!ValidateData()) {
        return;
      }
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      IIntermediateWorkPiece intermediateWorkPiece = ModelDAOHelper.ModelFactory.CreateIntermediateWorkPiece(null);
      intermediateWorkPiece.Name = nameTextBox.Text;
      intermediateWorkPiece.Code = codeTextBox.Text;
      intermediateWorkPiece.DocumentLink = documentLinkTextBox.Text;
      intermediateWorkPiece.OperationQuantity = Convert.ToInt32(Math.Floor(operationQuantityNumericUpDown.Value));
      if (weightTextBox.Text.Trim().Length == 0) {
        intermediateWorkPiece.Weight = null;
      }
      else {
        intermediateWorkPiece.Weight = double.Parse(weightTextBox.Text.Trim());
      }

      IOperation operation;
      
      using (IDAOSession daoSession = daoFactory.OpenSession ())
      {
        IOperationType operationType = daoFactory.OperationTypeDAO.FindById(1);
        operation = ModelDAOHelper.ModelFactory.CreateOperation(operationType);
      }
      intermediateWorkPiece.Operation = operation;
      
      if(this.FindForm() is CreateItemForm) {
        // new intermediateWorkPiece must be attached to selected node if CreateItemForm.Goal==BIND
        CreateItemForm createItemForm = this.FindForm() as CreateItemForm;
        if(createItemForm.Goal == CreateItemForm.GoalType.BIND) {
          TreeNode parentNode = createItemForm.OperationTreeView.TreeView.SelectedNode;
          using (IDAOSession daoSession = daoFactory.OpenSession ())
          {
            using (IDAOTransaction transaction = daoSession.BeginTransaction ())
            {
              daoFactory.OperationDAO.MakePersistent(operation);
              daoFactory.IntermediateWorkPieceDAO.MakePersistent(intermediateWorkPiece);
              if(parentNode.Tag is Tuple<bool, IComponent>) {
                Lemoine.Model.IComponent component = ((Tuple<bool, IComponent>)parentNode.Tag).Item2;
                daoFactory.ComponentDAO.Lock(component);
                Lemoine.Model.IComponentIntermediateWorkPiece ciwp = component.AddIntermediateWorkPiece (intermediateWorkPiece);
                daoFactory.ComponentDAO.MakePersistent(component);
                daoFactory.ComponentIntermediateWorkPieceDAO.MakePersistent(ciwp);
              }
              else if(parentNode.Tag is Tuple<bool, IPart>) {
                IPart part = (IPart)(((Tuple<bool, IPart>)parentNode.Tag).Item2);
                ModelDAOHelper.DAOFactory.PartDAO.Lock (part);
                IComponentIntermediateWorkPiece ciwp = part.AddIntermediateWorkPiece (intermediateWorkPiece);
                daoFactory.ComponentIntermediateWorkPieceDAO.MakePersistent(ciwp);
              }
              transaction.Commit();
            }
            this.m_intermediateWorkPiece = intermediateWorkPiece;
            createItemForm.OperationTreeView.BuildTreeNodes(parentNode);
            foreach (TreeNode childNode in parentNode.Nodes) {
              if (childNode.Name == ((Lemoine.Collections.IDataWithId)intermediateWorkPiece).Id.ToString()) {
                parentNode.TreeView.SelectedNode = childNode;
                break;
              }
            }
            parentNode.TreeView.Focus();
            createItemForm.OperationTreeView.NotifyObservers();
          }
        }
        // new IntermediateWorkPiece is just save if CreateItemForm.Goal==NEW
        else if(createItemForm.Goal == CreateItemForm.GoalType.NEW){
          using (IDAOSession daoSession = daoFactory.OpenSession ())
          {
            using (IDAOTransaction transaction = daoSession.BeginTransaction ())
            {
              daoFactory.OperationDAO.MakePersistent(operation);
              daoFactory.IntermediateWorkPieceDAO.MakePersistent(intermediateWorkPiece);
              transaction.Commit();
            }
            OrphanedItemsTreeView orphanedItemsTreeView = createItemForm.OperationTreeView.OrphanedItemsTreeView;
            if (orphanedItemsTreeView != null) {
              TreeNode rootNode = orphanedItemsTreeView.TreeView.Nodes.Find("IntermediateWorkPiece",false)[0];
              TreeNode childNode = new TreeNode(intermediateWorkPiece.Display,(int)TreeViewImageIndex.IntermediateWorkPiece,(int)TreeViewImageIndex.IntermediateWorkPiece);
              childNode.Name = ((Lemoine.Collections.IDataWithId)intermediateWorkPiece).Id.ToString();
              childNode.Tag = new Tuple<bool, IIntermediateWorkPiece> (false, intermediateWorkPiece);
              rootNode.Nodes.Add(childNode);
              rootNode.TreeView.SelectedNode = childNode;
              rootNode.TreeView.Focus();
            }
          }
        }
      }
      this.FindForm().Close();
    }
    
    
    /// <summary>
    ///   Close form
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    void CancelBtnClick(object sender, EventArgs e) {
      this.FindForm().Close();
    }
    
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
    
    #endregion
    
    
  }
}
