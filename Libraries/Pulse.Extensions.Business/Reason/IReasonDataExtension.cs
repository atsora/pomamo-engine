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
    /// 
    /// The modification parameter may be used for optimization.
    /// If false is returned, there is no data processing.
    /// </summary>
    /// <param name="reasonSlot"></param>
    /// <param name="modification"></param>
    /// <returns></returns>
    bool DoMerge (IReasonSlot reasonSlot, IModification modification);

    /// <summary>
    /// Merge the data (there is always a new data)
    /// </summary>
    /// <param name="data">Existing data in ReasonSlot</param>
    /// <param name="modification">modification</param>
    /// <param name="newData"></param>
    void Merge (IDictionary<string, object> data, object newData, IModification modification);

    /// <summary>
    /// Keep the old value if it is not present in the modification
    /// 
    /// The modification parameter may be used for optimization.
    /// If true is returned, there is no specific data processing.
    /// </summary>
    /// <param name="reasonSlot"></param>
    /// <param name="modification"></param>
    bool Keep (IReasonSlot reasonSlot, IModification modification);

    /// <summary>
    /// Initialize the extension. Do not activate it if false
    /// </summary>
    /// <returns></returns>
    bool Initialize ();
  }
}
