// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lemoine.Plugin.AcquisitionDelayEvent
{
  public class EventExtension : Pulse.Extensions.Database.IEventExtension
  {
    public string TypeText
    {
      get { return "Acquisition delay"; }
    }

    public string Type
    {
      get
      {
        return typeof (EventAcquisitionDelay).Name;
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
