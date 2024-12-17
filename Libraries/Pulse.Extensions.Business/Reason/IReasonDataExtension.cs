// Copyright (C) 2024 Atsora Solutions

using Lemoine.Extensions;
using Lemoine.Extensions.Extension.Categorized;
using Lemoine.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pulse.Extensions.Business.Reason
{
  /// <summary>
  /// Extension to add additional data to ReasonSlot and ReasonMachineModification
  /// </summary>
  public interface IReasonDataExtension: INamed, IExtension
  {
    /// <summary>
    /// Reason data type (for Json deserialization)
    /// </summary>
    Type ReasonDataType { get; }

    /// <summary>
    /// Keep the data for the specified machine modifications when no data is present in the machine modification
    /// </summary>
    IEnumerable<string> ModificationsToKeep { get; }

    /// <summary>
    /// Apply the reset method on the Reason Data when reset reason is requested
    /// </summary>
    /// <param name="reasonSlot"></param>
    /// <returns></returns>
    bool DoReset (IReasonSlot reasonSlot);

    /// <summary>
    /// Make changes in reason slot data when reset reason is requested
    /// </summary>
    /// <param name="reasonSlot"></param>
    void Reset (IDictionary<string, object> data);

    /// <summary>
    /// Apply the merge method on the reason data
    /// </summary>
    /// <param name="reasonSlot"></param>
    /// <param name="reasonMachineAssociation"></param>
    /// <returns></returns>
    bool DoMerge (IReasonSlot reasonSlot, IReasonMachineAssociation reasonMachineAssociation);

    /// <summary>
    /// Merge the data
    /// </summary>
    /// <param name="reasonSlot"></param>
    /// <param name="machineModification"></param>
    /// <param name="newData"></param>
    void Merge (IDictionary<string, object> data, object newData);

    /// <summary>
    /// Initialize the extension. Do not activate it if false
    /// </summary>
    /// <returns></returns>
    bool Initialize ();
  }
}
