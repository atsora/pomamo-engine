// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Lemoine.Model;

namespace Lemoine.Stamping
{
  /// <summary>
  /// Extension to manage events in NC program
  /// </summary>
  public interface IStampingEventHandler
  {
    /// <summary>
    /// Notify a new block starts
    /// </summary>
    /// <param name="edit">Is the file in edit mode (allows updates)?</param>
    /// <param name="level">sub-program level. 0 corresponds to the main program</param>
    void NotifyNewBlock (bool edit, int level);

    /// <summary>
    /// Start a cycle
    /// </summary>
    /// <returns></returns>
    void StartCycle ();

    /// <summary>
    /// Stop a cycle
    /// </summary>
    /// <returns></returns>
    void StopCycle ();

    /// <summary>
    /// Start a sequence
    /// </summary>
    /// <returns></returns>
    void StartSequence (SequenceKind sequenceKind);

    /// <summary>
    /// Set a data
    /// 
    /// Here is a list of keys (not exclusive):
    /// <item>F: feedrate</item>
    /// <item>G-Motion</item>
    /// <item>G-FeedRateMode</item>
    /// <item>k when is an axis, the value is the position</item>
    /// 
    /// Here is a list of specific G-code keys:
    /// <item>#x: variable value</item>
    /// <item>G05P: 0:inactive, 10000: active</item>
    /// <item>G05.1Q: 0: inactive, else active</item>
    /// <item>G08P: 0: inactive, else active</item>
    /// 
    /// Here is a list of specific Heidenhain keys:
    /// <item>Qx: variable value</item>
    /// </summary>
    /// <param name="key"></param>
    /// <param name="v"></param>
    /// <returns></returns>
    void SetData (string key, object v);

    /// <summary>
    /// Set the next tool number
    /// </summary>
    /// <param name="toolNumber"></param>
    void SetNextToolNumber (string toolNumber);

    /// <summary>
    /// Trigger a tool change
    /// </summary>
    /// <param name="toolNumber">optional</param>
    /// <returns></returns>
    void TriggerToolChange (string toolNumber = "");

    /// <summary>
    /// Start a program or a sub-program
    /// </summary>
    /// <param name="edit">Is the file in edit mode (allows updates)?</param>
    /// <param name="level">sub-program level. 0 corresponds to the main program</param>
    void StartProgram (bool edit, int level);

    /// <summary>
    /// End a program or a sub-program (the end of the program is reached)
    /// </summary>
    /// <param name="edit">Is the file in edit mode (allows updates)?</param>
    /// <param name="level">sub-program level. 0 corresponds to the main program</param>
    /// <param name="endOfFile">the end of the file is reached</param>
    void EndProgram (bool edit, int level, bool endOfFile);

    /// <summary>
    /// Resume the program that was previously hold
    /// </summary>
    /// <param name="edit">Is the file in edit mode (allows updates)?</param>
    /// <param name="level">sub-program level. 0 corresponds to the main program</param>
    void ResumeProgram (bool edit, int level);

    /// <summary>
    /// Suspend a program execution, for example with M0 or M1
    /// </summary>
    /// <param name="optional"></param>
    /// <param name="details"></param>
    void SuspendProgram (bool optional = false, string details = "");

    /// <summary>
    /// Set a comment
    /// </summary>
    /// <param name="message"></param>
    void SetComment (string message);

    /// <summary>
    /// Set a machining time
    /// </summary>
    /// <param name="duration"></param>
    void SetMachiningTime (TimeSpan duration);

    /// <summary>
    /// Trigger a machining start
    /// </summary>
    void TriggerMachining ();

    /// <summary>
    /// Next stamping event handler
    /// 
    /// If last, null is returned
    /// </summary>
    IStampingEventHandler? Next { get; set; }

    /// <summary>
    /// Reference to the stamper
    /// </summary>
    IStamper Stamper { get; }
  }
}
