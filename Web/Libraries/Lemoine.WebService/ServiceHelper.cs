// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Diagnostics;
using System.Linq;
using System.Collections.Generic;
using Lemoine.Core.Log;

using Lemoine.Model;
using Lemoine.ModelDAO;

using Lemoine.DTO;

namespace Lemoine.WebService
{
  /// <summary>
  /// Helper class for Web Services
  /// </summary>
  public class ServiceHelper
  {
    static readonly ILog log = LogManager.GetLogger(typeof (ServiceHelper).FullName);

    /// <summary>
    /// Returns an OkDTO
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    public static Lemoine.DTO.OkDTO ResponseOkDTO(string message) {
      return new Lemoine.DTO.OkDTO(message);
    }
    
    /// <summary>
    /// Returns an OkDTO
    /// </summary>
    /// <param name="id"></param>
    /// <param name="message"></param>
    /// <returns></returns>
    public static Lemoine.DTO.OkDTO ResponseOkDTO(int id, string message) {
      return new Lemoine.DTO.OkDTO(id, message);
    }
    
    #region Error DTO builders
    /// <summary>
    /// "No machine" error DTO
    /// </summary>
    /// <param name="machineId"></param>
    /// <returns></returns>
    public static Lemoine.DTO.ErrorDTO NoMachineWithIdErrorDTO(int machineId) {
      return new Lemoine.DTO.ErrorDTO(String.Format("No machine with id {0}", machineId),
                                      ErrorStatus.PERMANENT);
    }

    /// <summary>
    /// "No next stop information" error DTO
    /// </summary>
    /// <param name="machineId"></param>
    /// <returns></returns>
    public static Lemoine.DTO.ErrorDTO NoOperationStateInformation(int machineId) {
      return new Lemoine.DTO.ErrorDTO(String.Format("No available next stop information for machine with id {0}",
                                                    machineId),
                                      ErrorStatus.NO_DATA);
    }
    
    /// <summary>
    /// "No cycle progress information" error DTO
    /// </summary>
    /// <param name="machineId"></param>
    /// <returns></returns>
    public static Lemoine.DTO.ErrorDTO NoCycleProgressInformation(int machineId) {
      return new Lemoine.DTO.ErrorDTO(String.Format("No cycle progress information for machine with id {0}",
                                                    machineId),
                                      ErrorStatus.NO_DATA);
    }
    
    /// <summary>
    /// "Bad datetime range" error DTO
    /// </summary>
    /// <param name="range"></param>
    /// <returns></returns>
    public static Lemoine.DTO.ErrorDTO BadDateTimeRange(Range<DateTime> range) {
      return new Lemoine.DTO.ErrorDTO(String.Format("Incorrect date/time range {0}",
                                                    range),
                                      ErrorStatus.PERMANENT);
    }
    
    /// <summary>
    /// "Bad datetime range" error DTO
    /// </summary>
    /// <param name="beginDateTime"></param>
    /// <param name="endDateTime"></param>
    /// <returns></returns>
    public static Lemoine.DTO.ErrorDTO BadDateTimeRange(LowerBound<DateTime> beginDateTime, UpperBound<DateTime> endDateTime) {
      return BadDateTimeRange (new UtcDateTimeRange (beginDateTime, endDateTime));
    }
    
    /// <summary>
    /// "Bad datetime range" error DTO
    /// </summary>
    /// <param name="beginDateTime"></param>
    /// <param name="endDateTime"></param>
    /// <returns></returns>
    public static Lemoine.DTO.ErrorDTO BadDateTimeRange(DateTime beginDateTime, DateTime endDateTime) {
      return new Lemoine.DTO.ErrorDTO(String.Format("Incorrect date/time range {0}-{1}", beginDateTime, endDateTime),
                                      ErrorStatus.PERMANENT);
    }
    
    /// <summary>
    /// "No modifications" error DTO
    /// </summary>
    /// <returns></returns>
    public static Lemoine.DTO.ErrorDTO NoModificationIdsErrorDTO() {
      return new Lemoine.DTO.ErrorDTO("No modification ids in request", ErrorStatus.PERMANENT);
    }
    
    /// <summary>
    /// "No revision" error DTO
    /// </summary>
    /// <param name="revId"></param>
    /// <returns></returns>
    public static Lemoine.DTO.ErrorDTO NoRevisionWithIdErrorDTO(int revId) {
      return new Lemoine.DTO.ErrorDTO(String.Format("No revision with id {0}", revId),
                                      ErrorStatus.PERMANENT);
    }

    /// <summary>
    /// "No reason" error DTO
    /// </summary>
    /// <param name="reasonId"></param>
    /// <returns></returns>
    public static Lemoine.DTO.ErrorDTO NoReasonWithIdErrorDTO(int reasonId) {
      return new Lemoine.DTO.ErrorDTO(String.Format("No reason with id {0}", reasonId),
                                      ErrorStatus.PERMANENT);
    }
    
    /// <summary>
    /// "No reason slot" error DTO
    /// </summary>
    /// <param name="machineId"></param>
    /// <returns></returns>
    public static Lemoine.DTO.ErrorDTO NoReasonSlotErrorDTO(int machineId) {
      return new Lemoine.DTO.ErrorDTO(String.Format("No reasonslot for machine with id {0}", machineId),
                                      ErrorStatus.NO_DATA);
    }
    
    /// <summary>
    /// "No shift slot" error DTO
    /// </summary>
    /// <returns></returns>
    public static Lemoine.DTO.ErrorDTO NoShiftSlotErrorDTO() {
      return new Lemoine.DTO.ErrorDTO("No shiftslot defined",
                                      ErrorStatus.NO_DATA);
    }
    
    /// <summary>
    /// "No monitored machine" error DTO
    /// </summary>
    /// <param name="machineId"></param>
    /// <returns></returns>
    public static Lemoine.DTO.ErrorDTO NoMonitoredMachineWithIdErrorDTO(int machineId) {
      return new Lemoine.DTO.ErrorDTO(String.Format("No monitored machine with id {0}", machineId),
                                      ErrorStatus.PERMANENT);
    }
    
    /// <summary>
    /// "No line" error DTO
    /// </summary>
    /// <param name="lineId"></param>
    /// <returns></returns>
    public static Lemoine.DTO.ErrorDTO NoLineWithIdErrorDTO(int lineId) {
      return new Lemoine.DTO.ErrorDTO(String.Format("No line with id {0}", lineId),
                                      ErrorStatus.PERMANENT);
    }
    
    /// <summary>
    /// "No intermediateworkpiece" error DTO
    /// </summary>
    /// <param name="iwpId"></param>
    /// <returns></returns>
    public static Lemoine.DTO.ErrorDTO NoIntermediateWorkPieceWithIdErrorDTO(int iwpId) {
      return new Lemoine.DTO.ErrorDTO(String.Format("No IWP with id {0}", iwpId),
                                      ErrorStatus.PERMANENT);
    }

    /// <summary>
    /// "No last operation slot" error DTO
    /// </summary>
    /// <param name="machineId"></param>
    /// <returns></returns>
    public static Lemoine.DTO.ErrorDTO NoLastOperationSlotErrorDTO(int machineId) {
      return new Lemoine.DTO.ErrorDTO(String.Format("No last operation slot on machine with id {0}", machineId),
                                      ErrorStatus.NO_DATA);
    }
    
    /// <summary>
    /// "No machine observation state" error DTO
    /// </summary>
    /// <param name="machineObservationStateId"></param>
    /// <returns></returns>
    public static Lemoine.DTO.ErrorDTO NoMachineObservationStateWithIdErrorDTO(int machineObservationStateId) {
      return new Lemoine.DTO.ErrorDTO(String.Format("No machine observation state with id {0}", machineObservationStateId),
                                      ErrorStatus.PERMANENT);
    }
    
    /// <summary>
    /// "No machine state template" error DTO
    /// </summary>
    /// <param name="machineStateTemplateId"></param>
    /// <returns></returns>
    public static Lemoine.DTO.ErrorDTO NoMachineStateTemplateWithIdErrorDTO(int machineStateTemplateId) {
      return new Lemoine.DTO.ErrorDTO(String.Format("No machine state template with id {0}", machineStateTemplateId),
                                      ErrorStatus.PERMANENT);
    }
    
    /// <summary>
    /// "No machine status" error DTO
    /// </summary>
    /// <param name="machineId"></param>
    /// <returns></returns>
    public static Lemoine.DTO.ErrorDTO NoMachineStatusWithIdErrorDTO(int machineId) {
      return new Lemoine.DTO.ErrorDTO(String.Format("No machine status for monitored machine with id {0}",
                                                    machineId), ErrorStatus.NO_DATA);
    }
    
    /// <summary>
    /// "No target utilization percentage" error DTO
    /// </summary>
    /// <param name="machineId"></param>
    /// <returns></returns>
    public static Lemoine.DTO.ErrorDTO NoTargetPercentageForMachineWithIdErrorDTO(int machineId) {
      return new Lemoine.DTO.ErrorDTO(String.Format("No target utilization percentage for monitored machine with id {0}",
                                                    machineId), ErrorStatus.PERMANENT_NO_CONFIG);
    }
    
    /// <summary>
    /// "Bad date range" error DTO
    /// </summary>
    /// <param name="offsetBegin"></param>
    /// <param name="offsetEnd"></param>
    /// <returns></returns>
    public static Lemoine.DTO.ErrorDTO BadDateRangeErrorDTO(int offsetBegin, int offsetEnd) {
      return new Lemoine.DTO.ErrorDTO(String.Format("Bad offsets begin={0} > end={1}", offsetBegin, offsetEnd),
                                      ErrorStatus.PERMANENT);
    }
    
    /// <summary>
    /// "No performance field defined" error DTO
    /// </summary>
    /// <param name="machineId"></param>
    /// <returns></returns>
    public static Lemoine.DTO.ErrorDTO NoPerformanceFieldDefinedForMachineErrorDTO(int machineId) {
      return new Lemoine.DTO.ErrorDTO(String.Format("No performance field defined for monitored machine with id {0}",
                                                    machineId), ErrorStatus.PERMANENT_NO_CONFIG);
    }

    
    /// <summary>
    /// "No last operation slot" error DTO
    /// </summary>
    /// <param name="machineId"></param>
    /// <param name="dateTime"></param>
    /// <returns></returns>
    public static Lemoine.DTO.ErrorDTO NoOperationSlotWithMachineIdErrorDTO(int machineId, DateTime dateTime) {
      return new Lemoine.DTO.ErrorDTO(String.Format("No last operation slot for machine with id {0} at {1}",
                                                    machineId, dateTime), ErrorStatus.NO_DATA);
    }
    
    /// <summary>
    /// "No operation slot" error DTO
    /// </summary>
    /// <param name="operationSlotId"></param>
    /// <returns></returns>
    public static Lemoine.DTO.ErrorDTO NoOperationSlotWithIdErrorDTO(int operationSlotId) {
      return new Lemoine.DTO.ErrorDTO(String.Format("No operation slot with id {0}",
                                                    operationSlotId), ErrorStatus.PERMANENT);
    }
    
    /// <summary>
    /// "Unknown workinformation kind" error DTO
    /// </summary>
    /// <param name="kind"></param>
    /// <returns></returns>
    public static Lemoine.DTO.ErrorDTO BadWorkInformationKindErrorDTO(string kind) {
      return new Lemoine.DTO.ErrorDTO(String.Format("Unknown workinformation kind: {0}", kind), ErrorStatus.PERMANENT);
    }
    
    /// <summary>
    /// "No work order" error DTO
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public static Lemoine.DTO.ErrorDTO NoWorkOrderWithIdErrorDTO(int id) {
      return new Lemoine.DTO.ErrorDTO(String.Format("No workorder with id {0}", id), ErrorStatus.PERMANENT);
    }
    
    /// <summary>
    /// "No operation" error DTO
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public static Lemoine.DTO.ErrorDTO NoOperationWithIdErrorDTO(int id) {
      return new Lemoine.DTO.ErrorDTO(String.Format("No operation with id {0}", id), ErrorStatus.PERMANENT);
    }
    
    /// <summary>
    /// "No component" error DTO
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public static Lemoine.DTO.ErrorDTO NoComponentWithIdErrorDTO(int id) {
      return new Lemoine.DTO.ErrorDTO(String.Format("No component with id {0}", id), ErrorStatus.PERMANENT);
    }
    
    
    /// <summary>
    /// "Incomptatible Service" error DTO
    /// </summary>
    /// <param name="useMsg"></param>
    /// <returns></returns>
    public static Lemoine.DTO.ErrorDTO IncompatibleServiceErrorDTO(string useMsg) {
      return new Lemoine.DTO.ErrorDTO(String.Format("Incompatible service: try to use {0}", useMsg), ErrorStatus.PERMANENT);
    }
    
    /// <summary>
    /// "No department" error DTO
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public static Lemoine.DTO.ErrorDTO NoDepartmentWithIdErrorDTO(int id) {
      return new Lemoine.DTO.ErrorDTO(String.Format("No department with id {0}", id), ErrorStatus.PERMANENT);
    }
    
    /// <summary>
    /// "No machine category" error DTO
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public static Lemoine.DTO.ErrorDTO NoMachineCategoryWithIdErrorDTO(int id) {
      return new Lemoine.DTO.ErrorDTO(String.Format("No machine category with id {0}", id), ErrorStatus.PERMANENT);
    }

    /// <summary>
    /// "No department" error DTO
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public static Lemoine.DTO.ErrorDTO NoMachineSubCategoryWithIdErrorDTO(int id) {
      return new Lemoine.DTO.ErrorDTO(String.Format("No machine sub-category with id {0}", id), ErrorStatus.PERMANENT);
    }

    /// <summary>
    /// "No department" error DTO
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public static Lemoine.DTO.ErrorDTO NoCellWithIdErrorDTO(int id) {
      return new Lemoine.DTO.ErrorDTO(String.Format("No Cell with id {0}", id), ErrorStatus.PERMANENT);
    }
    
    /// <summary>
    /// "No department" error DTO
    /// </summary>
    /// <param name="group"></param>
    /// <returns></returns>
    public static Lemoine.DTO.ErrorDTO UnknownMachineGroupKindErrorDTO(String group) {
      return new Lemoine.DTO.ErrorDTO(String.Format("Unknown machine group kind: {0}", group), ErrorStatus.PERMANENT);
    }
    
    /// <summary>
    /// "Nullable parameter" error DTO
    /// </summary>
    /// <returns></returns>
    public static Lemoine.DTO.ErrorDTO NullableParameterErrorDTO(String param) {
      return new Lemoine.DTO.ErrorDTO(String.Format("Parameter can not be nullable: {0}", param), ErrorStatus.PERMANENT);
    }
    
    /// <summary>
    /// "No operation slot found" error DTO
    /// </summary>
    /// <returns></returns>
    public static Lemoine.DTO.ErrorDTO NoOperationSlotFoundErrorDTO() {
      return new Lemoine.DTO.ErrorDTO("No operation slot found", ErrorStatus.PERMANENT);
    }

    #endregion // Error DTO builders
    
    #region Helper functions
    /// <summary>
    /// Tries to parse a "yyyy/MM/dd" formatted string representing a day as a DateTime
    /// 
    /// This is a deprecated format. It must not be used any more in the new services
    /// </summary>
    /// <param name="urlDay"></param>
    /// <returns></returns>
    public static DateTime GetDayWithOldFormat(string urlDay)
    {
      if (string.IsNullOrEmpty (urlDay)) {
        DateTime today = ModelDAOHelper.DAOFactory.TimeConfigDAO.GetToday();
        log.DebugFormat ("GetDayWithOldFormat: " +
                         "empty string {0} => return today {1}",
                         urlDay, today);
        Debug.Assert (DateTimeKind.Unspecified == today.Kind);
        return today;
      }
      
      DateTime result = DateTime.ParseExact(urlDay, "yyyy/MM/dd",
                                            System.Globalization.CultureInfo.InvariantCulture);
      Debug.Assert (DateTimeKind.Unspecified == result.Kind);
      return result;
    }
    
    /// <summary>
    /// Tries to parse a string representing a day as a DateTime
    /// 
    /// If the string is empty, consider today
    /// </summary>
    /// <param name="urlDay"></param>
    /// <returns></returns>
    public static DateTime GetDay (string urlDay)
    {
      if (string.IsNullOrEmpty (urlDay)) {
        DateTime today = ModelDAOHelper.DAOFactory.TimeConfigDAO.GetToday();
        log.DebugFormat ("GetDay: " +
                         "empty string {0} => return today {1}",
                         urlDay, today);
        return today;
      }
      
      DateTime result = DateTime.ParseExact(urlDay, "yyyy-MM-dd",
                                            System.Globalization.CultureInfo.InvariantCulture);
      Debug.Assert (DateTimeKind.Unspecified == result.Kind);
      return result;
    }

    /// <summary>
    /// Returns "current period" associated with a machine.
    /// By default 12 hours. TODO: refine this
    /// </summary>
    /// <param name="machine"></param>
    /// <returns></returns>
    public static TimeSpan GetCurrentPeriodDuration(IMachine machine) {
      return TimeSpan.FromHours(12);
    }
    #endregion // Helper functions

#if NSERVICEKIT
#region RequestRemoteIp
    /// <summary>
    /// Fetch IP of request
    /// </summary>
    /// <returns></returns>
    public static string RequestRemoteIp(NServiceKit.ServiceHost.IHttpRequest httpRequest) {
      // string remoteIP = base.RequestContext.IpAddress; // does not work
      if (httpRequest != null) {
        System.Net.HttpListenerRequest httpListenerRequest = (System.Net.HttpListenerRequest) httpRequest.OriginalRequest;
        if (httpListenerRequest != null) {
          string ipAddress = httpListenerRequest.RemoteEndPoint.ToString();
          // address is of the form ipv4:port
          // or ipv6:port : so remove everything from last ':' onward
          int lastColonIndex = ipAddress.LastIndexOf(':');
          if (lastColonIndex != -1) {
            return ipAddress.Remove(lastColonIndex, ipAddress.Length - lastColonIndex);
          }
        }
      }
      return null;
    }
#endregion // RequestRemoteIp
#endif // NSERVICEKIT
  }
}