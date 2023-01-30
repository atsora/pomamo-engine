// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Drawing;
using System.Windows.Forms;

namespace ConfiguratorAlarms
{
  /// <summary>
  /// CheckedListBox with two different colors
  /// OnDraw signal doesn't work for CheckedListBox, that's why we need to derive this class
  /// </summary>
  public partial class ColorCheckedListBox : CheckedListBox
  {
    #region Getters / Setters
    /// <summary>
    /// Init alarm manager
    /// </summary>
    public AlarmManager AlarmManager { get; set; }
    
    /// <summary>
    /// Current datatype
    /// </summary>
    public string Datatype { get; set; }
    #endregion // Getters / Setters
    
    #region Constructors
    /// <summary>
    /// Default constructor
    /// </summary>
    public ColorCheckedListBox()
    {
      Datatype = "";
      AlarmManager = null;
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Change the color of the alarm
    /// Black by default, blue if the email is linked to an alarm
    /// </summary>
    /// <param name="e"></param>
    protected override void OnDrawItem(DrawItemEventArgs e)
    {
      if (this.DesignMode)
      {
        base.OnDrawItem(e);
      }
      else
      {
        Color fontColor = Color.Black;
        if (AlarmManager != null) {
          var email = (EmailWithName)Items[e.Index];
          if (AlarmManager.IsEmailLinked(email, Datatype)) {
            fontColor = Color.Blue;
          }
        }
        
        var e2 = new DrawItemEventArgs
          (e.Graphics,
           e.Font,
           new Rectangle(e.Bounds.Location, e.Bounds.Size),
           e.Index,
           /* Remove 'selected' state so that the base.OnDrawItem doesn't obliterate the work we are doing here. */
           (e.State & DrawItemState.Focus) == DrawItemState.Focus ? DrawItemState.Focus : DrawItemState.None,
           fontColor,
           this.BackColor);

        base.OnDrawItem(e2);
      }
    }
    #endregion // Methods
  }
}
