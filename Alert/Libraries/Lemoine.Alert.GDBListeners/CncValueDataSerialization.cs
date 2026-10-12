// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.GDBPersistentClasses;
using Lemoine.Model;
using Lemoine.ModelDAO;

namespace Lemoine.Alert.GDBListeners
{
  /// <summary>
  /// Get a CncValue to serialize from a cnc value read from cncvalue or cncdataperiod,
  /// so that the XML of the alerts keeps the same format
  /// </summary>
  internal static class CncValueDataSerialization
  {
    /// <summary>
    /// CncValue to serialize, with its references unproxied
    ///
    /// To run in a session
    /// <item>from cncvalue: the persistent cnc value, reloaded</item>
    /// <item>from cncdataperiod: a transient cnc value (not persisted) with the same properties</item>
    /// </summary>
    /// <param name="data">not null</param>
    /// <returns></returns>
    public static CncValue ToSerializableCncValue (ICncValueData data)
    {
      CncValue cncValue;
      if (CncValueSource.CncValue == data.Source) {
        // Note: Lock does not work here, it generates a message when the Unproxy() method is called.
        //       Because the event data is read only, FindById may be used instead of Lock.
        cncValue = ModelDAOHelper.DAOFactory.CncValueDAO.FindById (data.Id, data.MachineModule) as CncValue;
        if (null == cncValue) {
          return null;
        }
      }
      else {
        var machineModule = ModelDAOHelper.DAOFactory.MachineModuleDAO.FindById (data.MachineModule.Id);
        var field = ModelDAOHelper.DAOFactory.FieldDAO.FindById (data.Field.Id);
        cncValue = (CncValue)ModelDAOHelper.ModelFactory.CreateCncValue (machineModule, field, data.Begin);
        cncValue.End = data.End;
        switch (field.Type) {
        case FieldType.Double:
          cncValue.Double = data.Double;
          break;
        case FieldType.Int32:
        case FieldType.Boolean:
          cncValue.Int = data.Int;
          break;
        default:
          cncValue.String = data.String;
          break;
        }
        cncValue.Deviation = data.Deviation;
        cncValue.Stopped = data.Stopped;
      }
      cncValue.Unproxy ();
      return cncValue;
    }
  }
}
