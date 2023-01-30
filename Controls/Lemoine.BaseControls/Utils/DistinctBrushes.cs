// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace Lemoine.BaseControls
{
  /// <summary>
  /// Convenient class allowing the generation of 20 * 8 = 168 distincts brushes
  /// </summary>
  public class DistinctBrushes
  {
    #region Members
    IDictionary<object, Brush> m_brushes = new Dictionary<object, Brush>();
    IList<Color> m_colors = new List<Color>();
    IList<HatchStyle> m_styles = new List<HatchStyle>();
    int m_colorIndex = 0;
    int m_styleIndex = -1;
    #endregion // Members

    #region Constructors
    /// <summary>
    /// Default constructor
    /// </summary>
    public DistinctBrushes()
    {
      // 21 distinct colors
      m_colors.Add(Color.FromArgb(255,   0,   0));  // red
      m_colors.Add(Color.FromArgb(140, 255, 140));  // pastel green
      m_colors.Add(Color.FromArgb(229, 229,   0));  // yellow
      m_colors.Add(Color.FromArgb(191, 191, 255));  // blue heaven
      m_colors.Add(Color.FromArgb(255,  41, 255));  // fuschia
      m_colors.Add(Color.FromArgb(255,  82,  82));  // salmon (orange - red)
      m_colors.Add(Color.FromArgb(  0, 187,   0));  // light green
      m_colors.Add(Color.FromArgb(187, 187,  86));  // sand
      m_colors.Add(Color.FromArgb(133, 133, 255));  // light blue
      m_colors.Add(Color.FromArgb(133,   0, 121));  // purple
      m_colors.Add(Color.FromArgb(  0, 197, 197));  // cyan
      m_colors.Add(Color.FromArgb(  0, 135,   0));  // green
      m_colors.Add(Color.FromArgb( 52, 103,  30));  // brown
      m_colors.Add(Color.FromArgb( 61,  61, 219));  // blue
      m_colors.Add(Color.FromArgb(  0, 152, 141));  // dark cyan
      m_colors.Add(Color.FromArgb(  0,  69,  33));  // dark green
      m_colors.Add(Color.FromArgb(  0,   0, 167));  // dark blue
      m_colors.Add(Color.FromArgb(143,  78, 126));  // kind of pink gray
      m_colors.Add(Color.FromArgb( 77,  77,  77));  // gray
      m_colors.Add(Color.FromArgb(140, 255, 255));  // light cyan
      
      // 7 distinct styles (+ no style)
      m_styles.Add(HatchStyle.Horizontal);        // Horizontal lines
      m_styles.Add(HatchStyle.Vertical);          // Vertical lines
      m_styles.Add(HatchStyle.ForwardDiagonal);   // Forward diagonal lines
      m_styles.Add(HatchStyle.BackwardDiagonal);  // Backward diagonal lines
      m_styles.Add(HatchStyle.Cross);             // Horizontal and vertical lines
      m_styles.Add(HatchStyle.DiagonalCross);     // Forward and backward diagonal lines
      m_styles.Add(HatchStyle.SmallConfetti);     // Dots
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Create or retrieve a brush for a specific object
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public Brush GetBrush(object obj)
    {
      // Creation of a new style if the brush doesn't exist yet
      if (!m_brushes.ContainsKey(obj)){
        if (m_styleIndex == -1) {
          m_brushes[obj] = new SolidBrush(m_colors[m_colorIndex]);
        }
        else {
          m_brushes[obj] = new HatchBrush(m_styles[m_styleIndex], Color.Black, m_colors[m_colorIndex]);
        }

        m_colorIndex++;
        if (m_colorIndex == m_colors.Count) {
          m_styleIndex++;
          m_colorIndex = 0;
          if (m_styleIndex == m_styles.Count) {
            m_styleIndex = -1;
          }
        }
      }
      
      return m_brushes[obj];
    }
    
    /// <summary>
    /// Clear all existing brushes
    /// </summary>
    public void ResetBrushes()
    {
      m_colorIndex = 0;
      m_styleIndex = -1;
      
      ICollection<object> keys = m_brushes.Keys;
      foreach (object obj in keys) {
        m_brushes[obj].Dispose();
      }

      m_brushes.Clear();
    }
    
    /// <summary>
    /// Retrieve all brushes, to display a legend for instance
    /// </summary>
    /// <returns></returns>
    public IDictionary<object, Brush> GetBrushes()
    {
      return m_brushes;
    }
    #endregion // Methods
  }
}
