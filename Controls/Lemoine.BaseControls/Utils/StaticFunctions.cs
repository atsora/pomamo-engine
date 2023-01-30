// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace Lemoine.BaseControls
{
  /// <summary>
  /// Class containing differents functions
  /// </summary>
  public static class Utils
  {
    /// <summary>
    /// Hack to activate the double buffering (private property)
    /// Double buffering is usefull to remove glitches on complex controls
    /// (for example a control made of several parts which reacts to size events)
    /// </summary>
    /// <param name="c"></param>
    public static void SetDoubleBuffered(Control c)
    {
      // https://stackoverflow.com/questions/76993/how-to-double-buffer-net-controls-on-a-form
//      if (SystemInformation.TerminalServerSession) // It is advised but appears to be not necessary
//        return;

      System.Reflection.PropertyInfo aProp = typeof(Control).GetProperty(
        "DoubleBuffered", System.Reflection.BindingFlags.NonPublic |
        System.Reflection.BindingFlags.Instance);
      aProp.SetValue(c, true, null);
      
      // https://stackoverflow.com/questions/10362988/treeview-flickering
      const int TVM_SETEXTENDEDSTYLE = 0x1100 + 44;
      const int TVS_EX_DOUBLEBUFFER = 0x0004;
      SendMessage(c.Handle, TVM_SETEXTENDEDSTYLE, (IntPtr)TVS_EX_DOUBLEBUFFER, (IntPtr)TVS_EX_DOUBLEBUFFER);
    }
    [DllImport("user32.dll")]
    private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wp, IntPtr lp);
    
    /// <summary>
    /// Clear and dispose all controls from a collection
    /// </summary>
    /// <param name="controls"></param>
    public static void ClearControls(Control.ControlCollection controls)
    {
      while (controls.Count > 0) {
        Control c = controls[0];
        controls.Remove(c);
        c.Dispose();
      }
    }
  }
}
