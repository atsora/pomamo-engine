// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Lemoine.BaseControls
{
  /// <summary>
  /// TreeView that is built with a TreeViewBuilder
  /// Single and multi selection are allowed
  /// </summary>
  public partial class CustomTreeView : UserControl
  {
    #region Members
    TreeViewTriState m_multiTreeView = new TreeViewTriState ();
    TreeViewWithBuilder m_singleTreeView = new TreeViewWithBuilder ();
    bool m_multiEnabled = false;
    readonly Pen PEN = new Pen (SystemColors.ControlDark);
    #endregion // Members

    #region Events
    /// <summary>
    /// Event emitted when selection changed
    /// </summary>
    public event Action SelectionChanged;
    #endregion // Events

    #region Getters / Setters
    /// <summary>
    /// Enable or disable multi selection
    /// After this change the tree has to be repopulated
    /// </summary>
    [DefaultValue (false)]
    [Description ("Allow several items to be selected.")]
    public bool MultiSelection
    {
      get {
        return m_multiEnabled;
      }
      set {
        m_multiEnabled = value;
        if (m_multiEnabled) {
          baseLayout.Controls.Remove (m_singleTreeView);
          baseLayout.Controls.Add (m_multiTreeView, 0, 0);
        }
        else {
          baseLayout.Controls.Remove (m_multiTreeView);
          baseLayout.Controls.Add (m_singleTreeView, 0, 0);
        }
      }
    }

    /// <summary>
    /// True if a node (having at least one child) can be returned in "SelectedElements"
    /// </summary>
    [DefaultValue (false)]
    [Description ("Allow a node (having at least one child) to be considered as a distinct entity and returned as a selected element.")]
    public bool NodeIsSelectable
    {
      get { return m_singleTreeView.NodeIsSelectable; }
      set {
        m_multiTreeView.NodeIsSelectable = value;
        m_singleTreeView.NodeIsSelectable = value;
      }
    }

    /// <summary>
    /// Reference to the treeview
    /// The reference changes if the selection mode changes!
    /// </summary>
    [Browsable (false), DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
    public TreeView TreeView
    {
      get { return CurrentTreeView; }
    }

    /// <summary>
    /// Internal reference to the current treeview, as a TreeViewWithBuilder
    /// </summary>
    [Browsable (false), DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
    TreeViewWithBuilder CurrentTreeView
    {
      get { return MultiSelection ? m_multiTreeView : m_singleTreeView; }
    }

    /// <summary>
    /// Reference to the treeviewbuilder
    /// The reference changes if the selection mode changes!
    /// </summary>
    [Browsable (false), DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
    public TreeViewBuilder TreeViewBuilder
    {
      get { return CurrentTreeView.GetBuilder (); }
    }

    /// <summary>
    /// Get or set the selected IDisplayable elements (several possible)
    /// </summary>
    [Browsable (false), DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
    public IList<object> SelectedElements
    {
      get { return CurrentTreeView.SelectedElements; }
      set { CurrentTreeView.SelectedElements = value; }
    }

    /// <summary>
    /// Get the selected paths
    /// </summary>
    [Browsable (false), DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
    public IList<IList<object>> SelectedPaths
    {
      get { return CurrentTreeView.SelectedPaths; }
    }

    /// <summary>
    /// Get or set the selected IDisplayable element (the first one)
    /// </summary>
    [Browsable (false), DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
    public object SelectedElement
    {
      get {
        var elements = CurrentTreeView.SelectedElements;
        return elements.Count > 0 ? elements[0] : null;
      }
      set {
        if (value != null) {
          // Expand the tree to see the selected element
          if (!MultiSelection) {
            var lists = new List<IList<object>> ();
            lists.Add (CurrentTreeView.GetPath (value));
            if (lists.Count > 0) {
              var copy = new List<object> (lists[0]);
              while (copy.Count > 1) {
                copy.RemoveAt (copy.Count - 1);
                lists.Add (new List<object> (copy));
              }
              lists.RemoveAt (0);
              CurrentTreeView.ExpandedNodes = lists;
            }
          }

          // Select the element
          var elements = new List<object> ();
          elements.Add (value);
          CurrentTreeView.SelectedElements = elements;
        }
        else {
          CurrentTreeView.SelectedElements = null;
        }
      }
    }

    /// <summary>
    /// Get or set the expanded nodes
    /// A list of path (made with the object contained in the tree)
    /// must be provided
    /// </summary>
    [Browsable (false), DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
    public IList<IList<object>> ExpandedNodes
    {
      get { return CurrentTreeView.ExpandedNodes; }
      set { CurrentTreeView.ExpandedNodes = value; }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public CustomTreeView ()
    {
      InitializeComponent ();

      // Style of the embedded treeviews
      m_multiTreeView.Margin = new Padding (0);
      m_singleTreeView.BorderStyle = BorderStyle.None;
      m_multiTreeView.Dock = DockStyle.Fill;
      m_singleTreeView.Margin = new Padding (0);
      m_multiTreeView.BorderStyle = BorderStyle.None;
      m_singleTreeView.Dock = DockStyle.Fill;

      // By default, single selection
      m_multiEnabled = false;
      baseLayout.Controls.Add (m_singleTreeView, 0, 0);

      // Connect the treeviews when the selection changed
      m_multiTreeView.SelectionChanged += OnSelectionChanged;
      m_singleTreeView.SelectionChanged += OnSelectionChanged;
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Set the builder
    /// Must be set before adding any element
    /// </summary>
    /// <param name="singleSelectionBuilder">builder for the single selection mode</param>
    /// <param name="multipleSelectionBuilder">builder for the multiple selection mode</param>
    public void SetBuilder (TreeViewBuilder singleSelectionBuilder, TreeViewBuilder multipleSelectionBuilder)
    {
      m_multiTreeView.SetBuilder (singleSelectionBuilder);
      m_singleTreeView.SetBuilder (multipleSelectionBuilder);
    }

    /// <summary>
    /// Add an element in the treeview
    /// "RefreshTreeview" must be called then so that all elements are displayed
    /// </summary>
    /// <param name="element"></param>
    public void AddElement (object element)
    {
      m_singleTreeView.GetBuilder ().AddElement (element);
      m_multiTreeView.GetBuilder ().AddElement (element);
    }

    /// <summary>
    /// Clear all elements in the treeview
    /// "RefreshTreeview" must be called then
    /// </summary>
    public void ClearElements ()
    {
      m_singleTreeView.GetBuilder ().ClearElements ();
      m_multiTreeView.GetBuilder ().ClearElements ();
    }

    /// <summary>
    /// Refresh the elements present in the treeview
    /// </summary>
    public void RefreshTreeview ()
    {
      CurrentTreeView.UpdateColors ();
      CurrentTreeView.GetBuilder ().Refresh ();
    }

    /// <summary>
    /// Expand all first levels
    /// </summary>
    public void ExpandFirstLevel ()
    {
      foreach (TreeNode node in CurrentTreeView.Nodes) {
        node.Expand ();
      }
    }

    /// <summary>
    /// Add a function to determine the color of an element
    /// </summary>
    public void AddDetermineColorFunction (Func<object, Color> lambda)
    {
      m_multiTreeView.ColorFunction = lambda;
      m_singleTreeView.ColorFunction = lambda;
    }
    #endregion // Methods

    #region Event reactions
    void OnSelectionChanged ()
    {
      if (SelectionChanged != null) {
        SelectionChanged ();
      }
    }

    void BaseLayoutCellPaint (object sender, TableLayoutCellPaintEventArgs e)
    {
      // Top border
      if (e.Row > 0) {
        e.Graphics.DrawLine (PEN, e.CellBounds.Left, e.CellBounds.Top,
                            e.CellBounds.Right, e.CellBounds.Top);
      }
    }
    #endregion // Event reactions
  }
}
