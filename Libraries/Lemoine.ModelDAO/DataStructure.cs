// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Log;

namespace Lemoine.Model
{
  /// <summary>
  /// DataStructure
  /// </summary>
  public class DataStructure
  {
    readonly ILog log = LogManager.GetLogger (typeof (DataStructure).FullName);

    /// <summary>
    /// Work order + Project = Job ?
    /// </summary>
    public bool WorkOrderProjectIsJob => Lemoine.Info.ConfigSet.LoadAndGet<bool> (ConfigKeys.GetDataStructureConfigKey (DataStructureConfigKey.WorkOrderProjectIsJob), false);

    /// <summary>
    /// Project + Component = Part ?
    /// </summary>
    public bool ProjectComponentIsPart => Lemoine.Info.ConfigSet.LoadAndGet<bool> (ConfigKeys.GetDataStructureConfigKey (DataStructureConfigKey.ProjectComponentIsPart), true);

    /// <summary>
    /// IntermediateWorkPiece + Operation = Simple operation ?
    /// </summary>
    public bool IntermediateWorkPieceOperationIsSimpleOperation => Lemoine.Info.ConfigSet.LoadAndGet<bool> (ConfigKeys.GetDataStructureConfigKey (DataStructureConfigKey.IntermediateWorkPieceOperationIsSimpleOperation), true);

    /// <summary>
    /// Should the work order be considered as the entry level ? Default is false
    /// </summary>
    public bool WorkOrderIsTop => !Lemoine.Info.ConfigSet.LoadAndGet<bool> (ConfigKeys.GetOperationExplorerConfigKey (OperationExplorerConfigKey.PartAtTheTop), true);

    /// <summary>
    /// Constructor
    /// </summary>
    public DataStructure ()
    {
    }
  }
}
