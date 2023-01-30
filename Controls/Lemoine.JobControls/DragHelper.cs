// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Runtime.InteropServices;
using Lemoine.Core.Log;

namespace Lemoine.JobControls
{
  /// <summary>
  /// Description of DragHelper.
  /// </summary>
  internal class DragHelper
  {

    #region Members
    
    [DllImport("comctl32.dll")]
    public static extern bool InitCommonControls();

    /// <summary>
    /// Begins dragging an image.
    /// </summary>
    /// <param name="himlTrack">Handle to the image list.</param>
    /// <param name="iTrack">Index of the image to drag.</param>
    /// <param name="dxHotspot">x-coordinate of the location of the drag position relative to the upper-left corner of the image.</param>
    /// <param name="dyHotspot">y-coordinate of the location of the drag position relative to the upper-left corner of the image.</param>
    /// <returns>Returns nonzero if successful, or zero otherwise.</returns>
    [DllImport("comctl32.dll", CharSet=CharSet.Auto)]
    public static extern bool ImageList_BeginDrag(IntPtr himlTrack, int iTrack, int dxHotspot, int dyHotspot);

    /// <summary>
    /// Moves the image that is being dragged during a drag-and-drop operation.
    /// This function is typically called in response to a WM_MOUSEMOVE message.
    /// </summary>
    /// <param name="x">X-coordinate at which to display the drag image.
    /// The coordinate is relative to the upper-left corner of the window, not the client area.</param>
    /// <param name="y">Y-coordinate at which to display the drag image.
    /// The coordinate is relative to the upper-left corner of the window, not the client area.</param>
    /// <returns>Returns nonzero if successful, or zero otherwise.</returns>
    [DllImport("comctl32.dll", CharSet=CharSet.Auto)]
    public static extern bool ImageList_DragMove(int x, int y);

    /// <summary>
    /// Ends a drag operation.
    /// </summary>
    [DllImport("comctl32.dll", CharSet=CharSet.Auto)]
    public static extern void ImageList_EndDrag();

    /// <summary>
    /// Displays the drag image at the specified position within the window.
    /// </summary>
    /// <param name="hwndLock">Handle to the window that owns the drag image.</param>
    /// <param name="x">X-coordinate at which to display the drag image.
    /// The coordinate is relative to the upper-left corner of the window, not the client area.</param>
    /// <param name="y">Y-coordinate at which to display the drag image.
    /// The coordinate is relative to the upper-left corner of the window, not the client area.</param>
    /// <returns>Returns nonzero if successful, or zero otherwise.</returns>
    [DllImport("comctl32.dll", CharSet=CharSet.Auto)]
    public static extern bool ImageList_DragEnter(IntPtr hwndLock, int x, int y);

    /// <summary>
    /// Unlocks the specified window and hides the drag image, allowing the window to be updated.
    /// </summary>
    /// <param name="hwndLock">Handle to the window that owns the drag image.</param>
    /// <returns>Returns nonzero if successful, or zero otherwise.</returns>
    [DllImport("comctl32.dll", CharSet=CharSet.Auto)]
    public static extern bool ImageList_DragLeave(IntPtr hwndLock);

    /// <summary>
    /// Shows or hides the image being dragged.
    /// </summary>
    /// <param name="fShow">Value specifying whether to show or hide the image being dragged.
    /// Specify <see langword="true"/> to show the image or <see langword="false"/> to hide the image.</param>
    /// <returns>Returns nonzero if successful, or zero otherwise. </returns>
    [DllImport("comctl32.dll", CharSet=CharSet.Auto)]
    public static extern bool ImageList_DragShowNolock(bool fShow);

    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (DragHelper).FullName);

    #region Constructor
    
    static DragHelper()
    {
      InitCommonControls();
    }
    
    #endregion

  }
}
