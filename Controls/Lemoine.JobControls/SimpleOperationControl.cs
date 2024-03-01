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
  /// Panel with fields to manage properties of a SimpleOperation.
  /// It enable a user to read, modify properties of a SimpleOperation
  /// or to create a SimpleOperation with basic information
  /// </summary>
  public partial class SimpleOperationControl : UserControl, ITreeViewObserver
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
    ISimpleOperation m_simpleOperation;
    IOperationType[] m_operationTypeArray;
    DisplayMode m_displayMode;
    TreeNode m_node;
    ITreeViewObservable m_observable;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger (typeof (SimpleOperationControl).FullName);

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public SimpleOperationControl (DisplayMode displayMode)
    {
      InitializeComponent ();

      nameLbl.Text = PulseCatalog.GetString ("Name");
      typeLbl.Text = PulseCatalog.GetString ("Type");
      codeLbl.Text = PulseCatalog.GetString ("Code");
      weightLbl.Text = PulseCatalog.GetString ("Weight");
      documentLinkLbl.Text = PulseCatalog.GetString ("DocumentLink");
      estimatedMachiningHoursLbl.Text = PulseCatalog.GetString ("EstimatedMachiningHours");
      estimatedSetupHoursLbl.Text = PulseCatalog.GetString ("EstimatedSetupHours");
      estimatedTearDownhoursLbl.Text = PulseCatalog.GetString ("EstimatedTearDownHours");
      loadingTimeLabel.Text = PulseCatalog.GetString ("LoadingDuration");
      unloadingTimeLabel.Text = PulseCatalog.GetString ("UnloadingDuration");
      nameTextBox.Clear ();
      codeTextBox.Clear ();
      documentLinkTextBox.Clear ();
      weightTextBox.Clear ();
      // quantityTextBox.Clear();
      quantityTextBox.Text = "1";

      this.m_simpleOperation = null;
      this.m_displayMode = displayMode;
      switch (displayMode) {
        case DisplayMode.VIEW:
          saveBtn.Text = PulseCatalog.GetString ("Save");
          saveBtn.Enabled = false;
          resetBtn.Text = PulseCatalog.GetString ("Reset");
          baseLayout.RowStyles[14].Height = 32;
          baseLayout.RowStyles[15].Height = 0;
          break;
        case DisplayMode.CREATE:
          createBtn.Text = PulseCatalog.GetString ("Create");
          cancelBtn.Text = PulseCatalog.GetString ("Cancel");
          baseLayout.RowStyles[14].Height = 0;
          baseLayout.RowStyles[15].Height = 32;
          break;
      }

      typeComboBox.Items.Clear ();
      using (IDAOSession daoSession = ModelDAOHelper.DAOFactory.OpenSession ()) {
        IList<IOperationType> operationTypes = ModelDAOHelper.DAOFactory.OperationTypeDAO.FindAllOrderByName ();
        m_operationTypeArray = new IOperationType[operationTypes.Count];
        int i = 0;
        foreach (IOperationType operationType in operationTypes) {
          m_operationTypeArray[i++] = operationType;
          typeComboBox.Items.Add ((operationType.Display == null) ? "" : operationType.Display);
        }
        if (operationTypes.Count > 0) {
          typeComboBox.SelectedIndex = 0;
        }
      }
    }
    /// <summary>
    ///   Default constructor without argument
    /// </summary>
    public SimpleOperationControl () : this (DisplayMode.VIEW)
    {
    }
    #endregion // Constructors

    #region inherited Methods
    /// <summary>
    /// Update state of this observer. In this case, SimpleOperationControl
    /// will hide or not according type of selected node  in ITreeViewObservable
    /// </summary>
    /// <param name="observable"></param>
    public void UpdateInfo (ITreeViewObservable observable)
    {
      this.m_observable = observable;
      TreeNode selectedNode = observable.GetSelectedNode ();
      if (selectedNode != null) {
        if (selectedNode.Tag is Tuple<bool, ISimpleOperation>) {
          m_node = selectedNode;
          observable.ReloadTreeNodes (m_node);
          LoadData (((Tuple<bool, ISimpleOperation>)m_node.Tag).Item2);
        }
      }
    }
    #endregion

    #region Button Click Methods
    /// <summary>
    /// Create an operation information for a given operation
    /// if the old estimated machining hours duration is
    /// different from the current one
    /// </summary>
    /// <param name="operation"></param>
    /// <param name="oldMachiningDuration"></param>
    void CreateOperationInformation (IOperation operation,
                                     TimeSpan? oldMachiningDuration)
    {
      if (operation.MachiningDuration != oldMachiningDuration) {
        IOperationInformation operationInformation =
          ModelDAOHelper.ModelFactory.CreateOperationInformation (operation, DateTime.UtcNow);
        operationInformation.OldMachiningDuration = null;
        ModelDAOHelper.DAOFactory.OperationInformationDAO.MakePersistent (operationInformation);
      }
    }

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

      m_simpleOperation.Name = nameTextBox.Text;
      m_simpleOperation.Code = codeTextBox.Text;
      m_simpleOperation.DocumentLink = documentLinkTextBox.Text;
      m_simpleOperation.Type = m_operationTypeArray[typeComboBox.SelectedIndex];
      if (weightTextBox.Text.Trim ().Length == 0) {
        m_simpleOperation.Weight = null;
      }
      else {
        m_simpleOperation.Weight = double.Parse (weightTextBox.Text.Trim ());
      }
      TimeSpan? oldMachiningDuration = m_simpleOperation.MachiningDuration;
      m_simpleOperation.MachiningDuration = machiningTimeSpanPicker.Value;
      m_simpleOperation.SetUpDuration = setupTimeSpanPicker.Value;
      m_simpleOperation.TearDownDuration = teardownTimeSpanPicker.Value;
      m_simpleOperation.LoadingDuration = loadingTimeSpanPicker.Value;
      m_simpleOperation.UnloadingDuration = unloadingTimeSpanPicker.Value;
      m_simpleOperation.Operation.MachineFilter = machineFilterSelection.SelectedMachineFilter;
      m_simpleOperation.Operation.Lock = lockCheckBox.Checked;
      m_simpleOperation.Quantity = QuantityTextBoxAsInt ();

      // Archive state
      if (!checkBoxArchive.Checked) {
        m_simpleOperation.ArchiveDateTime = null;
      }
      else if (!m_simpleOperation.ArchiveDateTime.HasValue) {
        m_simpleOperation.ArchiveDateTime = DateTime.Now;
        checkBoxArchive.Text = "(" + m_simpleOperation.ArchiveDateTime.Value.ToString () + ")";
      }

      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      using (IDAOSession daoSession = daoFactory.OpenSession ()) {
        using (IDAOTransaction transaction = daoSession.BeginTransaction ()) {
          daoFactory.SimpleOperationDAO.MakePersistent (m_simpleOperation);
          CreateOperationInformation (m_simpleOperation.Operation, oldMachiningDuration);
          transaction.Commit ();
        }
      }

      if (!Lemoine.WebClient.Request.ClearDomain ("operationinformationchange")) {
        log.ErrorFormat ("SaveBtnClick: clear domain failed");
      }

      m_observable.ReloadTreeNodes (m_node);
      m_node.TreeView.Sort (); // necessary here
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
      if (m_simpleOperation == null) {
        return;
      }

      nameTextBox.Text = m_simpleOperation.Name;
      codeTextBox.Text = m_simpleOperation.Code;
      documentLinkTextBox.Text = m_simpleOperation.DocumentLink;
      weightTextBox.Text = (m_simpleOperation.Weight.HasValue) ? m_simpleOperation.Weight.Value.ToString () : "";

      machiningTimeSpanPicker.Value = m_simpleOperation.MachiningDuration;
      setupTimeSpanPicker.Value = m_simpleOperation.SetUpDuration;
      teardownTimeSpanPicker.Value = m_simpleOperation.TearDownDuration;
      loadingTimeSpanPicker.Value = m_simpleOperation.LoadingDuration;
      unloadingTimeSpanPicker.Value = m_simpleOperation.UnloadingDuration;
      machineFilterSelection.SelectedMachineFilter = m_simpleOperation.Operation.MachineFilter;
      lockCheckBox.Checked = m_simpleOperation.Operation.Lock;

      quantityTextBox.Text = m_simpleOperation.Quantity.ToString ();

      // Archive state
      if (m_simpleOperation.ArchiveDateTime.HasValue) {
        checkBoxArchive.Text = "(" + m_simpleOperation.ArchiveDateTime.Value.ToString () + ")";
        checkBoxArchive.Checked = true;
      }
      else {
        checkBoxArchive.Text = "";
        checkBoxArchive.Checked = false;
      }

      IOperationType operationType;
      for (int i = 0; i < m_operationTypeArray.Length; i++) {
        operationType = m_operationTypeArray[i];
        if (m_simpleOperation.Type.Id == operationType.Id) {
          typeComboBox.SelectedIndex = i;
          break;
        }
      }
    }


    /// <summary>
    ///   Create a SimpleOperation
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    void CreateBtnClick (object sender, EventArgs e)
    {
      // If values entered is not correct, exit
      if (!ValidateData ()) {
        return;
      }

      ISimpleOperation simpleOperation = ModelDAOHelper.ModelFactory
        .CreateSimpleOperation (m_operationTypeArray[typeComboBox.SelectedIndex]);
      simpleOperation.Name = nameTextBox.Text;
      simpleOperation.Code = codeTextBox.Text;
      simpleOperation.DocumentLink = documentLinkTextBox.Text;
      simpleOperation.Weight = (weightTextBox.Text.Trim ().Length == 0) ? (double?)null : double.Parse (weightTextBox.Text.Trim ());
      simpleOperation.MachiningDuration = machiningTimeSpanPicker.Value;
      simpleOperation.SetUpDuration = setupTimeSpanPicker.Value;
      simpleOperation.TearDownDuration = teardownTimeSpanPicker.Value;
      simpleOperation.LoadingDuration = loadingTimeSpanPicker.Value;
      simpleOperation.UnloadingDuration = unloadingTimeSpanPicker.Value;
      simpleOperation.Operation.MachineFilter = machineFilterSelection.SelectedMachineFilter;
      simpleOperation.Operation.Lock = lockCheckBox.Checked;
      simpleOperation.Quantity = QuantityTextBoxAsInt ();

      if (checkBoxArchive.Checked) {
        simpleOperation.ArchiveDateTime = DateTime.Now;
        checkBoxArchive.Text = simpleOperation.ArchiveDateTime.Value.ToString ();
      }

      if (this.FindForm () is CreateItemForm) {
        // created Operation must be attached to current selected node
        // if CreateFItemForm.Goal==BIND
        CreateItemForm createItemForm = this.FindForm () as CreateItemForm;
        IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
        if (createItemForm.Goal == CreateItemForm.GoalType.BIND) {
          TreeNode parentNode = createItemForm.OperationTreeView.TreeView.SelectedNode;

          using (IDAOSession daoSession = daoFactory.OpenSession ()) {
            IComponent component;
            if (parentNode.Tag is Tuple<bool, IComponent>) {
              component = ((Tuple<bool, IComponent>)parentNode.Tag).Item2;
            }
            else if (parentNode.Tag is Tuple<bool, IPart>) {
              component = ((Tuple<bool, IPart>)parentNode.Tag).Item2.Component;
            }
            else {
              throw new InvalidOperationException ();
            }
            ModelDAOHelper.DAOFactory.ComponentDAO.Lock (component);
            Lemoine.Model.IComponentIntermediateWorkPiece ciwp = component.AddIntermediateWorkPiece (simpleOperation.IntermediateWorkPiece);
            using (IDAOTransaction transaction = daoSession.BeginTransaction ()) {
              daoFactory.SimpleOperationDAO.MakePersistent (simpleOperation);
              CreateOperationInformation (simpleOperation.Operation, null);
              daoFactory.ComponentDAO.MakePersistent (component);
              daoFactory.ComponentIntermediateWorkPieceDAO.MakePersistent (ciwp);
              transaction.Commit ();
            }
            this.m_simpleOperation = simpleOperation;

            createItemForm.OperationTreeView.BuildTreeNodes (parentNode);
            foreach (TreeNode childNode in parentNode.Nodes) {
              if (childNode.Name == simpleOperation.IntermediateWorkPieceId.ToString ()) {
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
              daoFactory.SimpleOperationDAO.MakePersistent (simpleOperation);
              CreateOperationInformation (simpleOperation.Operation, null);
              transaction.Commit ();
            }
            OrphanedItemsTreeView orphanedItemsTreeView = createItemForm.OperationTreeView.OrphanedItemsTreeView;
            if (orphanedItemsTreeView != null) {
              TreeNode rootNode = orphanedItemsTreeView.TreeView.Nodes.Find ("SimpleOperation", false)[0];
              TreeNode childNode = new TreeNode (simpleOperation.Display, (int)TreeViewImageIndex.SimpleOperation, (int)TreeViewImageIndex.SimpleOperation);
              childNode.Name = simpleOperation.IntermediateWorkPieceId.ToString ();
              childNode.Tag = new Tuple<bool, ISimpleOperation> (false, simpleOperation);
              rootNode.Nodes.Add (childNode);
              rootNode.TreeView.SelectedNode = childNode;
              rootNode.TreeView.Focus ();
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

    void DocumentLinkTextBoxKeyDown (object sender, KeyEventArgs e)
    {
      if ((e.KeyCode != Keys.Left) && (e.KeyCode != Keys.Right)) {
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
    /// Use informations in SimpleOperation to fill text in box
    /// </summary>
    public void LoadData (ISimpleOperation simpleOperation)
    {
      if (simpleOperation == null) {
        return;
      }

      nameTextBox.Text = simpleOperation.Name;
      codeTextBox.Text = simpleOperation.Code;
      documentLinkTextBox.Text = simpleOperation.DocumentLink;
      weightTextBox.Text = (simpleOperation.Weight == null) ? "" : simpleOperation.Weight.Value.ToString ();
      machiningTimeSpanPicker.Value = simpleOperation.MachiningDuration;
      setupTimeSpanPicker.Value = simpleOperation.SetUpDuration;
      teardownTimeSpanPicker.Value = simpleOperation.TearDownDuration;
      loadingTimeSpanPicker.Value = simpleOperation.LoadingDuration;
      unloadingTimeSpanPicker.Value = simpleOperation.UnloadingDuration;
      machineFilterSelection.SelectedMachineFilter = simpleOperation.Operation.MachineFilter;
      lockCheckBox.Checked = simpleOperation.Operation.Lock;

      // Archive state
      if (simpleOperation.ArchiveDateTime.HasValue) {
        checkBoxArchive.Text = "(" + simpleOperation.ArchiveDateTime.Value.ToString () + ")";
        checkBoxArchive.Checked = true;
      }
      else {
        checkBoxArchive.Text = "";
        checkBoxArchive.Checked = false;
      }

      for (int i = 0; i < m_operationTypeArray.Length; i++) {
        // Get index of current value
        if (simpleOperation.Type.Id == m_operationTypeArray[i].Id) {
          typeComboBox.SelectedIndex = i;
          break;
        }
      }
      resetBtn.Enabled = true;
      saveBtn.Enabled = false;
      this.m_simpleOperation = simpleOperation;

      // Fill list of Component or Part associated with this IntermediateWorkPiece
      int row = 0;
      tableLayoutPanel.RowStyles.Clear ();
      tableLayoutPanel.ColumnStyles.Clear ();
      tableLayoutPanel.Controls.Clear ();
      tableLayoutPanel.RowCount = 1;
      tableLayoutPanel.AutoSize = true;
      Label titleLbl = new Label ();
      bool isComponent = true;
      if (m_node.Parent.Tag is Tuple<bool, IComponent>) {
        titleLbl.Text = PulseCatalog.GetString ("ListOfAssociatedComponent");
        isComponent = true;
      }
      else if (m_node.Parent.Tag is Tuple<bool, IPart>) {
        titleLbl.Text = PulseCatalog.GetString ("ListOfAssociatedPart");
        isComponent = false;
      }
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
        ModelDAOHelper.DAOFactory.SimpleOperationDAO.Lock (simpleOperation);
        IIntermediateWorkPiece intermediateWorkPiece = simpleOperation.IntermediateWorkPiece;
        m_simpleOperation.Quantity = intermediateWorkPiece.OperationQuantity;
        quantityTextBox.Text = m_simpleOperation.Quantity.ToString ();

        foreach (Lemoine.Model.IComponentIntermediateWorkPiece componentIntermediateWorkPiece in intermediateWorkPiece.ComponentIntermediateWorkPieces) {
          Lemoine.Model.IComponent component = componentIntermediateWorkPiece.Component;
          LinkLabel linkLabel = new LinkLabel ();
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
            display = component.Part.Display;
            linkLabel.Tag = component.Part;
          }
          if (display.Length == 0) {
            linkLabel.Text = "___";
          }
          else {
            linkLabel.Text = display;
          }
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
      if (((LinkLabel)sender).Tag is Lemoine.Model.IComponent) {
        IComponent component = (IComponent)(((LinkLabel)sender).Tag);
        m_observable.GiveFocusToAllNodeInstances (typeof (IComponent), component);
      }
      else if (((LinkLabel)sender).Tag is IPart) {
        IPart part = ((IPart)((LinkLabel)sender).Tag);
        m_observable.GiveFocusToAllNodeInstances (typeof (IPart), part);
      }
    }

    /// <summary>
    /// Tell whether input values are correct. If not a messagebox is displayed to tell incorrects fields.
    /// </summary>
    /// <returns>true if all fields have corrects input, false otherwise.</returns>
    bool ValidateData ()
    {
      String msg = PulseCatalog.GetString ("FollowingFieldsHaveIncorrectValues");
      bool valid = true;

      if (typeComboBox.SelectedIndex == -1) {
        msg = msg + "\n\t" + typeLbl.Text;
        valid = false;
      }

      try {
        if (weightTextBox.Text.Trim ().Length != 0) {
          double.Parse (weightTextBox.Text.Trim ());
        }
      }
      catch (Exception) {
        msg = msg + "\n\t" + weightLbl.Text;
        valid = false;
      }

      // no need to validate time picker controls

      try {
        QuantityTextBoxAsInt ();
      }
      catch (System.FormatException) {
        msg = msg + "\n\t" + quantityLbl.Text;
        valid = false;
      }

      if (!valid) {
        MessageBox.Show (msg, "", MessageBoxButtons.OK, MessageBoxIcon.Error);
      }
      return valid;
    }

    /// <summary>
    /// Test if a change on field occurs and then enable or disable save button
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    void NameTextBoxTextChanged (object sender, EventArgs e)
    {
      CheckHasChanged ();
    }

    /// <summary>
    /// return Quantity Textbox as an integer may raise a FormatException
    /// </summary>
    /// <returns></returns>
    int QuantityTextBoxAsInt ()
    {
      return Int32.Parse (quantityTextBox.Text);
    }

    void QuantityTextBoxTextChanged (object sender, System.EventArgs e)
    {

    }

    /// <summary>
    /// Get whether a field has been changed
    /// </summary>
    /// <returns>True if a fields has been changed, false otherwise</returns>
    bool HasBeenChanged ()
    {
      if (!nameTextBox.Text.Equals (m_simpleOperation.Name ?? "") ||
        !codeTextBox.Text.Equals (m_simpleOperation.Code ?? "") ||
        !documentLinkTextBox.Text.Equals (m_simpleOperation.DocumentLink ?? "") ||
        !m_simpleOperation.MachiningDuration.Equals (machiningTimeSpanPicker.Value) ||
        !m_simpleOperation.SetUpDuration.Equals (setupTimeSpanPicker.Value) ||
        !m_simpleOperation.TearDownDuration.Equals (teardownTimeSpanPicker.Value) ||
        !m_simpleOperation.LoadingDuration.Equals (loadingTimeSpanPicker.Value) ||
        !m_simpleOperation.UnloadingDuration.Equals (unloadingTimeSpanPicker.Value) ||
        m_operationTypeArray[typeComboBox.SelectedIndex].Id != m_simpleOperation.Type.Id ||
        !object.Equals (m_simpleOperation.Operation.MachineFilter, machineFilterSelection.SelectedMachineFilter) ||
        !object.Equals (m_simpleOperation.Operation.Lock, lockCheckBox.Checked) ||
        checkBoxArchive.Checked != m_simpleOperation.ArchiveDateTime.HasValue) {
        return true;
      }

      String s = (m_simpleOperation.Weight == null) ? "" : m_simpleOperation.Weight.ToString ();
      if (!weightTextBox.Text.Trim ().Equals (s)) {
        return true;
      }

      try {
        return (m_simpleOperation.Quantity != QuantityTextBoxAsInt ());
      }
      catch (FormatException) {
        return true;
      }
    }

    void CheckHasChanged ()
    {
      if (m_simpleOperation != null && m_displayMode == DisplayMode.VIEW) {
        saveBtn.Enabled = HasBeenChanged ();
      }
    }

    private void lockCheckBox_CheckedChanged (object sender, EventArgs e)
    {
      CheckHasChanged ();
    }

    private void machineFilterSelection_AfterSelect (object sender, EventArgs e)
    {
      CheckHasChanged ();
    }
    #endregion // Methods
  }
}
