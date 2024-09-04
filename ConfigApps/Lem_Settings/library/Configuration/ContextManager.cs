// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Lemoine.Settings
{
  /// <summary>
  /// This class describes the context of the application:
  /// the user, the commercial options taken, the computer...
  /// </summary>
  public sealed class ContextManager
  {
    #region Members
    string m_userLogin;
    LemSettingsGlobal.UserCategory m_userCategory;
    Options m_options = new Options();
    bool m_isLctr = false;
    bool m_isLpost = false;
    #endregion // Members

    #region Getters / Setters
    /// <summary>
    /// User login
    /// </summary>
    public static string UserLogin {
      get { return Instance.m_userLogin; }
      set { Instance.m_userLogin = value; }
    }
    
    /// <summary>
    /// UserCategory
    /// </summary>
    public static LemSettingsGlobal.UserCategory UserCategory {
      get { return Instance.m_userCategory; }
      set { Instance.m_userCategory = value; }
    }
    
    /// <summary>
    /// Window title suffix
    /// </summary>
    public static String WindowTitleSuffix {
      get {
        String suffix = "";
        switch (Instance.m_userCategory) {
          case LemSettingsGlobal.UserCategory.ADMINISTRATOR:
            suffix = " (Administrator)";
            break;
          case LemSettingsGlobal.UserCategory.SUPER_ADMIN:
            suffix = " (Super Admin)";
            break;
        }
        return suffix;
      }
    }
    
    /// <summary>
    /// Options coming from the arguments at start up
    /// </summary>
    public static Options Options {
      get { return Instance.m_options; }
      set { Instance.m_options = value; }
    }
    
    /// <summary>
    /// Return true if the current computer is an LCTR
    /// </summary>
    public static bool IsLctr {
      get { return Instance.m_isLctr; }
      set { Instance.m_isLctr = value; }
    }
    
    /// <summary>
    /// Return true if the current computer is an LPOST
    /// </summary>
    public static bool IsLpost {
      get { return Instance.m_isLpost; }
      set { Instance.m_isLpost = value; }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Private constructor (singleton class !)
    /// Comprise the default configuration
    /// </summary>
    ContextManager()
    {
      m_userCategory = LemSettingsGlobal.UserCategory.END_USER;
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Get a context to be passed to an item
    /// </summary>
    /// <returns></returns>
    public static ItemContext GetItemContext()
    {
      var context = new ItemContext();
      context.UserLogin = UserLogin;
      context.UserCategory = UserCategory;
      context.Options = Instance.m_options;
      context.IsLctr = Instance.m_isLctr;
      context.IsLpost = Instance.m_isLpost;
      
      return context;
    }
    #endregion // Methods
    
    #region Instance
    static ContextManager Instance { get { return Nested.instance; } }
    class Nested
    {
      // Explicit static constructor to tell C# compiler
      // not to mark type as beforefieldinit
      static Nested() {}
      internal static readonly ContextManager instance = new ContextManager();
    }
    #endregion // Instance
  }
}
