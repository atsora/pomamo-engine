// Copyright (C) 2009-2023 Lemoine Automation Technologies
// Copyright (C) 2024 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Log;
using Lemoine.Extensions.Business.Group;
using Lemoine.Model;
using Lemoine.ModelDAO;

namespace Lemoine.Plugin.GroupZoomPartOperation
{
  public class GroupZoomExtension
    : Lemoine.Extensions.MultipleInstanceConfigurableExtension<Configuration>
    , IGroupZoomExtension
  {
    readonly ILog log = LogManager.GetLogger (typeof (GroupZoomExtension).FullName);

    Configuration m_configuration;

    public bool Dynamic => false;

    public bool Initialize ()
    {
      if (!LoadConfiguration (out m_configuration)) {
        return false;
      }

      return true;
    }

    public bool ZoomIn (string parentGroupId, out IEnumerable<string> children)
    {
      if (!this.IsGroupIdMatchFromPrefixNumber (m_configuration.PartPrefix, parentGroupId)) {
        if (log.IsDebugEnabled) {
          log.Debug ($"ZoomIn: parentGroupId {parentGroupId} is not a matching group");
        }
        children = new List<string> ();
        return false;
      }

      try {
        var componentId = this.ExtractIdAfterPrefix (m_configuration.PartPrefix, parentGroupId);
        using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
          var part = ModelDAOHelper.DAOFactory.PartDAO
            .FindById (componentId);
          if (part is null) {
            log.Error ($"ZoomIn: part with componentId={componentId} does not exist");
            children = new List<string> ();
            return false;
          }
          children = part.Component.ComponentIntermediateWorkPieces
            .Select (x => x.IntermediateWorkPiece.Operation)
            .Where (x => (null != x))
            .Select (x => $"{m_configuration.OperationPrefix}{x.Id}")
            .Distinct ()
            .ToList ();
          return true;
        }
      }
      catch (Exception ex) {
        log.Error ($"ZoomIn: exception", ex);
        throw;
      }
    }

    public bool ZoomOut (string childGroupId, out string parentGroupId)
    {
      if (!this.IsGroupIdMatchFromPrefixNumber (m_configuration.OperationPrefix, childGroupId)) {
        if (log.IsDebugEnabled) {
          log.Debug ($"ZoomOut: childGroupId {childGroupId} is not a matching group");
        }
        parentGroupId = null;
        return false;
      }

      var operationId = this.ExtractIdAfterPrefix (m_configuration.PartPrefix, childGroupId);
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        var operation = ModelDAOHelper.DAOFactory.OperationDAO
          .FindById (operationId);
        if (operation is null) {
          log.Error ($"ZoomOut: operation with id={operationId} does not exist");
          parentGroupId = null;
          return false;
        }
        var intermediateWorkPieces = operation.IntermediateWorkPieces;
        var intermediateWorkPieceCount = intermediateWorkPieces.Count;
        if (0 == intermediateWorkPieceCount) {
          log.Error ($"ZoomOut: no intermediate work piece is associated to operationId={operationId}");
          parentGroupId = null;
          return false;
        }
        else if (1 < intermediateWorkPieceCount) {
          log.Error ($"ZoomOut: more than one intermediate work piece is associated to operationId={operationId}");
          parentGroupId = null;
          return false;
        }
        else { // 1 == intermediateWorkPieceCount
          var intermediateWorkPiece = intermediateWorkPieces
            .Single ();
          var components = intermediateWorkPiece.ComponentIntermediateWorkPieces
            .Select (x => x.Component);
          if (1 != components.Count ()) {
            log.Error ($"ZoomOut: more than one component is associated to operationId={operationId}");
            parentGroupId = null;
            return false;
          }
          var component = components.Single ();
          parentGroupId = $"{m_configuration.PartPrefix}{component.Id}";
        }
        return true;
      }
    }
  }
}
