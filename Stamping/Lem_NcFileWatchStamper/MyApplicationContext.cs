// Copyright (c) 2023 Atsora Solutions

using Lemoine.Core.Log;
using Lemoine.Stamping.Lem_NcFileWatchStamper;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Resources;
using System.Windows.Forms;

namespace Lem_NcFileWatchStamper
{
  /// <summary>
  /// ApplicationContext
  /// </summary>
  public class MyApplicationContext : ApplicationContext
  {
    readonly ILog log = LogManager.GetLogger (typeof (MyApplicationContext).FullName);

    static readonly string ALLOW_EXIT_KEY = "NcFileWatchStamper.AllowExit";
    static readonly bool ALLOW_EXIT_DEFAULT = false;

    NotifyIcon m_trayIcon;
    readonly NcFileWatchStamper m_stamper;

    public MyApplicationContext (NcFileWatchStamper stamper)
    {
      m_stamper = stamper;

      m_trayIcon = new NotifyIcon () {
        Icon = new System.Drawing.Icon ("NcFileWatchStamper.ico"),
        Text = "Atsora NC File Watch Stamper",
        Visible = true
      };

      if (Lemoine.Info.ConfigSet.LoadAndGet (ALLOW_EXIT_KEY, ALLOW_EXIT_DEFAULT)) {
        m_trayIcon.ContextMenuStrip = new ContextMenuStrip () {
          Items = { new ToolStripMenuItem ("Exit", null, Exit) }
        };
      }
    }

    void Exit (object? sender, EventArgs e)
    {
      m_trayIcon.Visible = false;
      m_stamper.OnStop ();
      Application.Exit ();
    }
  }
}
