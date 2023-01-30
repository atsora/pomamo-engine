// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Xml;
using System.Xml.Serialization;

using Lemoine.Core.Log;

namespace Lemoine.Alert
{
  /// <summary>
  /// Basic E-mail action with hard-coded body and subject
  /// </summary>
  [Serializable]
  public class BasicEMailAction: GenericEMailAction
  {
    #region Members
    string m_subject;
    string m_body;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (BasicEMailAction).FullName);

    #region Getters / Setters
    /// <summary>
    /// Subject of the E-Mail
    /// </summary>
    [XmlAttribute("Subject")]
    public string Subject {
      get { return m_subject; }
      set { m_subject = value; }
    }
    
    /// <summary>
    /// Body of the E-Mail
    /// </summary>
    [XmlElement]
    public string Body {
      get { return m_body; }
      set { m_body = value; }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public BasicEMailAction ()
    {
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Implements <see cref="GenericEMailAction" />
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public override string GetSubject (XmlElement data)
    {
      return m_subject;
    }
    
    /// <summary>
    /// Implements <see cref="GenericEMailAction" />
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public override string GetBody (XmlElement data)
    {
      return m_body;
    }
    #endregion // Methods
  }
}
