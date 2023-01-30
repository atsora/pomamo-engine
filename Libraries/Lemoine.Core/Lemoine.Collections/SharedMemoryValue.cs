// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

#if NETSTANDARD

using System;

using System.Diagnostics;
using Lemoine.Core.Log;

namespace Lemoine.Collections
{
  /// <summary>
  /// Implementation of ISharedValue with SharedMemory.BufferReadWrite
  /// </summary>
  internal sealed class SharedMemoryValue<T>
    : ISharedValue<T>
    where T: struct
  {
    #region Members
    readonly string m_name;
    readonly SharedMemory.BufferReadWrite m_buffer;
    #endregion // Members

    readonly ILog log = LogManager.GetLogger(typeof (SharedMemoryValue<T>).FullName);

    #region Getters / Setters
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    /// <param name="name">not null and empty</param>
    public SharedMemoryValue (string name)
    {
      Debug.Assert (!string.IsNullOrEmpty (name));

      m_name = name;

      try {
        m_buffer = new SharedMemory.BufferReadWrite (name, System.Runtime.InteropServices.Marshal.SizeOf(typeof(T)));
      }
      catch (System.IO.IOException) {
        m_buffer = new SharedMemory.BufferReadWrite (name);
      }
    }
    #endregion // Constructors

    #region ISharedValue implementation
    /// <summary>
    /// 
    /// </summary>
    public T Value {
      get {
        T v;
        m_buffer.Read<T> (out v, 0);
        return v;
      }
      set {
        T v = value;
        m_buffer.Write<T> (ref v, 0);
      }
    }
    #endregion
  }
}

#endif // NETSTANDARD
