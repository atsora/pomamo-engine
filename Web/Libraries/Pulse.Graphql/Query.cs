// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using GraphQL;
using GraphQL.Types;
using Lemoine.Core.Log;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Pulse.Graphql.Type;

namespace Pulse.Graphql
{
  /// <summary>
  /// GraphQL Query
  /// </summary>
  public class Query : ObjectGraphType
  {
    readonly ILog log = LogManager.GetLogger (typeof (Query).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public Query (DataStructure dataStructure)
    {
      Name = "Query";
      Field<NonNullGraphType<MachineGraphType>, IMachine> ("machine")
        .Argument<NonNullGraphType<IdGraphType>> ("id")
        .Resolve (ctx => GetMachine (ctx.GetArgument<int> ("id")));
      Field<NonNullGraphType<ListGraphType<NonNullGraphType<WorkOrderStatusGraphType>>>> ("workOrderStatuses")
        .Resolve (ctx => ModelDAOHelper.DAOFactory.WorkOrderStatusDAO.FindAll ());
      Field<NonNullGraphType<ListGraphType<NonNullGraphType<ComponentTypeGraphType>>>> ("componentTypes")
        .Resolve (ctx => ModelDAOHelper.DAOFactory.ComponentTypeDAO.FindAll ());
      Field<NonNullGraphType<ListGraphType<NonNullGraphType<OperationTypeGraphType>>>> ("operationTypes")
        .Resolve (ctx => ModelDAOHelper.DAOFactory.OperationTypeDAO.FindAll ());
      if (dataStructure.WorkOrderProjectIsJob) { // Job is top
        Field<NonNullGraphType<ListGraphType<NonNullGraphType<WorkInfoGraphType>>>, IList<IJob>> ("topWorkInfos")
          .Resolve (ctx => ModelDAOHelper.DAOFactory.JobDAO.FindAll ());
      }
      else if (dataStructure.WorkOrderIsTop) {
        Field<NonNullGraphType<ListGraphType<NonNullGraphType<WorkInfoGraphType>>>, IList<IWorkOrder>> ("topWorkInfos")
          .Resolve (ctx => ModelDAOHelper.DAOFactory.WorkOrderDAO.FindAll ());
      }
      else if (dataStructure.ProjectComponentIsPart) { // part
        Field<NonNullGraphType<ListGraphType<NonNullGraphType<WorkInfoGraphType>>>, IList<IPart>> ("topWorkInfos")
          .Resolve (ctx => ModelDAOHelper.DAOFactory.PartDAO.FindAll ());
      }
      else { // project 
        Field<NonNullGraphType<ListGraphType<NonNullGraphType<WorkInfoGraphType>>>, IList<IProject>> ("topWorkInfos")
          .Resolve (ctx => ModelDAOHelper.DAOFactory.ProjectDAO.FindAll ());
      }
      Field<NonNullGraphType<WorkOrderGraphType>> ("workOrder")
        .Argument<NonNullGraphType<IdGraphType>> ("id")
        .Resolve (ctx => GetWorkOrder (ctx.GetArgument<int> ("id")));
      Field<NonNullGraphType<JobGraphType>> ("job")
        .Argument<NonNullGraphType<IdGraphType>> ("id")
        .Resolve (ctx => GetJob (ctx.GetArgument<int> ("id")));
      Field<NonNullGraphType<ComponentGraphType>> ("component")
        .Argument<NonNullGraphType<IdGraphType>> ("id")
        .Resolve (ctx => GetComponent (ctx.GetArgument<int> ("id")));
      Field<NonNullGraphType<PartGraphType>> ("part")
        .Argument<NonNullGraphType<IdGraphType>> ("id")
        .Resolve (ctx => GetPart (ctx.GetArgument<int> ("id")));
      Field<NonNullGraphType<IntermediateWorkPieceGraphType>> ("intermediateWorkPiece")
        .Argument<NonNullGraphType<IdGraphType>> ("id")
        .Resolve (ctx => GetIntermediateWorkPiece (ctx.GetArgument<int> ("id")));
      Field<NonNullGraphType<OperationGraphType>> ("operation")
        .Argument<NonNullGraphType<IdGraphType>> ("id")
        .Resolve (ctx => GetOperation (ctx.GetArgument<int> ("id")));
      Field<NonNullGraphType<OperationGraphType>> ("operationRevision")
        .Argument<NonNullGraphType<IdGraphType>> ("id")
        .Resolve (ctx => GetOperationRevision (ctx.GetArgument<int> ("id")));
      Field<NonNullGraphType<OperationGraphType>> ("operationModel")
        .Argument<NonNullGraphType<IdGraphType>> ("id")
        .Resolve (ctx => GetOperationModel (ctx.GetArgument<int> ("id")));
      Field<NonNullGraphType<ListGraphType<NonNullGraphType<SequenceGraphType>>>> ("sequences")
        .Argument<NonNullGraphType<IdGraphType>> ("operationModelId")
        .Argument<IntGraphType> ("pathNumber")
        .Resolve (ctx => GetSequences (ctx.GetArgument<int> ("operationModelId"), ctx.GetArgument<int?> ("pathNumber")));
      Field<NonNullGraphType<ListGraphType<NonNullGraphType<CncConfigGraphType>>>, IEnumerable<CncConfig>> ("cncConfigs")
        .Resolve (ctx => GetCncConfigs ());
      Field<NonNullGraphType<CncConfigGraphType>, CncConfig> ("cncConfig")
        .Argument<NonNullGraphType<StringGraphType>> ("name")
        .Resolve (ctx => GetCncConfig (ctx.GetArgument<string> ("name")));
    }

    IMachine GetMachine (int machineId)
    {
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        var machine = ModelDAOHelper.DAOFactory.MachineDAO
          .FindById (machineId);
        if (machine is null) {
          log.Error ($"GetMachine: no machine with id {machineId}");
          throw new DataProcessingException ("No machine with the specified id", Lemoine.Extensions.Web.Responses.ErrorStatus.WrongRequestParameter);
        }
        return machine;
      }
    }

    IWorkOrder GetWorkOrder (int workOrderId)
    {
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        var workOrder = ModelDAOHelper.DAOFactory.WorkOrderDAO
          .FindById (workOrderId);
        if (workOrder is null) {
          log.Error ($"GetWorkOrder: no work order with id {workOrderId}");
          throw new DataProcessingException ("No work order with the specified id", Lemoine.Extensions.Web.Responses.ErrorStatus.WrongRequestParameter);
        }
        return workOrder;
      }
    }

    IJob GetJob (int jobId)
    {
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        var job = ModelDAOHelper.DAOFactory.JobDAO
          .FindById (jobId);
        if (job is null) {
          log.Error ($"GetJob: no job with id {jobId}");
          throw new DataProcessingException ("No job with the specified id", Lemoine.Extensions.Web.Responses.ErrorStatus.WrongRequestParameter);
        }
        return job;
      }
    }

    IComponent GetComponent (int componentId)
    {
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        var component = ModelDAOHelper.DAOFactory.ComponentDAO
          .FindById (componentId);
        if (component is null) {
          log.Error ($"GetComponent: no component with id {componentId}");
          throw new DataProcessingException ("No component with the specified id", Lemoine.Extensions.Web.Responses.ErrorStatus.WrongRequestParameter);
        }
        return component;
      }
    }

    IPart GetPart (int partId)
    {
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        var part = ModelDAOHelper.DAOFactory.PartDAO
          .FindById (partId);
        if (part is null) {
          log.Error ($"GetPart: no part with id {partId}");
          throw new DataProcessingException ("No part with the specified id", Lemoine.Extensions.Web.Responses.ErrorStatus.WrongRequestParameter);
        }
        return part;
      }
    }

    IIntermediateWorkPiece GetIntermediateWorkPiece (int intermediateWorkPieceId)
    {
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        var intermediateWorkPiece = ModelDAOHelper.DAOFactory.IntermediateWorkPieceDAO
          .FindById (intermediateWorkPieceId);
        if (intermediateWorkPiece is null) {
          log.Error ($"GetIntermediateWorkPiece: no part with id {intermediateWorkPieceId}");
          throw new DataProcessingException ("No intermediate work piece with the specified id", Lemoine.Extensions.Web.Responses.ErrorStatus.WrongRequestParameter);
        }
        return intermediateWorkPiece;
      }
    }

    IOperation GetOperation (int operationId)
    {
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        var part = ModelDAOHelper.DAOFactory.OperationDAO
          .FindById (operationId);
        if (part is null) {
          log.Error ($"GetOperation: no operation with id {operationId}");
          throw new DataProcessingException ("No operation with the specified id", Lemoine.Extensions.Web.Responses.ErrorStatus.WrongRequestParameter);
        }
        return part;
      }
    }

    IOperationRevision GetOperationRevision (int operationRevisionId)
    {
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        var operationRevision = ModelDAOHelper.DAOFactory.OperationRevisionDAO
          .FindById (operationRevisionId);
        if (operationRevision is null) {
          log.Error ($"GetOperationRevision: no operationRevision with id {operationRevisionId}");
          throw new DataProcessingException ("No operationRevision with the specified id", Lemoine.Extensions.Web.Responses.ErrorStatus.WrongRequestParameter);
        }
        return operationRevision;
      }
    }

    IOperationModel GetOperationModel (int operationModelId)
    {
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        var operationModel = ModelDAOHelper.DAOFactory.OperationModelDAO
          .FindById (operationModelId);
        if (operationModel is null) {
          log.Error ($"GetOperationModel: no operationModel with id {operationModelId}");
          throw new DataProcessingException ("No operationModel with the specified id", Lemoine.Extensions.Web.Responses.ErrorStatus.WrongRequestParameter);
        }
        return operationModel;
      }
    }

    IEnumerable<ISequence> GetSequences (int operationModelId, int? pathNumber)
    {
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        var operationModel = GetOperationModel (operationModelId);
        return operationModel.GetSequences (pathNumber);
      }
    }

    IEnumerable<CncConfig> GetCncConfigs ()
    {
      return Lemoine.FileRepository.FileRepoClient.ListFilesInDirectory ("cncconfigs")
        .Select (x => new CncConfig (x));
    }

    CncConfig GetCncConfig (string name) => new CncConfig (name + ".xml");
  }
}
