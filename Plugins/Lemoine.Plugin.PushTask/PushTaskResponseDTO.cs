// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using Lemoine.Model;
using Pulse.Extensions.Web.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lemoine.Plugin.PushTask
{
#if NSERVICEKIT
  [NServiceKit.ServiceHost.Api ("PushTask Response DTO")]
#endif // NSERVICEKIT
  public class PushTaskResponseDTO : NewModificationsDTO
  {
    public PushTaskResponseDTO (IRevision revision)
      : base (revision)
    {
    }
  }
}
