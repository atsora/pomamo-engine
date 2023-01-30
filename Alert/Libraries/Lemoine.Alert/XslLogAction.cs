// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.XPath;
using System.Xml.Xsl;

using Lemoine.Xml;
using Lemoine.Core.Log;
using Lemoine.Extensions.Alert;

namespace Lemoine.Alert
{
  /// <summary>
  /// Action to log the data with log4net
  /// </summary>
  [Serializable]
  public class XslLogAction: IAction
  {
    #region Members
    [NonSerialized]
    string m_category;
    ILog m_logger;
    XslDefinition m_xslLevel;
    XslDefinition m_xslMessage;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (XslLogAction).FullName);

    #region Getters / Setters
    /// <summary>
    /// Category of the log
    /// </summary>
    [XmlAttribute ("Category")]
    public string Category {
      get { return m_category; }
      set
      {
        m_category = value;
        m_logger = LogManager.GetLogger (m_category);
      }
    }
    
    /// <summary>
    /// Xsl to use build the level to log
    /// 
    /// The result of the XSL transformation must be:
    /// Debug, Info, Warn, Error or Fatal
    /// </summary>
    [XmlElement]
    public XslDefinition XslLevel {
      get { return m_xslLevel; }
      set { m_xslLevel = value; }
    }

    /// <summary>
    /// Xsl to use build the message to log
    /// </summary>
    [XmlElement]
    public XslDefinition XslMessage {
      get { return m_xslMessage; }
      set { m_xslMessage = value; }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public XslLogAction ()
    {
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Implementation of <see cref="IAction.Execute" />
    /// </summary>
    /// <param name="data"></param>
    public void Execute (XmlElement data)
    {
      string message = GetMessage (data);
      var level = GetLevel (data);
      log.DebugFormat ("Execute:" +
                       "log message {0} with level {1}",
                       message, level);
      
      if (level.Equals (Lemoine.Core.Log.Level.Debug)) {
        m_logger.Debug (message);
      }
      else if (level.Equals (Lemoine.Core.Log.Level.Info)) {
        m_logger.Info (message);
      }
      else if (level.Equals (Lemoine.Core.Log.Level.Warn)) {
        m_logger.Warn (message);
      }
      else if (level.Equals (Lemoine.Core.Log.Level.Error)) {
        m_logger.Error (message);
      }
      else if (level.Equals (Lemoine.Core.Log.Level.Fatal)) {
        m_logger.Fatal (message);
      }
    }

    /// <summary>
    /// Get the level to log from the data
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    Lemoine.Core.Log.Level GetLevel (XmlElement data)
    {
      XPathDocument xpd = new XPathDocument(new StringReader(data.OuterXml));
      StringWriter stringWriter = new StringWriter ();
      m_xslMessage.Xslt.Transform (xpd.CreateNavigator (), null, stringWriter);
      string levelString = stringWriter.ToString ();
      if (levelString.Equals ("Debug", StringComparison.InvariantCultureIgnoreCase)) {
        return Lemoine.Core.Log.Level.Debug;
      }
      else if (levelString.Equals ("Info", StringComparison.InvariantCultureIgnoreCase)) {
        return Lemoine.Core.Log.Level.Info;
      }
      else if (levelString.Equals ("Warn", StringComparison.InvariantCultureIgnoreCase)) {
        return Lemoine.Core.Log.Level.Warn;
      }
      else if (levelString.Equals ("Error", StringComparison.InvariantCultureIgnoreCase)) {
        return Lemoine.Core.Log.Level.Error;
      }
      else if (levelString.Equals ("Fatal", StringComparison.InvariantCultureIgnoreCase)) {
        return Lemoine.Core.Log.Level.Fatal;
      }
      else {
        log.ErrorFormat ("GetLevel: " +
                         "unrecognized level {0}",
                         levelString);
        throw new InvalidOperationException ("Unknown level");
      }
    }

    /// <summary>
    /// Get the message to log from the data
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    string GetMessage (XmlElement data)
    {
      XPathDocument xpd = new XPathDocument(new StringReader(data.OuterXml));
      StringWriter stringWriter = new StringWriter ();
      m_xslMessage.Xslt.Transform (xpd.CreateNavigator (), null, stringWriter);
      return stringWriter.ToString ();
    }
    #endregion // Methods
  }
}
