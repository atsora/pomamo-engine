// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Threading;
using Lemoine.Core.Log;
using NHibernate;
using Lemoine.Business.Config;
using Lemoine.Extensions.Database;
using System.Linq;
using Pulse.Extensions.Database.Accumulator;
using System.Collections.Concurrent;
using Lemoine.Database.Persistent;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Description of AnalysisAccumulator.
  /// </summary>
  public sealed class AnalysisAccumulator : ISessionAccumulator
  {
    #region Members
    IDictionary<ISession, HashSet<string>> m_messages = new ConcurrentDictionary<ISession, HashSet<string>> (); // Web service messages
    IDictionary<ISession, List<IAccumulator>> m_accumulators = new ConcurrentDictionary<ISession, List<IAccumulator>> (); // NHibernate session => Accumulators
    // Because ISession is not thread safe, each session is in its own thread, and there is no need to keep a mutex for it

    IEnumerable<IMessageExtension> m_messageExtensions = null;
    object m_messageExtensionsLock = new object ();
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger (typeof (AnalysisAccumulator).FullName);

    #region Methods
    /// <summary>
    /// Check if there are some messages for the specified session
    /// 
    /// This method is thread safe
    /// </summary>
    /// <param name="session"></param>
    /// <returns></returns>
    bool HasMessages (ISession session)
    {
      return m_messages.ContainsKey (session);
    }

    /// <summary>
    /// Push a message for the specified session
    /// </summary>
    /// <param name="session"></param>
    /// <param name="message"></param>
    void PushMessage (ISession session, string message)
    {
      if (null == session) {
        log.ErrorFormat ("GetMessages: " +
                         "null session");
        throw new ArgumentNullException ("session");
      }

      HashSet<string> messages;

      if (false == m_messages.TryGetValue (session, out messages)) {
        messages = new HashSet<string> ();
        Debug.Assert (!m_messages.ContainsKey (session));
        m_messages[session] = messages;
      }
      messages.Add (message);
    }

    /// <summary>
    /// Get the messages for the current session
    /// 
    /// This method is thread safe
    /// </summary>
    /// <param name="session">not null</param>
    /// <returns></returns>
    IEnumerable<string> GetMessages (ISession session)
    {
      if (null == session) {
        log.Error ("GetMessages: null session");
        throw new ArgumentNullException ("session");
      }

      HashSet<string> messages;

      if (false == m_messages.TryGetValue (session, out messages)) {
        return new List<string> ();
      }
      return messages;
    }

    /// <summary>
    /// Remove the messages for the specified session
    /// </summary>
    /// <param name="session"></param>
    /// <returns></returns>
    void RemoveMessages (ISession session)
    {
      m_messages.Remove (session);
    }

    /// <summary>
    /// Check if the accumulators has some accumulators for the current session
    /// 
    /// This method is thread safe
    /// </summary>
    /// <param name="session"></param>
    /// <returns></returns>
    bool HasAccumulators (ISession session)
    {
      return m_accumulators.ContainsKey (session);
    }

    /// <summary>
    /// Is there at least one operation cycle accumulator ?
    /// </summary>
    /// <returns></returns>
    internal static bool HasOperationCycleAccumulator ()
    {
      foreach (IAccumulator accumulator in Instance.GetAccumulators (NHibernateHelper.GetCurrentSession ())) {
        if (accumulator.IsOperationCycleAccumulator ()) {
          return true;
        }
      }
      return false;
    }

    /// <summary>
    /// Get the accumulators for the current session
    /// 
    /// This method is thread safe
    /// </summary>
    /// <param name="session">not null</param>
    /// <returns></returns>
    IList<IAccumulator> GetAccumulators (ISession session)
    {
      if (null == session) {
        log.ErrorFormat ("GetAccumulators: " +
                         "null session");
        throw new ArgumentNullException ("session");
      }

      List<IAccumulator> accumulators;

      if (false == m_accumulators.TryGetValue (session, out accumulators)) {
        // IReasonSlotAccumulators (100)
        // IOperationSlotAccumulators (200)
        // IOperationSlotAccumulators / IOperationCycleAccumulators (300)
        // IOperationCycleAccumulators (400)
        accumulators = Lemoine.Business.ServiceProvider
.Get (new Lemoine.Business.Extension.GlobalExtensions<IAccumulatorExtension> (ext => ext.Initialize ()))
.OrderBy (x => x.Priority)
.Select (x => x.Create ())
.ToList ();

        Debug.Assert (!m_accumulators.ContainsKey (session));
        m_accumulators[session] = accumulators;
      }

      return accumulators;
    }

    /// <summary>
    /// Remove the accumulators for the current session
    /// </summary>
    /// <param name="session"></param>
    /// <returns></returns>
    void RemoveAccumulators (ISession session)
    {
      m_accumulators.Remove (session);
    }

    /// <summary>
    /// Check if there is no accumulator
    /// </summary>
    /// <param name="session"></param>
    /// <returns></returns>
    public bool IsEmpty (ISession session)
    {
      if (session is null) {
        log.Error ("IsEmpty: session is null !");
        throw new ArgumentNullException ("session");
      }

      return !HasAccumulators (session);
    }

    /// <summary>
    /// Clear the accumulators and messages that are associated to the specified session
    /// </summary>
    /// <param name="session">not null</param>
    /// <returns></returns>
    public void Clear (ISession session)
    {
      if (session is null) {
        log.Error ("Store: session is null !");
        throw new ArgumentNullException ("session");
      }

      if (HasAccumulators (session)) {
        RemoveAccumulators (session); // Requires a write lock
      }

      if (HasMessages (session)) {
        RemoveMessages (session);
      }
    }

    /// <summary>
    /// Push a message to the web service (delayed until SendMessages is called, after Commit ())
    /// </summary>
    /// <param name="message"></param>
    internal static void PushMessage (string message)
    {
      Instance.PushMessage (NHibernateHelper.GetCurrentSession (), message);
    }

    static IEnumerable<IMessageExtension> GetMessageExtensions ()
    {
      if (null != Instance.m_messageExtensions) {
        if (log.IsDebugEnabled) {
          log.Debug ($"GetMessageExtensions: from cache {Instance.m_messageExtensions.Count ()} extensions");
        }
        return Instance.m_messageExtensions;
      }

      if (log.IsDebugEnabled) {
        log.Debug ("GetMessageExtensions: not in cache");
      }
      var extensionRequest = new Lemoine.Business.Extension
        .GlobalExtensions<IMessageExtension> ();
      var messageExtensions = Lemoine.Business.ServiceProvider
        .Get (extensionRequest);
      if (log.IsDebugEnabled) {
        log.Debug ("GetMessageExtensions: about to get lock");
      }
      lock (Instance.m_messageExtensionsLock) {
        if (null == Instance.m_messageExtensions) {
          Instance.m_messageExtensions = messageExtensions;
        }
      }
      if (log.IsDebugEnabled) {
        log.Debug ($"GetMessageExtensions: {Instance.m_messageExtensions.Count ()} loaded");
      }

      return Instance.m_messageExtensions;
    }

    static async System.Threading.Tasks.Task SendMessageAsync (string message, IMessageExtension messageExtension)
    {
      try {
        await messageExtension.ProcessMessageAsync (message);
        if (log.IsDebugEnabled) {
          log.Debug ($"SendMessage: message {message} was successfully processed by {messageExtension}");
        }
      }
      catch (Exception ex) {
        log.Error ($"SendMessage: message {message} could not be processed by {messageExtension}", ex);
      }
    }

    /// <summary>
    /// Send a specific message (after the transaction is committed)
    /// 
    /// No exception is thrown here
    /// </summary>
    /// <param name="message"></param>
    public static async System.Threading.Tasks.Task SendMessageAsync (string message)
    {
      try {
        var messageExtensions = GetMessageExtensions ();
        if (!messageExtensions.Any ()) {
          return;
        }

        var tasks = messageExtensions
          .Select (ext => SendMessageAsync (message, ext));
        await System.Threading.Tasks.Task.WhenAll (tasks);
      }
      catch (Exception ex) {
        log.Error ($"SendMessageAsync: unexpected exception, but do not throw it", ex);
      }
    }

    /// <summary>
    /// Send the delayed messages to the web service
    /// </summary>
    public async System.Threading.Tasks.Task SendMessagesAsync (ISession session)
    {
      if (log.IsDebugEnabled) {
        log.Debug ("SendMessages");
      }

      var messageExtensions = GetMessageExtensions ();
      if (!messageExtensions.Any ()) {
        log.Debug ("SendMessages: no message extension");
        return;
      }
      if (log.IsDebugEnabled) {
        log.Debug ($"SendMessages: {messageExtensions.Count ()} message extensions");
      }

      var messages = GetMessages (session);
      if (!messages.Any ()) {
        if (log.IsDebugEnabled) {
          log.Debug ($"SendMessages: no message in session");
        }
        return;
      }
      if (log.IsDebugEnabled) {
        log.Debug ($"SendMessages: {messages.Count ()} messages");
      }

      foreach (var message in messages) {
        if (log.IsDebugEnabled) {
          log.Debug ($"SendMessages: processing message {message}");
        }
        foreach (var messageExtension in messageExtensions) {
          try {
            if (log.IsDebugEnabled) {
              log.Debug ($"SendMessages: about to send {message} extension {messageExtension}");
            }
            await SendMessageAsync (message, messageExtension);
          }
          catch (Exception ex) {
            log.Error ($"SendMessages: exception in SendMessage of {message}, but continue", ex);
          }
        }
      }

      try {
        RemoveMessages (session);
      }
      catch (Exception ex) {
        log.Error ($"SendMessages: exception in RemoveMessages", ex);
      }

      if (log.IsDebugEnabled) {
        log.Debug ($"SendMessages: completed");
      }
    }

    /// <summary>
    /// Store the content of all the accumulators into the database with the current session
    /// </summary>
    /// <param name="transactionName"></param>
    internal static void Store (string transactionName)
    {
      try {
        Instance.Store (NHibernateHelper.GetCurrentSession (), transactionName);
      }
      catch (Exception ex) {
        log.Error ($"Store: {transactionName} ended in error", ex);
        throw;
      }
    }

    /// <summary>
    /// Store the content of all the accumulators into the database
    /// </summary>
    /// <param name="session"></param>
    /// <param name="transactionName"></param>
    public void Store (ISession session, string transactionName)
    {
      if (null == session) {
        log.Error ("Store: session is null !");
        throw new ArgumentNullException ("session");
      }

      if (!HasAccumulators (session)) {
        // There is nothing to do, return at once
        // This is not necessary to run GetAccumulators for which a write lock is necessary
        return;
      }

      IList<IAccumulator> accumulators = GetAccumulators (session);
      if (log.IsDebugEnabled) {
        log.Debug ($"Store: about to store the data from {accumulators.Count} accumulators");
      }
      try {
        foreach (IAccumulator accumulator in accumulators) {
          if (!NHibernateHelper.GetCurrentSession ().Equals (session)) {
            log.Fatal ("Store: session discrepancy");
          }
          Debug.Assert (NHibernateHelper.GetCurrentSession ().Equals (session));
          if (log.IsDebugEnabled) {
            log.Debug ($"Store: store transaction={transactionName} accumulator={accumulator}");
          }
          try {
            accumulator.Store (transactionName);
            if (log.IsDebugEnabled) {
              log.Debug ($"Store: transaction={transactionName} accumulator={accumulator} completed");
            }
          }
          catch (Exception ex) {
            log.Error ($"Store: transaction={transactionName} accumulator={accumulator} ended in error", ex);
            throw;
          }
        }
      }
      finally {
        // Clean now the session (remove the accumulators and the session locks, else there is a memory leak)
        RemoveAccumulators (session);
      }
    }

    /// <summary>
    /// Initialize the caller
    /// </summary>
    /// <param name="caller"></param>
    public static void SetCheckedCaller (IChecked caller)
    {
      if (null != caller) {
        IList<IAccumulator> accumulators = Instance.GetAccumulators (NHibernateHelper.GetCurrentSession ());
        foreach (var accumulator in accumulators) {
          if (accumulator is ICheckedCaller) {
            ICheckedCaller checkedAccumulator = (ICheckedCaller)accumulator;
            checkedAccumulator.SetCheckedCaller (caller);
          }
          if (accumulator is ICheckedCallers) {
            ICheckedCallers checkedAccumulator = (ICheckedCallers)accumulator;
            checkedAccumulator.AddCheckedCaller (caller);
          }
        }
      }
    }

    /// <summary>
    /// Merge two reason slots
    /// </summary>
    /// <param name="previous"></param>
    /// <param name="next"></param>
    /// <param name="result"></param>
    public static void MergeReasonSlots (IReasonSlot previous, IReasonSlot next, IReasonSlot result)
    {
      Debug.Assert (previous.ReferenceDataEquals (next), "ReferenceDataEquals not checked");
      Debug.Assert ((result.Reason.Id == (int)ReasonId.Processing) || (previous.Reason.Id == next.Reason.Id));

      if (log.IsDebugEnabled) {
        log.Debug ($".{previous.Machine.Id} MergeReasonSlots: {previous.Reason.Id}-{next.Reason.Id}=>{result.Reason.Id} dayRange={result.DayRange}");
      }

      if (result.Reason.Id == (int)ReasonId.Processing) {
        RemoveReasonSlot (previous);
        RemoveReasonSlot (next);
        AddReasonSlot (result);
      }
      else if (previous.Reason.Id == next.Reason.Id) { // && result.ReasonId not processing
        if (previous.Reason.Id == (int)ReasonId.Processing) {
          RemoveReasonSlot (previous);
          RemoveReasonSlot (next);
          AddReasonSlot (result);
        }
        else { // previous.Reason.Id not processing
          if (result.Reason.Id != previous.Reason.Id) {
            log.Fatal ($".{previous.Machine.Id} MergeReasonSlots: invalid reasons for {previous.Reason.Id}-{next.Reason.Id}=>{result.Reason.Id}");
          }
          if (!Bound.Equals<DateTime> (result.DayRange.Lower, result.DayRange.Upper)) {
            RemoveReasonSlotNumber (previous);
            RemoveReasonSlotNumber (next);
            AddReasonSlotNumber (result);
          }
          else {
            RemoveReasonSlotNumber (result);
          }
        }
      }
      else {
        log.Fatal ($".{previous.Machine.Id} MergeReasonSlots: invalid reasons for {previous.Reason.Id}-{next.Reason.Id}=>{result.Reason.Id}");
        RemoveReasonSlot (previous);
        RemoveReasonSlot (next);
        AddReasonSlot (result);
      }
    }

    /// <summary>
    /// Add a reason slot to the main analysis accumulator
    /// </summary>
    /// <param name="reasonSlot">not null</param>
    public static void AddReasonSlot (IReasonSlot reasonSlot)
    {
      Debug.Assert (null != reasonSlot);
      Debug.Assert (null != reasonSlot.Reason);
      Debug.Assert (reasonSlot.EndDateTime.HasValue);

      if (log.IsDebugEnabled) {
        log.Debug ($".{reasonSlot.Machine.Id} AddReasonSlot: reasonid={reasonSlot.Reason.Id}");
      }

      AddReasonSlotPeriodOnly (reasonSlot);
      AddReasonSlotNumber (reasonSlot);
    }

    /// <summary>
    /// Remove a reason slot from the main analysis accumulator
    /// </summary>
    /// <param name="reasonSlot"></param>
    public static void RemoveReasonSlot (IReasonSlot reasonSlot)
    {
      Debug.Assert (null != reasonSlot);
      Debug.Assert (null != reasonSlot.Reason);
      Debug.Assert (reasonSlot.EndDateTime.HasValue);

      if (log.IsDebugEnabled) {
        log.Debug ($".{reasonSlot.Machine.Id} RemoveReasonSlot: reasonid={reasonSlot.Reason.Id}");
      }

      RemoveReasonSlotPeriodOnly (reasonSlot);
      RemoveReasonSlotNumber (reasonSlot);
    }

    /// <summary>
    /// Add a reason slot period considering the number
    /// </summary>
    /// <param name="reasonSlot"></param>
    /// <param name="oldReasonSlot"></param>
    /// <param name="range"></param>
    public static void AddReasonSlotPeriod (IReasonSlot reasonSlot,
                                            IReasonSlot oldReasonSlot,
                                            UtcDateTimeRange range)
    {
      Debug.Assert (null != reasonSlot);
      Debug.Assert (null != reasonSlot.Reason);
      Debug.Assert (null != oldReasonSlot);
      Debug.Assert (null != oldReasonSlot.Reason);
      Debug.Assert (!range.IsEmpty ());

      if (log.IsDebugEnabled) {
        log.Debug ($".{reasonSlot.Machine.Id} AddReasonSlotPeriod: reasonid={reasonSlot.Reason.Id} range={range}");
      }

      // A reason slot may not have an unbound period
      Debug.Assert (range.Lower.HasValue);
      Debug.Assert (range.Upper.HasValue);

      // Split by day
      IList<IAccumulator> accumulators = Instance.GetAccumulators (NHibernateHelper.GetCurrentSession ());
      IList<IDaySlot> daySlots = ModelDAOHelper.DAOFactory.DaySlotDAO.FindProcessedInRange (range);
      foreach (IDaySlot daySlot in daySlots) {
        Debug.Assert (daySlot.Day.HasValue);
        if (!daySlot.Day.HasValue) {
          log.Fatal ($"AddReasonSlotPeriod: skip daySlot {daySlot} with no day defined");
          continue;
        }
        UtcDateTimeRange intersection = new UtcDateTimeRange (range.Intersects (daySlot.DateTimeRange));
        if (intersection.IsEmpty ()) {
          Debug.Assert (false);
          log.Fatal ($"AddReasonSlotPeriod: empty intersection of {reasonSlot} with day slot {daySlot} => skip it");
        }
        else {
          Debug.Assert (intersection.Duration.HasValue);
          foreach (IAccumulator accumulator in accumulators) {
            if (accumulator.IsReasonSlotAccumulator ()) {
              IReasonSlotAccumulator reasonSlotAccumulator = (IReasonSlotAccumulator)accumulator;
              reasonSlotAccumulator.AddReasonSlotDuration (reasonSlot,
                                                           daySlot.Day.Value,
                                                           intersection.Duration.Value);
              // If day is not in reasonSlot add one number
              if (!oldReasonSlot.DayRange.ContainsElement (daySlot.Day.Value)) {
                reasonSlotAccumulator.AddReasonSlotNumber (reasonSlot,
                                                           daySlot.Day.Value,
                                                           1);
              }
            }
          }
        }
      }
    }

    /// <summary>
    /// Add a reason slot period without considering the number
    /// </summary>
    /// <param name="reasonSlot"></param>
    static void AddReasonSlotPeriodOnly (IReasonSlot reasonSlot)
    {
      Debug.Assert (null != reasonSlot);
      Debug.Assert (reasonSlot.BeginDay.HasValue);
      Debug.Assert (reasonSlot.EndDay.HasValue);
      Debug.Assert (null != reasonSlot.Reason);

      if (log.IsDebugEnabled) {
        log.DebugFormat (".{0} AddReasonSlotPeriodOnly: reasonid={1} range={2}",
                         reasonSlot.Machine.Id, reasonSlot.Reason.Id, reasonSlot.DateTimeRange);
      }

      // Split by day
      IList<IAccumulator> accumulators = Instance.GetAccumulators (NHibernateHelper.GetCurrentSession ());
      IList<IDaySlot> daySlots = ModelDAOHelper.DAOFactory.DaySlotDAO.FindProcessedInDayRange (reasonSlot.DayRange);
      foreach (IDaySlot daySlot in daySlots) {
        Debug.Assert (daySlot.Day.HasValue);
        if (!daySlot.Day.HasValue) {
          log.FatalFormat ("AddReasonSlotPeriodOnly: " +
                           "skip daySlot {0} with no day defined",
                           daySlot);
          continue;
        }
        UtcDateTimeRange intersection = new UtcDateTimeRange (reasonSlot.DateTimeRange.Intersects (daySlot.DateTimeRange));
        if (intersection.IsEmpty ()) {
          Debug.Assert (false);
          log.FatalFormat ("AddReasonSlotPeriodOnly: " +
                           "empty intersection of {0} with day slot {1} " +
                           "=> skip it",
                           reasonSlot, daySlot);
        }
        else {
          Debug.Assert (intersection.Duration.HasValue);
          foreach (IAccumulator accumulator in accumulators) {
            if (accumulator.IsReasonSlotAccumulator ()) {
              IReasonSlotAccumulator reasonSlotAccumulator = (IReasonSlotAccumulator)accumulator;
              reasonSlotAccumulator.AddReasonSlotDuration (reasonSlot,
                                                           daySlot.Day.Value,
                                                           intersection.Duration.Value);
            }
          }
        }
      }
    }

    /// <summary>
    /// Remove a reason slot period considering the number
    /// </summary>
    /// <param name="reasonSlot">not null</param>
    /// <param name="periodBegin"></param>
    /// <param name="periodEnd"></param>
    public static void RemoveReasonSlotPeriod (IReasonSlot reasonSlot,
                                               LowerBound<DateTime> periodBegin,
                                               UpperBound<DateTime> periodEnd)
    {
      RemoveReasonSlotPeriod (reasonSlot,
                              new UtcDateTimeRange (periodBegin, periodEnd));
    }

    /// <summary>
    /// Remove a reason slot period considering the number
    /// </summary>
    /// <param name="reasonSlot"></param>
    /// <param name="range"></param>
    public static void RemoveReasonSlotPeriod (IReasonSlot reasonSlot,
                                               UtcDateTimeRange range)
    {
      Debug.Assert (null != reasonSlot);
      Debug.Assert (null != reasonSlot.Reason);

      if (log.IsDebugEnabled) {
        log.DebugFormat (".{0} RemoveReasonSlotPeriod: reasonid={1} range={2}",
          reasonSlot.Machine.Id, reasonSlot.Reason.Id, range);
      }

      Debug.Assert (range.Lower.HasValue);
      Debug.Assert (range.Upper.HasValue);

      // Split by day
      IList<IAccumulator> accumulators = Instance.GetAccumulators (NHibernateHelper.GetCurrentSession ());
      IList<IDaySlot> daySlots = ModelDAOHelper.DAOFactory.DaySlotDAO.FindProcessedInRange (range);
      foreach (IDaySlot daySlot in daySlots) {
        Debug.Assert (daySlot.Day.HasValue);
        if (!daySlot.Day.HasValue) {
          log.FatalFormat ("RemoveReasonSlotPeriod: " +
                           "skip daySlot {0} with no day defined",
                           daySlot);
          continue;
        }
        UtcDateTimeRange intersection = new UtcDateTimeRange (range.Intersects (daySlot.DateTimeRange));
        if (intersection.IsEmpty ()) {
          Debug.Assert (false);
          log.FatalFormat ("RemoveReasonSlotPeriod: " +
                           "empty intersection of {0} with day slot {1} " +
                           "=> skip it",
                           reasonSlot, daySlot);
        }
        else {
          Debug.Assert (intersection.Duration.HasValue);
          foreach (IAccumulator accumulator in accumulators) {
            if (accumulator.IsReasonSlotAccumulator ()) {
              IReasonSlotAccumulator reasonSlotAccumulator = (IReasonSlotAccumulator)accumulator;
              reasonSlotAccumulator.AddReasonSlotDuration (reasonSlot,
                                                           daySlot.Day.Value,
                                                           -intersection.Duration.Value);
              // If range covers a full day in reasonSlot, remove one number
              Range<DateTime> reasonSlotDay = reasonSlot.DateTimeRange.Intersects (daySlot.DateTimeRange);
              if (range.ContainsRange (reasonSlotDay)) {
                reasonSlotAccumulator.AddReasonSlotNumber (reasonSlot,
                                                           daySlot.Day.Value,
                                                           -1);
              }
            }
          }
        }
      }
    }

    /// <summary>
    /// Remove a reason slot period without considering the number
    /// </summary>
    /// <param name="reasonSlot"></param>
    static void RemoveReasonSlotPeriodOnly (IReasonSlot reasonSlot)
    {
      Debug.Assert (null != reasonSlot);
      Debug.Assert (reasonSlot.BeginDay.HasValue);
      Debug.Assert (reasonSlot.EndDay.HasValue);
      Debug.Assert (!reasonSlot.DateTimeRange.IsEmpty ());
      Debug.Assert (null != reasonSlot.Reason);

      if (log.IsDebugEnabled) {
        log.DebugFormat (".{0} RemoveReasonSlotPeriodOnly: reasonid={1} range={2}",
          reasonSlot.Machine.Id, reasonSlot.Reason.Id, reasonSlot.DateTimeRange);
      }

      if (reasonSlot.DateTimeRange.IsEmpty ()) {
        log.ErrorFormat ("RemoveReasonSlotPeriodOnly: " +
                         "empty date/time range " +
                         "=> do nothing");
        return;
      }

      // Split by day
      IList<IAccumulator> accumulators = Instance.GetAccumulators (NHibernateHelper.GetCurrentSession ());
      IList<IDaySlot> daySlots = ModelDAOHelper.DAOFactory.DaySlotDAO.FindProcessedInDayRange (reasonSlot.DayRange);
      foreach (IDaySlot daySlot in daySlots) {
        Debug.Assert (daySlot.Day.HasValue);
        if (!daySlot.Day.HasValue) {
          log.FatalFormat ("RemoveReasonSlotPeriodOnly: " +
                           "skip daySlot {0} with no day defined",
                           daySlot);
          continue;
        }
        UtcDateTimeRange intersection = new UtcDateTimeRange (reasonSlot.DateTimeRange.Intersects (daySlot.DateTimeRange));
        if (intersection.IsEmpty ()) {
          Debug.Assert (false);
          log.FatalFormat ("RemoveReasonSlotPeriodOnly: " +
                           "empty intersection of {0} with day slot {1} " +
                           "=> skip it",
                           reasonSlot, daySlot);
        }
        else {
          Debug.Assert (intersection.Duration.HasValue);
          foreach (IAccumulator accumulator in accumulators) {
            if (accumulator.IsReasonSlotAccumulator ()) {
              IReasonSlotAccumulator reasonSlotAccumulator = (IReasonSlotAccumulator)accumulator;
              reasonSlotAccumulator.AddReasonSlotDuration (reasonSlot,
                                                           daySlot.Day.Value,
                                                           -intersection.Duration.Value);
            }
          }
        }
      }
    }

    /// <summary>
    /// Add a reason slot number
    /// </summary>
    /// <param name="reasonSlot"></param>
    static void AddReasonSlotNumber (IReasonSlot reasonSlot)
    {
      Debug.Assert (null != reasonSlot, "reasonSlot null");
      Debug.Assert (null != reasonSlot.Reason, "reason null");

      UpdateReasonSlotNumber (reasonSlot, 1);
    }

    /// <summary>
    /// Remove a reason slot number
    /// </summary>
    /// <param name="reasonSlot"></param>
    static void RemoveReasonSlotNumber (IReasonSlot reasonSlot)
    {
      Debug.Assert (null != reasonSlot);
      Debug.Assert (null != reasonSlot.Reason);

      UpdateReasonSlotNumber (reasonSlot, -1);
    }

    static void UpdateReasonSlotNumber (IReasonSlot reasonSlot,
                                        int offset)
    {
      Debug.Assert (null != reasonSlot, "reasonSlot null");
      Debug.Assert (null != reasonSlot.Reason, "reason null");

      if (log.IsDebugEnabled) {
        log.DebugFormat (".{0} UpdateReasonSlotNumber: reasonid={1} range={2} dayRange={3} offset={3}",
                         reasonSlot.Machine.Id, reasonSlot.Reason.Id, reasonSlot.DateTimeRange, reasonSlot.DayRange, offset);
      }

      // Note: for the moment, offset takes only the values 1 and -1 because it is private and only called by Add... and Remove...

      if (0 != offset) { // Shortcut: only if there is effectively a change to make
                         // Split by day

        var accumulators = Instance.GetAccumulators (NHibernateHelper.GetCurrentSession ());
        IList<IDaySlot> daySlots = ModelDAOHelper.DAOFactory.DaySlotDAO.FindProcessedInDayRange (reasonSlot.DayRange);
        foreach (IDaySlot daySlot in daySlots) {
          if (log.IsDebugEnabled) {
            log.DebugFormat (".{0} UpdateReasonSlotNumber: day={4} reasonid={1} range={2} offset={3}",
                             reasonSlot.Machine.Id, reasonSlot.Reason.Id, reasonSlot.DateTimeRange, offset, daySlot.Day);
          }
          Debug.Assert (daySlot.Day.HasValue);
          if (!daySlot.Day.HasValue) {
            log.FatalFormat (".{0} UpdateReasonSlotNumber: " +
                             "skip daySlot {1} with no day defined",
                             reasonSlot.Machine.Id, daySlot);
            continue;
          }
          UtcDateTimeRange intersection = new UtcDateTimeRange (reasonSlot.DateTimeRange.Intersects (daySlot.DateTimeRange));
          if (intersection.IsEmpty ()) {
            Debug.Assert (false, "Empty intersection");
            log.FatalFormat (".{0} UpdateReasonSlotNumber: " +
                             "empty intersection of {1} with day slot {2} " +
                             "=> skip it",
                             reasonSlot.Machine.Id, reasonSlot, daySlot);
          }
          else {
            Debug.Assert (intersection.Duration.HasValue);
            foreach (IAccumulator accumulator in accumulators) {
              if (accumulator.IsReasonSlotAccumulator ()) {
                IReasonSlotAccumulator reasonSlotAccumulator = (IReasonSlotAccumulator)accumulator;
                reasonSlotAccumulator.AddReasonSlotNumber (reasonSlot,
                                                           daySlot.Day.Value,
                                                           offset);
              }
            }
          }
        }
      }
    }

    /// <summary>
    /// Warn the accumulators an operation slot was updated
    /// </summary>
    /// <param name="before"></param>
    /// <param name="after"></param>
    public static void OperationSlotUpdated (IOperationSlot before, IOperationSlot after)
    {
      IList<IAccumulator> accumulators = Instance.GetAccumulators (NHibernateHelper.GetCurrentSession ());
      foreach (IAccumulator accumulator in accumulators) {
        if (accumulator.IsOperationSlotAccumulator ()) {
          IOperationSlotAccumulator operationSlotAccumulator = (IOperationSlotAccumulator)accumulator;
          operationSlotAccumulator.OperationSlotUpdated (before, after);
        }
      }
    }

    /// <summary>
    /// Warn the accumulators an operation slot was removed
    /// </summary>
    /// <param name="operationSlot">Removed operation slot</param>
    /// <param name="initialState">Initial state of the operation slot (without the ID)</param>
    public static void OperationSlotRemoved (IOperationSlot operationSlot, IOperationSlot initialState)
    {
      IList<IAccumulator> accumulators = Instance.GetAccumulators (NHibernateHelper.GetCurrentSession ());
      foreach (IAccumulator accumulator in accumulators) {
        if (accumulator.IsOperationSlotAccumulator ()) {
          IOperationSlotAccumulator operationSlotAccumulator = (IOperationSlotAccumulator)accumulator;
          operationSlotAccumulator.OperationSlotRemoved (operationSlot, initialState);
        }
      }
    }

    /// <summary>
    /// Warn the accumulators an operation cycle was updated
    /// </summary>
    /// <param name="before">if null, transient</param>
    /// <param name="after">if null, transient</param>
    public static void OperationCycleUpdated (IOperationCycle before, IOperationCycle after)
    {
      IList<IAccumulator> accumulators = Instance.GetAccumulators (NHibernateHelper.GetCurrentSession ());

      if (accumulators.Any (a => a.IsOperationCycleAccumulator ())) {
        if (null != before) {
          if (!ModelDAOHelper.DAOFactory.IsInitialized (before)) {
            log.ErrorFormat ("OperationCycleUpdated: before is not initialized. StackTrace={0}",
              System.Environment.StackTrace);
          }
        }
        if (null != after) {
          if (!ModelDAOHelper.DAOFactory.IsInitialized (after)) {
            log.ErrorFormat ("OperationCycleUpdated: after is not initialized. StackTrace={0}",
              System.Environment.StackTrace);
          }
        }
      }

      foreach (IAccumulator accumulator in accumulators) {
        if (accumulator.IsOperationCycleAccumulator ()) {
          IOperationCycleAccumulator operationCycleAccumulator = (IOperationCycleAccumulator)accumulator;
          operationCycleAccumulator.OperationCycleUpdated (before, after);
        }
      }
    }

    /// <summary>
    /// Add an observation state slot period
    /// </summary>
    /// <param name="slot"></param>
    /// <param name="range"></param>
    public static void AddObservationStateSlotPeriod (IObservationStateSlot slot, UtcDateTimeRange range)
    {
      Debug.Assert (null != slot);
      if (!range.IsEmpty ()) {
        IList<IAccumulator> accumulators = Instance.GetAccumulators (NHibernateHelper.GetCurrentSession ());
        foreach (IAccumulator accumulator in accumulators) {
          if (accumulator.IsObservationStateSlotAccumulator ()) {
            IObservationStateSlotAccumulator observationStateSlotAccumulator = (IObservationStateSlotAccumulator)accumulator;
            observationStateSlotAccumulator.AddObservationStateSlotPeriod (slot, range);
          }
        }
      }
    }

    /// <summary>
    /// Remove an observation state slot period
    /// </summary>
    /// <param name="slot"></param>
    /// <param name="range"></param>
    public static void RemoveObservationStateSlotPeriod (IObservationStateSlot slot, UtcDateTimeRange range)
    {
      Debug.Assert (null != slot);
      if (!range.IsEmpty ()) {
        IList<IAccumulator> accumulators = Instance.GetAccumulators (NHibernateHelper.GetCurrentSession ());
        foreach (IAccumulator accumulator in accumulators) {
          if (accumulator.IsObservationStateSlotAccumulator ()) {
            IObservationStateSlotAccumulator observationStateSlotAccumulator = (IObservationStateSlotAccumulator)accumulator;
            observationStateSlotAccumulator.RemoveObservationStateSlotPeriod (slot, range);
          }
        }
      }
    }
    #endregion // Methods

    #region Instance
    static internal AnalysisAccumulator Instance
    {
      get { return Nested.instance; }
    }

    class Nested
    {
      // Explicit static constructor to tell C# compiler
      // not to mark type as beforefieldinit
      static Nested ()
      {
      }

      internal static readonly AnalysisAccumulator instance = new AnalysisAccumulator ();
    }
    #endregion // Instance
  }
}
