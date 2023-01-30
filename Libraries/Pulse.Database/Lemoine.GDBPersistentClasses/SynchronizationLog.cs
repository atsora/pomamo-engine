// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Lemoine.Model;
using Lemoine.Core.Log;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Persistent class of table SynchronizationLog
  /// </summary>
  [Serializable]
  public class SynchronizationLog : Log, ISynchronizationLog
  {
    #region Members
    string m_xmlElement;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger (typeof (SynchronizationLog).FullName);

    #region Getters / Setters
    /// <summary>
    /// XML Element containing the node where the log was recorded
    /// </summary>
    [XmlAttribute ("XmlElement")]
    public virtual string XmlElement
    {
      get { return m_xmlElement; }
      set { m_xmlElement = value; }
    }
    #endregion // Getters / Setters

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="level"></param>
    /// <param name="message"></param>
    /// <param name="xmlElement"></param>
    public SynchronizationLog (LogLevel level,
                               string message,
                               string xmlElement)
      : base (level, message)
    {
      m_xmlElement = xmlElement;
    }

    /// <summary>
    /// Protected constructor for NHibernate
    /// </summary>
    internal protected SynchronizationLog ()
    {
    }

    /// <summary>
    /// <see cref="Lemoine.Model.ISerializableModel"></see>
    /// </summary>
    public override void Unproxy ()
    {
      // Do nothing here for the moment
    }
  }
}
