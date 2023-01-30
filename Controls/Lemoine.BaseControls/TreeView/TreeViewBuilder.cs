// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Lemoine.BaseControls;

namespace Lemoine.BaseControls
{
  /// <summary>
  /// TreeViewBuilder populates a TreeViewWithBuilder based on a series of object
  /// Base features:
  /// - reselection of elements after an update
  /// - re-expand previously expanded elements
  /// </summary>
  public abstract class TreeViewBuilder
  {
    #region Members
    IList<object> m_elements = new List<object>();
    IList<IList<object>> m_storedExpandedElements = new List<IList<object>>();
    #endregion // Members

    #region Getters / Setters
    /// <summary>
    /// Reference to the treeview
    /// </summary>
    internal TreeViewWithBuilder TreeView { get; set; }
    
    /// <summary>
    /// Elements that have to be stored in the treeview
    /// </summary>
    internal protected IList<object> Elements {
      get { return m_elements; }
    }
    
    /// <summary>
    /// Behaviour when the tree is refreshed: the previous selected items
    /// can be selected again
    /// </summary>
    public bool KeepSelectionAfterRefresh { get; set; }
    
    /// <summary>
    /// Behaviour when the tree is refreshed: the previous expanded nodes
    /// are restored.
    /// </summary>
    public bool KeepExpandStatesAfterRefresh { get; set; }
    #endregion // Getters / Setters
    
    #region Constructor
    /// <summary>
    /// Main constructor
    /// </summary>
    protected TreeViewBuilder()
    {
      TreeView = null;
      KeepSelectionAfterRefresh = true;
      KeepExpandStatesAfterRefresh = true;
    }
    #endregion // Constructor

    #region Methods
    /// <summary>
    /// Add an element in the tree
    /// "Refresh" is needed to display them
    /// </summary>
    /// <param name="element"></param>
    public void AddElement(object element)
    {
      m_elements.Add(element);
    }
    
    /// <summary>
    /// Remove all element in the tree
    /// "Refresh" is needed to display the empty tree
    /// </summary>
    public void ClearElements()
    {
      m_elements.Clear();
    }
    
    /// <summary>
    /// Populate the tree based on the elements added
    /// </summary>
    public void Refresh()
    {
      IList<object> selectedElements = null;
      IList<IList<object>> expandedElements = null;
      if (KeepSelectionAfterRefresh) {
        selectedElements = TreeView.SelectedElements;
      }
      if (KeepExpandStatesAfterRefresh) {
        expandedElements = TreeView.ExpandedNodes;
        
        // Merge with stored expanded elements
        foreach (List<object> element in expandedElements) {
          if (IndexOf(element) == -1) {
            m_storedExpandedElements.Add(element);
          }
        }
      }
      
      using (new SuspendDrawing(TreeView)) {
        TreeView.Nodes.Clear();
        BuildTree();
      }
      
      if (KeepSelectionAfterRefresh) {
        TreeView.SelectedElements = selectedElements;
      }
      if (KeepExpandStatesAfterRefresh) {
        TreeView.ExpandedNodes = m_storedExpandedElements;
        
        // Only expanded elements that are not used in the current tree are kept
        expandedElements = TreeView.ExpandedNodes;
        foreach (List<object> element in expandedElements) {
          int index = IndexOf(element);
          if (index != -1) {
            m_storedExpandedElements.RemoveAt(index);
          }
        }
      }
    }
    
    int IndexOf(List<object> list)
    {
      for (int i = 0; i < m_storedExpandedElements.Count; i++) {
        if (m_storedExpandedElements[i].SequenceEqual(list)) {
          return i;
        }
      }
      return -1;
    }
    
    /// <summary>
    /// Build the tree, dispose and sort all elements
    /// </summary>
    protected abstract void BuildTree();
    
    /// <summary>
    /// Add an element to the tree, called when refreshing the tree
    /// </summary>
    /// <param name="label">Name appearing to the user</param>
    /// <param name="tag">Object identifying the node (shouldn't be null)</param>
    /// <param name="parentNode">Node under which the new node will take place
    ///                          Null if a root node has to be added</param>
    protected TreeNode AddElementToTree(string label, object tag, TreeNode parentNode)
    {
      TreeNode nodeAdded;
      nodeAdded = (parentNode == null) ? TreeView.Nodes.Add(label) :
        parentNode.Nodes.Add(label);
      nodeAdded.Tag = tag;
      
      TreeView.OnTreeNodeAdded(nodeAdded);
      return nodeAdded;
    }
    #endregion // Methods
  }
}
