// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;

using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using Lemoine.Business.Config;

namespace Lemoine.JobControls
{
  /// <summary>
  /// Image index
  /// </summary>
  public enum TreeViewImageIndex {
    /// <summary>
    /// Index assigned to WorkOrder
    /// </summary>
    WorkOrder = 0,
    /// <summary>
    /// Index assigned to Project
    /// </summary>
    Project = 1,
    /// <summary>
    /// Index assigned to Component
    /// </summary>
    Component = 2,
    /// <summary>
    /// Index assigned to IntermediateWorkPiece
    /// </summary>
    IntermediateWorkPiece = 3,
    /// <summary>
    /// Index assigned to Operation
    /// </summary>
    Operation = 4,
    /// <summary>
    /// Index assigned to Job
    /// </summary>
    Job = 5,
    /// <summary>
    /// Index assigned to Part
    /// </summary>
    Part = 6,
    /// <summary>
    /// Index assigned to SimpleOperation
    /// </summary>
    SimpleOperation = 7,
    /// <summary>
    /// Index assigned to Path
    /// </summary>
    Path = 8,
    /// <summary>
    /// Index assigned to Sequence
    /// </summary>
    Sequence = 9
  };
  
  /// <summary>
  /// Description of BaseOperationTreeView.
  /// </summary>
  public class BaseOperationTreeView: UserControl, ITreeViewObservable
  {
    #region Members
    /// <summary>
    ///   List of control which are impacted by change of treeview;
    ///   Main change is node selection
    /// </summary>
    List<ITreeViewObserver> m_observers = new List<ITreeViewObserver>();
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (BaseOperationTreeView).FullName);

    #region Getters / Setters
    /// <summary>
    /// Get wrapped treeview
    /// </summary>
    public virtual TreeView TreeView { get; }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public BaseOperationTreeView ()
    {
    }
    #endregion // Constructors

    #region ITReeViewObserver implementation
    /// <summary>
    /// Add observer control associated with OperationTreeView
    /// </summary>
    /// <param name="observer"></param>
    public void AddObserver(ITreeViewObserver observer){
      m_observers.Add(observer);
    }
    
    /// <summary>
    /// Delete observer control associated with OperationTreeView
    /// </summary>
    /// <param name="observer"></param>
    public void DeleteObserver(ITreeViewObserver observer){
      m_observers.Remove(observer);
    }
    
    /// <summary>
    /// Notify associated observers in order that they update theirs state
    /// </summary>
    public void NotifyObservers(){
      foreach (ITreeViewObserver observer in m_observers) {
        observer.UpdateInfo(this);
      }
    }
    #endregion // ITreeViewObservable implementation
    
    /// <summary>
    /// Get selected node of wrapped treeview
    /// </summary>
    /// <returns></returns>
    public virtual TreeNode GetSelectedNode() { return null; }
    
    /// <summary>
    ///   Initialization of this observable
    /// </summary>
    public virtual void Init() {}

    /// <summary>
    /// Give the focus to all the nodes with the same name and type
    /// </summary>
    /// <param name="node"></param>
    /// <returns>Number of found nodes</returns>
    public virtual int GiveFocusToAllNodeInstances(TreeNode node)
    {
      return GiveFocusToAllNodeInstances (GetTreeNodeType (node), GetTreeNodeData (node));
    }
    
    /// <summary>
    /// Give the focus to all the nodes with the same name and type
    /// </summary>
    /// <param name="type"></param>
    /// <param name="data"></param>
    /// <returns>Number of found nodes</returns>
    public virtual int GiveFocusToAllNodeInstances(Type type, Lemoine.Collections.IDataWithId data)
    {
      IList<IList<Tuple<Type, int>>> listBranches =
        this.GetAllBranches(data, type);
      Cursor.Current = Cursors.WaitCursor;
      this.TreeView.BeginUpdate();
      BuildBranches (listBranches);
      IList<TreeNode> nodes = FindTreeNodes (type, data.Id.ToString ());
      TreeNode[] nodeArray = ((List<TreeNode>)nodes).ToArray();
      SelectNodes (nodeArray);
      this.TreeView.EndUpdate();
      Cursor.Current = Cursors.Default;
      this.TreeView.Focus();
      return nodes.Count;
    }
    
    /// <summary>
    /// Select the specified nodes
    /// </summary>
    /// <param name="nodeArray"></param>
    public virtual void SelectNodes (TreeNode[] nodeArray) {}
    
    /// <summary>
    /// Get the data of a tree node
    /// </summary>
    /// <param name="node"></param>
    /// <returns></returns>
    public virtual Lemoine.Collections.IDataWithId GetTreeNodeData (TreeNode node) { return null; }
    
    /// <summary>
    /// Get the type of a tree node
    /// </summary>
    /// <param name="node"></param>
    /// <returns></returns>
    public virtual Type GetTreeNodeType (TreeNode node) { return null; }
    
    /// <summary>
    /// Build nodes with the same type and name and load their children
    /// </summary>
    /// <param name="node"></param>
    public virtual void BuildTreeNodes (TreeNode node)
    {
      BuildTreeNodes (GetTreeNodeType (node), node.Name);
    }

    /// <summary>
    /// Build nodes with the same type and name and load their children
    /// </summary>
    /// <param name="type"></param>
    /// <param name="name"></param>
    protected virtual void BuildTreeNodes (Type type, string name) {}

    /// <summary>
    /// Reload nodes with the same type and name
    /// </summary>
    /// <param name="node"></param>
    /// <returns></returns>
    public virtual IList<TreeNode> ReloadTreeNodes (TreeNode node)
    {
      return ReloadTreeNodes (GetTreeNodeType (node), node.Name);
    }

    /// <summary>
    /// Reload a set of tree nodes of a same type and name
    /// </summary>
    /// <param name="type"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    protected virtual IList<TreeNode> ReloadTreeNodes (Type type, string name)
    {
      IList<TreeNode> nodes = FindTreeNodes (type, name);
      if (0 < nodes.Count) {
        Cursor.Current = Cursors.WaitCursor;
        TreeView.BeginUpdate();
        ReloadTreeNodes (type, nodes);
        TreeView.EndUpdate();
      }
      
      return nodes;
    }

    /// <summary>
    /// Reload a set of tree nodes of a same type
    /// </summary>
    /// <param name="type"></param>
    /// <param name="nodes"></param>
    protected virtual void ReloadTreeNodes (Type type, IList<TreeNode> nodes)
    {
      if (type.Equals (typeof (IWorkOrder))) {
        ReloadTreeNodes<IWorkOrder, int> (ModelDAOHelper.DAOFactory.WorkOrderDAO, nodes);
      }
      else if (type.Equals (typeof (IProject))) {
        ReloadTreeNodes<IProject, int> (ModelDAOHelper.DAOFactory.ProjectDAO, nodes);
      }
      else if (type.Equals (typeof (IComponent))) {
        ReloadTreeNodes<IComponent, int> (ModelDAOHelper.DAOFactory.ComponentDAO, nodes);
      }
      else if (type.Equals (typeof (IIntermediateWorkPiece))) {
        ReloadTreeNodes<IIntermediateWorkPiece, int> (ModelDAOHelper.DAOFactory.IntermediateWorkPieceDAO, nodes);
      }
      else if (type.Equals (typeof (IOperation))) {
        ReloadTreeNodes<IOperation, int> (ModelDAOHelper.DAOFactory.OperationDAO, nodes);
      }
      else if (type.Equals (typeof (IJob))) {
        ReloadTreeNodes<IJob, int> (ModelDAOHelper.DAOFactory.JobDAO, nodes);
      }
      else if (type.Equals (typeof (IPart))) {
        ReloadTreeNodes<IPart, int> (ModelDAOHelper.DAOFactory.PartDAO, nodes);
      }
      else if (type.Equals (typeof (ISimpleOperation))) {
        ReloadTreeNodes<ISimpleOperation, int> (ModelDAOHelper.DAOFactory.SimpleOperationDAO, nodes);
      }
      else if (type.Equals (typeof (IPath))) {
        ReloadTreeNodes<IPath, int> (ModelDAOHelper.DAOFactory.PathDAO, nodes);
      }
      else if (type.Equals (typeof (ISequence))) {
        ReloadTreeNodes<ISequence, int> (ModelDAOHelper.DAOFactory.SequenceDAO, nodes);
      }
      else {
        log.FatalFormat ("ReloadTreeNodes: " +
                         "unsupported type {0}",
                         type);
        throw new ArgumentException ("Unsupported type");
      }
    }

    /// <summary>
    /// Reload a set of tree nodes of the same type
    /// </summary>
    /// <param name="dao"></param>
    /// <param name="nodes"></param>
    protected virtual void ReloadTreeNodes<T, ID> (IBaseGenericUpdateDAO<T, ID> dao, IList <TreeNode> nodes)
      where T: IDataWithVersion, Lemoine.Model.IDisplayable
    {
      T data = default (T);
      bool first = true;
      foreach (TreeNode node in nodes) {
        if (first) {
          data = ((Tuple<bool, T>)node.Tag).Item2;
          using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
          {
            data = dao.Reload (data);
          }
          first = false;
        }
        node.Tag = new Tuple<bool, T> (((Tuple<bool, T>)node.Tag).Item1, data);
        node.Text = data.Display;
      }
    }

    /// <summary>
    /// Get the item that is associated to a node
    /// </summary>
    /// <param name="node"></param>
    /// <returns></returns>
    public virtual object GetItem (TreeNode node) { return null; }

    /// <summary>
    /// Get the object type of a node
    /// </summary>
    /// <param name="node"></param>
    /// <returns></returns>
    public virtual Type GetItemType (TreeNode node)
    {
      object tag = node.Tag;
      
      if ((tag is Tuple<bool, IWorkOrder>) || (tag is IWorkOrder))  {
        return typeof(IWorkOrder);
      }
      else if ((tag is Tuple<bool, IProject>) || (tag is IProject)) {
        return typeof(IProject);
      }
      else if ((tag is Tuple<bool, IJob>) || (tag is IJob)) {
        return typeof(IJob);
      }
      else if ((tag is Tuple<bool, IComponent>) || (tag is Lemoine.Model.IComponent)) {
        return typeof(Lemoine.Model.IComponent);
      }
      else if ((tag is Tuple<bool, IPart>) || (tag is IPart)) {
        return typeof(IPart);
      }
      else if ((tag is Tuple<bool, IIntermediateWorkPiece>) || (tag is IIntermediateWorkPiece)) {
        return typeof(IIntermediateWorkPiece);
      }
      else if ((tag is Tuple<bool, IOperation>) || (tag is IOperation)) {
        return typeof(IOperation);
      }
      else if ((tag is Tuple<bool, ISimpleOperation>) || (tag is ISimpleOperation)) {
        return typeof(ISimpleOperation);
      }
      else if ((tag is Tuple<bool, IPath>) || (tag is IPath)) {
        return typeof(IPath);
      }
      else if ((tag is Tuple<bool, ISequence>) || (tag is ISequence))  {
        return typeof(ISequence);
      }
      return null;
    }

    /// <summary>
    ///   Get the image index of a node
    /// </summary>
    /// <param name="node">Tree node</param>
    /// <returns>index associated with tag of a tree node</returns>
    public virtual int GetImageIndex (TreeNode node)
    {
      object tag = node.Tag;
      
      if ((tag is Tuple<bool, IWorkOrder>) || (tag is IWorkOrder)) {
        return (int)TreeViewImageIndex.WorkOrder;
      }
      else if ((tag is Tuple<bool, IProject>) || (tag is IProject)) {
        return (int)TreeViewImageIndex.Project;
      }
      else if ((tag is Tuple<bool, IJob>) || (tag is IJob)) {
        return (int)TreeViewImageIndex.Job;
      }
      else if ((tag is Tuple<bool, IComponent>) || (tag is Lemoine.Model.IComponent)) {
        return (int)TreeViewImageIndex.Component;
      }
      else if ((tag is Tuple<bool, IPart>) || (tag is IPart))  {
        return (int)TreeViewImageIndex.Part;
      }
      else if ((tag is Tuple<bool, IIntermediateWorkPiece>) || (tag is IIntermediateWorkPiece)) {
        return (int)TreeViewImageIndex.IntermediateWorkPiece;
      }
      else if ((tag is Tuple<bool, IOperation>) || (tag is IOperation))  {
        return (int)TreeViewImageIndex.Operation;
      }
      else if ((tag is Tuple<bool, ISimpleOperation>) || (tag is ISimpleOperation))  {
        return (int)TreeViewImageIndex.SimpleOperation;
      }
      else if ((tag is Tuple<bool, IPath>) || (tag is IPath))  {
        return (int)TreeViewImageIndex.Path;
      }
      else if ((tag is Tuple<bool, ISequence>) || (tag is ISequence)) {
        return (int)TreeViewImageIndex.Sequence;
      }
      return -1;
    }

    /// <summary>
    /// Get the level of a type
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public virtual int GetTypeLevel (Type type) { return 0; }
    
    /// <summary>
    /// Get the type of a child
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public virtual Type GetChildType (Type type) { return null; }
    
    /// <summary>
    /// Get the type of a parent
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public virtual Type GetParentType (Type type) { return null; }

    /// <summary>
    /// Create and build Tree which has root with given data and type
    /// </summary>
    /// <param name="data">item</param>
    /// <param name="type">Item type</param>
    /// <returns>A node which is root of tree</returns>
    InversedTreeNode GetTree(Lemoine.Collections.IDataWithId data, Type type)
    {
      InversedTreeNode rootNode = new InversedTreeNode(this, data, type);
      GetTreeRec(rootNode);
      return rootNode;
    }

    /// <summary>
    /// Recursive methods use to build a Tree
    /// </summary>
    /// <param name="root">Current node</param>
    void GetTreeRec(InversedTreeNode root)
    {
      root.SetChildren(this);
      foreach (InversedTreeNode childNode in root.Children) {
        GetTreeRec(childNode);
      }
    }

    /// <summary>
    /// Build a Tree which has root with given id and type, and
    /// return list of branches of this tree
    /// </summary>
    /// <param name="data">data of root node</param>
    /// <param name="type">type of root node</param>
    /// <returns></returns>
    protected virtual IList<IList<Tuple<Type, int>>> GetAllBranches(Lemoine.Collections.IDataWithId data, Type type)
    {
      InversedTreeNode rootNode = GetTree (data, type);
      IList<IList<Tuple<Type, int>>> listBranches = new List<IList<Tuple<Type, int>>> ();
      GetBranches(rootNode, listBranches);
      return listBranches;
    }

    /// <summary>
    /// Retrieve list of branches obtained using given node as root and
    /// add them in given list
    /// </summary>
    /// <param name="node"></param>
    /// <param name="list"></param>
    void GetBranches(InversedTreeNode node, IList<IList<Tuple<Type, int>>> list)
    {
      if (node.Children.Count == 0) {
        int level = node.Level;
        IList<Tuple<Type, int>> branch = new List<Tuple<Type, int>> ();
        InversedTreeNode temp = node;
        branch.Add(new Tuple<Type, int> (node.Type, temp.Data.Id));
        while (temp.Parent != null) {
          temp = temp.Parent;
          branch.Add(new Tuple<Type, int> (temp.Type,temp.Data.Id));
        }
        list.Add(branch);
      }
      else {
        foreach (InversedTreeNode child in node.Children) {
          GetBranches(child,list);
        }
      }
    }

    /// <summary>
    /// Update nodes with the same type and name and reload their children
    /// starting from the parent
    /// </summary>
    /// <param name="treeNode"></param>
    public virtual void UpdateTreeNodeFromParent (TreeNode treeNode)
    {
      Type type = GetTreeNodeType (treeNode);
      IList<TreeNode> childNodes = FindTreeNodes (type, treeNode.Name);
      if (childNodes.Count > 0) {
        Cursor.Current = Cursors.WaitCursor;
        this.TreeView.BeginUpdate();
        
        Type parentType = GetParentType (type);
        foreach (TreeNode childNode in childNodes) {
          if (null != childNode.Parent) {
            TreeNode node = childNode.Parent;
            IList<TreeNode> nodes = FindTreeNodes (parentType, node.Name);
            UpdateTreeNode (parentType, nodes);
            node.Collapse (false);
            // TODO: expand, but load the data first
          }
        }

        this.TreeView.EndUpdate();
      }
    }

    /// <summary>
    /// Update nodes with the same type and name and reload their children
    /// </summary>
    /// <param name="node"></param>
    public virtual void UpdateTreeNode (TreeNode node)
    {
      UpdateTreeNode (GetTreeNodeType (node), node.Name);
    }
    
    /// <summary>
    /// Update nodes with the same type and name and reload their children
    /// </summary>
    /// <param name="type"></param>
    /// <param name="name"></param>
    protected virtual void UpdateTreeNode (Type type, string name)
    {
      IList<TreeNode> nodes = FindTreeNodes (type, name);
      if (nodes.Count > 0) {
        Cursor.Current = Cursors.WaitCursor;
        this.TreeView.BeginUpdate();
        
        UpdateTreeNode (type, nodes);
        
        foreach (TreeNode node in nodes) {
          node.Collapse(false);
          node.Expand();
        }
        this.TreeView.EndUpdate();
      }
    }
    
    /// <summary>
    /// Update a set of nodes of a same type and reload their children
    /// </summary>
    /// <param name="type"></param>
    /// <param name="nodes"></param>
    void UpdateTreeNode (Type type, IList<TreeNode> nodes)
    {
      if (0 < nodes.Count) {
        ReloadTreeNodes (type, nodes);
        AddNodes (type, nodes);
      }
    }
    
    void AddNodes (Type type, IList<TreeNode> nodes)
    {
      foreach (TreeNode node in nodes) {
        node.Nodes.Clear();
      }

      if (typeof(IWorkOrder).Equals(type)) {
        foreach (TreeNode node in nodes) {
          node.Tag = new Tuple<bool, IWorkOrder> (false, ((Tuple<bool, IWorkOrder>)node.Tag).Item2);
        }
        AddNodesToWorkOrder(nodes);
      }
      else if (typeof(IProject).Equals(type)) {
        foreach (TreeNode node in nodes) {
          node.Tag = new Tuple<bool, IWorkOrder> (false, ((Tuple<bool, IWorkOrder>)node.Tag).Item2);
        }
        AddNodesToProject(nodes);
      }
      else if (typeof(Lemoine.Model.IComponent).Equals(type)) {
        foreach (TreeNode node in nodes) {
          node.Tag = new Tuple<bool, IWorkOrder> (false, ((Tuple<bool, IWorkOrder>)node.Tag).Item2);
        }
        AddNodesToComponent(nodes);
      }
      else if (typeof(IIntermediateWorkPiece).Equals(type)) {
        foreach (TreeNode node in nodes) {
          node.Tag = new Tuple<bool, IWorkOrder> (false, ((Tuple<bool, IWorkOrder>)node.Tag).Item2);
        }
        AddNodesToIntermediateWorkPiece(nodes);
      }
      else if (typeof(IOperation).Equals(type)) {
        foreach (TreeNode node in nodes) {
          node.Tag = new Tuple<bool, IWorkOrder> (false, ((Tuple<bool, IWorkOrder>)node.Tag).Item2);
        }
        // single path iff next level is composed of sequences
        bool singlePath = Lemoine.Info.ConfigSet.Get<bool> (ConfigKeys.GetDataStructureConfigKey (DataStructureConfigKey.SinglePath));
        AddNodesToOperation(nodes);
      }
      else if (typeof(IJob).Equals(type)) {
        foreach (TreeNode node in nodes) {
          node.Tag = new Tuple<bool, IWorkOrder> (false, ((Tuple<bool, IWorkOrder>)node.Tag).Item2);
        }
        AddNodesToJob (nodes);
      }
      else if (typeof(IPart).Equals(type)) {
        foreach (TreeNode node in nodes) {
          node.Tag = new Tuple<bool, IWorkOrder> (false, ((Tuple<bool, IWorkOrder>)node.Tag).Item2);
        }
        AddNodesToPart (nodes);
      }
      else if (typeof(ISimpleOperation).Equals(type)) {
        foreach (TreeNode node in nodes) {
          node.Tag = new Tuple<bool, IWorkOrder> (false, ((Tuple<bool, IWorkOrder>)node.Tag).Item2);
        }
        // single path iff next level is composed of sequences
        bool singlePath = Lemoine.Info.ConfigSet.Get<bool> (ConfigKeys.GetDataStructureConfigKey (DataStructureConfigKey.SinglePath));
        AddNodesToSimpleOperation(nodes);
      }
      else if (typeof(IPath).Equals(type)) {
        foreach (TreeNode node in nodes) {
          node.Tag = new Tuple<bool, IWorkOrder> (false, ((Tuple<bool, IWorkOrder>)node.Tag).Item2);
        }
        AddNodesToPath (nodes);
      }
    }

    /// <summary>
    /// Find the tree nodes with a specified type and name
    /// </summary>
    /// <param name="type"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    protected virtual IList<TreeNode> FindTreeNodes (Type type, string name)
    {
      TreeNode[] possibleNodes = TreeView.Nodes.Find (name, true);
      IList<TreeNode> nodes = new List<TreeNode>();
      foreach (TreeNode node in possibleNodes) {
        Type treeNodeType = GetTreeNodeType (node);
        if (object.Equals (treeNodeType, type)) {
          nodes.Add(node);
        }
      }
      return nodes;
    }
    
    void BuildBranch(IList<Tuple<Type, int>> branch)
    {
      bool searchRoot = true;
      TreeNode node = null;
      foreach (Tuple<Type, int> item in branch) {
        Type type = item.Item1;
        int id = item.Item2;
        if (searchRoot) {
          IList<TreeNode> nodes = FindTreeNodes (type, id.ToString ());
          if (0 < nodes.Count) {
            Debug.Assert (1 == nodes.Count);
            node = nodes [0];
            searchRoot = false;
          }
        }
        else {
          Debug.Assert (null != node);
          if (null != node.Parent) {
            node.Parent.Expand ();
          }
          TreeNode[] children = node.Nodes.Find (id.ToString (), false);
          if (0 == children.Length) {
            log.WarnFormat ("BuildBranch: " +
                            "give up because there is no children");
            return;
          }
          Debug.Assert (1 == children.Length);
          node = children [0];
        }
      }
    }

    /// <summary>
    /// Make instanciation of all node of given TreeView which appears list of Branches
    /// </summary>
    /// <param name="list"></param>
    public virtual void BuildBranches(IList<IList<Tuple<Type, int>>> list)
    {
      foreach (IList<Tuple<Type, int>> dict in list) {
        this.BuildBranch ((IList<Tuple<Type, int>>)dict);
      }
    }

    /// <summary>
    /// Expand branch that leads to given node
    /// </summary>
    /// <param name="treeNode"></param>
    public virtual void ExpandBranch(TreeNode treeNode)
    {
      if (treeNode.Parent != null) {
        treeNode.Parent.Expand();
        if (treeNode.Parent.Parent != null) {
          ExpandBranch (treeNode.Parent);
        }
      }
    }

    #region AddNode Methods
    /// <summary>
    /// Add child nodes to a node which represents WorkOrder object
    /// </summary>
    /// <param name="workOrderNodes">List of nodes on which we must add child nodes</param>
    public virtual void AddNodesToWorkOrder(IList<TreeNode> workOrderNodes)
    {
      Type childType = GetChildType (typeof (IWorkOrder));
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      using (IDAOSession daoSession = daoFactory.OpenSession ()) {
        foreach (TreeNode workOrderNode in workOrderNodes) {
          IList<TreeNode> workOrderNode2 = new List<TreeNode> ();
          workOrderNode2.Add(workOrderNode);
          ReloadWorkOrderNode (workOrderNode.Name, workOrderNode2);
          IWorkOrder workOrder = ((Tuple<bool, IWorkOrder>)workOrderNode.Tag).Item2;
          ModelDAOHelper.DAOFactory.WorkOrderDAO.Lock (workOrder);
          if (!((Tuple<bool, IWorkOrder>)workOrderNode.Tag).Item1) {
            // this code is used load WorkOrder related to current TreeNode
            // and initialize set of Project associated to him
            ModelDAOHelper.DAOFactory.WorkOrderDAO.InitializeProjects (workOrder);
            TreeNode childNode;
            if (childType.Equals(typeof(IProject))) {
              foreach (IProject project in workOrder.Projects) {
                childNode = new TreeNode (project.Display,(int)TreeViewImageIndex.Project,(int)TreeViewImageIndex.Project);
                childNode.Name = ((Lemoine.Collections.IDataWithId)project).Id.ToString();
                childNode.Tag = new Tuple<bool, IProject> (false, project);
                workOrderNode.Nodes.Add(childNode);
              }
            }
            else if (childType.Equals(typeof(IPart))) {
              foreach (IPart part in workOrder.Parts) {
                childNode = new TreeNode (part.Display,(int)TreeViewImageIndex.Part,(int)TreeViewImageIndex.Part);
                childNode.Name = part.ComponentId.ToString();
                childNode.Tag = new Tuple<bool, IPart> (false, part);
                workOrderNode.Nodes.Add(childNode);
              }
            }
            // assign true to tell that child of this node was already attach
            workOrderNode.Tag = new Tuple<bool, IWorkOrder> (true, ((Tuple<bool, IWorkOrder>)workOrderNode.Tag).Item2);
          }
        }
      }
    }

    /// <summary>
    /// Add child nodes to a node which represents Job object
    /// </summary>
    /// <param name="jobNodes">List of nodes on which we must add child nodes</param>
    public void AddNodesToJob(IList<TreeNode> jobNodes)
    {
      using (IDAOSession daoSession = ModelDAOHelper.DAOFactory.OpenSession ()) {
        foreach (TreeNode jobNode in jobNodes) {
          if (!((Tuple<bool, IJob>)jobNode.Tag).Item1) {
            IJob job = ((Tuple<bool, IJob>)jobNode.Tag).Item2;
            ModelDAOHelper.DAOFactory.JobDAO.Lock (job);

            TreeNode childNode;
            foreach (Lemoine.Model.IComponent component in job.Components) {
              childNode = new TreeNode (component.Display,(int)TreeViewImageIndex.Component,(int)TreeViewImageIndex.Component);
              childNode.Name = ((Lemoine.Collections.IDataWithId)component).Id.ToString();
              childNode.Tag = new Tuple<bool, IComponent> (false, component);
              jobNode.Nodes.Add(childNode);
            }
            // assign true to tell that child of this node was already attach
            jobNode.Tag = new Tuple<bool, IJob> (true, ((Tuple<bool, IJob>)jobNode.Tag).Item2);
          }
        }
      }
    }

    /// <summary>
    /// Add child nodes to a node which represents Project object
    /// </summary>
    /// <param name="projectNodes">List of nodes on which we must add child nodes</param>
    public virtual void AddNodesToProject(IList<TreeNode> projectNodes)
    {
      using (IDAOSession daoSession = ModelDAOHelper.DAOFactory.OpenSession ())
      {
        foreach (TreeNode projectNode in projectNodes) {
          if (!((Tuple<bool, IProject>)projectNode.Tag).Item1) {
            // this code is used load Project related to current TreeNode
            // and initialize set of Component associated to him
            IProject project = ((Tuple<bool, IProject>)projectNode.Tag).Item2;
            ModelDAOHelper.DAOFactory.ProjectDAO.Lock (project);
            TreeNode childNode;
            foreach (Lemoine.Model.IComponent component in project.Components) {
              childNode = new TreeNode (component.Display,(int)TreeViewImageIndex.Component,(int)TreeViewImageIndex.Component);
              childNode.Name = ((Lemoine.Collections.IDataWithId)component).Id.ToString();
              childNode.Tag = new Tuple<bool, IComponent> (false, component);
              projectNode.Nodes.Add(childNode);
            }
            // assign true to tell that child of this node was already attach
            projectNode.Tag = new Tuple<bool, IProject> (true, ((Tuple<bool, IProject>)projectNode.Tag).Item2);
          }
        }
      }
    }


    /// <summary>
    /// Add child nodes to a node which represents Component object
    /// </summary>
    /// <param name="componentNodes">List of nodes on which we must add child nodes</param>
    public virtual void AddNodesToComponent(IList<TreeNode> componentNodes)
    {
      Type childType = GetChildType (typeof (IComponent));
      Lemoine.Model.IComponent component;
      using (IDAOSession daoSession = ModelDAOHelper.DAOFactory.OpenSession ()) {
        foreach (TreeNode componentNode in componentNodes) {
          if (!((Tuple<bool, IComponent>)componentNode.Tag).Item1) {

            // this code is used load Component related to current TreeNode
            // and initialize set of IntermediateWorkPiece associated to him
            component = ((Tuple<bool, IComponent>)componentNode.Tag).Item2;
            ModelDAOHelper.DAOFactory.ComponentDAO.Lock (component);
            TreeNode treeNode;
            if (childType.Equals(typeof(IIntermediateWorkPiece))) {
              foreach (IComponentIntermediateWorkPiece componentIntermediateWorkPiece in component.ComponentIntermediateWorkPieces) {
                IIntermediateWorkPiece intermediateWorkPiece = componentIntermediateWorkPiece.IntermediateWorkPiece;
                treeNode = new TreeNode (intermediateWorkPiece.Display,(int)TreeViewImageIndex.IntermediateWorkPiece,(int)TreeViewImageIndex.IntermediateWorkPiece);
                treeNode.Name = ((Lemoine.Collections.IDataWithId)intermediateWorkPiece).Id.ToString();
                treeNode.Tag = new Tuple<bool, IIntermediateWorkPiece> (false, intermediateWorkPiece);
                componentNode.Nodes.Add(treeNode);
              }
            }
            else if (childType.Equals(typeof(ISimpleOperation))){
              ISimpleOperation simpleOperation;
              foreach (IComponentIntermediateWorkPiece componentIntermediateWorkPiece in component.ComponentIntermediateWorkPieces) {
                IIntermediateWorkPiece intermediateWorkPiece = componentIntermediateWorkPiece.IntermediateWorkPiece;
                simpleOperation = intermediateWorkPiece.SimpleOperation;
                treeNode = new TreeNode (simpleOperation.Display,(int)TreeViewImageIndex.SimpleOperation,(int)TreeViewImageIndex.SimpleOperation);
                treeNode.Name = simpleOperation.IntermediateWorkPieceId.ToString();
                treeNode.Tag = new Tuple<bool, ISimpleOperation> (false, simpleOperation);
                componentNode.Nodes.Add(treeNode);
              }
            }
          }
          // assign true to tell that child of this node was already attach
          componentNode.Tag = new Tuple<bool, IComponent> (true, ((Tuple<bool, IComponent>)componentNode.Tag).Item2);
        }
      }
    }


    /// <summary>
    /// Add child nodes to a node which represents IntermediateWorkPiece object
    /// </summary>
    /// <param name="intermediateWorkPieceNodes">List of nodes on which we must add child nodes</param>
    public virtual void AddNodesToIntermediateWorkPiece(IList<TreeNode> intermediateWorkPieceNodes)
    {
      using (IDAOSession daoSession = ModelDAOHelper.DAOFactory.OpenSession ()) {
        IIntermediateWorkPiece intermediateWorkPiece;
        foreach (TreeNode intermediateWorkPieceNode in intermediateWorkPieceNodes) {
          if (!((Tuple<bool, IIntermediateWorkPiece>)intermediateWorkPieceNode.Tag).Item1) {
            // this code is used load IntermediateWorkPiece related to current TreeNode
            // and initialize Operation associated to him
            intermediateWorkPiece = ((Tuple<bool, IIntermediateWorkPiece>)intermediateWorkPieceNode.Tag).Item2;
            ModelDAOHelper.DAOFactory.IntermediateWorkPieceDAO.Lock (intermediateWorkPiece);
            IOperation operation = intermediateWorkPiece.Operation;
            TreeNode childNode = new TreeNode (operation.Display,(int)TreeViewImageIndex.Operation,(int)TreeViewImageIndex.Operation);
            childNode.Name = ((Lemoine.Collections.IDataWithId)operation).Id.ToString();
            childNode.Tag = new Tuple<bool, IOperation> (false, operation);
            intermediateWorkPieceNode.Nodes.Add(childNode);
            // assign true to tell that child of this node was already attach
            intermediateWorkPieceNode.Tag = new Tuple<bool, IIntermediateWorkPiece> (true, ((Tuple<bool, IIntermediateWorkPiece>)intermediateWorkPieceNode.Tag).Item2);
          }
        }
      }
    }

    /// <summary>
    /// Add child nodes to a node which represents Part object
    /// </summary>
    /// <param name="partNodes">List of nodes on which we must add child nodes</param>
    public void AddNodesToPart(IList<TreeNode> partNodes)
    {
      Type childType = GetChildType (typeof (IPart));
      using (IDAOSession daoSession = ModelDAOHelper.DAOFactory.OpenSession ())
      {
        foreach (TreeNode partNode in partNodes) {
          if (!((Tuple<bool, IPart>)partNode.Tag).Item1) {
            // this code is used load Part related to current TreeNode
            // and initialize set of IntermediateWorkPiece associated to him
            IPart part = ((Tuple<bool, IPart>)partNode.Tag).Item2;
            ModelDAOHelper.DAOFactory.PartDAO.Lock (part);
            TreeNode treeNode;
            if (childType.Equals(typeof(IIntermediateWorkPiece))) {
              foreach (IComponentIntermediateWorkPiece componentIntermediateWorkPiece in part.ComponentIntermediateWorkPieces) {
                IIntermediateWorkPiece intermediateWorkPiece = componentIntermediateWorkPiece.IntermediateWorkPiece;
                treeNode = new TreeNode (intermediateWorkPiece.Display,(int)TreeViewImageIndex.IntermediateWorkPiece,(int)TreeViewImageIndex.IntermediateWorkPiece);
                treeNode.Name = ((Lemoine.Collections.IDataWithId)intermediateWorkPiece).Id.ToString();
                treeNode.Tag = new Tuple<bool, IIntermediateWorkPiece> (false, intermediateWorkPiece);
                partNode.Nodes.Add(treeNode);
              }
            }
            else if (childType.Equals(typeof(ISimpleOperation))){
              ISimpleOperation simpleOperation;
              foreach (IComponentIntermediateWorkPiece componentIntermediateWorkPiece in part.ComponentIntermediateWorkPieces) {
                IIntermediateWorkPiece intermediateWorkPiece = componentIntermediateWorkPiece.IntermediateWorkPiece;
                simpleOperation = intermediateWorkPiece.SimpleOperation;
                treeNode = new TreeNode (simpleOperation.Display,(int)TreeViewImageIndex.SimpleOperation,(int)TreeViewImageIndex.SimpleOperation);
                treeNode.Name = simpleOperation.IntermediateWorkPieceId.ToString();
                treeNode.Tag = new Tuple<bool, ISimpleOperation> (false, simpleOperation);
                partNode.Nodes.Add(treeNode);
              }
            }
            // assign true to tell that child of this node was already attach
            partNode.Tag = new Tuple<bool, IPart> (true, ((Tuple<bool, IPart>)partNode.Tag).Item2);
          }
        }
      }
    }

    /// <summary>
    /// Return the first path associated to the operation
    /// </summary>
    /// <param name="operation"></param>
    /// <returns></returns>
    IPath GetSinglePath(IOperation operation)
    {
      if (operation.Paths.Count > 1) {
        log.WarnFormat("Operation {0} with more than one path in single path mode",
                       operation.ToString());
      }
      
      IEnumerator<IPath> pathEnumerator = operation.Paths.GetEnumerator();
      if (pathEnumerator.MoveNext()) {
        IPath path = pathEnumerator.Current;
        return path;
      }
      return null;
    }

    IList<TreeNode> AddNodesToSingleOperation(IOperation operation, Type childType)
    {
      IList<TreeNode> childNodes = new List<TreeNode>();
      using (IDAOSession daoSession = ModelDAOHelper.DAOFactory.OpenSession ())
      {
        ModelDAOHelper.DAOFactory.OperationDAO.Lock (operation);
        if (childType.Equals(typeof(ISequence))) {
          // only show first path
          
          IPath path = GetSinglePath(operation);
          
          if (path != null) {
            ModelDAOHelper.DAOFactory.PathDAO.InitializeSequences (path);
            foreach (ISequence sequence in path.Sequences) {
              TreeNode treeNode = new TreeNode(sequence.Display,(int)TreeViewImageIndex.Sequence,(int)TreeViewImageIndex.Sequence);
              treeNode.Name = ((Lemoine.Collections.IDataWithId)sequence).Id.ToString();
              treeNode.Tag = new Tuple<bool, ISequence> (false, sequence);
              childNodes.Add(treeNode);
            }
          }
        }
        else {
          // multi-paths
          foreach (IPath path in operation.Paths) {
            TreeNode treeNode = new TreeNode(path.Display,(int)TreeViewImageIndex.Path,(int)TreeViewImageIndex.Path);
            treeNode.Name = ((Lemoine.Collections.IDataWithId)path).Id.ToString();
            treeNode.Tag = new Tuple<bool, IPath> (false, path);
            childNodes.Add(treeNode);
          }
        }
      }
      return childNodes;
    }

    /// <summary>
    ///   Add child nodes to a node which represents Operation object
    /// </summary>
    /// <param name="operationNodes">List of nodes on which we must add child nodes</param>
    void AddNodesToOperation(IList<TreeNode> operationNodes)
    {
      Type childType = GetChildType (typeof (IOperation));
      using (IDAOSession daoSession = ModelDAOHelper.DAOFactory.OpenSession ()) {
        foreach(TreeNode operationNode in operationNodes) {
          if(!((Tuple<bool, IOperation>)operationNode.Tag).Item1) {
            IOperation operation = ((Tuple<bool, IOperation>)operationNode.Tag).Item2;
            ModelDAOHelper.DAOFactory.OperationDAO.Lock (operation);
            IList<TreeNode> childNodes = AddNodesToSingleOperation (operation, childType);
            foreach(TreeNode childNode in childNodes) {
              operationNode.Nodes.Add(childNode);
            }
            operationNode.Tag = new Tuple<bool, IOperation> (true, ((Tuple<bool, IOperation>)operationNode.Tag).Item2);
          }
        }
      }
    }

    /// <summary>
    ///   Add child nodes to a node which represents SimpleOperation object
    /// </summary>
    /// <param name="simpleOperationNodes">List of nodes on which we must add child nodes</param>
    void AddNodesToSimpleOperation(IList<TreeNode> simpleOperationNodes)
    {
      Type childType = GetChildType (typeof (ISimpleOperation));
      using (IDAOSession daoSession = ModelDAOHelper.DAOFactory.OpenSession ()) {
        foreach(TreeNode simpleOperationNode in simpleOperationNodes) {
          if(!((Tuple<bool, ISimpleOperation>)simpleOperationNode.Tag).Item1) {
            IOperation operation = ((Tuple<bool, ISimpleOperation>)simpleOperationNode.Tag).Item2.Operation;
            IList<TreeNode> childNodes = AddNodesToSingleOperation (operation, childType);
            foreach(TreeNode childNode in childNodes) {
              simpleOperationNode.Nodes.Add(childNode);
            }
            simpleOperationNode.Tag = new Tuple<bool, ISimpleOperation> (true, ((Tuple<bool, ISimpleOperation>)simpleOperationNode.Tag).Item2);
          }
        }
      }
    }

    /// <summary>
    /// Add child nodes to a node which represents Path object
    /// </summary>
    /// <param name="pathNodes">List of nodes on which we must add child nodes</param>
    void AddNodesToPath(IList<TreeNode> pathNodes)
    {
      using (IDAOSession daoSession = ModelDAOHelper.DAOFactory.OpenSession ())
      {
        foreach(TreeNode pathNode in pathNodes) {
          if(!((Tuple<bool, IPath>)pathNode.Tag).Item1) {
            IPath path = ((Tuple<bool, IPath>)pathNode.Tag).Item2;
            ModelDAOHelper.DAOFactory.PathDAO.Lock (path);
            ModelDAOHelper.DAOFactory.PathDAO.InitializeSequences (path);
            foreach (ISequence sequence in path.Sequences) {
              TreeNode treeNode = new TreeNode (sequence.Display,(int)TreeViewImageIndex.Sequence,(int)TreeViewImageIndex.Sequence);
              treeNode.Name = ((Lemoine.Collections.IDataWithId)sequence).Id.ToString();
              treeNode.Tag = new Tuple<bool, ISequence> (false, sequence);
              pathNode.Nodes.Add(treeNode);
            }
          }
          pathNode.Tag = new Tuple<bool, IPath> (true, ((Tuple<bool, IPath>)pathNode.Tag).Item2);
        }
      }
    }
    #endregion

    /// <summary>
    /// Add children to treeNode (use after an expand)
    /// </summary>
    /// <param name="node"></param>
    public virtual void TreeViewAfterExpand(TreeNode node)
    {
      Type type = GetItemType (node);
      
      //for each type of node, if expanded node has child, add node to his children
      if (object.Equals (type, typeof (IWorkOrder))) {  // Selected node represents WorkOrder instance
        TreeViewAfterExpandWorkOrder(node);
      }
      else if (object.Equals (type, typeof (IProject))) {// Selected node represents Project instance
        TreeViewAfterExpandProject(node);
      }
      else if (object.Equals (type, typeof (IJob))) { // Selected node represents Job instance
        TreeViewAfterExpandJob(node);
      }
      else if (object.Equals (type, typeof (Lemoine.Model.IComponent))) {// Selected node represents Component instance
        TreeViewAfterExpandComponent(node);
      }
      else if (object.Equals (type, typeof (IPart))) {// Selected node represents Part instance
        TreeViewAfterExpandPart(node);
      }
      else if (object.Equals (type, typeof (IIntermediateWorkPiece))) {// Selected node represents IntermediateWorkPiece instance
        TreeViewAfterExpandIntermediateWorkPiece(node);
      }
      else if (object.Equals (type, typeof (IOperation))) {
        TreeViewAfterExpandOperation(node);
      }
      else if (object.Equals (type, typeof (ISimpleOperation))) {
        TreeViewAfterExpandSimpleOperation(node);
      }
    }

    #region TreeViewAfterExpandHelper

    /// <summary>
    /// build children for a WorkOrder node
    /// </summary>
    /// <param name="node"></param>
    void TreeViewAfterExpandWorkOrder(TreeNode node)
    {
      if (node.Nodes.Count != 0) {
        Type childType = GetChildType (typeof (IWorkOrder));
        List<TreeNode> nodeList = new List<TreeNode>();
        foreach (TreeNode childNode in node.Nodes) {
          nodeList.Add(childNode);
        }
        if (object.Equals (childType, typeof (IProject))) {
          AddNodesToProject(nodeList);
        }
        else if (object.Equals (childType, typeof (IPart))) {
          AddNodesToPart (nodeList);
        }
      }
    }

    /// <summary>
    /// build children for a Project node
    /// </summary>
    /// <param name="node"></param>
    void TreeViewAfterExpandProject(TreeNode node)
    {
      if (node.Nodes.Count != 0) {
        List<TreeNode> listNode = new List<TreeNode>();
        foreach (TreeNode childNode in node.Nodes) {
          listNode.Add(childNode);
        }
        AddNodesToComponent (listNode);
      }
    }

    /// <summary>
    /// build children for a Job node
    /// </summary>
    /// <param name="node"></param>
    void TreeViewAfterExpandJob(TreeNode node)
    {
      if (node.Nodes.Count != 0) {
        List<TreeNode> listNode = new List<TreeNode>();
        foreach (TreeNode childNode in node.Nodes) {
          listNode.Add(childNode);
        }
        AddNodesToComponent(listNode);
      }
    }

    /// <summary>
    /// build children for a Component node
    /// </summary>
    /// <param name="node"></param>
    void TreeViewAfterExpandComponent(TreeNode node)
    {
      if (node.Nodes.Count != 0) {
        Type childType = GetChildType (typeof (IComponent));
        List<TreeNode> listNode = new List<TreeNode>();
        foreach (TreeNode childNode in node.Nodes) {
          listNode.Add(childNode);
        }
        if (object.Equals (childType, typeof (IIntermediateWorkPiece))) {
          AddNodesToIntermediateWorkPiece(listNode);
        }
        else if (object.Equals (childType, typeof (ISimpleOperation))) {
          AddNodesToSimpleOperation (listNode);
        }
      }
    }

    /// <summary>
    /// build children for a Part node
    /// </summary>
    /// <param name="node"></param>
    void TreeViewAfterExpandPart(TreeNode node)
    {
      if (node.Nodes.Count != 0) {
        Type childType = GetChildType (typeof (IPart));
        List<TreeNode> listNode = new List<TreeNode>();
        foreach (TreeNode childNode in node.Nodes) {
          listNode.Add(childNode);
        }
        if (object.Equals (childType, typeof (IIntermediateWorkPiece))) {
          AddNodesToIntermediateWorkPiece(listNode);
        }
        else if(object.Equals (childType, typeof (ISimpleOperation))) {
          AddNodesToSimpleOperation (listNode);
        }
      }
    }

    /// <summary>
    /// build children for an IntermediateWorkPiece node
    /// </summary>
    /// <param name="node"></param>
    void TreeViewAfterExpandIntermediateWorkPiece(TreeNode node)
    {
      if (node.Nodes.Count != 0) {
        List<TreeNode> listNode = new List<TreeNode>();
        foreach (TreeNode childNode in node.Nodes) {
          listNode.Add(childNode);
        }
        AddNodesToOperation(listNode);
      }
    }

    /// <summary>
    /// build children for an Operation node
    /// </summary>
    /// <param name="treeNode"></param>
    void TreeViewAfterExpandOperation(TreeNode treeNode)
    {
      if (treeNode.Nodes.Count != 0) {
        Type childType = GetChildType (typeof (IOperation));
        List<TreeNode> listNode = new List<TreeNode>();
        foreach (TreeNode childNode in treeNode.Nodes) {
          listNode.Add(childNode);
        }
        if (object.Equals (childType, typeof (IPath))) {
          AddNodesToPath (listNode);
        }
      }
    }

    /// <summary>
    /// build children for an SimpleOperation node
    /// </summary>
    /// <param name="treeNode"></param>
    void TreeViewAfterExpandSimpleOperation(TreeNode treeNode)
    {
      if (treeNode.Nodes.Count != 0) {
        Type childType = GetChildType (typeof (ISimpleOperation));
        List<TreeNode> listNode = new List<TreeNode>();
        foreach (TreeNode childNode in treeNode.Nodes) {
          listNode.Add(childNode);
        }
        if (object.Equals (childType, typeof (IPath))) {
          AddNodesToPath (listNode);
        }
      }
    }
    #endregion // TreeViewAfterExpandHelper

    /// <summary>
    /// Rebuild nodes with a given key among treeNodeList
    /// (nodes and their children are rebuilt)
    /// </summary>
    /// <param name="key"></param>
    /// <param name="treeNodeList"></param>
    /// <param name="type"></param>
    public virtual void PopulateNodes(String key, IList<TreeNode> treeNodeList, Type type)
    {
      if (type.Equals(typeof(IWorkOrder))) {
        PopulateWorkOrderNode(key, treeNodeList);
      }
      else if (type.Equals(typeof(IProject))) {
        PopulateProject(key, treeNodeList);
      }
      else if (type.Equals(typeof(Lemoine.Model.IComponent))) {
        PopulateComponent(key, treeNodeList);
      }
      else if (type.Equals(typeof(IIntermediateWorkPiece))) {
        PopulateIntermediateWorkPiece(key, treeNodeList);
      }
      else if (type.Equals(typeof(IOperation))) {
        PopulateOperation(key, treeNodeList);
      }
      else if (type.Equals(typeof(IJob))) {
        PopulateJob(key, treeNodeList);
      }
      else if (type.Equals(typeof(IPart))) {
        PopulatePart(key, treeNodeList);
      }
      else if (type.Equals(typeof(ISimpleOperation))) {
        PopulateSimpleOperation(key, treeNodeList);
      }
      else if (type.Equals(typeof(ISequence))) {
        ReloadSequence(key, treeNodeList);
      }
      else if (type.Equals(typeof(IPath))) {
        PopulatePath(key, treeNodeList);
      }
    }

    /// <summary>
    /// Rebuild nodes with a given key among treeNodeList
    /// (children are not rebuilt)
    /// </summary>
    /// <param name="key"></param>
    /// <param name="treeNodeList"></param>
    /// <param name="type"></param>
    public virtual void ReloadNodes(String key, IList<TreeNode> treeNodeList, Type type)
    {
      if (type.Equals(typeof(IWorkOrder))) {
        ReloadWorkOrderNode(key, treeNodeList);
      }
      else if (type.Equals(typeof(IProject))) {
        ReloadProject(key, treeNodeList);
      }
      else if (type.Equals(typeof(Lemoine.Model.IComponent))) {
        ReloadComponent(key, treeNodeList);
      }
      else if (type.Equals(typeof(IIntermediateWorkPiece))) {
        ReloadIntermediateWorkPiece(key, treeNodeList);
      }
      else if (type.Equals(typeof(IOperation))) {
        ReloadOperation(key, treeNodeList);
      }
      else if (type.Equals(typeof(IJob))) {
        ReloadJob(key, treeNodeList);
      }
      else if (type.Equals(typeof(IPart))) {
        ReloadPart(key, treeNodeList);
      }
      else if (type.Equals(typeof(ISimpleOperation))) {
        ReloadSimpleOperation(key, treeNodeList);
      }
      else if (type.Equals(typeof(ISequence))) {
        ReloadSequence(key, treeNodeList);
      }
      else if (type.Equals(typeof(IPath))) {
        ReloadPath(key, treeNodeList);
      }
    }

    #region PopulateNodesHelper
    void ReloadWorkOrderNode(String key, IList<TreeNode> treeNodeList)
    {
      IWorkOrder workOrder = null;
      bool first = true;
      foreach (TreeNode treeNode in treeNodeList) {
        if (first) {
          workOrder = ((Tuple<bool, IWorkOrder>)treeNode.Tag).Item2;
          using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
          {
            workOrder = ModelDAOHelper.DAOFactory.WorkOrderDAO.Reload (workOrder);
          }
          first = false;
        }
        treeNode.Tag = new Tuple<bool, IWorkOrder> (false, workOrder);
        treeNode.Text = workOrder.Display;
      }
    }

    void PopulateWorkOrderNode(String key, IList<TreeNode> treeNodeList)
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
      {
        ReloadWorkOrderNode(key, treeNodeList);
        AddNodesToWorkOrder (treeNodeList);
      }
    }


    void ReloadProject(String key, IList<TreeNode> treeNodeList)
    {
      IProject project = null;
      bool first = true;
      foreach (TreeNode treeNode in treeNodeList) {
        if (first) {
          project = ((Tuple<bool, IProject>)treeNode.Tag).Item2;
          using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
          {
            project = ModelDAOHelper.DAOFactory.ProjectDAO.Reload (project);
          }
          first = false;
        }
        treeNode.Tag = new Tuple<bool, IProject> (false, project);
        treeNode.Text = project.Display;
      }
    }

    void PopulateProject(String key, IList<TreeNode> treeNodeList)
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
      {
        ReloadProject(key, treeNodeList);
        AddNodesToProject(treeNodeList);
      }
    }

    void ReloadComponent(String key, IList<TreeNode> treeNodeList)
    {
      Lemoine.Model.IComponent component = null;
      bool first = true;
      foreach (TreeNode treeNode in treeNodeList) {
        if (first) {
          component = ((Tuple<bool, IComponent>)treeNode.Tag).Item2;
          using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
          {
            component = ModelDAOHelper.DAOFactory.ComponentDAO.Reload (component);
          }
          first = false;
        }
        treeNode.Tag = new Tuple<bool, IComponent> (false, component);
        treeNode.Text = component.Display;
      }
    }

    void PopulateComponent(String key, IList<TreeNode> treeNodeList)
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
      {
        ReloadComponent(key, treeNodeList);
        AddNodesToComponent(treeNodeList);
      }
    }

    void ReloadIntermediateWorkPiece(String key, IList<TreeNode> treeNodeList)
    {
      IIntermediateWorkPiece intermediateWorkPiece = null;
      bool first = true;
      foreach (TreeNode treeNode in treeNodeList) {
        if (first) {
          intermediateWorkPiece = ((Tuple<bool, IIntermediateWorkPiece>)treeNode.Tag).Item2;
          using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
          {
            intermediateWorkPiece = ModelDAOHelper.DAOFactory.IntermediateWorkPieceDAO.Reload (intermediateWorkPiece);
          }
          first = false;
        }
        treeNode.Tag = new Tuple<bool, IIntermediateWorkPiece> (false, intermediateWorkPiece);
        treeNode.Text = intermediateWorkPiece.Display;
      }
    }

    void PopulateIntermediateWorkPiece(String key, IList<TreeNode> treeNodeList)
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
      {
        ReloadIntermediateWorkPiece(key, treeNodeList);
        AddNodesToIntermediateWorkPiece(treeNodeList);
      }
    }

    void ReloadOperation(String key, IList<TreeNode> treeNodeList)
    {
      IOperation operation = null;
      bool first = true;
      foreach (TreeNode treeNode in treeNodeList) {
        if (first) {
          operation = ((Tuple<bool, IOperation>)treeNode.Tag).Item2;
          using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
          {
            operation = ModelDAOHelper.DAOFactory.OperationDAO.Reload (operation);
          }
          first = false;
        }
        treeNode.Tag = new Tuple<bool, IOperation> (false, operation);
        treeNode.Text = operation.Display;
      }
    }

    void PopulateOperation(String key, IList<TreeNode> treeNodeList)
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
      {
        ReloadOperation (key, treeNodeList);
        AddNodesToOperation (treeNodeList);
      }
    }

    void ReloadJob(String key, IList<TreeNode> treeNodeList)
    {
      IJob job = null;
      bool first = true;
      foreach (TreeNode treeNode in treeNodeList) {
        if (first) {
          job = ((Tuple<bool, IJob>)treeNode.Tag).Item2;
          using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
          {
            job = ModelDAOHelper.DAOFactory.JobDAO.Reload (job);
          }
          first = false;
        }
        treeNode.Tag = new Tuple<bool, IJob> (false, job);
        treeNode.Text = job.Display;
      }
    }

    void PopulateJob(String key, IList<TreeNode> treeNodeList)
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
      {
        ReloadJob(key, treeNodeList);
        AddNodesToJob(treeNodeList);
      }
    }

    void ReloadPart(String key, IList<TreeNode> treeNodeList)
    {
      IPart part = null;
      bool first = true;
      foreach (TreeNode treeNode in treeNodeList) {
        if (first) {
          part = ((Tuple<bool, IPart>)treeNode.Tag).Item2;
          using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
          {
            part = ModelDAOHelper.DAOFactory.PartDAO.Reload (part);
          }
          first = false;
        }
        treeNode.Tag = new Tuple<bool, IPart> (false, part);
        treeNode.Text = part.Display;
      }
    }

    void PopulatePart(String key, IList<TreeNode> treeNodeList)
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
      {
        ReloadPart(key, treeNodeList);
        AddNodesToPart (treeNodeList);
      }
    }

    void ReloadSimpleOperation(String key, IList<TreeNode> treeNodeList)
    {
      ISimpleOperation simpleOperation = null;
      bool first = true;
      foreach (TreeNode treeNode in treeNodeList) {
        if (first) {
          simpleOperation = ((Tuple<bool, ISimpleOperation>)treeNode.Tag).Item2;
          using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
          {
            simpleOperation = ModelDAOHelper.DAOFactory.SimpleOperationDAO.Reload (simpleOperation);
          }
          first = false;
        }
        treeNode.Tag = new Tuple<bool, ISimpleOperation> (false, simpleOperation);
        treeNode.Text = simpleOperation.Display;
      }
    }

    void PopulateSimpleOperation(String key, IList<TreeNode> treeNodeList)
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
      {
        ReloadSimpleOperation(key, treeNodeList);
        AddNodesToSimpleOperation (treeNodeList);
      }
    }

    void ReloadSequence (String key, IList<TreeNode> treeNodeList)
    {
      ISequence sequence = null;
      bool first = true;
      foreach (TreeNode treeNode in treeNodeList) {
        if (first) {
          sequence = ((Tuple<bool, ISequence>)treeNode.Tag).Item2;
          using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
          {
            sequence = ModelDAOHelper.DAOFactory.SequenceDAO.Reload (sequence);
          }
          first = false;
        }
        treeNode.Tag = new Tuple<bool, ISequence> (false, sequence);
        treeNode.Text = sequence.Display;
      }
    }

    void ReloadPath (String key, IList<TreeNode> treeNodeList)
    {
      IPath path = null;
      bool first = true;
      foreach (TreeNode treeNode in treeNodeList) {
        if (first) {
          path = ((Tuple<bool, IPath>)treeNode.Tag).Item2;
          using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
          {
            path = ModelDAOHelper.DAOFactory.PathDAO.Reload (path);
          }
          first = false;
        }
        treeNode.Tag = new Tuple<bool, IPath> (false, path);
        treeNode.Text = path.Display;
      }
    }

    void PopulatePath(String key, IList<TreeNode> treeNodeList)
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
      {
        ReloadPath (key, treeNodeList);
        AddNodesToPath (treeNodeList);
      }
    }
    #endregion // PopulateNodesHelper
  }
}
