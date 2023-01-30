// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Threading;
using Lemoine.Core.Log;

namespace Lemoine.BaseControls
{
  /// <summary>
  /// Class comprising static functions which add animation to a control
  /// </summary>
  public static class ControlAnimation
  {
    const int FRAME_RATE = 50;
    
    /// <summary>
    /// Inner class for computing a color range
    /// https://codereview.stackexchange.com/questions/62840/design-of-colour-fading-for-winforms-controls-effect
    /// </summary>
    class ColorFader
    {
      readonly Color _from;
      readonly Color _to;
      readonly double _stepR;
      readonly double _stepG;
      readonly double _stepB;
      readonly uint _steps;

      /// <summary>
      /// Default constructor
      /// </summary>
      /// <param name="from">initial color</param>
      /// <param name="to">last color</param>
      /// <param name="steps">step number, must be a positive number, not null</param>
      public ColorFader(Color from, Color to, uint steps)
      {
        if (steps == 0) {
          throw new ArgumentException("steps must be a positive number");
        }

        _from = from;
        _to = to;
        _steps = steps;

        _stepR = (double)(_to.R - _from.R) / _steps;
        _stepG = (double)(_to.G - _from.G) / _steps;
        _stepB = (double)(_to.B - _from.B) / _steps;
      }

      /// <summary>
      /// Compute a range of color
      /// </summary>
      /// <returns></returns>
      public IEnumerable<Color> Fade()
      {
        for (uint i = 0; i < _steps; ++i) {
          yield return Color.FromArgb(
            (int)(_from.R + i * _stepR),
            (int)(_from.G + i * _stepG),
            (int)(_from.B + i * _stepB));
        }
        yield return _to; // make sure we always return the exact target color last
      }
    }

    static readonly ILog log = LogManager.GetLogger(typeof (ControlAnimation).FullName);

    #region Methods
    /// <summary>
    /// Animate the forecolor of a control
    /// </summary>
    /// <param name="control"></param>
    /// <param name="fromColor"></param>
    /// <param name="toColor"></param>
    /// <param name="duration">duration in seconds</param>
    static public void AnimateForeColor(Control control, Color fromColor, Color toColor, double duration)
    {
      if (control == null) {
        return;
      }

      var thread = new Thread(
        () => AnimateForeColorAsync(control, fromColor, toColor, duration)
       );
      thread.Start();
    }
    
    static void AnimateForeColorAsync(Control control, Color fromColor, Color toColor, double duration)
    {
      var colorFader = new ColorFader(fromColor, toColor, (uint)(duration * FRAME_RATE));
      foreach (var color in colorFader.Fade()) {
        try {
          SetControlForeColor(control, color);
        } catch (Exception) {
          // The control can be deleted
          break;
        }
        Thread.Sleep(1000 / FRAME_RATE);
      }
    }
    
    static void SetControlForeColor(Control control, Color color)
    {
      if (control.InvokeRequired) {
        control.Invoke((MethodInvoker)delegate{ control.ForeColor = color; });
      }
      else {
        control.ForeColor = color;
      }
    }
    
    /// <summary>
    /// Animate the backcolor of a control
    /// </summary>
    /// <param name="control"></param>
    /// <param name="fromColor"></param>
    /// <param name="toColor"></param>
    /// <param name="duration">duration in seconds</param>
    static public void AnimateBackColor(Control control, Color fromColor, Color toColor, double duration)
    {
      if (control == null) {
        return;
      }

      var thread = new Thread(
        () => AnimateBackColorAsync(control, fromColor, toColor, duration)
       );
      thread.Start();
    }
    
    static void AnimateBackColorAsync(Control control, Color fromColor, Color toColor, double duration)
    {
      var colorFader = new ColorFader(fromColor, toColor, (uint)(duration * FRAME_RATE));
      foreach (var color in colorFader.Fade()) {
        try {
          SetControlBackColor(control, color);
        } catch (Exception) {
          // The control can be deleted
          break;
        }
        Thread.Sleep(1000 / FRAME_RATE);
      }
    }
    
    static void SetControlBackColor(Control control, Color color)
    {
      if (control.InvokeRequired) {
        control.Invoke((MethodInvoker)delegate{ control.BackColor = color; });
      }
      else {
        control.BackColor = color;
      }
    }
    #endregion // Methods
  }
}
