// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Log;
#if NSERVICEKIT
using NServiceKit.ServiceHost;
#else // !NSERVICEKIT
using Lemoine.Extensions.Web.Attributes;
using Lemoine.Extensions.Web.Interfaces;
#endif // NSERVICEKIT

namespace Pulse.Web.CommonResponseDTO
{
  /// <summary>
  /// Response DTO for Operation
  /// </summary>
  [Api("Operation Response DTO")]
  public class OperationDTO
  {
    /// <summary>
    /// Id of machine
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// Display of machine
    /// </summary>
    public string Display { get; set; }
    
    /// <summary>
    /// Documentation path
    /// </summary>
    public string DocumentLink { get; set; }
    
    /// <summary>
    /// Machining duration in seconds. Not available in all the services
    /// </summary>
    public int? MachiningDuration { get; set; }
    
    /// <summary>
    /// Unloading+Loading duration in seconds. Now available in all the services
    /// </summary>
    public int? UnloadingLoadingDuration { get; set; }
  }
  
  /// <summary>
  /// Assembler for OperationDTO
  /// </summary>
  public class OperationDTOAssembler : IGenericDTOAssembler<OperationDTO, Lemoine.Model.IOperation>
  {
    readonly ILog log = LogManager.GetLogger<OperationDTOAssembler> ();

    /// <summary>
    /// OperationDTO assembler
    /// </summary>
    /// <param name="operation">nullable</param>
    /// <returns></returns>
    public OperationDTO Assemble(Lemoine.Model.IOperation operation)
    {
      if (null == operation) {
        return null;
      }
      if (!Lemoine.ModelDAO.ModelDAOHelper.DAOFactory.IsInitialized (operation)) {
        using (var session = Lemoine.ModelDAO.ModelDAOHelper.DAOFactory.OpenSession ()) {
          using (var transaction = session.BeginReadOnlyTransaction ("Web.OperationDTO.Assemble")) {
            var initialized = Lemoine.ModelDAO.ModelDAOHelper.DAOFactory.OperationDAO
              .FindById (((Lemoine.Collections.IDataWithId<int>)operation).Id);
            if (null == initialized) {
              log.Error ($"Assemble: operation with id {((Lemoine.Collections.IDataWithId<int>)operation).Id} does not exist");
              return null;
            }
            else {
              return Assemble (initialized);
            }
          }
        }
      }
      OperationDTO operationDTO = new OperationDTO();
      operationDTO.Id = ((Lemoine.Collections.IDataWithId<int>)operation).Id;
      operationDTO.Display = operation.Display;
      operationDTO.DocumentLink = operation.DocumentLink;
      return operationDTO;
    }
    
    /// <summary>
    /// OperationDTO assembler for long display
    /// </summary>
    /// <param name="operation">nullable</param>
    /// <returns></returns>
    public OperationDTO AssembleLong(Lemoine.Model.IOperation operation)
    {
      if (null == operation) {
        return null;
      }
      if (!Lemoine.ModelDAO.ModelDAOHelper.DAOFactory.IsInitialized (operation)) {
        using (var session = Lemoine.ModelDAO.ModelDAOHelper.DAOFactory.OpenSession ()) {
          using (var transaction = session.BeginReadOnlyTransaction ("Web.OperationDTO.AssembleLong")) {
            var initialized = Lemoine.ModelDAO.ModelDAOHelper.DAOFactory.OperationDAO
              .FindById (((Lemoine.Collections.IDataWithId<int>)operation).Id);
            if (null == initialized) {
              log.Error ($"AssembleLong: operation with id {((Lemoine.Collections.IDataWithId<int>)operation).Id} does not exist");
              return null;
            }
            else {
              return AssembleLong (initialized);
            }
          }
        }
      }
      OperationDTO operationDTO = new OperationDTO();
      operationDTO.Id = ((Lemoine.Collections.IDataWithId<int>)operation).Id;
      operationDTO.Display = operation.LongDisplay;
      operationDTO.DocumentLink = operation.DocumentLink;
      return operationDTO;
    }

    /// <summary>
    /// OperationDTO assembler for long display with the standard durations
    /// </summary>
    /// <param name="operation">nullable</param>
    /// <returns></returns>
    public OperationDTO AssembleLongWithStandardDurations (Lemoine.Model.IOperation operation)
    {
      if (null == operation) {
        return null;
      }
      if (!Lemoine.ModelDAO.ModelDAOHelper.DAOFactory.IsInitialized (operation)) {
        using (var session = Lemoine.ModelDAO.ModelDAOHelper.DAOFactory.OpenSession ()) {
          using (var transaction = session.BeginReadOnlyTransaction ("Web.OperationDTO.AssembleLongWithStandardDurations")) {
            var initialized = Lemoine.ModelDAO.ModelDAOHelper.DAOFactory.OperationDAO
              .FindById (((Lemoine.Collections.IDataWithId<int>)operation).Id);
            if (null == initialized) {
              log.Error ($"AssembleLongWithStandardDurations: operation with id {((Lemoine.Collections.IDataWithId<int>)operation).Id} does not exist");
              return null;
            }
            else {
              return AssembleLongWithStandardDurations (initialized);
            }
          }
        }
      }
      var operationDto = AssembleLong (operation);
      if (null != operationDto) {
        if (operation.MachiningDuration.HasValue) {
          operationDto.MachiningDuration = (int) operation.MachiningDuration.Value.TotalSeconds;
        }
        if (operation.UnloadingDuration.HasValue) {
          if (operation.LoadingDuration.HasValue) {
            operationDto.UnloadingLoadingDuration =
              (int) operation.UnloadingDuration.Value.TotalSeconds
              + (int) operation.LoadingDuration.Value.TotalSeconds;
          }
          else { // no loading duration
            operationDto.UnloadingLoadingDuration = (int) operation.UnloadingDuration.Value.TotalSeconds;
          }
        }
        else if (operation.LoadingDuration.HasValue) {
          operationDto.UnloadingLoadingDuration = (int) operation.LoadingDuration.Value.TotalSeconds;
        }
      }
      return operationDto;
    }
    
    /// <summary>
    /// OperationDTO assembler for short display
    /// </summary>
    /// <param name="operation">nullable</param>
    /// <returns></returns>
    public OperationDTO AssembleShort(Lemoine.Model.IOperation operation)
    {
      if (null == operation) {
        return null;
      }
      if (!Lemoine.ModelDAO.ModelDAOHelper.DAOFactory.IsInitialized (operation)) {
        using (var session = Lemoine.ModelDAO.ModelDAOHelper.DAOFactory.OpenSession ()) {
          using (var transaction = session.BeginReadOnlyTransaction ("Web.OperationDTO.AssembleShort")) {
            var initialized = Lemoine.ModelDAO.ModelDAOHelper.DAOFactory.OperationDAO
              .FindById (((Lemoine.Collections.IDataWithId<int>)operation).Id);
            if (null == initialized) {
              log.Error ($"AssembleShort: operation with id {((Lemoine.Collections.IDataWithId<int>)operation).Id} does not exist");
              return null;
            }
            else {
              return AssembleShort (initialized);
            }
          }
        }
      }
      OperationDTO operationDTO = new OperationDTO();
      operationDTO.Id = ((Lemoine.Collections.IDataWithId<int>)operation).Id;
      operationDTO.Display = operation.ShortDisplay;
      operationDTO.DocumentLink = operation.DocumentLink;
      return operationDTO;
    }
    
    /// <summary>
    /// OperationDTO list assembler (default display)
    /// </summary>
    /// <param name="operations"></param>
    /// <returns></returns>
    public IEnumerable<OperationDTO> Assemble(IEnumerable<Lemoine.Model.IOperation> operations)
    {
      Debug.Assert (null != operations);
      
      IList<OperationDTO> result = new List<OperationDTO> ();
      foreach (var operation in operations) {
        result.Add (Assemble (operation));
      }
      return result;
    }
  }
}
