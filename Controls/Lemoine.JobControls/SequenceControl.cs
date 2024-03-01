// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Data;

using Lemoine.I18N;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.DataReferenceControls;
using Lemoine.Collections;

using Lemoine.Core.Log;


namespace Lemoine.JobControls
{
  /// <summary>
  /// Panel with fields to manage properties of a Sequence.
  /// It enable a user to read, modify properties of a Sequence
  /// or to create a Sequence with basic information
  /// </summary>
  public partial class SequenceControl : UserControl, ITreeViewObserver
  {
    /// <summary>
    /// Accepted types for display mode
    /// </summary>
    public enum DisplayMode
    {
      /// <summary>
      /// Display UserControl to view or/and modify Sequence
      /// </summary>
      VIEW = 1,
      /// <summary>
      /// Display UserControl to fill information and create new Sequence
      /// </summary>
      CREATE = 2
    };

    #region Members
    ISequence m_sequence;
    DisplayMode m_displayMode;
    TreeNode m_node;
    ITreeViewObservable m_observable;
    Type m_parentType;
    IPath m_path;
    IOperation m_operation;
    ToolDialog m_toolDialog;
    CadModelDialog m_cadModelDialog;
    FieldDialog m_fieldDialog;
    DataTable m_dataTable;
    DataTable m_stampingDetailsDataTable;
    bool m_showPath = true;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger (typeof (SequenceControl).FullName);

    #region Getters / Setters
    /// <summary>
    /// Getter / Setter for type of parent of Sequence node
    /// </summary>
    public Type ParentType
    {
      get { return m_parentType; }
      set {
        m_parentType = value;
        if (m_parentType != null) {
          if (m_parentType.Equals (typeof (IOperation))) {
            operationLbl.Text = PulseCatalog.GetString ("Operation");
          }
          else if (m_parentType.Equals (typeof (ISimpleOperation))) {
            operationLbl.Text = PulseCatalog.GetString ("SimpleOperation");
          }
        }
      }
    }

    /// <summary>
    /// Getter / Setter for Path related to this Sequence
    /// </summary>
    public IPath Path
    {
      get { return m_path; }
      set {
        m_path = value;
        if (m_path != null) {
          pathTextBox.Text = m_path.Number.ToString ();
          pathTextBox.Tag = ((Lemoine.Collections.IDataWithId)m_path).Id;
          pathTextBox.Text = m_path.Number.ToString ();
          this.Operation = m_path.Operation;
        }
      }
    }

    /// <summary>
    /// Getter / Setter for Operation related to this Sequence
    /// </summary>
    public IOperation Operation
    {
      get { return m_operation; }
      set {
        m_operation = value;
        if (m_operation != null) {
          operationTextBox.Text = m_operation.Name;
          operationTextBox.Tag = ((Lemoine.Collections.IDataWithId)m_operation).Id;
        }
      }
    }

    /// <summary>
    /// Getter for DataTable which contains stampingvalue information associated to the sequence
    /// </summary>
    public DataTable DataTable
    {
      get { return m_dataTable; }
    }

    /// <summary>
    /// Getter for DataTable which contains stamping details information associated to the sequence
    /// </summary>
    public DataTable StampingDetailsDataTable
    {
      get { return m_stampingDetailsDataTable; }
    }

    /// <summary>
    /// Getter / Setter for Sequence binds with this control
    /// </summary>
    public ISequence Sequence
    {
      get { return m_sequence; }
      set { m_sequence = value; }
    }

    /// <summary>
    /// Should we show path information or not ?
    /// </summary>
    public bool ShowPath
    {
      get { return m_showPath; }
      set {
        m_showPath = value;
        this.pathLbl.Visible = m_showPath;
        this.pathTextBox.Visible = m_showPath;
      }
    }

    /// <summary>
    /// DisplayMode getter
    /// </summary>
    public DisplayMode SelectedDisplayMode
    {
      get { return m_displayMode; }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public SequenceControl (DisplayMode displayMode)
    {
      this.m_displayMode = displayMode;
      InitializeComponent ();

      nameLbl.Text = PulseCatalog.GetString ("Name");
      estimatedtimeLbl.Text = PulseCatalog.GetString ("EstimatedTime");
      descriptionLbl.Text = PulseCatalog.GetString ("Description");
      cadModelLbl.Text = PulseCatalog.GetString ("CADModel");
      toolLbl.Text = PulseCatalog.GetString ("Tool");
      operationLbl.Text = PulseCatalog.GetString ("Operation");
      nameTextBox.Clear ();
      estimatedTimeTimeSpanPicker.Value = null;
      descriptionTextBox.Clear ();
      cadModelTextBox.Clear ();
      toolTextBox.Clear ();
      toolNumberTextBox.Clear ();
      operationTextBox.Clear ();

      this.kindComboBox.Items.AddRange (SequenceKind.GetNames (typeof (SequenceKind)));

      this.m_sequence = null;

      switch (displayMode) {
        case DisplayMode.VIEW:
          orderLabel.Visible = true;
          orderTextBox.Visible = true;
          saveBtn.Text = PulseCatalog.GetString ("Save");
          resetBtn.Text = PulseCatalog.GetString ("Reset");
          saveBtn.Enabled = false;
          resetBtn.Enabled = true;
          createBtn.Visible = false;
          cancelBtn.Visible = false;

          stampingValueLbl.Visible = true;
          dataGridView1.Visible = true;
          m_dataTable = new DataTable ();
          this.DataTable.Columns.Add ("FieldDisplay", typeof (string));
          this.DataTable.Columns.Add ("StampingValue", typeof (string));
          this.DataTable.Columns.Add ("StampingValueId", typeof (int));
          this.DataTable.Columns.Add ("FieldType", typeof (FieldType));
          dataGridView1.AutoGenerateColumns = false;
          DataGridViewComboBoxColumn comboboxcolumn = new DataGridViewComboBoxColumn ();
          dataGridView1.Columns[0].DataPropertyName = "FieldDisplay";
          dataGridView1.Columns[0].Name = "FieldDisplay";
          dataGridView1.Columns[1].DataPropertyName = "StampingValue";
          dataGridView1.Columns[1].Name = "StampingValue";
          dataGridView1.Columns[2].DataPropertyName = "StampingValueId";
          dataGridView1.Columns[2].Name = "StampingValueId";
          dataGridView1.Columns[3].DataPropertyName = "FieldType";
          dataGridView1.Columns[3].Name = "FieldType";
          dataGridView1.DataSource = this.DataTable;

          stampingDetailsLabel.Visible = true;
          stampingDetailsDataGridView.Visible = true;
          m_stampingDetailsDataTable = new DataTable ();
          StampingDetailsDataTable.Columns.Add ("IsoFileName", typeof (string));
          StampingDetailsDataTable.Columns.Add ("StampPosition", typeof (int));
          StampingDetailsDataTable.Columns.Add ("IsCycleBegin", typeof (bool));
          StampingDetailsDataTable.Columns.Add ("SourceDirectory", typeof (string));
          StampingDetailsDataTable.Columns.Add ("TargetDirectory", typeof (string));
          StampingDetailsDataTable.Columns.Add ("StampingDateTime", typeof (DateTime));
          stampingDetailsDataGridView.AutoGenerateColumns = false;
          stampingDetailsDataGridView.Columns[0].DataPropertyName = "IsoFileName";
          stampingDetailsDataGridView.Columns[0].Name = "IsoFileName";
          stampingDetailsDataGridView.Columns[1].DataPropertyName = "StampPosition";
          stampingDetailsDataGridView.Columns[1].Name = "StampPosition";
          stampingDetailsDataGridView.Columns[2].DataPropertyName = "IsCycleBegin";
          stampingDetailsDataGridView.Columns[2].Name = "IsCycleBegin";
          stampingDetailsDataGridView.Columns[3].DataPropertyName = "SourceDirectory";
          stampingDetailsDataGridView.Columns[3].Name = "SourceDirectory";
          stampingDetailsDataGridView.Columns[4].DataPropertyName = "TargetDirectory";
          stampingDetailsDataGridView.Columns[4].Name = "TargetDirectory";
          stampingDetailsDataGridView.Columns[5].DataPropertyName = "StampingDateTime";
          stampingDetailsDataGridView.Columns[5].Name = "StampingDateTime";
          stampingDetailsDataGridView.DataSource = this.StampingDetailsDataTable;

          break;

        case DisplayMode.CREATE:
          int gridViewsTotalHeight = stampingDetailsDataGridView.Height + dataGridView1.Height;
          orderLabel.Visible = false;
          orderTextBox.Visible = false;
          cancelBtn.Text = PulseCatalog.GetString ("Cancel");
          cancelBtn.Visible = true;
          cancelBtn.Location = new System.Drawing.Point (cancelBtn.Location.X, cancelBtn.Location.Y - gridViewsTotalHeight);

          createBtn.Text = PulseCatalog.GetString ("Create");
          createBtn.Visible = true;
          createBtn.Location = new System.Drawing.Point (createBtn.Location.X, createBtn.Location.Y - gridViewsTotalHeight);

          saveBtn.Visible = false;
          resetBtn.Visible = false;

          stampingValueLbl.Visible = false;
          dataGridView1.Visible = false;
          stampingDetailsLabel.Visible = false;
          stampingDetailsDataGridView.Visible = false;

          this.Size = new System.Drawing.Size (this.Size.Width, this.Size.Height - gridViewsTotalHeight);
          break;
      }


    }

    /// <summary>
    ///   Default constructor without argument
    /// </summary>
    public SequenceControl () : this (DisplayMode.VIEW)
    { }
    #endregion // Constructors

    #region inherited Methods

    /// <summary>
    /// Update state of this observer. In this case, SequenceControl
    /// will hide or not according type of selected node  in ITreeViewObservable
    /// </summary>
    /// <param name="observable"></param>
    public void UpdateInfo (ITreeViewObservable observable)
    {
      this.m_observable = observable;
      TreeNode selectedNode = observable.GetSelectedNode ();
      if (selectedNode != null) {
        if (selectedNode.Tag is Tuple<bool, ISequence>) {
          m_node = selectedNode;
          try {
            observable.ReloadTreeNodes (m_node);
          }
          catch (Exception ex) {
            log.Error ($"UpdateInfo: ReloadTreeNodes for sequence={m_node.Tag}", ex);
          }
          LoadData (((Tuple<bool, ISequence>)m_node.Tag).Item2);
        }
      }
    }
    #endregion


    #region Methods

    /// <summary>
    /// Use informations in Sequence to fill text in box
    /// </summary>
    public void LoadData (ISequence sequence)
    {
      if (sequence != null) {
        nameTextBox.Text = sequence.Name;

        estimatedTimeTimeSpanPicker.Value = sequence.EstimatedTime;
        descriptionTextBox.Text = sequence.Description;

        if (!string.IsNullOrEmpty (sequence.ToolNumber)) {
          toolNumberTextBox.Text = sequence.ToolNumber;
        }
        else {
          toolNumberTextBox.Text = "";
        }

        if (sequence.OperationStep.HasValue) {
          operationStepTextBox.Text = sequence.OperationStep.Value.ToString ();
        }
        else {
          operationStepTextBox.Text = "";
        }

        TreeNode opNode = null;
        if (m_node.Parent.Tag is Tuple<bool, IPath>) {
          this.ShowPath = true;
          opNode = m_node.Parent.Parent;
        }
        else {
          opNode = m_node.Parent;
          this.ShowPath = false;
        }

        if (opNode.Tag is Tuple<bool, IOperation>) {
          ParentType = typeof (IOperation);
        }
        else {
          ParentType = typeof (ISimpleOperation);
        }

        IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
        using (IDAOSession daoSession = daoFactory.OpenSession ()) {
          if (ModelDAOHelper.DAOFactory.IsInitialized (sequence)) {
            ModelDAOHelper.DAOFactory.SequenceDAO.Lock (sequence);
          }
          else { 
            ModelDAOHelper.DAOFactory.Initialize (sequence);
          }
          this.m_sequence = sequence;
          NHibernate.NHibernateUtil.Initialize (this.m_sequence.CadModel);
          NHibernate.NHibernateUtil.Initialize (this.m_sequence.Tool);
          NHibernate.NHibernateUtil.Initialize (this.m_sequence.Operation);
          NHibernate.NHibernateUtil.Initialize (this.m_sequence.StampingValues);
          NHibernate.NHibernateUtil.Initialize (this.m_sequence.Detail);

          if (m_sequence.CadModel != null) {
            cadModelTextBox.Text = m_sequence.CadModel.Name;
            cadModelTextBox.Tag = m_sequence.CadModel.Id;
          }
          else {
            cadModelTextBox.Text = "";
            cadModelTextBox.Tag = null;
          }

          if (m_sequence.Tool != null) {
            toolTextBox.Text = m_sequence.Tool.Name;
            toolTextBox.Tag = m_sequence.Tool.Id;
          }
          else {
            toolTextBox.Text = "";
            toolTextBox.Tag = null;
          }

          operationTextBox.Text = m_sequence.Operation.Name;
          operationTextBox.Tag = ((Lemoine.Collections.IDataWithId)m_sequence.Operation).Id;
          orderTextBox.Text = m_sequence.Order.ToString ();
          pathTextBox.Text = m_sequence.Path.Number.ToString ();
          pathTextBox.Tag = ((Lemoine.Collections.IDataWithId)m_sequence.Path).Id;

          kindComboBox.SelectedIndex =
            kindComboBox.Items.IndexOf (SequenceKind.GetName (typeof (SequenceKind), m_sequence.Kind));

          this.DataTable.Clear ();
          foreach (IStampingValue stampingValue in m_sequence.StampingValues) {
            switch (stampingValue.Field.Type) {
              case FieldType.String:
                this.DataTable.Rows.Add (stampingValue.Field.Display, stampingValue.String, ((Lemoine.Collections.IDataWithId<int>)stampingValue).Id, stampingValue.Field.Type);
                break;
              case FieldType.Int32:
                this.DataTable.Rows.Add (stampingValue.Field.Display, stampingValue.Int.ToString (), ((Lemoine.Collections.IDataWithId<int>)stampingValue).Id, stampingValue.Field.Type);
                break;
              case FieldType.Double:
                this.DataTable.Rows.Add (stampingValue.Field.Display, stampingValue.Double.ToString (), ((Lemoine.Collections.IDataWithId<int>)stampingValue).Id, stampingValue.Field.Type);
                break;
              case FieldType.Boolean:
                throw new Exception ("Invalid value for FieldType");
              default:
                throw new Exception ("Invalid value for FieldType");
            }
          }


          this.StampingDetailsDataTable.Clear ();
          IList<IStamp> associatedStamps = ModelDAOHelper.DAOFactory.StampDAO.FindAllWithSequence (m_sequence);
          foreach (IStamp stamp in associatedStamps) {
            IIsoFile isoFile = stamp.IsoFile;
            if (null != isoFile) {
              this.StampingDetailsDataTable.Rows.Add (isoFile.Name, stamp.Position, stamp.OperationCycleBegin, isoFile.SourceDirectory, isoFile.StampingDirectory, isoFile.StampingDateTime);
            }
          }

        }
        resetBtn.Enabled = true;
        saveBtn.Enabled = false;
      }
    }


    /// <summary>
    ///   Test if a change on field occurs and then enable or disable
    ///   save button
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    void NameTextBoxTextChanged (object sender, EventArgs e)
    {
      if ((m_sequence != null) && (this.SelectedDisplayMode == DisplayMode.VIEW)) {
        if (HasBeenChanged ()) {
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
    bool HasBeenChanged ()
    {
      String s;
      s = (m_sequence.Name == null) ? "" : m_sequence.Name;
      if (!nameTextBox.Text.Equals (s)) {
        return true;
      }

      if (((string)kindComboBox.SelectedItem) != SequenceKind.GetName (typeof (SequenceKind), m_sequence.Kind)) {
        return true;
      }

      if (estimatedTimeTimeSpanPicker.Value == null) {
        if (m_sequence.EstimatedTime.HasValue) {
          return true;
        }
      }
      else {
        if (!m_sequence.EstimatedTime.HasValue ||
            ((!m_sequence.EstimatedTime.Value.Equals (estimatedTimeTimeSpanPicker.Value.Value)))) {
          return true;
        }
      }

      s = (m_sequence.Description == null) ? "" : m_sequence.Description;
      if (!descriptionTextBox.Text.Equals (s)) {
        return true;
      }

      if (m_sequence.CadModel == null) {
        if (cadModelTextBox.Tag != null) {
          return true;
        }
      }
      else {
        if (cadModelTextBox.Tag == null) {
          return true;
        }
        else if (m_sequence.CadModel.Id != (int)cadModelTextBox.Tag) {
          return true;
        }
      }

      if (m_sequence.Tool == null) {
        if (toolTextBox.Tag != null) {
          return true;
        }
      }
      else {
        if (toolTextBox.Tag == null) {
          return true;
        }
        else if (m_sequence.Tool.Id != (int)toolTextBox.Tag) {
          return true;
        }
      }

      if (!string.Equals ((m_sequence.ToolNumber == null) ? "" : m_sequence.ToolNumber,
        toolNumberTextBox.Text.Trim (),
        StringComparison.InvariantCultureIgnoreCase)) {
        return true;
      }

      if (!string.Equals (m_sequence.OperationStep.HasValue ? m_sequence.OperationStep.Value.ToString () : "",
        operationStepTextBox.Text, StringComparison.InvariantCultureIgnoreCase)) {
        return true;
      }

      return false;
    }


    /// <summary>
    ///   Tell whether input values are corrects. If not a messagebox
    ///   is displayed to tell incorrects fields.
    /// </summary>
    /// <returns>true if all fields have corrects input, false otherwise.</returns>
    bool ValidateData ()
    {
      return true; // estimated time uses timespan picker (correct by construction)
    }

    #endregion // Methods


    #region button click Methods

    /// <summary>
    /// Reset values displayed in information panel
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    void ResetBtnClick (object sender, EventArgs e)
    {
      if (m_sequence != null) {
        nameTextBox.Text = m_sequence.Name;
        estimatedTimeTimeSpanPicker.Value = m_sequence.EstimatedTime;
        descriptionTextBox.Text = m_sequence.Description;

        if (string.IsNullOrEmpty (m_sequence.ToolNumber)) {
          toolNumberTextBox.Text = "";
        }
        else {
          toolNumberTextBox.Text = m_sequence.ToolNumber;
        }

        IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
        using (IDAOSession daoSession = daoFactory.OpenSession ()) {
          ModelDAOHelper.DAOFactory.SequenceDAO.Lock (m_sequence);
          NHibernate.NHibernateUtil.Initialize (this.m_sequence.CadModel);
          NHibernate.NHibernateUtil.Initialize (this.m_sequence.Tool);
          NHibernate.NHibernateUtil.Initialize (this.m_sequence.Operation);
          NHibernate.NHibernateUtil.Initialize (this.m_sequence.StampingValues);
          NHibernate.NHibernateUtil.Initialize (this.m_sequence.Detail);

          if (m_sequence.CadModel != null) {
            cadModelTextBox.Text = m_sequence.CadModel.Name;
            cadModelTextBox.Tag = m_sequence.CadModel.Id;
          }
          else {
            cadModelTextBox.Text = "";
            cadModelTextBox.Tag = null;
          }

          if (m_sequence.Tool != null) {
            toolTextBox.Text = m_sequence.Tool.Name;
            toolTextBox.Tag = m_sequence.Tool.Id;
          }
          else {
            toolTextBox.Text = "";
            toolTextBox.Tag = null;
          }

          kindComboBox.SelectedIndex =
            kindComboBox.Items.IndexOf (SequenceKind.GetName (typeof (SequenceKind), m_sequence.Kind));

          operationTextBox.Text = m_sequence.Operation.Name;
          operationTextBox.Tag = ((Lemoine.Collections.IDataWithId)m_sequence.Operation).Id;
        }
      }
      saveBtn.Enabled = false;
    }


    /// <summary>
    /// C
    /// lose creation window
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    void CancelBtnClick (object sender, EventArgs e)
    {
      this.FindForm ().Close ();
    }

    /// <summary>
    /// Save modification of Sequence properties
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    void SaveBtnClick (object sender, EventArgs e)
    {
      if (!ValidateData ()) { //If entered values are not correct
        return;
      }
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      using (IDAOSession daoSession = daoFactory.OpenSession ()) {
        using (IDAOTransaction transaction = daoSession.BeginTransaction ()) {
          ModelDAOHelper.DAOFactory.SequenceDAO.Lock (m_sequence);
          ISequence sequence = m_sequence;
          sequence.Name = nameTextBox.Text;

          sequence.Kind = (SequenceKind)Enum.Parse (typeof (SequenceKind), (string)kindComboBox.SelectedItem);

          sequence.EstimatedTime = estimatedTimeTimeSpanPicker.Value;

          sequence.Description = descriptionTextBox.Text;

          if (cadModelTextBox.Tag == null) {
            sequence.CadModel = null;
          }
          else {
            ICadModel cadModel = daoFactory.CadModelDAO.FindById ((int)cadModelTextBox.Tag);
            sequence.CadModel = cadModel;
          }

          if (toolTextBox.Tag == null) {
            sequence.Tool = null;
          }
          else {
            ITool tool = daoFactory.ToolDAO.FindById ((int)toolTextBox.Tag);
            sequence.Tool = tool;
          }

          sequence.ToolNumber = toolNumberTextBox.Text.Trim ();

          if (string.IsNullOrEmpty (operationStepTextBox.Text)) {
            sequence.OperationStep = null;
          }
          else {
            int operationStep;
            if (int.TryParse (operationStepTextBox.Text, out operationStep)) {
              sequence.OperationStep = operationStep;
            }
            else {
              log.ErrorFormat ("Invalid operation step");
              sequence.OperationStep = null;
            }
          }

          transaction.Commit ();
        }
      }

      if (!Lemoine.WebClient.Request.ClearDomain ("operationinformationchange")) {
        log.ErrorFormat ("SaveBtnClick: clear domain failed");
      }

      m_observable.ReloadTreeNodes (m_node);
      m_node.TreeView.SelectedNode = m_node;
      m_node.TreeView.Focus ();
      m_observable.NotifyObservers ();
      saveBtn.Enabled = false;
    }

    /// <summary>
    /// Create new Sequence
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    void CreateBtnClick (object sender, EventArgs e)
    {
      //if values entered is not correct, exit
      if (!ValidateData ()) {
        return;
      }

      CreateItemForm createItemForm = this.FindForm () as CreateItemForm;

      ISequence sequence = ModelDAOHelper.ModelFactory.CreateSequence (nameTextBox.Text);
      sequence.Description = descriptionTextBox.Text;

      sequence.Kind = (SequenceKind)Enum.Parse (typeof (SequenceKind), (string)kindComboBox.SelectedItem);

      sequence.EstimatedTime = estimatedTimeTimeSpanPicker.Value;

      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      TreeNode parentNode = createItemForm.OperationTreeView.TreeView.SelectedNode;

      IPath path = this.Path;
      using (IDAOSession daoSession = daoFactory.OpenSession ()) {

        int maxOrder = 0;
        if (path == null) {
          // create path in parent
          path = ModelDAOHelper.ModelFactory.CreatePath ();
          path.Number = 1;
          path.Operation = this.Operation;
        }
        else {
          foreach (ISequence sequenceInPath in path.Sequences) {
            if (maxOrder <= sequenceInPath.Order) {
              maxOrder = sequenceInPath.Order + 1;
            }
          }
        }

        sequence.Order = maxOrder;
        sequence.Path = path;

        if (!string.IsNullOrEmpty (operationStepTextBox.Text)) {
          int operationStep;
          if (int.TryParse (operationStepTextBox.Text, out operationStep)) {
            sequence.OperationStep = operationStep;
          }
        }

        if (cadModelTextBox.Tag != null) {
          ICadModel cadModel = daoFactory.CadModelDAO.FindById ((int)cadModelTextBox.Tag);
          sequence.CadModel = cadModel;
        }

        if (toolTextBox.Tag != null) {
          ITool tool = daoFactory.ToolDAO.FindById ((int)toolTextBox.Tag);
          sequence.Tool = tool;
        }

        if (!string.IsNullOrEmpty (toolNumberTextBox.Text)) {
          sequence.ToolNumber = toolNumberTextBox.Text;
        }

        using (IDAOTransaction transaction = daoSession.BeginTransaction ()) {
          path.Sequences.Add (sequence);
          daoFactory.PathDAO.MakePersistent (path);
          daoFactory.SequenceDAO.MakePersistent (sequence);
          transaction.Commit ();
        }

        if (!Lemoine.WebClient.Request.ClearDomain ("operationinformationchange")) {
          log.ErrorFormat ("SaveBtnClick: clear domain failed");
        }

        this.m_sequence = sequence;
        this.Path = path;

        TreeNode treeNode = new TreeNode (sequence.Display, (int)TreeViewImageIndex.Sequence, (int)TreeViewImageIndex.Sequence);
        treeNode.Name = ((Lemoine.Collections.IDataWithId)sequence).Id.ToString ();
        treeNode.Tag = new Tuple<bool, ISequence> (false, sequence);
        parentNode.Nodes.Add (treeNode);
        createItemForm.OperationTreeView.BuildTreeNodes (parentNode);
        createItemForm.OperationTreeView.TreeView.Focus ();
        createItemForm.OperationTreeView.TreeView.SelectedNode = treeNode;
        createItemForm.OperationTreeView.NotifyObservers ();
      }

      this.FindForm ().Close ();

    }

    void ToolTextBoxDoubleClick (object sender, EventArgs e)
    {
      if (m_toolDialog == null) {
        m_toolDialog = new ToolDialog ();
        m_toolDialog.Nullable = true;
        m_toolDialog.DisplayedProperty = "Display";
      }

      m_toolDialog.ShowDialog ();
      if (m_toolDialog.DialogResult == DialogResult.OK) {
        if (m_toolDialog.SelectedValue == null) {
          toolTextBox.Tag = null;
          toolTextBox.Text = "";
        }
        else {
          ITool tool = m_toolDialog.SelectedValue;
          toolTextBox.Tag = tool.Id;
          toolTextBox.Text = tool.Name;
        }
      }
    }


    void CadModelTextBoxDoubleClick (object sender, EventArgs e)
    {
      if (m_cadModelDialog == null) {
        m_cadModelDialog = new CadModelDialog ();
        m_cadModelDialog.Nullable = true;
        m_cadModelDialog.DisplayedProperty = "Name";
      }

      m_cadModelDialog.ShowDialog ();
      if (m_cadModelDialog.DialogResult == DialogResult.OK) {
        if (m_cadModelDialog.SelectedValue == null) {
          cadModelTextBox.Tag = null;
          cadModelTextBox.Text = "";
        }
        else {
          ICadModel cadModel = m_cadModelDialog.SelectedValue;
          cadModelTextBox.Tag = cadModel.Id;
          cadModelTextBox.Text = cadModel.Name;
        }
      }
    }

    #endregion


    #region Methods about DataGridView for StampingValue
    void DataGridView1CellEndEdit (object sender, DataGridViewCellEventArgs e)
    {

      if (e.RowIndex == dataGridView1.RowCount - 1) {
        return;
      }

      DataGridViewRow row = dataGridView1.CurrentRow;
      String stampingvaluestring = row.Cells["StampingValue"].Value.ToString ();
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      switch ((FieldType)row.Cells["FieldType"].Value) {
        case FieldType.String:
          using (IDAOSession daoSession = daoFactory.OpenSession ()) {
            IStampingValue stampingvalue = daoFactory.StampingValueDAO.FindById (Convert.ToInt32 (row.Cells["StampingValueId"].Value));
            stampingvalue.String = stampingvaluestring;
            using (IDAOTransaction transaction = daoSession.BeginTransaction ()) {
              transaction.Commit ();
            }
          }
          break;
        case FieldType.Int32:
          try {
            Int32 stampingvalueint = Convert.ToInt32 (stampingvaluestring);
            using (IDAOSession daoSession = daoFactory.OpenSession ()) {
              IStampingValue stampingvalue = daoFactory.StampingValueDAO.FindById (Convert.ToInt32 (row.Cells["StampingValueId"].Value));
              stampingvalue.Int = stampingvalueint;
              using (IDAOTransaction transaction = daoSession.BeginTransaction ()) {
                transaction.Commit ();
              }
            }
          }
          catch (Exception) {
            MessageBox.Show ("Incorrect value entered.\nYou must typed an integer value.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
          }

          break;
        case FieldType.Double:
          try {
            Double stampingvaluedouble = Convert.ToDouble (stampingvaluestring);
            using (IDAOSession daoSession = daoFactory.OpenSession ()) {
              IStampingValue stampingvalue = daoFactory.StampingValueDAO.FindById (Convert.ToInt32 (row.Cells["StampingValueId"].Value));
              stampingvalue.Double = stampingvaluedouble;
              using (IDAOTransaction transaction = daoSession.BeginTransaction ()) {
                transaction.Commit ();
              }
            }
          }
          catch (Exception) {
            MessageBox.Show ("Incorrect value entered.\nYou must typed an real value.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
          }
          break;
        case FieldType.Boolean:
          throw new Exception ("Invalid value for FieldType");
        default:
          throw new Exception ("Invalid value for FieldType");
      }
    }

    void DataGridView1CellClick (object sender, DataGridViewCellEventArgs e)
    {
      if (e.RowIndex == (dataGridView1.RowCount - 1)) {
        if (m_fieldDialog == null) {
          m_fieldDialog = new FieldDialog ();
          m_fieldDialog.Nullable = false;
          m_fieldDialog.DisplayedProperty = "Display";
        }

        m_fieldDialog.ShowDialog ();
        if (m_fieldDialog.DialogResult == DialogResult.OK) {
          if (m_fieldDialog.SelectedValue == null) {

          }
          else {
            IField field = m_fieldDialog.SelectedValue;
            IStampingValue stampingvalue;
            String value;

            IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
            using (IDAOSession daoSession = daoFactory.OpenSession ()) {
              stampingvalue = ModelDAOHelper.ModelFactory.CreateStampingValue (this.Sequence, field);
              switch (field.Type) {
                case FieldType.String:
                  value = "";
                  stampingvalue.String = value;
                  break;
                case FieldType.Int32:
                  value = "0";
                  stampingvalue.Int = 0;
                  break;
                case FieldType.Double:
                  value = "0";
                  stampingvalue.Double = 0;
                  break;
                case FieldType.Boolean:
                default:
                  throw new Exception ("Invalid value for FieldType");
              }
              daoFactory.StampingValueDAO.MakePersistent (stampingvalue);
              using (IDAOTransaction transaction = daoSession.BeginTransaction ()) {
                transaction.Commit ();
              }
            }
            this.DataTable.Rows.Add (field.Display, value, ((Lemoine.Collections.IDataWithId)stampingvalue).Id, field.Type);
            dataGridView1.Rows.RemoveAt (dataGridView1.RowCount - 2);
            DataGridViewCell cell = dataGridView1.Rows[dataGridView1.RowCount - 2].Cells[1];
            dataGridView1.CurrentCell = cell;
          }
        }
      }
    }

    void DataGridView1CellBeginEdit (object sender, DataGridViewCellCancelEventArgs e)
    {
      if (e.RowIndex == dataGridView1.RowCount - 1) {
        e.Cancel = true;
      }
    }

    void DataGridView1UserDeletingRow (object sender, DataGridViewRowCancelEventArgs e)
    {
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      using (IDAOSession daoSession = daoFactory.OpenSession ()) {
        IStampingValue stampingvalue = daoFactory.StampingValueDAO.FindById ((int)dataGridView1.Rows[e.Row.Index].Cells[2].Value);
        daoFactory.StampingValueDAO.MakeTransient (stampingvalue);
        using (IDAOTransaction transaction = daoSession.BeginTransaction ()) {
          transaction.Commit ();
        }
      }
    }
    #endregion //Methods about DataGridView for StampingValue

    void KindComboBoxSelectedIndexChanged (object sender, EventArgs e)
    {
      CheckHasChanged ();
    }

    private void toolNumberTextBox_TextChanged (object sender, EventArgs e)
    {
      CheckHasChanged ();
    }

    void CheckHasChanged ()
    {
      if ((m_sequence != null) && (this.SelectedDisplayMode == DisplayMode.VIEW)) {
        if (HasBeenChanged ()) {
          saveBtn.Enabled = true;
        }
        else {
          saveBtn.Enabled = false;
        }
      }
    }

    private void operationStepTextBox_TextChanged (object sender, EventArgs e)
    {
      CheckHasChanged ();
    }
  }
}
