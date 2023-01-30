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
  /// Panel with fields to manage properties of a Component.
  /// It enable a user to read, modify properties of a Component
  /// or to create a Component with basic information
  /// </summary>
  public partial class ComponentControl : UserControl, ITreeViewObserver
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
    Lemoine.Model.IComponent m_component;
    IComponentType[] m_componentTypeArray;
    IProject[] m_projectArray;
    DisplayMode m_displayMode;
    TreeNode m_node;
    ITreeViewObservable m_observable;
    Type m_parentType;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (ComponentControl).FullName);

    #region Getters / Setters
    /// <summary>
    /// Getter / Setter for type of parent of Component node
    /// </summary>
    public Type ParentType {
      get { return m_parentType;}
      set {
        m_parentType = value;
        if (m_parentType != null) {
          if (m_parentType.Equals(typeof(IProject))) {
            projectLbl.Text = PulseCatalog.GetString("Project");
          }
          else if (m_parentType.Equals(typeof(IJob))) {
            projectLbl.Text = PulseCatalog.GetString("Job");
          }
        }
      }
    }
    #endregion // Getters / Setters

    
    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public ComponentControl(DisplayMode displayMode)
    {
      //
      // The InitializeComponent() call is required for Windows Forms designer support.
      //
      InitializeComponent();
      this.m_component = null;
      nameLbl.Text = PulseCatalog.GetString("Name");
      codeLbl.Text = PulseCatalog.GetString("Code");
      documentLinkLbl.Text = PulseCatalog.GetString("DocumentLink");
      estimatedHoursLbl.Text = PulseCatalog.GetString("EstimatedHours");
      typeLbl.Text = PulseCatalog.GetString("Type");
      nameTextBox.Clear();
      codeTextBox.Clear();
      documentLinkTextBox.Clear();
      estimatedHoursTextBox.Clear();

      typeComboBox.Items.Clear();
      projectComboBox.Items.Clear();
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      using (IDAOSession daoSession = daoFactory.OpenSession ())
      {
        IList<IComponentType> componentTypes = daoFactory.ComponentTypeDAO.FindAllOrderByName();
        m_componentTypeArray = new IComponentType[componentTypes.Count];
        int i = 0;
        foreach (IComponentType componentType in componentTypes) {
          m_componentTypeArray[i++] = componentType;
          typeComboBox.Items.Add((componentType.Display == null)?"":componentType.Display);
        }
        if (componentTypes.Count > 0) {
          typeComboBox.SelectedIndex = 0;
        }
        
        IList<IProject> projects = daoFactory.ProjectDAO.FindAllOrderByName();

        m_projectArray = new IProject[projects.Count];
        i = 0;
        foreach (IProject project in projects) {
          m_projectArray[i++] = project;
          projectComboBox.Items.Add((project.Display == null)?"":project.Display);
        }
        if (projects.Count > 0) {
          projectComboBox.SelectedIndex = 0;
        }
        
      }
      
      this.m_displayMode = displayMode;
      switch (displayMode) {
        case DisplayMode.VIEW :
          saveBtn.Text = PulseCatalog.GetString("Save");
          saveBtn.Enabled = false;
          resetBtn.Text = PulseCatalog.GetString("Reset");
          createBtn.Visible = false;
          cancelBtn.Visible = false;
          projectComboBox.Enabled = false;
          break;
        case DisplayMode.CREATE :
          createBtn.Text = PulseCatalog.GetString("Create");
          cancelBtn.Text = PulseCatalog.GetString("Cancel");
          saveBtn.Visible = false;
          resetBtn.Visible = false;
          projectComboBox.Enabled = false;
          
          if(this.FindForm() is CreateItemForm) {
            CreateItemForm createItemForm = this.FindForm() as CreateItemForm;
            // new component must be attached to selected node if CreateItemForm.Goal==BIND
          }
          break;
      }
    }

    /// <summary>
    ///   Default constructor without argument
    /// </summary>
    public ComponentControl() : this(DisplayMode.VIEW)
    {
    }

    #endregion // Constructors

    
    #region inherited Methods
    
    /// <summary>
    ///   Update state of this observer. In this case, ComponentControl
    ///   will hide or not according type of selected node  in ITreeViewObservable
    /// </summary>
    public void UpdateInfo (ITreeViewObservable observable)
    {
      this.m_observable = observable;
      TreeNode selectedNode = observable.GetSelectedNode();
      if (selectedNode != null) {
        if (selectedNode.Tag is Tuple<bool, IComponent>) {
          m_node = selectedNode;
          observable.ReloadTreeNodes (m_node);
          LoadData (((Tuple<bool, IComponent>)m_node.Tag).Item2);
        }
      }
    }
    #endregion

    
    #region Methods button click
    
    /// <summary>
    ///   Save all changes
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    void SaveBtnClick(object sender, EventArgs e) {
      if (!ValidateData()) { //If entered values are not correct
        return;
      }
      m_component.Name = nameTextBox.Text;
      m_component.Code = codeTextBox.Text;
      m_component.DocumentLink = documentLinkTextBox.Text;
      if (estimatedHoursTextBox.Text.Trim().Length == 0) {
        m_component.EstimatedHours = null;
      }
      else {
        m_component.EstimatedHours = double.Parse(estimatedHoursTextBox.Text.Trim());
      }
      m_component.Type = m_componentTypeArray[typeComboBox.SelectedIndex];
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      using (IDAOSession daoSession = daoFactory.OpenSession ())
      {
        using (IDAOTransaction transaction = daoSession.BeginTransaction ())
        {
          daoFactory.ComponentDAO.MakePersistent(m_component);
          transaction.Commit();
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
      if (m_component !=  null) {
        nameTextBox.Text = m_component.Name;
        codeTextBox.Text = m_component.Code;
        documentLinkTextBox.Text = m_component.DocumentLink;
        estimatedHoursTextBox.Text = (m_component.EstimatedHours == null)?"" : m_component.EstimatedHours.Value.ToString();
        IComponentType componentType;
        for (int i = 0; i < m_componentTypeArray.Length; i++) {
          componentType = m_componentTypeArray[i];
          if (m_component.Type.Id == componentType.Id) {
            typeComboBox.SelectedIndex = i;
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
    ///   Create a Component
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    void CreateBtnClick(object sender, EventArgs e) {
      //if values entered is not correct, exit
      if (!ValidateData()) {
        return;
      }
      Lemoine.Model.IComponent component = ModelDAOHelper.ModelFactory
        .CreateComponentFromName(null, nameTextBox.Text, m_componentTypeArray[typeComboBox.SelectedIndex]);
      component.Code = codeTextBox.Text;
      component.DocumentLink = documentLinkTextBox.Text;
      if(estimatedHoursTextBox.Text.Trim().Length == 0) {
        component.EstimatedHours = null;
      }
      else {
        component.EstimatedHours = double.Parse(estimatedHoursTextBox.Text.Trim());
      }

      if(this.FindForm() is CreateItemForm) {
        CreateItemForm createItemForm = this.FindForm() as CreateItemForm;
        // new component must be attached to selected node if CreateItemForm.Goal==BIND
        if(createItemForm.Goal == CreateItemForm.GoalType.BIND) {
          TreeNode parentNode = createItemForm.OperationTreeView.TreeView.SelectedNode;
          IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
          using (IDAOSession daoSession = daoFactory.OpenSession ())
          {
            using (IDAOTransaction transaction = daoSession.BeginTransaction ())
            {
              if(parentNode.Tag is Tuple<bool, IProject>) {
                IProject project = ((Tuple<bool, IProject>)parentNode.Tag).Item2;
                ModelDAOHelper.DAOFactory.ProjectDAO.Lock (project);
                project.AddComponent (component);
                daoFactory.ComponentDAO.MakePersistent(component);
                daoFactory.ProjectDAO.MakePersistent(project);
              }
              else if(parentNode.Tag is Tuple<bool, IJob>) {
                IJob job = (IJob)((Tuple<bool, IJob>)parentNode.Tag).Item2;
                ModelDAOHelper.DAOFactory.JobDAO.Lock (job);
                job.AddComponent (component);
                daoFactory.ComponentDAO.MakePersistent(component);
                daoFactory.JobDAO.MakePersistent(job);
              }
              transaction.Commit();
            }
            this.m_component = component;
            
            createItemForm.OperationTreeView.BuildTreeNodes(parentNode);
            foreach (TreeNode childNode in parentNode.Nodes) {
              if (childNode.Name == ((Lemoine.Collections.IDataWithId)component).Id.ToString()) {
                parentNode.TreeView.SelectedNode = childNode;
                break;
              }
            }
            parentNode.TreeView.Focus();
            createItemForm.OperationTreeView.NotifyObservers();
          }
        }
      }
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
    ///   Use informations in Component to fill text in box
    /// </summary>
    public void LoadData(Lemoine.Model.IComponent component) {
      if (component != null) {
        IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
        using (IDAOSession daoSession = daoFactory.OpenSession ())
        {
          ModelDAOHelper.DAOFactory.ComponentDAO.Lock (component);
          this.m_component = component;
          NHibernate.NHibernateUtil.Initialize(this.m_component.Project);
        }
        nameTextBox.Text = component.Name;
        codeTextBox.Text = component.Code;
        documentLinkTextBox.Text = component.DocumentLink;
        estimatedHoursTextBox.Text = (component.EstimatedHours == null)? "" : component.EstimatedHours.Value.ToString();
        for (int i = 0; i < m_componentTypeArray.Length; i++) {
          if (component.Type.Id == m_componentTypeArray[i].Id) {
            typeComboBox.SelectedIndex = i;
            break;
          }
        }
        for (int i = 0; i < m_projectArray.Length; i++) {
          if (((Lemoine.Collections.IDataWithId)this.m_component.Project).Id == ((Lemoine.Collections.IDataWithId)m_projectArray[i]).Id) {
            projectComboBox.SelectedIndex = i;
            break;
          }
        }
        resetBtn.Enabled = true;
        saveBtn.Enabled = false;
        
        // Fill list of WorkOrder associated with this Part
        int row = 0;
        tableLayoutPanel.RowStyles.Clear();
        tableLayoutPanel.ColumnStyles.Clear();
        tableLayoutPanel.Controls.Clear();
        tableLayoutPanel.RowCount = 1;
        tableLayoutPanel.AutoSize = true;
        Label titleLbl = new Label();
        bool isProject = true;
        if (m_node.Parent.Tag is Tuple<bool, IProject>) {
          titleLbl.Text = PulseCatalog.GetString("ListOfAssociatedProject");
          ParentType = typeof(IProject);
          isProject = true;
        }
        else if (m_node.Parent.Tag is Tuple<bool, IJob>) {
          titleLbl.Text = PulseCatalog.GetString("ListOfAssociatedJob");
          ParentType = typeof(IJob);
          isProject = false;
        }
        titleLbl.AutoSize = true;
        titleLbl.Padding = new Padding(titleLbl.Padding.Left,titleLbl.Padding.Top,titleLbl.Padding.Right,5);
        titleLbl.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        titleLbl.Font = new Font(titleLbl.Font, titleLbl.Font.Style | FontStyle.Bold | FontStyle.Underline);
        tableLayoutPanel.Controls.Add(titleLbl,0,row);
        tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        row++;
        
        LinkLabel linkLabel = new LinkLabel();
        linkLabel.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
        linkLabel.Height = 20;
        linkLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        linkLabel.AutoSize = true;
        String display = "";
        if (isProject) {
          display = this.m_component.Project.Display;
          linkLabel.Tag = this.m_component.Project;
        }
        else
        {
          display = this.m_component.Project.Job.Display;
          linkLabel.Tag = this.m_component.Project.Job;
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
    
    
    private void linkLabelClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
    {
      e.Link.Visited = true;
      if (((LinkLabel)sender).Tag is IProject) {
        IProject project = (IProject)(((LinkLabel)sender).Tag);
        if (m_node.Level > 0) {
          m_observable.GiveFocusToAllNodeInstances(typeof(IProject), project);
        }
      }
      else if (((LinkLabel)sender).Tag is IJob){
        IJob job = (IJob)(((LinkLabel)sender).Tag);
        if (m_node.Level > 0) {
          m_observable.GiveFocusToAllNodeInstances(typeof(IJob), job);
        }
      }
    }
    
    
    /// <summary>
    ///   Set the project associated with this Component
    /// </summary>
    /// <param name="project">Project associated with this Component</param>
    public void SetProject(IProject project) {
      for (int i = 0; i < m_projectArray.Length; i++) {
        if (((Lemoine.Collections.IDataWithId)project).Id == ((Lemoine.Collections.IDataWithId)m_projectArray[i]).Id) {
          projectComboBox.SelectedIndex = i;
          break;
        }
      }
    }

    /// <summary>
    ///   Test if a change on field occurs and then enable or disable
    ///   save button
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    void NameTextBoxTextChanged(object sender, EventArgs e) {
      if ((m_component != null) && (m_displayMode == DisplayMode.VIEW)) {
        if (HasBeenChanged()) {
          saveBtn.Enabled = true;
        }
        else {
          saveBtn.Enabled = false;
        }
      }
    }

    /// <summary>
    ///   get whether a field has been changed
    /// </summary>
    /// <returns>True if a fields has been changed, false otherwise</returns>
    bool HasBeenChanged(){
      String s;
      s = (m_component.Name==null)?"":m_component.Name;
      if (!nameTextBox.Text.Equals(s)) {
        return true;
      }
      s = (m_component.Code==null)?"":m_component.Code;
      if (!codeTextBox.Text.Equals(s)) {
        return true;
      }
      s = (m_component.DocumentLink==null)?"":m_component.DocumentLink;
      if (!documentLinkTextBox.Text.Equals(s)) {
        return true;
      }
      s = (m_component.EstimatedHours==null)?"":m_component.EstimatedHours.ToString();
      if (!estimatedHoursTextBox.Text.Trim().Equals(s)) {
        return true;
      }
      if (m_componentTypeArray[typeComboBox.SelectedIndex].Id != m_component.Type.Id) {
        return true;
      }
      return false;
    }
    
    
    /// <summary>
    ///   Tell whether input values are corrects. If not a messagebox
    ///   is displayed to tell incorrects fields.
    /// </summary>
    /// <returns>true if all fields have corrects input, false otherwise.</returns>
    bool ValidateData(){
      if ((nameTextBox.Text.Length == 0) && (codeTextBox.Text.Length == 0)) {
        MessageBox.Show (PulseCatalog.GetString("FieldsNameAndCodeCanNotHaveBothNullValue"), "",MessageBoxButtons.OK, MessageBoxIcon.Error);
        return false;
      }
      IProject project = m_projectArray[projectComboBox.SelectedIndex];
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      switch (m_displayMode) {
        case DisplayMode.VIEW :
          using (IDAOSession daoSession = daoFactory.OpenSession ())
          {
            if (daoFactory.ComponentDAO.IfExistsOtherWithSameName(nameTextBox.Text,((Lemoine.Collections.IDataWithId)m_component).Id,((Lemoine.Collections.IDataWithId)project).Id)) {
              MessageBox.Show (PulseCatalog.GetString("ThereIsAnotherItemWithSameNameThatBelongsToThisProject")+"\n"+PulseCatalog.GetString("PleaseChangeIt"),"",MessageBoxButtons.OK, MessageBoxIcon.Error);
              return false;
            }
            
            if (daoFactory.ComponentDAO.IfExistsOtherWithSameCode(codeTextBox.Text,((Lemoine.Collections.IDataWithId)m_component).Id,((Lemoine.Collections.IDataWithId)project).Id)) {
              MessageBox.Show (PulseCatalog.GetString("ThereIsAnotherItemWithSameCodeThatBelongsToThisProject")+"\n"+PulseCatalog.GetString("PleaseChangeIt"),"",MessageBoxButtons.OK, MessageBoxIcon.Error);
              return false;
            }
          }
          break;
        case DisplayMode.CREATE :
          using (IDAOSession daoSession = ModelDAOHelper.DAOFactory.OpenSession ())
          {
            if (daoFactory.ComponentDAO.IfExistsWithSameName(codeTextBox.Text,((Lemoine.Collections.IDataWithId)project).Id)) {
              MessageBox.Show (PulseCatalog.GetString("ThereIsAnotherItemWithSameNameThatBelongsToThisProject")+"\n"+PulseCatalog.GetString("PleaseChangeIt"),"",MessageBoxButtons.OK, MessageBoxIcon.Error);
              return false;
            }
            
            if (daoFactory.ComponentDAO.IfExistsWithSameCode(codeTextBox.Text,((Lemoine.Collections.IDataWithId)project).Id)) {
              MessageBox.Show (PulseCatalog.GetString("ThereIsAnotherItemWithSameCodeThatBelongsToThisProject")+"\n"+PulseCatalog.GetString("PleaseChangeIt"),"",MessageBoxButtons.OK, MessageBoxIcon.Error);
              return false;
            }
          }
          break;
      }
      
      String msg = PulseCatalog.GetString("FollowingFieldsHaveIncorrectValues");
      bool valid = true;

      if (typeComboBox.SelectedIndex == -1) {
        msg = msg + "\n\t" + typeLbl.Text;
        valid = false;
      }
      if (projectComboBox.SelectedIndex == -1) {
        msg = msg + "\n\t" + projectLbl.Text;
        valid = false;
      }
      try {
        if (estimatedHoursTextBox.Text.Trim().Length != 0) {
          double.Parse(estimatedHoursTextBox.Text.Trim());
        }
      }
      catch (Exception) {
        msg = msg + "\n\t" + estimatedHoursLbl.Text;
        valid = false;
      }
      if (!valid) {
        MessageBox.Show (msg, "",MessageBoxButtons.OK, MessageBoxIcon.Error);
      }
      return valid;
    }
    
    #endregion // Methods


  }
}
