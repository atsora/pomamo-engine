// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Lemoine.DataReferenceControls
{
  /// <summary>
  /// Treeview allowing single and multi selection of elements
  /// A property is defined and represent the parent of each elements
  /// They are then ordered and displayed based on their parent
  /// </summary>
  public partial class RecursiveTreeView : UserControl
  {
    #region Members
    readonly TreeViewBuilderRecursive m_singleSelectionBuilder = new TreeViewBuilderRecursive();
    readonly TreeViewBuilderRecursive m_multipleSelectionBuilder = new TreeViewBuilderRecursive();
    #endregion // Members
    
    #region Getters / Setters
    /// <summary>
    /// Name of the property that will be used to find the parent of each item in the tree
    /// </summary>
    [DefaultValue("")]
    [Description("Property defining the parent of an element.")]
    public string ParentProperty {
      get { return m_singleSelectionBuilder.ParentProperty; }
      set {
        m_singleSelectionBuilder.ParentProperty = value;
        m_multipleSelectionBuilder.ParentProperty = value;
      }
    }
    
    /// <summary>
    /// Property used to display an item
    /// </summary>
    [DefaultValue("")]
    [Description("Property used to display an element.")]
    public string DisplayedProperty {
      get { return m_singleSelectionBuilder.DisplayProperty; }
      set {
        m_singleSelectionBuilder.DisplayProperty = value;
        m_multipleSelectionBuilder.DisplayProperty = value;
      }
    }
    
    /// <summary>
    /// Enable or disable multi selection
    /// After this change the tree has to be repopulated
    /// </summary>
    [DefaultValue(false)]
    [Description("Allow several elements to be selectable and returned as selected elements.")]
    public bool MultiSelection {
      get { return treeView.MultiSelection; }
      set { treeView.MultiSelection = value; }
    }
    
    /// <summary>
    /// True if a node (having at least one child) can be returned as a selected element
    /// </summary>
    [DefaultValue(false)]
    [Description("Allow a node (having at least one child) to be considered as a distinct entity and returned as a selected element.")]
    public bool NodeIsSelectable {
      get { return treeView.NodeIsSelectable; }
      set { treeView.NodeIsSelectable = value; }
    }
    
    /// <summary>
    /// Get or set the selected IDisplayable element (the first one)
    /// </summary>
    [Browsable (false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public object SelectedElement {
      get { return treeView.SelectedElement; }
      set { treeView.SelectedElement = value; }
    }
    
    /// <summary>
    /// Get or set the selected IDisplayable elements (several possible)
    /// </summary>
    [Browsable (false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public IList<object> SelectedElements {
      get { return treeView.SelectedElements; }
      set { treeView.SelectedElements = value; }
    }
    
    /// <summary>
    /// Get the selected paths
    /// </summary>
    [Browsable (false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public IList<IList<object>> SelectedPaths {
      get { return treeView.SelectedPaths; }
    }
    #endregion // Getters / Setters
    
    #region Events
    /// <summary>
    /// Event emitted when selection changed
    /// </summary>
    public event Action SelectionChanged;
    #endregion // Events

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public RecursiveTreeView()
    {
      InitializeComponent();
      treeView.SelectionChanged += OnSelectionChanged;
      treeView.SetBuilder(m_singleSelectionBuilder, m_multipleSelectionBuilder);
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Add a Displayable element in the treeview
    /// "RefreshTreeview" must be called then
    /// </summary>
    /// <param name="element"></param>
    public void AddElement(object element)
    {
      treeView.AddElement(element);
    }
    
    /// <summary>
    /// Clear all elements in the treeview
    /// "RefreshTreeview" must be called then
    /// </summary>
    public void ClearElements()
    {
      treeView.ClearElements();
    }
    
    /// <summary>
    /// Refresh the elements present in the treeview
    /// </summary>
    public void RefreshTreeview()
    {
      treeView.RefreshTreeview();
    }
    
    /// <summary>
    /// Add a function to determine the color of an element
    /// </summary>
    public void AddDetermineColorFunction(Func<object, Color> lambda)
    {
      treeView.AddDetermineColorFunction(lambda);
    }
    #endregion // Methods
    
    #region Event reactions
    void OnSelectionChanged()
    {
      if (SelectionChanged != null) {
        SelectionChanged ();
      }
    }
    #endregion // Event reactions
  }
}
