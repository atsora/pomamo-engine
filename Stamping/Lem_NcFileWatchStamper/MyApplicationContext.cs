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

    NotifyIcon m_trayIcon;
    readonly NcFileWatchStamper m_stamper;

    public MyApplicationContext (NcFileWatchStamper stamper)
    {
      m_stamper = stamper;

      m_trayIcon = new NotifyIcon () {
        Icon = new System.Drawing.Icon ("NcFileWatchStamper.ico"),
        ContextMenuStrip = new ContextMenuStrip () {
          Items = { new ToolStripMenuItem ("Exit", null, Exit) }
        },
        Visible = true
      };
    }

    void Exit (object? sender, EventArgs e)
    {
      m_trayIcon.Visible = false;
      m_stamper.OnStop ();
      Application.Exit ();
    }
  }
}
