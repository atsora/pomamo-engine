// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Globalization;
using System.Collections.Generic;
using System.Windows.Forms;
using System.IO;

using Lemoine.Core.Log;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using Lemoine.I18N;

namespace Lemoine.ExcelDataGrid
{
  /// <summary>
  /// Description of ExcelFileDataGrid.
  /// </summary>
  public class ExcelFileDataGrid
  {
    static readonly ILog log = LogManager.GetLogger(typeof (ExcelFileDataGrid).FullName);
    #region Members
    /// <summary>
    /// maximum number of columns which will be imported
    /// </summary>
    static readonly int MAX_COLUMNS = 50;
    
    /// <summary>
    /// maximum numbers of decimal kept for Excel formula returning doubles
    /// </summary>
    static readonly int MAX_DECIMALS = 3;
    #endregion // Members
    
    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public ExcelFileDataGrid ()
    {
    }
    #endregion // Constructors

    #region Methods
    
    private static string processCellAsNumeric(ICell cell, CellValue cellValue) {
      ICellStyle cellStyle = cell.CellStyle;
      if ((cellStyle != null) && (cellStyle.DataFormat > 0)) {
        double cellValueAsDouble;
        if (HSSFDateUtil.IsCellDateFormatted(cell)) {
          // a date time is translated into a double representing the total number of seconds
          DateTime dateTime = HSSFDateUtil.GetJavaDate(cellValue.NumberValue);
          DateTime refDateTime = HSSFDateUtil.GetJavaDate(0);
          TimeSpan elapsedTime = new TimeSpan(dateTime.Ticks - refDateTime.Ticks);
          cellValueAsDouble = elapsedTime.TotalSeconds;
        } else {
          HSSFDataFormatter hsformat = new HSSFDataFormatter();
          NPOI.SS.Util.FormatBase format = hsformat.CreateFormat(cell);
          if (format != null) {
            try {
              string formattedAsString = format.Format(cellValue.NumberValue);
              cellValueAsDouble = double.Parse(formattedAsString);
            } catch(FormatException) {
              // unable to decipher format
              cellValueAsDouble = Math.Round(cellValue.NumberValue, MAX_DECIMALS);
            }
          } else {
            cellValueAsDouble = Math.Round(cellValue.NumberValue, MAX_DECIMALS);
          }
        }
        return cellValueAsDouble.ToString(CultureInfo.InvariantCulture);
      } else {
        // format to MAX_DECIMALS if no formatting available
        return Math.Round(cellValue.NumberValue, MAX_DECIMALS).ToString(CultureInfo.InvariantCulture);
      }
    }

    /// <summary>
    /// import sheet number <paramref name="sheetNumber"/>
    /// from workbook <paramref name="workbook" />
    /// into a DataGridView <paramrefname name="dataGridView" />
    /// </summary>
    /// <param name="workbook"> </param>
    /// <param name="dataGridView"></param>
    /// <param name="sheetNumber"></param>
    /// <returns>List of named regions of first sheet</returns>
    public static IList<GridSelectionEvent>
      ImportWorkBookAsGrid(HSSFWorkbook workbook, DataGridView dataGridView, int sheetNumber) {
      IList<GridSelectionEvent> regions = new List<GridSelectionEvent>();
      
      if (workbook.NumberOfSheets <= sheetNumber)
        return regions;
      
      ISheet sheet = workbook.GetSheetAt(sheetNumber);
      
      // create regions for first sheet
      for(int i = 0 ; i < workbook.NumberOfNames ; i++) {
        IName name = workbook.GetNameAt(i);
        if (name.SheetName == sheet.SheetName) {
          CellRangeAddress cellRange =
            CellRangeAddress.ValueOf(name.RefersToFormula); // hopefully good enough
          GridSelectionEvent gridSelectionEvent =
            new GridSelectionEvent(name.NameName,
                                   cellRange.FirstRow,
                                   cellRange.LastRow,
                                   cellRange.FirstColumn,
                                   cellRange.LastColumn);
          regions.Add(gridSelectionEvent);
        }
      }
      
      dataGridView.Rows.Clear();
      dataGridView.Columns.Clear();
      
      for (int j = 0; j < MAX_COLUMNS ; j++) // provide enough colums to fetch excel file
      {
        string columnName = "COL" + j.ToString();
        dataGridView.Columns.Add(columnName, columnName);
      }

      // go through logical rows until
      // the right number of physical rows of the sheet have been read
      // (cf. regions are defined wrt. logical rows,
      //  while enumerator on rows proceeds only on physical rows)
      int indexPhysicalRow = 0;
      int indexLogicalRow = 0;
      while(indexPhysicalRow < sheet.PhysicalNumberOfRows) {
        IRow row = sheet.GetRow(indexLogicalRow); // logical row: may be null
        indexLogicalRow++;
        int newRowIndex = dataGridView.Rows.Add();
        DataGridViewRow dataGridRow = dataGridView.Rows[newRowIndex];
        if (row != null) {
          indexPhysicalRow++;
          for (int i = 0; i < (row.LastCellNum < MAX_COLUMNS ? row.LastCellNum : MAX_COLUMNS) ; i++)
          {
            ICell cell = row.GetCell(i);
            if (cell == null) dataGridRow.Cells[i].Value = "";
            else {
              switch(cell.CellType) {
                case CellType.Formula:
                  // in case a cell contains a formula, evaluate it
                  IFormulaEvaluator formulaEvaluator = workbook.GetCreationHelper().CreateFormulaEvaluator();
                  CellValue cellValue = formulaEvaluator.Evaluate(cell);
                  
                  if (cellValue.CellType == CellType.Numeric) {
                    dataGridRow.Cells[i].Value = processCellAsNumeric(cell, cellValue);
                  } else {
                    if (cellValue.CellType == CellType.String) {
                      dataGridRow.Cells[i].Value = cellValue.StringValue;
                    } else {
                      dataGridRow.Cells[i].Value = cellValue.FormatAsString();
                    }
                    // get rid of cell containing string ""
                    if (dataGridRow.Cells[i].Value.ToString().Equals(@""""""))
                      dataGridRow.Cells[i].Value = "";
                  }
                  break;
                  
                case CellType.Numeric:
                  // format numeric as string but using invariant culture (avoids problem later on when importing in db)
                  dataGridRow.Cells[i].Value =
                    processCellAsNumeric(cell,
                                         workbook.GetCreationHelper().CreateFormulaEvaluator().Evaluate(cell));
                  break;
                  
                default:
                  dataGridRow.Cells[i].Value = cell.ToString();
                  if (dataGridRow.Cells[i].Value.ToString().Equals(@""""""))
                    dataGridRow.Cells[i].Value = "";
                  break;
              }
            }
          }
        }
      }
      
      // disable sorting
      foreach (DataGridViewColumn col in dataGridView.Columns)
        col.SortMode = DataGridViewColumnSortMode.NotSortable;
      
      return regions;
    }
    
    /// <summary>
    /// import sheet number <paramref name="sheetNumber"/>
    /// from Excel .xls file found in file path <paramref name="path" />
    /// into a DataGridView <paramrefname name="dataGridView" />
    /// </summary>
    /// <param name="path"> </param>
    /// <param name="dataGridView"></param>
    /// <param name="sheetNumber"></param>
    /// <returns>List of named regions of first sheet</returns>
    public static IList<GridSelectionEvent>
      ImportFileAsGrid(string path, DataGridView dataGridView, int sheetNumber) {
      
      HSSFWorkbook workbook;
      
      // initialize workbook from Excel file (.xls)
      // TODO: also handle .xlsx files
      try {
        using (FileStream file = new FileStream(path, FileMode.Open, FileAccess.Read))
        {
          workbook = new HSSFWorkbook(file); // PH: old .xls format ?
        }
      } catch (IOException) {
        string errorMsg = PulseCatalog.GetString("ProblemOnFileOpenA") + path + ".\n" + PulseCatalog.GetString("CheckNotOpenedInOtherApp");
        log.ErrorFormat(errorMsg);
        MessageBox.Show(errorMsg);
        return null;
      }

      return ImportWorkBookAsGrid(workbook, dataGridView, sheetNumber);
    }

    /// <summary>
    /// create a Sheet with name <paramref name="sheetName"/>
    /// in workbook <paramref name="workbook"/> with header
    /// row style <paramref name="headerRowStyle" />
    /// using the content of <paramref name="dataGridView" />
    /// </summary>
    /// <param name="workbook"></param>
    /// <param name="sheetName"></param>
    /// <param name="headerRowStyle"></param>
    /// <param name="dataGridView"></param>
    /// <returns></returns>
    ///
    private static ISheet
      CreateExportDataTableSheetAndHeaderRow(IWorkbook workbook,
                                             string sheetName,
                                             ICellStyle headerRowStyle,
                                             DataGridView dataGridView)
    {
      ISheet sheet =  workbook.CreateSheet(sheetName);
      IRow row = sheet.CreateRow(0);
      for(int colIndex = 0 ; colIndex < dataGridView.Columns.Count ; colIndex++)
      {
        ICell cell = row.CreateCell(colIndex);
        cell.SetCellType(CellType.String);
        cell.SetCellValue(dataGridView.Columns[colIndex].Name);
        if (headerRowStyle != null)
          cell.CellStyle = headerRowStyle;
      }
      return sheet;
    }

    /// <summary>
    /// Save grid <paramref name="dataGridView" /> in file
    /// <paramref name="fileName" /> as an Excel workbook
    /// with a single sheet <paramref name="sheetName" />
    /// </summary>
    /// <param name="fileName"></param>
    /// <param name="sheetName"></param>
    /// <param name="dataGridView"></param>
    /// <param name="regions"></param>
    public static void ExportGridAsFile(string fileName, string sheetName, DataGridView dataGridView,
                                        IList<GridSelectionEvent> regions)
    {
      HSSFWorkbook workbook = new HSSFWorkbook();
      ISheet sheet = CreateExportDataTableSheetAndHeaderRow(workbook, sheetName, null, dataGridView);
      for (int rowIndex = 0 ; rowIndex < dataGridView.Rows.Count  ; rowIndex++) {
        IRow row = sheet.CreateRow(rowIndex);
        for(int colIndex = 0 ; colIndex < dataGridView.Columns.Count ; colIndex++) {
          ICell cell = row.CreateCell(colIndex);
          cell.SetCellType(CellType.String);
          DataGridViewCell gridCell = dataGridView.Rows[rowIndex].Cells[colIndex];
          if ((gridCell != null) && (gridCell.Value !=null))
            cell.SetCellValue(gridCell.Value.ToString());
          else cell.SetCellValue("");
        }
      }
      
      if (regions != null) {
        foreach(GridSelectionEvent region in regions) {
          CellRangeAddress cellRange =
            new CellRangeAddress(region.MinRow, region.MaxRow,
                                 region.MinColumn, region.MaxColumn);
          
          string cellRangeAsString = cellRange.FormatAsString(sheetName, true);
          
          // create named region in workbook
          // do not position name.SheetIndex
          IName name = workbook.CreateName();
          name.NameName = region.RegionName;
          name.RefersToFormula = cellRangeAsString;
        }
      }
      
      try {
        FileStream file = new FileStream(fileName, FileMode.Create);
        workbook.Write(file);
        file.Close();
      } catch (IOException ex) {
        string errorMsg = 
          PulseCatalog.GetString("ProblemOnExcelFileGenA") + " " + ex.ToString();
        log.Error(errorMsg);
        MessageBox.Show(errorMsg);
        return;
      }
    }

    #endregion // Methods
  }
}
