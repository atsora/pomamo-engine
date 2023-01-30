// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Windows.Forms;
using System.Drawing;
using System.Runtime.InteropServices;

namespace Lemoine.BaseControls
{
  /// <summary>
  /// Class identical to MessageBox, except that it will appear at the center
  /// of the parent window
  /// </summary>
  public class MessageBoxCentered
  {
    /// <summary>
    /// Show dialog centered in the current active window
    /// </summary>
    /// <param name="text">Text inside the window</param>
    /// <returns></returns>
    public static DialogResult Show(string text)
    {
      Initialize();
      return MessageBox.Show(text);
    }

    /// <summary>
    /// Show dialog centered in the current active window
    /// </summary>
    /// <param name="text">Text inside the window</param>
    /// <param name="caption">Window title</param>
    /// <returns></returns>
    public static DialogResult Show(string text, string caption)
    {
      Initialize();
      return MessageBox.Show(text, caption);
    }

    /// <summary>
    /// Show dialog centered in the current active window
    /// </summary>
    /// <param name="text">Text inside the window</param>
    /// <param name="caption">Window title</param>
    /// <param name="buttons">Series of buttons (MessageBoxButtons.OK for instance)</param>
    /// <returns></returns>
    public static DialogResult Show(string text, string caption, MessageBoxButtons buttons)
    {
      Initialize();
      return MessageBox.Show(text, caption, buttons);
    }

    /// <summary>
    /// Show dialog centered in the current active window
    /// </summary>
    /// <param name="text">Text inside the window</param>
    /// <param name="caption">Window title</param>
    /// <param name="buttons">Series of buttons (MessageBoxButtons.OK for instance)</param>
    /// <param name="icon">Icon (MessageBoxIcon.Warning for instance)</param>
    /// <returns></returns>
    public static DialogResult Show(string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon)
    {
      Initialize();
      return MessageBox.Show(text, caption, buttons, icon);
    }

    /// <summary>
    /// Show dialog centered in the current active window
    /// </summary>
    /// <param name="text">Text inside the window</param>
    /// <param name="caption">Window title</param>
    /// <param name="buttons">Series of buttons (MessageBoxButtons.OK for instance)</param>
    /// <param name="icon">Icon (MessageBoxIcon.Warning for instance)</param>
    /// <param name="defButton">Button selected by default</param>
    /// <returns></returns>
    public static DialogResult Show(string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon, MessageBoxDefaultButton defButton)
    {
      Initialize();
      return MessageBox.Show(text, caption, buttons, icon, defButton);
    }

    /// <summary>
    /// Show dialog centered in the current active window
    /// </summary>
    /// <param name="text">Text inside the window</param>
    /// <param name="caption">Window title</param>
    /// <param name="buttons">Series of buttons (MessageBoxButtons.OK for instance)</param>
    /// <param name="icon">Icon (MessageBoxIcon.Warning for instance)</param>
    /// <param name="defButton">Button selected by default</param>
    /// <param name="options">Options of the dialog (MessageBoxOptions.RightAlign for instance)</param>
    /// <returns></returns>
    public static DialogResult Show(string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon, MessageBoxDefaultButton defButton, MessageBoxOptions options)
    {
      Initialize();
      return MessageBox.Show(text, caption, buttons, icon, defButton, options);
    }

    /// <summary>
    /// Show dialog centered in the owner
    /// </summary>
    /// <param name="owner">Parent window</param>
    /// <param name="text">Text inside the window</param>
    /// <returns></returns>
    public static DialogResult Show(IWin32Window owner, string text)
    {
      _owner = owner;
      Initialize();
      return MessageBox.Show(owner, text);
    }

    /// <summary>
    /// Show dialog centered in the owner
    /// </summary>
    /// <param name="owner">Parent window</param>
    /// <param name="text">Text inside the window</param>
    /// <param name="caption">Window title</param>
    /// <returns></returns>
    public static DialogResult Show(IWin32Window owner, string text, string caption)
    {
      _owner = owner;
      Initialize();
      return MessageBox.Show(owner, text, caption);
    }

    /// <summary>
    /// Show dialog centered in the owner
    /// </summary>
    /// <param name="owner">Parent window</param>
    /// <param name="text">Text inside the window</param>
    /// <param name="caption">Window title</param>
    /// <param name="buttons">Series of buttons (MessageBoxButtons.OK for instance)</param>
    /// <returns></returns>
    public static DialogResult Show(IWin32Window owner, string text, string caption, MessageBoxButtons buttons)
    {
      _owner = owner;
      Initialize();
      return MessageBox.Show(owner, text, caption, buttons);
    }

    /// <summary>
    /// Show dialog centered in the owner
    /// </summary>
    /// <param name="owner">Parent window</param>
    /// <param name="text">Text inside the window</param>
    /// <param name="caption">Window title</param>
    /// <param name="buttons">Series of buttons (MessageBoxButtons.OK for instance)</param>
    /// <param name="icon">Icon (MessageBoxIcon.Warning for instance)</param>
    /// <returns></returns>
    public static DialogResult Show(IWin32Window owner, string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon)
    {
      _owner = owner;
      Initialize();
      return MessageBox.Show(owner, text, caption, buttons, icon);
    }

    /// <summary>
    /// Show dialog centered in the owner
    /// </summary>
    /// <param name="owner">Parent window</param>
    /// <param name="text">Text inside the window</param>
    /// <param name="caption">Window title</param>
    /// <param name="buttons">Series of buttons (MessageBoxButtons.OK for instance)</param>
    /// <param name="icon">Icon (MessageBoxIcon.Warning for instance)</param>
    /// <param name="defButton">Button selected by default</param>
    /// <returns></returns>
    public static DialogResult Show(IWin32Window owner, string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon, MessageBoxDefaultButton defButton)
    {
      _owner = owner;
      Initialize();
      return MessageBox.Show(owner, text, caption, buttons, icon, defButton);
    }

    /// <summary>
    /// Show dialog centered in the owner
    /// </summary>
    /// <param name="owner">Parent window</param>
    /// <param name="text">Text inside the window</param>
    /// <param name="caption">Window title</param>
    /// <param name="buttons">Series of buttons (MessageBoxButtons.OK for instance)</param>
    /// <param name="icon">Icon (MessageBoxIcon.Warning for instance)</param>
    /// <param name="defButton">Button selected by default</param>
    /// <param name="options">Options of the dialog (MessageBoxOptions.RightAlign for instance)</param>
    /// <returns></returns>
    public static DialogResult Show(IWin32Window owner, string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon, MessageBoxDefaultButton defButton, MessageBoxOptions options)
    {
      _owner = owner;
      Initialize();
      return MessageBox.Show(owner, text, caption, buttons, icon, defButton, options);
    }
    
 #region Private stuff
    [DllImport("user32.dll")]
    private static extern bool GetWindowRect(IntPtr hWnd, ref Rectangle lpRect);

    [DllImport("user32.dll")]
    private static extern int MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);

    [DllImport("user32.dll")]
    private static extern IntPtr SetWindowsHookEx(int idHook, HookProc lpfn, IntPtr hInstance, int threadId);

    [DllImport("user32.dll")]
    private static extern int UnhookWindowsHookEx(IntPtr idHook);

    [DllImport("user32.dll")]
    private static extern IntPtr CallNextHookEx(IntPtr idHook, int nCode, IntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll", SetLastError = true)]
    static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

    [DllImport("user32.dll")]
    static extern IntPtr GetActiveWindow();
    
    private static IWin32Window _owner;
    private static HookProc _hookProc;
    private static IntPtr _hHook;
    private delegate IntPtr HookProc(int nCode, IntPtr wParam, IntPtr lParam);
    private const int WH_CALLWNDPROCRET = 12;

    private enum CbtHookAction : int
    {
      HCBT_MOVESIZE = 0,
      HCBT_MINMAX = 1,
      HCBT_QS = 2,
      HCBT_CREATEWND = 3,
      HCBT_DESTROYWND = 4,
      HCBT_ACTIVATE = 5,
      HCBT_CLICKSKIPPED = 6,
      HCBT_KEYSKIPPED = 7,
      HCBT_SYSCOMMAND = 8,
      HCBT_SETFOCUS = 9
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct CWPRETSTRUCT
    {
      public IntPtr lResult;
      public IntPtr lParam;
      public IntPtr wParam;
      public uint message;
      public IntPtr hwnd;
    } ;

    static MessageBoxCentered()
    {
      _hookProc = new HookProc(MessageBoxHookProc);
      _hHook = IntPtr.Zero;
    }

    private static void Initialize()
    {
      if (_hHook != IntPtr.Zero)
      {
        throw new NotSupportedException("multiple calls are not supported");
      }

      uint processID = 0;
      _hHook = SetWindowsHookEx(WH_CALLWNDPROCRET, _hookProc, IntPtr.Zero,
                                (int)GetWindowThreadProcessId(GetParentWindow(), out processID));
    }

    private static IntPtr MessageBoxHookProc(int nCode, IntPtr wParam, IntPtr lParam)
    {
      if (nCode < 0)
      {
        return CallNextHookEx(_hHook, nCode, wParam, lParam);
      }

      CWPRETSTRUCT msg = (CWPRETSTRUCT)Marshal.PtrToStructure(lParam, typeof(CWPRETSTRUCT));
      IntPtr hook = _hHook;

      if (msg.message == (int)CbtHookAction.HCBT_ACTIVATE)
      {
        try
        {
          CenterWindow(msg.hwnd);
        }
        finally
        {
          UnhookWindowsHookEx(_hHook);
          _hHook = IntPtr.Zero;
        }
      }

      return CallNextHookEx(hook, nCode, wParam, lParam);
    }

    private static void CenterWindow(IntPtr hChildWnd)
    {
      Rectangle recChild = new Rectangle(0, 0, 0, 0);
      bool success = GetWindowRect(hChildWnd, ref recChild);

      int width = recChild.Width - recChild.X;
      int height = recChild.Height - recChild.Y;

      Rectangle recParent = new Rectangle(0, 0, 0, 0);
      success = GetWindowRect(GetParentWindow(), ref recParent);

      Point ptCenter = new Point(0, 0);
      ptCenter.X = recParent.X + ((recParent.Width - recParent.X) / 2);
      ptCenter.Y = recParent.Y + ((recParent.Height - recParent.Y) / 2);


      Point ptStart = new Point(0, 0);
      ptStart.X = (ptCenter.X - (width / 2));
      ptStart.Y = (ptCenter.Y - (height / 2));

      ptStart.X = (ptStart.X < 0) ? 0 : ptStart.X;
      ptStart.Y = (ptStart.Y < 0) ? 0 : ptStart.Y;

      int result = MoveWindow(hChildWnd, ptStart.X, ptStart.Y, width, height, false);
    }
    
    private static IntPtr GetParentWindow()
    {
      if (_owner != null) {
        return _owner.Handle;
      }
      else {
        return GetActiveWindow();
      }
    }
 #endregion // Private stuff
  }
}