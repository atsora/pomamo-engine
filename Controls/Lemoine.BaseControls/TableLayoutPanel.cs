// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Windows.Forms;

using Lemoine.Core.Log;

namespace Lemoine.BaseControls
{
  /// <summary>
  /// New TableLayoutPanel control that contains a workaround
  /// for a Microsoft's bug:
  /// when the auto vertical scroll is displayed
  /// the horizontal scroll is displayed in the same time
  /// </summary>
  // See https://connect.microsoft.com/VisualStudio/feedback/details/276291/tablelayoutpanel-with-autoscroll-incorrectly-shows-horizontal-scrollbar-when-only-vertical-should-show-not-compensating-for-the-vertical-scrollbars-size#
  // and http://forums.microsoft.com/MSDN/ShowPost.aspx?PostID=1595892&SiteID=1&mode=1
  // for more details
  public class TableLayoutPanel: System.Windows.Forms.TableLayoutPanel
  {
    #region Members
    bool busy;
    bool vScroll;
    bool hScroll;
    #endregion

    static readonly ILog log = LogManager.GetLogger(typeof (TableLayoutPanel).FullName);

    #region Methods
    /// <summary>
    /// Method that contains the workaround to Microsoft's bug
    /// </summary>
    /// <param name="levent"></param>
    protected override void OnLayout (LayoutEventArgs levent)
    {
      base.OnLayout (levent);
      bool isVScroll = this.GetScrollState (ScrollableControl.ScrollStateVScrollVisible);
      if ( (vScroll != isVScroll) && !busy) {
        vScroll = isVScroll;
        busy = true;
        this.BeginInvoke (new MethodInvoker (AdjustWidth));
      }
      bool isHScroll = this.GetScrollState (ScrollableControl.ScrollStateHScrollVisible);
      if ( (hScroll != isHScroll) && !busy) {
        hScroll = isHScroll;
        busy = true;
        this.BeginInvoke (new MethodInvoker (AdjustHeight));
      }
    }
    
    void AdjustWidth ()
    {
      if (vScroll) {
        this.Width += SystemInformation.VerticalScrollBarWidth;
      }
      else {
        this.Width -= SystemInformation.VerticalScrollBarWidth;
      }
      busy = false;
    }
    
    void AdjustHeight ()
    {
      if (hScroll) {
        this.Height += SystemInformation.HorizontalScrollBarHeight;
      }
      else {
        this.Height -= SystemInformation.HorizontalScrollBarHeight;
      }
      busy = false;
    }
    #endregion
  }
}
