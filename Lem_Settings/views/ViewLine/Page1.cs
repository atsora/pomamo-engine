// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Lemoine.Settings;
using Lemoine.BaseControls;
using Lemoine.Model;
using Lemoine.ModelDAO;

namespace ViewLine
{
  /// <summary>
  /// Description of Page1.
  /// </summary>
  internal partial class Page1 : GenericViewPage, IViewPage
  {
    #region Members
    readonly LineDetails m_lineDetails = new LineDetails();
    readonly OperationDetails m_operationDetails = new OperationDetails();
    readonly MachineDetails m_machineDetails = new MachineDetails();
    #endregion // Members
    
    #region Getters / Setters
    /// <summary>
    /// Title
    /// </summary>
    public string Title { get { return "Lines"; } }
    
    /// <summary>
    /// Some help to fill the page
    /// </summary>
    public string Help { get { return "Overview of all lines, with their operations and machines associated.\n\n" +
          "The first level of the tree comprises all existing lines. " +
          "The second level represents the operations associated to the lines. " +
          "The third level lists all machines involved in an operation.\n\n" +
          "Clicking on an element displays its details."; } }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public Page1()
    {
      InitializeComponent();
      m_lineDetails.Dock = m_operationDetails.Dock = m_machineDetails.Dock = DockStyle.Fill;
    }
    #endregion // Constructors

    #region Page methods
    /// <summary>
    /// Initialization of the page, happening before the first load
    /// This method is called again if the database has been updated
    /// </summary>
    /// <param name="context">Context of the item</param>
    public void Initialize(ItemContext context) {}
    
    /// <summary>
    /// Load the page
    /// </summary>
    /// <param name="data">data containing parameters</param>
    public void LoadPageFromData(ItemData data)
    {
      FillTreeView();
    }
    
    /// <summary>
    /// Save the parameters
    /// </summary>
    /// <param name="data">data to be modified</param>
    public void SavePageInData(ItemData data) {}
    #endregion // Page methods
    
    #region Private methods
    void FillTreeView()
    {
      treeView.Nodes.Clear();
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession()) {
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction()) {
          IList<ILine> lines = ModelDAOHelper.DAOFactory.LineDAO.FindAll();
          foreach (ILine line in lines) {
            // Create a node
            TreeNode nodeLine = treeView.Nodes.Add(line.Display);
            nodeLine.Tag = line;
            
            // Retrieve operations
            IList<IOperation> operations = new List<IOperation>();
            if (line.Components != null) {
              foreach ( IComponent component in line.Components) {
                if (component.ComponentIntermediateWorkPieces != null) {
                  foreach (IComponentIntermediateWorkPiece ciwp in component.ComponentIntermediateWorkPieces) {
                    if (ciwp.IntermediateWorkPiece != null && ciwp.IntermediateWorkPiece.Operation != null) {
                      operations.Add(ciwp.IntermediateWorkPiece.Operation);
                    }
                  }
                }
              }
            }
            
            // Create nodes
            IDictionary<IOperation, TreeNode> nodes = new Dictionary<IOperation, TreeNode>();
            foreach (IOperation operation in operations) {
              TreeNode nodeOperation = nodeLine.Nodes.Add(operation.Display);
              nodeOperation.Tag = operation;
              nodes[operation] = nodeOperation;
            }
            
            // Retrieve machines
            IList<ILineMachine> lineMachines = ModelDAOHelper.DAOFactory.LineMachineDAO.FindAllByLine(line);
            
            foreach (ILineMachine lineMachine in lineMachines) {
              if (nodes.ContainsKey(lineMachine.Operation)) {
                TreeNode nodeOperation = nodes[lineMachine.Operation];
                TreeNode nodeMachine = nodeOperation.Nodes.Add(lineMachine.Machine.Display);
                nodeMachine.Tag = lineMachine;
              }
            }
            
            nodeLine.Expand();
          }
        }
      }
      
      treeView.Sort();
    }
    #endregion // Private methods
    
    #region Event reactions
    void TreeViewAfterSelect(object sender, TreeViewEventArgs e)
    {
      using (new SuspendDrawing(treeView)) {
        TreeNode selectedNode = treeView.SelectedNode;
        panel.Controls.Clear();
        if (selectedNode.Tag is ILine) {
          m_lineDetails.Init(selectedNode.Tag as ILine);
          panel.Controls.Add(m_lineDetails);
          labelDetails.Text = "Line details";
        } else if (selectedNode.Tag is IOperation) {
          m_operationDetails.Init(selectedNode.Tag as IOperation);
          panel.Controls.Add(m_operationDetails);
          labelDetails.Text = "Operation details";
        } else if (selectedNode.Tag is ILineMachine) {
          m_machineDetails.Init(selectedNode.Tag as ILineMachine);
          panel.Controls.Add(m_machineDetails);
          labelDetails.Text = "Machine details";
        } else {
          labelDetails.Text = "Details";
        }
      }
    }
    #endregion // Event reactions
  }
}
