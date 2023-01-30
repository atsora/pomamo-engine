// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Xml;

using Lemoine.DataRepository;
using Lemoine.Core.Log;

namespace Lemoine.ExcelDataGrid
{
  /// <summary>
  /// Build Odbc connection parameters to Excel file.
  /// </summary>
  public class ExcelConnectionParameters : IConnectionParameters
  {
    #region Members
    string m_excelFileName = "";
    #endregion // Members

    static readonly ILog log =
      LogManager.GetLogger(typeof (ExcelConnectionParameters).FullName);

    #region Getters / Setters
    
    /// <summary>
    /// return target file path + name
    /// </summary>
    public string ExcelFileName {
      get { return m_excelFileName; }
      set { m_excelFileName = value; }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public ExcelConnectionParameters (string fileName)
    {
      m_excelFileName = fileName;
    }
    
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Connection string
    /// </summary>
    public string ConnectionString()
    {
      return "";
    }
    
    /// <summary>
    /// ODBC connection string
    /// </summary>
    public string OdbcConnectionString()
    {
      
      return "Driver={Microsoft Excel Driver (*.xls, *.xlsx, *.xlsm, *.xlsb)}"
        +";DBQ=" + m_excelFileName;
    }
    
    /// <summary>
    /// build connection parameters from excel file if available
    /// or XML configuration file
    /// </summary>
    public void Build(XmlDocument doc)
    {
      if (m_excelFileName == "")
      {
        string excelFileName =
          doc.DocumentElement.GetAttribute("filename",
                                           PulseResolver.PULSE_ODBC_NAMESPACE);
        // if (0 == excelFileName.Length) {
        // throw new ODBCFactory.SchemaException("No connection parameter root/@pulse:filename");
        // }
        
        m_excelFileName = excelFileName;
      }
    }
    #endregion // Methods
  }
}
