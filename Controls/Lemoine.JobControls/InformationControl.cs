// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;

using Lemoine.I18N;
using Lemoine.Model;
using Lemoine.Collections;
using Lemoine.Core.Log;

namespace Lemoine.JobControls
{
  /// <summary>
  /// Description of InformationControl.
  /// </summary>
  public partial class InformationControl : UserControl, ITreeViewObserver
  {

    static readonly ILog log = LogManager.GetLogger(typeof (InformationControl).FullName);

    #region Members
    
    ITreeViewObservable m_observable;
    bool m_showId;
    #endregion
    
    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public InformationControl()
    {
      if (null == Lemoine.ModelDAO.ModelDAOHelper.ModelFactory) {
        Lemoine.ModelDAO.ModelDAOHelper.ModelFactory =
          new Lemoine.GDBPersistentClasses.GDBPersistentClassFactory ();
      }
      //
      // The InitializeComponent() call is required for Windows Forms designer support.
      //
      InitializeComponent();
    }

    
    #endregion // Constructors
    
    
    #region Getters / Setters
    /// <summary>
    /// List of all observes
    /// </summary>
    public virtual IList<ITreeViewObserver> AllObservers {
      get {
        IList<ITreeViewObserver> observerList = new List<ITreeViewObserver>();
        observerList.Add(WorkOrderControl);
        observerList.Add(ProjectControl);
        observerList.Add(ComponentControl);
        observerList.Add(IntermediateWorkPieceControl);
        observerList.Add(OperationControl);
        observerList.Add(JobControl);
        observerList.Add(PartControl);
        observerList.Add(SimpleOperationControl);
        observerList.Add(SequenceControl);
        observerList.Add(PathControl);
        return observerList;
        /* for C# 3.0 onward, use
        return new List<ITreeViewObserver> {
          WorkOrderControl,
          ProjectControl, ComponentControl, IntermediateWorkPieceControl, OperationControl,
          JobControl, PartControl, SimpleOperationControl, SequenceControl, PathControl
        };
         */
      }
    }
    
    /// <summary>
    ///   Get WorkOrderControl panel
    /// </summary>
    public virtual WorkOrderControl WorkOrderControl
    {
      get{return workOrderControl;}
    }
    
    /// <summary>
    ///   Get ProjectControl panel
    /// </summary>
    public virtual ProjectControl ProjectControl
    {
      get{return projectControl;}
    }
    
    /// <summary>
    ///   Get ComponentControl panel
    /// </summary>
    public virtual ComponentControl ComponentControl
    {
      get{return componentControl;}
    }
    
    /// <summary>
    ///   Get Control panel
    /// </summary>
    public virtual IntermediateWorkPieceControl IntermediateWorkPieceControl
    {
      get{return intermediateWorkPieceControl;}
    }
    
    /// <summary>
    ///   Get OperationControl panel
    /// </summary>
    public virtual OperationControl OperationControl
    {
      get{return operationControl;}
    }
    
    /// <summary>
    ///   Get JobControl panel
    /// </summary>
    public virtual JobControl JobControl
    {
      get{return jobControl;}
    }
    
    /// <summary>
    ///   Get PartControl panel
    /// </summary>
    public virtual PartControl PartControl
    {
      get{return partControl;}
    }
    
    /// <summary>
    ///   Get SimpleOperationControl panel
    /// </summary>
    public virtual SimpleOperationControl SimpleOperationControl
    {
      get{return simpleOperationControl;}
    }
    
    /// <summary>
    ///   Get SequenceControl panel
    /// </summary>
    public virtual SequenceControl SequenceControl
    {
      get{return sequenceControl;}
    }

    /// <summary>
    ///   Get PathControl panel
    /// </summary>
    public virtual PathControl PathControl
    {
      get{return pathControl;}
    }
    
    /// <summary>
    /// Show object Ids in title or not
    /// </summary>
    public virtual bool ShowId {
      get { return m_showId; }
      set { m_showId = value;}
    }
    #endregion // Getters / Setters

    
    #region inherited Methods
    
    /// <summary>
    /// Complete title with value of fields of object passed as parameters
    /// </summary>
    /// <param name="title"></param>
    /// <param name="fields"></param>
    /// <param name="obj"></param>
    /// <returns></returns>
    private string AddIdToTitle(string title, string[] fields, object obj) {
      if (!this.ShowId) {
        return title;
      }
      
      string newTitle = title;
      foreach (string field in fields) {
        PropertyInfo propertyInfoId =
          obj.GetType().GetProperty(field, BindingFlags.Public | BindingFlags.Instance);
        if (propertyInfoId != null) {
          newTitle += " " + field + "=" + propertyInfoId.GetValue(obj, null);
        }
      }
      return newTitle;
    }
    
    /// <summary>
    /// Update state of this observer. In this case, change text
    /// displayed in title
    /// </summary>
    /// <param name="observable"></param>
    public void UpdateInfo (ITreeViewObservable observable)
    {
      workOrderControl.Visible = false;
      projectControl.Visible = false;
      componentControl.Visible = false;
      intermediateWorkPieceControl.Visible = false;
      operationControl.Visible = false;
      jobControl.Visible = false;
      partControl.Visible = false;
      simpleOperationControl.Visible = false;
      sequenceControl.Visible = false;
      pathControl.Visible = false;

      this.m_observable = observable;
      TreeNode node = observable.GetSelectedNode();
      if (node == null) {
        groupBox.Text = "";
      }
      else if (node.Tag is Tuple<bool, IWorkOrder>) {
        groupBox.Text = AddIdToTitle (PulseCatalog.GetString("WorkOrder"), new string[] {"Id"}, ((Tuple<bool, IWorkOrder>) node.Tag).Item2);
        this.WorkOrderControl.Visible = true;
        this.WorkOrderControl.Dock = DockStyle.Fill;
      }
      else if (node.Tag is Tuple<bool, IProject>) {
        groupBox.Text = AddIdToTitle (PulseCatalog.GetString("Project"), new string[] {"Id"}, ((Tuple<bool, IProject>) node.Tag).Item2);
        this.ProjectControl.Visible = true;
        this.ProjectControl.Dock = DockStyle.Fill;
      }
      else if (node.Tag is Tuple<bool, Model.IComponent>) {
        groupBox.Text = AddIdToTitle (PulseCatalog.GetString("Component"), new string[] {"Id"},  ((Tuple<bool, Model.IComponent>) node.Tag).Item2);
        this.ComponentControl.Visible = true;
        this.ComponentControl.Dock = DockStyle.Fill;
      }
      else if (node.Tag is Tuple<bool, IIntermediateWorkPiece>) {
        groupBox.Text = AddIdToTitle (PulseCatalog.GetString("IntermediateWorkPiece"), new string[] {"Id"},  ((Tuple<bool, IIntermediateWorkPiece>) node.Tag).Item2);
        this.IntermediateWorkPieceControl.Visible = true;
        this.IntermediateWorkPieceControl.Dock = DockStyle.Fill;
      }
      else if (node.Tag is Tuple<bool, IOperation>) {
        groupBox.Text = AddIdToTitle (PulseCatalog.GetString("Operation"), new string[] {"Id"},  ((Tuple<bool, IOperation>) node.Tag).Item2);
        this.OperationControl.Visible = true;
        this.OperationControl.Dock = DockStyle.Fill;
      }
      else if (node.Tag is Tuple<bool, IJob>) {
        groupBox.Text = AddIdToTitle (PulseCatalog.GetString("Job"), new string[] {"ProjectId", "WorkOrderId"},  ((Tuple<bool, IJob>) node.Tag).Item2);
        this.JobControl.Visible = true;
        this.JobControl.Dock = DockStyle.Fill;
      }
      else if (node.Tag is Tuple<bool, IPart>) {
        groupBox.Text = AddIdToTitle (PulseCatalog.GetString("Part"), new string[] { "ComponentId", "ProjectId"}, ((Tuple<bool, IPart>) node.Tag).Item2);
        this.PartControl.Visible = true;
        this.PartControl.Dock = DockStyle.Fill;
      }
      else if (node.Tag is Tuple<bool, ISimpleOperation>) {
        groupBox.Text = AddIdToTitle (PulseCatalog.GetString("SimpleOperation"), new string[] { "IntermediateWorkPieceId", "OperationId"},  ((Tuple<bool, ISimpleOperation>) node.Tag).Item2);
        this.SimpleOperationControl.Visible = true;
        this.SimpleOperationControl.Dock = DockStyle.Fill;
      }
      else if (node.Tag is Tuple<bool, ISequence>) {
        groupBox.Text = AddIdToTitle (PulseCatalog.GetString("Sequence"), new string[] {"Id"}, ((Tuple<bool, ISequence>) node.Tag).Item2);
        this.SequenceControl.Visible = true;
        this.SequenceControl.Dock = DockStyle.Fill;
      }
      else if (node.Tag is Tuple<bool, IPath>) {
        groupBox.Text = AddIdToTitle (PulseCatalog.GetString("Path"), new string[] {"Id"}, ((Tuple<bool, IPath>) node.Tag).Item2);
        this.PathControl.Visible = true;
        this.PartControl.Dock = DockStyle.Fill;
      }
      else {
        groupBox.Text = "";
      }
    }
    #endregion
  }
}
