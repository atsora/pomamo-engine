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

using Lemoine.Core.Log;

namespace Lemoine.JobControls
{
  /// <summary>
  /// Panel with fields to manage properties of an Operation.
  /// It enable a user to read, modify properties of an Operation
  /// or to create an Operation with basic information
  /// </summary>
  public partial class OperationControl : UserControl, ITreeViewObserver
  {
    /// <summary>
    /// Accepted types for display mode
    /// </summary>
    public enum DisplayMode
    {
      /// <summary>
      /// Display UserControl to view or/and modify Operation
      /// </summary>
      VIEW = 1,
      /// <summary>
      /// Display UserControl to fill information and create new Operation
      /// </summary>
      CREATE = 2
    };
    
    #region Members
    IOperation m_operation;
    IOperationType[] m_operationTypeArray;
    DisplayMode m_displayMode;
    TreeNode m_node;
    ITreeViewObservable m_observable;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (OperationControl).FullName);

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public OperationControl(DisplayMode displayMode)
    {
      InitializeComponent();

      nameLbl.Text = PulseCatalog.GetString("Name");
      typeLbl.Text = PulseCatalog.GetString("Type");
      codeLbl.Text = PulseCatalog.GetString("Code");
      documentLinkLbl.Text = PulseCatalog.GetString("DocumentLink");
      estimatedMachiningHoursLbl.Text = PulseCatalog.GetString("EstimatedMachiningHours");
      estimatedSetupHoursLbl.Text = PulseCatalog.GetString("EstimatedSetupHours");
      estimatedTearDownhoursLbl.Text = PulseCatalog.GetString("EstimatedTearDownHours");
      loadingTimeLabel.Text = PulseCatalog.GetString ("LoadingDuration");
      unloadingTimeLabel.Text = PulseCatalog.GetString ("UnloadingDuration");
      nameTextBox.Clear();
      codeTextBox.Clear();
      documentLinkTextBox.Clear();

      this.m_operation = null;
      this.m_displayMode = displayMode;
      switch (displayMode) {
        case DisplayMode.VIEW :
          saveBtn.Text = PulseCatalog.GetString("Save");
          saveBtn.Enabled = false;
          resetBtn.Text = PulseCatalog.GetString("Reset");
          baseLayout.RowStyles[12].Height = 32;
          baseLayout.RowStyles[13].Height = 0;
          break;
        case DisplayMode.CREATE :
          cancelBtn.Text = PulseCatalog.GetString("Cancel");
          createBtn.Text = PulseCatalog.GetString("Create");
          baseLayout.RowStyles[12].Height = 0;
          baseLayout.RowStyles[13].Height = 32;
          break;
      }
      typeComboBox.Items.Clear();
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      using (var daoSession = daoFactory.OpenSession ())
      {
        IList<IOperationType> operationTypes = daoFactory.OperationTypeDAO.FindAllOrderByName();
        m_operationTypeArray = new IOperationType[operationTypes.Count];
        int i = 0;
        foreach (IOperationType operationType in operationTypes) {
          m_operationTypeArray[i++] = operationType;
          typeComboBox.Items.Add((operationType.Display == null)?"":operationType.Display);
        }
        if (operationTypes.Count > 0) {
          typeComboBox.SelectedIndex = 0;
        }
      }
    }

    /// <summary>
    ///   Default constructor without argument
    /// </summary>
    public OperationControl() : this(DisplayMode.VIEW)
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
        if (selectedNode.Tag is Tuple<bool, IOperation>) {
          m_node = selectedNode;
          observable.ReloadTreeNodes (m_node);
          LoadData (((Tuple<bool, IOperation>)m_node.Tag).Item2);
        }
      }
    }
    #endregion

    #region Methods button click
    /// <summary>
    /// Create an operation information for a given operation
    /// if the old estimated machining hours duration is
    /// different from the current one
    /// </summary>
    /// <param name="operation"></param>
    /// <param name="oldMachiningDuration"></param>
    void CreateOperationInformation (IOperation operation, TimeSpan? oldMachiningDuration)
    {
      if (operation.MachiningDuration != oldMachiningDuration) {
        IOperationInformation operationInformation =
          ModelDAOHelper.ModelFactory.CreateOperationInformation (operation, DateTime.UtcNow);
        operationInformation.OldMachiningDuration = null;
        ModelDAOHelper.DAOFactory.OperationInformationDAO.MakePersistent (operationInformation);
      }
    }

    /// <summary>
    ///   Save all changes
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    void SaveBtnClick(object sender, EventArgs e)
    {
      if (!ValidateData()) { // If entered values are not correct
        return ;
      }
      m_operation.Name = nameTextBox.Text;
      m_operation.Code = codeTextBox.Text;
      m_operation.DocumentLink = documentLinkTextBox.Text;
      m_operation.Type = m_operationTypeArray[typeComboBox.SelectedIndex];
      
      TimeSpan? oldMachiningDuration = m_operation.MachiningDuration;
      
      m_operation.MachiningDuration = machiningTimeSpanPicker.Value;
      m_operation.SetUpDuration = setupTimeSpanPicker.Value;
      m_operation.TearDownDuration = teardownTimeSpanPicker.Value;
      m_operation.LoadingDuration = loadingTimeSpanPicker.Value;
      m_operation.UnloadingDuration = unloadingTimeSpanPicker.Value;
      m_operation.MachineFilter = machineFilterSelection.SelectedMachineFilter;
      m_operation.Lock = lockCheckBox.Checked;

      // Archive state
      if (!checkBoxArchive.Checked) {
        m_operation.ArchiveDateTime = null;
      } else if (!m_operation.ArchiveDateTime.HasValue) {
        m_operation.ArchiveDateTime = DateTime.Now;
        checkBoxArchive.Text = "(" + m_operation.ArchiveDateTime.Value.ToString () + ")";
      }

      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      using (IDAOSession daoSession = daoFactory.OpenSession ())
      {
        using (var transaction = daoSession.BeginTransaction ())
        {
          daoFactory.OperationDAO.MakePersistent(m_operation);
          CreateOperationInformation(m_operation, oldMachiningDuration);
          transaction.Commit();
        }
      }
      if (!Lemoine.WebClient.Request.ClearDomain ("operationinformationchange")) {
        log.ErrorFormat ("SaveBtnClick: clear domain failed");
      }
      m_observable.ReloadTreeNodes(m_node);
      m_node.TreeView.SelectedNode = m_node;
      m_node.TreeView.Focus();
      m_observable.NotifyObservers();
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
      if (m_operation == null) {
        return;
      }

      nameTextBox.Text = m_operation.Name;
      codeTextBox.Text = m_operation.Code;
      documentLinkTextBox.Text = m_operation.DocumentLink;
      machiningTimeSpanPicker.Value = m_operation.MachiningDuration;
      setupTimeSpanPicker.Value = m_operation.SetUpDuration;
      teardownTimeSpanPicker.Value = m_operation.TearDownDuration;
      loadingTimeSpanPicker.Value = m_operation.LoadingDuration;
      unloadingTimeSpanPicker.Value = m_operation.UnloadingDuration;
      machineFilterSelection.SelectedMachineFilter = m_operation.MachineFilter;
      lockCheckBox.Checked = m_operation.Lock;

      // Archive state
      if (m_operation.ArchiveDateTime.HasValue) {
        checkBoxArchive.Text = "(" + m_operation.ArchiveDateTime.Value.ToString () + ")";
        checkBoxArchive.Checked = true;
      } else {
        checkBoxArchive.Text = "";
        checkBoxArchive.Checked = false;
      }
        
      IOperationType operationType;
      for (int i = 0; i < m_operationTypeArray.Length; i++) {
        operationType = m_operationTypeArray[i];
        if (m_operation.Type.Id == operationType.Id) {
          typeComboBox.SelectedIndex = i;
          break;
        }
      }
    }
    
    /// <summary>
    /// Create an Operation
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    void CreateBtnClick(object sender, EventArgs e)
    {
      // If values entered is not correct, exit
      if (!ValidateData()) {
        return;
      }

      IOperation operation = ModelDAOHelper.ModelFactory
        .CreateOperation(m_operationTypeArray[typeComboBox.SelectedIndex]);
      operation.Name = nameTextBox.Text;
      operation.Code = codeTextBox.Text;
      operation.DocumentLink = documentLinkTextBox.Text;
      operation.MachiningDuration = machiningTimeSpanPicker.Value;
      operation.SetUpDuration = setupTimeSpanPicker.Value;
      operation.TearDownDuration = teardownTimeSpanPicker.Value;
      operation.LoadingDuration = loadingTimeSpanPicker.Value;
      operation.UnloadingDuration = unloadingTimeSpanPicker.Value;

      if (checkBoxArchive.Checked) {
        operation.ArchiveDateTime = DateTime.Now;
        checkBoxArchive.Text = operation.ArchiveDateTime.Value.ToString ();
      }

      if (this.FindForm() is CreateItemForm) {
        // created Operation must be attached to current selected node
        // if CreateFItemForm.Goal==BIND
        CreateItemForm createItemForm = this.FindForm() as CreateItemForm;
        IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
        if(createItemForm.Goal == CreateItemForm.GoalType.BIND) {
          TreeNode parentNode = createItemForm.OperationTreeView.TreeView.SelectedNode;
          IIntermediateWorkPiece intermediateWorkPiece = ((Tuple<bool, IIntermediateWorkPiece>)parentNode.Tag).Item2;
          using (IDAOSession daoSession = daoFactory.OpenSession ())
          {
            daoFactory.IntermediateWorkPieceDAO.Lock(intermediateWorkPiece);
            intermediateWorkPiece.PossibleNextOperations.Add(operation);
            operation.IntermediateWorkPieces.Add(intermediateWorkPiece);
            using (IDAOTransaction transaction = daoSession.BeginTransaction()) {
              daoFactory.OperationDAO.MakePersistent(operation);
              CreateOperationInformation(operation, null);
              daoFactory.IntermediateWorkPieceDAO.MakePersistent(intermediateWorkPiece);
              transaction.Commit();
            }
            this.m_operation = operation;
            
            createItemForm.OperationTreeView.BuildTreeNodes(parentNode);
            foreach (TreeNode childNode in parentNode.Nodes) {
              if (childNode.Name == ((Lemoine.Collections.IDataWithId)operation).Id.ToString()) {
                parentNode.TreeView.SelectedNode = childNode;
                break;
              }
            }
            parentNode.TreeView.Focus();
            createItemForm.OperationTreeView.NotifyObservers();
          }
        }
        else if (createItemForm.Goal == CreateItemForm.GoalType.NEW) {
          using (IDAOSession daoSession = ModelDAOHelper.DAOFactory.OpenSession ())
          {
            using (IDAOTransaction transaction = daoSession.BeginTransaction ())
            {
              daoFactory.OperationDAO.MakePersistent(operation);
              CreateOperationInformation(operation, null);
              transaction.Commit();
            }
          }
        }
      }
      this.FindForm().Close();
    }

    /// <summary>
    /// Close form
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

    #region Methods
    /// <summary>
    /// Use informations in Operation to fill text in box
    /// </summary>
    public void LoadData (IOperation operation)
    {
      if (operation == null) {
        return;
      }

      nameTextBox.Text = operation.Name;
      codeTextBox.Text = operation.Code;
      documentLinkTextBox.Text = operation.DocumentLink;
        
      machiningTimeSpanPicker.Value = operation.MachiningDuration;
      setupTimeSpanPicker.Value = operation.SetUpDuration;
      teardownTimeSpanPicker.Value = operation.TearDownDuration;
      loadingTimeSpanPicker.Value = operation.LoadingDuration;
      unloadingTimeSpanPicker.Value = operation.UnloadingDuration;
      machineFilterSelection.SelectedMachineFilter = operation.MachineFilter;
      lockCheckBox.Checked = operation.Lock;

      // Archive state
      if (operation.ArchiveDateTime.HasValue) {
        checkBoxArchive.Text = "(" + operation.ArchiveDateTime.Value.ToString () + ")";
        checkBoxArchive.Checked = true;
      } else {
        checkBoxArchive.Text = "";
        checkBoxArchive.Checked = false;
      }
        
      for (int i = 0; i < m_operationTypeArray.Length; i++) {
        if (operation.Type.Id == m_operationTypeArray[i].Id) {
          typeComboBox.SelectedIndex = i;
          break;
        }
      }
      resetBtn.Enabled = true;
      saveBtn.Enabled = false;
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      using (IDAOSession daoSession = daoFactory.OpenSession ())
      {
        ModelDAOHelper.DAOFactory.OperationDAO.Lock (operation);
        this.m_operation = operation;
        NHibernate.NHibernateUtil.Initialize(this.m_operation.Type);
      }
        
      // Fill list of associated IntermediateWorkPiece with this Operation
      int row = 0;
      tableLayoutPanel.RowStyles.Clear();
      tableLayoutPanel.ColumnStyles.Clear();
      tableLayoutPanel.Controls.Clear();
      tableLayoutPanel.RowCount = 1;
      tableLayoutPanel.AutoSize = true;
      Label titleLbl = new Label();
      titleLbl.Text = PulseCatalog.GetString("ListOfAssociatedIntermediateWorkPiece");
      titleLbl.AutoSize = true;
      titleLbl.Padding = new Padding(titleLbl.Padding.Left,titleLbl.Padding.Top,titleLbl.Padding.Right,5);
      titleLbl.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      titleLbl.Font = new Font(titleLbl.Font, titleLbl.Font.Style | FontStyle.Bold | FontStyle.Underline);
      tableLayoutPanel.Controls.Add(titleLbl,0,row);
      tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
      tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
      row++;
      using (IDAOSession daoSession = daoFactory.OpenSession ())
      {
        daoFactory.OperationDAO.Lock(operation);
        foreach (IIntermediateWorkPiece intermediateWorkPiece in operation.IntermediateWorkPieces) {
          LinkLabel linkLabel = new LinkLabel();
          linkLabel.AutoSize = true;
          if (intermediateWorkPiece.Display.Length == 0) {
            linkLabel.Text = "___";
          }
          else {
            linkLabel.Text = intermediateWorkPiece.Display;
          }
          linkLabel.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
          linkLabel.Height = 20;
          linkLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
          linkLabel.Tag = intermediateWorkPiece;
          linkLabel.LinkClicked += linkLabelClicked;
          tableLayoutPanel.Controls.Add(linkLabel);
          tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
          row++;
        }
      }
    }
    
    private void linkLabelClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
    {
      e.Link.Visited = true;
      IIntermediateWorkPiece intermediateWorkPiece = (IIntermediateWorkPiece)((LinkLabel)sender).Tag;
      m_observable.GiveFocusToAllNodeInstances(typeof (IIntermediateWorkPiece), intermediateWorkPiece);
    }

    void lockCheckBox_CheckedChanged (object sender, EventArgs e)
    {
      CheckHasChanged ();
    }

    void machineFilterSelection_AfterSelect (object sender, EventArgs e)
    {
      CheckHasChanged ();
    }

    /// <summary>
    /// Test if a change on field occurs and then enable or disable save button
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    void NameTextBoxTextChanged(object sender, EventArgs e)
    {
      CheckHasChanged ();
    }
    
    /// <summary>
    ///   Get whether a field has been changed
    /// </summary>
    /// <returns>True if a fields has been changed, false otherwise</returns>
    bool HasBeenChanged()
    {
      return !nameTextBox.Text.Equals(m_operation.Name ?? "") ||
        !codeTextBox.Text.Equals (m_operation.Code ?? "") ||
        !documentLinkTextBox.Text.Equals (m_operation.DocumentLink ?? "") ||
        !m_operation.MachiningDuration.Equals (machiningTimeSpanPicker.Value) ||
        !m_operation.SetUpDuration.Equals (setupTimeSpanPicker.Value) ||
        !m_operation.TearDownDuration.Equals (teardownTimeSpanPicker.Value) ||
        !m_operation.LoadingDuration.Equals (loadingTimeSpanPicker.Value) ||
        !m_operation.UnloadingDuration.Equals (unloadingTimeSpanPicker.Value) ||
        m_operationTypeArray[typeComboBox.SelectedIndex].Id != m_operation.Type.Id ||
        !object.Equals (m_operation.MachineFilter, machineFilterSelection.SelectedMachineFilter) ||
        !object.Equals (m_operation.Lock, lockCheckBox.Checked) ||
        checkBoxArchive.Checked != m_operation.ArchiveDateTime.HasValue;
    }
    
    /// <summary>
    /// Tell whether input values are corrects. If not a messagebox
    /// is displayed to tell incorrects fields.
    /// </summary>
    /// <returns>true if all fields have corrects input, false otherwise.</returns>
    bool ValidateData()
    {
      String msg = PulseCatalog.GetString("FollowingFieldsHaveIncorrectValues");
      bool valid = true;
      
      if (typeComboBox.SelectedIndex == -1) {
        msg = msg + "\n\t"+ typeLbl.Text;
        valid = false;
      }
      
      // no need to validate timePickerControls
      
      if (!valid) {
        MessageBox.Show (msg, "",MessageBoxButtons.OK, MessageBoxIcon.Error);
      }
      
      return valid;
    }

    void CheckHasChanged ()
    {
      if (m_operation != null && m_displayMode == DisplayMode.VIEW) {
        saveBtn.Enabled = HasBeenChanged ();
      }
    }
    #endregion // Methods
  }
}
