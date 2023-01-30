// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.XPath;
using Lemoine.Xml;
using Lemoine.Core.Log;

namespace Lemoine.Alert
{
  /// <summary>
  /// E-mail action with an XSLT to build the body and the subject
  /// </summary>
  [Serializable]
  public class XslEMailAction: GenericEMailAction
  {
    #region Members
    XslDefinition m_subject;
    XslDefinition m_body;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (XslEMailAction).FullName);

    #region Getters / Setters
    /// <summary>
    /// Subject of the E-Mail
    /// </summary>
    [XmlElement]
    public XslDefinition Subject {
      get { return m_subject; }
      set { m_subject = value; }
    }
    
    /// <summary>
    /// Body of the E-Mail
    /// </summary>
    [XmlElement]
    public XslDefinition Body {
      get { return m_body; }
      set { m_body = value; }
    }

    /// <summary>
    /// UTC Date/time of the e-mail
    /// </summary>
    [XmlElement]
    public XslDefinition UtcDateTime {
      get; set;
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public XslEMailAction ()
    {
      // Default EmailDateTime XSL definition returns an empty string => default to now
      UtcDateTime = new XslTextDefinition ("");
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
      return GetFromXslDefinition (m_subject, data);
    }
    
    /// <summary>
    /// Implements <see cref="GenericEMailAction" />
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public override string GetBody (XmlElement data)
    {
      return GetFromXslDefinition (m_body, data);
    }

    /// <summary>
    /// Override <see cref="Lemoine.Alert.GenericEMailAction" />
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public override DateTime GetDateTime (XmlElement data)
    {
      string utcDateTimeString = GetFromXslDefinition (this.UtcDateTime, data);
      DateTime utcDateTime = DateTime.UtcNow;
      if (string.IsNullOrEmpty (utcDateTimeString)) {
        log.DebugFormat ("GetMatchedEmailConfigs: " +
                         "no UtcDateTime XslDefinition => fallback to now {0}",
                         utcDateTime);
      }
      else if (false == DateTime.TryParse (utcDateTimeString, out utcDateTime)) { // && (null != dateTimeString)
        log.ErrorFormat ("GetMatchedEmailConfigs: " +
                         "error while parsing date/time {0}",
                         utcDateTimeString);
      }
      else {
        utcDateTime = DateTime.SpecifyKind (utcDateTime, DateTimeKind.Utc);
      }

      return utcDateTime;
    }

    string GetFromXslDefinition (XslDefinition xslDefinition, XmlElement data)
    {
      XPathDocument xpd = new XPathDocument (new StringReader (data.OuterXml));
      StringWriter stringWriter = new StringWriter ();
      xslDefinition.Xslt.Transform (xpd.CreateNavigator (), null, stringWriter);
      return stringWriter.ToString ();
    }
    #endregion // Methods
  }
}
