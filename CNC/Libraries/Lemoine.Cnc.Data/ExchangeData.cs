// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using Lemoine.Core.Log;

namespace Lemoine.Cnc.Data
{
  /// <summary>
  /// Command to use in the ExchangeData
  /// </summary>
  public enum ExchangeDataCommand
  {
    /// <summary>
    /// Set a new machine mode
    /// </summary>
    MachineMode = 0,
    /// <summary>
    /// Add a CncValue
    /// </summary>
    CncValue = 1,
    /// <summary>
    /// Got a new stamp to process
    /// </summary>
    Stamp = 2,
    /// <summary>
    /// Other kind of action, like StartCycle, StopCycle, StopIsoFile
    /// </summary>
    Action = 3,
    /// <summary>
    /// Stop recording a CNC value (because it is not valid)
    /// </summary>
    StopCncValue = 5,
    /// <summary>
    /// Cnc alarm
    /// </summary>
    CncAlarm = 7,
    /// <summary>
    /// Neutral detection time stamp just to tell the system is still active
    /// </summary>
    DetectionTimeStamp = 8,
    /// <summary>
    /// Machine module activity (machine mode that is associated to a specific machine module)
    /// </summary>
    MachineModuleActivity = 9,
    /// <summary>
    /// Cnc variable set
    /// </summary>
    CncVariableSet = 10,
    /// <summary>
    /// Sequence milestone (in seconds)
    /// </summary>
    SequenceMilestone = 11,
  }
  
  /// <summary>
  /// Exchange data structure that is created by the Lem_CncService
  /// and read by the Lem_CncDataService
  /// </summary>
  [Serializable]
  public class ExchangeData : ISerializable
  {
    /// <summary>
    /// Key value to stop an Iso File
    /// in case the command is Action
    /// </summary>
    public static readonly string ACTION_STOP_ISO_FILE = "StopIsoFile";
    /// <summary>
    /// Key value to start a cycle
    /// in case the command is Action
    /// </summary>
    public static readonly string ACTION_START_CYCLE = "StartCycle";
    /// <summary>
    /// Key value to stop a cycle
    /// in case the command is Action
    /// </summary>
    public static readonly string ACTION_STOP_CYCLE = "StopCycle";
    /// <summary>
    /// Key value to stop a cycle with a given quantity
    /// in case the command is Action
    /// </summary>
    public static readonly string ACTION_QUANTITY = "Quantity";
    /// <summary>
    /// Key value to stop a cycle with a given operation code and quantity
    /// in case the command is Action
    /// </summary>
    public static readonly string ACTION_OPERATIONCODE_QUANTITY = "OperationCodeQuantity";
    /// <summary>
    /// For the command MachineMode, the given value corresponds to the Machine Mode Id
    /// </summary>
    public static readonly string MACHINE_MODE_ID = "Id";
    /// <summary>
    /// For the command MachineMode, the given value corresponds to the Translation Key or Name
    /// </summary>
    public static readonly string MACHINE_MODE_TRANSLATION_KEY_OR_NAME = "TranslationKeyOrName";
    
    #region Members
    DateTime m_dateTime = DateTime.UtcNow;
    #endregion // Members

    #region Helper Method
    static DateTime TruncateToSeconds(DateTime dt)
    {
      return new DateTime(dt.Ticks - (dt.Ticks % TimeSpan.TicksPerSecond), dt.Kind);
    }
    #endregion // Helper Method
    
    static readonly ILog log = LogManager.GetLogger(typeof (ExchangeData).FullName);

    #region Getters / Setters
    /// <summary>
    /// Machine Id
    /// </summary>
    public int MachineId { get; set; }
    
    /// <summary>
    /// Machine module Id
    /// </summary>
    public int MachineModuleId { get; set; }
    
    /// <summary>
    /// Date/time of the data
    /// </summary>
    public DateTime DateTime {
      get { return TruncateToSeconds(m_dateTime); }
      set { m_dateTime = value; }
    }
    
    /// <summary>
    /// Command
    /// </summary>
    public ExchangeDataCommand Command { get; set; }
    
    /// <summary>
    /// Key
    /// 
    /// This can be:
    /// <item>a Field Code in case the command is CncValue</item>
    /// <item>Id, TranslationKey or Name in case the command is MachineMode</item>
    /// <item>StartCycle, StopCycle, StopIsoFile in case the command is Action</item>
    /// </summary>
    public string Key { get; set; }
    
    /// <summary>
    /// Value
    /// </summary>
    public object Value { get; set; }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Private constructor
    /// </summary>
    ExchangeData () {}
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// ToString method
    /// </summary>
    /// <returns></returns>
    public override string ToString ()
    {
      return string.Format ("[ExchangeData MachineModuleId={0} Command={1} Key={2} Value={3} Date={4}]",
                            MachineModuleId, Command, Key, Value, DateTime);
    }
    
    /// <summary>
    ///   Determines whether the specified Object
    ///   is equal to the current Object
    /// </summary>
    /// <param name="obj">The object to compare with the current object</param>
    /// <returns>true if the specified Object is equal to the current Object; otherwise, false</returns>
    public override bool Equals(object obj)
    {
      if (object.ReferenceEquals(this,obj)) {
        return true;
      }

      if (obj == null) {
        return false;
      }

      var other = obj as ExchangeData;
      if (null == other) {
        return false;
      }
      return object.Equals (other.Command, this.Command)
        && object.Equals (other.DateTime, this.DateTime)
        && object.Equals (other.MachineId, this.MachineId)
        && object.Equals (other.MachineModuleId, this.MachineModuleId)
        && object.Equals (other.Key, this.Key)
        && object.Equals (other.Value, this.Value);
    }
    
    /// <summary>
    ///   Serves as a hash function for a particular type
    /// </summary>
    /// <returns>A hash code for the current Object</returns>
    public override int GetHashCode()
    {
      int hashCode = 0;
      unchecked {
        hashCode += 1000000007 * MachineModuleId.GetHashCode();
        hashCode += 1000000011 * Command.GetHashCode ();
        hashCode += 1000000013 * DateTime.GetHashCode ();
        hashCode += 1000000015 * Key.GetHashCode ();
        if (Value != null) {
          hashCode += 1000000017 * Value.GetHashCode ();
        }
      }
      return hashCode;
    }
    
    /// <summary>
    /// Generic builder for ExchangeData
    /// </summary>
    /// <param name="machineId"></param>
    /// <param name="machineModuleId"></param>
    /// <param name="dateTime"></param>
    /// <param name="command"></param>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public static ExchangeData BuildExchangeData(int machineId,
                                                 int machineModuleId,
                                                 DateTime dateTime,
                                                 ExchangeDataCommand command,
                                                 string key,
                                                 object value)
    {
      var data = new ExchangeData();
      data.MachineId = machineId;
      if (0 != machineModuleId) {
        data.MachineModuleId = machineModuleId;
      }
      data.DateTime = dateTime;
      data.Command = command;
      data.Key = key;
      data.Value = value;
      return data;
    }
    
    /// <summary>
    /// Build an exchange data corresponding to a start of cyle
    /// </summary>
    /// <param name="machineId"></param>
    /// <param name="machineModuleId"></param>
    /// <param name="dateTime"></param>
    /// <returns></returns>
    public static ExchangeData BuildStartCycleExchangeData(int machineId,
                                                           int machineModuleId,
                                                           DateTime dateTime)
    {
      return BuildExchangeData(machineId, machineModuleId, dateTime,
                               ExchangeDataCommand.Action,
                               ExchangeData.ACTION_START_CYCLE,
                               null);
    }
    
    /// <summary>
    /// Build an exchange data corresponding to a start of cyle
    /// </summary>
    /// <param name="machineId"></param>
    /// <param name="machineModuleId"></param>
    /// <param name="dateTime"></param>
    /// <param name="operationCode"></param>
    /// <returns></returns>
    public static ExchangeData
      BuildStartCycleExchangeDataWithOperationCode(int machineId,
                                                   int machineModuleId,
                                                   DateTime dateTime,
                                                   string operationCode)
    {
      return BuildExchangeData(machineId, machineModuleId, dateTime,
                               ExchangeDataCommand.Action,
                               ExchangeData.ACTION_START_CYCLE,
                               operationCode);
    }
    
    /// <summary>
    /// Build an exchange data corresponding to an end of cyle
    /// </summary>
    /// <param name="machineId"></param>
    /// <param name="machineModuleId"></param>
    /// <param name="dateTime"></param>
    /// <returns></returns>
    public static ExchangeData
      BuildStopCycleExchangeData(int machineId,
                                 int machineModuleId,
                                 DateTime dateTime)
    {
      return BuildExchangeData(machineId, machineModuleId, dateTime,
                               ExchangeDataCommand.Action,
                               ExchangeData.ACTION_STOP_CYCLE,
                               null);
    }

    /// <summary>
    /// Build an exchange data corresponding to an end of cyle
    /// </summary>
    /// <param name="machineId"></param>
    /// <param name="machineModuleId"></param>
    /// <param name="dateTime"></param>
    /// <param name="operationCode"></param>
    /// <returns></returns>
    public static ExchangeData
      BuildStopCycleExchangeDataWithOperationCode(int machineId,
                                                  int machineModuleId,
                                                  DateTime dateTime,
                                                  string operationCode)
    {
      return BuildExchangeData(machineId, machineModuleId, dateTime,
                               ExchangeDataCommand.Action,
                               ExchangeData.ACTION_STOP_CYCLE,
                               operationCode);
    }

    /// <summary>
    /// Build an exchange data corresponding to an end of cyle with a quantity
    /// </summary>
    /// <param name="machineId"></param>
    /// <param name="machineModuleId"></param>
    /// <param name="dateTime"></param>
    /// <param name="operationCodeQuantity"></param>
    /// <returns></returns>
    public static ExchangeData
      BuildStopCycleExchangeDataWithOperationCodeQuantity (int machineId,
                                                           int machineModuleId,
                                                           DateTime dateTime,
                                                           int operationCodeQuantity)
    {
      return BuildExchangeData(machineId, machineModuleId, dateTime,
                               ExchangeDataCommand.Action,
                               ExchangeData.ACTION_OPERATIONCODE_QUANTITY,
                               operationCodeQuantity);
    }

    /// <summary>
    /// Build an exchange data corresponding to an end of cyle with a quantity
    /// </summary>
    /// <param name="machineId"></param>
    /// <param name="machineModuleId"></param>
    /// <param name="dateTime"></param>
    /// <param name="operationCodeQuantity"></param>
    /// <returns></returns>
    public static ExchangeData
      BuildStopCycleExchangeDataWithOperationCodeQuantity (int machineId,
                                                           int machineModuleId,
                                                           DateTime dateTime,
                                                           string operationCodeQuantity)
    {
      return BuildExchangeData(machineId, machineModuleId, dateTime,
                               ExchangeDataCommand.Action,
                               ExchangeData.ACTION_OPERATIONCODE_QUANTITY,
                               operationCodeQuantity);
    }

    /// <summary>
    /// Build an exchange data corresponding to an end of cyle with a quantity
    /// </summary>
    /// <param name="machineId"></param>
    /// <param name="machineModuleId"></param>
    /// <param name="dateTime"></param>
    /// <param name="quantity"></param>
    /// <returns></returns>
    public static ExchangeData
      BuildQuantityExchangeData(int machineId,
                                int machineModuleId,
                                DateTime dateTime,
                                int quantity)
    {
      return BuildExchangeData(machineId, machineModuleId, dateTime,
                               ExchangeDataCommand.Action,
                               ExchangeData.ACTION_QUANTITY,
                               quantity);
    }
    
    /// <summary>
    /// Build an exchange data corresponding to a detection time stamp
    /// </summary>
    /// <param name="machineId"></param>
    /// <param name="machineModuleId"></param>
    /// <param name="dateTime"></param>
    /// <returns></returns>
    public static ExchangeData
      BuildDetectionTimeStampExchangeData(int machineId,
                                          int machineModuleId,
                                          DateTime dateTime)
    {
      return BuildExchangeData(machineId, machineModuleId, dateTime,
                               ExchangeDataCommand.DetectionTimeStamp,
                               null,
                               null);
    }
    
    /// <summary>
    /// Build an exchange data corresponding to a stamp
    /// </summary>
    /// <param name="machineId"></param>
    /// <param name="machineModuleId"></param>
    /// <param name="dateTime"></param>
    /// <param name="stampId"></param>
    /// <param name="key"></param>
    /// <returns></returns>
    public static ExchangeData
      BuildStampExchangeData(int machineId,
                             int machineModuleId,
                             DateTime dateTime,
                             string key,
                             double stampId)
    {
      if (0 == stampId) {
        return BuildExchangeData(machineId, machineModuleId, dateTime,
                                 ExchangeDataCommand.Action,
                                 ExchangeData.ACTION_STOP_ISO_FILE,
                                 null);
      }
      else if (stampId < 0) {
        log.DebugFormat ("BuildStampExchangeData: " +
                         "negative stamp value {0}, turn into a positive value",
                         stampId);
        return BuildExchangeData(machineId, machineModuleId, dateTime,
                                 ExchangeDataCommand.Stamp,
                                 key,
                                 -stampId);
      }
      else { // 0 < stampId
        Debug.Assert (0 < stampId);
        return BuildExchangeData(machineId, machineModuleId, dateTime,
                                 ExchangeDataCommand.Stamp,
                                 key,
                                 stampId);
      }
    }

    /// <summary>
    /// Build an exchange data corresponding to a sequence milestone (in seconds)
    /// </summary>
    /// <param name="machineId"></param>
    /// <param name="machineModuleId"></param>
    /// <param name="dateTime"></param>
    /// <param name="key"></param>
    /// <param name="sequenceMilestone"></param>
    /// <returns></returns>
    public static ExchangeData
      BuildSequenceMilestoneExchangeData (int machineId, int machineModuleId, DateTime dateTime, string key, TimeSpan sequenceMilestone)
    {
      return BuildExchangeData (machineId, machineModuleId, dateTime, ExchangeDataCommand.SequenceMilestone, key, sequenceMilestone.TotalSeconds);
    }
    
    /// <summary>
    /// Build an exchange data corresponding to a CNC value
    /// </summary>
    /// <param name="machineId"></param>
    /// <param name="machineModuleId"></param>
    /// <param name="dateTime"></param>
    /// <param name="key"></param>
    /// <param name="v"></param>
    /// <returns></returns>
    public static ExchangeData
      BuildCncValueExchangeData(int machineId,
                                int machineModuleId,
                                DateTime dateTime,
                                string key,
                                object v)
    {
      return BuildExchangeData(machineId, machineModuleId, dateTime,
                               ExchangeDataCommand.CncValue,
                               key, v);
    }
    
    /// <summary>
    /// Build an exchange data corresponding to stop of CNC value
    /// </summary>
    /// <param name="machineId"></param>
    /// <param name="machineModuleId"></param>
    /// <param name="dateTime"></param>
    /// <param name="key"></param>
    /// <returns></returns>
    public static ExchangeData
      BuildStopCncValueExchangeData(int machineId,
                                    int machineModuleId,
                                    DateTime dateTime,
                                    string key)
    {
      return BuildExchangeData(machineId, machineModuleId, dateTime,
                               ExchangeDataCommand.StopCncValue,
                               key, null);
    }

    /// <summary>
    /// Build an exchange data corresponding to a CNC variable
    /// </summary>
    /// <param name="machineId"></param>
    /// <param name="machineModuleId"></param>
    /// <param name="dateTime"></param>
    /// <param name="v"></param>
    /// <returns></returns>
    public static ExchangeData
      BuildCncVariableSetExchangeData (int machineId,
                                int machineModuleId,
                                DateTime dateTime,
                                object v)
    {
      return BuildExchangeData (machineId, machineModuleId, dateTime,
                               ExchangeDataCommand.CncVariableSet,
                               "", v);
    }

    /// <summary>
    /// Build an exchange data corresponding to a CncAlarm
    /// </summary>
    /// <param name="machineId"></param>
    /// <param name="machineModuleId"></param>
    /// <param name="dateTime"></param>
    /// <param name="v"></param>
    /// <returns></returns>
    public static ExchangeData BuildCncAlarmExchangeData(int machineId, int machineModuleId,
                                                         DateTime dateTime, object v)
    {
      return BuildExchangeData(machineId, machineModuleId, dateTime, ExchangeDataCommand.CncAlarm, "", v);
    }
    
    /// <summary>
    /// Build an exchange data corresponding to a MachineModeId
    /// </summary>
    /// <param name="machineId"></param>
    /// <param name="machineModuleId"></param>
    /// <param name="dateTime"></param>
    /// <param name="v"></param>
    /// <returns></returns>
    public static ExchangeData BuildMachineModeIDExchangeData(int machineId,
                                                              int machineModuleId,
                                                              DateTime dateTime, object v)
    {
      return BuildExchangeData(machineId, machineModuleId, dateTime,
                               ExchangeDataCommand.MachineMode,
                               ExchangeData.MACHINE_MODE_ID, v);
    }
    
    
    /// <summary>
    /// Build an exchange data corresponding to a MachineMode translation key or name
    /// </summary>
    /// <param name="machineId"></param>
    /// <param name="machineModuleId"></param>
    /// <param name="dateTime"></param>
    /// <param name="v"></param>
    /// <returns></returns>
    public static ExchangeData
      BuildMachineModeTranslationKeyOrNameExchangeData(int machineId,
                                                       int machineModuleId,
                                                       DateTime dateTime,
                                                       object v)
    {
      return BuildExchangeData(machineId, machineModuleId, dateTime,
                               ExchangeDataCommand.MachineMode,
                               ExchangeData.MACHINE_MODE_TRANSLATION_KEY_OR_NAME, v);
    }

    /// <summary>
    /// Build an exchange data corresponding to a MachineModuleActivity / MachineModeId
    /// </summary>
    /// <param name="machineId"></param>
    /// <param name="machineModuleId"></param>
    /// <param name="dateTime"></param>
    /// <param name="v"></param>
    /// <returns></returns>
    public static ExchangeData
      BuildMachineModuleActivityIDExchangeData(int machineId,
                                               int machineModuleId,
                                               DateTime dateTime,
                                               object v)
    {
      return BuildExchangeData(machineId, machineModuleId, dateTime,
                               ExchangeDataCommand.MachineModuleActivity,
                               ExchangeData.MACHINE_MODE_ID, v);
    }
    
    
    /// <summary>
    /// Build an exchange data corresponding to a MachineModuleActivity / MachineMode translation key or name
    /// </summary>
    /// <param name="machineId"></param>
    /// <param name="machineModuleId"></param>
    /// <param name="dateTime"></param>
    /// <param name="v"></param>
    /// <returns></returns>
    public static ExchangeData
      BuildMachineModuleActivityTranslationKeyOrNameExchangeData(int machineId,
                                                                 int machineModuleId,
                                                                 DateTime dateTime,
                                                                 object v)
    {
      return BuildExchangeData(machineId, machineModuleId, dateTime,
                               ExchangeDataCommand.MachineModuleActivity,
                               ExchangeData.MACHINE_MODE_TRANSLATION_KEY_OR_NAME, v);
    }
    #endregion // Methods
    
    
    #region ISerializable interface
    /// <summary>
    /// GetBytesFromHexString (revert of BitConverter.ToString)
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    static byte[] GetBytesFromHexString(string str) {
      string[] splittedStr = str.Split('-');
      byte[] bytes = new byte[splittedStr.Length];
      int index = 0;
      foreach(string b in splittedStr) {
        bytes[index] = Convert.ToByte(b, 16);
        index++;
      }
      return bytes;
      // requires System.Linq
      // return str.Split('-').Select(b => Convert.ToByte(b, 16)).ToArray();
    }

    /// <summary>
    /// GetObjectData:  ISerializable interface
    /// </summary>
    /// <param name="info"></param>
    /// <param name="context"></param>
    public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      info.AddValue("M", this.MachineId, typeof(int));
      info.AddValue("MM", this.MachineModuleId, typeof(int));
      info.AddValue("DT", this.DateTime, typeof(DateTime));
      info.AddValue("CO", this.Command.ToString(), typeof(ExchangeDataCommand));
      info.AddValue("K", this.Key, typeof(string));
      
      if (this.Value != null) {
        Type valueType = this.Value.GetType();
        
        info.AddValue("T", this.Value.GetType().ToString(), typeof(string));
        
        if ((valueType.IsValueType) || (valueType == typeof(string))) {
          info.AddValue("V", this.Value, valueType);
        }
        else {
          IFormatter formatter = new BinaryFormatter ();
          using (MemoryStream stream = new MemoryStream ())
          {
            formatter.Serialize (stream, this.Value);
            string hex = BitConverter.ToString (stream.ToArray ());
            info.AddValue("V", hex, typeof(string));
          }
        }
      }
    }
    
    /// <summary>
    /// ExchangeData: ISerializable interface
    /// </summary>
    /// <param name="info"></param>
    /// <param name="context"></param>
    protected ExchangeData(SerializationInfo info, StreamingContext context)
    {
      this.MachineId = info.GetInt32("M");
      this.MachineModuleId = info.GetInt32("MM");
      this.DateTime = info.GetDateTime("DT");
      this.Command  = (ExchangeDataCommand) ExchangeDataCommand.Parse(typeof(ExchangeDataCommand), info.GetString("CO"));
      this.Key = info.GetString("K");
      try {
        string valueType = info.GetString("T");
        if (valueType.Equals (typeof (int).ToString ())) {
          this.Value = info.GetInt32("V");
        }
        else if (valueType.Equals (typeof (Int16).ToString ())) {
          this.Value = info.GetInt16("V");
        }
        else if (valueType.Equals (typeof (Int64).ToString ())) {
          this.Value = info.GetInt64("V");
        }
        else if (valueType.Equals (typeof (bool).ToString ())) {
          this.Value = info.GetBoolean("V");
        }
        else if (valueType.Equals (typeof (double).ToString ())) {
          this.Value = info.GetDouble("V");
        }
        else if (valueType.Equals (typeof (string).ToString ())) {
          this.Value = info.GetString("V");
        }
        else {
          BinaryFormatter formatter = new BinaryFormatter ();
          using (MemoryStream stream =
                 new MemoryStream (GetBytesFromHexString(info.GetString("V"))))
          {
            this.Value = formatter.Deserialize (stream);
          }
        }
      }
      catch (Exception) {
        this.Value = null;
      }
    }
    #endregion
  }
}
