// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

#if USE_FLUENT_NHIBERNATE

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Log;
using Lemoine.GDBPersistentClasses;

namespace Lemoine.Plugin.AcquisitionErrorEvent
{
  /// <summary>
  /// EventAcquisitionErrorMapping
  /// </summary>
  public class EventAcquisitionErrorMap: FluentNHibernate.Mapping.SubclassMap<EventAcquisitionError>
  {
    readonly ILog log = LogManager.GetLogger (typeof (EventAcquisitionErrorMap).FullName);

    public EventAcquisitionErrorMap ()
    {
      Extends<EventMachineGeneric> ();
      DiscriminatorValue ("EventAcquisitionError");
    }
  }
}

#endif // USE_FLUENT_NHIBERNATE
