// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Reflection;

using CommandLine;
using CommandLine.Text;
using Lemoine.Core.Options;
using Lemoine.Info;

namespace Lemoine.Settings
{
  /// <summary>
  /// Description of Options.
  /// </summary>
  public class Options: IMicrosoftParameters
  {
    #region Options
    /// <summary>
    /// Additional parameters
    /// 
    /// The syntax is: Key1=Value1 Key2=Value2
    /// </summary>
    [Option ('p', "parameters", Required = false, HelpText = "Additional parameters")]
    public IEnumerable<string> Parameters { get; set; } = new List<string> ();

    /// <summary>
    /// <see cref="IMicrosoftParameters"/>
    /// 
    /// Additional microsoft parameters
    /// 
    /// The syntax is:
    /// <item>Key1=Value1 Key2=Value2</item>
    /// <item>/Key1 Value1 /Key2 Value2</item>
    /// <item>/Key1=Value1 /Key2=Value2 /Key3=</item>
    /// 
    /// Note that the syntax --Key1 Value1 or --Key1==Value1 is not supported
    /// </summary>
    [Option ('m', "config", Required = false, HelpText = "Additional Microsoft configuration parameters")]
    public IEnumerable<string> MicrosoftParameters { get; set; } = new List<string> ();

    /// <summary>
    /// The login / password box is displayed to the user
    /// If not, logged automatically as a normal user
    /// </summary>
    [Option ('l', "withLogin",
            HelpText = "Login / password is proposed to the user in the splashscreen")]
    public bool WithLogin { get; set; } = false;

    /// <summary>
    /// If true, the libraries loading wizards, configurators, views (and others) are loaded and locked.
    /// The debugger is enabled within these libs but Lem_Settings cannot move / rename / delete the files.
    /// If false the dlls can be updated without exiting Lem_Settings but the debugger cannot go inside them.
    /// </summary>
    [Option ('f', "loadLibraryFiles",
            HelpText = "This option allows the debugger to investigate through the dll, " +
            "but the dlls will be locked: updating them within the software will not be possible")]
    public bool LoadLibraryFiles { get; set; } = false;
    
    /// <summary>
    /// If true, the current computer is recognized as an LCTR
    /// (may be necessary to show some items on a development computer)
    /// </summary>
    [Option ('c', "simulateLCTR",
            HelpText = "Force the current computer to be recognized as an LCTR")]
    public bool SimulateLCTR { get; set; } = false;
    
    /// <summary>
    /// If true, the current computer is recognized as an LPOST
    /// (may be necessary to show some items on a development computer)
    /// </summary>
    [Option ('t', "simulateLPOST",
            HelpText = "Force the current computer to be recognized as an LPOST")]
    public bool SimulateLPOST { get; set; } = false;
    
    /// <summary>
    /// If the value is specified, LemSettings opens directly an item on startup.
    /// 
    /// If the input is in the format "Id:SubId", the item is defined with no ambiguity.
    /// => 400005:1 opens  the configurator "Department" in the machine categories
    /// If the input is in the format "Id", the item having the smallest SubId will be open.
    /// </summary>
    [Option ('i', "item",
            HelpText = "Item to launch. The argument must be in the form 'Id' or 'Id:SubId', " +
            "for example '1' or '12:1'")]
    public string Item { get; set; } = "";
    
    /// <summary>
    /// If true, the libraries loading wizards, configurators, views (and others) are loaded and locked.
    /// The debugger is enabled within these libs but Lem_Settings cannot move / rename / delete the files.
    /// If false the dlls can be updated without exiting Lem_Settings but the debugger cannot go inside them.
    /// </summary>
    [Option ('v', "viewMode",
            HelpText = "This option can be used to launch a configurator as a view on startup. It must be used with the option '-i' " +
            "and with a configurator allowing the view mode.")]
    public bool ViewMode { get; set; } = false;
    
    /// <summary>
    /// Item id launched on startup (null or empty if none)
    /// Must be used in combination with ItemSubId
    /// </summary>
    public string ItemId {
      get {
        if (!m_itemOptionParsed) {
          ParseItemOption ();
        }

        return m_itemOptionId;
      }
    }
    
    /// <summary>
    /// Item subid launched on startup (null if none)
    /// Must be used in combination with ItemId
    /// </summary>
    public string ItemSubId {
      get {
        if (!m_itemOptionParsed) {
          ParseItemOption ();
        }

        return m_itemOptionSubId;
      }
    }
    #endregion // Options
    
    #region Members
    bool m_itemOptionParsed = false;
    string m_itemOptionId = null;
    string m_itemOptionSubId = null;
    #endregion // Members
    
    #region Methods
    void ParseItemOption()
    {
      if (!String.IsNullOrEmpty(Item)) {
        string[] values = Item.Split('.');
        if (values.Length == 1) {
          try {
            m_itemOptionId = values[0];
            m_itemOptionParsed = true;
          } catch {
            m_itemOptionId = null;
          }
        } else if (values.Length == 2) {
          try {
            m_itemOptionId = values[0];
            m_itemOptionSubId = values[1];
            m_itemOptionParsed = true;
          } catch {
            m_itemOptionId = null;
            m_itemOptionSubId = null;
          }
        }
      }
    }
    #endregion // Methods
  }
}
