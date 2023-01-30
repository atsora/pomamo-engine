// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Windows.Forms;

namespace Lemoine.BaseControls
{
  /// <summary>
  /// Stop and resume drawing of a Control
  /// (may prevent flickering)
  /// </summary>
  public class SuspendDrawing : IDisposable
  {
    #region Members
    static private readonly int WM_SETREDRAW = 0x000B;
    private readonly Control m_control;
    #endregion // Members

    #region Methods
    /// <summary>
    /// Suspend the drawing of a control, until this object is deleted.
    /// Should be used with using new SuspendDrawing...
    /// This class is usefull when a listbox has to be cleared and then populated.
    /// Flickering is avoided.
    /// </summary>
    /// <param name="control"></param>
    public SuspendDrawing(Control control)
    {
      m_control = control;
      
      Message msgSuspendUpdate = Message.Create(m_control.Handle, WM_SETREDRAW, IntPtr.Zero,
                                                IntPtr.Zero);

      NativeWindow window = NativeWindow.FromHandle(m_control.Handle);
      window.DefWndProc(ref msgSuspendUpdate);
    }
    
    /// <summary>
    /// Dispose method to be used with using()
    /// </summary>
    public void Dispose()
    {
      // Create a C "true" boolean as an IntPtr
      var wparam = new IntPtr(1);
      Message msgResumeUpdate = Message.Create(m_control.Handle, WM_SETREDRAW, wparam,
                                               IntPtr.Zero);

      NativeWindow window = NativeWindow.FromHandle(m_control.Handle);
      window.DefWndProc(ref msgResumeUpdate);

      m_control.Invalidate();
      m_control.Refresh();
    }
    #endregion // Methods
  }
}
