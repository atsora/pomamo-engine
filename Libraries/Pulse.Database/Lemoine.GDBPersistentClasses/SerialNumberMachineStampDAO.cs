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
  /// Implementation of <see cref="Lemoine.ModelDAO.ISerialNumberMachineStampDAO">ISerialNumberMachineStampDAO</see>
  /// </summary>
  public class SerialNumberMachineStampDAO
    : SaveOnlyByMachineNHibernateDAO<SerialNumberMachineStamp, ISerialNumberMachineStamp, long>
    , ISerialNumberMachineStampDAO
  {
  }
}
