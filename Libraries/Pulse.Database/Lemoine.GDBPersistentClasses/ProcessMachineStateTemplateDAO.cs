// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using NHibernate;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Implementation of the DAO of ProcessMachineStateTemplate
  /// </summary>
  public class ProcessMachineStateTemplateDAO
    : SaveOnlyByMachineNHibernateDAO<ProcessMachineStateTemplate, IProcessMachineStateTemplate, long>
    , IProcessMachineStateTemplateDAO
  {
  }
}
