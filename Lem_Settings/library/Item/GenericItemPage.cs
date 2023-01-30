// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Windows.Forms;

namespace Lemoine.Settings
{
  /// <summary>
  /// Description of GenericItemPage.
  /// </summary>
  public class GenericItemPage: UserControl
  {
    #region Events
    /// <summary>
    /// Change the title of the page
    /// In most cases you can take the initial title of the page
    /// and append precisions between brackets
    /// A null value will restore the original title
    /// </summary>
    public event Action<string> SetTitle;
    
    /// <summary>
    /// Change the title of the page
    /// In most cases you can take the initial title of the page
    /// and append precisions between brackets
    /// A null value will restore the original title
    /// </summary>
    /// <param name="text">text to append to the title</param>
    protected void EmitSetTitle(string text)
    {
      SetTitle(text);
    }
    
    /// <summary>
    /// Specify a header with a text and a color:
    /// * LemSettingsGlobal.COLOR_OK
    /// * LemSettingsGlobal.COLOR_WARNING
    /// * LemSettingsGlobal.COLOR_ERROR
    /// An empty text remove the header.
    /// </summary>
    public event Action<System.Drawing.Color, string> SpecifyHeader;
    
    /// <summary>
    /// Specify a header with a text and a color
    /// </summary>
    /// <param name="color">LemSettingsGlobal.COLOR_OK, .COLOR_WARNING or .COLOR_ERROR</param>
    /// <param name="text">If empty, remove the header</param>
    protected void EmitSpecifyHeader(System.Drawing.Color color, string text)
    {
      SpecifyHeader(color, text);
    }
    #endregion // Events

    #region Getters / Setters
    /// <summary>
    /// Characterization of the page (see the documentation of the PageFlags)
    /// </summary>
    public virtual LemSettingsGlobal.PageFlag Flags {
      get { return LemSettingsGlobal.PageFlag.NONE; }
    }
    #endregion // Getters / Setters
    
    #region Default methods
    /// <summary>
    /// Method called every second
    /// </summary>
    public virtual void OnTimeOut()
    {
      // Nothing
    }
    #endregion // Default methods

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public GenericItemPage() : base() {}
    #endregion // Constructors
  }
}
