// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Lemoine.Model;

namespace Lemoine.DataReferenceControls
{
  /// <summary>
  /// Treeview allowing single and multi selection of elements that implement
  /// the interface "Displayable".
  /// Elements may be ordered based on a series of their properties
  /// Each property must also return objects implementing the interface
  /// "Displayable"
  /// </summary>
  public partial class DisplayableTreeView : UserControl
  {
    #region Members
    List<string[]> m_orderProperties = new List<string[]>();
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
    [DefaultValue(false)]
    [Description("Allow several items to be selected.")]
    public bool MultiSelection {
      get { return treeView.MultiSelection; }
      set {
        treeView.MultiSelection = value;
        ComboBoxSelectedIndexChanged(null, null);
      }
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
    /// Reference to the TreeView (Control from Microsoft)
    /// The reference changes if the selection mode changes!
    /// </summary>
    [Browsable (false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public TreeView TreeView { get { return treeView.TreeView; } }
    
    /// <summary>
    /// Get or set the selected IDisplayable element (the first one)
    /// </summary>
    [Browsable (false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public IDisplayable SelectedElement {
      get { return treeView.SelectedElement as IDisplayable; }
      set { treeView.SelectedElement = value; }
    }
    
    /// <summary>
    /// Get or set the selected IDisplayable elements (several possible)
    /// </summary>
    [Browsable (false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public IList<IDisplayable> SelectedElements {
      get { return treeView.SelectedElements.Cast<IDisplayable>().ToList(); }
      set { treeView.SelectedElements = (value != null) ? value.Cast<object>().ToList() : null; }
    }
    
    /// <summary>
    /// Get the selected paths
    /// </summary>
    [Browsable (false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public IList<IList<IDisplayable>> SelectedPaths {
      get {
        IList<IList<object>> objectPaths = treeView.SelectedPaths;
          
        IList<IList<IDisplayable>> paths = new List<IList<IDisplayable>>();
        foreach (IList<object> objectPath in objectPaths) {
          paths.Add(objectPath.Cast<IDisplayable>().ToList());
        }

        return paths;
      }
    }
    
    /// <summary>
    /// Get or set the selected order in the combobox
    /// </summary>
    [Browsable (false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public int SelectedOrder {
      get { return comboBox.SelectedIndex; }
      set {
        if (value < m_orderProperties.Count) {
          comboBox.SelectedIndex = value;
        }
      }
    }
    #endregion // Getters / Setters
    
    #region Constructors
    /// <summary>
    /// Default constructor, initialize the treeview with a builder
    /// By default, single selection
    /// </summary>
    public DisplayableTreeView()
    {
      InitializeComponent();
      treeView.SetBuilder(new TreeViewBuilderDisplayProperties(),
                          new TreeViewBuilderDisplayProperties());
      treeView.SelectionChanged += OnSelectionChanged;
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Add a possible way to order elements in the tree
    /// </summary>
    /// <param name="label">Text displayed in the combobox</param>
    /// <param name="properties">Array of properties that will be called to order elements</param>
    /// <returns>Index of the order added</returns>
    public int AddOrder(string label, params string[] properties)
    {
      m_orderProperties.Add(properties);
      comboBox.Items.Add(label);
      UpdateComboboxVisibility();
      return m_orderProperties.Count - 1;
    }
    
    /// <summary>
    /// Remove a possible way to order elements
    /// </summary>
    /// <param name="index">Index of the order (returned by the function AddOrder)</param>
    public void RemoveOrder(int index)
    {
      if (index < m_orderProperties.Count) {
        m_orderProperties.RemoveAt(index);
        comboBox.Items.RemoveAt(index);
        UpdateComboboxVisibility();
      }
    }
    
    /// <summary>
    /// Clear all possible ways to order elements
    /// </summary>
    public void ClearOrders()
    {
      m_orderProperties.Clear();
      comboBox.Items.Clear();
      UpdateComboboxVisibility();
    }
    
    void UpdateComboboxVisibility()
    {
      if (m_orderProperties.Count <= 1) {
        baseLayout.RowStyles[1].Height = 0;
        comboBox.Visible = false;
      } else {
        baseLayout.RowStyles[1].Height = 21;
        comboBox.Visible = true;
      }
    }
    
    /// <summary>
    /// Add a Displayable element in the treeview
    /// "RefreshTreeview" must be called then
    /// </summary>
    /// <param name="element"></param>
    public void AddElement(IDisplayable element)
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
    /// Expand all first levels
    /// </summary>
    public void ExpandFirstLevel()
    {
      treeView.ExpandFirstLevel();
    }
    
    /// <summary>
    /// Add a function to determine the color of an element
    /// </summary>
    /// <param name="lambda"></param>
    public void AddDetermineColorFunction(Func<object, Color> lambda)
    {
      treeView.AddDetermineColorFunction(lambda);
    }
    #endregion // Methods
    
    #region Event reactions
    void ComboBoxSelectedIndexChanged(object sender, EventArgs e)
    {
      int index = comboBox.SelectedIndex;
      if (index < 0) {
        index = 0;
      }

      if (m_orderProperties.Count > index) {
        ((TreeViewBuilderDisplayProperties)treeView.TreeViewBuilder).Order = m_orderProperties[index];
        RefreshTreeview();
      }
    }
    
    void OnSelectionChanged()
    {
      if (SelectionChanged != null) {
        SelectionChanged ();
      }
    }
    #endregion // Event reactions
  }
}
