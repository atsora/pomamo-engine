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
  /// Description of TreeViewBasicsOperations.
  /// </summary>
  internal class TreeViewBasicsOperations
  {
    static readonly ILog log = LogManager.GetLogger (typeof (TreeViewBasicsOperations).FullName);

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public TreeViewBasicsOperations ()
    {
    }

    #endregion // Constructors


    #region MoveNode Methods
    /// <summary>
    ///   Move dragged node from his parent node to a target node
    ///   dragged node is cut from its parent and add in the son of target node.
    ///   Return true if moving succeed, false otherwise
    /// </summary>
    /// <param name="dragTreeView"></param>
    /// <param name="dragNode">Dragged node</param>
    /// <param name="dropTreeView"></param>
    /// <param name="dropNode">Drop node</param>
    /// <param name="levelToType">Array of type contained in TreeView for each level</param>
    /// <returns>True if move has succeeded, false otherwise</returns>
    public static bool MoveNode (BaseOperationTreeView dragTreeView, TreeNode dragNode,
                                BaseOperationTreeView dropTreeView, TreeNode dropNode, Type[] levelToType)
    {
      Debug.Assert (null != dropNode);
      Debug.Assert (null != dragNode);

      TreeNode parentDragNode = dragNode.Parent;
      Debug.Assert (null != parentDragNode);

      bool result = false;
      try {
        if (dropNode.Tag is Tuple<bool, IWorkOrder>) {
          result = MoveNodeOnWorkOrder (dragTreeView, dragNode, parentDragNode, dropTreeView, dropNode);
        }
        else if (dropNode.Tag is Tuple<bool, IProject>) {
          result = MoveNodeOnProject (dragTreeView, dragNode, parentDragNode, dropTreeView, dropNode);
        }
        else if (dropNode.Tag is Tuple<bool, IJob>) {
          result = MoveNodeOnJob (dragTreeView, dragNode, parentDragNode, dropTreeView, dropNode);
        }
        else if (dropNode.Tag is Tuple<bool, IComponent>) {
          result = MoveNodeOnComponent (dragTreeView, dragNode, parentDragNode, dropTreeView, dropNode);
        }
        else if (dropNode.Tag is Tuple<bool, IPart>) {
          result = MoveNodeOnPart (dragTreeView, dragNode, parentDragNode, dropTreeView, dropNode);
        }
        else if (dropNode.Tag is Tuple<bool, IIntermediateWorkPiece>) {
          result = MoveNodeOnIntermediateWorkPiece (dragTreeView, dragNode, parentDragNode, dropTreeView, dropNode);
        }
        if (result) {
          dragTreeView.UpdateTreeNode (parentDragNode);
          dropTreeView.UpdateTreeNode (dropNode);
        }
        return result;
      }
      catch (Exception e) {
        Cursor.Current = Cursors.Default;
        MessageBox.Show ("Exception : \n" + e.Message + "\n" + e.StackTrace, "", MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
        return false;
      }
    }

    /// <summary>
    ///   Call when drop node represents WorkOrder
    /// </summary>
    /// <param name="dragTreeView"></param>
    /// <param name="dragNode">Dragged node</param>
    /// <param name="parentDragNode">parent of dragged node before move</param>
    /// <param name="dropTreeView"></param>
    /// <param name="dropNode">Drop node</param>
    /// <returns>True if move has succeeded, false otherwise</returns>
    private static bool MoveNodeOnWorkOrder (BaseOperationTreeView dragTreeView, TreeNode dragNode, TreeNode parentDragNode,
                                            BaseOperationTreeView dropTreeView, TreeNode dropNode)
    {
      IWorkOrder targetWorkOrder;
      IWorkOrder formerWorkOrder;
      IProject project = null;

      Cursor.Current = Cursors.WaitCursor;
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      using (IDAOSession daoSession = daoFactory.OpenSession ()) {
        if (dragNode.Tag is Tuple<bool, IProject>) {
          project = ((Tuple<bool, IProject>)dragNode.Tag).Item2;
        }
        else if (dragNode.Tag is Tuple<bool, IPart>) {
          project = ((Tuple<bool, IPart>)dragNode.Tag).Item2.Project;
        }
        ModelDAOHelper.DAOFactory.ProjectDAO.Lock (project);
        targetWorkOrder = ((Tuple<bool, IWorkOrder>)dropNode.Tag).Item2;
        ModelDAOHelper.DAOFactory.WorkOrderDAO.Lock (targetWorkOrder);
        formerWorkOrder = ((Tuple<bool, IWorkOrder>)parentDragNode.Tag).Item2;
        ModelDAOHelper.DAOFactory.WorkOrderDAO.Lock (formerWorkOrder);
        ModelDAOHelper.DAOFactory.WorkOrderDAO.InitializeProjects (targetWorkOrder);

        foreach (IProject projectInTargetWorkOrder in targetWorkOrder.Projects) {
          if (((Lemoine.Collections.IDataWithId<int>)projectInTargetWorkOrder).Id == ((Lemoine.Collections.IDataWithId)project).Id) {
            Cursor.Current = Cursors.Default;
            MessageBox.Show (PulseCatalog.GetString ("DraggedNodeAlreadyExistsInTheTargetNode"), "", MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
            return false;
          }
        }

        using (IDAOTransaction tx = daoSession.BeginTransaction ()) {
          project.RemoveWorkOrder (formerWorkOrder);
          project.AddWorkOrder (targetWorkOrder);
          DateTime dateTime = DateTime.Now.ToUniversalTime ();
          IWorkOrderProjectUpdate updateRemove = ModelDAOHelper.ModelFactory.CreateWorkOrderProjectUpdate (formerWorkOrder, project, WorkOrderProjectUpdateModificationType.DELETE);
          updateRemove.DateTime = dateTime;
          IWorkOrderProjectUpdate updateAdd = ModelDAOHelper.ModelFactory.CreateWorkOrderProjectUpdate (targetWorkOrder, project, WorkOrderProjectUpdateModificationType.NEW);
          updateAdd.DateTime = dateTime;
          daoFactory.WorkOrderProjectUpdateDAO.MakePersistent (updateRemove);
          daoFactory.WorkOrderProjectUpdateDAO.MakePersistent (updateAdd);
          daoFactory.ProjectDAO.MakePersistent (project);
          daoFactory.WorkOrderDAO.MakePersistent (targetWorkOrder);
          daoFactory.WorkOrderDAO.MakePersistent (formerWorkOrder);
          tx.Commit ();
        }
      }
      Cursor.Current = Cursors.Default;
      return true;
    }

    /// <summary>
    ///   Call when drop node represents Project
    /// </summary>
    /// <param name="dragTreeView"></param>
    /// <param name="dragNode">Dragged node</param>
    /// <param name="parentDragNode">parent of dragged node before move</param>
    /// <param name="dropTreeView"></param>
    /// <param name="dropNode">Drop node</param>
    /// <returns>True if move has succeeded, false otherwise</returns>
    private static bool MoveNodeOnProject (BaseOperationTreeView dragTreeView, TreeNode dragNode, TreeNode parentDragNode,
                                          BaseOperationTreeView dropTreeView, TreeNode dropNode)
    {
      IProject targetProject;
      IComponent component;

      Cursor.Current = Cursors.WaitCursor;
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      using (IDAOSession daoSession = daoFactory.OpenSession ()) {
        targetProject = ((Tuple<bool, IProject>)dropNode.Tag).Item2;
        ModelDAOHelper.DAOFactory.ProjectDAO.Lock (targetProject);
        component = ((Tuple<bool, IComponent>)dragNode.Tag).Item2;
        ModelDAOHelper.DAOFactory.ComponentDAO.Lock (component);

        foreach (var componentInTargetProject in targetProject.Components) {
          if (((Lemoine.Collections.IDataWithId)componentInTargetProject).Id == ((Lemoine.Collections.IDataWithId)component).Id) {
            Cursor.Current = Cursors.Default;
            MessageBox.Show (PulseCatalog.GetString ("DraggedNodeAlreadyExistsInTheTargetNode"), "", MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
            return false;
          }
          if ((componentInTargetProject.Code != null) && (componentInTargetProject.Code == component.Code)) {
            Cursor.Current = Cursors.Default;
            MessageBox.Show (PulseCatalog.GetString ("ThereIsAnotherItemWithSameNameThatBelongsToThisProject") + "\n" + PulseCatalog.GetString ("YouCanNotMoveThisItem"), "", MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
            return false;
          }
          if ((componentInTargetProject.Name != null) && (componentInTargetProject.Name == component.Name)) {
            Cursor.Current = Cursors.Default;
            MessageBox.Show (PulseCatalog.GetString ("ThereIsAnotherItemWithSameNameThatBelongsToThisProject") + "\n" + PulseCatalog.GetString ("YouCanNotMoveThisItem"), "", MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
            return false;
          }
        }

        using (IDAOTransaction tx = daoSession.BeginTransaction ()) {
          ModelDAOHelper.DAOFactory.ComponentDAO.ChangeProject (component, targetProject);
          tx.Commit ();
        }
      }
      Cursor.Current = Cursors.Default;
      return true;
    }

    /// <summary>
    ///   Call when drop node represents Job
    /// </summary>
    /// <param name="dragTreeView"></param>
    /// <param name="dragNode">Dragged node</param>
    /// <param name="parentDragNode">parent of dragged node before move</param>
    /// <param name="dropTreeView"></param>
    /// <param name="dropNode">Drop node</param>
    /// <returns>True if move has succeeded, false otherwise</returns>
    private static bool MoveNodeOnJob (BaseOperationTreeView dragTreeView, TreeNode dragNode, TreeNode parentDragNode,
                                      BaseOperationTreeView dropTreeView, TreeNode dropNode)
    {
      IProject targetProject;
      IProject formerProject;
      IComponent component;

      Cursor.Current = Cursors.WaitCursor;
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      using (IDAOSession daoSession = daoFactory.OpenSession ()) {
        targetProject = ((Tuple<bool, IJob>)dropNode.Tag).Item2.Project;
        ModelDAOHelper.DAOFactory.ProjectDAO.Lock (targetProject);
        formerProject = ((Tuple<bool, IJob>)parentDragNode.Tag).Item2.Project;
        ModelDAOHelper.DAOFactory.ProjectDAO.Lock (formerProject);
        component = ((Tuple<bool, IComponent>)dragNode.Tag).Item2;
        ModelDAOHelper.DAOFactory.ComponentDAO.Lock (component);
        ModelDAOHelper.DAOFactory.ProjectDAO.InitializeComponents (targetProject);

        foreach (var componentInTarget in targetProject.Components) {
          if (((Lemoine.Collections.IDataWithId)componentInTarget).Id == ((Lemoine.Collections.IDataWithId)component).Id) {
            Cursor.Current = Cursors.Default;
            MessageBox.Show (PulseCatalog.GetString ("DraggedNodeAlreadyExistsInTheTargetNode"), "", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
          }
          if ((componentInTarget.Code != null) && (componentInTarget.Code == component.Code)) {
            Cursor.Current = Cursors.Default;
            MessageBox.Show (PulseCatalog.GetString ("ThereIsAnotherItemWithSameCodeThatBelongsToThisJob") + "\n" + PulseCatalog.GetString ("YouCanNotMoveThisItem"), "", MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
            return false;
          }
          if ((componentInTarget.Name != null) && (componentInTarget.Name == component.Name)) {
            Cursor.Current = Cursors.Default;
            MessageBox.Show (PulseCatalog.GetString ("ThereIsAnotherItemWithSameNameThatBelongsToThisJob") + "\n" + PulseCatalog.GetString ("YouCanNotMoveThisItem"), "", MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
            return false;
          }
        }

        using (IDAOTransaction tx = daoSession.BeginTransaction ()) {
          DateTime dateTime = DateTime.Now.ToUniversalTime ();
          IProjectComponentUpdate update = ModelDAOHelper.ModelFactory.CreateProjectComponentUpdate (component, formerProject, targetProject);
          update.DateTime = dateTime;
          formerProject.RemoveComponent (component);
          targetProject.AddComponent (component);
          daoFactory.ProjectDAO.MakePersistent (targetProject);
          daoFactory.ProjectDAO.MakePersistent (formerProject);
          daoFactory.ComponentDAO.MakePersistent (component);
          daoFactory.ProjectComponentUpdateDAO.MakePersistent (update);
          tx.Commit ();
        }
      }
      Cursor.Current = Cursors.Default;
      return true;
    }

    /// <summary>
    ///   Call when drop node represents Component
    /// </summary>
    /// <param name="dragTreeView"></param>
    /// <param name="dragNode">Dragged node</param>
    /// <param name="parentDragNode">parent of dragged node before move</param>
    /// <param name="dropTreeView"></param>
    /// <param name="dropNode">Drop node</param>
    /// <returns>True if move has succeeded, false otherwise</returns>
    private static bool MoveNodeOnComponent (BaseOperationTreeView dragTreeView, TreeNode dragNode, TreeNode parentDragNode,
                                            BaseOperationTreeView dropTreeView, TreeNode dropNode)
    {
      IComponent targetComponent;
      IComponent formerComponent;
      IIntermediateWorkPiece intermediateWorkPiece = null;

      Cursor.Current = Cursors.WaitCursor;
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      using (IDAOSession daoSession = daoFactory.OpenSession ()) {
        targetComponent = ((Tuple<bool, IComponent>)dropNode.Tag).Item2;
        ModelDAOHelper.DAOFactory.ComponentDAO.Lock (targetComponent);
        formerComponent = ((Tuple<bool, IComponent>)parentDragNode.Tag).Item2;
        ModelDAOHelper.DAOFactory.ComponentDAO.Lock (formerComponent);
        ModelDAOHelper.DAOFactory.ComponentDAO.InitializeComponentIntermediateWorkPieces (targetComponent);

        if (dragNode.Tag is Tuple<bool, IIntermediateWorkPiece>) {
          intermediateWorkPiece = ((Tuple<bool, IIntermediateWorkPiece>)dragNode.Tag).Item2;
        }
        else if (dragNode.Tag is Tuple<bool, ISimpleOperation>) {
          intermediateWorkPiece = ((Tuple<bool, ISimpleOperation>)dragNode.Tag).Item2.IntermediateWorkPiece;
        }
        ModelDAOHelper.DAOFactory.IntermediateWorkPieceDAO.Lock (intermediateWorkPiece);

        foreach (IComponentIntermediateWorkPiece componentIntermediateWorkPieceInTarget in targetComponent.ComponentIntermediateWorkPieces) {
          IIntermediateWorkPiece intermediateWorkPieceInTarget = componentIntermediateWorkPieceInTarget.IntermediateWorkPiece;
          if (((Lemoine.Collections.IDataWithId)intermediateWorkPieceInTarget).Id == ((Lemoine.Collections.IDataWithId)intermediateWorkPiece).Id) {
            Cursor.Current = Cursors.Default;
            MessageBox.Show (PulseCatalog.GetString ("DraggedNodeAlreadyExistsInTheTargetNode"), "", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
          }
        }

        using (IDAOTransaction tx = daoSession.BeginTransaction ()) {
          IList<IComponentIntermediateWorkPiece> listciwp = formerComponent.RemoveIntermediateWorkPiece (intermediateWorkPiece);
          Lemoine.Model.IComponentIntermediateWorkPiece ciwp = targetComponent.AddIntermediateWorkPiece (intermediateWorkPiece);
          DateTime dateTime = DateTime.Now.ToUniversalTime ();
          IComponentIntermediateWorkPieceUpdate updateRemove = ModelDAOHelper.ModelFactory.CreateComponentIntermediateWorkPieceUpdate (formerComponent, intermediateWorkPiece, ComponentIntermediateWorkPieceUpdateModificationType.DELETE);
          updateRemove.DateTime = dateTime;
          IComponentIntermediateWorkPieceUpdate updateAdd = ModelDAOHelper.ModelFactory.CreateComponentIntermediateWorkPieceUpdate (targetComponent, intermediateWorkPiece, ComponentIntermediateWorkPieceUpdateModificationType.NEW);
          updateAdd.DateTime = dateTime;
          daoFactory.ComponentDAO.MakePersistent (targetComponent);
          daoFactory.ComponentDAO.MakePersistent (formerComponent);
          daoFactory.IntermediateWorkPieceDAO.MakePersistent (intermediateWorkPiece);
          daoFactory.ComponentIntermediateWorkPieceUpdateDAO.MakePersistent (updateRemove);
          daoFactory.ComponentIntermediateWorkPieceUpdateDAO.MakePersistent (updateAdd);
          daoFactory.ComponentIntermediateWorkPieceDAO.MakePersistent (ciwp);
          foreach (IComponentIntermediateWorkPiece ciwpelt in listciwp) {
            daoFactory.ComponentIntermediateWorkPieceDAO.MakeTransient (ciwpelt);
          }
          tx.Commit ();
        }
      }
      Cursor.Current = Cursors.Default;
      return true;
    }

    /// <summary>
    ///   Call when drop node represents Part
    /// </summary>
    /// <param name="dragTreeView"></param>
    /// <param name="dragNode">Dragged node</param>
    /// <param name="parentDragNode">parent of dragged node before move</param>
    /// <param name="dropTreeView"></param>
    /// <param name="dropNode">Drop node</param>
    /// <returns>True if move has succeeded, false otherwise</returns>
    private static bool MoveNodeOnPart (BaseOperationTreeView dragTreeView, TreeNode dragNode, TreeNode parentDragNode,
                                       BaseOperationTreeView dropTreeView, TreeNode dropNode)
    {
      IComponent targetComponent;
      IComponent formerComponent;
      IIntermediateWorkPiece intermediateWorkPiece = null;

      Cursor.Current = Cursors.WaitCursor;
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      using (IDAOSession daoSession = daoFactory.OpenSession ()) {
        targetComponent = ((Tuple<bool, IPart>)dropNode.Tag).Item2.Component;
        ModelDAOHelper.DAOFactory.ComponentDAO.Lock (targetComponent);
        formerComponent = ((Tuple<bool, IPart>)parentDragNode.Tag).Item2.Component;
        ModelDAOHelper.DAOFactory.ComponentDAO.Lock (formerComponent);
        ModelDAOHelper.DAOFactory.ComponentDAO.InitializeComponentIntermediateWorkPieces (targetComponent);

        if (dragNode.Tag is Tuple<bool, IIntermediateWorkPiece>) {
          intermediateWorkPiece = ((Tuple<bool, IIntermediateWorkPiece>)dragNode.Tag).Item2;
        }
        else if (dragNode.Tag is Tuple<bool, ISimpleOperation>) {
          intermediateWorkPiece = ((Tuple<bool, ISimpleOperation>)dragNode.Tag).Item2.IntermediateWorkPiece;
        }
        ModelDAOHelper.DAOFactory.IntermediateWorkPieceDAO.Lock (intermediateWorkPiece);

        foreach (IComponentIntermediateWorkPiece componentIntermediateWorkPieceInTarget in targetComponent.ComponentIntermediateWorkPieces) {
          IIntermediateWorkPiece intermediateWorkPieceInTarget = componentIntermediateWorkPieceInTarget.IntermediateWorkPiece;
          if (((Lemoine.Collections.IDataWithId)intermediateWorkPieceInTarget).Id == ((Lemoine.Collections.IDataWithId)intermediateWorkPiece).Id) {
            Cursor.Current = Cursors.Default;
            MessageBox.Show (PulseCatalog.GetString ("DraggedNodeAlreadyExistsInTheTargetNode"), "", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
          }
        }

        using (IDAOTransaction tx = daoSession.BeginTransaction ()) {
          DateTime dateTime = DateTime.UtcNow;
          IComponentIntermediateWorkPieceUpdate updateRemove = ModelDAOHelper.ModelFactory.CreateComponentIntermediateWorkPieceUpdate (formerComponent, intermediateWorkPiece, ComponentIntermediateWorkPieceUpdateModificationType.DELETE);
          updateRemove.DateTime = dateTime;
          IComponentIntermediateWorkPieceUpdate updateAdd = ModelDAOHelper.ModelFactory.CreateComponentIntermediateWorkPieceUpdate (targetComponent, intermediateWorkPiece, ComponentIntermediateWorkPieceUpdateModificationType.NEW);
          updateAdd.DateTime = dateTime;
          Lemoine.Model.IComponentIntermediateWorkPiece ciwp = targetComponent.AddIntermediateWorkPiece (intermediateWorkPiece);
          IList<IComponentIntermediateWorkPiece> listciwp = formerComponent.RemoveIntermediateWorkPiece (intermediateWorkPiece);
          daoFactory.ComponentDAO.MakePersistent (targetComponent);
          daoFactory.ComponentDAO.MakePersistent (formerComponent);
          daoFactory.IntermediateWorkPieceDAO.MakePersistent (intermediateWorkPiece);
          daoFactory.ComponentIntermediateWorkPieceUpdateDAO.MakePersistent (updateRemove);
          daoFactory.ComponentIntermediateWorkPieceUpdateDAO.MakePersistent (updateAdd);
          daoFactory.ComponentIntermediateWorkPieceDAO.MakePersistent (ciwp);
          foreach (IComponentIntermediateWorkPiece ciwpelt in listciwp) {
            daoFactory.ComponentIntermediateWorkPieceDAO.MakeTransient (ciwpelt);
          }
          tx.Commit ();
        }
      }
      Cursor.Current = Cursors.Default;
      return true;
    }


    /// <summary>
    ///   Call when drop node represents IntermediateWorkPiece
    /// </summary>
    /// <param name="dragTreeView"></param>
    /// <param name="dragNode">Dragged node</param>
    /// <param name="parentDragNode">parent of dragged node before move</param>
    /// <param name="dropTreeView"></param>
    /// <param name="dropNode">Drop node</param>
    /// <returns>True if move has succeeded, false otherwise</returns>
    private static bool MoveNodeOnIntermediateWorkPiece (BaseOperationTreeView dragTreeView, TreeNode dragNode, TreeNode parentDragNode,
                                                        BaseOperationTreeView dropTreeView, TreeNode dropNode)
    {
      IIntermediateWorkPiece targetIntermediateWorkPiece;
      IIntermediateWorkPiece formerIntermediateWorkPiece;
      IOperation operation;

      Cursor.Current = Cursors.WaitCursor;
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      using (IDAOSession daoSession = daoFactory.OpenSession ()) {
        targetIntermediateWorkPiece = ((Tuple<bool, IIntermediateWorkPiece>)dropNode.Tag).Item2;
        ModelDAOHelper.DAOFactory.IntermediateWorkPieceDAO.Lock (targetIntermediateWorkPiece);
        formerIntermediateWorkPiece = ((Tuple<bool, IIntermediateWorkPiece>)parentDragNode.Tag).Item2;
        ModelDAOHelper.DAOFactory.IntermediateWorkPieceDAO.Lock (formerIntermediateWorkPiece);
        ModelDAOHelper.DAOFactory.IntermediateWorkPieceDAO.InitializePossibleNextOperations (targetIntermediateWorkPiece);
        operation = ((Tuple<bool, IOperation>)dragNode.Tag).Item2;
        ModelDAOHelper.DAOFactory.OperationDAO.Lock (operation);

        foreach (IOperation operationInTarget in targetIntermediateWorkPiece.PossibleNextOperations) {
          if (((Lemoine.Collections.IDataWithId<int>)operationInTarget).Id == ((Lemoine.Collections.IDataWithId)operation).Id) {
            Cursor.Current = Cursors.Default;
            MessageBox.Show (PulseCatalog.GetString ("DraggedNodeAlreadyExistsInTheTargetNode"), "", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
          }
        }

        using (IDAOTransaction tx = daoSession.BeginTransaction ()) {
          DateTime dateTime = DateTime.Now.ToUniversalTime ();
          IIntermediateWorkPieceOperationUpdate updateRemove =
            ModelDAOHelper.ModelFactory
            .CreateIntermediateWorkPieceOperationUpdate (formerIntermediateWorkPiece,
                                                         operation,
                                                         null);
          updateRemove.DateTime = dateTime;
          IIntermediateWorkPieceOperationUpdate updateAdd =
            ModelDAOHelper.ModelFactory
            .CreateIntermediateWorkPieceOperationUpdate (targetIntermediateWorkPiece,
                                                         null,
                                                         operation);
          updateAdd.DateTime = dateTime;
          formerIntermediateWorkPiece.RemovePossibleNextOperation (operation);
          targetIntermediateWorkPiece.AddPossibleNextOperation (operation);
          daoFactory.IntermediateWorkPieceDAO.MakePersistent (targetIntermediateWorkPiece);
          daoFactory.IntermediateWorkPieceDAO.MakePersistent (formerIntermediateWorkPiece);
          daoFactory.OperationDAO.MakePersistent (operation);
          daoFactory.IntermediateWorkPieceOperationUpdateDAO.MakePersistent (updateRemove);
          daoFactory.IntermediateWorkPieceOperationUpdateDAO.MakePersistent (updateAdd);
          tx.Commit ();
        }
      }
      Cursor.Current = Cursors.Default;
      return true;
    }


    #endregion


    #region CopyNode Methods

    //Return true if copying succeed, false otherwise
    /// <summary>
    ///   Copy dragged node to drop node.
    /// </summary>
    /// <param name="dragTreeView"></param>
    /// <param name="dragNode">Dragged node</param>
    /// <param name="dropTreeView"></param>
    /// <param name="dropNode">Drop node</param>
    /// <param name="levelToType">Array of type contained in TreeView for each level</param>
    /// <returns>True if copy succeeded, false otherwise</returns>
    public static bool CopyNode (BaseOperationTreeView dragTreeView, TreeNode dragNode,
                                BaseOperationTreeView dropTreeView, TreeNode dropNode, Type[] levelToType)
    {
      bool result = false;
      try {
        if (dropNode.Tag is Tuple<bool, IWorkOrder>) {
          result = CopyNodeToWorkOrder (dragNode, dropNode);
        }
        else if (dropNode.Tag is Tuple<bool, IProject>) {
          // Do nothing because child of Project(Component) can not be
          // related which more than one Project
          //return CopyNodeToProject(draggedNode, targetNode);
        }
        else if (dropNode.Tag is Tuple<bool, IComponent>) {
          result = CopyNodeToComponent (dragNode, dropNode);
        }
        else if (dropNode.Tag is Tuple<bool, IIntermediateWorkPiece>) {
          result = CopyNodeToIntermediateWorkPiece (dragNode, dropNode);
        }
        else if (dropNode.Tag is Tuple<bool, IJob>) {
          // Do nothing because child of Job(Component) can not be
          // related which more than one Project therefore Job
          // return CopyNodeToJob(draggedNode, targetNode);
        }
        else if (dropNode.Tag is Tuple<bool, IPart>) {
          result = CopyNodeToPart (dragNode, dropNode);
        }
        if (result) {
          dropTreeView.UpdateTreeNode (dropNode);
        }
        return result;
      }
      catch (Exception e) {
        Cursor.Current = Cursors.Default;
        MessageBox.Show (PulseCatalog.GetString ("ExceptionCaught") + "\n" + e.Message + "\n" + PulseCatalog.GetString ("StackTrace") + "\n" + e.StackTrace, "", MessageBoxButtons.OK, MessageBoxIcon.Error);
        return false;
      }
    }


    /// <summary>
    ///   Copy method used when drop node represents WorkOrder
    /// </summary>
    /// <param name="dragNode">Dragged node</param>
    /// <param name="dropNode">Drop node</param>
    /// <returns>True if copy succeeded, false otherwise</returns>
    private static bool CopyNodeToWorkOrder (TreeNode dragNode, TreeNode dropNode)
    {
      IWorkOrder targetWorkOrder;
      IProject project = null;

      Cursor.Current = Cursors.WaitCursor;
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      using (IDAOSession daoSession = daoFactory.OpenSession ()) {
        targetWorkOrder = ((Tuple<bool, IWorkOrder>)dropNode.Tag).Item2;
        ModelDAOHelper.DAOFactory.WorkOrderDAO.Lock (targetWorkOrder);
        if (dragNode.Tag is Tuple<bool, IProject>) {
          project = ((Tuple<bool, IProject>)dragNode.Tag).Item2;
        }
        else if (dragNode.Tag is Tuple<bool, IPart>) {
          project = ((Tuple<bool, IPart>)dragNode.Tag).Item2.Project;
        }
        ModelDAOHelper.DAOFactory.ProjectDAO.Lock (project);
        ModelDAOHelper.DAOFactory.WorkOrderDAO.InitializeProjects (targetWorkOrder);

        bool found = false;
        //Check that target node do not already has a node equivalent to the dragged node
        foreach (IProject projectInTargetNode in targetWorkOrder.Projects) {
          if (((Lemoine.Collections.IDataWithId<int>)projectInTargetNode).Id == ((Lemoine.Collections.IDataWithId)project).Id) {
            found = true;
            break;
          }
        }
        if (!found) { // Dragged Project node do not exist in target WorkOrder node
          using (IDAOTransaction tx = daoSession.BeginTransaction ()) {
            DateTime dateTime = DateTime.Now.ToUniversalTime ();
            IWorkOrderProjectUpdate update = ModelDAOHelper.ModelFactory.CreateWorkOrderProjectUpdate (targetWorkOrder, project, WorkOrderProjectUpdateModificationType.NEW);
            update.DateTime = dateTime;
            project.AddWorkOrder (targetWorkOrder);
            daoFactory.WorkOrderProjectUpdateDAO.MakePersistent (update);
            daoFactory.WorkOrderDAO.MakePersistent (targetWorkOrder);
            daoFactory.ProjectDAO.MakePersistent (project);
            tx.Commit ();
            Cursor.Current = Cursors.Default;
            return true;
          }
        }

        else { // Dragged node already exist in children of target node
          Cursor.Current = Cursors.Default;
          MessageBox.Show (PulseCatalog.GetString ("DraggedNodeAlreadyExistsInTheTargetNode"), "", MessageBoxButtons.OK, MessageBoxIcon.Error);
          return false;
        }
      }

    }


    /// <summary>
    ///   Copy method used when drop node represents Component
    /// </summary>
    /// <param name="dragNode">Dragged node</param>
    /// <param name="dropNode">Drop node</param>
    /// <returns>True if copy succeeded, false otherwise</returns>
    private static bool CopyNodeToComponent (TreeNode dragNode, TreeNode dropNode)
    {
      IComponent targetComponent;
      IIntermediateWorkPiece intermediateWorkPiece = null;

      Cursor.Current = Cursors.WaitCursor;
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      using (IDAOSession daoSession = daoFactory.OpenSession ()) {
        targetComponent = ((Tuple<bool, IComponent>)dropNode.Tag).Item2;
        ModelDAOHelper.DAOFactory.ComponentDAO.Lock (targetComponent);
        if (dragNode.Tag is Tuple<bool, IIntermediateWorkPiece>) {
          intermediateWorkPiece = ((Tuple<bool, IIntermediateWorkPiece>)dragNode.Tag).Item2;
        }
        else if (dragNode.Tag is Tuple<bool, ISimpleOperation>) {
          intermediateWorkPiece = ((Tuple<bool, ISimpleOperation>)dragNode.Tag).Item2.IntermediateWorkPiece;
        }
        ModelDAOHelper.DAOFactory.IntermediateWorkPieceDAO.Lock (intermediateWorkPiece);
        ModelDAOHelper.DAOFactory.ComponentDAO.InitializeComponentIntermediateWorkPieces (targetComponent);

        bool found = false;
        //Check that target node do not already has a node equivalent to the dragged node
        foreach (IComponentIntermediateWorkPiece componentIntermediateWorkPieceInTargetNode in targetComponent.ComponentIntermediateWorkPieces) {
          IIntermediateWorkPiece intermediateWorkPieceInTargetNode = componentIntermediateWorkPieceInTargetNode.IntermediateWorkPiece;
          if (((Lemoine.Collections.IDataWithId)intermediateWorkPieceInTargetNode).Id == ((Lemoine.Collections.IDataWithId)intermediateWorkPiece).Id) {
            found = true;
            break;
          }
        }
        if (!found) {
          using (IDAOTransaction tx = daoSession.BeginTransaction ()) {
            DateTime dateTime = DateTime.UtcNow;
            IComponentIntermediateWorkPieceUpdate update = ModelDAOHelper.ModelFactory.CreateComponentIntermediateWorkPieceUpdate (targetComponent, intermediateWorkPiece, ComponentIntermediateWorkPieceUpdateModificationType.NEW);
            update.DateTime = dateTime;
            Lemoine.Model.IComponentIntermediateWorkPiece ciwp = targetComponent.AddIntermediateWorkPiece (intermediateWorkPiece);
            daoFactory.ComponentDAO.MakePersistent (targetComponent);
            daoFactory.IntermediateWorkPieceDAO.MakePersistent (intermediateWorkPiece);
            daoFactory.ComponentIntermediateWorkPieceUpdateDAO.MakePersistent (update);
            daoFactory.ComponentIntermediateWorkPieceDAO.MakePersistent (ciwp);
            tx.Commit ();
          }
          Cursor.Current = Cursors.Default;
          return true;
        }
        else {// Dragged node already exist in children of target node
          Cursor.Current = Cursors.Default;
          MessageBox.Show (PulseCatalog.GetString ("DraggedNodeAlreadyExistsInTheTargetNode"), "", MessageBoxButtons.OK, MessageBoxIcon.Error);
          return false;
        }
      }

    }

    /// <summary>
    ///   Copy method used when drop node represents IntermediateWorkPiece
    /// </summary>
    /// <param name="dragNode">Dragged node</param>
    /// <param name="dropNode">Drop node</param>
    /// <returns>True if copy succeeded, false otherwise</returns>
    private static bool CopyNodeToIntermediateWorkPiece (TreeNode dragNode, TreeNode dropNode)
    {
      IIntermediateWorkPiece targetIntermediateWorkPiece;
      IOperation operation;
      Cursor.Current = Cursors.WaitCursor;
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      using (IDAOSession daoSession = daoFactory.OpenSession ()) {
        targetIntermediateWorkPiece = ((Tuple<bool, IIntermediateWorkPiece>)dropNode.Tag).Item2;
        ModelDAOHelper.DAOFactory.IntermediateWorkPieceDAO.Lock (targetIntermediateWorkPiece);
        operation = ((Tuple<bool, IOperation>)dragNode.Tag).Item2;
        ModelDAOHelper.DAOFactory.OperationDAO.Lock (operation);
        ModelDAOHelper.DAOFactory.IntermediateWorkPieceDAO.InitializePossibleNextOperations (targetIntermediateWorkPiece);

        bool found = false;
        //Check that target node do not already has a node equivalent to the dragged node
        foreach (IOperation operationInTargetNode in targetIntermediateWorkPiece.PossibleNextOperations) {
          if (((Lemoine.Collections.IDataWithId)operationInTargetNode).Id == ((Lemoine.Collections.IDataWithId)operation).Id) {
            found = true;
            break;
          }
        }
        if (!found) {
          using (IDAOTransaction tx = daoSession.BeginTransaction ()) {
            DateTime dateTime = DateTime.UtcNow;
            IIntermediateWorkPieceOperationUpdate update =
              ModelDAOHelper.ModelFactory
              .CreateIntermediateWorkPieceOperationUpdate (targetIntermediateWorkPiece,
                                                           null,
                                                           operation);
            update.DateTime = dateTime;
            targetIntermediateWorkPiece.AddPossibleNextOperation (operation);
            daoFactory.IntermediateWorkPieceDAO.MakePersistent (targetIntermediateWorkPiece);
            daoFactory.OperationDAO.MakePersistent (operation);
            daoFactory.IntermediateWorkPieceOperationUpdateDAO.MakePersistent (update);
            tx.Commit ();
          }
          Cursor.Current = Cursors.Default;
          return true;
        }
        else {// Dragged node already exist in children of target node
          Cursor.Current = Cursors.Default;
          MessageBox.Show (PulseCatalog.GetString ("DraggedNodeAlreadyExistsInTheTargetNode"), "", MessageBoxButtons.OK, MessageBoxIcon.Error);
          return false;
        }
      }
    }


    /// <summary>
    ///   Copy method used when drop node represents Part
    /// </summary>
    /// <param name="dragNode">Dragged node</param>
    /// <param name="dropNode">Drop node</param>
    /// <returns>True if copy succeeded, false otherwise</returns>
    private static bool CopyNodeToPart (TreeNode dragNode, TreeNode dropNode)
    {
      IComponent targetComponent;
      IIntermediateWorkPiece intermediateWorkPiece = null;

      Cursor.Current = Cursors.WaitCursor;
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      using (IDAOSession daoSession = daoFactory.OpenSession ()) {
        targetComponent = ((Tuple<bool, IPart>)dropNode.Tag).Item2.Component;
        ModelDAOHelper.DAOFactory.ComponentDAO.Lock (targetComponent);
        if (dragNode.Tag is Tuple<bool, IIntermediateWorkPiece>) {
          intermediateWorkPiece = ((Tuple<bool, IIntermediateWorkPiece>)dragNode.Tag).Item2;
        }
        else if (dragNode.Tag is Tuple<bool, ISimpleOperation>) {
          intermediateWorkPiece = ((Tuple<bool, ISimpleOperation>)dragNode.Tag).Item2.IntermediateWorkPiece;
        }
        ModelDAOHelper.DAOFactory.IntermediateWorkPieceDAO.Lock (intermediateWorkPiece);
        ModelDAOHelper.DAOFactory.ComponentDAO.InitializeComponentIntermediateWorkPieces (targetComponent);

        bool found = false;
        //Check that target node do not already has a node equivalent to the dragged node
        foreach (IComponentIntermediateWorkPiece componentIntermediateWorkPieceInTargetNode in targetComponent.ComponentIntermediateWorkPieces) {
          IIntermediateWorkPiece intermediateWorkPieceInTargetNode = componentIntermediateWorkPieceInTargetNode.IntermediateWorkPiece;
          if (((Lemoine.Collections.IDataWithId)intermediateWorkPieceInTargetNode).Id == ((Lemoine.Collections.IDataWithId)intermediateWorkPiece).Id) {
            found = true;
            break;
          }
        }
        if (!found) {
          using (IDAOTransaction tx = daoSession.BeginTransaction ()) {
            DateTime dateTime = DateTime.Now.ToUniversalTime ();
            IComponentIntermediateWorkPieceUpdate update = ModelDAOHelper.ModelFactory.CreateComponentIntermediateWorkPieceUpdate (targetComponent, intermediateWorkPiece, ComponentIntermediateWorkPieceUpdateModificationType.NEW);
            update.DateTime = dateTime;
            Lemoine.Model.IComponentIntermediateWorkPiece ciwp = targetComponent.AddIntermediateWorkPiece (intermediateWorkPiece);
            daoFactory.ComponentDAO.MakePersistent (targetComponent);
            daoFactory.IntermediateWorkPieceDAO.MakePersistent (intermediateWorkPiece);
            daoFactory.ComponentIntermediateWorkPieceUpdateDAO.MakePersistent (update);
            daoFactory.ComponentIntermediateWorkPieceDAO.MakePersistent (ciwp);
            tx.Commit ();
          }
          Cursor.Current = Cursors.Default;
          return true;
        }
        else {// Dragged node already exist in children of target node
          Cursor.Current = Cursors.Default;
          MessageBox.Show (PulseCatalog.GetString ("DraggedNodeAlreadyExistsInTheTargetNode"), "", MessageBoxButtons.OK, MessageBoxIcon.Error);
          return false;
        }
      }
    }

    #endregion


    #region MergeNode Methods
    /// <summary>
    ///   Merge data associated with each tag contained in dragged and target nodes
    /// 
    /// TODO: move it in BaseOperationTreeView
    /// </summary>
    /// <param name="dragTreeView">Tree view</param>
    /// <param name="dragNode">Dragged node</param>
    /// <param name="dropTreeView"></param>
    /// <param name="dropNode">Target node</param>
    /// <param name="levelToType">Array of type contained in TreeView for each level</param>
    public static void MergeNode (BaseOperationTreeView dragTreeView, TreeNode dragNode,
                                 BaseOperationTreeView dropTreeView, TreeNode dropNode, Type[] levelToType)
    {
      bool result = false;

      if (dropNode.Tag is Tuple<bool, IWorkOrder>) {
        result = MergeNode<IWorkOrder, IWorkOrderDAO> (dragTreeView, dragNode, dropNode, ModelDAOHelper.DAOFactory.WorkOrderDAO);
      }
      else if (dropNode.Tag is Tuple<bool, IProject>) {
        result = MergeNode<IProject, IProjectDAO> (dragTreeView, dragNode, dropNode, ModelDAOHelper.DAOFactory.ProjectDAO);
      }
      else if (dropNode.Tag is Tuple<bool, IJob>) {
        result = MergeNode<IJob, IJobDAO> (dragTreeView, dragNode, dropNode, ModelDAOHelper.DAOFactory.JobDAO);
      }
      else if (dropNode.Tag is Tuple<bool, IComponent>) {
        result = MergeNode<IComponent, IComponentDAO> (dragTreeView, dragNode, dropNode, ModelDAOHelper.DAOFactory.ComponentDAO);
      }
      else if (dropNode.Tag is Tuple<bool, IPart>) {
        result = MergeNode<IPart, IPartDAO> (dragTreeView, dragNode, dropNode, ModelDAOHelper.DAOFactory.PartDAO);
      }
      else if (dropNode.Tag is Tuple<bool, IIntermediateWorkPiece>) {
        result = MergeNode<IIntermediateWorkPiece, IIntermediateWorkPieceDAO> (dragTreeView, dragNode, dropNode, ModelDAOHelper.DAOFactory.IntermediateWorkPieceDAO);
      }
      else if (dropNode.Tag is Tuple<bool, IOperation>) {
        result = MergeNode<IOperation, IOperationDAO> (dragTreeView, dragNode, dropNode, ModelDAOHelper.DAOFactory.OperationDAO);
      }
      else if (dropNode.Tag is Tuple<bool, ISimpleOperation>) {
        result = MergeNode<ISimpleOperation, ISimpleOperationDAO> (dragTreeView, dragNode, dropNode, ModelDAOHelper.DAOFactory.SimpleOperationDAO);
      }
      // paths and sequences are non mergeable

      if (result) {
        if (0 == dropNode.Level) {
          dropTreeView.UpdateTreeNode (dropNode);
          dragNode.Remove ();
        }
        else {
          // rebuild parents of nodes with name/level equivalent to dragged node
          dragTreeView.UpdateTreeNodeFromParent (dragNode);
          // rebuild parents of nodes with name/level equivalent to dropped node
          dropTreeView.UpdateTreeNodeFromParent (dropNode);
          // nb: parents of nodes with name/level equivalent to dragged node are rebuilt twice
        }
      }
    }

    static bool MergeNode<I, IDAO> (ITreeViewObservable treeView, TreeNode dragNode, TreeNode dropNode, IDAO dao)
      where I : class, IVersionable
      where IDAO : IMergeDAO<I>, IGenericUpdateDAO<I, int>
    {
      Cursor.Current = Cursors.WaitCursor;
      I drag = ((Tuple<bool, I>)dragNode.Tag).Item2;
      I drop = ((Tuple<bool, I>)dropNode.Tag).Item2;

      if (object.Equals (drag, drop)) {
        // Nothing to do
        return false;
      }

      try {
        using (IDAOSession daoSession = ModelDAOHelper.DAOFactory.OpenSession ()) {
          TreeNode[] impactedNodes = ((OperationTreeView)treeView).TreeView.Nodes.Find (dropNode.Name, true);

          using (IDAOTransaction transaction = daoSession.BeginTransaction ()) {
            I merged = dao.Merge (drag, drop, ConflictResolution.Keep);
            transaction.Commit ();
            dropNode.Tag = new Tuple<bool, I> (((Tuple<bool, I>)dropNode.Tag).Item1, merged);
          }
          foreach (TreeNode impactedNode in impactedNodes) {
            if (impactedNode.Level == dropNode.Level) {
              treeView.ReloadTreeNodes (impactedNode);
            }
          }
        }
        Cursor.Current = Cursors.Default;
        return true;
      }
      catch (Exception ex) {
        Cursor.Current = Cursors.Default;
        string errorMessage = string.Format (@"{0}
{1}",
                                             PulseCatalog.GetString ("ExceptionCaught"),
                                             ex.Message);
        MessageBox.Show (errorMessage, PulseCatalog.GetString ("ExceptionCaught"), MessageBoxButtons.OK, MessageBoxIcon.Error);
        // Reload the data
        using (IDAOSession daoSession = ModelDAOHelper.DAOFactory.OpenSession ()) {
          treeView.ReloadTreeNodes (dragNode);
          treeView.ReloadTreeNodes (dropNode);
        }
        return false;
      }
    }
    #endregion
  }
}
