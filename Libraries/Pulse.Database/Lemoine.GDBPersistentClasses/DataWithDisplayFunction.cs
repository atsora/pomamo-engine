// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Xml.Serialization;
using Lemoine.Core.Log;
using Lemoine.Database.Persistent;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Base class for some persistent classes
  /// 
  /// This allows a set of persistent classes to have a same base set of methods
  /// </summary>
  [Serializable]
  public abstract class DataWithDisplayFunction : BaseData, IDisplayable
  {
    static readonly ILog log = LogManager.GetLogger (typeof (DataWithDisplayFunction).FullName);

    string m_display; // Display computed by PostgreSQL, field mapped by the classes that translate their display

    /// <summary>
    /// Display name that is retrieved with a display function
    ///
    /// It is the display computed by PostgreSQL, unless a class translates it
    /// in the current UI culture (<see cref="GetTranslatedDisplay(string)"/>)
    /// </summary>
    [XmlIgnore]
    public virtual string Display
    {
      get => GetTranslatedDisplay (m_display);
      set => m_display = value;
    }

    /// <summary>
    /// Display in the current UI culture, from the display that PostgreSQL computed
    /// with the locale of the database
    ///
    /// Default is the display of the database. The classes whose display pattern
    /// contains a translation override it (with <see cref="DisplayTranslation"/>),
    /// and map their display column with access="field.camelcase-m-underscore",
    /// so that NHibernate never gets the translated display.
    /// </summary>
    /// <param name="sqlDisplay">display computed by PostgreSQL</param>
    /// <returns></returns>
    protected virtual string GetTranslatedDisplay (string sqlDisplay) => sqlDisplay;
  }
}
