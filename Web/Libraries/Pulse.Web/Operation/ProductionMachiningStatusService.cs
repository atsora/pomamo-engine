// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Business.Operation;
using Lemoine.Core.Log;
using Lemoine.Extensions.Business;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Pulse.Web.CommonResponseDTO;
using Lemoine.Extensions.Web.Responses;
using System.Threading;
using Lemoine.Web;
using Pulse.Extensions.Web;

namespace Pulse.Web.Operation
{
  /// <summary>
  /// Description of XxxService
  /// </summary>
  public class ProductionMachiningStatusService
    : GenericCachedService<ProductionMachiningStatusRequestDTO>
  {
    static readonly string CURRENT_MACHINE_MODE_VALIDITY_KEY = "Web.Operation.ProductionMachiningStatus.CurrentMachineModeValidity";
    static readonly TimeSpan CURRENT_MACHINE_MODE_VALIDITY_DEFAULT = TimeSpan.FromSeconds (20);

    static readonly ILog log = LogManager.GetLogger (typeof (ProductionMachiningStatusService).FullName);

    readonly IDictionary<int, IEnumerable<IProductionMachiningStatusExtension>> m_extensions = new Dictionary<int, IEnumerable<IProductionMachiningStatusExtension>> ();
    readonly SemaphoreSlim m_semaphore = new SemaphoreSlim (1, 1);

    /// <summary>
    /// 
    /// </summary>
    public ProductionMachiningStatusService ()
      : base (Lemoine.Core.Cache.CacheTimeOut.CurrentShort)
    {
    }

    /// <summary>
    /// Response to GET request (no cache)
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public override object GetWithoutCache (ProductionMachiningStatusRequestDTO request)
    {
      Debug.Assert (null != request);

      if (0 < request.MachineId) {
        return GetByMachine (request);
      }
      else {
        return GetByGroup (request);
      }
    }

    /// <summary>
    /// Response to GET request (no cache)
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    object GetByMachine (ProductionMachiningStatusRequestDTO request)
    {
      Debug.Assert (null != request);

      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        var machine = ModelDAOHelper.DAOFactory.MonitoredMachineDAO.FindById (request.MachineId);
        if (null == machine) {
          if (log.IsErrorEnabled) {
            log.ErrorFormat ("GetWithoutCache: machine with id {0} is not a monitored machine",
              request.MachineId);
          }
          return new ErrorDTO ("Invalid machine", ErrorStatus.WrongRequestParameter);
        }

        var responseDto = new ProductionMachiningStatusResponseDTO ();

        IPartProductionCurrentShiftResponse partProductionCurrentShiftResponse;
        if (ProductionMachiningOption.TrackManufacturingOrder == request.Option) {
          var businessRequest = new Lemoine.Business.Operation
            .PartProductionCurrentShiftManufacturingOrder (machine);
          var businessResponse = Lemoine.Business.ServiceProvider
            .Get (businessRequest);

          responseDto.GoalNowShift = businessResponse.GoalCurrentShift;
          responseDto.GoalNowGlobal = businessResponse.GoalWholeManufacturingOrder;
          responseDto.NbPiecesDoneDuringShift = businessResponse.NbPiecesCurrentShift;
          responseDto.NbPiecesDoneGlobal = businessResponse.NbPiecesWholeManufacturingOrder;

          var manufacturingOrder = Initialize<IManufacturingOrder> (businessResponse.ManufacturingOrder, ModelDAOHelper.DAOFactory.ManufacturingOrderDAO.FindById);
          var workOrder = Initialize<IWorkOrder> (businessResponse.WorkOrder, ModelDAOHelper.DAOFactory.WorkOrderDAO.FindById);
          var component = Initialize<IComponent> (businessResponse.Component, ModelDAOHelper.DAOFactory.ComponentDAO.FindById);
          var operation = Initialize<IOperation> (businessResponse.Operation, ModelDAOHelper.DAOFactory.OperationDAO.FindById);

          if (null != operation) {
            responseDto.Operation = new OperationDTOAssembler ()
              .AssembleLong (operation);
          }
          if (null != component) {
            responseDto.Component = new ComponentDTOAssembler ()
              .Assemble (component);
          }
          if (null != workOrder) {
            responseDto.WorkOrder = new WorkOrderDTOAssembler ()
              .Assemble (workOrder);
          }
          if (null != manufacturingOrder) {
            responseDto.ManufacturingOrder = new ManufacturingOrderDTOAssembler ()
              .Assemble (manufacturingOrder);
          }

          // - Work information (deprecated)
          responseDto.WorkInformations = new List<WorkInformationDTO> ();
          if (null != workOrder) {
            responseDto.WorkInformations.Add (new WorkInformationDTO (WorkInformationKind.WorkOrder,
              workOrder.Display));
          }
          if (null != component) {
            responseDto.WorkInformations.Add (new WorkInformationDTO (WorkInformationKind.Component,
              component.Display));
          }
          if (null != operation) {
            responseDto.WorkInformations.Add (new WorkInformationDTO (WorkInformationKind.Operation,
              operation.Display));
          }

          // - Day/shift
          if (businessResponse.Day.HasValue) {
            responseDto.Day = ConvertDTO.DayToIsoString (businessResponse.Day.Value);
          }
          if (null != businessResponse.Shift) {
            responseDto.Shift = new ShiftDTOAssembler ().Assemble (businessResponse.Shift);
          }
          if (!businessResponse.Range.IsEmpty ()) {
            responseDto.Range = businessResponse.Range.ToString (x => ConvertDTO.DateTimeUtcToIsoString (x));
          }

          partProductionCurrentShiftResponse = businessResponse;
        }
        else {
          var businessRequest = new Lemoine.Business.Operation
            .PartProductionCurrentShiftOperation (machine);
          var businessResponse = Lemoine.Business.ServiceProvider
            .Get (businessRequest);

          responseDto.GoalNowShift = businessResponse.GoalCurrentShift;
          responseDto.NbPiecesDoneDuringShift = businessResponse.NbPiecesCurrentShift;

          var component = Initialize<IComponent> (businessResponse.Component, ModelDAOHelper.DAOFactory.ComponentDAO.FindById);
          var operation = Initialize<IOperation> (businessResponse.Operation, ModelDAOHelper.DAOFactory.OperationDAO.FindById);

          if (null != operation) {
            responseDto.Operation = new OperationDTOAssembler ()
              .AssembleLong (operation);
          }
          if (null != component) {
            responseDto.Component = new ComponentDTOAssembler ()
              .Assemble (component);
          }

          // - Work information (deprecated)
          responseDto.WorkInformations = new List<WorkInformationDTO> ();
          if (null != component) {
            responseDto.WorkInformations.Add (new WorkInformationDTO (WorkInformationKind.Component,
                                                              component.Display));
          }
          if (null != operation) {
            responseDto.WorkInformations.Add (new WorkInformationDTO (WorkInformationKind.Operation,
                                                              operation.Display));
          }

          // - Day/shift
          if (businessResponse.Day.HasValue) {
            responseDto.Day = ConvertDTO.DayToIsoString (businessResponse.Day.Value);
          }
          if (null != businessResponse.Shift) {
            responseDto.Shift = new ShiftDTOAssembler ().Assemble (businessResponse.Shift);
          }
          if (!businessResponse.Range.IsEmpty ()) {
            responseDto.Range = businessResponse.Range.ToString (x => ConvertDTO.DateTimeUtcToIsoString (x));
          }

          partProductionCurrentShiftResponse = businessResponse;
        }

        if (request.IncludeEvents) {
          AddEvents (responseDto, machine, partProductionCurrentShiftResponse);
        }

        return responseDto;
      }
    }

    /// <summary>
    /// Response to GET request (no cache)
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    object GetByGroup (ProductionMachiningStatusRequestDTO request)
    {
      Debug.Assert (null != request);

      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        var groupId = request.GroupId;
        var groupRequest = new Lemoine.Business.Machine.GroupFromId (groupId);
        var group = Lemoine.Business.ServiceProvider
          .Get (groupRequest);

        if (null == group) {
          if (log.IsErrorEnabled) {
            log.ErrorFormat ("GetWithoutCache: group with id {0} is not valid",
              request.GroupId);
          }
          return new ErrorDTO ("Invalid group", ErrorStatus.WrongRequestParameter);
        }

        var responseDto = new ProductionMachiningStatusResponseDTO ();
        responseDto.WorkInformations = new List<WorkInformationDTO> ();

        IPartProductionDataCurrentShift partProductionCurrentShift = null;
        if (null == group.PartProductionCurrentShift) {
          if (log.IsDebugEnabled) {
            log.Debug ($"GetByGroup: no production machining status available for group {group.Name}");
          }
          if (request.IncludeEvents) {
            responseDto.EventsOnly = true;
            // TODO: if this makes sense to return only events, continue (do not return an error status NotApplicable)
            return new ErrorDTO ($"Production machining status is not applicable for group {group.Name}", ErrorStatus.NotApplicable);
          }
          else {
            return new ErrorDTO ($"Production machining status is not applicable for group {group.Name}", ErrorStatus.NotApplicable);
          }
        }
        else { // null != group.PartProductionCurrentShift
          partProductionCurrentShift = group.PartProductionCurrentShift ();
        }
        if (null != partProductionCurrentShift) {
          responseDto.GoalNowShift = partProductionCurrentShift.GoalCurrentShift;
          responseDto.NbPiecesDoneDuringShift = partProductionCurrentShift.NbPiecesCurrentShift;

          var component = Initialize<IComponent> (partProductionCurrentShift.Component, ModelDAOHelper.DAOFactory.ComponentDAO.FindById);
          var operation = Initialize<IOperation> (partProductionCurrentShift.Operation, ModelDAOHelper.DAOFactory.OperationDAO.FindById);

          if (null != operation) {
            responseDto.Operation = new OperationDTOAssembler ()
              .AssembleLong (operation);
          }
          if (null != component) {
            responseDto.Component = new ComponentDTOAssembler ()
              .Assemble (component);
          }

          // - Work information (deprecated)
          if (null != component) {
            responseDto.WorkInformations.Add (new WorkInformationDTO (WorkInformationKind.Component, component.Display));
          }
          if (null != operation) {
            responseDto.WorkInformations.Add (new WorkInformationDTO (WorkInformationKind.Operation, operation.Display));
          }

          // - Day/shift
          if (partProductionCurrentShift.Day.HasValue) {
            responseDto.Day = ConvertDTO.DayToIsoString (partProductionCurrentShift.Day.Value);
          }
          if (null != partProductionCurrentShift.Shift) {
            responseDto.Shift = new ShiftDTOAssembler ().Assemble (partProductionCurrentShift.Shift);
          }
        }

        if (request.IncludeEvents && group.SingleMachine) {
          var firstMachine = group.GetMachines ().First ();
          var monitoredMachine = ModelDAOHelper.DAOFactory.MonitoredMachineDAO
            .FindById (firstMachine.Id);
          if (null != monitoredMachine) {
            AddEvents (responseDto, monitoredMachine, partProductionCurrentShift);
          }
        }

        return responseDto;
      }
    }

    /// <summary>
    /// Initialize an object
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="v"></param>
    /// <param name="findById"></param>
    /// <returns></returns>
    T Initialize<T> (T v, Func<int, T> findById)
      where T : Lemoine.Collections.IDataWithId<int>
    {
      if (null == v) {
        return v;
      }
      if (!Lemoine.ModelDAO.ModelDAOHelper.DAOFactory.IsInitialized (v)) {
        using (var session = Lemoine.ModelDAO.ModelDAOHelper.DAOFactory.OpenSession ()) {
          using (var transaction = session.BeginReadOnlyTransaction ("Web.ProductionMachiningStatus.Initialize")) {
            var initialized = findById (v.Id);
            if (null == initialized) {
              log.Error ($"Initialize: object with type {typeof (T)} and id {v.Id} does not exist");
              return default (T);
            }
            else {
              return initialized;
            }
          }
        }
      }
      return v;
    }

    void AddEvents (ProductionMachiningStatusResponseDTO responseDto, IMonitoredMachine machine, Lemoine.Extensions.Business.IPartProductionDataCurrentShift partProductionCurrentShiftResponse)
    {
      // Events
      var extensions = GetExtensions (machine);
      { // - coming
        IEnumerable<Event> comingEvents = extensions
          .SelectMany (ext => ext.GetComingEvents (partProductionCurrentShiftResponse));
        responseDto.ComingEvents = comingEvents
          .OrderBy (e => e.Severity.LevelValue)
          .OrderBy (e => e.DateTime)
          .Select (e => new EventDTO (e))
          .ToList ();
      }
      { // - active
        IEnumerable<Event> activeEvents = extensions
          .SelectMany (ext => ext.GetActiveEvents (partProductionCurrentShiftResponse));
        responseDto.ActiveEvents = activeEvents
          .OrderBy (e => e.Severity.LevelValue)
          .Select (e => new EventDTO (e))
          .ToList ();
      }

      // In case there is a coming event to display,
      // (and no active event)
      // return if the machine is currently active or not
      if (!responseDto.ActiveEvents.Any ()
        && responseDto.ComingEvents.Any ()) {
        var currentMachineMode = ModelDAOHelper.DAOFactory.CurrentMachineModeDAO
          .FindWithMachineMode (machine);
        if (null != currentMachineMode) {
          var currentMachineModeValidity = Lemoine.Info.ConfigSet.LoadAndGet<TimeSpan> (CURRENT_MACHINE_MODE_VALIDITY_KEY,
            CURRENT_MACHINE_MODE_VALIDITY_DEFAULT);
          if (DateTime.UtcNow <= currentMachineMode.DateTime.Add (currentMachineModeValidity)) {
            responseDto.Running = currentMachineMode.MachineMode.Running;
          }
          else {
            log.WarnFormat ("GetWithoutCache: current machine mode at {0} is too old", currentMachineMode.DateTime);
          }
        }
      }
    }

    IEnumerable<IProductionMachiningStatusExtension> GetExtensions (IMonitoredMachine machine)
    {
      Debug.Assert (null != machine);

      if (m_extensions.ContainsKey (machine.Id)) {
        return m_extensions[machine.Id];
      }
      else {
        IEnumerable<IProductionMachiningStatusExtension> extensions;
        using (var semaphoreHolder = Lemoine.Threading.SemaphoreSlimHolder.Create (m_semaphore)) {
          extensions = Lemoine.Business.ServiceProvider
            .Get (new Lemoine.Business.Extension
            .MonitoredMachineExtensions<IProductionMachiningStatusExtension> (machine, (ext, m) => ext.Initialize (m)));
          m_extensions[machine.Id] = extensions;
        }
        return extensions;
      }
    }
  }
}
