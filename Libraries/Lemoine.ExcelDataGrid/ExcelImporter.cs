// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Xml;
using System.Windows.Forms;
using System.Collections.Generic;

using Lemoine.Core.Log;
using Lemoine.DataRepository;
using Lemoine.Model;
using Lemoine.ExcelDataGrid;


namespace Lemoine.ExcelDataGrid
{
  /// <summary>
  /// Description of ExcelImporter.
  /// </summary>
  public class ExcelImporter
  {
    #region Members
    string m_configFile;
    string m_excelFileName;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger (typeof (ExcelImporter).FullName);

    #region Getters / Setters
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// ExcelImporter initialized on the basis of an XML configuration file
    /// describing the import procedure
    /// and an excel file name containing the data to be imported
    /// </summary>
    /// <param name="configFile"> XML configuration file name describing import procedure </param>
    /// <param name="fileName"> Excel file name to import </param>
    public ExcelImporter (string configFile, string fileName)
    {
      m_configFile = configFile;
      m_excelFileName = fileName;
    }

    #endregion // Constructors

    #region Methods
    /// <summary>
    /// create a temporary Excel file
    /// from the input file <paramref name="excelFileName"/>
    /// sheet number <paramref name="sheetNumber"/>
    /// and add a "PATH" region
    /// (related to the input path <paramref name="path"/>)
    /// return name of this file or null on failure
    /// </summary>
    /// <param name="excelFileName"></param>
    /// <param name="path"></param>
    /// <param name="sheetNumber"></param>
    /// <returns></returns>
    /// 
    public static string AddPathRegionInFile (string excelFileName,
                                             IPath path, int sheetNumber)
    {
      DataGridView gridView = new DataGridView ();
      IList<GridSelectionEvent> regions =
        ExcelFileDataGrid.ImportFileAsGrid (excelFileName, gridView, sheetNumber);
      if (regions != null) {
        gridView.Rows.Add ();
        gridView.Rows.Add ();
        int nbRows = gridView.Rows.Count;
        gridView.Rows[nbRows - 2].Cells[0].Value = "pathid";
        gridView.Rows[nbRows - 2].Cells[1].Value = "operationid";
        gridView.Rows[nbRows - 1].Cells[0].Value = ((Lemoine.Collections.IDataWithId)path).Id;
        gridView.Rows[nbRows - 1].Cells[1].Value = ((Lemoine.Collections.IDataWithId)path.Operation).Id;
        regions.Add (new GridSelectionEvent ("PATH", nbRows - 2, nbRows - 1, 0, 1));
        string outputExcelFile = System.IO.Path.GetTempFileName ();
        ExcelFileDataGrid.ExportGridAsFile (outputExcelFile, "Sheet1", gridView, regions);
        return outputExcelFile;
      }
      else {
        return null;
      }
    }

    /// <summary>
    /// Import excel file in DB
    /// </summary>
    public void Build ()
    {
      ExcelConnectionParameters connectionParams = new ExcelConnectionParameters (m_excelFileName);
      ODBCFactory factory = new ODBCFactory (XmlSourceType.URI, m_configFile, connectionParams);

#if DEBUG
      
      
      XmlDocument doc = factory.GetData(System.Threading.CancellationToken.None);
      doc.Save(System.IO.Path.Combine(System.IO.Path.GetTempPath(), "output.xml"));
      Repository xmlrep = new Repository();
      xmlrep.MainFactory = factory;
      xmlrep.CopyBuilder = new XMLBuilder(System.IO.Path.Combine(System.IO.Path.GetTempPath(), "xmloutput.xml"));
      xmlrep.UpdateAndSynchronize(System.Threading.CancellationToken.None);

      
#endif

      Repository repository = new Repository ();
      repository.MainFactory = factory;
      repository.CopyBuilder = new LemoineGDBBuilder ();

      repository.UpdateAndSynchronize (System.Threading.CancellationToken.None);

#if DEBUG
      // Console.WriteLine("Excel file has been imported in Database");
#endif
    }

    #endregion // Methods
  }
}
