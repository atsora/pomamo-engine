// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Windows.Forms;

namespace Lem_MTConnectSimpleViewer
{
  /// <summary>
  /// Class with program entry point.
  /// </summary>
  internal sealed class Program
  {
    /// <summary>
    /// Program entry point.
    /// </summary>
    [STAThread]
    private static void Main(string[] args)
    {
      Application.EnableVisualStyles();
      Application.SetCompatibleTextRenderingDefault(false);
      Application.Run(new MainForm());
    }
  
  }
}
