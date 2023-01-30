// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Windows.Forms;

using Lemoine.Model;

namespace Lemoine.JobControls
{
  /// <summary>
  /// Description of ITreeViewObservable.
  /// </summary>
  public interface ITreeViewObservable
  {
    /// <summary>
    ///   Add an observer
    /// </summary>
    /// <param name="observer"></param>
    void AddObserver(ITreeViewObserver observer);
    
    /// <summary>
    ///   delete an observer
    /// </summary>
    /// <param name="observer"></param>
    void DeleteObserver(ITreeViewObserver observer);
    
    /// <summary>
    ///   Notify all the observers related to change theirs states
    /// </summary>
    void NotifyObservers();
    
    /// <summary>
    ///   Get selected TreeNode
    /// </summary>
    /// <returns></returns>
    TreeNode GetSelectedNode();
    
    /// <summary>
    /// Get the object type of a node
    /// </summary>
    /// <param name="node"></param>
    /// <returns></returns>
    Type GetItemType (TreeNode node);
      
    /// <summary>
    ///   Initialization of this observable
    /// </summary>
    void Init();
    
    /// <summary>
    /// Give the focus to all the nodes with the same type and name
    /// </summary>
    /// <param name="type"></param>
    /// <param name="data"></param>
    /// <returns>Number of found nodes</returns>
    int GiveFocusToAllNodeInstances(Type type, Lemoine.Collections.IDataWithId data);
    
    /// <summary>
    /// Reload nodes with the same type and name
    /// </summary>
    /// <param name="node"></param>
    /// <returns></returns>
    IList<TreeNode> ReloadTreeNodes(TreeNode node);
  }
}
