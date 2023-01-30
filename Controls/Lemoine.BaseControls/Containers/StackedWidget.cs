// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Windows.Forms;

namespace Lemoine.BaseControls
{
  /// <summary>
  /// StackedWidget like in Qt
  /// (TabControl with no header)
  /// </summary>
  public class StackedWidget : TabControl
  {
    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public StackedWidget() : base() {}
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// cf http://stackoverflow.com/questions/6953487/hide-tab-header-on-c-sharp-tabcontrol
    /// </summary>
    /// <param name="m"></param>
    protected override void WndProc(ref Message m)
    {
      // Hide the tab headers at run-time
      if (m.Msg == 0x1328 && !DesignMode) {
        m.Result = (IntPtr)1;
      }
      else {
        base.WndProc(ref m);
      }
    }
    #endregion // Methods
  }
}
