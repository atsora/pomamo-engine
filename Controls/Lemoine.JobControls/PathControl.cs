// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Windows.Forms;
using Lemoine.I18N;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Collections;

using Lemoine.Core.Log;

namespace Lemoine.JobControls
{
  /// <summary>
  /// Panel with fields to manage properties of a Path.
  /// It enable a user to read, modify properties of a Path
  /// or to create a Path with basic information
  /// </summary>
  public partial class PathControl : UserControl, ITreeViewObserver
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
    IPath m_path;
    DisplayMode m_displayMode;
    TreeNode m_node;
    IOperation m_operation;
    ITreeViewObservable m_observable;
    Type m_parentType;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger (typeof (PathControl).FullName);

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
    /// Getter / Setter for Path which binds with this control
    /// </summary>
    public IPath Path
    {
      get { return m_path; }
      set { m_path = value; }
    }

    /// <summary>
    /// Getter / Setter for Operation related to this Path
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

    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public PathControl (DisplayMode displayMode)
    {
      //
      // The InitializeComponent() call is required for Windows Forms designer support.
      //
      InitializeComponent ();

      numberLbl.Text = PulseCatalog.GetString ("Number");
      operationLbl.Text = PulseCatalog.GetString ("Operation");

      numberTextBox.Clear ();
      operationTextBox.Clear ();

      this.m_path = null;
      this.m_displayMode = displayMode;
      switch (displayMode) {
        case DisplayMode.VIEW:
          saveBtn.Text = PulseCatalog.GetString ("Save");
          resetBtn.Text = PulseCatalog.GetString ("Reset");
          saveBtn.Enabled = false;
          resetBtn.Enabled = false;
          saveBtn.Visible = true;
          resetBtn.Visible = true;
          createBtn.Visible = false;
          cancelBtn.Visible = false;
          numberTextBox.ReadOnly = false;
          break;
        case DisplayMode.CREATE:
          saveBtn.Visible = false;
          resetBtn.Visible = false;
          numberTextBox.ReadOnly = false;
          cancelBtn.Text = PulseCatalog.GetString ("Cancel");
          cancelBtn.Visible = true;
          //cancelBtn.Location = new Point(cancelBtn.Location.X,210);
          createBtn.Text = PulseCatalog.GetString ("Create");
          createBtn.Visible = true;
          //createBtn.Location = new Point(createBtn.Location.X,210);
          break;
      }
    }

    /// <summary>
    /// default constructor
    /// </summary>
    public PathControl () : this (DisplayMode.VIEW)
    { }

    #endregion // Constructors

    #region inherited Methods
    /// <summary>
    /// Update state of this observer. In this case, PathControl
    /// will hide or not according type of selected node  in ITreeViewObservable
    /// </summary>
    /// <param name="observable"></param>
    public void UpdateInfo (ITreeViewObservable observable)
    {
      this.m_observable = observable;
      TreeNode selectedNode = observable.GetSelectedNode ();
      if (selectedNode != null) {
        if (selectedNode.Tag is Tuple<bool, IPath>) {
          m_node = selectedNode;
          observable.ReloadTreeNodes (m_node);
          LoadData (((Tuple<bool, IPath>)m_node.Tag).Item2);
        }
      }
    }
    #endregion

    #region Methods
    /// <summary>
    /// Use informations in Path to fill text in box
    /// </summary>
    public void LoadData (IPath path)
    {
      if (path != null) {
        numberTextBox.Text = path.Number.ToString ();

        if (m_node.Parent.Tag is Tuple<bool, IOperation>) {
          ParentType = typeof (IOperation);
        }
        else if (m_node.Parent.Tag is Tuple<bool, ISimpleOperation>) {
          ParentType = typeof (ISimpleOperation);
        }

        IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
        using (IDAOSession daoSession = daoFactory.OpenSession ()) {
          this.m_path = path;
          ModelDAOHelper.DAOFactory.PathDAO.Lock (path);
          NHibernate.NHibernateUtil.Initialize (this.m_path.Operation);

          operationTextBox.Text = m_path.Operation.Name;
          operationTextBox.Tag = ((Lemoine.Collections.IDataWithId)m_path.Operation).Id;
        }
        resetBtn.Enabled = false;
        saveBtn.Enabled = false;
      }
    }
    #endregion // Methods

    #region button click Methods

    /// <summary>
    /// Reset values displayed in information panel
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    void ResetBtnClick (object sender, System.EventArgs e)
    {
      if (Path != null) {
        numberTextBox.Text = Path.Number.ToString ();
        // operationTextBox.Text = Path.Operation.Name;
      }
      saveBtn.Enabled = false;
    }

    /// <summary>
    /// Create new Path
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    void CreateBtnClick (object sender, System.EventArgs e)
    {
      //if values entered is not correct, exit
      if (!ValidateData ()) {
        return;
      }

      CreateItemForm createItemForm = this.FindForm () as CreateItemForm;

      IPath path = ModelDAOHelper.ModelFactory.CreatePath ();
      path.Number = Int32.Parse (numberTextBox.Text);

      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      TreeNode parentNode = createItemForm.OperationTreeView.TreeView.SelectedNode;

      using (IDAOSession daoSession = daoFactory.OpenSession ()) {
        IOperation operation = ModelDAOHelper.DAOFactory.OperationDAO.FindById ((int)operationTextBox.Tag);
        path.Operation = operation;

        using (IDAOTransaction transaction = daoSession.BeginTransaction ()) {
          daoFactory.PathDAO.MakePersistent (path);
          daoFactory.OperationDAO.MakePersistent (operation);
          transaction.Commit ();
        }

        this.Path = path;

        TreeNode treeNode = new TreeNode (path.Display, (int)TreeViewImageIndex.Path, (int)TreeViewImageIndex.Path);
        treeNode.Name = ((Lemoine.Collections.IDataWithId)path).Id.ToString ();
        treeNode.Tag = new Tuple<bool, IPath> (false, path);
        parentNode.Nodes.Add (treeNode);
        createItemForm.OperationTreeView.BuildTreeNodes (parentNode);
        createItemForm.OperationTreeView.TreeView.Focus ();
        createItemForm.OperationTreeView.TreeView.SelectedNode = treeNode;
        createItemForm.OperationTreeView.NotifyObservers ();
      }

      this.FindForm ().Close ();
    }

    /// <summary>
    /// Close creation window
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    void CancelBtnClick (object sender, System.EventArgs e)
    {
      this.FindForm ().Close ();
    }

    /// <summary>
    /// Save modification of Path properties
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    void SaveBtnClick (object sender, System.EventArgs e)
    {
      if (!ValidateData ()) { // If entered values are not correct
        return;
      }
      IPath path = this.Path;
      int pathNumber = Int32.Parse (numberTextBox.Text);
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      using (IDAOSession daoSession = daoFactory.OpenSession ()) {
        ModelDAOHelper.DAOFactory.PathDAO.Lock (path);

        using (IDAOTransaction transaction = daoSession.BeginTransaction ()) {
          path.Number = pathNumber;
          transaction.Commit ();
        }
      }

      m_observable.ReloadTreeNodes (m_node);
      m_observable.NotifyObservers ();
      saveBtn.Enabled = false;
    }

    /// <summary>
    ///   Tell whether input values are corrects. If not a messagebox
    ///   is displayed to tell incorrects fields.
    /// </summary>
    /// <returns>true if all fields have corrects input, false otherwise.</returns>
    bool ValidateData ()
    {
      String msg = PulseCatalog.GetString ("FollowingFieldsHaveIncorrectValues");
      bool valid = true;

      int pathNumber;
      if ((!Int32.TryParse (numberTextBox.Text.Trim (), out pathNumber)) || (pathNumber < 0)) {
        msg = msg + "\n\t" + numberLbl.Text;
        valid = false;
      }
      else {
        IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
        using (IDAOSession daoSession = daoFactory.OpenSession ()) {

          IOperation operation = (this.Path == null) ? this.Operation : this.Path.Operation;
          IPath numberedPath =
            ModelDAOHelper.DAOFactory.PathDAO
            .FindByOperationAndNumber (operation,
                                      pathNumber);
          if (numberedPath != null) {
            msg = PulseCatalog.GetString ("PathSameNumber");
            valid = false;
          }
        }
      }
      if (!valid) {
        MessageBox.Show (msg, "", MessageBoxButtons.OK, MessageBoxIcon.Error);
      }

      return valid;
    }

    /// <summary>
    /// Test if a change on field occurs and then enable or disable
    /// save button
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    void NameTextBoxTextChanged (object sender, EventArgs e)
    {
      if ((this.Path != null) && (m_displayMode == DisplayMode.VIEW)) {
        if (HasBeenChanged ()) {
          saveBtn.Enabled = true;
          resetBtn.Enabled = true;
        }
        else {
          saveBtn.Enabled = false;
          resetBtn.Enabled = false;
        }
      }
    }

    /// <summary>
    ///   Get whether a field has been changed
    /// </summary>
    /// <returns>True if a fields has been changed, false otherwise</returns>
    bool HasBeenChanged ()
    {
      if (!numberTextBox.Text.Equals (this.Path.Number.ToString ())) {
        return true;
      }

      return false;
    }

    #endregion // button click Methods

  }
}
