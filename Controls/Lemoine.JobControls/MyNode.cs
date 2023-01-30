// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;

using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;

namespace Lemoine.JobControls
{
  /// <summary>
  ///   This class is used to display all instance of an item in
  ///   OperationTreeView. This class enable us to get list of
  ///   branches of OperationTreeView starting from a node which given id
  ///   and type and ending to nodes which information of roots of OperationTreeView
  /// </summary>
  internal class InversedTreeNode
  {
    #region Members
    BaseOperationTreeView m_treeView;
    Lemoine.Collections.IDataWithId m_data;
    Type m_type;
    InversedTreeNode m_parent = null;
    List<InversedTreeNode> m_children = new List<InversedTreeNode> ();
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (InversedTreeNode).FullName);

    #region Getters / Setters
    /// <summary>
    /// Get Data
    /// </summary>
    public Lemoine.Collections.IDataWithId Data {
      get { return m_data; }
    }
    
    /// <summary>
    ///   Get type
    /// </summary>
    public Type Type {
      get {
        return m_type;
      }
    }
    
    /// <summary>
    ///   Get or set parent
    /// </summary>
    public InversedTreeNode Parent {
      get {
        return m_parent;
      }
      set {
        m_parent = value;
      }
    }
    
    /// <summary>
    ///   Get Children list
    /// </summary>
    public List<InversedTreeNode> Children {
      get {
        return m_children;
      }
    }
    
    /// <summary>
    ///   Get level of this node in its associated Tree
    /// </summary>
    public int Level {
      get {
        int i = 0;
        InversedTreeNode temp = this;
        while (temp.Parent != null) {
          temp = temp.Parent;
          i++;
        }
        return i;
      }
    }
    
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Constructor with data and type
    /// </summary>
    /// <param name="treeView"></param>
    /// <param name="data"></param>
    /// <param name="type"></param>
    public InversedTreeNode (BaseOperationTreeView treeView, Lemoine.Collections.IDataWithId data, Type type)
    {
      this.m_treeView = treeView;
      this.m_data = data;
      this.m_type = type;
      this.m_parent = null;
      this.m_children = new List<InversedTreeNode>();
    }

    /// <summary>
    ///   Constructor with Data, Type and Node's parent
    /// </summary>
    /// <param name="data">Data</param>
    /// <param name="type">Type</param>
    /// <param name="parent">Parent</param>
    public InversedTreeNode (Lemoine.Collections.IDataWithId data, Type type, InversedTreeNode parent)
    {
      this.m_treeView = parent.m_treeView;
      this.m_data = data;
      this.m_type = type;
      this.m_parent = null;
      this.m_children = new List<InversedTreeNode>();
    }

    #endregion // Constructors

    #region Methods

    /// <summary>
    ///  Add a node in children list
    /// </summary>
    /// <param name="child"></param>
    public void AddChild (InversedTreeNode child)
    {
      m_children.Add(child);
      child.Parent = this;
    }
    
    /// <summary>
    ///  Remove node in children list
    /// </summary>
    /// <param name="child">child noe</param>
    /// <returns>return true if deletion succeed, false otherwise</returns>
    public bool RemoveChild(InversedTreeNode child)
    {
      bool res = m_children.Remove(child);
      if (res) {
        child.Parent = null;
      }
      return res;
    }
    
    /// <summary>
    ///  Get data associated with this node
    /// </summary>
    /// <returns>Item associated with this node</returns>
    public Object GetData()
    {
      return m_data;
    }
    
    /// <summary>
    /// Fill list of children node associated with current node
    /// </summary>
    /// <param name="treeView"></param>
    public void SetChildren(BaseOperationTreeView treeView)
    {
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      if (Type.Equals(typeof(IProject))) {
        IProject project = (IProject) GetData();
        using (IDAOSession daoSession = daoFactory.OpenSession ())
        {
          daoFactory.ProjectDAO.Lock(project);
          foreach (IWorkOrder workorder in project.WorkOrders) {
            InversedTreeNode node = new InversedTreeNode(workorder, typeof(IWorkOrder),this);
            this.AddChild(node);
          }
        }
      }
      else if (Type.Equals(typeof(Lemoine.Model.IComponent))) {
        Lemoine.Model.IComponent component = (Lemoine.Model.IComponent) GetData();
        using (IDAOSession daoSession = daoFactory.OpenSession ())
        {
          daoFactory.ComponentDAO.Lock(component);
          IProject project = component.Project;
          Type childType = treeView.GetParentType (typeof(Lemoine.Model.IComponent));
          if (object.Equals (childType, typeof(IProject))) {
            InversedTreeNode node = new InversedTreeNode(project, typeof(IProject),this);
            this.AddChild(node);
          }
          else if (object.Equals (childType,typeof(IJob))) {
            InversedTreeNode node = new InversedTreeNode(project.Job, typeof(IJob),this);
            this.AddChild(node);
          }
        }
      }
      else if (Type.Equals(typeof(IPart))) {
        IPart part = (IPart) GetData();
        using (IDAOSession daoSession = daoFactory.OpenSession ())
        {
          ModelDAOHelper.DAOFactory.PartDAO.Lock (part);
          foreach (IWorkOrder workOrder in part.WorkOrders) {
            InversedTreeNode node = new InversedTreeNode(workOrder, typeof(IWorkOrder), this);
            this.AddChild(node);
          }
        }
      }
      else if (Type.Equals(typeof(IIntermediateWorkPiece))) {
        IIntermediateWorkPiece intermediateWorkPiece = (IIntermediateWorkPiece) GetData();;
        using (IDAOSession daoSession = daoFactory.OpenSession ())
        {
          daoFactory.IntermediateWorkPieceDAO.Lock(intermediateWorkPiece);
          Type childType = treeView.GetParentType (typeof(IIntermediateWorkPiece));
          foreach (Lemoine.Model.IComponentIntermediateWorkPiece componentIntermediateWorkPiece in intermediateWorkPiece.ComponentIntermediateWorkPieces) {
            Lemoine.Model.IComponent component = componentIntermediateWorkPiece.Component;
            if (object.Equals (childType, typeof(Lemoine.Model.IComponent))) {
              InversedTreeNode node = new InversedTreeNode(component, typeof(Lemoine.Model.IComponent),this);
              this.AddChild(node);
            }
            else if (object.Equals (childType, typeof(IPart))) {
              InversedTreeNode node = new InversedTreeNode(component.Part, typeof(IPart),this);
              this.AddChild(node);
            }
          }
        }
      }
      else if (Type.Equals(typeof(IOperation))) {
        Object data = GetData();
        IOperation operation = (IOperation) data;
        using (IDAOSession daoSession = daoFactory.OpenSession ())
        {
          daoFactory.OperationDAO.Lock(operation);
          foreach (IIntermediateWorkPiece intermediateWorkPiece in operation.IntermediateWorkPieces) {
            InversedTreeNode node = new InversedTreeNode(intermediateWorkPiece, typeof(IIntermediateWorkPiece),this);
            this.AddChild(node);
          }
        }
      }
      else if (Type.Equals(typeof(ISimpleOperation))) {
        Object data = GetData();
        ISimpleOperation simpleOperation = (ISimpleOperation) data;
        using (IDAOSession daoSession = daoFactory.OpenSession ())
        {
          IIntermediateWorkPiece intermediateWorkPiece = simpleOperation.IntermediateWorkPiece;
          daoFactory.IntermediateWorkPieceDAO.Lock (intermediateWorkPiece);
          Type childType = treeView.GetParentType (typeof (ISimpleOperation));
          foreach (Lemoine.Model.IComponentIntermediateWorkPiece componentIntermediateWorkPiece in intermediateWorkPiece.ComponentIntermediateWorkPieces) {
            Lemoine.Model.IComponent component = componentIntermediateWorkPiece.Component;
            if (object.Equals (childType, typeof(Lemoine.Model.IComponent))) {
              InversedTreeNode node = new InversedTreeNode(component, typeof(Lemoine.Model.IComponent),this);
              this.AddChild(node);
            }
            else if (object.Equals (childType, typeof(IPart))) {
              InversedTreeNode node = new InversedTreeNode(component.Part, typeof(IPart), this);
              this.AddChild(node);
            }
          }
        }
      }
    }
    #endregion // Methods
    
  }
}
