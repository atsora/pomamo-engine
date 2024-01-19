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
using Pulse.Graphql.Type;

namespace Pulse.Graphql.InputType
{
  /// <summary>
  /// Class to update a <see cref="ICncAcquisition"/>
  /// </summary>
  public class UpdateCncAcquisition
  {
    /// <summary>
    /// Check the configuration file exists first
    /// </summary>
    static readonly string CHECK_CONFIG_FILE_KEY = "Graphql.CncAcquisition.CheckConfigFile";
    static readonly bool CHECK_CONFIG_FILE_DEFAULT = true;

    /// <summary>
    /// Activate the new configuration key params
    /// </summary>
    static readonly string KEY_PARAMS_KEY = "Graphql.CncAcquisition.KeyParams";
    static readonly bool KEY_PARAMS_DEFAULT = true;

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
    public CncAcquisitionResponse Update ()
    {
      bool error = false;
      try {
        using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
          using (var transaction = session.BeginTransaction ("UpdateCncAcquisition")) {
            var cncAcquisition = ModelDAOHelper.DAOFactory.CncAcquisitionDAO
              .FindById (this.Id);
            if (cncAcquisition is null) {
              log.Error ($"Update: unknown id {this.Id}");
              throw new Exception ("Invalid cnc acquisition id");
            }
            if (m_cncConfigNameSet) {
              cncAcquisition.ConfigFile = m_cncConfigName + ".xml";
            }
            if (m_parametersSet) {
              if (Lemoine.Info.ConfigSet.LoadAndGet (KEY_PARAMS_KEY, KEY_PARAMS_DEFAULT)) {
                cncAcquisition.ConfigKeyParams = CncConfigParamValueInput.GetKeyParams (this.Parameters);
              }
              else {
                cncAcquisition.ConfigParameters = CncConfigParamValueInput.GetParametersString (this.Parameters);
              }
            }
            var response = new CncAcquisitionResponse (cncAcquisition);
            if ((m_cncConfigNameSet || m_parametersSet)
              && Lemoine.Info.ConfigSet.LoadAndGet (CHECK_CONFIG_FILE_KEY, CHECK_CONFIG_FILE_DEFAULT)
              && !response.CheckParameters (this.Parameters.ToDictionary (x => x.Name, x => x.Value))) {
              log.Error ("CreateCncAcquisition: load error or invalid parameters");
              response.UpdateAborted = true;
              error = true;
              transaction.Rollback ();
              return response;
            }
            ModelDAOHelper.DAOFactory.CncAcquisitionDAO.MakePersistent (cncAcquisition);
            transaction.Commit ();
            return response;
          }
        }
      }
      catch (Exception ex) {
        log.Error ($"Update: exception", ex);
        error = true;
        throw;
      }
      finally {
        if (!error) {
          ConfigUpdater.Notify ();
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
