// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

#if NETSTANDARD || NET48 || NETCOREAPP

using System;
using System.Xml;

using Lemoine.Core.Log;
using Lemoine.Info;

namespace Lemoine.DataRepository
{
  /// <summary>
  /// Description of Class1.
  /// </summary>
  public class ClassicConnectionParameters : IConnectionParameters
  {
    #region Members
    readonly ConnectionParameters m_connectionParameters = new ConnectionParameters ();
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger (typeof (ClassicConnectionParameters).FullName);

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public ClassicConnectionParameters ()
    {
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Connection string
    /// </summary>
    public string ConnectionString ()
    {
      return m_connectionParameters.ConnectionString;
    }

    /// <summary>
    /// ODBC connection string
    /// </summary>
    public string OdbcConnectionString ()
    {
      return m_connectionParameters.OdbcConnectionString;
    }

    /// <summary>
    /// build connection parameters from XML file
    /// </summary>
    public void Build (XmlDocument doc)
    {
      // DSNName
      string dsnName =
        doc.DocumentElement.GetAttribute ("dsnname", PulseResolver.PULSE_ODBC_NAMESPACE);
      if (0 == dsnName.Length) {
        throw new SchemaException ("No connection parameter root/@pulse:dsnname");
      }
      m_connectionParameters.DsnName = dsnName;

      // UserName
      m_connectionParameters.Username =
        doc.DocumentElement.GetAttribute ("user", PulseResolver.PULSE_ODBC_NAMESPACE);
      if (0 == m_connectionParameters.Username.Length) {
        log.InfoFormat ("InitConnectParam: " +
                        "no parameter pulse:user " +
                        "in the root element of the schema");
      }

      // Password
      m_connectionParameters.Password =
        doc.DocumentElement.GetAttribute ("password",
                                          PulseResolver.PULSE_ODBC_NAMESPACE);
      if (0 == m_connectionParameters.Password.Length) {
        log.InfoFormat ("InitConnectParam: " +
                        "no parameter pulse:password " +
                        "in the root element of the schema");
      }
    }
    #endregion // Methods
  }
}

#endif // NETSTANDARD || NET48_OR_GREATER || NETCOREAPP
