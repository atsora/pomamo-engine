// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;
using Lemoine.Core.Log;
#if NSERVICEKIT
using NServiceKit.ServiceHost;
#else // !NSERVICEKIT
using Lemoine.Extensions.Web.Attributes;
using Lemoine.Extensions.Web.Interfaces;
#endif // NSERVICEKIT
using Lemoine.Model;
using Pulse.Extensions.Web.Responses;

namespace Pulse.Web.CommonResponseDTO
{
  /// <summary>
  /// Assembler for MachineDTO
  /// </summary>
  public class MachineDTOAssembler
    : IGenericDTOAssembler<MachineDTO, Lemoine.Model.IMachine>
  {
    /// <summary>
    /// MachineDTO assembler
    /// </summary>
    /// <param name="machine"></param>
    /// <returns></returns>
    public MachineDTO Assemble (Lemoine.Model.IMachine machine)
    {
      MachineDTO machineDTO = new MachineDTO (machine);
      return machineDTO;
    }

    /// <summary>
    /// MachineDTO list assembler
    /// </summary>
    /// <param name="machines"></param>
    /// <returns></returns>
    public IEnumerable<MachineDTO> Assemble (IEnumerable<Lemoine.Model.IMachine> machines)
    {
      IList<MachineDTO> result = new List<MachineDTO> ();
      foreach (var machine in machines) {
        result.Add (Assemble (machine));
      }
      return result;
    }
  }
}
