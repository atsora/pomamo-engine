﻿// Copyright (C) 2009-2023 Lemoine Automation Technologies
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
  /// Class to update a <see cref="ICncAcquisition"/>
  /// </summary>
  public class UpdateCncAcquisition
  {
    readonly ILog log = LogManager.GetLogger<UpdateCncAcquisition> ();

    string? m_cncConfigName = null;
    bool m_cncConfigNameSet = false;
    IList<CncConfigParamValueInput>? m_parameters = null;
    bool m_parametersSet = false;

    /// <summary>
    /// Constructor
    /// </summary>
    public UpdateCncAcquisition ()
    {
    }

    /// <summary>
    /// Id
    /// </summary>
    public int Id { get; set; } = 0;

    /// <summary>
    /// Cnc config name
    /// </summary>
    public string CncConfigName
    {
      get => m_cncConfigName ?? throw new NullReferenceException ();
      set {
        m_cncConfigName = value;
        m_cncConfigNameSet = true;
      }
    }

    /// <summary>
    /// Parameters
    /// </summary>
    public IList<CncConfigParamValueInput> Parameters
    {
      get => m_parameters ?? throw new NullReferenceException ();
      set {
        m_parameters = value;
        m_parametersSet = true;
      }
    }

    /// <summary>
    /// Create a new cnc acquisition
    /// </summary>
    /// <returns></returns>
    public ICncAcquisition Update ()
    {
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (var transaction = session.BeginTransaction ("CreateCncAcquisition")) {
          var cncAcquisition = ModelDAOHelper.DAOFactory.CncAcquisitionDAO
            .FindById (this.Id);
          if (m_cncConfigNameSet) {
            cncAcquisition.ConfigFile = m_cncConfigName + ".xml";
          }
          if (m_parametersSet) {
            cncAcquisition.ConfigParameters = CncConfigParamValueInput.GetParametersString (this.Parameters);
          }
          transaction.Commit ();
          return cncAcquisition;
        }
      }
    }
  }

  /// <summary>
  /// Input graphql type for a new cnc acquisition
  /// </summary>
  public class UpdateCncAcquisitionInputType : InputObjectGraphType<UpdateCncAcquisition>
  {
    readonly ILog log = LogManager.GetLogger (typeof (UpdateCncAcquisitionInputType).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public UpdateCncAcquisitionInputType ()
    {
      Name = "UpdateCncAcquisition";
      Field<NonNullGraphType<IdGraphType>> ("id");
      Field<string> ("cncConfigName", nullable: true);
      Field<ListGraphType<NonNullGraphType<CncConfigParamValueInputType>>> ("parameters");
    }
  }
}