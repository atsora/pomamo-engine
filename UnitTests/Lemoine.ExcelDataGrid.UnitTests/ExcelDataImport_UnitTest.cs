// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections;
using System.IO;
using System.Windows.Forms;

using Lemoine.Core.Log;
using NUnit.Framework;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using Lemoine.ExcelDataGrid;
using Lemoine.ModelDAO;
using Lemoine.Model;
using Lemoine.I18N;
using Lemoine.Database.Persistent;

namespace Lemoine.ExcelDataImport.UnitTests
{
  /// <summary>
  /// Unit tests for Lemoine.ExcelDataImport
  /// </summary>
  [TestFixture]
  public class ExcelDataImport_UnitTest
  {
    static readonly ILog log = LogManager.GetLogger(typeof (ExcelDataImport_UnitTest).FullName);
    #region Members
    string m_previousDSNName;
    #endregion // Member
    
    private static bool gridEquivalent(DataGridView grid1, DataGridView grid2) {
      if ((grid1.Rows.Count != grid2.Rows.Count)
          || (grid1.Columns.Count != grid2.Columns.Count)) {
        return false;
      }

      for (int i = 0 ; i < grid1.Rows.Count ; i++) {
        DataGridViewRow row1 = grid1.Rows[i];
        DataGridViewRow row2 = grid2.Rows[i];
        if (row1.Cells.Count != row2.Cells.Count) {
          return false;
        }

        for (int j = 0 ; j < row1.Cells.Count ; j++) {
          DataGridViewCell cell1 = row1.Cells[j];
          DataGridViewCell cell2 = row2.Cells[j];
          if (cell1 == null) {
            if (cell2 != null) {
              return false;
            }
          } else if (cell2 == null) {
            return false;
          }
          else {
            object val1 = cell1.Value;
            object val2 = cell2.Value;
            if (val1 == null) {
              if ((val2 != null) && (val2.ToString() != "")) {
                return false;
              }
            } else if (val2 == null) {
              if (val1.ToString() != "") {
                return false;
              }
            } else {
              if (!val1.Equals(val2)) {
                Console.WriteLine("cell index ({0},{1}) values {2} != {3}",
                                  i, j, val1.ToString(), val2.ToString());
                return false;
              }
            }
          }
        }
      }
      return true;
    }
    
    
    /// <summary>
    /// Test export to Excel file and import from Excel file (or workbook)
    /// Export sequences to a file, read file into workbook, check, modify it, reimport sequences, check
    /// </summary>
    [Test]
    public void TestExportImport()
    {
      if (null == Lemoine.ModelDAO.ModelDAOHelper.ModelFactory) {
        NHibernateHelper.ApplicationName = "Lemoine.ExcelDataGrid.UnitTests";
        Lemoine.ModelDAO.ModelDAOHelper.ModelFactory =
          new Lemoine.GDBPersistentClasses.GDBPersistentClassFactory ();
      }
      
      ExcelExporter exporter = new ExcelExporter(true);
      exporter.ColumnPropertyIdentifiers = new String[] { "Name", "EstimatedTimeSeconds"};
      exporter.ColumnFieldIdentifiers = new String[] { "BadField1", "SpindleSpeed", "ToolNb", "FeedrateUS", "ToolName", "ToolNb", "ToolDiameter", "ToolNb" };
      
      string firstSequenceName = "Seq1";
      string secondSequenceName = "Seq2";
      double firstSequenceFeedRate = 57.34;
      double secondSequenceFeedRate = 135.2;
      string firstSequenceToolName = "Tool for Seq1";
      double firstSequenceEstimatedTime = 20.5;

      string feedRateDisplay;
      string excelExportFile;
      int firstSeqId, secondSeqId;
      using(IDAOSession daoSession = ModelDAOHelper.DAOFactory.OpenSession()) {
        using(IDAOTransaction transaction = daoSession.BeginTransaction()) {
          
          // create sequences
          IOperationType opType = ModelDAOHelper.DAOFactory.OperationTypeDAO.FindById(1);
          Assert.IsNotNull(opType);
          IOperation op1 = ModelDAOHelper.ModelFactory.CreateOperation(opType);
          op1.Name = "Op1";
          IPath path1 = ModelDAOHelper.ModelFactory.CreatePath();
          path1.Operation = op1;
          path1.Number = 1;
          
          ISequence seq1 = ModelDAOHelper.ModelFactory.CreateSequence(firstSequenceName);
          seq1.Order = 1;
          seq1.EstimatedTime = TimeSpan.FromSeconds (firstSequenceEstimatedTime);
          seq1.Path = path1;
          IField feedRateUSField = ModelDAOHelper.DAOFactory.FieldDAO.FindByCode("FeedrateUS");
          feedRateDisplay = feedRateUSField.Display;
          IStampingValue seq1FeedRateUS = ModelDAOHelper.ModelFactory.CreateStampingValue(seq1, feedRateUSField);
          seq1FeedRateUS.Double = firstSequenceFeedRate;
          IField toolNameField = ModelDAOHelper.DAOFactory.FieldDAO.FindByCode("ToolName");
          IStampingValue seq1ToolNameValue = ModelDAOHelper.ModelFactory.CreateStampingValue(seq1, toolNameField);
          seq1ToolNameValue.String = firstSequenceToolName;
          
          ISequence seq2 = ModelDAOHelper.ModelFactory.CreateSequence(secondSequenceName);
          seq2.Order = 2;
          seq2.Path = path1;
          IStampingValue seq2FeedRateUS = ModelDAOHelper.ModelFactory.CreateStampingValue(seq2, feedRateUSField);
          seq2FeedRateUS.Double = secondSequenceFeedRate;
          
          ModelDAOHelper.DAOFactory.OperationDAO.MakePersistent(op1);
          ModelDAOHelper.DAOFactory.PathDAO.MakePersistent(path1);
          ModelDAOHelper.DAOFactory.SequenceDAO.MakePersistent(seq1);
          ModelDAOHelper.DAOFactory.SequenceDAO.MakePersistent(seq2);
          ModelDAOHelper.DAOFactory.StampingValueDAO.MakePersistent(seq1FeedRateUS);
          ModelDAOHelper.DAOFactory.StampingValueDAO.MakePersistent(seq1ToolNameValue);
          ModelDAOHelper.DAOFactory.StampingValueDAO.MakePersistent(seq2FeedRateUS);
          
          firstSeqId = ((Lemoine.Collections.IDataWithId)seq1).Id;
          secondSeqId = ((Lemoine.Collections.IDataWithId)seq2).Id;
          
          // export sequences to excel file
          string tmpFileName = System.IO.Path.GetTempPath() + Guid.NewGuid().ToString() + ".xls";
          
          Assert.IsTrue(exporter.ExportPathAsFile(path1, tmpFileName));
          
          excelExportFile = tmpFileName;
          
          Assert.IsNotNull(excelExportFile);

          // read generated file in workbook
          HSSFWorkbook workbook;
          try {

            using (FileStream file = new FileStream(excelExportFile, FileMode.Open, FileAccess.Read))
            {
              workbook = new HSSFWorkbook(file);
            }
          } catch (IOException) {
            Assert.True(false, "Unable to open export file");
            return;
          }
          
          // check workbook = check if export is correct
          Assert.IsNotNull(workbook);
          NPOI.SS.UserModel.ISheet sheet = workbook.GetSheetAt(0);
          Assert.IsNotNull(sheet);
          Assert.AreEqual(2, sheet.LastRowNum, "Exported sheet number of rows problem");
          NPOI.SS.UserModel.IRow firstRow = sheet.GetRow(0);
          NPOI.SS.UserModel.IRow secondRow = sheet.GetRow(1);
          NPOI.SS.UserModel.IRow thirdRow = sheet.GetRow(2);
          
          NPOI.SS.UserModel.ICell cellHeaderSequenceName = firstRow.GetCell(1);
          Assert.IsNotNull(cellHeaderSequenceName, "Problem cell 1 of row 0");
          Assert.AreEqual(PulseCatalog.GetString("SequenceName"), cellHeaderSequenceName.StringCellValue, "Bad Sequence Name column header");
          
          NPOI.SS.UserModel.ICell cellHeaderFeedrate = firstRow.GetCell(4);
          Assert.IsNotNull(cellHeaderFeedrate, "Problem cell 4 of row 0");
          Assert.AreEqual(feedRateDisplay, cellHeaderFeedrate.StringCellValue, "Bad Feedrate column header");
          
          NPOI.SS.UserModel.ICell cellFirstSequenceName = secondRow.GetCell(1);
          Assert.IsNotNull(cellFirstSequenceName, "Problem cell 1 of row 1");
          Assert.AreEqual(firstSequenceName, cellFirstSequenceName.StringCellValue, "Bad first sequence name");
          
          NPOI.SS.UserModel.ICell cellSecondSequenceName = thirdRow.GetCell(1);
          Assert.IsNotNull(cellSecondSequenceName, "Problem cell 1 of row 2");
          Assert.AreEqual(secondSequenceName, cellSecondSequenceName.StringCellValue, "Bad second sequence name");
          
          NPOI.SS.UserModel.ICell cellFirstSequenceToolName = secondRow.GetCell(5);
          Assert.IsNotNull(cellFirstSequenceToolName, "Problem cell 5 of row 1");
          Assert.AreEqual(firstSequenceToolName, cellFirstSequenceToolName.StringCellValue, "Bad first sequence tool name");
          
          NPOI.SS.UserModel.ICell cellFirstSequenceEstimatedTime = secondRow.GetCell(2);
          Assert.IsNotNull(cellFirstSequenceEstimatedTime, "Problem cell 2 of row 1");
          Assert.AreEqual(firstSequenceEstimatedTime, cellFirstSequenceEstimatedTime.NumericCellValue, "Bad first sequence estimated time");

          NPOI.SS.UserModel.ICell cellFirstSequenceFeedrate = secondRow.GetCell(4);
          Assert.IsNotNull(cellFirstSequenceFeedrate, "Problem cell 4 of row 1");
          Assert.AreEqual(firstSequenceFeedRate, cellFirstSequenceFeedrate.NumericCellValue, "Bad first sequence feed rate");

          NPOI.SS.UserModel.ICell cellSecondSequenceFeedrate = thirdRow.GetCell(4);
          Assert.IsNotNull(cellSecondSequenceFeedrate, "Problem cell 4 of row 2");
          Assert.AreEqual(secondSequenceFeedRate, cellSecondSequenceFeedrate.NumericCellValue, "Bad second sequence feed rate");
          
          NPOI.SS.UserModel.ICell cellFirstSequenceToolDiameter = secondRow.GetCell(6);
          Assert.IsNotNull(cellFirstSequenceToolDiameter, "Problem cell 6 of row 1");
          Assert.AreEqual(NPOI.SS.UserModel.CellType.Blank, cellFirstSequenceToolDiameter.CellType, "Bad first sequence tool diameter");

          NPOI.SS.UserModel.ICell cellSecondSequenceEstimatedTime = thirdRow.GetCell(2);
          Assert.IsNotNull(cellSecondSequenceEstimatedTime, "Problem cell 2 of row 2");
          Assert.AreEqual(NPOI.SS.UserModel.CellType.Blank, cellSecondSequenceEstimatedTime.CellType, "Bad second sequence estimated time");
          
          // update on sequences in workbook
          double firstSequenceToolDiameter = 1.25;
          cellFirstSequenceToolDiameter.SetCellValue(firstSequenceToolDiameter);
          TimeSpan firstSequenceNewEstimatedTime = TimeSpan.FromSeconds (10.5);
          cellFirstSequenceEstimatedTime.SetCellValue(firstSequenceNewEstimatedTime.TotalSeconds);
          double firstSequenceNewFeedRate = 12.54;
          cellFirstSequenceFeedrate.SetCellValue(firstSequenceNewFeedRate);
          string firstSequenceNewToolName = "New Tool name for Seq1";
          cellFirstSequenceToolName.SetCellValue(firstSequenceNewToolName);
          string secondSequenceNewName = "Seq2NewName";
          cellSecondSequenceName.SetCellValue(secondSequenceNewName);
          TimeSpan secondSequenceEstimatedTime = TimeSpan.FromSeconds (99.99);
          cellSecondSequenceEstimatedTime.SetCellValue(secondSequenceEstimatedTime.TotalSeconds);
          
          // import workbook in DB (no path given)
          exporter.ImportSequenceWorkbook(null, workbook);
          
          // check if sequences are correctly imported from workbook
          Assert.AreEqual(firstSequenceNewEstimatedTime, seq1.EstimatedTime, "Bad new estimated time for seq1");
          Assert.AreEqual(firstSequenceNewFeedRate, seq1FeedRateUS.Double, "Bad new feed rate for seq1");
          Assert.AreEqual(firstSequenceNewToolName, seq1ToolNameValue.String, "Bad new tool name for seq1");
          
          Assert.AreEqual(secondSequenceNewName, seq2.Name, "Bad new name for seq2");
          Assert.AreEqual(secondSequenceEstimatedTime, seq2.EstimatedTime, "Bad estimated time for seq2");
          
          IField toolDiameterField = ModelDAOHelper.DAOFactory.FieldDAO.FindByCode("ToolDiameter");
          
          bool foundNewStampingValue = false;
          foreach(IStampingValue seq1StampingValue in seq1.StampingValues) {
            if (seq1StampingValue.Field.Equals(toolDiameterField)) {
              foundNewStampingValue = true;
              Assert.AreEqual(firstSequenceToolDiameter, seq1StampingValue.Double, "Bad tool diameter for seq1");
              break;
            }
          }
          
          Assert.IsTrue(foundNewStampingValue, "No tool diameter for seq1");
        }
      }
    }

    /// <summary>
    /// Test correct conversion from datetime cell
    /// </summary>
    [Test]
    public void TestGetCellValueWhenDateTime()
    {
      HSSFWorkbook workbook;
      try {

        using (FileStream file = new FileStream(Path.Combine (TestContext.CurrentContext.TestDirectory, "DateTime.xls"), FileMode.Open, FileAccess.Read))
        {
          workbook = new HSSFWorkbook(file);
          ISheet firstSheet = workbook.GetSheetAt(0);
          Assert.IsNotNull(firstSheet);
          IRow firstRow = firstSheet.GetRow(0);
          Assert.IsNotNull(firstRow);
          
          IEnumerator enumCells = firstRow.GetEnumerator();
          Assert.IsTrue(enumCells.MoveNext());
          NPOI.SS.UserModel.ICell cell = (NPOI.SS.UserModel.ICell) enumCells.Current;
          Assert.IsNotNull(cell);
          object v = ExcelExporter.GetCellValue(cell);
          Assert.IsNotNull(v);          
          double? d = v as double?;
          Assert.IsNotNull(d);
          Assert.AreEqual(d, 60 + 40);          
        }
      } catch (IOException) {
        Assert.True(false, "Unable to open file");
        return;
      }
    }
    
    /// <summary>
    /// Test export to Excel file and import from Excel file (or workbook)
    /// Export sequences to a file, read file into workbook, check, modify it, reimport sequences, check
    /// This version use hh:mm:ss formatting for EstimatedTime
    /// </summary>
    [Test]
    public void TestExportImport2()
    {
      if (null == Lemoine.ModelDAO.ModelDAOHelper.ModelFactory) {
        NHibernateHelper.ApplicationName = "Lemoine.ExcelDataGrid.UnitTests";
        Lemoine.ModelDAO.ModelDAOHelper.ModelFactory =
          new Lemoine.GDBPersistentClasses.GDBPersistentClassFactory ();
      }
      
      ExcelExporter exporter = new ExcelExporter(true);
      exporter.ColumnPropertyIdentifiers = new String[] { "Name", "EstimatedTimeSeconds=hh:mm:ss"};
      exporter.ColumnFieldIdentifiers = new String[] { "BadField1", "SpindleSpeed", "ToolNb", "FeedrateUS", "ToolName", "ToolNb", "ToolDiameter", "ToolNb" };
      
      string firstSequenceName = "Seq1";
      string secondSequenceName = "Seq2";
      double firstSequenceFeedRate = 57.34;
      double secondSequenceFeedRate = 135.2;
      string firstSequenceToolName = "Tool for Seq1";
      double firstSequenceEstimatedTime = 2100;

      string feedRateDisplay;
      string excelExportFile;
      int firstSeqId, secondSeqId;
      using(IDAOSession daoSession = ModelDAOHelper.DAOFactory.OpenSession()) {
        using(IDAOTransaction transaction = daoSession.BeginTransaction()) {
          
          // create sequences
          IOperationType opType = ModelDAOHelper.DAOFactory.OperationTypeDAO.FindById(1);
          Assert.IsNotNull(opType);
          IOperation op1 = ModelDAOHelper.ModelFactory.CreateOperation(opType);
          op1.Name = "Op1";
          IPath path1 = ModelDAOHelper.ModelFactory.CreatePath();
          path1.Operation = op1;
          path1.Number = 1;
          
          ISequence seq1 = ModelDAOHelper.ModelFactory.CreateSequence(firstSequenceName);
          seq1.Order = 1;
          seq1.EstimatedTime = TimeSpan.FromSeconds (firstSequenceEstimatedTime);
          seq1.Path = path1;
          IField feedRateUSField = ModelDAOHelper.DAOFactory.FieldDAO.FindByCode("FeedrateUS");
          feedRateDisplay = feedRateUSField.Display;
          IStampingValue seq1FeedRateUS = ModelDAOHelper.ModelFactory.CreateStampingValue(seq1, feedRateUSField);
          seq1FeedRateUS.Double = firstSequenceFeedRate;
          IField toolNameField = ModelDAOHelper.DAOFactory.FieldDAO.FindByCode("ToolName");
          IStampingValue seq1ToolNameValue = ModelDAOHelper.ModelFactory.CreateStampingValue(seq1, toolNameField);
          seq1ToolNameValue.String = firstSequenceToolName;
          
          ISequence seq2 = ModelDAOHelper.ModelFactory.CreateSequence(secondSequenceName);
          seq2.Order = 2;
          seq2.Path = path1;
          IStampingValue seq2FeedRateUS = ModelDAOHelper.ModelFactory.CreateStampingValue(seq2, feedRateUSField);
          seq2FeedRateUS.Double = secondSequenceFeedRate;
          
          ModelDAOHelper.DAOFactory.OperationDAO.MakePersistent(op1);
          ModelDAOHelper.DAOFactory.PathDAO.MakePersistent(path1);
          ModelDAOHelper.DAOFactory.SequenceDAO.MakePersistent(seq1);
          ModelDAOHelper.DAOFactory.SequenceDAO.MakePersistent(seq2);
          ModelDAOHelper.DAOFactory.StampingValueDAO.MakePersistent(seq1FeedRateUS);
          ModelDAOHelper.DAOFactory.StampingValueDAO.MakePersistent(seq1ToolNameValue);
          ModelDAOHelper.DAOFactory.StampingValueDAO.MakePersistent(seq2FeedRateUS);
          
          firstSeqId = ((Lemoine.Collections.IDataWithId)seq1).Id;
          secondSeqId = ((Lemoine.Collections.IDataWithId)seq2).Id;
          
          // export sequences to excel file
          string tmpFileName = System.IO.Path.GetTempPath() + Guid.NewGuid().ToString() + ".xls";
          
          Assert.IsTrue(exporter.ExportPathAsFile(path1, tmpFileName));
          
          excelExportFile = tmpFileName;
          
          Assert.IsNotNull(excelExportFile);

          // read generated file in workbook
          HSSFWorkbook workbook;
          try {

            using (FileStream file = new FileStream(excelExportFile, FileMode.Open, FileAccess.Read))
            {
              workbook = new HSSFWorkbook(file);
            }
          } catch (IOException) {
            Assert.True(false, "Unable to open export file");
            return;
          }
          
          // check workbook = check if export is correct
          Assert.IsNotNull(workbook);
          NPOI.SS.UserModel.ISheet sheet = workbook.GetSheetAt(0);
          Assert.IsNotNull(sheet);
          Assert.AreEqual(2, sheet.LastRowNum, "Exported sheet number of rows problem");
          NPOI.SS.UserModel.IRow firstRow = sheet.GetRow(0);
          NPOI.SS.UserModel.IRow secondRow = sheet.GetRow(1);
          NPOI.SS.UserModel.IRow thirdRow = sheet.GetRow(2);
          
          NPOI.SS.UserModel.ICell cellHeaderSequenceName = firstRow.GetCell(1);
          Assert.IsNotNull(cellHeaderSequenceName, "Problem cell 1 of row 0");
          Assert.AreEqual(PulseCatalog.GetString("SequenceName"), cellHeaderSequenceName.StringCellValue, "Bad Sequence Name column header");
          
          NPOI.SS.UserModel.ICell cellHeaderFeedrate = firstRow.GetCell(4);
          Assert.IsNotNull(cellHeaderFeedrate, "Problem cell 4 of row 0");
          Assert.AreEqual(feedRateDisplay, cellHeaderFeedrate.StringCellValue, "Bad Feedrate column header");
          
          NPOI.SS.UserModel.ICell cellFirstSequenceName = secondRow.GetCell(1);
          Assert.IsNotNull(cellFirstSequenceName, "Problem cell 1 of row 1");
          Assert.AreEqual(firstSequenceName, cellFirstSequenceName.StringCellValue, "Bad first sequence name");
          
          NPOI.SS.UserModel.ICell cellSecondSequenceName = thirdRow.GetCell(1);
          Assert.IsNotNull(cellSecondSequenceName, "Problem cell 1 of row 2");
          Assert.AreEqual(secondSequenceName, cellSecondSequenceName.StringCellValue, "Bad second sequence name");
          
          NPOI.SS.UserModel.ICell cellFirstSequenceToolName = secondRow.GetCell(5);
          Assert.IsNotNull(cellFirstSequenceToolName, "Problem cell 5 of row 1");
          Assert.AreEqual(firstSequenceToolName, cellFirstSequenceToolName.StringCellValue, "Bad first sequence tool name");
          
          NPOI.SS.UserModel.ICell cellFirstSequenceEstimatedTime = secondRow.GetCell(2);
          Assert.IsNotNull(cellFirstSequenceEstimatedTime, "Problem cell 2 of row 1");
          Assert.AreEqual("00:35:00", cellFirstSequenceEstimatedTime.StringCellValue, "Bad first sequence estimated time");

          NPOI.SS.UserModel.ICell cellFirstSequenceFeedrate = secondRow.GetCell(4);
          Assert.IsNotNull(cellFirstSequenceFeedrate, "Problem cell 4 of row 1");
          Assert.AreEqual(firstSequenceFeedRate, cellFirstSequenceFeedrate.NumericCellValue, "Bad first sequence feed rate");

          NPOI.SS.UserModel.ICell cellSecondSequenceFeedrate = thirdRow.GetCell(4);
          Assert.IsNotNull(cellSecondSequenceFeedrate, "Problem cell 4 of row 2");
          Assert.AreEqual(secondSequenceFeedRate, cellSecondSequenceFeedrate.NumericCellValue, "Bad second sequence feed rate");
          
          NPOI.SS.UserModel.ICell cellFirstSequenceToolDiameter = secondRow.GetCell(6);
          Assert.IsNotNull(cellFirstSequenceToolDiameter, "Problem cell 6 of row 1");
          Assert.AreEqual(NPOI.SS.UserModel.CellType.Blank, cellFirstSequenceToolDiameter.CellType, "Bad first sequence tool diameter");

          NPOI.SS.UserModel.ICell cellSecondSequenceEstimatedTime = thirdRow.GetCell(2);
          Assert.IsNotNull(cellSecondSequenceEstimatedTime, "Problem cell 2 of row 2");
          Assert.AreEqual(NPOI.SS.UserModel.CellType.Blank, cellSecondSequenceEstimatedTime.CellType, "Bad second sequence estimated time");
          
          // update on sequences in workbook
          double firstSequenceToolDiameter = 1.25;
          // put a string value in place of a double
          cellFirstSequenceToolDiameter.SetCellValue(firstSequenceToolDiameter.ToString());
          string firstSequenceNewEstimatedTime = "00:00:10";
          cellFirstSequenceEstimatedTime.SetCellValue(firstSequenceNewEstimatedTime);
          double firstSequenceNewFeedRate = 12.54;
          // put a string value in place of a double
          cellFirstSequenceFeedrate.SetCellValue(firstSequenceNewFeedRate.ToString());
          string firstSequenceNewToolName = "New Tool name for Seq1";
          cellFirstSequenceToolName.SetCellValue(firstSequenceNewToolName);
          string secondSequenceNewName = "Seq2NewName";
          cellSecondSequenceName.SetCellValue(secondSequenceNewName);
          string secondSequenceEstimatedTime = "00:01:40";
          cellSecondSequenceEstimatedTime.SetCellValue(secondSequenceEstimatedTime);
          
          // import workbook in DB (no path given)
          exporter.ImportSequenceWorkbook(null, workbook);
          
          // check if sequences are correctly imported from workbook
          Assert.AreEqual(TimeSpan.FromSeconds (10.0), seq1.EstimatedTime, "Bad new estimated time for seq1");
          Assert.AreEqual(firstSequenceNewFeedRate, seq1FeedRateUS.Double, "Bad new feed rate for seq1");
          Assert.AreEqual(firstSequenceNewToolName, seq1ToolNameValue.String, "Bad new tool name for seq1");
          
          Assert.AreEqual(secondSequenceNewName, seq2.Name, "Bad new name for seq2");
          Assert.AreEqual(TimeSpan.FromSeconds (100.0), seq2.EstimatedTime, "Bad estimated time for seq2");
          
          IField toolDiameterField = ModelDAOHelper.DAOFactory.FieldDAO.FindByCode("ToolDiameter");
          
          bool foundNewStampingValue = false;
          foreach(IStampingValue seq1StampingValue in seq1.StampingValues) {
            if (seq1StampingValue.Field.Equals(toolDiameterField)) {
              foundNewStampingValue = true;
              Assert.AreEqual(firstSequenceToolDiameter, seq1StampingValue.Double, "Bad tool diameter for seq1");
              break;
            }
          }
          
          Assert.IsTrue(foundNewStampingValue, "No tool diameter for seq1");
        }
      }
    }

    /// <summary>
    /// Test export to Excel file and import from Excel file (or workbook)
    /// Export sequences to a file, read file into workbook, check, modify it, reimport sequences, check
    /// This version use "MM" formatting for EstimatedTime (decimal representing minutes)
    /// </summary>
    [Test]
    public void TestExportImport3()
    {
      if (null == Lemoine.ModelDAO.ModelDAOHelper.ModelFactory) {
        NHibernateHelper.ApplicationName = "Lemoine.ExcelDataGrid.UnitTests";
        Lemoine.ModelDAO.ModelDAOHelper.ModelFactory =
          new Lemoine.GDBPersistentClasses.GDBPersistentClassFactory ();
      }
      
      ExcelExporter exporter = new ExcelExporter(true);
      exporter.ColumnPropertyIdentifiers = new String[] { "Name", "EstimatedTime=MM"};
      exporter.ColumnFieldIdentifiers = new String[] { "BadField1", "SpindleSpeed", "ToolNb", "FeedrateUS", "ToolName", "ToolNb", "ToolDiameter", "ToolNb" };
      
      string firstSequenceName = "Seq1";
      string secondSequenceName = "Seq2";
      double firstSequenceFeedRate = 57.34;
      double secondSequenceFeedRate = 135.2;
      string firstSequenceToolName = "Tool for Seq1";
      double firstSequenceEstimatedTime = 2100;

      string feedRateDisplay;
      string excelExportFile;
      int firstSeqId, secondSeqId;
      using(IDAOSession daoSession = ModelDAOHelper.DAOFactory.OpenSession()) {
        using(IDAOTransaction transaction = daoSession.BeginTransaction()) {
          
          // create sequences
          IOperationType opType = ModelDAOHelper.DAOFactory.OperationTypeDAO.FindById(1);
          Assert.IsNotNull(opType);
          IOperation op1 = ModelDAOHelper.ModelFactory.CreateOperation(opType);
          op1.Name = "Op1";
          IPath path1 = ModelDAOHelper.ModelFactory.CreatePath();
          path1.Operation = op1;
          path1.Number = 1;
          
          ISequence seq1 = ModelDAOHelper.ModelFactory.CreateSequence(firstSequenceName);
          seq1.Order = 1;
          seq1.EstimatedTime = TimeSpan.FromSeconds (firstSequenceEstimatedTime);
          seq1.Path = path1;
          IField feedRateUSField = ModelDAOHelper.DAOFactory.FieldDAO.FindByCode("FeedrateUS");
          feedRateDisplay = feedRateUSField.Display;
          IStampingValue seq1FeedRateUS = ModelDAOHelper.ModelFactory.CreateStampingValue(seq1, feedRateUSField);
          seq1FeedRateUS.Double = firstSequenceFeedRate;
          IField toolNameField = ModelDAOHelper.DAOFactory.FieldDAO.FindByCode("ToolName");
          IStampingValue seq1ToolNameValue = ModelDAOHelper.ModelFactory.CreateStampingValue(seq1, toolNameField);
          seq1ToolNameValue.String = firstSequenceToolName;
          
          ISequence seq2 = ModelDAOHelper.ModelFactory.CreateSequence(secondSequenceName);
          seq2.Order = 2;
          seq2.Path = path1;
          IStampingValue seq2FeedRateUS = ModelDAOHelper.ModelFactory.CreateStampingValue(seq2, feedRateUSField);
          seq2FeedRateUS.Double = secondSequenceFeedRate;
          
          ModelDAOHelper.DAOFactory.OperationDAO.MakePersistent(op1);
          ModelDAOHelper.DAOFactory.PathDAO.MakePersistent(path1);
          ModelDAOHelper.DAOFactory.SequenceDAO.MakePersistent(seq1);
          ModelDAOHelper.DAOFactory.SequenceDAO.MakePersistent(seq2);
          ModelDAOHelper.DAOFactory.StampingValueDAO.MakePersistent(seq1FeedRateUS);
          ModelDAOHelper.DAOFactory.StampingValueDAO.MakePersistent(seq1ToolNameValue);
          ModelDAOHelper.DAOFactory.StampingValueDAO.MakePersistent(seq2FeedRateUS);
          
          firstSeqId = ((Lemoine.Collections.IDataWithId)seq1).Id;
          secondSeqId = ((Lemoine.Collections.IDataWithId)seq2).Id;
          
          // export sequences to excel file
          string tmpFileName = System.IO.Path.GetTempPath() + Guid.NewGuid().ToString() + ".xls";
          
          Assert.IsTrue(exporter.ExportPathAsFile(path1, tmpFileName));
          
          excelExportFile = tmpFileName;
          
          Assert.IsNotNull(excelExportFile);

          // read generated file in workbook
          HSSFWorkbook workbook;
          try {

            using (FileStream file = new FileStream(excelExportFile, FileMode.Open, FileAccess.Read))
            {
              workbook = new HSSFWorkbook(file);
            }
          } catch (IOException) {
            Assert.True(false, "Unable to open export file");
            return;
          }
          
          // check workbook = check if export is correct
          Assert.IsNotNull(workbook);
          NPOI.SS.UserModel.ISheet sheet = workbook.GetSheetAt(0);
          Assert.IsNotNull(sheet);
          Assert.AreEqual(2, sheet.LastRowNum, "Exported sheet number of rows problem");
          NPOI.SS.UserModel.IRow firstRow = sheet.GetRow(0);
          NPOI.SS.UserModel.IRow secondRow = sheet.GetRow(1);
          NPOI.SS.UserModel.IRow thirdRow = sheet.GetRow(2);
          
          NPOI.SS.UserModel.ICell cellHeaderSequenceName = firstRow.GetCell(1);
          Assert.IsNotNull(cellHeaderSequenceName, "Problem cell 1 of row 0");
          Assert.AreEqual(PulseCatalog.GetString("SequenceName"), cellHeaderSequenceName.StringCellValue, "Bad Sequence Name column header");
          
          NPOI.SS.UserModel.ICell cellHeaderFeedrate = firstRow.GetCell(4);
          Assert.IsNotNull(cellHeaderFeedrate, "Problem cell 4 of row 0");
          Assert.AreEqual(feedRateDisplay, cellHeaderFeedrate.StringCellValue, "Bad Feedrate column header");
          
          NPOI.SS.UserModel.ICell cellFirstSequenceName = secondRow.GetCell(1);
          Assert.IsNotNull(cellFirstSequenceName, "Problem cell 1 of row 1");
          Assert.AreEqual(firstSequenceName, cellFirstSequenceName.StringCellValue, "Bad first sequence name");
          
          NPOI.SS.UserModel.ICell cellSecondSequenceName = thirdRow.GetCell(1);
          Assert.IsNotNull(cellSecondSequenceName, "Problem cell 1 of row 2");
          Assert.AreEqual(secondSequenceName, cellSecondSequenceName.StringCellValue, "Bad second sequence name");
          
          NPOI.SS.UserModel.ICell cellFirstSequenceToolName = secondRow.GetCell(5);
          Assert.IsNotNull(cellFirstSequenceToolName, "Problem cell 5 of row 1");
          Assert.AreEqual(firstSequenceToolName, cellFirstSequenceToolName.StringCellValue, "Bad first sequence tool name");
          
          NPOI.SS.UserModel.ICell cellFirstSequenceEstimatedTime = secondRow.GetCell(2);
          Assert.IsNotNull(cellFirstSequenceEstimatedTime, "Problem cell 2 of row 1");
          Assert.AreEqual(35.0, cellFirstSequenceEstimatedTime.NumericCellValue, "Bad first sequence estimated time");

          NPOI.SS.UserModel.ICell cellFirstSequenceFeedrate = secondRow.GetCell(4);
          Assert.IsNotNull(cellFirstSequenceFeedrate, "Problem cell 4 of row 1");
          Assert.AreEqual(firstSequenceFeedRate, cellFirstSequenceFeedrate.NumericCellValue, "Bad first sequence feed rate");

          NPOI.SS.UserModel.ICell cellSecondSequenceFeedrate = thirdRow.GetCell(4);
          Assert.IsNotNull(cellSecondSequenceFeedrate, "Problem cell 4 of row 2");
          Assert.AreEqual(secondSequenceFeedRate, cellSecondSequenceFeedrate.NumericCellValue, "Bad second sequence feed rate");
          
          NPOI.SS.UserModel.ICell cellFirstSequenceToolDiameter = secondRow.GetCell(6);
          Assert.IsNotNull(cellFirstSequenceToolDiameter, "Problem cell 6 of row 1");
          Assert.AreEqual(NPOI.SS.UserModel.CellType.Blank, cellFirstSequenceToolDiameter.CellType, "Bad first sequence tool diameter");

          NPOI.SS.UserModel.ICell cellSecondSequenceEstimatedTime = thirdRow.GetCell(2);
          Assert.IsNotNull(cellSecondSequenceEstimatedTime, "Problem cell 2 of row 2");
          Assert.AreEqual(NPOI.SS.UserModel.CellType.Blank, cellSecondSequenceEstimatedTime.CellType, "Bad second sequence estimated time");
          
          // update on sequences in workbook
          double firstSequenceToolDiameter = 1.25;
          // put a string value in place of a double
          cellFirstSequenceToolDiameter.SetCellValue(firstSequenceToolDiameter.ToString());
          string firstSequenceNewEstimatedTime = (10.0/60.0).ToString(); // 10 secs in mins as decimal
          cellFirstSequenceEstimatedTime.SetCellValue(firstSequenceNewEstimatedTime);
          double firstSequenceNewFeedRate = 12.54;
          // put a string value in place of a double
          cellFirstSequenceFeedrate.SetCellValue(firstSequenceNewFeedRate.ToString());
          string firstSequenceNewToolName = "New Tool name for Seq1";
          cellFirstSequenceToolName.SetCellValue(firstSequenceNewToolName);
          string secondSequenceNewName = "Seq2NewName";
          cellSecondSequenceName.SetCellValue(secondSequenceNewName);
          string secondSequenceEstimatedTime = (1.0 + 40.0 / 60.0).ToString(); // 100 secs in mins as decimal
          cellSecondSequenceEstimatedTime.SetCellValue(secondSequenceEstimatedTime);
          
          // import workbook in DB (no path given)
          exporter.ImportSequenceWorkbook(null, workbook);
          
          // check if sequences are correctly imported from workbook
          Assert.AreEqual(TimeSpan.FromSeconds (10.0), seq1.EstimatedTime, "Bad new estimated time for seq1");
          Assert.AreEqual(firstSequenceNewFeedRate, seq1FeedRateUS.Double, "Bad new feed rate for seq1");
          Assert.AreEqual(firstSequenceNewToolName, seq1ToolNameValue.String, "Bad new tool name for seq1");
          
          Assert.AreEqual(secondSequenceNewName, seq2.Name, "Bad new name for seq2");
          Assert.AreEqual(100, Math.Round (seq2.EstimatedTime.Value.TotalSeconds), "Bad estimated time for seq2");
          
          IField toolDiameterField = ModelDAOHelper.DAOFactory.FieldDAO.FindByCode("ToolDiameter");
          
          bool foundNewStampingValue = false;
          foreach(IStampingValue seq1StampingValue in seq1.StampingValues) {
            if (seq1StampingValue.Field.Equals(toolDiameterField)) {
              foundNewStampingValue = true;
              Assert.AreEqual(firstSequenceToolDiameter, seq1StampingValue.Double, "Bad tool diameter for seq1");
              break;
            }
          }
          
          Assert.IsTrue(foundNewStampingValue, "No tool diameter for seq1");
        }
      }
    }
    
    /// <summary>
    /// A test for both DataGridModifier and ExcelFileDataGrid
    /// </summary>
    [Test]
    public void TestExcelFileDataGrid()
    {

      DataGridModifier initialGridModifier = new DataGridModifier();
      DataGridView initialGridView = new DataGridView();
      initialGridView.AllowUserToAddRows = false;
      initialGridModifier.GridView = initialGridView;
      ExcelFileDataGrid.ImportFileAsGrid(Path.Combine (TestContext.CurrentContext.TestDirectory, "T3296AB 140.xls"), initialGridView, 0);
      
      DataGridModifier gridModifier = new DataGridModifier();
      gridModifier.GridView = new DataGridView();
      gridModifier.GridView.AllowUserToAddRows = false;
      ExcelFileDataGrid.ImportFileAsGrid(Path.Combine (TestContext.CurrentContext.TestDirectory, "T3296AB 140.xls"), gridModifier.GridView, 0);
      
      // prune empty rows and columns
      // undo pruning
      // resulting grid should be equivalent to initial grid
      gridModifier.PruneRowCol();
      gridModifier.Undo();
      Assert.IsTrue(gridEquivalent(gridModifier.GridView, initialGridView),
                    "Grids are not Equivalent after pruneRowCol / undo");
      
      // now prune row/cols once and for all
      initialGridModifier.PruneRowCol();
      gridModifier.PruneRowCol();
      
      // delete/insert a few rows and columns
      // then undo all
      // resulting grid should be equivalent to initial grid
      gridModifier.DeleteColumn(0);
      gridModifier.DeleteRow(0, true); // true <=> non gui mode
      gridModifier.DeleteRow(6, true);
      gridModifier.DeleteColumn(3);
      gridModifier.InsertColumn(0);
      gridModifier.InsertRow(0);
      gridModifier.DeleteColumn(2);
      gridModifier.UndoAll();
      gridModifier.PruneRowCol();
      Assert.IsTrue(gridEquivalent(gridModifier.GridView, initialGridView),
                    "Grids are not Equivalent after deleteRow, deleteColumn / undo");
      
      // do a few shift up/down
      // then undo all
      // resulting grid should be equivalent to initial grid
      gridModifier.ShiftDownColumn(1);
      gridModifier.ShiftUpColumn(0);
      gridModifier.ShiftDownColumn(0);
      gridModifier.ShiftUpColumn(0);
      gridModifier.ShiftDownColumn(2);
      gridModifier.ShiftDownColumn(2);
      gridModifier.UndoAll();
      gridModifier.PruneRowCol();
      Assert.IsTrue(gridEquivalent(gridModifier.GridView, initialGridView),
                    "Grids are not Equivalent after shift column up or down / undo");
      
      // apply a macro to initial grid
      // load the oracle in form of an Excel file
      // the two resulting grids should be equivalent
      // nb: it's important to prune out the data grid obtained
      // from the Excel file since there are almost always
      // irrelevant trailing columns
      gridModifier.LoadMacro(Path.Combine (TestContext.CurrentContext.TestDirectory, "macro1.xml"));
      DataGridView resultGridView = new DataGridView();
      resultGridView.AllowUserToAddRows = false;
      ExcelFileDataGrid.ImportFileAsGrid(Path.Combine (TestContext.CurrentContext.TestDirectory, "macro1Result.xls"), resultGridView, 0);
      DataGridModifier resultGridModifier = new DataGridModifier();
      resultGridModifier.GridView = resultGridView;
      resultGridModifier.PruneRowCol();
      Assert.IsTrue(gridEquivalent(gridModifier.GridView, resultGridView),
                    "Grids are not Equivalent after LoadMacro / Import");
      
      // undo all macro instructions then prune out
      // should roll back to initial grid
      gridModifier.UndoAll();
      gridModifier.PruneRowCol();
      Assert.IsTrue(gridEquivalent(gridModifier.GridView, initialGridView),
                    "Grids are not Equivalent after LoadMacro / undoAll");
      
      /// save as excel then reload and compare
      string tmpFileName = System.IO.Path.GetTempPath() + Guid.NewGuid().ToString() + ".xls";
      DataGridView tmpGridView = new DataGridView();
      DataGridModifier tmpGridModifier = new DataGridModifier();
      tmpGridView.AllowUserToAddRows = false;
      tmpGridModifier.GridView = tmpGridView;
      ExcelFileDataGrid.ExportGridAsFile(tmpFileName, "MySheet", gridModifier.GridView, null);
      ExcelFileDataGrid.ImportFileAsGrid(tmpFileName, tmpGridView, 0);
      File.Delete(tmpFileName);
      tmpGridModifier.PruneRowCol();
      Assert.IsTrue(gridEquivalent(gridModifier.GridView, tmpGridView),
                    "Grids are not Equivalent after Save / Load");
    }
    
    [OneTimeSetUp]
    public void Init()
    {
      
      m_previousDSNName = System.Environment.GetEnvironmentVariable ("DefaultDSNName");
      System.Environment.SetEnvironmentVariable ("DefaultDSNName",
                                                 "LemoineUnitTests");
    }
    
    [OneTimeTearDown]
    public void Dispose()
    {
      
      if (m_previousDSNName != null) {
        System.Environment.SetEnvironmentVariable ("DefaultDSNName",
                                                   m_previousDSNName);
      }
    }

  }
}
