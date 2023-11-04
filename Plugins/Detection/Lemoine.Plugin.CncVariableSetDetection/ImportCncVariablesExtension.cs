// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using Lemoine.Extensions.Cnc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lemoine.Model;
using Pulse.CncExtensionImpl;

namespace Lemoine.Plugin.CncVariableSetDetection
{
  public class ImportCncVariablesExtension : MachineModuleDetectionVariableList<Configuration>
    , IImportCncVariablesExtension
  {
    protected override bool IsMachineFilterRequired ()
    {
      return false;
    }
  }
}
