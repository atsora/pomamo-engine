// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using System.Collections.Generic;
using System.Diagnostics;
using Lemoine.Core.Log;

namespace Lemoine.Web
{
  /// <summary>
  /// Description of ColorGenerator.
  /// </summary>
  public sealed class ColorGenerator
  {
    #region Members
    readonly string m_colorId0 = "#D3D3D3"; // Specific color for Id 0/null, lightgray
    readonly string[] m_colors = { // bright
      "#00FFFF", "#FF0000", "#0000FF",
      "#FF00FF", "#FFFF00", "#00FF00",
      "#0000E0",
      "#00E000", "#00E0E0", "#00E0FF",
      "#00FFE0",
      "#E00000", "#E000E0", "#E000FF",
      "#E0E000",            "#E0E0FF",
      "#E0FF00", "#E0FFE0",
      "#FF00E0",
      "#FFE000", "#FFE0E0" };
    readonly string[] m_opacity7Colors = { // Same colors as above but with an opacity of 0.7
      "#4CFFFF", "#FF4C4C", "#4C4CFF",
      "#FF4CFF", "#FFFF4C", "#4CFF4C",
      "#4C4CE9",
      "#4CE4C0", "#4CE9E9", "#4CE9FF",
      "#4CFFE9",
      "#E4C4C0", "#E4C0E9", "#E4C0FF",
      "#E9E4C0",            "#E9E9FF",
      "#E9FF4C", "#E9FFE9",
      "#FF4CE9",
      "#FFE4C0", "#FFE9E9" };
    readonly string[] m_opacity5Colors = { // Same colors as above but with an opacity of 0.5
      "#80FFFF", "#FF8080", "#8080FF",
      "#FF80FF", "#FFFF80", "#80FF80",
      "#8080F0",
      "#80F080", "#80F0F0", "#80F0FF",
      "#80FFF0",
      "#F08080", "#F080F0", "#F080FF",
      "#F0F080",            "#F0F0FF",
      "#F0FF80", "#F0FFF0",
      "#FF80F0",
      "#FFF080", "#FFF0F0" };
    readonly string[] m_opacity3Colors = { // Same colors as above but with an opacity of 0.3
      "#B2FFFF", "#FFB2B2", "#B2B2FF",
      "#FFB2FF", "#FFFFB2", "#B2FFB2",
      "#B2B2F6",
      "#B2F6B2", "#B2F6F6", "#B2F6FF",
      "#B2FFF6",
      "#F6B2B2", "#F6B2F6", "#F6B2FF",
      "#F6F6B2",            "#F6F6FF",
      "#F6FFB2", "#F6FFF6",
      "#FFB2F6",
      "#FFF6B2", "#FFF6F6" };
    // Colors from https://htmlcolorcodes.com/fr/tableau-de-couleur/tableau-de-couleur-design-plat/
    readonly string[] m_pomegranateColors = { // orange/red/braun
      "#c0392b", "#e6b0aa", "#a93226", "#f9ebea", "#d98880", "#f2d7d5", "#641e16"
    };
    readonly string[] m_wisteriaColors = { // violet
      "#8e44ad", "#e8daef", "#6c3483", "#f4ecf7", "#bb8fce", "#d2b4de", "#4a235a"
    };
    readonly string[] m_peteRiverColors = { // blue
      "#3498db", "#aed6f1", "#1b4f72", "#d6eaf8", "#2874a6", "#ebf5fb", "#85c1e9"
    };
    readonly string[] m_greenSeaColors = { // green
      "#73c6b6", "#d0ece7", "#117a65", "#e8f6f3", "#a2d9ce", "#0b5345", "#16a085"
    };
    readonly string[] m_nephritisColors = { // green
      "#7dcea0", "#d4efdf", "#1e8449", "#e9f7ef", "#27ae60", "#a9dfbf", "#145a32"
    };
    readonly string[] m_sunflowerColors = { // yellow
      "#f7dc6f", "#fcf3cf", "#b7950b", "#fef9e7", "#f1c40f", "#f9e79f", "#7d6608"
    };
    readonly string[] m_orangeColors = { // orange
      "#f5b041", "#fdebd0", "#b9770e", "#fef5e7", "#f8c471", "#f39c12", "#fad7a0", "#7e5109"
    };
    readonly string[] m_carrotColors = { // carrot
      "#eb984e", "#fae5d3", "#af601a", "#e67e22", "#fdf2e9", "#f5cba7", "#784212", "#f0b27a"
    };
    readonly string[] m_pumpkinColors = { // orange/braun
      "#dc7633", "#f6ddcc", "#a04000", "#fbeee6", "#e59866", "#edbb99", "#d35400", "#6e2c00"
    };
    readonly string[] m_cloudsColors = { // grey
      "#fdfefe", "#f0f3f4", "#ecf0f1", "#d0d3d4", "#b3b6b7", "#979a9a", "#7b7d7d"
    };
    readonly string[] m_silverColors = { // grey
      "#d7dbdd", "#f2f3f4", "#909497", "#f8f9f9", "#626567", "#e5e7e9", "#bdc3c7"
    };
    readonly string[] m_wetAsphaltColors = { // dark blue
      "#5d6d7e", "#d6dbdf", "#283747", "#ebedef", "#aeb6bf", "#34495e", "#85929e", "#1b2631"
    };
    IDictionary<string, IList<object>> m_dictionary = new Dictionary<string, IList<object>> ();
    object m_lock = new object ();
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (ColorGenerator).FullName);

    #region Getters / Setters
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Private constructor (singleton class !)
    /// </summary>
    private ColorGenerator()
    {
      Lemoine.Info.ConfigSet.Load (GetPalletConfigKey ("MachineStateTemplate"), "violet");
      Lemoine.Info.ConfigSet.Load (GetPalletConfigKey ("MachineObservationState"), "blue");

      Lemoine.Info.ConfigSet.Load (GetPalletConfigKey ("WorkOrder"), "orange");
      Lemoine.Info.ConfigSet.Load (GetPalletConfigKey ("Component"), "yellow");
      Lemoine.Info.ConfigSet.Load (GetPalletConfigKey ("Operation"), "darkblue");
      Lemoine.Info.ConfigSet.Load (GetPalletConfigKey ("IsoFile"), "grey");
      Lemoine.Info.ConfigSet.Load (GetPalletConfigKey ("Sequence"), "blue");
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Check if id must be considered as the ID 0 / null
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    static bool IsId0 (object id)
    {
      return (null == id)
        || (id is int && (0 == (int) id))
        || (id is long && (0 == (long) id));
    }

    /// <summary>
    /// Get the colors for the specified category and id
    /// </summary>
    /// <param name="category"></param>
    /// <param name="id"></param>
    /// <returns></returns>
    public static string GetColor (string category, object id)
    {
      return Instance.GetColorFromCategory (category, id);
    }

    /// <summary>
    /// Get the colors for the specified category and id
    /// </summary>
    /// <param name="category"></param>
    /// <param name="id"></param>
    string GetColorFromCategory (string category, object id)
    {
      var colorPallet = GetDefaultPallet (category);
      if (log.IsDebugEnabled) {
        log.Debug ($"GetColorFromCategory: pallet for category {category} is {colorPallet}");
      }
      return GetColorFromPallet (category, id, colorPallet);
    }

    /// <summary>
    /// Get the colors for the specified category and id
    /// </summary>
    /// <param name="category"></param>
    /// <param name="id"></param>
    /// <param name="colorPallet">Color pallet name</param>
    string GetColorFromPallet (string category, object id, string colorPallet)
    {
      if (IsId0 (id)) {
        return m_colorId0;
      }

      var colors = GetColorPallet (colorPallet);
      
      lock (m_lock)
      {
        IList<object> list;
        if (!m_dictionary.TryGetValue (category, out list)) {
          list = new List<object> ();
          m_dictionary [category] = list;
        }
        int index = list.IndexOf (id);
        if (-1 == index) {
          index = list.Count;
          list.Add (id);
        }
        Debug.Assert (0 <= index);
        int colorsSize = colors.Length;
        int correctedIndex = index % colorsSize;
        return colors [correctedIndex];
      }
    }

    static string GetPalletConfigKey (string category)
    {
      return "ColorPallet." + category;
    }

    string GetDefaultPallet (string category)
    {
      return Lemoine.Info.ConfigSet
        .LoadAndGet (GetPalletConfigKey (category), "");
    }

    string[] GetColorPallet (string name)
    {
      switch (name.ToLowerInvariant ()) {
      case "carrot":
        return m_carrotColors;
      case "clouds":
        return m_cloudsColors;
      case "greensea":
        return m_greenSeaColors;
      case "nephritis":
      case "green":
        return m_nephritisColors;
      case "orange":
        return m_orangeColors;
      case "peteriver":
      case "blue":
        return m_peteRiverColors;
      case "pomegranate":
        return m_pomegranateColors;
      case "pumpkin":
        return m_pumpkinColors;
      case "silver":
      case "grey":
        return m_silverColors;
      case "sunflower":
      case "yellow":
        return m_sunflowerColors;
      case "wetasphalt":
      case "darkblue":
        return m_wetAsphaltColors;
      case "wisteria":
      case "violet":
        return m_wisteriaColors;
      case "bright":
        return m_colors;
      case "opacity3":
        return m_opacity3Colors;
      case "opacity7":
        return m_opacity7Colors;
      case "opacity5":
      case "default":
      case "":
        return m_opacity5Colors;
      default:
        log.FatalFormat ("GetColorPallet: invalid name {0}", name);
        return m_colors;
      }
    }

    /// <summary>
    /// Get the contrast color
    /// </summary>
    /// <param name="color"></param>
    /// <returns></returns>
    public static string GetContrastColor (string color)
    {
      Debug.Assert (7 == color.Length);
      Debug.Assert ('#' == color [0]);
      int r = Convert.ToInt32 (color.Substring (1, 2), 16);
      int g = Convert.ToInt32 (color.Substring (3, 2), 16);
      int b = Convert.ToInt32 (color.Substring (5, 2), 16);
      double luminance = r*0.299 + g*0.587 + b*0.114;
      if (luminance < 128) {
        return "#FFFFFF";
      }
      else {
        return "#000000";
      }
    }
    #endregion // Methods
    
    #region Instance
    static ColorGenerator Instance
    {
      get { return Nested.instance; }
    }
    
    class Nested
    {
      // Explicit static constructor to tell C# compiler
      // not to mark type as beforefieldinit
      static Nested()
      {
      }

      internal static readonly ColorGenerator instance = new ColorGenerator ();
    }
    #endregion // Instance
  }
}
