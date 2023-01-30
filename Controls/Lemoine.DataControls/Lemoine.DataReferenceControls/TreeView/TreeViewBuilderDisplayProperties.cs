// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows.Forms;
using Lemoine.BaseControls;
using Lemoine.Model;

namespace Lemoine.DataReferenceControls
{
  /// <summary>
  /// Used with a treeViewWithBuilder, this builder provides convenient features to
  /// display a series of IDisplayable objects comprising properties being also
  /// IDisplayable objects.
  /// The tree is automatically populated based on the specified properties and order.
  /// </summary>
  public class TreeViewBuilderDisplayProperties : TreeViewBuilder
  {
    #region Members
    string[] m_properties;
    #endregion // Members

    #region Getters / Setters
    /// <summary>
    /// List of the properties used to sort elements in the tree
    /// </summary>
    public string[] Order {
      get { return m_properties; }
      set { m_properties = value; }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Default constructor
    /// </summary>
    public TreeViewBuilderDisplayProperties ()
    {
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Build the tree, dispose and sort all elements
    /// </summary>
    protected override void BuildTree()
    {
      if (Order != null) {
        BuildTree (new List<string>(Order), Elements, null);
      }
      else {
        BuildTree (new List<string>(), Elements, null);
      }
    }
    
    private void BuildTree(IList<string> properties, IList<object> elements, TreeNode parentNode)
    {
      if (properties.Count != 0) {
        string currentProperty = properties[0];
        properties.RemoveAt(0);
        
        Dictionary<IDisplayable, IList<object>> map = new Dictionary<IDisplayable, IList<object>>();
        IList<object> objectsWithNoKey = new List<object>();
        foreach(Object element in elements) {
          PropertyInfo info = element.GetType().GetProperty(currentProperty);
          IDisplayable displayableProperty = info.GetValue(element, null) as IDisplayable;
          if (displayableProperty == null) {
            objectsWithNoKey.Add(element);
          } else {
            if (!map.ContainsKey(displayableProperty)) {
              map.Add(displayableProperty, new List<object>());
            }

            map[displayableProperty].Add(element);
          }
        }
        
        foreach (IDisplayable displayableProperty in map.Keys) {
          TreeNode node = AddElementToTree(displayableProperty.Display,
                                          displayableProperty, parentNode);
          BuildTree(new List<string>(properties), map[displayableProperty], node);
        }
        
        if (objectsWithNoKey.Count > 0) {
          // No category for these elements
          TreeNode node = AddElementToTree("Unknown", currentProperty, parentNode);
          BuildTree(properties, objectsWithNoKey, node);
        }
      } else {
        foreach (Object element in elements) {
          AddElementToTree(((IDisplayable)element).Display, element, parentNode);
        }
      }
    }
    #endregion // Methods
  }
}
