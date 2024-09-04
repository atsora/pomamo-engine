// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;

namespace Lemoine.Settings
{
  /// <summary>
  /// Utility class formatting a text for the Lemoine Wiki
  /// Can be used with wizards, configurators and views
  /// </summary>
  public class WikiTextFormatter
  {
    enum ItemType
    {
      WIZARD,
      CONFIGURATOR,
      VIEW,
      UNKNOWN
    }
    
    #region Members
    readonly IItem m_item;
    readonly ItemType m_itemType;
    #endregion // Members

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public WikiTextFormatter(IItem item)
    {
      m_item = item;
      if (m_item is IWizard) {
        m_itemType = ItemType.WIZARD;
      }
      else if (m_item is IConfigurator) {
        m_itemType = ItemType.CONFIGURATOR;
      }
      else if (m_item is IView) {
        m_itemType = ItemType.VIEW;
      }
      else {
        m_itemType = ItemType.UNKNOWN;
      }
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Get a formatted text for the Lemoine wiki
    /// </summary>
    /// <returns></returns>
    public string GetWikiText()
    {
      string text = GetFirstPart();
      string tmp = GetMiddlePart();
      if (!String.IsNullOrEmpty(tmp)) {
        text += "\n\n\n" + tmp;
      }

      tmp = GetFinalPart();
      if (!String.IsNullOrEmpty(tmp)) {
        text += "\n\n\n" + tmp;
      }

      return text;
    }
    
    string GetFirstPart()
    {
      const string text = "(:title Lemoine Settings - {0} \"{1}\":)\n\n" +
        "Back to: [[LemSettings | User Manual]] [[LemSettings{2} | Category \"{3}\"]]\n\n\n" +
        "!! Purpose\n\n" +
        "''Please check the links above and fill the purpose here''\n\n" +
        "{4}";
      
      string typeStr;
      switch (m_itemType) {
        case ItemType.WIZARD:
          typeStr = "Wizard";
          break;
        case ItemType.CONFIGURATOR:
          typeStr = "Configurator";
          break;
        case ItemType.VIEW:
          typeStr = "View";
          break;
        default:
          typeStr = "Unknown";
          break;
      }
      
      // Special flags?
      string strFlag = "";
      var flags = new List<string>();
      if ((m_item.Flags & LemSettingsGlobal.ItemFlag.ONLY_ADMIN_AND_SUPER_ADMIN) != 0) {
        flags.Add("only available for Admin or SuperAdmin");
      }
      else if ((m_item.Flags & LemSettingsGlobal.ItemFlag.ONLY_SUPER_ADMIN) != 0) {
        flags.Add("only available for SuperAdmin");
      }

      if ((m_item.Flags & LemSettingsGlobal.ItemFlag.ONLY_LCTR) != 0) {
        flags.Add("only available on the computer LCTR");
      }

      if ((m_item.Flags & LemSettingsGlobal.ItemFlag.ONLY_LPOST) != 0) {
        flags.Add("only available on a computer LPOST");
      }

      if ((m_item.Flags & LemSettingsGlobal.ItemFlag.VIEW_MODE_ALLOWED) != 0) {
        flags.Add("also available in view mode");
      }

      if (flags.Count > 0) {
        strFlag = "\n\nThis item is";
        if (flags.Count == 1) {
          strFlag += " " + flags[0] + ".";
        }
        else {
          strFlag += "\n* " + String.Join(";\n* ", flags.ToArray()) + ".";
        }
      }
      
      return string.Format(text,
                           typeStr, m_item.Title,
                           GetCategoryWikiPage(m_item.Category), m_item.Category,
                           ProcessString(m_item.Description) + strFlag);
    }
    
    // Format the name of the wiki page corresponding to the category
    static string GetCategoryWikiPage(string itemCategory)
    {
      // Split each word, everything in lower case
      string[] categoryTextWords = itemCategory.ToLower().Split(' ');
      
      // First letter of each word in upper case
      for (int i = 0; i < categoryTextWords.Length; i++) {
        categoryTextWords[i] = char.ToUpper(categoryTextWords[i][0]) + categoryTextWords[i].Substring(1);
      }

      // Concatenate all words and return the result
      return string.Join("", categoryTextWords);
    }
    
    string GetMiddlePart()
    {
      IList<string> pageDescriptions = new List<string>();
      switch (m_itemType) {
        case ItemType.WIZARD:
          foreach (var page in (m_item as IWizard).Pages) {
          pageDescriptions.Add(GetPageText(page));
        }

        break;
        case ItemType.CONFIGURATOR:
          foreach (var page in (m_item as IConfigurator).Pages) {
          pageDescriptions.Add(GetPageText(page));
        }

        break;
        case ItemType.VIEW:
          foreach (var page in (m_item as IView).Pages) {
          pageDescriptions.Add(GetPageText(page));
        }

        break;
      }
      
      if (pageDescriptions.Count > 0) {
        return "!! Pages\n\n" + String.Join("\n\n", pageDescriptions.ToArray());
      }

      return "";
    }
    
    string GetPageText(IItemPage page)
    {
      string descriptionModel = "'''Description'''\n\n" +
        "''Complete the description below''\n\n" +
        "{0}";
      switch (m_itemType) {
        case ItemType.WIZARD:
          descriptionModel += "\n\n'''Warnings'''\n\n" +
            "* ''Fill the warnings here or write \"None\"''.\n" +
            "* ''An operation comprises no machines''.";
          descriptionModel += "\n\n'''Errors'''\n\n" +
            "* ''Fill the errors here or write \"None\"''.\n" +
            "* ''The name of ... cannot be empty''.";
          break;
        case ItemType.CONFIGURATOR:
          descriptionModel += "\n\n'''Errors'''\n\n" +
            "* ''Fill the errors here or write \"None\"''.\n" +
            "* ''The name of ... cannot be empty''.";
          descriptionModel += "\n\n'''Results'''\n\n" +
            "* ''Fill the results here''.\n" +
            "* ''Something is edited''.";
          break;
        case ItemType.VIEW:
          // Nothing else
          break;
      }
      
      return String.Format("!!! {0}\n\n" +
                           "(:table border=0 width=100% cellspacing=0 cellpadding=5px :)\n" +
                           "(:cellnr width=376px align=center valign=top :)\n\n" +
                           "Attach:{1}.png\n\n" +
                           "(:cell:)\n" +
                           "{2}\n\n" +
                           "(:tableend:)",
                           page.Title,
                           page.GetType(),
                           String.Format(descriptionModel, ProcessString(page.Help)));
    }
    
    string GetFinalPart()
    {
      string text = "";
      
      if (m_itemType == ItemType.WIZARD) {
        text = "!! Result\n\n" +
          "* ''Add all results here''.\n" +
          "* ''Something is created''.\n" +
          "* ''Something else may be reordered''.";
      }
      
      return text;
    }
    
    static string ProcessString(string input)
    {
      return input
        .Replace("\n - ", "\n* ")
        .Replace("\n- ", "\n* ");
    }
    #endregion // Methods
  }
}
