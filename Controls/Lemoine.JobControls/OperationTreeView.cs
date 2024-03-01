// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

using Lemoine.BaseControls;
using Lemoine.I18N;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Collections;

using Lemoine.Core.Log;
using System.Text;
using Lemoine.Business.Config;
using Lemoine.GDBPersistentClasses;

namespace Lemoine.JobControls
{
  /// <summary>
  /// This control wrap a treeview which represents associations between WorkOrder,
  /// Project, Component, IntermediateworkPiece and Operation.
  /// Through DragnDrop or ContextMenu, some actions can made.
  /// This control is an observable and therefore can be associated with a list
  /// of observers. These observers can react to a change on treeview wrapped.
  /// </summary>
  public partial class OperationTreeView : BaseOperationTreeView, ITreeViewObservable
  {
    #region OperationTreeViewNodeSorter
    /// Create a node sorter that implements the IComparer interface.
    public class OperationTreeViewNodeSorter : IComparer
    {
      /// <summary>
      ///  Comparison between two nodes (of same type)
      /// </summary>
      /// <param name="x"></param>
      /// <param name="y"></param>
      /// <returns></returns>
      public int Compare (object x, object y)
      {
        TreeNode tx = x as TreeNode;
        TreeNode ty = y as TreeNode;

        Debug.Assert (tx.GetType () == ty.GetType ());

        // on add, no level yet, and should only compare nodes at the same level anyway
        /*
        if (tx.Level < ty.Level) {
          return -1;
        }
        else if (tx.Level > ty.Level) {
          return 1;
        }
        else
         */


        if (tx.Tag is Tuple<bool, IWorkOrder>) {
          IWorkOrder txWorkOrder = ((Tuple<bool, IWorkOrder>)tx.Tag).Item2;
          IWorkOrder tyWorkOrder = ((Tuple<bool, IWorkOrder>)ty.Tag).Item2;
          return string.Compare (txWorkOrder.Display, tyWorkOrder.Display);
        }
        else if (tx.Tag is Tuple<bool, IProject>) {
          IProject txProject = ((Tuple<bool, IProject>)tx.Tag).Item2;
          IProject tyProject = ((Tuple<bool, IProject>)ty.Tag).Item2;
          return string.Compare (txProject.Display, tyProject.Display);
        }
        else if (tx.Tag is Tuple<bool, Model.IComponent>) {
          Lemoine.Model.IComponent txComponent = ((Tuple<bool, Model.IComponent>)tx.Tag).Item2;
          Lemoine.Model.IComponent tyComponent = ((Tuple<bool, Model.IComponent>)ty.Tag).Item2;
          return string.Compare (txComponent.Display, tyComponent.Display);
        }
        else if (tx.Tag is Tuple<bool, IIntermediateWorkPiece>) {
          IIntermediateWorkPiece txIntermediateWorkPiece = ((Tuple<bool, IIntermediateWorkPiece>)tx.Tag).Item2;
          IIntermediateWorkPiece tyIntermediateWorkPiece = ((Tuple<bool, IIntermediateWorkPiece>)ty.Tag).Item2;
          return string.Compare (txIntermediateWorkPiece.Display, tyIntermediateWorkPiece.Display);
        }
        else if (tx.Tag is Tuple<bool, IOperation>) {
          IOperation txOperation = ((Tuple<bool, IOperation>)tx.Tag).Item2;
          IOperation tyOperation = ((Tuple<bool, IOperation>)ty.Tag).Item2;
          return string.Compare (txOperation.Display, tyOperation.Display);
        }
        else if (tx.Tag is Tuple<bool, IJob>) {
          IJob txJob = ((Tuple<bool, IJob>)tx.Tag).Item2;
          IJob tyJob = ((Tuple<bool, IJob>)ty.Tag).Item2;
          return string.Compare (txJob.Display, tyJob.Display);
        }
        else if (tx.Tag is Tuple<bool, IPart>) {
          IPart txPart = ((Tuple<bool, IPart>)tx.Tag).Item2;
          IPart tyPart = ((Tuple<bool, IPart>)ty.Tag).Item2;
          return string.Compare (txPart.Display, tyPart.Display);
        }
        else if (tx.Tag is Tuple<bool, ISimpleOperation>) {
          ISimpleOperation txSimpleOperation = ((Tuple<bool, ISimpleOperation>)tx.Tag).Item2;
          ISimpleOperation tySimpleOperation = ((Tuple<bool, ISimpleOperation>)ty.Tag).Item2;
          return string.Compare (txSimpleOperation.Display, tySimpleOperation.Display);
        }
        else if (tx.Tag is Tuple<bool, ISequence>) {
          ISequence txSequence = ((Tuple<bool, ISequence>)tx.Tag).Item2;
          ISequence tySequence = ((Tuple<bool, ISequence>)ty.Tag).Item2;
          return txSequence.Order - tySequence.Order;
        }
        else if (tx.Tag is Tuple<bool, IPath>) {
          IPath txPath = ((Tuple<bool, IPath>)tx.Tag).Item2;
          IPath tyPath = ((Tuple<bool, IPath>)ty.Tag).Item2;
          return txPath.Number - tyPath.Number;
        }
        else {
          return 0;
        }
      }

    }

    #endregion

    #region Members
    /// <summary>
    /// List of control which are impacted by change of treeview;
    /// Main change is node selection
    /// </summary>
    List<ITreeViewObserver> m_observers = new List<ITreeViewObserver> ();

    /// <summary>
    /// use to store hierachy of class type use to build treeview.
    /// We define which kind of node for each level in wrapped treeview
    /// For example it is use to know whoch item we have at first level of
    /// our treeview: WorkOrder or Job, etc
    /// </summary>
    Type[] m_treeLevels = new Type[10]; // hierarchy depth at most 7 for the time being

    int m_level;

    /// <summary>
    /// Value for option WorkOrder + Project = Job
    /// Use to initialize treeview hierachy
    /// </summary>
    bool m_workOrderProjectIsJob;
    /// <summary>
    /// Value for option Project + Component = Part
    /// Use to initialize treeview hierachy
    /// </summary>
    bool m_projectComponentIsPart;
    /// <summary>
    /// Value for option Intermediate Work Piece + Operation = SimpleOperation
    /// Use to initialize treeview hierachy
    /// </summary>
    bool m_intermediateWorkPieceOperationIsSimpleOperation;

    /// <summary>
    /// Value for option SinglePath
    /// Used to initialize treeview hierarchy
    /// </summary>
    bool m_SinglePath;

    /// <summary>
    /// top of tree is part
    /// </summary>
    bool m_partAtTheTop;

    /// <summary>
    ///   Node being dragged
    /// </summary>
    private TreeNode m_dragNode = null;

    /// <summary>
    ///   Temporary drop node for selection
    /// </summary>
    private TreeNode m_tempDropNode = null;

    /// <summary>
    ///   Timer for scrolling
    /// </summary>
    private Timer m_timer = new Timer ();
    /// <summary>
    ///   Associated OrphanedItemsTreeView
    /// </summary>
    private OrphanedItemsTreeView m_orphanedItemsTreeView = new OrphanedItemsTreeView ();
    /// <summary>
    ///   Tells whether orphanedItemsTreeView is visible or not
    /// </summary>
    private bool m_orphansIsVisible;

    private const int MAPSIZE = 128;
    private StringBuilder m_newNodeMap = new StringBuilder (MAPSIZE);
    private string m_nodeMap;

    /// <summary>
    /// Allow path export/import to/from Excel file
    /// </summary>
    bool m_allowExcelImportExport = true;

    /// <summary>
    /// tag used for identifying which context menu items are to be disabled when no sequence is present
    /// </summary>
    static readonly string m_disableIfNoSequence = "disableIfNoSequence";

    string[] m_columnPropertyIdentifiers = new String[] { "Name", "EstimatedTime" };
    string[] m_columnFieldIdentifiers = new String[] { "SpindleSpeed", "Feedrate", /*"FeedrateUS",*/ "ToolName", "ToolNumber", "ToolDiameter" };

    bool m_removeNonActiveFieldsFromImportExport = true;
    bool m_initializing = true;
    #endregion // Members

    #region Getters / Setters
    /// <summary>
    /// For Excel export/import: properties of sequence to export/import
    /// </summary>
    public string[] ColumnPropertyIdentifiers
    {
      get { return m_columnPropertyIdentifiers; }
      set { m_columnPropertyIdentifiers = value; }
    }

    /// <summary>
    /// For Excel export/import: fields of sequence to export/import
    /// </summary>
    public string[] ColumnFieldIdentifiers
    {
      get { return m_columnFieldIdentifiers; }
      set { m_columnFieldIdentifiers = value; }
    }

    /// <summary>
    /// For Excel export/import: only consider active fields, or else all fields
    /// </summary>
    public bool RemoveNonActiveFieldsFromImportExport
    {
      get { return m_removeNonActiveFieldsFromImportExport; }
      set { m_removeNonActiveFieldsFromImportExport = value; }
    }
    #endregion

    static readonly ILog log = LogManager.GetLogger (typeof (OperationTreeView).FullName);

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public OperationTreeView ()
    {
      //
      // The InitializeComponent() call is required for Windows Forms designer support.
      //
      InitializeComponent ();

      //set boolean value of option
      WorkOrderProjectIsJob = Lemoine.Info.ConfigSet.Get<bool> (ConfigKeys.GetDataStructureConfigKey (DataStructureConfigKey.WorkOrderProjectIsJob));
      ProjectComponentIsPart = Lemoine.Info.ConfigSet.Get<bool> (ConfigKeys.GetDataStructureConfigKey (DataStructureConfigKey.ProjectComponentIsPart));
      IntermediateWorkPieceOperationIsSimpleOperation = Lemoine.Info.ConfigSet.Get<bool> (ConfigKeys.GetDataStructureConfigKey (DataStructureConfigKey.IntermediateWorkPieceOperationIsSimpleOperation));
      SinglePath = Lemoine.Info.ConfigSet.Get<bool> (ConfigKeys.GetDataStructureConfigKey (DataStructureConfigKey.SinglePath));
      PartAtTheTop = OperationExplorerConfigHelper.PartAtTheTop;

      InitializeTreeLevels ();
      searchComboBox.SelectedIndex = 0;
      TreeViewMS.AllowDrop = true;
      TreeViewMS.MultiSelect = true;
      TreeViewMS.TreeViewNodeSorter = new OperationTreeViewNodeSorter ();

      m_timer.Interval = 150;
      m_timer.Tick += new EventHandler (timer_Tick);

      disclosurePanel.Title = "Orphaned items";
      disclosurePanel.State = false;
      disclosurePanel.Content = OrphanedItemsTreeView;
      disclosurePanel.ContentHeight = 150;
      m_orphanedItemsTreeView.Dock = DockStyle.Fill;
      m_orphanedItemsTreeView.Init (this);
      Dictionary<int, IWorkOrderStatus> dict = new Dictionary<int, IWorkOrderStatus> ();
      int index;
      statusComboBox.Items.Clear ();
      index = statusComboBox.Items.Add (PulseCatalog.GetString ("AllItem"));
      dict.Add (index, null);
      using (IDAOSession daoSession = ModelDAOHelper.DAOFactory.OpenSession ()) {
        IList<IWorkOrderStatus> workOrderStatus = ModelDAOHelper.DAOFactory.WorkOrderStatusDAO.FindAllOrderByName ();
        foreach (var aWorkOrderStatus in workOrderStatus) {
          index = statusComboBox.Items.Add (aWorkOrderStatus.Display ?? "");
          dict.Add (index, aWorkOrderStatus);
        }
        statusComboBox.Tag = dict;
      }
      statusComboBox.SelectedIndex = 0;

      labelWorkOrderStatus.Text = m_workOrderProjectIsJob ? "Job status" : "Workorder status";

      projectCombobox.Items.Clear ();
      projectCombobox.Items.Add (PulseCatalog.GetString ("AllItem"));
      projectCombobox.Items.Add ("Non-archived projects");
      projectCombobox.Items.Add ("Archived projects");
      projectCombobox.SelectedIndex = 0;

      operationCombobox.Items.Clear ();
      operationCombobox.Items.Add (PulseCatalog.GetString ("AllItem"));
      operationCombobox.Items.Add ("Non-archived operations");
      operationCombobox.Items.Add ("Archived operations");
      operationCombobox.SelectedIndex = 0;

      if (m_treeLevels[0].Equals (typeof (IJob))) {
        // Hide the filter "operation"
        tableLayoutPanel1.RowStyles[2].Height = 0;

        // Projects are jobs
        labelProjectStatus.Text = "Job status";
      }
      else if (m_treeLevels[0].Equals (typeof (IWorkOrder))) {
        // Hide the filter "operation"
        tableLayoutPanel1.RowStyles[2].Height = 0;

        if (m_treeLevels[1].Equals (typeof (IProject))) {
          // Use of projects
          labelProjectStatus.Text = "Project status";
        }
        else if (m_treeLevels[1].Equals (typeof (IPart))) {
          // Projects are parts
          labelProjectStatus.Text = "Part status";
        }
      }
      else if (m_treeLevels[0].Equals (typeof (IPart))) {
        // Projects are parts
        labelProjectStatus.Text = "Part status";
      }

      m_initializing = false;
      LoadTreeView (null, null, null);
    }
    #endregion // Constructors

    #region Getters / Setters
    /// <summary>
    /// Get wrapped treeview
    /// </summary>
    public override TreeView TreeView
    {
      get {
        return m_treeViewMS;
      }
    }

    /// <summary>
    /// Get wrapped treeview
    /// </summary>
    public TreeViewMS TreeViewMS
    {
      get {
        return m_treeViewMS;
      }
    }

    /// <summary>
    ///
    /// </summary>
    public Type[] LevelToType
    {
      get {
        return m_treeLevels;
      }
    }

    /// <summary>
    /// Level of TreeView associated
    /// </summary>
    public int Level
    {
      get {
        return m_level;
      }
    }

    /// <summary>
    ///   Getter for TreeView of Orphan item
    /// </summary>
    public OrphanedItemsTreeView OrphanedItemsTreeView
    {
      get {
        return m_orphanedItemsTreeView;
      }
    }

    /// <summary>
    /// Datastructure: WorkOrder + Project = Job ?
    /// </summary>
    public bool WorkOrderProjectIsJob
    {
      get {
        return m_workOrderProjectIsJob;
      }
      private set {
        m_workOrderProjectIsJob = value;
      }
    }

    /// <summary>
    /// Datastructure: Project + Component = Part ?
    /// </summary>
    public bool ProjectComponentIsPart
    {
      get {
        return m_projectComponentIsPart;
      }
      private set {
        m_projectComponentIsPart = value;
      }
    }

    /// <summary>
    /// DataStructure: IWP + Operation = SimpleOperation ?
    /// </summary>
    public bool IntermediateWorkPieceOperationIsSimpleOperation
    {
      get {
        return m_intermediateWorkPieceOperationIsSimpleOperation;
      }
      private set {
        m_intermediateWorkPieceOperationIsSimpleOperation = value;
      }
    }

    /// <summary>
    ///   Tell whether orphanedItemsTreeView is visible
    /// </summary>
    [DescriptionAttribute ("Tell whether OrphanedItemsTreeView is visible")]
    public bool OrphansIsVisible
    {
      get {
        return m_orphansIsVisible;
      }
      set {
        m_orphansIsVisible = value;
        if (m_orphansIsVisible) {
          disclosurePanel.Visible = true;
        }
        else {
          disclosurePanel.Visible = false;
        }
      }
    }

    /// <summary>
    /// A single path per operation ?
    /// </summary>
    public bool SinglePath
    {
      get { return m_SinglePath; }
      private set { m_SinglePath = value; }
    }

    /// <summary>
    /// Part at the top ?
    /// </summary>
    public bool PartAtTheTop
    {
      get { return m_partAtTheTop; }
      set { m_partAtTheTop = value; }
    }

    /// <summary>
    /// Allow path export/import to/from Excel file
    /// </summary>
    public bool AllowExcelImportExport
    {
      get { return m_allowExcelImportExport; }
      set { m_allowExcelImportExport = value; }
    }
    #endregion

    #region inherited methods
    /// <summary>
    /// Get the level of a type
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public override int GetTypeLevel (Type type)
    {
      return GetTreeViewLevelFromType (type);
    }

    /// <summary>
    /// Get level in our TreeView, associated with an item type
    /// </summary>
    /// <param name="type">An item type</param>
    /// <returns></returns>
    int GetTreeViewLevelFromType (Type type)
    {
      if (type == null) {
        return -1;
      }
      for (int i = 0; i < m_treeLevels.Length; i++) {
        if (type.Equals (m_treeLevels[i])) {
          return i;
        }
      }
      return -1;
    }

    /// <summary>
    /// Get the type of a child
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public override Type GetChildType (Type type)
    {
      int level = GetTypeLevel (type);
      if (level + 1 < m_treeLevels.Length) {
        return m_treeLevels[level + 1];
      }
      else {
        return null;
      }
    }

    /// <summary>
    /// Get the type of a parent
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public override Type GetParentType (Type type)
    {
      int level = GetTypeLevel (type);
      if (1 <= level) {
        return m_treeLevels[level - 1];
      }
      else {
        return null;
      }
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
      TreeView.CollapseAll ();
      TreeView.SelectedNode = TreeView.TopNode;
      NotifyObservers ();
    }

    /// <summary>
    /// Give the focus to all the nodes with the same name and type
    /// </summary>
    /// <param name="type"></param>
    /// <param name="data"></param>
    /// <returns>Number of found nodes</returns>
    public override int GiveFocusToAllNodeInstances (Type type, Lemoine.Collections.IDataWithId data)
    {
      int found = base.GiveFocusToAllNodeInstances (type, data);
      if ((0 == found) && this.OrphansIsVisible && (null != m_orphanedItemsTreeView)) {
        disclosurePanel.Open ();
        return m_orphanedItemsTreeView.GiveFocusToAllNodeInstances (type, data);
      }
      else {
        return found;
      }
    }

    /// <summary>
    /// Update nodes with the same type and name and reload their children
    /// </summary>
    /// <param name="type"></param>
    /// <param name="name"></param>
    protected override void BuildTreeNodes (Type type, string name)
    {
      IList<TreeNode> nodes = FindTreeNodes (type, name);
      if (nodes.Count > 0) {
        Cursor.Current = Cursors.WaitCursor;
        this.TreeView.BeginUpdate ();

        // Reload the nodes
        ReloadTreeNodes (type, nodes);

        // Remove the children before re-building the tree
        foreach (TreeNode node in nodes) {
          node.Nodes.Clear ();
        }

        // Populate the children
        PopulateNodes (name, nodes, type);

        // collapse + expand to correctly rebuild children's children
        // (nb: children won't be expansed even if they were before since complete rebuild)
        foreach (TreeNode node in nodes) {
          if (node.IsExpanded) {
            node.Collapse (false);
            node.Expand ();
          }
        }

        this.TreeView.EndUpdate ();
      }
    }

    /// <summary>
    /// Select the specified nodes
    /// </summary>
    /// <param name="nodeArray"></param>
    public override void SelectNodes (TreeNode[] nodeArray)
    {
      this.TreeViewMS.SelectedNodes = nodeArray;
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
      return this.LevelToType[node.Level];
    }
    #endregion // inherited methods

    #region Methods
    /// <summary>
    /// Build hierachy of our treeview by initializing treeLevels variable.
    /// We also initialize contextMenuStrip associated with each kind of node.
    /// It is mean that there is a contextMenuStrip associated with WorkOrder node,
    /// another one with Project node, etc.
    /// </summary>
    void InitializeTreeLevels ()
    {
      //List ofkind of data is set here
      searchComboBox.Items.Clear ();
      searchComboBox.Items.Add (PulseCatalog.GetString ("AllItem"));

      //Context Menus are set here
      contextMenuStripTreeView.Items.Clear ();
      contextMenuStripWorkOrder.Items.Clear ();
      contextMenuStripProject.Items.Clear ();
      contextMenuStripJob.Items.Clear ();
      contextMenuStripComponent.Items.Clear ();
      contextMenuStripPart.Items.Clear ();
      contextMenuStripIntermediateWorkPiece.Items.Clear ();
      contextMenuStripOperation.Items.Clear ();
      contextMenuStripSimpleOperation.Items.Clear ();
      contextMenuStripSequence.Items.Clear ();

      m_level = 0;

      if (WorkOrderProjectIsJob) {
        InitializeTreeLevelWorkOrderProjectIsJob (ref m_level);
      }
      else {
        if (PartAtTheTop && ProjectComponentIsPart) {
          InitializeTreeLevelProjectComponentIsPart (ref m_level);
        }
        else {
          InitializeTreeLevelWorkOrderProjectIsNotJob (ref m_level);
          if (ProjectComponentIsPart) {
            InitializeTreeLevelProjectComponentIsPart (ref m_level);
          }
          else {
            InitializeTreeLevelProjectComponentIsNotPart (ref m_level);
          }
        }
      }

      if (IntermediateWorkPieceOperationIsSimpleOperation) {
        InitializeTreeLevelIntermediateWorkPieceIsSimpleOperation (ref m_level);
      }
      else {
        InitializeTreeLevelIntermediateWorkPieceIsNotSimpleOperation (ref m_level);
      }

      contextMenuStripProject.Items.Add (PulseCatalog.GetString ("Unlink"), null, UnlinkProject_MenuItemClick);
    }

    #region InitializeTreeLevelHelper

    private void AddExportImportExcelToContextMenu (ContextMenuStrip contextMenuStrip)
    {
      ToolStripItem exportItem = new ToolStripMenuItem (PulseCatalog.GetString ("ExportSequencesToExcelFile"), null, ExportSequences_MenuItemClick);
      exportItem.Tag = m_disableIfNoSequence;
      contextMenuStrip.Items.Add (exportItem);
      ToolStripItem importItem = new ToolStripMenuItem (PulseCatalog.GetString ("UpdateSequencesFromExcelFile"), null, UpdateSequences_MenuItemClick);
      importItem.Tag = m_disableIfNoSequence;
      contextMenuStrip.Items.Add (importItem);
    }

    private void InitializeTreeLevelWorkOrderProjectIsJob (ref int level)
    {
      m_treeLevels[level] = typeof (IJob);  //IDX_JOB;
      level++;
      searchComboBox.Items.Add (PulseCatalog.GetString ("Job"));
      contextMenuStripTreeView.Items.Add (PulseCatalog.GetString ("NewJob"), null, CreateJob_MenuItemClick);
      m_treeLevels[level] = typeof (Lemoine.Model.IComponent);
      level++;
      searchComboBox.Items.Add (PulseCatalog.GetString ("Component"));
      contextMenuStripJob.Items.Add (PulseCatalog.GetString ("LinkNewComponent"), null, LinkNewComponent_MenuItemClick);
    }

    private void InitializeTreeLevelWorkOrderProjectIsNotJob (ref int level)
    {
      m_treeLevels[level] = typeof (IWorkOrder);
      level++;
      searchComboBox.Items.Add (PulseCatalog.GetString ("WorkOrder"));
      contextMenuStripTreeView.Items.Add (PulseCatalog.GetString ("NewWorkOrder"), null, CreateWorkOrder_MenuItemClick);
    }

    private void InitializeTreeLevelIntermediateWorkPieceIsSimpleOperation (ref int level)
    {
      m_treeLevels[level] = typeof (ISimpleOperation);
      level++;
      searchComboBox.Items.Add (PulseCatalog.GetString ("SimpleOperation"));
      contextMenuStripTreeView.Items.Add (PulseCatalog.GetString ("NewSimpleOperation"), null, CreateSimpleOperation_MenuItemClick);
      contextMenuStripPart.Items.Add (PulseCatalog.GetString ("LinkNewSimpleOperation"), null, LinkNewSimpleOperation_MenuItemClick);
      contextMenuStripComponent.Items.Add (PulseCatalog.GetString ("LinkNewSimpleOperation"), null, LinkNewSimpleOperation_MenuItemClick);
      contextMenuStripSimpleOperation.Items.Add (PulseCatalog.GetString ("Unlink"), null, UnlinkSimpleOperation_MenuItemClick);

      if (SinglePath) {

        contextMenuStripSimpleOperation.Items.Add (PulseCatalog.GetString ("LinkNewSequence"), null, LinkNewSequence_MenuItemClick);

        AddExportImportExcelToContextMenu (contextMenuStripSimpleOperation);
        InitializeTreeLevelSinglePath (ref level);
      }
      else {
        contextMenuStripSimpleOperation.Items.Add (PulseCatalog.GetString ("LinkNewPath"), null, LinkNewPath_MenuItemClick);
        InitializeTreeLevelMultiPath (ref level);
      }
    }

    private void InitializeTreeLevelMultiPath (ref int level)
    {
      contextMenuStripPath.Items.Add (PulseCatalog.GetString ("LinkNewSequence"), null, LinkNewSequence_MenuItemClick);
      AddExportImportExcelToContextMenu (contextMenuStripPath);
      contextMenuStripPath.Items.Add (PulseCatalog.GetString ("Delete"), null, DeletePath_MenuItemClick);

      m_treeLevels[level] = typeof (IPath);
      level++;
      searchComboBox.Items.Add (PulseCatalog.GetString ("Path"));

      contextMenuStripSequence.Items.Add (PulseCatalog.GetString ("Delete"), null, DeleteSequence_MenuItemClick);

      m_treeLevels[level] = typeof (ISequence);
      level++;
      searchComboBox.Items.Add (PulseCatalog.GetString ("Sequence"));
    }

    private void InitializeTreeLevelSinglePath (ref int level)
    {
      m_treeLevels[level] = typeof (ISequence);
      level++;
      searchComboBox.Items.Add (PulseCatalog.GetString ("Sequence"));

      contextMenuStripSequence.Items.Add (PulseCatalog.GetString ("Delete"), null, DeleteSequence_MenuItemClick);
    }

    private void InitializeTreeLevelIntermediateWorkPieceIsNotSimpleOperation (ref int level)
    {
      m_treeLevels[level] = typeof (IIntermediateWorkPiece);
      level++;
      searchComboBox.Items.Add (PulseCatalog.GetString ("IntermediateWorkPiece"));
      contextMenuStripTreeView.Items.Add (PulseCatalog.GetString ("NewIntermediateWorkPiece"), null, CreateIntermediateWorkPiece_MenuItemClick);
      contextMenuStripPart.Items.Add (PulseCatalog.GetString ("LinkNewIntermediateWorkPiece"), null, LinkNewIntermediateWorkPiece_MenuItemClick);
      contextMenuStripComponent.Items.Add (PulseCatalog.GetString ("LinkNewIntermediateWorkPiece"), null, LinkNewIntermediateWorkPiece_MenuItemClick);
      contextMenuStripIntermediateWorkPiece.Items.Add (PulseCatalog.GetString ("Unlink"), null, UnlinkIntermediateWorkPiece_MenuItemClick);
      m_treeLevels[level] = typeof (IOperation);
      level++;

      searchComboBox.Items.Add (PulseCatalog.GetString ("Operation"));

      if (SinglePath) {
        contextMenuStripOperation.Items.Add (PulseCatalog.GetString ("LinkNewSequence"), null, LinkNewSequence_MenuItemClick);
        AddExportImportExcelToContextMenu (contextMenuStripOperation);
        InitializeTreeLevelSinglePath (ref level);
      }
      else {
        contextMenuStripOperation.Items.Add (PulseCatalog.GetString ("LinkNewPath"), null, LinkNewPath_MenuItemClick);
        InitializeTreeLevelMultiPath (ref level);
      }
    }

    private void InitializeTreeLevelProjectComponentIsPart (ref int level)
    {
      m_treeLevels[level] = typeof (IPart);
      level++;
      searchComboBox.Items.Add (PulseCatalog.GetString ("Part"));
      contextMenuStripTreeView.Items.Add (PulseCatalog.GetString ("NewPart"), null, CreatePart_MenuItemClick);
      contextMenuStripWorkOrder.Items.Add (PulseCatalog.GetString ("LinkNewPart"), null, LinkNewPart_MenuItemClick);
      // Add the Unlink button, only if Part is not the top item
      if (!PartAtTheTop) {
        contextMenuStripPart.Items.Add (PulseCatalog.GetString ("Unlink"), null, UnlinkPart_MenuItemClick);
      }
    }

    private void InitializeTreeLevelProjectComponentIsNotPart (ref int level)
    {
      m_treeLevels[level] = typeof (IProject);
      level++;
      searchComboBox.Items.Add (PulseCatalog.GetString ("Project"));
      contextMenuStripTreeView.Items.Add (PulseCatalog.GetString ("NewProject"), null, CreateProject_MenuItemClick);
      contextMenuStripWorkOrder.Items.Add (PulseCatalog.GetString ("LinkNewProject"), null, LinkNewProject_MenuItemClick);

      m_treeLevels[level] = typeof (Lemoine.Model.IComponent);
      level++;
      searchComboBox.Items.Add (PulseCatalog.GetString ("Component"));
      contextMenuStripProject.Items.Add (PulseCatalog.GetString ("LinkNewComponent"), null, LinkNewComponent_MenuItemClick);
    }
    #endregion // InitializeTreeLevelHelper

    /// <summary>
    /// Initialize treeview by adding nodes for level 1 and 2
    /// </summary>
    /// <param name="workOrderStatusFilter"></param>
    /// <param name="projectArchivedFilter">null if no filter, true if only archived projects, false if only non-archived projects</param>
    /// <param name="operationArchivedFilter">null if no filter, true if only archived operations, false if only non-archived operations</param>
    void LoadTreeView (IWorkOrderStatus workOrderStatusFilter, bool? projectArchivedFilter, bool? operationArchivedFilter)
    {
      if (m_initializing) {
        return;
      }

      // nb: sorting managed by OperationTreeViewNodeSorter (managed on each insertion)
      Cursor.Current = Cursors.WaitCursor;
      TreeView.BeginUpdate ();
      TreeView.Nodes.Clear ();

      if (m_treeLevels[0].Equals (typeof (IJob))) {
        LoadTreeViewJob (workOrderStatusFilter, projectArchivedFilter, operationArchivedFilter);
      }
      else if (m_treeLevels[0].Equals (typeof (IWorkOrder))) { // add nodes of level 2 which are of type WorkOrder
        LoadTreeViewWorkOrder (workOrderStatusFilter, projectArchivedFilter, operationArchivedFilter);
      }
      else if (m_treeLevels[0].Equals (typeof (IPart))) {
        LoadTreeViewPart (workOrderStatusFilter, projectArchivedFilter, operationArchivedFilter);
      }
      TreeView.EndUpdate ();
      TreeView.SelectedNode = TreeView.TopNode;
      Cursor.Current = Cursors.Default;
    }

    #region LoadTreeViewHelper
    void LoadTreeViewJob (IWorkOrderStatus workOrderStatusFilter, bool? projectArchivedFilter, bool? operationArchivedFilter)
    {
      IList<IJob> jobList = null;
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      using (IDAOSession daoSession = daoFactory.OpenSession ()) {
        jobList = daoFactory.JobDAO.FindAll ();
        ISet<IJob> jobSet = new HashSet<IJob> (jobList);
        foreach (IJob job in jobSet) {
          daoFactory.JobDAO.Lock (job);

          // Possibly filter on the workorder status
          if (workOrderStatusFilter != null && job.Status.Id != workOrderStatusFilter.Id) {
            continue;
          }

          // Possibly filter if the project is archived or not
          if (projectArchivedFilter.HasValue && job.Project != null) {
            var archived = job.Project.ArchiveDateTime.HasValue;
            if (archived && !projectArchivedFilter.Value || !archived && projectArchivedFilter.Value) {
              continue;
            }
          }

          // Populate the tree: first jobs and then components
          TreeNode treeNode = new TreeNode (job.Display, (int)TreeViewImageIndex.Job, (int)TreeViewImageIndex.Job);
          treeNode.Name = job.ProjectId.ToString ();
          treeNode.Tag = new Tuple<bool, IJob> (true, job);
          TreeView.Nodes.Add (treeNode);
          TreeNode childNode;
          foreach (Lemoine.Model.IComponent component in job.Components) {
            // when adding a node, first build it and build its Tag
            // before inserting it (OperationTreeViewNodeSorter uses .Tag)
            childNode = new TreeNode (component.Display, (int)TreeViewImageIndex.Component, (int)TreeViewImageIndex.Component);
            childNode.Name = ((Lemoine.Collections.IDataWithId)component).Id.ToString ();
            childNode.Tag = new Tuple<bool, Model.IComponent> (false, component);
            treeNode.Nodes.Add (childNode);
          }
        }
      }
    }

    void LoadTreeViewWorkOrder (IWorkOrderStatus workOrderStatusFilter, bool? projectArchivedFilter, bool? operationArchivedFilter)
    {
      IList<IWorkOrder> workOrderList;
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      using (IDAOSession daoSession = daoFactory.OpenSession ()) {
        workOrderList = daoFactory.WorkOrderDAO.FindAllEager ();
        ISet<IWorkOrder> workOrderSet = new HashSet<IWorkOrder> (workOrderList);
        foreach (IWorkOrder workOrder in workOrderSet) {
          // Possibly filter on the workorder status
          if (workOrderStatusFilter != null && workOrder.Status.Id != workOrderStatusFilter.Id) {
            continue;
          }

          // Populate the tree: first workorders and then projects or parts
          TreeNode treeNode = new TreeNode (workOrder.Display, (int)TreeViewImageIndex.WorkOrder, (int)TreeViewImageIndex.WorkOrder);
          treeNode.Name = ((Lemoine.Collections.IDataWithId)workOrder).Id.ToString ();
          treeNode.Tag = new Tuple<bool, IWorkOrder> (true, workOrder);
          TreeView.Nodes.Add (treeNode);
          int nextLevel = treeNode.Level + 1;
          if (m_treeLevels[nextLevel].Equals (typeof (IProject))) {
            LoadTreeViewProjectFromWorkOrder (workOrder, treeNode, projectArchivedFilter);
          }
          else if (m_treeLevels[nextLevel].Equals (typeof (IPart))) {
            LoadTreeViewPartFromWorkOrder (workOrder, treeNode, projectArchivedFilter);
          }
        }
      }
    }

    private void LoadTreeViewProjectFromWorkOrder (IWorkOrder workOrder, TreeNode treeNode, bool? projectArchivedFilter)
    {
      foreach (IProject project in workOrder.Projects) {
        // Possibly filter if the project is archived or not
        if (projectArchivedFilter.HasValue) {
          var archived = project.ArchiveDateTime.HasValue;
          if (archived && !projectArchivedFilter.Value || !archived && projectArchivedFilter.Value) {
            continue;
          }
        }

        // Populate the tree with the second level
        TreeNode childNode = new TreeNode (project.Display, (int)TreeViewImageIndex.Project, (int)TreeViewImageIndex.Project);
        childNode.Name = ((Lemoine.Collections.IDataWithId<int>)project).Id.ToString ();
        childNode.Tag = new Tuple<bool, IProject> (false, project);
        treeNode.Nodes.Add (childNode);
      }
    }

    private void LoadTreeViewPartFromWorkOrder (IWorkOrder workOrder, TreeNode treeNode, bool? projectArchivedFilter)
    {
      foreach (IPart part in workOrder.Parts) {
        // Possibly filter if the project is archived or not
        if (projectArchivedFilter.HasValue) {
          var archived = part.ArchiveDateTime.HasValue;
          if (archived && !projectArchivedFilter.Value || !archived && projectArchivedFilter.Value) {
            continue;
          }
        }

        // Populate the tree with the second level
        TreeNode childNode = new TreeNode (part.Display, (int)TreeViewImageIndex.Part, (int)TreeViewImageIndex.Part);
        childNode.Name = part.ComponentId.ToString ();
        childNode.Tag = new Tuple<bool, IPart> (false, part);
        treeNode.Nodes.Add (childNode);
      }
    }

    void LoadTreeViewPart (IWorkOrderStatus workOrderStatusFilter, bool? projectArchivedFilter, bool? operationArchivedFilter)
    {
      IList<IPart> parts;
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;

      using (IDAOSession daoSession = daoFactory.OpenSession ()) {
        parts = daoFactory.PartDAO.FindAll ();
        foreach (IPart part in parts) {
          // Possibly filter if the project is archived or not
          if (projectArchivedFilter.HasValue) {
            var archived = part.ArchiveDateTime.HasValue;
            if (archived && !projectArchivedFilter.Value || !archived && projectArchivedFilter.Value) {
              continue;
            }
          }

          // If not a top part and workorders are empty, skip the element
          if (!PartAtTheTop && part.WorkOrders.Count == 0) {
            continue;
          }

          // Skip the element if there are no workorder with the right status
          if (workOrderStatusFilter != null) {
            bool keep = false;
            foreach (IWorkOrder workOrder in part.WorkOrders) {
              if (workOrder.Status.Id == workOrderStatusFilter.Id) {
                keep = true;
                break;
              }
            }

            if (!keep) {
              continue;
            }
          }

          // Populate the tree: part, then intermediate work piece
          TreeNode treeNode = new TreeNode (part.Display, (int)TreeViewImageIndex.Part, (int)TreeViewImageIndex.Part);
          treeNode.Name = part.ComponentId.ToString ();
          treeNode.Tag = new Tuple<bool, IPart> (true, part);
          TreeView.Nodes.Add (treeNode);
          foreach (Lemoine.Model.IComponentIntermediateWorkPiece componentIntermediateWorkPiece in part.ComponentIntermediateWorkPieces) {
            IIntermediateWorkPiece iwp = componentIntermediateWorkPiece.IntermediateWorkPiece;

            // Possibly filter the operation
            if (iwp.Operation != null && operationArchivedFilter.HasValue) {
              bool archived = iwp.Operation.ArchiveDateTime.HasValue;
              if (archived && !operationArchivedFilter.Value || !archived && operationArchivedFilter.Value) {
                continue;
              }
            }

            // when adding a node, first build it and build its Tag
            // before inserting it (OperationTreeViewNodeSorter uses .Tag)
            if (m_intermediateWorkPieceOperationIsSimpleOperation) {
              ISimpleOperation simpleOperation = iwp.SimpleOperation;
              TreeNode childNode = new TreeNode (simpleOperation.Display, (int)TreeViewImageIndex.SimpleOperation, (int)TreeViewImageIndex.SimpleOperation);
              childNode.Name = simpleOperation.IntermediateWorkPieceId.ToString ();
              childNode.Tag = new Tuple<bool, ISimpleOperation> (false, simpleOperation);
              treeNode.Nodes.Add (childNode);
            }
            else {
              TreeNode childNode = new TreeNode (iwp.Display, (int)TreeViewImageIndex.IntermediateWorkPiece, (int)TreeViewImageIndex.IntermediateWorkPiece);
              childNode.Name = ((Lemoine.Collections.IDataWithId)iwp).Id.ToString ();
              childNode.Tag = new Tuple<bool, IIntermediateWorkPiece> (false, iwp);
              treeNode.Nodes.Add (childNode);
            }
          }
        }
      }
    }
    #endregion LoadTreeViewHelper

    /// <summary>
    ///   Get whether location represents point on given node(his label or icon) for given treeview
    /// </summary>
    /// <param name="treeView"></param>
    /// <param name="node"></param>
    /// <param name="location"></param>
    /// <returns></returns>
    private bool IsClickOnTextOrIcon (TreeView treeView, TreeNode node, Point location)
    {
      TreeViewHitTestInfo hitTest = treeView.HitTest (location);
      return (hitTest.Node == node)
        && ((hitTest.Location == TreeViewHitTestLocations.Label) || (hitTest.Location == TreeViewHitTestLocations.Image));
    }

    /// <summary>
    ///   Used to manage mouse click event on nodes of treeview
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    void TreeViewNodeMouseClick (object sender, TreeNodeMouseClickEventArgs e)
    {
      //Get point where click occurs and node allocated at line containing this point
      Point location = new Point (e.X, e.Y);
      // TreeNode treeNode =  TreeView.GetNodeAt(location);
      TreeNode treeNode;
      if (TreeView.SelectedNode == null) {
        treeNode = TreeView.GetNodeAt (location);
      }
      else {
        treeNode = TreeView.SelectedNode;
      }

      //notify treeview's observers to make them change state
      //according new selection of node
      NotifyObservers ();

      //if node selection occurs due to right click, assign to treeview
      //a context menu related to type of selected node
      if (e.Button == MouseButtons.Right) {
        if (!IsClickOnTextOrIcon (TreeView, treeNode, location)) {
          TreeView.ContextMenuStrip = contextMenuStripTreeView;
        }
        else if (treeNode.Tag is Tuple<bool, IWorkOrder>) {
          TreeView.ContextMenuStrip = contextMenuStripWorkOrder;
        }
        else if (treeNode.Tag is Tuple<bool, IProject>) {
          TreeView.ContextMenuStrip = contextMenuStripProject;
        }
        else if (treeNode.Tag is Tuple<bool, IJob>) {
          TreeView.ContextMenuStrip = contextMenuStripJob;
        }
        else if (treeNode.Tag is Tuple<bool, Model.IComponent>) {
          TreeView.ContextMenuStrip = contextMenuStripComponent;
        }
        else if (treeNode.Tag is Tuple<bool, IPart>) {
          TreeView.ContextMenuStrip = contextMenuStripPart;
        }
        else if (treeNode.Tag is Tuple<bool, IIntermediateWorkPiece>) {
          TreeView.ContextMenuStrip = contextMenuStripIntermediateWorkPiece;
        }
        else if (treeNode.Tag is Tuple<bool, IOperation>) {
          TreeView.ContextMenuStrip = contextMenuStripOperation;
        }
        else if (treeNode.Tag is Tuple<bool, ISimpleOperation>) {
          TreeView.ContextMenuStrip = contextMenuStripSimpleOperation;
        }
        else if (treeNode.Tag is Tuple<bool, ISequence>) {
          TreeView.ContextMenuStrip = contextMenuStripSequence;
        }
        else if (treeNode.Tag is Tuple<bool, IPath>) {
          TreeView.ContextMenuStrip = contextMenuStripPath;
        }
      }
    }

    /// <summary>
    ///   This method is used to fire scrolling in TreeView when mouse
    ///   cursor is near top or bottom of TreeView during DragNDrop
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void timer_Tick (object sender, EventArgs e)
    {
      if ((this.m_tempDropNode == null)
          || (null == m_tempDropNode.TreeView)) {
        return;
      }

      TreeView treeViewSender = this.m_tempDropNode.TreeView;
      //Length of the gap from which treeview scroll automatically during drag and drop
      int threshold = treeViewSender.ItemHeight - 5;//30;

      TreeNode node = treeViewSender.SelectedNode;
      if (node == null) {
        return;
      }

      // if mouse is near to the top, scroll up
      Point point = treeViewSender.PointToClient (Control.MousePosition);
      if ((point.Y <= threshold) && (point.Y >= 0)) {
        // set actual node to the upper one
        if (node.PrevVisibleNode != null) {
          node = node.PrevVisibleNode;
          // hide drag image
          DragHelper.ImageList_DragShowNolock (false);
          // scroll and refresh
          node.EnsureVisible ();
          treeViewSender.Refresh ();
          // show drag image
          DragHelper.ImageList_DragShowNolock (true);
        }
      }
      // if mouse is near to the bottom, scroll down
      //else if(pt.Y > (treeViewSender.Size.Height - threshold))
      else if ((point.Y <= treeViewSender.Height) && (point.Y >= (treeViewSender.Height - threshold))) {
        if (node.NextVisibleNode != null) {
          node = node.NextVisibleNode;
          // hide drag image
          DragHelper.ImageList_DragShowNolock (false);
          // scroll and refresh
          node.EnsureVisible ();
          treeViewSender.Refresh ();
          // show drag image
          DragHelper.ImageList_DragShowNolock (true);
        }
      }
    }

    #endregion


    #region TreeView DragNDrop Methods

    /// <summary>
    /// Occurs when the user begins dragging a node.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    internal void TreeViewItemDrag (object sender, ItemDragEventArgs e)
    {
      TreeView treeViewSender = (TreeView)sender;
      // Get drag node
      this.m_dragNode = (TreeNode)e.Item;
      //Select drag node
      treeViewSender.SelectedNode = this.m_dragNode;
      // For OrphanedItemsTreeView, we select only node at level 1
      if (OrphanedItemsTreeView != null) {
        if (OrphanedItemsTreeView.TreeView == sender) {
          if (this.m_dragNode.Level != 1) {
            return;
          }
        }
      }

      // Reset image list used for drag image
      this.imageListDrag.Images.Clear ();
      int nwidth = this.m_dragNode.Bounds.Size.Width + treeViewSender.Indent;
      if (nwidth <= 0) { nwidth = 16; }
      if (nwidth > 256) { nwidth = 256; }
      int nheight = this.m_dragNode.Bounds.Height;
      if (nheight <= 0) { nheight = 16; }
      if (nheight > 256) { nheight = 256; }
      this.imageListDrag.ImageSize = new Size (nwidth, nheight);

      // Create new bitmap
      // This bitmap will contain the tree node image to be dragged
      Bitmap bmp = new Bitmap (this.m_dragNode.Bounds.Width + treeViewSender.Indent, this.m_dragNode.Bounds.Height);

      // Get graphics from bitmap
      Graphics gfx = Graphics.FromImage (bmp);

      // Draw node icon into the bitmap
      int index = this.GetImageIndex (m_dragNode);
      gfx.DrawImage (this.imageList.Images[index], 0, 0);
      // Draw node label into bitmap
      gfx.DrawString (this.m_dragNode.Text,
                     treeViewSender.Font,
                     new SolidBrush (treeViewSender.ForeColor),
                     (float)treeViewSender.Indent, 1.0f);

      // Add bitmap to imagelist
      this.imageListDrag.Images.Add (bmp);
      // Get mouse position in client coordinates
      Point p = treeViewSender.PointToClient (Control.MousePosition);
      // Compute delta between mouse position and node bounds
      int dx = p.X + treeViewSender.Indent - this.m_dragNode.Bounds.Left;
      int dy = p.Y - this.m_dragNode.Bounds.Top;

      // Begin dragging image
      if (DragHelper.ImageList_BeginDrag (this.imageListDrag.Handle, 0, dx, dy)) {
        //DragDrop is launch if mouse left buttton is pressed
        if (e.Button == MouseButtons.Left) {
          // Begin dragging
          this.TreeView.DoDragDrop (bmp, DragDropEffects.All);
          // End dragging image
          DragHelper.ImageList_EndDrag ();
        }
      }
    }


    /// <summary>
    ///   Occurs when an object is dragged into the control's bounds.
    ///   Use to assign DragDropEffects
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    internal void TreeViewDragEnter (object sender, DragEventArgs e)
    {
      TreeView treeViewSender = (TreeView)sender;
      if (treeViewSender == TreeView) {
        // Mouse left button and CTRL button are pressed
        //left mouse button=1, right mouse button=2, Shift key=4, CTRL key=8,
        //middle mouse button=16, ALT key=32
        if ((e.KeyState) == 33) {
          e.Effect = DragDropEffects.Copy;
        }
        // Only mouse left button are pressed
        else if ((e.KeyState) == 1) {
          e.Effect = DragDropEffects.Move;
        }
        else {
          e.Effect = DragDropEffects.None;
        }
      }
      else {
        if (OrphanedItemsTreeView != null) {
          if (OrphanedItemsTreeView.TreeView == sender) {
            e.Effect = DragDropEffects.None;
          }
        }
      }
      DragHelper.ImageList_DragEnter (treeViewSender.Handle, e.X - treeViewSender.Left, e.Y - treeViewSender.Top);
      // Enable timer for scrolling dragged item
      this.m_timer.Enabled = true;
    }


    internal void TreeViewDragLeave (object sender, EventArgs e)
    {
      TreeView treeViewSender = (TreeView)sender;
      DragHelper.ImageList_DragLeave (treeViewSender.Handle);
      // Disable timer for scrolling dragged item
      this.m_timer.Enabled = false;

      if (this.TreeView == treeViewSender) {
        this.TreeView.Refresh ();
      }
    }


    /// <summary>
    ///   Occurs when an object is dragged over the control's bounds.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    internal void TreeViewDragOver (object sender, DragEventArgs e)
    {
      Point point;
      TreeNode dropNode;
      TreeView treeViewSender = (TreeView)sender;

      if (treeViewSender == TreeView) {
        // Compute drag position and move image
        point = this.PointToClient (new Point (e.X, e.Y));

        DragHelper.ImageList_DragMove (point.X - treeViewSender.Left, point.Y - treeViewSender.Top);

        // Get actual drop node
        dropNode = treeViewSender.GetNodeAt (treeViewSender.PointToClient (new Point (e.X, e.Y)));
        if (dropNode == null) {
          e.Effect = DragDropEffects.None;
          return;
        }
        // Mouse left button and CTRL button are pressed
        //left mouse button=1, right mouse button=2, Shift key=4, CTRL key=8,
        //middle mouse button=16, ALT key=32
        if (m_dragNode.Tag is Tuple<bool, ISequence>) {
          e.Effect = DragDropEffects.Move;
        }
        else if ((e.KeyState) == 33) {
          e.Effect = DragDropEffects.Copy;
        }
        //Only mouse left button are pressed
        else if ((e.KeyState) == 1) {
          e.Effect = DragDropEffects.Move;
        }
        else {
          e.Effect = DragDropEffects.None;
        }

        if (m_dragNode.TreeView == TreeView) {
          // Only allow drag and drop when it is necessary
          if ((m_dragNode.Level != dropNode.Level) && (m_dragNode.Level != dropNode.Level + 1)) {
            e.Effect = DragDropEffects.None;
          }
          //because we only merge nodes which have same parent
          //not true: cf. merge of operations from different parts/components
          /*
          else if ((m_dragNode.Level == dropNode.Level) && (m_dragNode.Parent != dropNode.Parent)) {
            e.Effect = DragDropEffects.None;
          }
           */
          else if ((m_dragNode.Level == dropNode.Level) && (m_dragNode.Parent == dropNode.Parent)
                   && (m_dragNode.Tag is Tuple<bool, ISequence>)) {
            int OffsetY = TreeView.PointToClient (Cursor.Position).Y - dropNode.Bounds.Top;
            int NodeOverImageWidth = imageList.Images[this.GetImageIndex (dropNode)].Size.Width + 8;
            Graphics g = TreeView.CreateGraphics ();

            if (OffsetY < (dropNode.Bounds.Height / 2)) {

              #region Store the placeholder info into a pipe delimited string
              SetNewNodeMap (dropNode, false);
              if (SetMapsEqual () == true) {
                return;
              }
              #endregion
              #region Clear placeholders above and below
              //this.Refresh();
              this.TreeView.Refresh ();
              #endregion
              #region Draw the placeholders
              this.DrawLeafTopPlaceholders (dropNode);
              #endregion
            }
            else {

              #region Allow drag drop to parent branches
              TreeNode ParentDragDrop = null;
              // If the node the mouse is over is the last node of the branch we should allow
              // the ability to drop the "nodemoving" node BELOW the parent node
              if (dropNode.Parent != null && dropNode.Index == (dropNode.Parent.Nodes.Count - 1)) {
                int XPos = TreeView.PointToClient (Cursor.Position).X;
                if (XPos < dropNode.Bounds.Left) {
                  ParentDragDrop = dropNode.Parent;

                  if (XPos < (ParentDragDrop.Bounds.Left - TreeView.ImageList.Images[ParentDragDrop.ImageIndex].Size.Width)) {
                    if (ParentDragDrop.Parent != null) {
                      ParentDragDrop = ParentDragDrop.Parent;
                    }
                  }
                }
              }
              #endregion
              #region Store the placeholder info into a pipe delimited string
              // Since we are in a special case here, use the ParentDragDrop node as the current "nodeover"
              SetNewNodeMap (ParentDragDrop != null ? ParentDragDrop : dropNode, true);
              if (SetMapsEqual () == true) {
                return;
              }
              #endregion
              #region Clear placeholders above and below
              //this.Refresh();
              this.TreeView.Refresh ();
              #endregion
              #region Draw the placeholders
              DrawLeafBottomPlaceholders (dropNode, ParentDragDrop);
              #endregion
            }

          }
        }
        else { // Not the same tree view => from orphaned tree view to main tree view
          // Only allow drag and drop when it is necessary
          Type dragType = this.OrphanedItemsTreeView.GetTreeNodeType (m_dragNode);
          Type dropType = this.GetTreeNodeType (dropNode);
          Type dropChildType = this.GetChildType (dropType);
          if (!object.Equals (dragType, dropChildType)) {
            e.Effect = DragDropEffects.None;
          }
        }

        // if mouse is on a new node select it
        if (this.m_tempDropNode != dropNode) {
          //if (!(m_dragNode.Tag is Pair<Boolean,ISequence>) && !(dropNode.Tag is Pair<Boolean,ISequence>)) {
          DragHelper.ImageList_DragShowNolock (false);
          this.TreeView.SelectedNode = dropNode;
          NotifyObservers ();
          DragHelper.ImageList_DragShowNolock (true);
          m_tempDropNode = dropNode;
          //}
        }

      }
      else {
        if (OrphanedItemsTreeView != null) {
          if (OrphanedItemsTreeView.TreeView == sender) {
            // Compute drag position and move image
            point = OrphanedItemsTreeView.PointToClient (new Point (e.X, e.Y));
            DragHelper.ImageList_DragMove (point.X - treeViewSender.Left, point.Y - treeViewSender.Top);
            // Get actual drop node
            dropNode = treeViewSender.GetNodeAt (treeViewSender.PointToClient (new Point (e.X, e.Y)));
            if (dropNode == null) {
              e.Effect = DragDropEffects.None;
              return;
            }
            // if mouse is on a new node select it
            if (this.m_tempDropNode != dropNode) {
              DragHelper.ImageList_DragShowNolock (false);
              treeViewSender.SelectedNode = dropNode;
              DragHelper.ImageList_DragShowNolock (true);
              m_tempDropNode = dropNode;
            }
          }
        }
      }

    }


    /// <summary>
    /// Occurs when a drag-and-drop operation is completed.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    void TreeViewDragDrop (object sender, DragEventArgs e)
    {
      log.Debug ("TREEVIEWDRAGDROP");
      // Unlock updates
      DragHelper.ImageList_DragLeave (this.TreeView.Handle);
      // Get drop node
      TreeNode dropNode = this.TreeView.GetNodeAt (this.TreeView.PointToClient (new Point (e.X, e.Y)));
      // If node at the drop location is same as the dragged node, exit
      if (m_dragNode.Equals (dropNode)) {
        return;
      }

      TreeNode topNode;
      #region Merge for Sequence node
      if (m_dragNode.TreeView == this.TreeView) {
        // Node at the drop location is at the same level as dragged node
        // then the two nodes must be merged if DragDropEffects is Move
        if ((m_dragNode.Level == dropNode.Level)) {
          if ((m_dragNode.Tag is Tuple<bool, ISequence>)
              && (m_dragNode.Parent == dropNode.Parent)) {
            TreeNode parentNode = dropNode.Parent;
            int dragIndex = parentNode.Nodes.IndexOf (m_dragNode);
            int dropIndex = parentNode.Nodes.IndexOf (dropNode);
            IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
            ISequence sequence;

            if (dragIndex < dropIndex) { // move down
              using (IDAOSession daoSession = daoFactory.OpenSession ()) {
                // performed in a single transition since db constraint on sequence order unicity is deferrable
                using (IDAOTransaction tx = daoSession.BeginTransaction ()) {
                  sequence = ((Tuple<bool, ISequence>)parentNode.Nodes[dropIndex].Tag).Item2;
                  ModelDAOHelper.DAOFactory.SequenceDAO.Lock (sequence);
                  int orderSequenceDrop = sequence.Order;
                  for (int i = dragIndex + 1; i <= dropIndex; i++) {
                    sequence = ((Tuple<bool, ISequence>)parentNode.Nodes[i].Tag).Item2;
                    ModelDAOHelper.DAOFactory.SequenceDAO.Lock (sequence);
                    sequence.Order--;
                    daoFactory.SequenceDAO.MakePersistent (sequence);
                  }
                  sequence = ((Tuple<bool, ISequence>)m_dragNode.Tag).Item2;
                  ModelDAOHelper.DAOFactory.SequenceDAO.Lock (sequence);
                  sequence.Order = orderSequenceDrop;
                  daoFactory.SequenceDAO.MakePersistent (sequence);
                  tx.Commit ();
                }
              }
            }
            else if (dragIndex > dropIndex) { // move up
              using (IDAOSession daoSession = daoFactory.OpenSession ()) {
                // performed in a single transition since db constraint on sequence order unicity is deferrable
                using (IDAOTransaction tx = daoSession.BeginTransaction ()) {
                  sequence = ((Tuple<bool, ISequence>)parentNode.Nodes[dropIndex].Tag).Item2;
                  ModelDAOHelper.DAOFactory.SequenceDAO.Lock (sequence);
                  int orderSequenceDrop = sequence.Order;
                  for (int i = dropIndex; i < dragIndex; i++) {
                    sequence = ((Tuple<bool, ISequence>)parentNode.Nodes[i].Tag).Item2;
                    ModelDAOHelper.DAOFactory.SequenceDAO.Lock (sequence);
                    sequence.Order++;
                    daoFactory.SequenceDAO.MakePersistent (sequence);
                  }
                  sequence = ((Tuple<bool, ISequence>)m_dragNode.Tag).Item2;
                  ModelDAOHelper.DAOFactory.SequenceDAO.Lock (sequence);
                  sequence.Order = orderSequenceDrop;
                  daoFactory.SequenceDAO.MakePersistent (sequence);
                  tx.Commit ();
                }
              }
            }

            base.BuildTreeNodes (parentNode);
            TreeView.SelectedNode = parentNode;
          }
          #endregion
          else if (e.Effect == DragDropEffects.Move) {
            topNode = TreeView.TopNode;
            log.Debug ("Merge nodes");
            TreeViewBasicsOperations.MergeNode (this, m_dragNode, this, dropNode, m_treeLevels);
            TreeView.SelectedNode = dropNode;
            TreeView.TopNode = topNode;

          }
        }
        //Node at the drop location has same level as parent of dragged node but is not his parent.
        //In this case, dragged node is either move or copy
        else if ((dropNode.Level == (m_dragNode.Level - 1)) && (!m_dragNode.Parent.Equals (dropNode))) {
          // If it is a move operation, remove the node from its current location
          // and add it to the node at the drop location.
          if (e.Effect == DragDropEffects.Move) {
            topNode = TreeView.TopNode;
            log.Debug ("MoveNode");
            TreeViewBasicsOperations.MoveNode (this, m_dragNode,
                                              this, dropNode, m_treeLevels);
            TreeView.SelectedNode = dropNode;
            TreeView.TopNode = topNode;
          }
          // If it is a copy operation, clone the dragged node
          // and add it to the node at the drop location.
          else if (e.Effect == DragDropEffects.Copy) {
            topNode = TreeView.TopNode;
            log.Debug ("CopyNode");
            TreeViewBasicsOperations.CopyNode (this, m_dragNode,
                                              this, dropNode, m_treeLevels);
            TreeView.SelectedNode = dropNode;
            TreeView.TopNode = topNode;
          }
        }
      }

      else {
        if ((OrphanedItemsTreeView != null) && (m_dragNode.TreeView == OrphanedItemsTreeView.TreeView)) {
          if (e.Effect == DragDropEffects.Move) {
            topNode = TreeView.TopNode;
            MoveOrphanNode (OrphanedItemsTreeView, m_dragNode, this, dropNode, LevelToType);
            TreeView.SelectedNode = dropNode;
            TreeView.TopNode = topNode;
          }
        }
      }

      // Set drag node to null
      this.m_dragNode = null;
      // Disable scroll timer
      this.m_timer.Enabled = false;

      TreeView.Refresh ();
    }

    #endregion


    #region Move Orphans nodes
    /// <summary>
    ///   Move node for OrphanTreeView to OperationTreeView
    /// </summary>
    /// <param name="dragTreeView"></param>
    /// <param name="dragNode">Dragged node</param>
    /// <param name="dropTreeView">Target tree view</param>
    /// <param name="dropNode">Target node</param>
    /// <param name="levelToType"></param>
    /// <returns></returns>
    public static bool MoveOrphanNode (BaseOperationTreeView dragTreeView, TreeNode dragNode,
                                      BaseOperationTreeView dropTreeView, TreeNode dropNode,
                                      Type[] levelToType)
    {
      bool result = false;
      try {
        Cursor.Current = Cursors.WaitCursor;
        if (dropNode.Tag is Tuple<bool, IWorkOrder>) {
          result = MoveOrphanNodeOnWorkOrder (dragTreeView, dragNode, dropTreeView, dropNode);
        }
        else if (dropNode.Tag is Tuple<bool, Model.IComponent>) {
          result = MoveOrphanNodeOnComponent (dragTreeView, dragNode, dropTreeView, dropNode);
        }
        else if (dropNode.Tag is Tuple<bool, IPart>) {
          result = MoveOrphanNodeOnPart (dragTreeView, dragNode, dropTreeView, dropNode);
        }
        if (result) {
          dropTreeView.UpdateTreeNode (dropNode);
          dragNode.Remove ();
        }
        Cursor.Current = Cursors.Default;
        return result;
      }
      catch (Exception e) {
        Cursor.Current = Cursors.Default;
        MessageBox.Show (PulseCatalog.GetString ("ExceptionCaught") + "\n" + e.Message + "\n" + PulseCatalog.GetString ("StackTrace") + "\n" + e.StackTrace, "", MessageBoxButtons.OK, MessageBoxIcon.Error);
        return false;
      }
    }

    static bool MoveOrphanNodeOnWorkOrder (BaseOperationTreeView dragTreeView, TreeNode dragNode,
                                           BaseOperationTreeView dropTreeView, TreeNode dropNode)
    {
      IProject project = null;
      IWorkOrder targetWorkOrder = null;
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
        using (IDAOTransaction tx = daoSession.BeginTransaction ()) {
          project.AddWorkOrder (targetWorkOrder);
          DateTime dateTime = DateTime.Now.ToUniversalTime ();
          IWorkOrderProjectUpdate updateAdd = ModelDAOHelper.ModelFactory.CreateWorkOrderProjectUpdate (targetWorkOrder, project, WorkOrderProjectUpdateModificationType.NEW);
          updateAdd.DateTime = dateTime;
          daoFactory.WorkOrderProjectUpdateDAO.MakePersistent (updateAdd);
          daoFactory.WorkOrderDAO.MakePersistent (targetWorkOrder);
          daoFactory.ProjectDAO.MakePersistent (project);
          tx.Commit ();
        }
      }
      return true;
    }

    static bool MoveOrphanNodeOnComponent (BaseOperationTreeView dragTreeView, TreeNode dragNode,
                                          BaseOperationTreeView dropTreeView, TreeNode dropNode)
    {
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      using (IDAOSession daoSession = daoFactory.OpenSession ()) {
        Lemoine.Model.IComponent targetComponent = ((Tuple<bool, Model.IComponent>)dropNode.Tag).Item2;
        ModelDAOHelper.DAOFactory.ComponentDAO.Lock (targetComponent);
        IIntermediateWorkPiece intermediateWorkPiece = null;
        if (dragNode.Tag is Tuple<bool, IIntermediateWorkPiece>) {
          intermediateWorkPiece = ((Tuple<bool, IIntermediateWorkPiece>)dragNode.Tag).Item2;
        }
        else if (dragNode.Tag is Tuple<bool, ISimpleOperation>) {
          intermediateWorkPiece = ((Tuple<bool, ISimpleOperation>)dragNode.Tag).Item2.IntermediateWorkPiece;
        }
        ModelDAOHelper.DAOFactory.IntermediateWorkPieceDAO.Lock (intermediateWorkPiece);
        using (IDAOTransaction tx = daoSession.BeginTransaction ()) {
          IComponentIntermediateWorkPiece ciwp = targetComponent.AddIntermediateWorkPiece (intermediateWorkPiece);
          DateTime dateTime = DateTime.Now.ToUniversalTime ();
          IComponentIntermediateWorkPieceUpdate updateAdd = ModelDAOHelper.ModelFactory.CreateComponentIntermediateWorkPieceUpdate (targetComponent, intermediateWorkPiece, ComponentIntermediateWorkPieceUpdateModificationType.NEW);
          updateAdd.DateTime = dateTime;
          daoFactory.ComponentDAO.MakePersistent (targetComponent);
          daoFactory.IntermediateWorkPieceDAO.MakePersistent (intermediateWorkPiece);
          daoFactory.ComponentIntermediateWorkPieceUpdateDAO.MakePersistent (updateAdd);
          daoFactory.ComponentIntermediateWorkPieceDAO.MakePersistent (ciwp);
          tx.Commit ();
        }
      }
      return true;
    }

    static bool MoveOrphanNodeOnPart (BaseOperationTreeView dragTreeView, TreeNode dragNode,
                                     BaseOperationTreeView dropTreeView, TreeNode dropNode)
    {
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      using (IDAOSession daoSession = daoFactory.OpenSession ()) {
        Lemoine.Model.IComponent targetComponent = ((Tuple<bool, IPart>)dropNode.Tag).Item2.Component;
        ModelDAOHelper.DAOFactory.ComponentDAO.Lock (targetComponent);
        IIntermediateWorkPiece intermediateWorkPiece = null;
        if (dragNode.Tag is Tuple<bool, IIntermediateWorkPiece>) {
          intermediateWorkPiece = ((Tuple<bool, IIntermediateWorkPiece>)dragNode.Tag).Item2;
        }
        else if (dragNode.Tag is Tuple<bool, ISimpleOperation>) {
          intermediateWorkPiece = ((Tuple<bool, ISimpleOperation>)dragNode.Tag).Item2.IntermediateWorkPiece;
        }
        ModelDAOHelper.DAOFactory.IntermediateWorkPieceDAO.Lock (intermediateWorkPiece);
        using (IDAOTransaction tx = daoSession.BeginTransaction ()) {
          DateTime dateTime = DateTime.Now.ToUniversalTime ();
          IComponentIntermediateWorkPieceUpdate updateAdd = ModelDAOHelper.ModelFactory.CreateComponentIntermediateWorkPieceUpdate (targetComponent, intermediateWorkPiece, ComponentIntermediateWorkPieceUpdateModificationType.NEW);
          updateAdd.DateTime = dateTime;
          Lemoine.Model.IComponentIntermediateWorkPiece ciwp = targetComponent.AddIntermediateWorkPiece (intermediateWorkPiece);
          daoFactory.ComponentDAO.MakePersistent (targetComponent);
          daoFactory.IntermediateWorkPieceDAO.MakePersistent (intermediateWorkPiece);
          daoFactory.ComponentIntermediateWorkPieceUpdateDAO.MakePersistent (updateAdd);
          daoFactory.ComponentIntermediateWorkPieceDAO.MakePersistent (ciwp);
          tx.Commit ();
        }
      }
      return true;
    }
    #endregion


    #region ContextMenu CreateItem Methods

    /// <summary>
    ///   Call when user click on context menu to create new WorkOrder
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void CreateWorkOrder_MenuItemClick (object sender, System.EventArgs e)
    {
      Create_MenuItemClick (sender, e, typeof (IWorkOrder), CreateItemForm.GoalType.NEW);
    }

    /// <summary>
    ///   Call when user click on context menu to create new Project
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void CreateProject_MenuItemClick (object sender, System.EventArgs e)
    {
      Create_MenuItemClick (sender, e, typeof (IProject), CreateItemForm.GoalType.NEW);
    }

    /// <summary>
    ///   Call when user click on context menu to create new Job
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void CreateJob_MenuItemClick (object sender, System.EventArgs e)
    {
      Create_MenuItemClick (sender, e, typeof (IJob), CreateItemForm.GoalType.NEW);
    }

    /// <summary>
    ///   Call when user click on context menu to create new Part
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void CreatePart_MenuItemClick (object sender, System.EventArgs e)
    {
      Create_MenuItemClick (sender, e, typeof (IPart), CreateItemForm.GoalType.NEW);
    }

    /// <summary>
    ///   Call when user click on context menu to create new IntermdediateWorkPiece
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void CreateIntermediateWorkPiece_MenuItemClick (object sender, System.EventArgs e)
    {
      Create_MenuItemClick (sender, e, typeof (IIntermediateWorkPiece), CreateItemForm.GoalType.NEW);
    }

    /// <summary>
    ///   Call when user click on context menu to create new Part
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void CreateComponent_MenuItemClick (object sender, System.EventArgs e)
    {
      Create_MenuItemClick (sender, e, typeof (Lemoine.Model.IComponent), CreateItemForm.GoalType.NEW);
    }


    /// <summary>
    ///   Call when user click on context menu to create new SimpleOperation
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void CreateSimpleOperation_MenuItemClick (object sender, System.EventArgs e)
    {
      Create_MenuItemClick (sender, e, typeof (ISimpleOperation), CreateItemForm.GoalType.NEW);
    }
    /// <summary>
    ///   Call to show form used to create new item or to create new item
    ///   and bind it with current selected node of treeview
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    /// <param name="type">Type of item we want to create</param>
    /// <param name="goal">Tell whether item is just created or created and bind</param>
    private void Create_MenuItemClick (object sender, System.EventArgs e, Type type, CreateItemForm.GoalType goal)
    {
      CreateItemForm createItemForm = new CreateItemForm (type, goal, this);
      createItemForm.ShowDialog (this);
    }

    #endregion //ContextMenu CreateItem Methods


    #region ContextMenu LinkWithNewItem Methods

    /// <summary>
    ///   Call when user click on context menu to create new Project
    ///   and bind it which selected node of treeview
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void LinkNewProject_MenuItemClick (object sender, System.EventArgs e)
    {
      Create_MenuItemClick (sender, e, typeof (IProject), CreateItemForm.GoalType.BIND);
    }

    /// <summary>
    ///   Call when user click on context menu to create new Component
    ///   and bind it which selected node of treeview
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void LinkNewComponent_MenuItemClick (object sender, System.EventArgs e)
    {
      Create_MenuItemClick (sender, e, typeof (Lemoine.Model.IComponent), CreateItemForm.GoalType.BIND);
    }

    /// <summary>
    ///   Call when user click on context menu to create new Part
    ///   and bind it which selected node of treeview
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void LinkNewPart_MenuItemClick (object sender, System.EventArgs e)
    {
      Create_MenuItemClick (sender, e, typeof (IPart), CreateItemForm.GoalType.BIND);
    }

    /// <summary>
    ///   Call when user click on context menu to create new IntermediateWorkPiece
    ///   and bind it which selected node of treeview
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void LinkNewIntermediateWorkPiece_MenuItemClick (object sender, System.EventArgs e)
    {
      Create_MenuItemClick (sender, e, typeof (IIntermediateWorkPiece), CreateItemForm.GoalType.BIND);
    }

    /// <summary>
    ///   Call when user click on context menu to create new SimpleOperation
    ///   and bind it which selected node of treeview
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void LinkNewSimpleOperation_MenuItemClick (object sender, System.EventArgs e)
    {
      Create_MenuItemClick (sender, e, typeof (ISimpleOperation), CreateItemForm.GoalType.BIND);
    }

    /// <summary>
    ///   Call when user click on context menu to create new Sequence
    ///   and bind it which selected node of treeview
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void LinkNewSequence_MenuItemClick (object sender, System.EventArgs e)
    {
      Create_MenuItemClick (sender, e, typeof (ISequence), CreateItemForm.GoalType.BIND);
    }

    /// <summary>
    ///   Call when user click on context menu to create new Path
    ///   and bind it which selected node of treeview
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void LinkNewPath_MenuItemClick (object sender, System.EventArgs e)
    {
      Create_MenuItemClick (sender, e, typeof (IPath), CreateItemForm.GoalType.BIND);
    }

    /// <summary>
    /// Call when user click on context menu to export sequences of a path/operation/simpleoperation to an Excel file
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ExportSequences_MenuItemClick (object sender, System.EventArgs e)
    {
      TreeNode treeNode = this.TreeView.SelectedNode;
      // Fetch selected path
      IPath selectedPath = SelectPathFromTreeNode (treeNode);

      if (selectedPath != null) {
        FileDialog exportFileDialog = new SaveFileDialog ();
        exportFileDialog.Filter = "Excel files|*.xls|All files|*.*";
        DialogResult result = exportFileDialog.ShowDialog ();
        if (result == DialogResult.OK) {
          Lemoine.ExcelDataGrid.ExcelExporter exporter = new Lemoine.ExcelDataGrid.ExcelExporter (this.RemoveNonActiveFieldsFromImportExport);
          exporter.ColumnPropertyIdentifiers = (string[])this.ColumnPropertyIdentifiers.Clone ();
          exporter.ColumnFieldIdentifiers = (string[])this.ColumnFieldIdentifiers.Clone ();
          exporter.ExportPathAsFile (selectedPath, exportFileDialog.FileName);
        }
      }
    }

    /// <summary>
    /// Call when user click on context menu to update sequences of a path/operation/simpleoperation from an Excel file
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void UpdateSequences_MenuItemClick (object sender, System.EventArgs e)
    {
      TreeNode treeNode = this.TreeView.SelectedNode;
      // Fetch selected path
      IPath selectedPath = SelectPathFromTreeNode (treeNode);

      if (selectedPath != null) {
        FileDialog importFileDialog = new OpenFileDialog ();
        importFileDialog.Filter = "Excel files|*.xls|All files|*.*";
        DialogResult result = importFileDialog.ShowDialog ();
        if (result == DialogResult.OK) {
          Lemoine.ExcelDataGrid.ExcelExporter exporter = new Lemoine.ExcelDataGrid.ExcelExporter (this.RemoveNonActiveFieldsFromImportExport);
          exporter.ColumnPropertyIdentifiers = (string[])this.ColumnPropertyIdentifiers.Clone ();
          exporter.ColumnFieldIdentifiers = (string[])this.ColumnFieldIdentifiers.Clone ();
          try {
            exporter.ImportSequenceFile (selectedPath, importFileDialog.FileName);
            // update sequence nodes
            BuildTreeNodes (treeNode);
          }
          catch (Lemoine.ExcelDataGrid.ExcelImporterFileAccessException) {
            string errorMsg = String.Format ("{0} {1}.\n{2}.",
                                            PulseCatalog.GetString ("ProblemOnFileOpenA"),
                                            importFileDialog.FileName,
                                            PulseCatalog.GetString ("CheckNotOpenedInOtherApp"));
            MessageBox.Show (errorMsg);
          }
          catch (Lemoine.ExcelDataGrid.ExcelImporterBadFormatException) {
            string errorMsg = String.Format ("{0} {1}.", PulseCatalog.GetString ("ProblemOnFileProcessA"),
                                            importFileDialog.FileName);
            MessageBox.Show (errorMsg);
          }
        }
      }
    }
    #endregion // ContextMenu BindWithNewItem Methods


    #region others events methods
    /// <summary>
    ///  Method called when node is expanded. When this event occurs we add nodes to the children
    ///  of the currently selected node
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    void TreeViewAfterExpand (object sender, TreeViewEventArgs e)
    {
      if (e.Node != null) {
        //Get node that has been expanded
        TreeNode treeNode = e.Node;
        Cursor.Current = Cursors.WaitCursor;
        TreeView.BeginUpdate ();
        this.TreeViewAfterExpand (treeNode);
        TreeView.EndUpdate ();
        Cursor.Current = Cursors.Default;
        // TreeView.SelectedNode = treeNode; // this has unwanted side-effects (e.g. expanding parent nodes)
      }
    }

    void TreeViewMSAfterSelect (object sender, TreeViewEventArgs e)
    {
      NotifyObservers ();
    }

    static TreeViewMS CloneTreeViewMS (TreeViewMS source)
    {
      TreeNode newTreeNode;
      TreeViewMS destination = new TreeViewMS ();
      destination.MultiSelect = source.MultiSelect;

      foreach (TreeNode treeNode in source.Nodes) {
        newTreeNode = (TreeNode)treeNode.Clone ();
        CopyTreeNodeChildren (newTreeNode, treeNode);
        destination.Nodes.Add (newTreeNode);
      }
      return destination;

    }

    static void CloneTreeView (TreeView source, TreeView destination)
    {
      TreeNode newTreeNode;
      foreach (TreeNode treeNode in source.Nodes) {
        newTreeNode = (TreeNode)treeNode.Clone ();
        CopyTreeNodeChildren (newTreeNode, treeNode);
        destination.Nodes.Add (newTreeNode);
      }
    }

    static void CopyTreeNodeChildren (TreeNode source, TreeNode destination)
    {
      TreeNode newTreeNode;
      foreach (TreeNode treeNode in source.Nodes) {
        newTreeNode = (TreeNode)treeNode.Clone ();
        CopyTreeNodeChildren (newTreeNode, treeNode);
        destination.Nodes.Add (newTreeNode);
      }
    }

    void StatusComboBoxSelectionChangeCommitted (object sender, EventArgs e)
    {
      projectCombobox_SelectedIndexChanged (null, null);
    }

    void projectCombobox_SelectedIndexChanged (object sender, EventArgs e)
    {
      Dictionary<int, IWorkOrderStatus> dict = (Dictionary<int, IWorkOrderStatus>)statusComboBox.Tag;
      IWorkOrderStatus status = dict[statusComboBox.SelectedIndex];

      bool? archiveFilter = null;
      if (projectCombobox.SelectedIndex == 1) {
        archiveFilter = false;
      }
      else if (projectCombobox.SelectedIndex == 2) {
        archiveFilter = true;
      }

      bool? operationFilter = null;
      if (operationCombobox.SelectedIndex == 1) {
        operationFilter = false;
      }
      else if (operationCombobox.SelectedIndex == 2) {
        operationFilter = true;
      }

      LoadTreeView (status, archiveFilter, operationFilter);
    }

    void operationCombobox_SelectedIndexChanged (object sender, EventArgs e)
    {
      projectCombobox_SelectedIndexChanged (null, null);
    }

    void TreeViewMSMouseClick (object sender, MouseEventArgs e)
    {
      if (e.Button == MouseButtons.Right) {
        //Get point where click occurs and node allocated at line containing this point
        Point location = new Point (e.X, e.Y);
        TreeNode treeNode = TreeView.GetNodeAt (location);
        if (!IsClickOnTextOrIcon (TreeView, treeNode, location)) {
          TreeView.ContextMenuStrip = contextMenuStripTreeView;
        }
      }
    }


    void TreeViewMSMouseDown (object sender, MouseEventArgs e)
    {
      if (e.Button == MouseButtons.Right) {
        //Get point where click occurs and node allocated at line containing this point
        Point location = new Point (e.X, e.Y);
        TreeNode treeNode = TreeView.GetNodeAt (location);
        if (!IsClickOnTextOrIcon (TreeView, treeNode, location)) {
          TreeView.ContextMenuStrip = contextMenuStripTreeView;
        }
      }
    }

    #endregion


    #region search Methods
    #endregion


    #region Unlink and Delete methods

    private void UnlinkProject_MenuItemClick (object sender, System.EventArgs e)
    {
      TreeNode treeNode = TreeView.SelectedNode;
      if ((treeNode == null) || (!(treeNode.Tag is Tuple<bool, IProject>))) {
        return;
      }
      TreeNode parentNode = treeNode.Parent;
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      IProject project = ((Tuple<bool, IProject>)treeNode.Tag).Item2;

      using (IDAOSession daoSession = daoFactory.OpenSession ()) {
        ModelDAOHelper.DAOFactory.ProjectDAO.Lock (project);
        IWorkOrder workOrder = ((Tuple<bool, IWorkOrder>)parentNode.Tag).Item2;
        ModelDAOHelper.DAOFactory.WorkOrderDAO.Lock (workOrder);
        using (IDAOTransaction transaction = daoSession.BeginTransaction ()) {
          project.RemoveWorkOrder (workOrder);
          IWorkOrderProjectUpdate updateRemove = ModelDAOHelper.ModelFactory.CreateWorkOrderProjectUpdate (workOrder, project, WorkOrderProjectUpdateModificationType.DELETE);
          updateRemove.DateTime = DateTime.Now.ToUniversalTime ();
          daoFactory.ProjectDAO.MakePersistent (project);
          daoFactory.WorkOrderDAO.MakePersistent (workOrder);
          daoFactory.WorkOrderProjectUpdateDAO.MakePersistent (updateRemove);
          transaction.Commit ();
        }
      }

      treeNode.Remove ();
      BuildTreeNodes (parentNode);
      if (project.WorkOrders.Count == 0) {
        this.OrphanedItemsTreeView.InsertNode (treeNode);
      }
      this.NotifyObservers ();
    }


    private void UnlinkIntermediateWorkPiece_MenuItemClick (object sender, System.EventArgs e)
    {
      TreeNode treeNode = TreeView.SelectedNode;
      if ((treeNode == null) || (!(treeNode.Tag is Tuple<bool, IIntermediateWorkPiece>))) {
        return;
      }
      TreeNode parentNode = treeNode.Parent;
      IIntermediateWorkPiece intermediateWorkPiece = ((Tuple<bool, IIntermediateWorkPiece>)treeNode.Tag).Item2;

      using (IDAOSession daoSession = ModelDAOHelper.DAOFactory.OpenSession ()) {
        ModelDAOHelper.DAOFactory.IntermediateWorkPieceDAO.Lock (intermediateWorkPiece);
        Lemoine.Model.IComponent component;
        if (parentNode.Tag is Tuple<bool, Model.IComponent>) {
          component = ((Tuple<bool, Model.IComponent>)parentNode.Tag).Item2;
        }
        else {
          component = ((Tuple<bool, IPart>)parentNode.Tag).Item2.Component;
        }
        ModelDAOHelper.DAOFactory.ComponentDAO.Lock (component);

        using (IDAOTransaction transaction = daoSession.BeginTransaction ()) {
          IList<IComponentIntermediateWorkPiece> listciwp = component.RemoveIntermediateWorkPiece (intermediateWorkPiece);
          IComponentIntermediateWorkPieceUpdate updateRemove = ModelDAOHelper.ModelFactory.CreateComponentIntermediateWorkPieceUpdate (component, intermediateWorkPiece, ComponentIntermediateWorkPieceUpdateModificationType.DELETE);
          updateRemove.DateTime = DateTime.Now.ToUniversalTime ();
          ModelDAOHelper.DAOFactory.IntermediateWorkPieceDAO.MakePersistent (intermediateWorkPiece);
          ModelDAOHelper.DAOFactory.ComponentDAO.MakePersistent (component);
          ModelDAOHelper.DAOFactory.ComponentIntermediateWorkPieceUpdateDAO.MakePersistent (updateRemove);
          foreach (IComponentIntermediateWorkPiece ciwp in listciwp) {
            ModelDAOHelper.DAOFactory.ComponentIntermediateWorkPieceDAO.MakeTransient (ciwp);
          }
          transaction.Commit ();
        }
      }

      treeNode.Remove ();
      BuildTreeNodes (parentNode);
      if (intermediateWorkPiece.ComponentIntermediateWorkPieces.Count == 0) {
        this.OrphanedItemsTreeView.InsertNode (treeNode);
      }
      this.NotifyObservers ();
    }

    private void UnlinkPart_MenuItemClick (object sender, System.EventArgs e)
    {
      TreeNode treeNode = TreeView.SelectedNode;
      if ((treeNode == null) || (!(treeNode.Tag is Tuple<bool, IPart>))) {
        return;
      }
      TreeNode parentNode = treeNode.Parent;
      IPart part = ((Tuple<bool, IPart>)treeNode.Tag).Item2;

      using (IDAOSession daoSession = ModelDAOHelper.DAOFactory.OpenSession ()) {
        ModelDAOHelper.DAOFactory.PartDAO.Lock (part);
        IWorkOrder workOrder = ((Tuple<bool, IWorkOrder>)parentNode.Tag).Item2;
        ModelDAOHelper.DAOFactory.WorkOrderDAO.Lock (workOrder);
        using (IDAOTransaction transaction = daoSession.BeginTransaction ()) {
          part.RemoveWorkOrder (workOrder);
          IWorkOrderProjectUpdate updateRemove = ModelDAOHelper.ModelFactory.CreateWorkOrderProjectUpdate (workOrder, part.Project, WorkOrderProjectUpdateModificationType.DELETE);
          updateRemove.DateTime = DateTime.Now.ToUniversalTime ();
          ModelDAOHelper.DAOFactory.PartDAO.MakePersistent (part);
          ModelDAOHelper.DAOFactory.WorkOrderDAO.MakePersistent (workOrder);
          ModelDAOHelper.DAOFactory.WorkOrderProjectUpdateDAO.MakePersistent (updateRemove);
          transaction.Commit ();
        }
      }

      treeNode.Remove ();
      BuildTreeNodes (parentNode);
      if (part.WorkOrders.Count == 0) {
        this.OrphanedItemsTreeView.InsertNode (treeNode);
      }
      this.NotifyObservers ();
    }


    private void DeletePath_MenuItemClick (object sender, System.EventArgs e)
    {
      TreeNode treeNode = TreeView.SelectedNode;

      if ((treeNode == null) || (!(treeNode.Tag is Tuple<bool, IPath>))) {
        return;
      }

      OKCancelMessageDialog okCancelDeleteDialog = new OKCancelMessageDialog ();
      okCancelDeleteDialog.Text = PulseCatalog.GetString ("Delete");
      okCancelDeleteDialog.Message = PulseCatalog.GetString ("ReallyDelete");

      if (okCancelDeleteDialog.ShowDialog () == DialogResult.OK) {
        TreeNode parentNode = treeNode.Parent;
        IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
        using (IDAOSession daoSession = daoFactory.OpenSession ()) {
          IPath path = ((Tuple<bool, IPath>)treeNode.Tag).Item2;
          ModelDAOHelper.DAOFactory.PathDAO.Lock (path);

          IOperation operation;
          if (parentNode.Tag is Tuple<bool, IOperation>) {
            operation = ((Tuple<bool, IOperation>)parentNode.Tag).Item2;
          }
          else {
            operation = ((Tuple<bool, ISimpleOperation>)parentNode.Tag).Item2.Operation;
          }
          ModelDAOHelper.DAOFactory.OperationDAO.Lock (operation);

          using (IDAOTransaction transaction = daoSession.BeginTransaction ()) {
            operation.RemovePath (path);
            daoFactory.PathDAO.MakeTransient (path);
            daoFactory.OperationDAO.MakePersistent (operation);
            transaction.Commit ();
          }

          treeNode.Remove ();
          BuildTreeNodes (parentNode);
          this.NotifyObservers ();
        }
      }
    }

    private void DeleteSequence_MenuItemClick (object sender, System.EventArgs e)
    {
      TreeNode treeNode = TreeView.SelectedNode;
      if ((treeNode == null) || (!(treeNode.Tag is Tuple<bool, ISequence>))) {
        return;
      }

      OKCancelMessageDialog okCancelDeleteDialog = new OKCancelMessageDialog ();
      okCancelDeleteDialog.Text = PulseCatalog.GetString ("Delete");
      okCancelDeleteDialog.Message = PulseCatalog.GetString ("ReallyDelete");

      if (okCancelDeleteDialog.ShowDialog () == DialogResult.OK) {
        TreeNode parentNode = treeNode.Parent;
        IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
        using (IDAOSession daoSession = daoFactory.OpenSession ()) {
          ISequence sequence = ((Tuple<bool, ISequence>)treeNode.Tag).Item2;
          ModelDAOHelper.DAOFactory.SequenceDAO.Lock (sequence);
          IPath path = sequence.Path;
          ModelDAOHelper.DAOFactory.PathDAO.Lock (path);
          ModelDAOHelper.DAOFactory.OperationDAO.Lock (sequence.Operation);
          NHibernate.NHibernateUtil.Initialize (path.Sequences);
          NHibernate.NHibernateUtil.Initialize (sequence.Operation.Sequences);
          using (IDAOTransaction transaction = daoSession.BeginTransaction ()) {
            path.RemoveSequence (sequence);
            daoFactory.SequenceDAO.MakeTransient (sequence);
            daoFactory.PathDAO.MakePersistent (path);
            transaction.Commit ();
          }
        }

        treeNode.Remove ();
        this.TreeView.SelectedNode = treeNode.Parent;
        //this.TreeView.Focus();
        BuildTreeNodes (parentNode);
        this.NotifyObservers ();
      }
    }

    private void UnlinkSimpleOperation_MenuItemClick (object sender, System.EventArgs e)
    {
      TreeNode treeNode = TreeView.SelectedNode;
      if ((treeNode == null) || (!(treeNode.Tag is Tuple<bool, ISimpleOperation>))) {
        return;
      }
      TreeNode parentNode = treeNode.Parent;
      ISimpleOperation simpleOperation = ((Tuple<bool, ISimpleOperation>)treeNode.Tag).Item2;

      using (IDAOSession daoSession = ModelDAOHelper.DAOFactory.OpenSession ()) {
        ModelDAOHelper.DAOFactory.SimpleOperationDAO.Lock (simpleOperation);
        Lemoine.Model.IComponent component;
        if (parentNode.Tag is Tuple<bool, Model.IComponent>) {
          component = ((Lemoine.Model.IComponent)((Tuple<bool, Model.IComponent>)parentNode.Tag).Item2);
        }
        else {
          component = ((IPart)((Tuple<bool, IPart>)parentNode.Tag).Item2).Component;
        }

        using (IDAOTransaction transaction = daoSession.BeginTransaction ()) {
          ModelDAOHelper.DAOFactory.ComponentDAO.Lock (component);
          IComponentIntermediateWorkPieceUpdate updateRemove = ModelDAOHelper.ModelFactory
            .CreateComponentIntermediateWorkPieceUpdate (component, simpleOperation.IntermediateWorkPiece,
                                                        ComponentIntermediateWorkPieceUpdateModificationType.DELETE);
          updateRemove.DateTime = DateTime.Now.ToUniversalTime ();
          ModelDAOHelper.DAOFactory.ComponentIntermediateWorkPieceUpdateDAO
            .MakePersistent (updateRemove);
          ModelDAOHelper.DAOFactory.ComponentDAO.RemoveIntermediateWorkPiece (component, simpleOperation.IntermediateWorkPiece);
          transaction.Commit ();
        }
      }

      treeNode.Remove ();
      BuildTreeNodes (parentNode);
      if (simpleOperation.ComponentIntermediateWorkPieces.Count == 0) {
        this.OrphanedItemsTreeView.InsertNode (treeNode);
      }
      this.NotifyObservers ();
    }
    #endregion // Unlink and delete methods


    #region draw placeholder for sequence moving

    private void DrawLeafTopPlaceholders (TreeNode NodeOver)
    {
      Graphics g = this.TreeView.CreateGraphics ();

      int NodeOverImageWidth = imageList.Images[this.GetImageIndex (NodeOver)].Size.Width;// + 8;
      int LeftPos = NodeOver.Bounds.Left - NodeOverImageWidth;
      int RightPos = this.TreeView.ClientSize.Width;// - 4;

      Point[] LeftTriangle = new Point[5]{
        new Point(LeftPos, NodeOver.Bounds.Top - 4),
        new Point(LeftPos, NodeOver.Bounds.Top + 4),
        new Point(LeftPos + 4, NodeOver.Bounds.Y),
        new Point(LeftPos + 4, NodeOver.Bounds.Top - 1),
        new Point(LeftPos, NodeOver.Bounds.Top - 5)};

      Point[] RightTriangle = new Point[5]{
        new Point(RightPos, NodeOver.Bounds.Top - 4),
        new Point(RightPos, NodeOver.Bounds.Top + 4),
        new Point(RightPos - 4, NodeOver.Bounds.Y),
        new Point(RightPos - 4, NodeOver.Bounds.Top - 1),
        new Point(RightPos, NodeOver.Bounds.Top - 5)};


      g.FillPolygon (System.Drawing.Brushes.Black, LeftTriangle);
      g.FillPolygon (System.Drawing.Brushes.Black, RightTriangle);
      g.DrawLine (new System.Drawing.Pen (Color.Black, 2), new Point (LeftPos, NodeOver.Bounds.Top), new Point (RightPos, NodeOver.Bounds.Top));

    }//eom

    private void DrawLeafBottomPlaceholders (TreeNode NodeOver, TreeNode ParentDragDrop)
    {
      Graphics g = this.TreeView.CreateGraphics ();

      int NodeOverImageWidth = imageList.Images[this.GetImageIndex (NodeOver)].Size.Width;// + 8;

      // Once again, we are not dragging to node over, draw the placeholder using the ParentDragDrop bounds
      int LeftPos, RightPos;
      if (ParentDragDrop != null) {
        LeftPos = ParentDragDrop.Bounds.Left - imageList.Images[this.GetImageIndex (ParentDragDrop)].Size.Width;// + 8;
      }
      else {
        LeftPos = NodeOver.Bounds.Left - NodeOverImageWidth;
      }

      //RightPos = this.TreeView.Width;// - 4;
      RightPos = this.TreeView.ClientSize.Width;// - 4;

      Point[] LeftTriangle = new Point[5]{
        new Point(LeftPos, NodeOver.Bounds.Bottom - 4),
        new Point(LeftPos, NodeOver.Bounds.Bottom + 4),
        new Point(LeftPos + 4, NodeOver.Bounds.Bottom),
        new Point(LeftPos + 4, NodeOver.Bounds.Bottom - 1),
        new Point(LeftPos, NodeOver.Bounds.Bottom - 5)};

      Point[] RightTriangle = new Point[5]{
        new Point(RightPos, NodeOver.Bounds.Bottom - 4),
        new Point(RightPos, NodeOver.Bounds.Bottom + 4),
        new Point(RightPos - 4, NodeOver.Bounds.Bottom),
        new Point(RightPos - 4, NodeOver.Bounds.Bottom - 1),
        new Point(RightPos, NodeOver.Bounds.Bottom - 5)};


      g.FillPolygon (System.Drawing.Brushes.Black, LeftTriangle);
      g.FillPolygon (System.Drawing.Brushes.Black, RightTriangle);
      g.DrawLine (new System.Drawing.Pen (Color.Black, 2), new Point (LeftPos, NodeOver.Bounds.Bottom), new Point (RightPos, NodeOver.Bounds.Bottom));
    }//eom



    private void SetNewNodeMap (TreeNode tnNode, bool boolBelowNode)
    {
      m_newNodeMap.Length = 0;

      if (boolBelowNode) {
        m_newNodeMap.Insert (0, (int)tnNode.Index + 1);
      }
      else {
        m_newNodeMap.Insert (0, (int)tnNode.Index);
      }

      TreeNode tnCurNode = tnNode;

      while (tnCurNode.Parent != null) {
        tnCurNode = tnCurNode.Parent;

        if (m_newNodeMap.Length == 0 && boolBelowNode == true) {
          m_newNodeMap.Insert (0, (tnCurNode.Index + 1) + "|");
        }
        else {
          m_newNodeMap.Insert (0, tnCurNode.Index + "|");
        }
      }
    }//oem

    private bool SetMapsEqual ()
    {
      if (this.m_newNodeMap.ToString () == this.m_nodeMap) {
        return true;
      }
      else {
        this.m_nodeMap = this.m_newNodeMap.ToString ();
        return false;
      }
    }//oem

    #endregion

    IPath SelectPathFromTreeNode (TreeNode treeNode)
    {
      using (IDAOSession daoSession = ModelDAOHelper.DAOFactory.OpenSession ()) {
        if (treeNode.Tag is Tuple<bool, IPath>) {
          IPath path = ((Tuple<bool, IPath>)treeNode.Tag).Item2;
          ModelDAOHelper.DAOFactory.PathDAO.Lock (path);
          NHibernate.NHibernateUtil.Initialize (path.Sequences);
          return path;
        }
        else if ((treeNode.Tag is Tuple<bool, IOperation>) || (treeNode.Tag is Tuple<bool, ISimpleOperation>)) {
          IEnumerator<IPath> pathEnumerator;
          if (this.SinglePath) {
            IOperation operation;
            if (treeNode.Tag is Tuple<bool, IOperation>) {
              operation = ((Tuple<bool, IOperation>)treeNode.Tag).Item2;
            }
            else {
              operation = ((Tuple<bool, ISimpleOperation>)treeNode.Tag).Item2.Operation;
            }
            ModelDAOHelper.DAOFactory.OperationDAO.Lock (operation);
            if (operation.Paths.Count < 1) {
              return null;
            }
            pathEnumerator = operation.Paths.GetEnumerator ();

            if (pathEnumerator.MoveNext ()) {
              IPath path = pathEnumerator.Current;
              NHibernate.NHibernateUtil.Initialize (path.Sequences);
              return path;
            }
          }
        }
        return null;
      }
    }

    void EnableDisableExcelExportImportOnPath (ContextMenuStrip contextMenuStrip)
    {
      TreeNode treeNode = this.TreeView.SelectedNode;
      IPath selectedPath = SelectPathFromTreeNode (treeNode);
      bool itemStatus = (selectedPath != null) && (selectedPath.Sequences.Count > 0);
      foreach (ToolStripItem item in contextMenuStrip.Items) {
        if ((string)item.Tag == m_disableIfNoSequence) {
          {
            item.Enabled = itemStatus;
          }
        }
      }
    }

    void ContextMenuStripPathOpening (object sender, CancelEventArgs e)
    {
      EnableDisableExcelExportImportOnPath (contextMenuStripPath);
    }


    void ContextMenuStripOperationOpening (object sender, CancelEventArgs e)
    {
      EnableDisableExcelExportImportOnPath (contextMenuStripOperation);
    }

    void ContextMenuStripSimpleOperationOpening (object sender, CancelEventArgs e)
    {
      EnableDisableExcelExportImportOnPath (contextMenuStripSimpleOperation);
    }

    /// <summary>
    /// Return the item that is associated to a node
    /// </summary>
    /// <param name="node"></param>
    /// <returns></returns>
    public override object GetItem (TreeNode node)
    {
      object tag = node.Tag;

      if (tag is Tuple<bool, IWorkOrder>) {
        return ((Tuple<bool, IWorkOrder>)tag).Item2;
      }
      else if (tag is Tuple<bool, IProject>) {
        return ((Tuple<bool, IProject>)tag).Item2;
      }
      else if (tag is Tuple<bool, IJob>) {
        return ((Tuple<bool, IJob>)tag).Item2;
      }
      else if (tag is Tuple<bool, Model.IComponent>) {
        return ((Tuple<bool, Model.IComponent>)tag).Item2;
      }
      else if (tag is Tuple<bool, IPart>) {
        return ((Tuple<bool, IPart>)tag).Item2;
      }
      else if (tag is Tuple<bool, IIntermediateWorkPiece>) {
        return ((Tuple<bool, IIntermediateWorkPiece>)tag).Item2;
      }
      else if (tag is Tuple<bool, IOperation>) {
        return ((Tuple<bool, IOperation>)tag).Item2;
      }
      else if (tag is Tuple<bool, ISimpleOperation>) {
        return ((Tuple<bool, ISimpleOperation>)tag).Item2;
      }
      else if (tag is Tuple<bool, IPath>) {
        return ((Tuple<bool, IPath>)tag).Item2;
      }
      else if (tag is Tuple<bool, ISequence>) {
        return ((Tuple<bool, ISequence>)tag).Item2;
      }
      return null;
    }
  }
}
