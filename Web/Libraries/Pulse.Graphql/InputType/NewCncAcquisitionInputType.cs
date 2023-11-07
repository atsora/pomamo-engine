// Copyright (C) 2009-2023 Lemoine Automation Technologies
// Copyright (C) 2023 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using GraphQL;
using GraphQL.Types;
using Lemoine.Collections;
using Lemoine.Core.Log;
using Lemoine.Model;
using Lemoine.ModelDAO;

namespace Pulse.Graphql.InputType
{
  /// <summary>
  /// Class to create a <see cref="ICncAcquisition"/>
  /// </summary>
  public class NewCncAcquisition
  {
    readonly ILog log = LogManager.GetLogger<NewCncAcquisition> ();

    /// <summary>
    /// Constructor
    /// </summary>
    public NewCncAcquisition ()
    {
    }

    /// <summary>
    /// Cnc config name
    /// </summary>
    public string? CncConfigName { get; set; }

    /// <summary>
    /// Parameters
    /// </summary>
    public IList<CncConfigParamValueInput> Parameters { get; set; } = new List<CncConfigParamValueInput> ();

    /// <summary>
    /// Create a new cnc acquisition
    /// </summary>
    /// <returns></returns>
    public ICncAcquisition CreateCncAcquisition ()
    {
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (var transaction = session.BeginTransaction ("CreateCncAcquisition")) {
          var cncAcquisition = ModelDAOHelper.ModelFactory.CreateCncAcquisition ();
          cncAcquisition.ConfigFile = this.CncConfigName + ".xml";
          cncAcquisition.ConfigParameters = CncConfigParamValueInput.GetParametersString (this.Parameters);
          IComputer computer = ModelDAOHelper.DAOFactory.ComputerDAO
            .GetOrCreateLocal ();
          cncAcquisition.Computer = computer;
          ModelDAOHelper.DAOFactory.CncAcquisitionDAO.MakePersistent (cncAcquisition);
          transaction.Commit ();
          return cncAcquisition;
        }
      }
    }
  }

  /// <summary>
  /// Input graphql type for a new cnc acquisition
  /// </summary>
  public class NewCncAcquisitionInputType : InputObjectGraphType<NewCncAcquisition>
  {
    readonly ILog log = LogManager.GetLogger (typeof (NewCncAcquisitionInputType).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public NewCncAcquisitionInputType ()
    {
      Name = "NewCncAcquisition";
      Field<string> ("cncConfigName", nullable: false);
      Field<NonNullGraphType<ListGraphType<NonNullGraphType<CncConfigParamValueInputType>>>> ("parameters");
    }
  }
}
