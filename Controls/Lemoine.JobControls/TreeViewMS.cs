// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;

namespace Lemoine.JobControls
{
  /// <summary>
  /// Implementation of a TreeView allowing multi-selection
  /// </summary>
  public class TreeViewMS : TreeView
  {
    #region Public properties

    /// <summary>
    /// Multiple selection of nodes allowed
    /// </summary>
    private bool multiSelect;

    /// <summary>
    /// Multiple selection of nodes allowed
    /// </summary>
    [DescriptionAttribute("Multiple selection of nodes allowed")]
    public bool MultiSelect
    {
      get { return multiSelect; }
      set {
        if (!value) {
          this.ClearPreviousSelection();
        }

        multiSelect = value;
      }
    }

    private List<TreeNode> selectedNodes = new List<TreeNode>();
    /// <summary>
    /// Currently selected nodes
    /// </summary>
    [DescriptionAttribute("Currently selected nodes")]
    public TreeNode[] SelectedNodes
    {
      get
      {
        if (!MultiSelect)
        {
          if (this.SelectedNode == null) {
            return new TreeNode[0];
          }

          return new TreeNode[] { this.SelectedNode };
        }

        foreach (TreeNode treeNode in this.selectedNodes)
        {
          if (treeNode.TreeView == null) {
            this.selectedNodes.Remove(treeNode);
          }
        }

        TreeNode[] treeNodes = null;
        treeNodes = new TreeNode[selectedNodes.Count];
        selectedNodes.CopyTo(treeNodes);
        return treeNodes;
      }
      set
      {
        this.ClearPreviousSelection();
        this.ReleaseCurrentSelection();

        for (int i = 0; i < value.Length; i++)
        {
          if (i == 0)
          {
            this.SelectedNode = value[i];
            this.ShiftStartTreeNode = value[i];
          }
          else
          {
            this.AddToPreviousSelection(value[i]);
          }
        }

        base.OnAfterSelect(new TreeViewEventArgs(this.SelectedNode));
      }
    }
    #endregion

    #region Private methods for manipulating the tree
    private void ShowSelectionState(TreeNode treeNode, bool selected)
    {
      if (treeNode.TreeView == null) {
        return;
      }

      if (selected)
      {
        treeNode.ForeColor = System.Drawing.SystemColors.Window;
        treeNode.BackColor = System.Drawing.SystemColors.Highlight;
      }
      else
      {
        treeNode.ForeColor = System.Drawing.SystemColors.WindowText;
        treeNode.BackColor = System.Drawing.SystemColors.Window;
      }
    }

    private void ClearPreviousSelection()
    {
      foreach (TreeNode treeNode in this.selectedNodes)
      {
        this.ShowSelectionState(treeNode, false);
      }
      this.selectedNodes.Clear();
    }

    private void AddToPreviousSelection(TreeNode treeNode)
    {
      if (!this.selectedNodes.Contains(treeNode))
      {
        this.selectedNodes.Add(treeNode);
        this.ShowSelectionState(treeNode, true);
      }
    }

    private void RemoveFromPreviousSelection(TreeNode treeNode)
    {
      if (this.selectedNodes.Contains(treeNode))
      {
        this.selectedNodes.Remove(treeNode);
        this.ShowSelectionState(treeNode, false);
      }
    }

    private TreeNode ShiftStartTreeNode = null;

    private void AddFromShiftStartToPreviousSelection(TreeNode treeNode)
    {
      if (ShiftStartTreeNode == null)
      {
        this.AddToPreviousSelection(treeNode);
      }
      else
      {
        int direction = this.ComparePosition(ShiftStartTreeNode, treeNode);
        TreeNode tempTreeNode = ShiftStartTreeNode;
        while (tempTreeNode != treeNode)
        {
          this.AddToPreviousSelection(tempTreeNode);
          if (direction > 0) {
            tempTreeNode = tempTreeNode.NextVisibleNode;
          }
          else {
            tempTreeNode = tempTreeNode.PrevVisibleNode;
          }
        }
        this.AddToPreviousSelection(tempTreeNode);
      }
    }

    /// <summary>
    /// Compares the viewposition of nodes
    /// -1 y is before x
    ///  0 y is x
    /// +1 y is after x
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    private int ComparePosition(TreeNode x, TreeNode y)
    {
      if (x == y) {
        return 0;
      }

      int ret = 0;

      if (y.Level > x.Level)
      {
        ret = this.ComparePosition(x, y.Parent);
        if (ret == 0) {
          return 1;
        }

        return ret;
      }

      if (x.Level > y.Level)
      {
        ret = this.ComparePosition(x.Parent, y);
        if (ret == 0) {
          return -1;
        }

        return ret;
      }

      if (x.Parent != y.Parent) {
        return this.ComparePosition(x.Parent, y.Parent);
      }

      if (x.Index < y.Index) {
        return 1;
      }

      return -1;
    }

    private void ReleaseCurrentSelection()
    {
      this.SelectedNode = null;
    }

    private void SetCurrentSelection(TreeNode treeNode)
    {
      this.SelectedNode = treeNode;
    }
    #endregion

    #region Event reactions
    private void SelectionWithoutStrgAndShift()
    {
      this.SuspendLayout();
      this.ClearPreviousSelection();
      this.AddToPreviousSelection(this.SelectedNode);
      this.ShiftStartTreeNode = this.SelectedNode;
      this.ResumeLayout();
    }

    private void SelectionWithShiftWithoutStrg()
    {
      this.SuspendLayout();
      this.ClearPreviousSelection();
      this.AddFromShiftStartToPreviousSelection(this.SelectedNode);
      this.ResumeLayout();
    }

    private void SelectionWithStrgToUnselected()
    {
      this.SuspendLayout();
      this.AddToPreviousSelection(this.SelectedNode);
      this.ShiftStartTreeNode = this.SelectedNode;
      this.ResumeLayout();
    }

    private void SelectionWithStrgToSelected()
    {
      this.SuspendLayout();
      this.RemoveFromPreviousSelection(this.SelectedNode);
      this.ShiftStartTreeNode = this.SelectedNode;
      this.ReleaseCurrentSelection();
      this.ResumeLayout();
    }
    #endregion

    #region Protected overrides
    /// <summary>
    /// Override method associated with after select event
    /// </summary>
    /// <param name="e"></param>
    protected override void OnAfterSelect(TreeViewEventArgs e)
    {
      if (this.MultiSelect)
      {
        if ((Control.ModifierKeys & Keys.Control) != 0)
        {
          if (this.selectedNodes.Contains(this.SelectedNode)) {
            this.SelectionWithStrgToSelected();
          }
          else {
            this.SelectionWithStrgToUnselected();
          }
        }
        else if ((Control.ModifierKeys & Keys.Shift) != 0) {
          this.SelectionWithShiftWithoutStrg();
        }
        else {
          this.SelectionWithoutStrgAndShift();
        }
      }      
      base.OnAfterSelect(e);
    }
    
    
    /// <summary>
    /// Override methods associated with mouse down event
    /// </summary>
    /// <param name="e"></param>
    protected override void OnMouseDown(MouseEventArgs e)
    {
      base.OnMouseDown(e);

      TreeViewHitTestInfo treeViewHitTestInfo = null;
      switch (e.Button)
      {
        case MouseButtons.Right:

          treeViewHitTestInfo = this.HitTest(e.Location);
          if (treeViewHitTestInfo.Location == TreeViewHitTestLocations.Label ||
              treeViewHitTestInfo.Location == TreeViewHitTestLocations.RightOfLabel)
          {
            if (!this.selectedNodes.Contains(treeViewHitTestInfo.Node))
            {
              this.ClearPreviousSelection();
              this.SelectedNode = treeViewHitTestInfo.Node;
            }
          }
          break;

        case MouseButtons.Left:

          treeViewHitTestInfo = this.HitTest(e.Location);
          if (treeViewHitTestInfo.Location == TreeViewHitTestLocations.RightOfLabel)
          {
            this.SelectedNode = treeViewHitTestInfo.Node;
          }
          break;
      }
    }
    #endregion
  }
}
