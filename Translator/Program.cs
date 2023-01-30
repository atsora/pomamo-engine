// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Runtime.Versioning;
using System.Windows.Forms;

namespace Lem_Translator
{
  /// <summary>
  /// Class with program entry point.
  /// </summary>
  [SupportedOSPlatform ("windows7.0")]
  internal sealed class Program
  {
    /// <summary>
    /// Program entry point.
    /// </summary>
    [STAThread]
    private static int Main(string[] args)
    {
      Application.EnableVisualStyles();
      Application.SetCompatibleTextRenderingDefault(false);
      MainForm mainForm = new MainForm ();
      foreach (string i in args) {
        if (i.Equals ("-r")) {
          mainForm.HideRegionSpecificLocales = true;
        }
        else if (i.Equals ("-s")) {
          mainForm.StandaloneMode = true;
        }
        else {
          Console.WriteLine ("Usage: Lem_Translator.exe [-r] [-s]");
          Console.WriteLine ("Options:");
          Console.WriteLine ("  -r    Hide the region specific locales by default");
          Console.WriteLine ("  -s    Standalone mode (do not take into account a potential installation)");
          return 1;
        }
      }
      Application.Run(mainForm);
      return 0;
    }
  }
  
}
