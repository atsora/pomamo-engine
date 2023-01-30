// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;
using Lemoine.Core.Log;

namespace Pulse.Web.WebDataAccess
{
  /// <summary>
  /// Assembler for ConfigDTO.
  /// </summary>
  public class ConfigDTOAssembler
    : IGenericDTOAssembler<ConfigDTO, Lemoine.Model.IConfig>
  {
    /// <summary>
    /// ConfigDTO assembler
    /// </summary>
    /// <param name="config"></param>
    /// <returns></returns>
    public ConfigDTO Assemble (Lemoine.Model.IConfig config)
    {
      ConfigDTO configDTO = new ConfigDTO ();
      configDTO.Id = config.Id;
      configDTO.Version = config.Version;
      configDTO.Key = config.Key;
      configDTO.Description = config.Description;
      configDTO.Value = config.Value;
      configDTO.Active = config.Active;
      return configDTO;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="configs"></param>
    /// <returns></returns>
    public IEnumerable<ConfigDTO> Assemble (IEnumerable<Lemoine.Model.IConfig> configs)
    {
      return configs.Select (x => Assemble (x));
    }
  }
}
