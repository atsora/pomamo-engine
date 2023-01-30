// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Lemoine.Model;
using Lemoine.I18N;
using Lemoine.ModelDAO;
using Lemoine.Collections;
using Lemoine.Core.Log;

namespace Lemoine.JobControls
{
  /// <summary>
  /// Description of CreateItemForm.
  /// </summary>
  public partial class CreateItemForm : Form
  {
    /// <summary>
    /// Accepted types for display mode
    /// </summary>
    public enum GoalType {
      /// <summary>
      /// Display CreateViewForm to just create an item
      /// </summary>
      NEW = 1,
      /// <summary>
      /// Display CreateViewForm to create an item and bind it
      /// </summary>
      BIND = 2
    };

    #region Members
    WorkOrderControl workOrderControl;
    ProjectControl projectControl;
    ComponentControl componentControl;
    IntermediateWorkPieceControl intermediateWorkPieceControl;
    JobControl jobControl;
    PartControl partControl;
    SimpleOperationControl simpleOperationControl;
    SequenceControl sequenceControl;
    PathControl pathControl;
    Type m_controlType;
    GoalType m_goal;
    OperationTreeView m_operationTreeView;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (CreateItemForm).FullName);

    #region Getters / Setters
    #endregion // Getters / Setters

    #region Constructors
    
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public CreateItemForm(Type type, GoalType goal, OperationTreeView operationtreeview)
    {
      //
      // The InitializeComponent() call is required for Windows Forms designer support.
      //
      InitializeComponent();
      m_controlType = type;
      this.m_goal = goal;
      this.m_operationTreeView = operationtreeview;
    }

    #endregion // Constructors

    #region Methods
    
    void CreateItemFormLoad(object sender, EventArgs e)
    {
      UserControl addedControl = null;
      if (m_controlType.Equals(typeof(IWorkOrder))) {
        workOrderControl = new WorkOrderControl(WorkOrderControl.DisplayMode.CREATE);
        workOrderControl.Location = new Point(0,0);
        this.Controls.Add(workOrderControl);
        addedControl = workOrderControl;
        this.Text = PulseCatalog.GetString ("WorkOrderCreation");
      }
      else if (m_controlType.Equals(typeof(IProject))) {
        projectControl = new ProjectControl(ProjectControl.DisplayMode.CREATE);
        projectControl.Location = new Point(0,0);
        this.Controls.Add(projectControl);
        addedControl = projectControl;
        this.Text = PulseCatalog.GetString ("ProjectCreation");
      }
      else if (m_controlType.Equals(typeof(IJob))) {
        jobControl = new JobControl(JobControl.DisplayMode.CREATE);
        jobControl.Location = new Point(0,0);
        this.Controls.Add(jobControl);
        addedControl = jobControl;
        this.Text = PulseCatalog.GetString ("JobCreation");
      }
      else if (m_controlType.Equals(typeof(Lemoine.Model.IComponent))) {
        componentControl = new ComponentControl(ComponentControl.DisplayMode.CREATE);
        componentControl.Location = new Point(0,0);
        this.Controls.Add(componentControl);
        addedControl = componentControl;
        this.Text = PulseCatalog.GetString ("ComponentCreation");
        
        TreeNode parent = m_operationTreeView.TreeView.SelectedNode;
        using (IDAOSession daoSession = ModelDAOHelper.DAOFactory.OpenSession ())
        {
          IProject project;
          if(parent.Tag is Tuple<bool, IProject>) {
            project = ((Tuple<bool, IProject>)parent.Tag).Item2;
          }
          else if(parent.Tag is Tuple<bool, IJob>) {
            project = ((Tuple<bool, IJob>)parent.Tag).Item2.Project;
          }
          else {
            throw new InvalidOperationException ();
          }
          ModelDAOHelper.DAOFactory.ProjectDAO.Lock (project);
          componentControl.SetProject(project);
        }
      }
      else if (m_controlType.Equals(typeof(IPart))) {
        partControl = new PartControl(PartControl.DisplayMode.CREATE);
        partControl.Location = new Point(0,0);
        this.Controls.Add(partControl);
        addedControl = partControl;
        this.Text = PulseCatalog.GetString ("PartCreation");
      }
      else if (m_controlType.Equals(typeof(IIntermediateWorkPiece))) {
        intermediateWorkPieceControl = new IntermediateWorkPieceControl(IntermediateWorkPieceControl.DisplayMode.CREATE);
        intermediateWorkPieceControl.Location = new Point(0,0);
        this.Controls.Add(intermediateWorkPieceControl);
        addedControl = intermediateWorkPieceControl;
        this.Text = PulseCatalog.GetString ("IntermediateWorkPieceCreation");
      }
      else if (m_controlType.Equals(typeof(ISimpleOperation))) {
        simpleOperationControl = new SimpleOperationControl(SimpleOperationControl.DisplayMode.CREATE);
        simpleOperationControl.Location = new Point(0,0);
        this.Controls.Add(simpleOperationControl);
        addedControl = simpleOperationControl;
        this.Text = PulseCatalog.GetString ("SimpleOperationCreation");
      }
      else if (m_controlType.Equals(typeof(IPath))) {
        pathControl = new PathControl(PathControl.DisplayMode.CREATE);
        pathControl.Location = new Point(0,0);
        this.Controls.Add(pathControl);
        addedControl = pathControl;
        this.Text = PulseCatalog.GetString("PathCreation");
        TreeNode parent = m_operationTreeView.TreeView.SelectedNode;
        
        if(parent.Tag is Tuple<bool, IOperation>) {
          pathControl.ParentType = typeof(IOperation);
          using (IDAOSession daoSession = ModelDAOHelper.DAOFactory.OpenSession ())
          {
            IOperation operation = ((Tuple<bool, IOperation>)parent.Tag).Item2;
            ModelDAOHelper.DAOFactory.OperationDAO.Lock (operation);
            pathControl.Operation = operation;
          }
        }
        else if(parent.Tag is Tuple<bool, ISimpleOperation>) {
          pathControl.ParentType = typeof(ISimpleOperation);
          using (IDAOSession daoSession = ModelDAOHelper.DAOFactory.OpenSession ())
          {
            IOperation operation = ((Tuple<bool, ISimpleOperation>)parent.Tag).Item2.Operation;
            ModelDAOHelper.DAOFactory.OperationDAO.Lock (operation);
            pathControl.Operation = operation;
          }
        }
      }
      else if (m_controlType.Equals(typeof(ISequence))) {
        
        sequenceControl = new SequenceControl(SequenceControl.DisplayMode.CREATE);
        sequenceControl.Location = new Point(0,0);
        this.Controls.Add(sequenceControl);
        addedControl = sequenceControl;
        this.Text = PulseCatalog.GetString("SequenceCreation");
        
        TreeNode parent = m_operationTreeView.TreeView.SelectedNode;
        IPath path = null;
        IOperation operation = null;
        
        using (IDAOSession daoSession = ModelDAOHelper.DAOFactory.OpenSession ())
        {
          if (parent.Tag is Tuple<bool, IPath>) {
            path = ((Tuple<bool, IPath>)parent.Tag).Item2;
            ModelDAOHelper.DAOFactory.PathDAO.Lock (path);
            sequenceControl.ShowPath = true;
          }
          else {
            sequenceControl.ShowPath = false;
            if (parent.Tag is Tuple<bool, IOperation>) {
              operation = ((Tuple<bool, IOperation>)parent.Tag).Item2;
              sequenceControl.ParentType = typeof(IOperation);
            }
            else if (parent.Tag is Tuple<bool, ISimpleOperation>) {
              operation = ((Tuple<bool, ISimpleOperation>)parent.Tag).Item2.Operation;
              sequenceControl.ParentType = typeof(ISimpleOperation);
            }
            ModelDAOHelper.DAOFactory.OperationDAO.Lock (operation);
          }
          
          if (path != null) {
            sequenceControl.Path = path;
            NHibernate.NHibernateUtil.Initialize(path.Sequences);
          } else {
            if (operation == null) {
              log.Error("Unable to create a sequenceControl (bad parent)");
              return;
            }
            
            if (operation.Paths.Count > 1) {
              log.WarnFormat("Operation {0} with more than one path in single path mode",
                             operation.ToString());
            }
            
            IEnumerator<IPath> pathEnumerator = operation.Paths.GetEnumerator();
            if (pathEnumerator.MoveNext()) {
              path = pathEnumerator.Current;
              NHibernate.NHibernateUtil.Initialize(path.Sequences);
              sequenceControl.Path = path;
            } else {
              sequenceControl.Operation = operation;
            }
          }
        }
      }
      if (addedControl != null) {
        // resize control w.r.t. embedded one
        this.Size = new Size((int) (addedControl.Size.Width * 1.1),
                             (int) (addedControl.Size.Height * 1.3));
      }
    }
    
    /// <summary>
    ///   Getter of OperationTreeView
    /// </summary>
    public OperationTreeView OperationTreeView {
      get { return m_operationTreeView;}
    }

    /// <summary>
    ///   getter of the GoalType assign to this form
    /// </summary>
    public GoalType Goal {
      get { return m_goal;}
    }

    #endregion // Methods


  }
}
