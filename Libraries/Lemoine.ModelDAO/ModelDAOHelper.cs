// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Model;
using Lemoine.Core.Log;

namespace Lemoine.ModelDAO
{
  /// <summary>
  /// Description of ModelDAOHelper.
  /// </summary>
  public sealed class ModelDAOHelper
  {
    #region Members
    IModelFactory m_modelFactory;
    IDAOFactory m_daoFactory;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (ModelDAOHelper).FullName);

    #region Getters / Setters
    /// <summary>
    /// Current ModelFactory
    /// </summary>
    public static IModelFactory ModelFactory {
      get { return Instance.m_modelFactory; }
      set
      {
        Instance.m_modelFactory = value;
        Instance.m_daoFactory = value.DAOFactory;
      }
    }
    
    /// <summary>
    /// Current DAOFactory
    /// </summary>
    public static IDAOFactory DAOFactory {
      get
      {
        if (log.IsDebugEnabled && Instance.m_daoFactory is null) {
          log.Debug ($"DAOFactory: null, not initialized. StackTrace={System.Environment.StackTrace}");
        }
        return Instance.m_daoFactory;
      }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Private constructor (singleton class !)
    /// </summary>
    private ModelDAOHelper()
    {
    }
    #endregion // Constructors

    /// <summary>
    /// Return if the DAO Factory is initialized
    /// </summary>
    /// <returns></returns>
    public static bool IsInitialized ()
    {
      return null != Instance.m_daoFactory;
    }

    #region Instance
    static ModelDAOHelper Instance
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

      internal static readonly ModelDAOHelper instance = new ModelDAOHelper ();
    }    
    #endregion // Instance
  }
}
