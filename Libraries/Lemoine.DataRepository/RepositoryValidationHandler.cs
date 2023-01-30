// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Xml.Schema;

using Lemoine.Core.Log;

namespace Lemoine.DataRepository
{
  /// <summary>
  /// Class that contains the custom validation handler for DataRepository
  /// and keeps some internal results, like if an error occured
  /// TODO: keep the information on the event handler (Exception, Message, Severity)
  /// </summary>
  public class RepositoryValidationHandler
  {
    #region Members
    bool m_errors = false;
    #endregion

    static readonly ILog log = LogManager.GetLogger (typeof (RepositoryValidationHandler).FullName);

    #region Getters / Setters
    /// <summary>
    /// Some errors were raised by the parser
    /// </summary>
    public bool Errors
    {
      get { return m_errors; }
    }
    #endregion

    /// <summary>
    /// Constructor
    /// </summary>
    public RepositoryValidationHandler ()
    {
    }

    /// <summary>
    /// Custom Validation Handler for XML parsing
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public void EventHandler (object sender, ValidationEventArgs e)
    {
      switch (e.Severity) {
        case XmlSeverityType.Error:
          log.ErrorFormat ("Error in XML parsing: {0}",
                           e.ToString ());
          m_errors = true;
          break;
        case XmlSeverityType.Warning:
          log.WarnFormat ("Warning in XML parsing: {0}",
                          e.ToString ());
          break;
      }
    }

    /// <summary>
    /// Reset the error flag
    /// </summary>
    public void resetErrors ()
    {
      m_errors = false;
    }
  }
}
