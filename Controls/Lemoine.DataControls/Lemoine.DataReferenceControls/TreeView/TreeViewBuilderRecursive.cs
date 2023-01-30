// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Lemoine.BaseControls;

namespace Lemoine.DataReferenceControls
{
  /// <summary>
  /// Description of TreeViewBuilderRecursive
  /// </summary>
  public class TreeViewBuilderRecursive : TreeViewBuilder
  {
    class Node
    {
      public object NodeObject { get; private set; }
      public IList<Node> Childs { get; private set; }
      
      public Node(object nodeObject)
      {
        Childs = new List<Node>();
        NodeObject = nodeObject;
      }
      
      /// <summary>
      /// Try to store an object in a node or in a child of this node
      /// </summary>
      /// <param name="objectToStore"></param>
      /// <param name="parentProperty">property to find the parent of an object</param>
      /// <returns>True if the object is stored</returns>
      public bool StoreObject(object objectToStore, string parentProperty)
      {
        // Parent of the object
        object parent = objectToStore.GetType().GetProperty(parentProperty).GetValue(objectToStore, null);
        
        // Try to store the object in the current node
        if (parent == null || Object.Equals(NodeObject, parent)) {
          Childs.Add(new Node(objectToStore));
          return true;
        }
        
        // Try to store the object in a child
        foreach (var child in Childs) {
          if (child.StoreObject(objectToStore, parentProperty)) {
            return true;
          }
        }

        // Notify that the object has not been stored
        return false;
      }
    }

    #region Getters / Setters
    /// <summary>
    /// Property used for the recursion (parent object)
    /// must be the same
    /// </summary>
    public string ParentProperty { get; set; }
    
    /// <summary>
    /// Property used to display an element
    /// </summary>
    public string DisplayProperty { get; set; }
    #endregion // Getters / Setters

    #region Methods
    /// <summary>
    /// Build the tree, dispose and sort all elements
    /// </summary>
    protected override void BuildTree()
    {
      // Copy of the items to store
      var elementsToStore = new List<object>(Elements);
      
      // Store all objects in nodes
      var rootNode = new Node(null);
      bool elementAdded = true;
      while (elementsToStore.Count > 0 && elementAdded) {
        // Store all objects having a parent in the tree
        elementAdded = false;
        for (int i = elementsToStore.Count - 1; i >= 0; i--) {
          if (rootNode.StoreObject(elementsToStore[i], ParentProperty)) {
            elementsToStore.RemoveAt(i);
            elementAdded = true;
          }
        }
      }
      
      // Populate the tree
      foreach (Node node in rootNode.Childs) {
        BuildTree (node, null);
      }
    }
    
    void BuildTree(Node node, TreeNode parentNode)
    {
      // Create a TreeNode under parentNode
      object objectToDisplay = node.NodeObject;
      TreeNode treeNode = AddElementToTree(GetDisplay(objectToDisplay),
                                           objectToDisplay, parentNode);
      
      // Childs
      foreach (var child in node.Childs) {
        BuildTree (child, treeNode);
      }
    }
    
    string GetDisplay(object element)
    {
      string txt = "-";
      if (element == null) {
        return txt;
      }

      if (string.IsNullOrEmpty(DisplayProperty)) {
        txt = element.ToString();
      }
      else {
        object prop = element.GetType().GetProperty(DisplayProperty).GetValue(element, null);
        if (prop != null) {
          txt = prop.ToString();
        }
      }
      
      return txt;
    }
    #endregion // Methods
  }
}
