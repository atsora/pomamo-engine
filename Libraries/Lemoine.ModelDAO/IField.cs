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
    /// Feed rate field
    /// </summary>
    Feedrate = 100,
    /// <summary>
    /// Spindle speed field
    /// </summary>
    SpindleSpeed = 101,
    /// <summary>
    /// Spindle load field
    /// </summary>
    SpindleLoad = 102,
    /// <summary>
    /// Feed rate override field
    /// </summary>
    FeedrateOverride = 103,
    /// <summary>
    /// Spindle speed override field
    /// </summary>
    SpindleSpeedOverride = 104,
    /// <summary>
    /// Rapid traverse override field
    /// </summary>
    RapidTraverseOverride = 105,

    /// <summary>
    /// Program name field
    /// </summary>
    ProgramName = 109,

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

    /// <summary>
    /// Warm up field
    /// </summary>
    WarmUp = 135,


    /// <summary>
    /// Effective override (rapid or feed rate)
    /// </summary>
    EffectiveOverride = 142,
    /// <summary>
    /// Effective feed rate override
    /// </summary>
    EffectiveFeedrateOverride = 143,
    /// <summary>
    /// Effective spindle speed override
    /// </summary>
    EffectiveSpindleSpeedOverride = 144,
    /// <summary>
    /// Effective rapid traverse override
    /// </summary>
    EffectiveRapidTraverseOverride = 145,
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
  ///
  /// The column is an integer: some new aggregation types may be added without any migration.
  /// Their behavior in table cncdataperiod is given by <see cref="CncDataAggregationTypeExtensions.GetPeriodAggregationKind(CncDataAggregationType?)"/>:
  /// a new type that is not known there is <see cref="CncDataPeriodAggregationKind.Custom"/>,
  /// to be processed by an extension.
  /// Recommendation: values from 100 for the types that are specific to a plugin
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
    /// <summary>
    /// The value is already a cumulated value (energy index, pulse counter...):
    /// a cumulated total is recorded at a regular interval (cncdataperiod only), resets and rollovers are corrected,
    /// see <see cref="CncDataPeriodAggregationKind.Counter"/>
    /// </summary>
    Counter = 5,
    /// <summary>
    /// The value is an instantaneous value (power, current, feedrate...) that is integrated over time:
    /// a cumulated total (energy, distance) is recorded at a regular interval (cncdataperiod only),
    /// see <see cref="CncDataPeriodAggregationKind.Integral"/>
    /// </summary>
    Integral = 6,
  }

  /// <summary>
  /// Table where the cnc data of a field are stored
  ///
  /// A single table at a time (no double storage): the historical data may be in a table and the new ones in another one,
  /// without any overlap, so that the view cncvalueall can make the union of all the tables
  /// </summary>
  public enum CncDataStorage
  {
    /// <summary>
    /// Not stored
    /// </summary>
    None = 0,
    /// <summary>
    /// Table cncvalue
    /// </summary>
    CncValue = 1,
    /// <summary>
    /// Table cncdataperiod
    /// </summary>
    CncDataPeriod = 2,
  }

  /// <summary>
  /// Kind of the periods of a field in table cncdataperiod
  /// </summary>
  public enum CncDataPeriodAggregationKind
  {
    /// <summary>
    /// No period
    /// </summary>
    None = 0,
    /// <summary>
    /// A new period each time the value changes (StringValue, JsonValue or LastValue)
    /// </summary>
    Change = 1,
    /// <summary>
    /// Combinable statistics (Count, Average, Min, Max, StdDev, LastValue),
    /// a new period after MinTime if the average differs from the previous period of more than LimitDeviation
    /// </summary>
    Statistics = 2,
    /// <summary>
    /// Aggregation type without any built-in behavior: to be processed by an extension
    /// </summary>
    Custom = 3,
    /// <summary>
    /// Cumulated total from a value that is already cumulated (energy index, pulse counter):
    /// total = (value + offset) * Factor, a decrease of the value being a reset (or a rollover, parameter Rollover).
    /// Periods as <see cref="Integral"/>
    /// </summary>
    Counter = 4,
    /// <summary>
    /// Cumulated total from an instantaneous value, integrated over time:
    /// total += value * Factor * duration / TimeUnit (trapezoidal rule, or the average of a batch over its duration),
    /// no increase during a gap or a stop.
    ///
    /// Periods: one every MinTime (default 30 s), with bounds aligned on the clock (multiples of MinTime since 00:00 UTC),
    /// at the week bounds and at the cuts. Min = total at the begin, Max = LastValue = total at the end
    /// (the total is interpolated at a bound), Count = number of values, Average = average rate (Integral only),
    /// JsonValue = state to continue after a restart
    /// </summary>
    Integral = 5,
  }

  /// <summary>
  /// Extensions to <see cref="CncDataAggregationType"/>
  /// </summary>
  public static class CncDataAggregationTypeExtensions
  {
    /// <summary>
    /// Kind of the periods in table cncdataperiod for an aggregation type
    /// <item>None (or null): no period</item>
    /// <item>NewValue: a new period each time the value changes</item>
    /// <item>Average, Sum, Max: statistics (all of them are stored, Sum = Average * Count)</item>
    /// <item>other: custom, to be processed by an extension</item>
    /// </summary>
    /// <param name="aggregationType"></param>
    /// <returns></returns>
    public static CncDataPeriodAggregationKind GetPeriodAggregationKind (this CncDataAggregationType? aggregationType)
    {
      if (!aggregationType.HasValue) {
        return CncDataPeriodAggregationKind.None;
      }
      switch (aggregationType.Value) {
      case CncDataAggregationType.None:
        return CncDataPeriodAggregationKind.None;
      case CncDataAggregationType.NewValue:
        return CncDataPeriodAggregationKind.Change;
      case CncDataAggregationType.Average:
      case CncDataAggregationType.Sum:
      case CncDataAggregationType.Max:
        return CncDataPeriodAggregationKind.Statistics;
      case CncDataAggregationType.Counter:
        return CncDataPeriodAggregationKind.Counter;
      case CncDataAggregationType.Integral:
        return CncDataPeriodAggregationKind.Integral;
      default:
        return CncDataPeriodAggregationKind.Custom;
      }
    }

    /// <summary>
    /// <see cref="GetPeriodAggregationKind(CncDataAggregationType?)"/>
    /// </summary>
    /// <param name="aggregationType"></param>
    /// <returns></returns>
    public static CncDataPeriodAggregationKind GetPeriodAggregationKind (this CncDataAggregationType aggregationType) =>
      ((CncDataAggregationType?)aggregationType).GetPeriodAggregationKind ();
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
    /// Table where the cnc data are stored (a single one).
    /// null: config Cnc.DefaultDataStorage, see <see cref="IFieldExtensions.GetCncDataStorage(IField)"/>
    /// </summary>
    CncDataStorage? CncDataStorage { get; set; }

    /// <summary>
    /// Maximum duration of a period in table cncdataperiod (any aggregation).
    /// null: no limit, but a period never crosses a week bound
    /// </summary>
    TimeSpan? PeriodMaxDuration { get; set; }

    /// <summary>
    /// Parameters of the cnc data of the field, as a json object (column fieldcncdataparameters), null if none.
    /// Read them with <see cref="IFieldExtensions.GetCncDataParameter{T}(IField, string, T)"/>,
    /// see the known names in <see cref="CncDataParameterNames"/>
    /// </summary>
    string CncDataParameters { get; set; }

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

    /// <summary>
    /// Table where the cnc data of a field are stored:
    /// <see cref="IField.CncDataStorage"/>, else the config Cnc.DefaultDataStorage (default: CncValue).
    /// An invalid value of the config is ignored
    /// </summary>
    /// <param name="field">not null</param>
    /// <returns></returns>
    public static CncDataStorage GetCncDataStorage (this IField field)
    {
      if (field.CncDataStorage.HasValue) {
        return field.CncDataStorage.Value;
      }
      var storage = Lemoine.Info.ConfigSet.LoadAndGet (ConfigKeys.GetCncConfigKey (CncConfigKey.DefaultDataStorage), Model.CncDataStorage.CncValue);
      if (!IsValid (storage)) {
        Lemoine.Core.Log.LogManager.GetLogger (typeof (IFieldExtensions).FullName)
          .Error ($"GetCncDataStorage: invalid config {ConfigKeys.GetCncConfigKey (CncConfigKey.DefaultDataStorage)}={storage} (a single table is expected) => use CncValue");
        return Model.CncDataStorage.CncValue;
      }
      return storage;
    }

    /// <summary>
    /// Is a storage valid: a single table (or none)?
    /// </summary>
    /// <param name="storage"></param>
    /// <returns></returns>
    public static bool IsValid (CncDataStorage storage) =>
      (storage == Model.CncDataStorage.None) || (storage == Model.CncDataStorage.CncValue) || (storage == Model.CncDataStorage.CncDataPeriod);

    /// <summary>
    /// Kind of the periods of a field in table cncdataperiod,
    /// see <see cref="CncDataAggregationTypeExtensions.GetPeriodAggregationKind(CncDataAggregationType?)"/>
    /// </summary>
    /// <param name="field">not null</param>
    /// <returns></returns>
    public static CncDataPeriodAggregationKind GetPeriodAggregationKind (this IField field) =>
      field.CncDataAggregationType.GetPeriodAggregationKind ();

    // Parsed parameters by json text (the same text is shared by the instances of a field)
    static readonly System.Collections.Concurrent.ConcurrentDictionary<string, System.Text.Json.JsonElement> s_parsedParameters
      = new System.Collections.Concurrent.ConcurrentDictionary<string, System.Text.Json.JsonElement> ();

    static readonly System.Text.Json.JsonSerializerOptions s_parameterOptions = new System.Text.Json.JsonSerializerOptions {
      PropertyNameCaseInsensitive = true,
    };

    /// <summary>
    /// Parse some cnc data parameters, that must be a json object
    /// </summary>
    /// <param name="parameters">not null</param>
    /// <returns>root element (an object)</returns>
    /// <exception cref="ArgumentException">not a json object</exception>
    public static System.Text.Json.JsonElement ParseCncDataParameters (string parameters)
    {
      return s_parsedParameters.GetOrAdd (parameters, p => {
        try {
          using (var document = System.Text.Json.JsonDocument.Parse (p)) {
            if (document.RootElement.ValueKind != System.Text.Json.JsonValueKind.Object) {
              throw new ArgumentException ($"The cnc data parameters must be a json object: {p}", nameof (parameters));
            }
            return document.RootElement.Clone ();
          }
        }
        catch (System.Text.Json.JsonException ex) {
          throw new ArgumentException ($"Invalid json in the cnc data parameters: {p}", nameof (parameters), ex);
        }
      });
    }

    /// <summary>
    /// Get a cnc data parameter of a field (case insensitive name), or a default value if it is not defined or invalid
    /// </summary>
    /// <typeparam name="T">for example double, int, string, TimeSpan (format "hh:mm:ss"), or a class</typeparam>
    /// <param name="field">not null</param>
    /// <param name="name">parameter name, see <see cref="CncDataParameterNames"/></param>
    /// <param name="defaultValue"></param>
    /// <returns></returns>
    public static T GetCncDataParameter<T> (this IField field, string name, T defaultValue = default)
    {
      return TryGetCncDataParameter<T> (field, name, out var value) ? value : defaultValue;
    }

    /// <summary>
    /// Try to get a cnc data parameter of a field (case insensitive name)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="field">not null</param>
    /// <param name="name"></param>
    /// <param name="value"></param>
    /// <returns>the parameter is defined and valid</returns>
    public static bool TryGetCncDataParameter<T> (this IField field, string name, out T value)
    {
      value = default;
      var parameters = field.CncDataParameters;
      if (string.IsNullOrEmpty (parameters)) {
        return false;
      }
      try {
        var root = ParseCncDataParameters (parameters);
        foreach (var property in root.EnumerateObject ()) {
          if (string.Equals (property.Name, name, StringComparison.OrdinalIgnoreCase)) {
            if (property.Value.ValueKind == System.Text.Json.JsonValueKind.Null) {
              return false;
            }
            value = System.Text.Json.JsonSerializer.Deserialize<T> (property.Value.GetRawText (), s_parameterOptions);
            return true;
          }
        }
        return false;
      }
      catch (Exception ex) {
        Lemoine.Core.Log.LogManager.GetLogger (typeof (IFieldExtensions).FullName)
          .Error ($"TryGetCncDataParameter: invalid parameter {name} of field {field.Code} in {parameters}", ex);
        return false;
      }
    }

    /// <summary>
    /// .NET type of the json values of a field (parameter <see cref="CncDataParameterNames.JsonType"/>), null if none or unknown
    /// </summary>
    /// <param name="field">not null</param>
    /// <returns></returns>
    public static Type GetJsonType (this IField field)
    {
      var typeName = field.GetCncDataParameter<string> (CncDataParameterNames.JsonType);
      if (string.IsNullOrEmpty (typeName)) {
        return null;
      }
      var type = Type.GetType (typeName);
      if (null == type) {
        Lemoine.Core.Log.LogManager.GetLogger (typeof (IFieldExtensions).FullName)
          .Error ($"GetJsonType: unknown type {typeName} of field {field.Code}");
      }
      return type;
    }

    /// <summary>
    /// Deserialize a json value of a field: in the type of the parameter <see cref="CncDataParameterNames.JsonType"/> if defined,
    /// else as a <see cref="System.Text.Json.JsonElement"/>
    /// </summary>
    /// <param name="field">not null</param>
    /// <param name="json">nullable</param>
    /// <returns>null if json is null</returns>
    public static object DeserializeJsonValue (this IField field, string json)
    {
      if (null == json) {
        return null;
      }
      var type = field.GetJsonType ();
      if (null != type) {
        return System.Text.Json.JsonSerializer.Deserialize (json, type, s_parameterOptions);
      }
      using (var document = System.Text.Json.JsonDocument.Parse (json)) {
        return document.RootElement.Clone ();
      }
    }
  }

  /// <summary>
  /// Names of the known cnc data parameters of a field (<see cref="IField.CncDataParameters"/>)
  /// </summary>
  public static class CncDataParameterNames
  {
    /// <summary>
    /// .NET type (assembly qualified name) to deserialize the json values of the field
    /// </summary>
    public static readonly string JsonType = "JsonType";

    /// <summary>
    /// Maximum duration without any value after which a new period (or cnc value) starts, as "hh:mm:ss".
    /// Default: config Cnc.DataImport.MaxCncValueGap (1 minute). For example longer for a slow source (LoRaWAN sensor)
    /// </summary>
    public static readonly string MaxGap = "MaxGap";

    /// <summary>
    /// Counter, Integral: multiplicative factor of the value (for example Wh per pulse, or a unit conversion). Default: 1
    /// </summary>
    public static readonly string Factor = "Factor";

    /// <summary>
    /// Counter: value after which the counter restarts from 0 (for example 65536 for a 16 bit counter).
    /// Default: a decrease is a reset, the counter restarts from 0
    /// </summary>
    public static readonly string Rollover = "Rollover";

    /// <summary>
    /// Integral: duration unit of the total, as "hh:mm:ss". Default: 01:00:00 (for example kW => kWh).
    /// 00:01:00 for a feedrate in mm/min => mm
    /// </summary>
    public static readonly string TimeUnit = "TimeUnit";

    /// <summary>
    /// Integral of a current: voltage (V). The factor becomes Voltage * PowerFactor (* √3 if ThreePhase) * Factor / 1000 (A => kW)
    /// </summary>
    public static readonly string Voltage = "Voltage";

    /// <summary>
    /// Integral of a current: power factor (cos φ). Default: 1
    /// </summary>
    public static readonly string PowerFactor = "PowerFactor";

    /// <summary>
    /// Integral of a current: three-phase (factor √3). Default: false
    /// </summary>
    public static readonly string ThreePhase = "ThreePhase";
  }
}
