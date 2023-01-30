// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Lemoine.Collections;

namespace Lemoine.BaseControls
{
  /// <summary>
  /// Addition of a TreeViewBuilder to a TreeView
  /// The way to add and dispose element will be the role of the TreeViewBuilder
  /// 
  /// This class is internal: if you want to use a treeview choose "customtreeview" which
  /// combines this treeview with a multi selection treeview comprising tri states
  /// </summary>
  internal class TreeViewWithBuilder : TreeView
  {
    #region Events
    /// <summary>
    /// Event triggered when a checkbox state changed
    /// (different from AfterCheck which is also triggered when expanding the tree)
    /// </summary>
    public event Action SelectionChanged;
    #endregion
    
    #region Members
    TreeViewBuilder m_builder = null;
    bool m_enableSelectionChanged = true;
    readonly NullableDictionary<object, Color> m_itemColors = new NullableDictionary<object, Color>();
    #endregion // Members
    
    #region Getters / Setters
    /// <summary>
    /// Get or set the selected elements
    /// return final and non final nodes (only if NodeIsSelectable) that are selected
    /// </summary>
    [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public IList<object> SelectedElements {
      get {
        List<Object> selectedElements = new List<object>();
        GetSelectedElements(selectedElements, this.Nodes);
        return selectedElements;
      }
      set {
        ClearSelection();
        SelectElements(value, this.Nodes);
      }
    }
    
    /// <summary>
    /// Get the selected paths (non final nodes that are selected)
    /// </summary>
    [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public IList<IList<object>> SelectedPaths {
      get {
        IList<IList<Object>> selectedPaths = new List<IList<object>>();
        IList<object> currentPath = new List<object>();
        GetSelectedNodes(selectedPaths, this.Nodes, currentPath);
        return selectedPaths;
      }
    }
    
    /// <summary>
    /// Get or set the expanded nodes
    /// </summary>
    [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public IList<IList<object>> ExpandedNodes {
      get {
        IList<IList<object>> expandedNodes = new List<IList<object>>();
        IList<object> currentPath = new List<object>();
        GetExpandedNodes(expandedNodes, this.Nodes, currentPath);
        return expandedNodes;
      }
      set {
        // Copy of the list
        IList<IList<object>> copy = new List<IList<object>>();
        foreach (IList<object> element in value) {
          copy.Add(new List<object>(element));
        }

        ExpandNodes (copy, this.Nodes);
      }
    }
    
    /// <summary>
    /// True if a node (having at least one child) can be returned in "SelectedElements"
    /// </summary>
    [DefaultValue(false)]
    [Description("Allow a node to be considered as a distinct entity and returned in SelectedElements.")]
    public bool NodeIsSelectable { get; set; }
    
    /// <summary>
    /// Function to color the elements
    /// </summary>
    public Func<object, Color> ColorFunction { get; set; }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Default constructor
    /// </summary>
    public TreeViewWithBuilder()
    {
      this.HideSelection = false;
      this.NodeIsSelectable = false;
      ColorFunction = null;
      DrawMode = TreeViewDrawMode.OwnerDrawText;
      Utils.SetDoubleBuffered(this);
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Get builder associated to the treeview
    /// </summary>
    public TreeViewBuilder GetBuilder()
    {
      return m_builder;
    }
    
    /// <summary>
    /// Set builder associated to the treeview
    /// </summary>
    /// <param name="builder"></param>
    public void SetBuilder(TreeViewBuilder builder)
    {
      m_builder = builder;
      if (m_builder != null) {
        m_builder.TreeView = this;
      }
    }
    
    /// <summary>
    /// Get the path of an element
    /// </summary>
    /// <param name="elt"></param>
    /// <returns></returns>
    public IList<object> GetPath(object elt)
    {
      return GetElementPath(elt, this.Nodes);
    }
    
    IList<object> GetElementPath(object element, TreeNodeCollection nodes)
    {
      foreach (TreeNode node in nodes) {
        if (object.Equals(element, node.Tag)) {
          var objects = new List<object>();
          objects.Add(node.Tag);
          return objects;
        }
        
        if (node.Nodes.Count != 0) {
          var objects = GetElementPath(element, node.Nodes);
          if (objects != null) {
            objects.Insert(0, node.Tag);
            return objects;
          }
        }
      }
      
      return null;
    }
    
    /// <summary>
    /// Access to the signal "SelectionChanged" for derived classes
    /// </summary>
    protected void EmitSelectionChanged()
    {
      if (SelectionChanged != null) {
        SelectionChanged ();
      }
    }
    
    void GetSelectedElements(List<object> elements, TreeNodeCollection nodes)
    {
      foreach (TreeNode node in nodes) {
        // Browse all children
        if (node.Nodes.Count > 0) {
          GetSelectedElements (elements, node.Nodes);
        }

        // Add an element if it is selected and if it comprises a Tag
        if (IsSelected(node) && node.Tag != null &&
            (NodeIsSelectable || node.Nodes.Count == 0)) // node selectable with a condition
{
          elements.Add(node.Tag);
        }
      }
    }
    
    void GetSelectedNodes(IList<IList<object>> selectedNodes, TreeNodeCollection nodes, IList<object> currentPath)
    {
      foreach (TreeNode node in nodes) {
        if (node.Nodes.Count > 0) {
          currentPath.Add(node.Tag);
          if (IsSelected(node)) {
            selectedNodes.Add(new List<object>(currentPath));
          }

          GetSelectedNodes (selectedNodes, node.Nodes, currentPath);
          currentPath.RemoveAt(currentPath.Count - 1);
        }
      }
    }
    
    void GetExpandedNodes(IList<IList<object>> expandedNodes, TreeNodeCollection nodes, IList<object> currentPath)
    {
      foreach (TreeNode node in nodes) {
        if (node.Nodes.Count > 0) {
          currentPath.Add(node.Tag);
          if (node.IsExpanded) {
            expandedNodes.Add(new List<object>(currentPath));
          }

          GetExpandedNodes (expandedNodes, node.Nodes, currentPath);
          currentPath.RemoveAt(currentPath.Count - 1);
        }
      }
    }
    
    void SelectElements(IList<object> elements, TreeNodeCollection nodes)
    {
      foreach (TreeNode node in nodes) {
        if (node.Nodes.Count > 0) {
          SelectElements (elements, node.Nodes);
        }

        if (elements != null) {
          int index = elements.IndexOf(node.Tag);
          if (index != -1) {
            Select(node);
            elements.RemoveAt(index);
          }
        }
      }
    }
    
    void ExpandNodes(IList<IList<object>> expandedNodes, TreeNodeCollection nodes)
    {
      foreach (TreeNode node in nodes) {
        if (node.Nodes.Count > 0) {
          object currentTag = node.Tag;
          
          IList<IList<object>> correspondingPaths = new List<IList<object>>();
          for (int i = expandedNodes.Count - 1; i >= 0; i--)
          {
            if (expandedNodes[i][0].Equals(node.Tag))
            {
              if (expandedNodes[i].Count == 1) {
                node.Expand();
              }
              else {
                expandedNodes[i].RemoveAt(0);
                correspondingPaths.Add(expandedNodes[i]);
              }
              expandedNodes.RemoveAt(i);
            }
          }
          
          if (correspondingPaths.Count > 0) {
            ExpandNodes (correspondingPaths, node.Nodes);
          }
        }
      }
    }
    
    /// <summary>
    /// Return true if the node is selected
    /// </summary>
    /// <param name="node">node to check</param>
    /// <returns></returns>
    protected virtual bool IsSelected(TreeNode node)
    {
      return node.IsSelected;
    }
    
    /// <summary>
    /// Select the node
    /// </summary>
    /// <param name="node">node to select</param>
    protected virtual void Select(TreeNode node)
    {
      m_enableSelectionChanged = false;
      this.SelectedNode = node;
      m_enableSelectionChanged = true;
    }
    
    /// <summary>
    /// Deselect all nodes
    /// </summary>
    protected virtual void ClearSelection()
    {
      m_enableSelectionChanged = false;
      this.SelectedNode = null;
      m_enableSelectionChanged = true;
    }
    
    /// <summary>
    /// Behaviour when a node is selected
    /// May be overriden to disable the emission of a signal
    /// </summary>
    /// <param name="e"></param>
    protected override void OnAfterSelect(TreeViewEventArgs e)
    {
      base.OnAfterSelect(e);
      if (m_enableSelectionChanged) {
        EmitSelectionChanged ();
      }
    }
    
    /// <summary>
    /// Methods called by the builder when a tree node is added
    /// By default, it does nothing
    /// </summary>
    /// <param name="node"></param>
    protected internal virtual void OnTreeNodeAdded(TreeNode node) {}
    
    /// <summary>
    /// Draw a node and possibly add a color
    /// </summary>
    /// <param name="e"></param>
    protected override void OnDrawNode(DrawTreeNodeEventArgs e)
    {
      if (e.Node == null) {
        return;
      }

      e.DrawDefault = true;
      base.OnDrawNode(e);
      
      // Determine the text color
      var nodeColor = SystemColors.ControlText;
      if (!this.Enabled) {
        nodeColor = SystemColors.GrayText;
      }
      else {
        // Custom function to color an element?
        if (ColorFunction != null) {
          if (!m_itemColors.ContainsKey(e.Node)) {
            m_itemColors[e.Node] = ColorFunction(e.Node.Tag);
          }

          nodeColor = m_itemColors[e.Node];
        }
      }
      
      if (e.Node.ForeColor != nodeColor) {
        e.Node.ForeColor = nodeColor;
      }
    }
    
    /// <summary>
    /// Compute again the colors
    /// </summary>
    public void UpdateColors()
    {
      m_itemColors.Clear();
    }
    #endregion // Methods
  }
}
