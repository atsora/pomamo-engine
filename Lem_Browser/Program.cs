// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Windows.Forms;

using Lemoine.Core.Log;

namespace Lem_Browser
{
  /// <summary>
  /// Class with program entry point.
  /// </summary>
  internal sealed class Program
  {
    static readonly ILog log = LogManager.GetLogger(typeof (Program).FullName);

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
