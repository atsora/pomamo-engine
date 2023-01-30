// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.GDBPersistentClasses;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;

namespace Lemoine.Plugin.AcquisitionDelayEvent
{
  public class EventAcquisitionDelayDAO
    : SaveOnlyByMachineNHibernateDAO<EventAcquisitionDelay, IEventMachine, int>
  {
  }
}
