// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.GDBPersistentClasses;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;

namespace Lemoine.Plugin.MachineStateTemplateChangeEvent
{
  public class EventMachineStateTemplateChangeDAO
    : SaveOnlyByMachineNHibernateDAO<EventMachineStateTemplateChange, IEventMachine, int>
  {
  }
}
