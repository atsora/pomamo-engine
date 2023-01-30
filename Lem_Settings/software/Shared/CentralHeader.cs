// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Drawing;
using System.Windows.Forms;
using Lemoine.Settings;

namespace Lem_Settings.Gui.Shared_controls
{
  /// <summary>
  /// Description of CentralHeader.
  /// </summary>
  public partial class CentralHeader : UserControl
  {
    #region Getters / Setters
    /// <summary>
    /// True is the header contains an item
    /// </summary>
    public bool HasMessage { get; set; }
    #endregion // Getters / Setters
    
    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public CentralHeader()
    {
      InitializeComponent();
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Set the text and the style
    /// Update the value "HasMessage"
    /// </summary>
    /// <param name="color"></param>
    /// <param name="text"></param>
    public void Set(Color color, String text)
    {
      if (String.IsNullOrEmpty(text)) {
        HideText ();
      }
      else {
        ShowText (color, text);
      }
    }
    
    /// <summary>
    /// Hide the header
    /// </summary>
    public void HideText()
    {
      label.BackColor = SystemColors.Control;
      label.ForeColor = SystemColors.ControlText;
      label.Text = "";
      HasMessage = false;
    }
    
    void ShowText(Color backColor, string text)
    {
      label.BackColor = backColor;
      label.ForeColor = GetContrastForeColor(backColor);
      label.Text = text;
      HasMessage = true;
    }
    
    Color GetContrastForeColor(Color color)
    {
      // Counting the perceptive luminance - human eye favors green color...
      double a = 1 - (0.299 * color.R + 0.587 * color.G + 0.114 * color.B) / 255;
      
      int d = (a < 0.5) ? 0 : 255;
      return  Color.FromArgb(d, d, d);
    }
    #endregion // Methods
  }
}
