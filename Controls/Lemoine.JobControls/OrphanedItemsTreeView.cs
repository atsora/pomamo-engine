// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;

using Lemoine.I18N;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Collections;

using Lemoine.Core.Log;

namespace Lemoine.JobControls
{
  /// <summary>
  /// Description of OrphanedItemsTreeView.
  /// </summary>
  public partial class OrphanedItemsTreeView : BaseOperationTreeView, ITreeViewObservable
  {
    #region members
    /// <summary>
    ///   List of control which are impacted by change of treeview;
    ///   Main change is node selection
    /// </summary>
    List<ITreeViewObserver> m_observers = new List<ITreeViewObserver> ();

    /// <summary>
    ///   Associated OperationTreeView
    /// </summary>
    OperationTreeView m_operationTreeView;

    TreeNode m_projectTopNode;
    TreeNode m_intermediateWorkPieceTopNode;
    TreeNode m_partTopNode;
    TreeNode m_simpleOperationTopNode;
    #endregion

    static readonly ILog log = LogManager.GetLogger (typeof (OrphanedItemsTreeView).FullName);

    #region Getters / Setters

    /// <summary>
    /// Get wrapped treeview
    /// </summary>
    public override TreeView TreeView
    {
      get {
        return m_treeView;
      }
    }

    /// <summary>
    /// 
    /// </summary>
    public OperationTreeView OperationTreeView
    {
      get {
        return m_operationTreeView;
      }
      set {
        m_operationTreeView = value;
      }
    }

    #endregion // Getters / Setters


    #region Constructors

    /// <summary>
    /// Description of the constructor
    /// </summary>
    public OrphanedItemsTreeView ()
    {
      //
      // The InitializeComponent() call is required for Windows Forms designer support.
      //
      InitializeComponent ();
      this.TreeView.TreeViewNodeSorter = new OperationTreeView.OperationTreeViewNodeSorter ();
    }

    #endregion // Constructors


    #region Init Methods

    /// <summary>
    ///   Initialize OrphanedItemsTreeView which is associated
    ///   with given OperationTreeView
    /// </summary>
    /// <param name="operationTreeView">Associated OperationTreeView</param>
    public void Init (OperationTreeView operationTreeView)
    {
      Type type;
      int level = operationTreeView.Level;

      this.OperationTreeView = operationTreeView;

      Debug.Assert (operationTreeView.TreeView.TreeViewNodeSorter.GetType ().Equals (typeof (OperationTreeView.OperationTreeViewNodeSorter)));
      for (int i = 0; i <= level - 1; i++) {
        type = operationTreeView.LevelToType[i];
        if (type == null) {
          continue;
        }
        else if (type.Equals (typeof (IProject))) {
          m_projectTopNode = new TreeNode (PulseCatalog.GetString ("Project"), (int)TreeViewImageIndex.Project, (int)TreeViewImageIndex.Project);
          m_projectTopNode.Name = "Project";
          m_projectTopNode.Tag = typeof (IProject);
          TreeView.Nodes.Add (m_projectTopNode);
          InitProjectNodes (m_projectTopNode);
        }
        else if ((!operationTreeView.PartAtTheTop) && (type.Equals (typeof (IPart)))) {
          m_partTopNode = new TreeNode (PulseCatalog.GetString ("Part"), (int)TreeViewImageIndex.Part, (int)TreeViewImageIndex.Part);
          m_partTopNode.Name = "Part";
          m_partTopNode.Tag = typeof (IPart);
          TreeView.Nodes.Add (m_partTopNode);
          InitPartNodes (m_partTopNode);
        }
        else if (type.Equals (typeof (IIntermediateWorkPiece))) {
          m_intermediateWorkPieceTopNode = new TreeNode (PulseCatalog.GetString ("IntermediateWorkPiece"), (int)TreeViewImageIndex.IntermediateWorkPiece, (int)TreeViewImageIndex.IntermediateWorkPiece);
          m_intermediateWorkPieceTopNode.Name = "IntermediateWorkPiece";
          m_intermediateWorkPieceTopNode.Tag = typeof (IIntermediateWorkPiece);
          TreeView.Nodes.Add (m_intermediateWorkPieceTopNode);
          InitIntermediateWorkPieceNodes (m_intermediateWorkPieceTopNode);
        }
        else if (type.Equals (typeof (ISimpleOperation))) {
          m_simpleOperationTopNode = new TreeNode (PulseCatalog.GetString ("SimpleOperation"), (int)TreeViewImageIndex.SimpleOperation, (int)TreeViewImageIndex.SimpleOperation);
          m_simpleOperationTopNode.Name = "SimpleOperation";
          m_simpleOperationTopNode.Tag = typeof (ISimpleOperation);
          TreeView.Nodes.Add (m_simpleOperationTopNode);
          InitSimpleOperationNodes (m_simpleOperationTopNode);
        }
      }
      // Add child to each Project node
      List<TreeNode> listNodes = new List<TreeNode> ();
      if (TreeView.Nodes.Find ("Project", false).Length > 0) {
        foreach (TreeNode treeNode in TreeView.Nodes.Find ("Project", false)[0].Nodes) {
          listNodes.Add (treeNode);
        }
        AddNodesToProject (listNodes);
      }
      // Add child to each IntermediateWorkPiece node
      listNodes.Clear ();
      if (TreeView.Nodes.Find ("IntermediateWorkPiece", false).Length > 0) {
        foreach (TreeNode treeNode in TreeView.Nodes.Find ("IntermediateWorkPiece", false)[0].Nodes) {
          listNodes.Add (treeNode);
        }
        AddNodesToIntermediateWorkPiece (listNodes);
      }
      // Add child to each Part node
      listNodes.Clear ();
      if (TreeView.Nodes.Find ("Part", false).Length > 0) {
        foreach (TreeNode treeNode in TreeView.Nodes.Find ("Part", false)[0].Nodes) {
          listNodes.Add (treeNode);
        }
        AddNodesToPart (listNodes);
      }

      this.TreeView.ItemDrag += new System.Windows.Forms.ItemDragEventHandler (operationTreeView.TreeViewItemDrag);
      this.m_treeView.DragEnter += new System.Windows.Forms.DragEventHandler (operationTreeView.TreeViewDragEnter);
      this.m_treeView.DragLeave += new System.EventHandler (operationTreeView.TreeViewDragLeave);
      this.m_treeView.DragOver += new System.Windows.Forms.DragEventHandler (operationTreeView.TreeViewDragOver);
    }

    private void InitProjectNodes (TreeNode rootNode)
    {
      using (IDAOSession daoSession = ModelDAOHelper.DAOFactory.OpenSession ()) {
        IList<IProject> projects = ModelDAOHelper.DAOFactory.ProjectDAO
          .GetOrphans ();
        foreach (IProject project in projects) {
          TreeNode childNode = new TreeNode (project.Display, (int)TreeViewImageIndex.Project, (int)TreeViewImageIndex.Project);
          childNode.Name = ((Lemoine.Collections.IDataWithId<int>)project).Id.ToString ();
          childNode.Tag = new Tuple<bool, IProject> (false, project);
          rootNode.Nodes.Add (childNode);
        }
      }
    }

    private void InitPartNodes (TreeNode rootNode)
    {
      using (IDAOSession daoSession = ModelDAOHelper.DAOFactory.OpenSession ()) {
        IList<IProject> projects = ModelDAOHelper.DAOFactory.ProjectDAO.GetOrphans ();
        foreach (IProject project in projects) {
          if (project.Components.Count == 0) {
            log.Warn ("Part build : There is a project without link with any Component.  " + " Id: " + ((Lemoine.Collections.IDataWithId)project).Id + " - Name: " + project.Name);
            continue;
          }
          TreeNode childNode = new TreeNode (project.Part.Display, (int)TreeViewImageIndex.Part, (int)TreeViewImageIndex.Part);
          childNode.Name = ((Lemoine.Collections.IDataWithId)project).Id.ToString ();
          childNode.Tag = new Tuple<bool, IPart> (false, project.Part);
          rootNode.Nodes.Add (childNode);
        }
      }
    }

    private void InitIntermediateWorkPieceNodes (TreeNode rootNode)
    {
      using (IDAOSession daoSession = ModelDAOHelper.DAOFactory.OpenSession ()) {
        IList<IIntermediateWorkPiece> intermediateWorkPieces = ModelDAOHelper.DAOFactory.IntermediateWorkPieceDAO.GetOrphans ();
        foreach (IIntermediateWorkPiece intermediateWorkPiece in intermediateWorkPieces) {
          TreeNode childNode = new TreeNode (intermediateWorkPiece.Display, (int)TreeViewImageIndex.IntermediateWorkPiece, (int)TreeViewImageIndex.IntermediateWorkPiece);
          childNode.Name = ((Lemoine.Collections.IDataWithId)intermediateWorkPiece).Id.ToString ();
          childNode.Tag = new Tuple<bool, IIntermediateWorkPiece> (false, intermediateWorkPiece);
          rootNode.Nodes.Add (childNode);
        }
      }
    }

    private void InitSimpleOperationNodes (TreeNode rootNode)
    {
      using (IDAOSession daoSession = ModelDAOHelper.DAOFactory.OpenSession ()) {
        IList<IIntermediateWorkPiece> intermediateWorkPieces = ModelDAOHelper.DAOFactory.IntermediateWorkPieceDAO.GetOrphans ();
        foreach (IIntermediateWorkPiece intermediateWorkPiece in intermediateWorkPieces) {
          TreeNode childNode = new TreeNode (intermediateWorkPiece.SimpleOperation.Display, (int)TreeViewImageIndex.SimpleOperation, (int)TreeViewImageIndex.SimpleOperation);
          childNode.Name = ((Lemoine.Collections.IDataWithId)intermediateWorkPiece).Id.ToString ();
          childNode.Tag = new Tuple<bool, ISimpleOperation> (false, intermediateWorkPiece.SimpleOperation);
          rootNode.Nodes.Add (childNode);
        }
      }
    }

    #endregion // Methods


    #region inherited methods
    /// <summary>
    /// Get the level of a type
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public override int GetTypeLevel (Type type)
    {
      return this.OperationTreeView.GetTypeLevel (type);
    }

    /// <summary>
    /// Get the type of a child
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public override Type GetChildType (Type type)
    {
      return this.OperationTreeView.GetChildType (type);
    }

    /// <summary>
    /// Get the type of a parent
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public override Type GetParentType (Type type)
    {
      return this.OperationTreeView.GetParentType (type);
    }

    /// <summary>
    /// Get selected node of wrapped treeview
    /// </summary>
    /// <returns></returns>
    public override TreeNode GetSelectedNode ()
    {
      return TreeView.SelectedNode;
    }

    /// <summary>
    /// Initialize state of wrapped treeview
    /// </summary>
    public override void Init ()
    {
    }

    /// <summary>
    /// Update nodes with the same type and name and reload their children
    /// </summary>
    /// <param name="type"></param>
    /// <param name="name"></param>
    protected override void BuildTreeNodes (Type type, string name)
    {
      // TODO: implement it
      throw new NotImplementedException ();
    }

    /// <summary>
    /// Select the specified nodes
    /// </summary>
    /// <param name="nodeArray"></param>
    public override void SelectNodes (TreeNode[] nodeArray)
    {
      if (0 == nodeArray.Length) {
        log.WarnFormat ("SelectNodes: " +
                        "empty nodeArray");
        return;
      }
      else if (1 < nodeArray.Length) {
        log.WarnFormat ("SelectNodes: " +
                        "more than one node in the specified array, " +
                        "this is not supported in OrphanedItemsTreeView");
      }
      this.TreeView.SelectedNode = nodeArray[0];
    }

    /// <summary>
    /// Get the data of a tree node
    /// </summary>
    /// <param name="node"></param>
    /// <returns></returns>
    public override Lemoine.Collections.IDataWithId GetTreeNodeData (TreeNode node)
    {
      return ((Tuple<bool, Lemoine.Collections.IDataWithId>)node.Tag).Item2;
    }

    /// <summary>
    /// Get the type of a tree node
    /// </summary>
    /// <param name="node"></param>
    /// <returns></returns>
    public override Type GetTreeNodeType (TreeNode node)
    {
      if (0 == node.Level) { // Top node
        return (Type)node.Tag;
      }
      else if (1 == node.Level) {
        Debug.Assert (null != node.Parent);
        return GetTreeNodeType (node.Parent);
      }
      else {
        Debug.Assert (null != node.Parent);
        return GetChildType (GetTreeNodeType (node.Parent));
      }
    }
    #endregion

    /// <summary>
    ///   Insert TreeNode that represents orphaned item
    /// </summary>
    /// <param name="treeNode"></param>
    public void InsertNode (TreeNode treeNode)
    {
      TreeNode root = null;
      if (treeNode.Tag is Tuple<bool, IProject>) {
        root = TreeView.Nodes.Find ("Project", false)[0];
      }
      else if (treeNode.Tag is Tuple<bool, IPart>) {
        root = TreeView.Nodes.Find ("Part", false)[0];
      }
      else if (treeNode.Tag is Tuple<bool, IIntermediateWorkPiece>) {
        root = TreeView.Nodes.Find ("IntermediateWorkPiece", false)[0];
      }
      else if (treeNode.Tag is Tuple<bool, ISimpleOperation>) {
        root = TreeView.Nodes.Find ("SimpleOperation", false)[0];
      }
      if (root != null) {
        root.Nodes.Add (treeNode);
        TreeView.SelectedNode = treeNode;
      }
    }

    void TreeViewAfterExpand (object sender, TreeViewEventArgs e)
    {
      if (e.Node != null) {
        //Get node that has been expanded
        TreeNode treeNode = e.Node;
        Cursor.Current = Cursors.WaitCursor;
        TreeView.BeginUpdate ();
        TreeViewAfterExpand (treeNode);
        TreeView.EndUpdate ();
        Cursor.Current = Cursors.Default;
        // TreeView.SelectedNode = treeNode;
      }
    }

    void TreeViewAfterSelect (object sender, TreeViewEventArgs e)
    {
      NotifyObservers ();
    }


    void TreeViewMouseDown (object sender, MouseEventArgs e)
    {
      TreeViewHitTestInfo treeViewHitTestInfo = null;
      treeViewHitTestInfo = this.TreeView.HitTest (e.Location);
      if (treeViewHitTestInfo.Location == TreeViewHitTestLocations.RightOfLabel) {
        this.TreeView.SelectedNode = treeViewHitTestInfo.Node;
      }
    }

    /// <summary>
    /// Return the item that is associated to a node
    /// </summary>
    /// <param name="node"></param>
    /// <returns></returns>
    public override object GetItem (TreeNode node)
    {
      return node.Tag;
    }

  }
}
