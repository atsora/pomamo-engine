// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.IO;

using Lemoine.Core.Log;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using Lemoine.ModelDAO;
using Lemoine.I18N;

namespace Lemoine.ExcelDataGrid
{
  /// <summary>
  /// Exception raised in case of Excel file Access problem
  /// </summary>
  public class ExcelImporterFileAccessException : Exception {
    
    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    public ExcelImporterFileAccessException() {}
    
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="msg"></param>
    public ExcelImporterFileAccessException(string msg) : base (msg) {}
    
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="msg"></param>
    /// <param name="innerException"></param>
    public ExcelImporterFileAccessException(string msg, Exception innerException) : base(msg, innerException) {}
    #endregion  // Constructors
  }
  
  /// <summary>
  /// Exception raised in case of a problem with content of Excel file
  /// </summary>
  public class ExcelImporterBadFormatException : Exception {
    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    public ExcelImporterBadFormatException() {}
    
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="msg"></param>
    public ExcelImporterBadFormatException(string msg) : base (msg) {}
    
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="msg"></param>
    /// <param name="innerException"></param>
    public ExcelImporterBadFormatException(string msg, Exception innerException) : base(msg, innerException) {}
    #endregion  // Constructors
  }
  
  
  /// <summary>
  /// Description of ExcelExporter.
  /// </summary>
  public class ExcelExporter
  {
    #region Members
    static readonly string m_sequenceIdColumnHeader = "Sequence Id";

    enum FormatInfo {
      None = 0,   // seconds (decimal)
      HHMMSS = 1, // HH:MM:SS
      MM = 2      // minutes (decimal)
    }
    
    class Pair<T, U> {
      /// <summary>
      /// Build a pair (first,second)
      /// </summary>
      /// <param name="first"></param>
      /// <param name="second"></param>
      public Pair(T first, U second) {
        this.First = first;
        this.Second = second;
      }

      /// <summary>
      /// First element of pair
      /// </summary>
      public T First { get; set; }
      
      /// <summary>
      /// Second element of pair
      /// </summary>
      public U Second { get; set; }
    }
    
    bool m_onlyActiveFields;
    string[] m_columnPropertyIdentifiers;
    string[] m_columnFieldIdentifiers;
    SortedDictionary<int, Pair<PropertyInfo,FormatInfo>> m_indexToPropertyInfo = new SortedDictionary<int, Pair<PropertyInfo,FormatInfo>>();
    SortedDictionary<int, Lemoine.Model.IField> m_indexToFieldInfo = new SortedDictionary<int, Lemoine.Model.IField>();
    
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (ExcelExporter).FullName);

    #region Getters / Setters
    /// <summary>
    /// List of properties (in descending hierarchical order) to look at each property column
    /// </summary>
    public string[] ColumnPropertyIdentifiers {
      get { return m_columnPropertyIdentifiers; }
      set {
        m_columnPropertyIdentifiers = value;
        PropertyInfo[] seqPropInfos = typeof(Lemoine.Model.ISequence).GetProperties(BindingFlags.Public|BindingFlags.Instance);
        for(int i = 0 ; i < value.Length ; i++) {
          string propertyString = value[i];
          string[] propertyInfoStrings = propertyString.Split(new char[] { '=' });
          string propertyName = propertyInfoStrings[0];
          FormatInfo propertyFormatInfo = FormatInfo.None;
          if (propertyInfoStrings.Length > 1) {
            string propertyInfoStrings1Upper = propertyInfoStrings[1].ToUpper();
            if (propertyInfoStrings1Upper == "HH:MM:SS") {
              propertyFormatInfo = FormatInfo.HHMMSS;
            } else if (propertyInfoStrings1Upper == "MM") {
              propertyFormatInfo = FormatInfo.MM;
            }
            m_columnPropertyIdentifiers[i] = propertyName;
          }
          foreach (PropertyInfo propertyInfo in seqPropInfos) {
            if (propertyInfo.Name == propertyName) {
              m_indexToPropertyInfo[i] = new Pair<PropertyInfo,FormatInfo>(propertyInfo,propertyFormatInfo);
            }
          }
        }
      }
    }
    
    /// <summary>
    /// List of stampingvalue fields to look for at each field column
    /// </summary>
    public string[] ColumnFieldIdentifiers {
      get { return m_columnFieldIdentifiers; }
      set {
        List<string> keptFieldList = new List<string>();
        int currentIndex = 0;
        using(IDAOSession daoSession = ModelDAOHelper.DAOFactory.OpenSession()) {
          for(int i = 0 ; i < value.Length ; i++) {
            string fieldCode = value[i];
            Lemoine.Model.IField field = ModelDAOHelper.DAOFactory.FieldDAO.FindByCode(fieldCode);
            if (field != null) {
              if (!m_onlyActiveFields || field.Active) {
                m_indexToFieldInfo[currentIndex] = field;
                currentIndex++;
                keptFieldList.Add(fieldCode);
              }
            }
          }
        }
        m_columnFieldIdentifiers = keptFieldList.ToArray();
      }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Exporter constructor
    /// </summary>
    public ExcelExporter (bool onlyActiveFields)
    {
      m_onlyActiveFields = onlyActiveFields;
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Interprets cell as a value
    /// </summary>
    /// <param name="cell"></param>
    /// <returns></returns>
    public static object GetCellValue(ICell cell) {
      switch(cell.CellType) {
        case CellType.Numeric:
          ICellStyle cellStyle = cell.CellStyle;
          if ((cellStyle != null) && (cellStyle.DataFormat > 0)) {
            if (HSSFDateUtil.IsCellDateFormatted(cell)) {
              // a date time is translated into a double representing the total number of seconds
              DateTime dateTime = HSSFDateUtil.GetJavaDate(cell.NumericCellValue);
              DateTime refDateTime = HSSFDateUtil.GetJavaDate(0);
              TimeSpan elapsedTime = new TimeSpan(dateTime.Ticks - refDateTime.Ticks);
              return elapsedTime.TotalSeconds;
            }
          }
          return cell.NumericCellValue;
        case CellType.String:
          return cell.StringCellValue;
        default:
          return null;
      }
    }

    object FormatPropertyValue(object newValue, PropertyInfo propertyInfo, FormatInfo propertyFormatInfo)
    {
      object formattedValue = newValue;
      if (propertyFormatInfo == FormatInfo.HHMMSS) {
        try {
          TimeSpan duration = TimeSpan.Parse (newValue.ToString ());
          if (propertyInfo.PropertyType.Equals (typeof (TimeSpan))
              || propertyInfo.PropertyType.Equals (typeof (TimeSpan?))) {
            formattedValue = duration;
          }
          else {
            formattedValue = duration.TotalSeconds;
          }
        }
        catch (Exception) {
        }
      }
      else if (propertyFormatInfo == FormatInfo.MM) {
        try {
          double durationInMinutes = Double.Parse(newValue.ToString());
          if (propertyInfo.PropertyType.Equals (typeof (TimeSpan))
              || propertyInfo.PropertyType.Equals (typeof (TimeSpan?))) {
            formattedValue = TimeSpan.FromMinutes (durationInMinutes);
          }
          else {
            formattedValue = Math.Round(durationInMinutes * 60, 0, MidpointRounding.AwayFromZero); // round to nearest integer, up if middlepoint
          }
        }
        catch (Exception) {
        }
      }
      
      try {
        if ( (propertyInfo.PropertyType.Equals (typeof (TimeSpan))
              || propertyInfo.PropertyType.Equals (typeof (TimeSpan?)))
            && !(formattedValue is TimeSpan)
            && !(formattedValue is TimeSpan?)) {
          formattedValue = TimeSpan.FromSeconds ((double)formattedValue);
        }
      }
      catch (Exception) {
      }
      
      return formattedValue;
    }
    
    void UpdateProperty(PropertyInfo propertyInfo, FormatInfo propertyFormatInfo,object objectToUpdate, object newValue)
    {
      if ((propertyInfo.PropertyType.BaseType == typeof(System.Enum)) &&
          (newValue.GetType() == typeof(string)))
      {
        // "hack" for parsing string as enum
        propertyInfo.SetValue(objectToUpdate,
                              Enum.Parse(propertyInfo.PropertyType,
                                         (string) newValue),
                              null);
      } else {
        object formattedValue = FormatPropertyValue(newValue, propertyInfo, propertyFormatInfo);
        propertyInfo.SetValue(objectToUpdate, formattedValue, null);
      }
    }

    void SetStampingValue(Lemoine.Model.IStampingValue stampingValue, Lemoine.Model.IField field, object newValue)
    {
      switch(field.Type) {
        case Lemoine.Model.FieldType.Double:
          if (newValue is double) {
            stampingValue.Double = new Nullable<double>((double) newValue);
          } else if (newValue is string) {
            double outValue;
            if (Double.TryParse((string) newValue, out outValue)) {
              stampingValue.Double = new Nullable<double>((double) outValue);
            }
          }
          break;
        case Lemoine.Model.FieldType.Int32:
          if (newValue is double) {
            stampingValue.Int = new Nullable<int>((int) newValue);
          } else if (newValue is string) {
            int outValue;
            if (Int32.TryParse((string) newValue, out outValue)) {
              stampingValue.Int = new Nullable<int>((int) outValue);
            }
          }
          break;
        case Lemoine.Model.FieldType.Boolean:
        case Lemoine.Model.FieldType.String:
          stampingValue.String = newValue.ToString();
          break;
      }
    }

    /// <summary>
    /// Update the sequences in DB as defined in an Excel file
    /// </summary>
    /// <param name="path">May be null (otherwise sequences should belong to it)</param>
    /// <param name="excelFilePath"></param>
    /// <returns></returns>
    public void ImportSequenceFile(Lemoine.Model.IPath path, string excelFilePath) {
      try {
        using (FileStream file = new FileStream(excelFilePath, FileMode.Open, FileAccess.Read))
        {
          HSSFWorkbook workbook = new HSSFWorkbook(file);
          ImportSequenceWorkbook(path, workbook);
        }
      } catch (IOException ex) {
        string errorMsg = PulseCatalog.GetString("ProblemOnFileOpenA") + excelFilePath + ".\n" + PulseCatalog.GetString("CheckNotOpenedInOtherApp");
        log.ErrorFormat(errorMsg);
        throw new ExcelImporterFileAccessException(String.Format("Problem while accessing Excel file {0}", excelFilePath),
                                                   ex);
      }
    }
    
    /// <summary>
    /// Update the sequences in DB as defined in an Excel workbook
    /// Sequences should belong to path passed as argument if not null
    /// </summary>
    /// <param name="path"></param>
    /// <param name="workbook"></param>
    public void ImportSequenceWorkbook(Lemoine.Model.IPath path, HSSFWorkbook workbook) {
      // find column index with header equal to m_sequenceIdColumnHeader (column of sequence ids)
      ISheet firstSheet = workbook.GetSheetAt(0);
      if (firstSheet == null) {
        throw new ExcelImporterBadFormatException("Problem with Excel workbook content: no sheet");
      }
      IRow firstRow = firstSheet.GetRow(0);
      if (firstRow == null) {
        throw new ExcelImporterBadFormatException("Problem with Excel workbook content: no first row");
      }
      IEnumerator enumCells = firstRow.GetEnumerator();
      int sequenceIdIndex = -1;
      int index = 0;
      while(enumCells.MoveNext()) {
        ICell cell = (ICell) enumCells.Current;
        if (cell.StringCellValue.Equals(m_sequenceIdColumnHeader)) {
          sequenceIdIndex = index;
          break;
        }
        index++;
      }
      if (sequenceIdIndex != 0) {
        // Id should be in first column
        throw new ExcelImporterBadFormatException("Problem with Excel workbook content: sequence Id should be in first column");
      }
      
      // Go through all non-empty rows starting from second one
      // and update sequence accordingly
      
      try {
        using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
          using (IDAOTransaction transaction = session.BeginTransaction()) {
            for(int rowIndex = 1; rowIndex <= firstSheet.LastRowNum ; rowIndex++) {
              IRow row = firstSheet.GetRow(rowIndex);
              ICell cellSeqId= row.GetCell(sequenceIdIndex);
              if (cellSeqId == null)  {
                log.InfoFormat("Excel workbook read up to row {0} which has no associated sequence", rowIndex);
                break;
              }
              int sequenceId = (int) cellSeqId.NumericCellValue;
              
              if (sequenceId > 0) {
                Lemoine.Model.ISequence seq = ModelDAOHelper.DAOFactory.SequenceDAO.FindById(sequenceId);
                
                if ((seq != null) && ((path == null) || (seq.Path.Equals(path)))) {
                  // update properties
                  foreach (KeyValuePair<int, Pair<PropertyInfo,FormatInfo>> colProp in this.m_indexToPropertyInfo) {
                    ICell cell = row.GetCell(1 + colProp.Key);
                    if (cell != null) {
                      PropertyInfo propertyInfo = colProp.Value.First;
                      FormatInfo propertyFormatInfo = colProp.Value.Second;
                      object propValue = propertyInfo.GetValue(seq, null);
                      object newValue = GetCellValue(cell);
                      if (newValue != null) {
                        UpdateProperty(propertyInfo, propertyFormatInfo, seq, newValue);
                      }
                    }
                  }
                  
                  // update or create stamping values (fields)
                  foreach(KeyValuePair<int, Lemoine.Model.IField> colField in this.m_indexToFieldInfo) {
                    bool foundField = false;
                    ICell cell = row.GetCell(1 + colField.Key + m_columnPropertyIdentifiers.Length);
                    if (cell != null) {
                      Lemoine.Model.IField field = ModelDAOHelper.DAOFactory.FieldDAO.FindById(colField.Value.Id);
                      foreach (Lemoine.Model.IStampingValue stampingValue in seq.StampingValues) {
                        if (stampingValue.Field.Code.Equals(field.Code)) {
                          object newValue = GetCellValue(cell);
                          if (newValue != null) {
                            SetStampingValue(stampingValue, field, newValue);
                          }
                          foundField = true;
                          break;
                        }
                      }
                      if (!foundField) {
                        object newValue = GetCellValue(cell);
                        if (newValue != null) {
                          // stamping value corresponding to field not found: create it
                          Lemoine.Model.IStampingValue newStampingValue = ModelDAOHelper.ModelFactory.CreateStampingValue(seq, field);
                          SetStampingValue(newStampingValue, field, newValue);
                          ModelDAOHelper.DAOFactory.StampingValueDAO.MakePersistent(newStampingValue);
                        }
                      }
                    }
                  }
                } // else sequence not found, do nothing
              }
            }
            
            transaction.Commit();
          }
        }
      } catch (Exception ex) {
        // TODO: problem with cell value
        throw new ExcelImporterBadFormatException("Problem with Excel workbook content", ex);
      }
      return;
    }

    void SetCellValue(ICell cell, object propValue, FormatInfo propertyFormatInfo) {
      if (propertyFormatInfo == FormatInfo.None) {
        if (propValue is double) {
          cell.SetCellValue((double) propValue);
        }
        else if (propValue is int) {
          cell.SetCellValue((int) propValue);
        }
        else if (propValue is TimeSpan) {
          cell.SetCellValue ( ((TimeSpan) propValue).TotalSeconds);
        }
        else {
          cell.SetCellValue(propValue.ToString());
        }
      } else if (propertyFormatInfo == FormatInfo.HHMMSS) {
        if ((propValue is double) || (propValue is int)) {
          TimeSpan duration = TimeSpan.FromSeconds((double) propValue);
          cell.SetCellValue(String.Format("{0:00}:{1:00}:{2:00}",
                                          (int) duration.TotalHours,
                                          duration.Minutes,
                                          duration.Seconds));
        }
        else {
          cell.SetCellValue(propValue.ToString());
        }
      } else if (propertyFormatInfo == FormatInfo.MM) {
        if ((propValue is double) || (propValue is int)) {
          double durationInMinutes = ((double) propValue) / 60;
          cell.SetCellValue((double) durationInMinutes);
        }
        else if (propValue is TimeSpan) {
          cell.SetCellValue ( ((TimeSpan) propValue).TotalMinutes);
        }
      }
    }

    /// <summary>
    /// Export sequences of a path as an Excel file
    /// </summary>
    /// <param name="path"></param>
    /// <param name="excelFilePath"></param>
    public bool ExportPathAsFile(Lemoine.Model.IPath path, string excelFilePath)
    {
      HSSFWorkbook workbook = ExportPathAsWorkbook(path);
      try {
        FileStream file = new FileStream(excelFilePath, FileMode.Create);
        workbook.Write(file);
        file.Close();
        return true;
      } catch (IOException ex) {
        string errorMsg =
          PulseCatalog.GetString("ProblemOnExcelFileGenA") + " " + ex.ToString();
        log.Error(errorMsg);
        return false;
      }
    }
    
    /// <summary>
    /// Export sequences of a path as an Excel workbook
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public HSSFWorkbook ExportPathAsWorkbook(Lemoine.Model.IPath path)
    {
      HSSFWorkbook workbook = new HSSFWorkbook();
      
      int maxPropertyColumn = m_columnPropertyIdentifiers.Length;
      int maxFieldColumn = m_columnFieldIdentifiers.Length;
      
      ISheet sheet = workbook.CreateSheet();
      
      IRow firstRow = sheet.CreateRow(0);

      {
        ICell cell = firstRow.CreateCell(0);
        cell.SetCellValue(m_sequenceIdColumnHeader);
      }

      for(int i = 0 ; i < m_columnPropertyIdentifiers.Length ; i++) {
        ICell cell = firstRow.CreateCell(i + 1);
        string columnIdentifier = m_columnPropertyIdentifiers[i];
        string columnHeader;
        switch(columnIdentifier) {
          case "Name":
            columnHeader = PulseCatalog.GetString("SequenceName");
            break;
          case "EstimatedTime":
            columnHeader = PulseCatalog.GetString("SequenceEstimatedTime");
            break;
          default:
            columnHeader = columnIdentifier;
            break;
        }
        cell.SetCellValue(columnHeader);
      }
      
      
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        for(int i = 0 ; i < m_columnFieldIdentifiers.Length ; i++) {
          ICell cell = firstRow.CreateCell(1 + i + maxPropertyColumn);
          string columnIdentifier = m_columnFieldIdentifiers[i];
          Lemoine.Model.IField field = ModelDAOHelper.DAOFactory.FieldDAO.FindByCode(columnIdentifier);
          if (field != null) {
            cell.SetCellValue(field.Display);
          } else {
            cell.SetCellValue(columnIdentifier);
          }
        }
      }
      
      if (path == null) {
        return workbook;
      }
      
      int rowIndex = 1;
      
      ICellStyle lockStyle = workbook.CreateCellStyle();
      lockStyle.IsLocked = false;
      
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {

        path = ModelDAOHelper.DAOFactory.PathDAO.FindById(((Lemoine.Collections.IDataWithId)path).Id);
        foreach(Lemoine.Model.ISequence seq in path.Sequences) {
          IRow row = sheet.CreateRow(rowIndex);

          int currentCellIndex = 0;
          
          {
            // add (hidden) id column
            ICell cell = row.CreateCell (currentCellIndex);
            cell.SetCellValue(((Lemoine.Collections.IDataWithId)seq).Id);
            currentCellIndex++;
          }
          
          // create property columns
          foreach (KeyValuePair<int, Pair<PropertyInfo,FormatInfo>> colProp in this.m_indexToPropertyInfo) {
            ICell cell = row.CreateCell(currentCellIndex);
            currentCellIndex++;
            
            PropertyInfo propertyInfo = colProp.Value.First;
            object propValue = propertyInfo.GetValue(seq, null);
            if (propValue != null) {
              SetCellValue(cell, propValue, colProp.Value.Second);
              cell.CellStyle = lockStyle;
            }
            
            // TODO: do this at end on all relevant cells
            if (propertyInfo.PropertyType.BaseType == typeof(System.Enum)) {
              CellRangeAddressList cellList = new CellRangeAddressList(cell.RowIndex, cell.RowIndex, cell.ColumnIndex, cell.ColumnIndex);

              IDataValidationConstraint dvcstr = DVConstraint.CreateExplicitListConstraint(Lemoine.Model.SequenceKind.GetNames(typeof(Lemoine.Model.SequenceKind)));
              IDataValidation dv = new HSSFDataValidation(cellList, dvcstr);
              sheet.AddValidationData(dv);
            }
            
          }
          
          // create field columns
          
          foreach(KeyValuePair<int, Lemoine.Model.IField> colField in this.m_indexToFieldInfo) {
            ICell cell = row.CreateCell(currentCellIndex);
            currentCellIndex++;
            cell.CellStyle = lockStyle;
            Lemoine.Model.IField field = ModelDAOHelper.DAOFactory.FieldDAO.FindById(colField.Value.Id);
            foreach (Lemoine.Model.IStampingValue stampingValue in seq.StampingValues) {
              if (stampingValue.Field.Code.Equals(field.Code)) {
                switch(field.Type) {
                  case Lemoine.Model.FieldType.Double:
                    if (stampingValue.Double.HasValue) {
                      cell.SetCellValue(stampingValue.Double.Value);
                    }
                    break;
                  case Lemoine.Model.FieldType.Int32:
                    if (stampingValue.Int.HasValue) {
                      cell.SetCellValue(stampingValue.Int.Value);
                    }
                    break;
                  case Lemoine.Model.FieldType.Boolean:
                  case Lemoine.Model.FieldType.String:
                    cell.SetCellValue(stampingValue.String);
                    break;
                }
              }

            }
          }
          
          rowIndex++;
        }
        
        for(int i = 0 ; i < maxPropertyColumn + maxFieldColumn ; i++) {
          sheet.AutoSizeColumn(i);
        }
        sheet.SetColumnHidden(0, true);
      }
      
      return workbook;
    }
    #endregion // Methods
  }
}
