// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Core.Log;

namespace Lemoine.Model
{
  /// <summary>
  /// List of some base default Ids for MachineMode
  /// </summary>
  public enum MachineModeId
  {
    /// <summary>
    /// Inactive
    /// </summary>
    Inactive = 1,
    /// <summary>
    /// Active
    /// </summary>
    Active = 2,
    /// <summary>
    /// Auto + active
    /// </summary>
    AutoActive = 3,
    /// <summary>
    /// Manual + active
    /// </summary>
    ManualActive = 4,
    /// <summary>
    /// Jog + active
    /// </summary>
    JogActive = 5,
    /// <summary>
    /// MDI + active
    /// </summary>
    MdiActive = 6,
    /// <summary>
    /// Single block + active: deprecated
    /// </summary>
    SingleBlockActive = 7,
    /// <summary>
    /// No data
    /// </summary>
    NoData = 8,
    /// <summary>
    /// Unavailable
    /// </summary>
    Unavailable = 9,
    /// <summary>
    /// Machine in error, either in manual or automatic mode
    /// </summary>
    Error = 10,
    /// <summary>
    /// Off
    /// </summary>
    Off = 11,
    /// <summary>
    /// Feed hold
    /// </summary>
    Hold = 12,
    /// <summary>
    /// The program is ready to be started
    /// </summary>
    Ready = 13,
    /// <summary>
    /// Interruption during a program execution
    /// </summary>
    Interrupted = 14,
    /// <summary>
    /// Rapid traverse during a program execution
    /// </summary>
    AutoRapidTraverse = 15,
    /// <summary>
    /// Rapid traverse
    /// </summary>
    RapidTraverse = 16,
    /// <summary>
    /// Handle / Handwheel + active
    /// </summary>
    HandleActive = 17,
    /// <summary>
    /// Automatic Tool change
    /// </summary>
    AutoToolChange = 18,
    /// <summary>
    /// Automatic Laser check
    /// </summary>
    AutoLaserCheck = 19,
    /// <summary>
    /// Automatic Pallet change
    /// </summary>
    AutoPalletChange = 20,
    /// <summary>
    /// Stop (M0)
    /// </summary>
    M0 = 21,
    /// <summary>
    /// OptionalStop (M1)
    /// </summary>
    M1 = 22,
    /// <summary>
    /// Emergency
    /// </summary>
    Emergency = 23,
    /// <summary>
    /// Probing cycle
    /// </summary>
    AutoProbingCycle = 24,
    /// <summary>
    /// Home positioning during a program execution
    /// </summary>
    AutoHomePositioning = 25,
    /// <summary>
    /// The machine is turned on but inactive
    /// </summary>
    InactiveOn = 26,
    /// <summary>
    /// Auto mode and inactive
    /// </summary>
    AutoInactive = 27,
    /// <summary>
    /// Auto mode and there is no running program. Wait for an operator input
    /// </summary>
    AutoNoRunningProgram = 28,
    /// <summary>
    /// The program was stopped
    /// </summary>
    Stopped = 29,
    /// <summary>
    /// The program has just been completed
    /// </summary>
    Finished = 30,
    /// <summary>
    /// The program execution was reset or the program was canceled
    /// </summary>
    Reset = 31,
    /// <summary>
    /// Auto mode and no program is ready to be started
    /// </summary>
    NotReady = 32,
    /// <summary>
    /// Machine in error during a program execution
    /// </summary>
    AutoError = 33,
    /// <summary>
    /// The error was cleared during a program execution. Waiting for the program to be restarted
    /// </summary>
    AutoErrorCleared = 34,
    /// <summary>
    /// Emergency stop during a program execution
    /// </summary>
    AutoEmergency = 35,
    /// <summary>
    /// The machine is inactive because of a null override (feedrate or rapid traverse rate)
    /// </summary>
    AutoNullOverride = 36,
    /// <summary>
    /// There is no feedrate because the override is 0
    /// </summary>
    AutoNullFeedrateOverride = 37,
    /// <summary>
    /// There is no rapid traverse because the override is 0
    /// </summary>
    AutoNullRapidTraverseOverride = 38,
    /// <summary>
    /// The machine is stopped during the program execution because of some m-codes 
    /// </summary>
    MStop = 39,
    /// <summary>
    /// M60 pallet shuttle + stop detected
    /// </summary>
    M60 = 40,
    /// <summary>
    /// The program is stopped and waits for some operator inputs
    /// </summary>
    MWait = 41,
    /// <summary>
    /// The machine is in manual mode and inactive
    /// </summary>
    ManualInactive = 42,
    /// <summary>
    /// The machine is in single block mode and inactive
    /// </summary>
    SingleBlockInactive = 43,
    /// <summary>
    /// The machine is in MDI mode and inactive
    /// </summary>
    MdiInactive = 44,
    /// <summary>
    /// The machine is in JOG mode and inactive
    /// </summary>
    JogInactive = 45,
    /// <summary>
    /// The machine is in HANDLE mode and inactive
    /// </summary>
    HandleInactive = 46,
    /// <summary>
    /// Axis motion during a program execution (feed or rapid traverse)
    /// </summary>
    AutoMachining = 47,
    /// <summary>
    /// Feed / machining during a program execution
    /// </summary>
    AutoFeed = 48,
    /// <summary>
    /// Auto active operation that is not a feed or a rapid traverse
    /// </summary>
    AutoOtherOperation = 49,
    /// <summary>
    /// Machining operation but the auto/manual status is unknown
    /// </summary>
    Machining = 50,
    /// <summary>
    /// A probing cycle was detected but the auto/manual status is unknown
    /// </summary>
    ProbingCycle = 51,
    /* HomePositioning = 52, deprecated */
    /// <summary>
    /// The active status of the machine is unknown
    /// </summary>
    Unknown = 53,
    /// <summary>
    /// The machine is in manual mode and the active status is unknown
    /// </summary>
    ManualUnknown = 54,
    /// <summary>
    /// Single block mode detected, but the active status is unknown
    /// </summary>
    SingleBlock = 55,
    /// <summary>
    /// MDI mode detected, but the active status is unknown
    /// </summary>
    Mdi = 56,
    /// <summary>
    /// Jog mode detected, but the active status is unknown
    /// </summary>
    Jog = 57,
    /// <summary>
    /// Handle mode detected, but the active status is unknown
    /// </summary>
    Handle = 58,
    /// <summary>
    /// There is an error during the acquisition (includes the connection errors)
    /// </summary>
    AcquisitionError = 59,
    /// <summary>
    /// Manual return to reference
    /// </summary>
    Reference = 60,
    /// <summary>
    /// Dry run execution
    /// </summary>
    DryRun = 61,
    /// <summary>
    /// Machine lock (test mode)
    /// </summary>
    MachineLock = 62,
    /// <summary>
    /// Active auto mode for a normal execution of a program
    /// </summary>
    AutoNormalActive = 63,
    /// <summary>
    /// Active auto mode for tests
    /// </summary>
    AutoTestActive = 64,
    /// <summary>
    /// Missing information to determine the status of the machine
    /// </summary>
    MissingInfo = 65,
    /// <summary>
    /// No feed and no rapid traverse, probably inactive
    /// </summary>
    NoMotion = 66,
    /// <summary>
    /// No feed and no rapid traverse, probably inactive, in auto mode
    /// </summary>
    AutoNoMotion = 67,
    /// <summary>
    /// No feed and no rapid traverse, probably inactive, in manual mode
    /// </summary>
    ManualNoMotion = 68,
    /// <summary>
    /// No feed and no rapid traverse, probably inactive, in single block mode
    /// </summary>
    SingleBlockNoMotion = 69,
    /// <summary>
    /// No feed and no rapid traverse, probably inactive, in MDI mode
    /// </summary>
    MdiNoMotion = 70,
    /// <summary>
    /// No feed and no rapid traverse, probably inactive, in JOG mode
    /// </summary>
    JogNoMotion = 71,
    /// <summary>
    /// No feed and no rapid traverse, probably inactive, in HANDLE mode
    /// </summary>
    HandleNoMotion = 72,
    /// <summary>
    /// The machine is probably off because of some communication errors
    /// </summary>
    ProbablyOff = 73,
    /// <summary>
    /// Auto mode and active status unknown
    /// </summary>
    AutoUnknown = 74,
    /// <summary>
    /// The machine is stopped because of a CNC alarm
    /// </summary>
    AlarmStop = 75,
    /// <summary>
    /// The machine is stopped because of a CNC alarm in AUTO mode
    /// </summary>
    AutoAlarmStop = 76,
  }

  /// <summary>
  /// List of some base default Ids for MachineModeCategory
  /// </summary>
  public enum MachineModeCategoryId
  {
    /// <summary>
    /// Inactive
    /// </summary>
    Inactive = 1,
    /// <summary>
    /// Active
    /// </summary>
    Active = 2,
    /// <summary>
    /// Machine error
    /// </summary>
    Error = 3,
    /// <summary>
    /// Unknown active status
    /// </summary>
    Unknown = 4,
    /// <summary>
    /// Eco
    /// </summary>
    Eco = 5,
    /// <summary>
    /// Stopping
    /// </summary>
    Stopping = 6,
  }
  
  /// <summary>
  /// Model for table MachineMode
  /// 
  /// List of possible Machine Modes (or CNC Modes) (Auto, Jog, Manual, MDI, ...)
  /// </summary>
  public interface IMachineMode: IDisplayable, ISelectionable, IDataWithVersion, ISerializableModel
  {
    // Note: IMachineMode does not inherit from IVersionable and IDataWithTranslation
    //       else the corresponding properties can't be used in a DataGridView binding
    
    /// <summary>
    /// ID
    /// </summary>
    int Id { get; }

    /// <summary>
    /// Name
    /// 
    /// Note an empty string is converted to null.
    /// </summary>
    string Name { get; set; }
    
    /// <summary>
    /// Translation key
    /// 
    /// Note an empty string is converted to null.
    /// </summary>
    string TranslationKey { get; set; }
    
    /// <summary>
    /// Translated name of the object (if applicable, else the name of the object)
    /// </summary>
    string NameOrTranslation { get; }

    /// <summary>
    /// Indicates if the machine is considered running in this mode
    /// </summary>
    bool? Running { get; set; }
    
    /// <summary>
    /// If not null, precise if the machine is running automatically
    /// </summary>
    bool? Auto { get; set; }
    
    /// <summary>
    /// If not null, precise if the machine is running manually
    /// </summary>
    bool? Manual { get; set; }
    
    /// <summary>
    /// Precise whether a sequence should be associated to the activity period
    /// in case it is detected by the CNC
    /// </summary>
    bool AutoSequence { get; set; }
    
    /// <summary>
    /// Color that is associated to the machine mode
    /// </summary>
    string Color { get; set; }
    
    /// <summary>
    /// Associated machine mode category
    /// </summary>
    MachineModeCategoryId MachineModeCategory { get; set; }
    
    /// <summary>
    /// Parent machine mode (nullable)
    /// </summary>
    IMachineMode Parent { get; set; }

    /// <summary>
    /// Machine cost associated with this machine mode (nullable)
    /// </summary>
    double? MachineCost { get; set; }
    
    /// <summary>
    /// Check if it is the descendant of the specified machine mode
    /// </summary>
    /// <param name="ancestor">if null, false is returned</param>
    /// <returns></returns>
    bool IsDescendantOrSelfOf (IMachineMode ancestor);
    
    /// <summary>
    /// Get a common ancestor
    /// </summary>
    /// <param name="other">null is returned if no common ancestor could be found</param>
    /// <returns></returns>
    IMachineMode GetCommonAncestor (IMachineMode other);
  }

  /// <summary>
  /// IMachineModeExtensions
  /// </summary>
  public static class IMachineModeExtensions
  {
    static readonly ILog log = LogManager.GetLogger (typeof (IMachineModeExtensions).FullName);

    /// <summary>
    /// Call the ToString () method only if initialized
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public static string ToStringIfInitialized (this IMachineMode data)
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
