// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using Lemoine.Extensions.Alert;
using Lemoine.Business.Operation;
using Lemoine.Extensions.Business.Group;
using System.Threading;

namespace Lemoine.Alert.GDBListeners
{
  [Serializable]
  public class ReserveCapacityInfo
  {
    public string Name;
    public int Shift;
    public double? NbPieces;
    public double? ReserveCapacity;
    public double? RemainingCapacity;
    public double? GoalCurrentShift;
    public double? ShiftGoal;
    public DateTime? Day;
    public DateTime BeginDateTime;
  }

  /// <summary>
  /// CncAlarmListener for a specific machine module and a specific field
  /// </summary>
  internal class ReserveCapacityGroupListener : IListener
  {
    static readonly string APPLICATION_STATE_KEY_REMAINING_PREFIX = "alert.listener.reserveCapacity.remaining.";
    static readonly string APPLICATION_STATE_KEY_LAST_SHIFT = "alert.listener.reserveCapacity.lastShift.";
    static readonly string APPLICATION_STATE_KEY_LAST_DAY = "alert.listener.reserveCapacity.lastDay.";
    static readonly string THRESHOLD_KEY = "ReserveCapacityAlert.Threshold";
    static readonly int THRESHOLD_DEFAULT = 0; // generate alert only if remaining < threshold

    #region Members
    IGroup m_group;
    string m_name;
    int m_threshold = 0;
    XmlSerializer m_xmlSerializer;
    #endregion // Members

    ILog log = LogManager.GetLogger (typeof (ReserveCapacityGroupListener).FullName);
    IApplicationState m_reserveCapacityGroupListenerStateLastShift = null;
    IApplicationState m_reserveCapacityGroupListenerStateLastDay = null;
    IApplicationState m_reserveCapacityGroupListenerStateRemaining = null;

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    /// <param name="machine"></param>
    public ReserveCapacityGroupListener (IGroup group)
    //    public ReserveCapacityGroupListener (IMachine machine)
    {
      log.Debug ($"GetData: group={group}");
      m_group = group;
      m_name = m_group.Id;

      m_threshold = Lemoine.Info.ConfigSet.LoadAndGet<int> (THRESHOLD_KEY, THRESHOLD_DEFAULT);
      log.Debug ($"ReserveCapacityGroupListener: threshold={m_threshold}");

      // Create a serializer for the class CncAlarm
      Type type = Type.GetType ("Lemoine.Alert.GDBListeners.ReserveCapacityInfo");
      m_xmlSerializer = new XmlSerializer (type);
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Get in the listener the next data.
    /// Returns null when there is no data anymore to return
    /// </summary>
    /// <returns>new data or null</returns>
    public XmlElement GetData ()
    {
      log.Debug ($"GetData: group={m_group}");
      CheckInitialization ();
      XmlElement data = null;

      ReserveCapacityCurrentShift request;
      request = new ReserveCapacityCurrentShift (m_group);

      ReserveCapacityCurrentShiftResponse responseDTO = request.Get ();

      if (null != responseDTO) {
        log.Debug ($"GetData: got request response");
        ReserveCapacityInfo reserveCapacityInfo = new ReserveCapacityInfo ();
        reserveCapacityInfo.Name = m_name;
        reserveCapacityInfo.Shift = (null != responseDTO.Shift ? responseDTO.Shift.Id : -1);
        reserveCapacityInfo.NbPieces = responseDTO.NbPiecesCurrentShift;
        reserveCapacityInfo.ReserveCapacity = (null != responseDTO.ReserveCapacity ? Math.Round((double)responseDTO.ReserveCapacity) : 0);
        reserveCapacityInfo.RemainingCapacity = (null != responseDTO.RemainingCapacity ? Math.Round ((double)responseDTO.RemainingCapacity) : 0);
        reserveCapacityInfo.GoalCurrentShift = responseDTO.GoalCurrentShift;
        reserveCapacityInfo.ShiftGoal = responseDTO.ShiftGoal;
        reserveCapacityInfo.Day = responseDTO.Day;
        reserveCapacityInfo.BeginDateTime = responseDTO.DateTime;// DateTime.UtcNow; 

        /*
        log.Debug ($"GetData: name={reserveCapacityInfo.Name}");
        log.Debug ($"GetData: shift={reserveCapacityInfo.Shift}");
        log.Debug ($"GetData: nbPieces={reserveCapacityInfo.NbPieces}");
        log.Debug ($"GetData: reserveCapacity={reserveCapacityInfo.ReserveCapacity}");
        log.Debug ($"GetData: remainingCapacity={reserveCapacityInfo.RemainingCapacity}");
        log.Debug ($"GetData: goalCurrentShift={reserveCapacityInfo.GoalCurrentShift}");
        log.Debug ($"GetData: shiftGoal={reserveCapacityInfo.ShiftGoal}");
        log.Debug ($"GetData: beginDateTime={reserveCapacityInfo.BeginDateTime}");
        */
        // check delay since latest alert
        // do we already send alert in same shift or day?
        if (reserveCapacityInfo.Shift != (int)(m_reserveCapacityGroupListenerStateLastShift.Value) 
          || (-1 == (int)(m_reserveCapacityGroupListenerStateLastShift.Value))
          || !(reserveCapacityInfo.Day.Equals((DateTime)(m_reserveCapacityGroupListenerStateLastDay.Value)))) {
          log.Debug ($"GetData: no alert in same shift or day. Previous={m_reserveCapacityGroupListenerStateLastShift.Value} new={reserveCapacityInfo.Shift}");

          // check remaining versus allowed value
          var delta = (reserveCapacityInfo.RemainingCapacity + reserveCapacityInfo.NbPieces) - reserveCapacityInfo.ShiftGoal;
          log.Debug ($"GetData: delta={delta}");

          if (delta < m_threshold) {
            try {
              // - serialization
              var sw = new StringWriter ();
              m_xmlSerializer.Serialize (sw, reserveCapacityInfo);
              var document = new XmlDocument ();
              document.LoadXml (sw.ToString ());
              data = document.DocumentElement;
              log.Debug ($"GetData: xml data={data.InnerXml}");

              // - Update the application state
              log.Debug ($"GetData: Update the application state");
              using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
                using (var transaction = session.BeginTransaction ("ReserveCapacityGroupListenerState")) {
                  ModelDAOHelper.DAOFactory.ApplicationStateDAO.Lock (m_reserveCapacityGroupListenerStateRemaining);
                  m_reserveCapacityGroupListenerStateRemaining.Value = (double)(reserveCapacityInfo.RemainingCapacity);
                  ModelDAOHelper.DAOFactory.ApplicationStateDAO.MakePersistent (m_reserveCapacityGroupListenerStateRemaining);
                  //ModelDAOHelper.DAOFactory.ApplicationStateDAO.Lock (m_reserveCapacityGroupListenerStateDate);
                  m_reserveCapacityGroupListenerStateLastShift.Value = (int)(reserveCapacityInfo.Shift);
                  ModelDAOHelper.DAOFactory.ApplicationStateDAO.MakePersistent (m_reserveCapacityGroupListenerStateLastShift);
                  m_reserveCapacityGroupListenerStateLastDay.Value = (DateTime)(reserveCapacityInfo.Day);
                  ModelDAOHelper.DAOFactory.ApplicationStateDAO.MakePersistent (m_reserveCapacityGroupListenerStateLastDay);
                  transaction.Commit ();
                }
              }
            }
            catch (Exception ex) {
              log.Debug ($"GetData: ex={ex}", ex);
            }
          }
          else {
            log.Debug ($"GetData: remaining change but below delta");
          }
        }
        else { // different shift
          log.Debug ($"GetData: already alert in same shift={m_reserveCapacityGroupListenerStateLastShift.Value}. Do not generate alert");
        }
      }
      else {
        log.Debug ($"GetData: response is null");
      }
      return data;

    }


    /// <summary>
    /// Check m_cncAlarmMachineModuleListenerState is initialized
    /// </summary>
    void CheckInitialization ()
    {
      log.Debug ($"CheckInitialization:");
      // keep latest reserve value and date
      
      if (m_reserveCapacityGroupListenerStateRemaining == null) {
        using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
          string keyLasttShift = APPLICATION_STATE_KEY_LAST_SHIFT + m_name;
          string keyLastDay = APPLICATION_STATE_KEY_LAST_DAY + m_name;
          string keyReserve = APPLICATION_STATE_KEY_REMAINING_PREFIX + m_name;

          m_reserveCapacityGroupListenerStateRemaining = ModelDAOHelper.DAOFactory.ApplicationStateDAO.GetApplicationState (keyReserve);
          m_reserveCapacityGroupListenerStateLastShift = ModelDAOHelper.DAOFactory.ApplicationStateDAO.GetApplicationState (keyLasttShift);
          m_reserveCapacityGroupListenerStateLastDay = ModelDAOHelper.DAOFactory.ApplicationStateDAO.GetApplicationState (keyLastDay);

          using (IDAOTransaction transaction = session.BeginTransaction ()) {
            if (m_reserveCapacityGroupListenerStateRemaining == null) {
              //ModelDAOHelper.DAOFactory.ApplicationStateDAO.Lock (m_reserveCapacityGroupListenerStateRemaining);
              m_reserveCapacityGroupListenerStateRemaining = ModelDAOHelper.ModelFactory.CreateApplicationState (keyReserve);
              m_reserveCapacityGroupListenerStateRemaining.Value = (double)0;
              ModelDAOHelper.DAOFactory.ApplicationStateDAO.MakePersistent (m_reserveCapacityGroupListenerStateRemaining);
            }
            if (m_reserveCapacityGroupListenerStateLastShift == null) {
              //ModelDAOHelper.DAOFactory.ApplicationStateDAO.Lock (m_reserveCapacityGroupListenerStateLastShift);
              m_reserveCapacityGroupListenerStateLastShift = ModelDAOHelper.ModelFactory.CreateApplicationState (keyLasttShift);
              m_reserveCapacityGroupListenerStateLastShift.Value = (int)-1;
              ModelDAOHelper.DAOFactory.ApplicationStateDAO.MakePersistent (m_reserveCapacityGroupListenerStateLastShift);
            }
            if (m_reserveCapacityGroupListenerStateLastDay == null) {
              //ModelDAOHelper.DAOFactory.ApplicationStateDAO.Lock (m_reserveCapacityGroupListenerStateLastDay);
              m_reserveCapacityGroupListenerStateLastDay = ModelDAOHelper.ModelFactory.CreateApplicationState (keyLastDay);
              m_reserveCapacityGroupListenerStateLastDay.Value = (DateTime)DateTime.MinValue;
              ModelDAOHelper.DAOFactory.ApplicationStateDAO.MakePersistent (m_reserveCapacityGroupListenerStateLastDay);
            }
            transaction.Commit ();
          }
        }
      }
      
    }
    #endregion // Methods
  }
}
