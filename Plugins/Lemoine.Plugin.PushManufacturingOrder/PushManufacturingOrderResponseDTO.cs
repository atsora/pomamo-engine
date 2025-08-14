// Copyright (C) 2009-2023 Lemoine Automation Technologies
// Copyright (C) 2025 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using Lemoine.Model;
using Pulse.Extensions.Web.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lemoine.Plugin.PushManufacturingOrder
{
  public class PushManufacturingOrderResponseDTO : NewModificationsDTO
  {
    public PushManufacturingOrderResponseDTO (IRevision revision)
      : base (revision)
    {
    }
  }
}
