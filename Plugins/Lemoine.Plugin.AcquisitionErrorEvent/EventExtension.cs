// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lemoine.Plugin.AcquisitionErrorEvent
{
  public class EventExtension : Pulse.Extensions.Database.IEventExtension
  {
    public string TypeText
    {
      get { return "Acquisition error"; }
    }

    public string Type
    {
      get
      {
        return typeof (EventAcquisitionError).Name;
      }
    }

    public bool UniqueInstance
    {
      get
      {
        return true;
      }
    }
  }
}
