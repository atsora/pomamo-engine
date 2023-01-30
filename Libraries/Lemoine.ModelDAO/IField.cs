// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Lemoine.Model
{
  /// <summary>
  /// List of some base default Ids for Fields
  /// </summary>
  public enum FieldId
  {
    /// <summary>
    /// Dry run field
    /// </summary>
    DryRun = 118,
    /// <summary>
    /// Stack light field
    /// </summary>
    StackLight = 126,
    /// <summary>
    /// Flow, for example in a waterjet cutting machine
    /// </summary>
    Flow = 132,
    /// <summary>
    /// Program file name
    /// </summary>
    ProgramFileName = 133,
    /// <summary>
    /// Sub-program file name
    /// </summary>
    SubProgramFileName = 134,
  }
  
  /// <summary>
     /// List of supported types for the Type property of the field
     /// </summary>
  public enum FieldType
  {
    /// <summary>
    /// string type
    /// </summary>
    String,
    /// <summary>
    /// int type
    /// </summary>
    Int32,
    /// <summary>
    /// double type
    /// </summary>
    Double,
    /// <summary>
    /// bool type
    /// </summary>
    Boolean
  }
  
  /// <summary>
  /// The Stamping Data Type allows to know what to do during the stamping phase
  /// if such a field is found
  /// </summary>
  [Flags]
  public enum StampingDataType
  {
    /// <summary>
    /// Do nothing
    /// </summary>
    None = 0,
    /// <summary>
    /// Add the field and its value in the Stamping Value table
    /// </summary>
    Data = 1,
    /// <summary>
    /// Try to match the right table and column
    /// and make the correct relations between the Sequence table and associated table
    /// </summary>
    DbField = 2
  }
  
  /// <summary>
  /// Action to take with a new Cnc value
  /// </summary>
  public enum CncDataAggregationType
  {
    /// <summary>
    /// Do nothing
    /// </summary>
    None = 0,
    /// <summary>
    /// Make an average of the got Cnc values
    /// </summary>
    Average = 1,
    /// <summary>
    /// Add the Cnc values
    /// </summary>
    Sum = 2,
    /// <summary>
    /// Replace the Cnc value
    /// </summary>
    NewValue = 3,
    /// <summary>
    /// Keep the local maximum
    /// </summary>
    Max = 4,
  }
  
  /// <summary>
  /// Model for table Field
  /// 
  /// This is a new table.
  /// 
  /// A field represents a type of data that is either drawn from the CNC
  /// or from the ISO file.
  /// It allows a much more flexibility that hard-coding the different
  /// types of data in the different tables.
  /// </summary>
  public interface IField: IDataWithIdentifiers, IDataWithVersion, ISelectionable, ISerializableModel, IDisplayable
  {
    // Note: IUnit does not inherit from IVersionable and IDataWithTranslation
    //       else the corresponding properties can't be used in a DataGridView binding
    
    /// <summary>
    /// ID
    /// </summary>
    int Id { get; }

    /// <summary>
    /// Name
    /// </summary>
    string Name { get; set; }
    
    /// <summary>
    /// TranslationKey
    /// </summary>
    string TranslationKey { get; set; }

    /// <summary>
    /// Code that identifies the field (unique)
    /// </summary>
    string Code { get; set; }
    
    /// <summary>
    /// Description of the field (optional)
    /// </summary>
    string Description { get; set; }
    
    /// <summary>
    /// Type of the field value: String, Double, Int32, Boolean, ...
    /// </summary>
    FieldType Type { get; set; }
    
    /// <summary>
    /// Reference to the Unit table
    /// 
    /// (nullable)
    /// </summary>
    IUnit Unit { get; set; }
    
    /// <summary>
    /// In case this field is identified during the stamping phase,
    /// type of data to associate 0x1:DATA 0x2:DBFIELD
    /// </summary>
    StampingDataType? StampingDataType { get; set; }
    
    /// <summary>
    /// In case the field is drawn from the CNC,
    /// how do you store it
    /// </summary>
    CncDataAggregationType? CncDataAggregationType { get; set; }
    
    /// <summary>
    /// In case this field is a DBFIELD, associated persistent class
    /// </summary>
    string AssociatedClass { get; set; }
    
    /// <summary>
    /// In case this field is a DBFIELD, associated property of the persistent class
    /// </summary>
    string AssociatedProperty { get; set; }
    
    /// <summary>
    /// Minimum time to compute an average or a max that makes sense  (in case the CncDataAggregationType is Average or Max)
    /// </summary>
    TimeSpan? MinTime { get; set; }
    
    /// <summary>
    /// Limit deviation
    /// 
    /// For an average aggregation type, try to keep this maximum standard deviation in the period
    /// 
    /// For a max aggregation type, keep a deviation that is less than this deviation
    /// </summary>
    double? LimitDeviation { get; set; }
    
    /// <summary>
    /// If false, the field can't be deleted
    /// </summary>
    bool Custom { get; }
    
    /// <summary>
    /// Can the field be considered as active ?
    /// </summary>
    bool Active { get; set; }
  }


  /// <summary>
  /// IFieldExtensions
  /// </summary>
  public static class IFieldExtensions
  {
    /// <summary>
    /// Call the ToString () method only if initialized
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public static string ToStringIfInitialized (this IField data)
    {
      if (ModelDAO.ModelDAOHelper.DAOFactory.IsInitialized (data)) {
        return data.ToString ();
      }
      else {
        return $"[{data.GetType ().Name} {data.Id}]";
      }
    }
  }
}
