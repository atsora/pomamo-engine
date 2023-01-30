// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using Lemoine.Core.Log;

namespace Lemoine.I18N
{
  /// <summary>
  /// Description of DummyCatalog.
  /// </summary>
  public class DummyCatalog: ICatalog
  {
    #region Members
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (DummyCatalog).FullName);

    #region Getters / Setters
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public DummyCatalog ()
    {
    }
    #endregion // Constructors

    #region Methods
    #endregion // Methods

    #region ICatalog implementation
    /// <summary>
    /// Implementation of <see cref="Lemoine.I18N.ICatalog">IPulseCatalog</see>
    /// </summary>
    /// <param name="key"></param>
    /// <param name="cultureInfo"></param>
    /// <returns></returns>
    public string GetString(string key, System.Globalization.CultureInfo cultureInfo)
    {
      return null;
    }

    /// <summary>
    /// Implementation of <see cref="Lemoine.I18N.ICatalog">IPulseCatalog</see>
    /// </summary>
    /// <param name="key"></param>
    /// <param name="cultureInfo"></param>
    /// <returns></returns>
    public string GetTranslation(string key, System.Globalization.CultureInfo cultureInfo)
    {
      return null;
    }
    #endregion
  }
}
