// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Net;
using System.Net.Sockets;

using Lemoine.Core.Log;

namespace Lemoine.Net
{
  /// <summary>
  /// UDP listener
  /// 
  /// This class is thread safe.
  /// The Start method can (should) be run in a dedicated thread
  /// </summary>
  public class UdpListener
  {
    /// <summary>
    /// Callback delegate
    /// </summary>
    public delegate void Callback (byte[] receivedData);

    volatile bool m_stopRequested = false; // To make it thread safe
    int m_portNumber;
    Callback m_callback;

    static readonly ILog log = LogManager.GetLogger (typeof (UdpListener).FullName);

    /// <summary>
    /// UDP port number to listen
    /// </summary>
    public int PortNumber
    {
      get { return m_portNumber; }
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="portNumber">UDP port number to listen</param>
    /// <param name="callback"></param>
    public UdpListener (int portNumber, Callback callback)
    {
      m_portNumber = portNumber;
      m_callback = callback;
    }

    /// <summary>
    /// Run the UDP listener
    /// </summary>
    public void Run ()
    {
      using (UdpClient udpClient = new UdpClient (m_portNumber)) {
        IPEndPoint groupEndPoint = new IPEndPoint (IPAddress.Any, m_portNumber);
        while (!m_stopRequested) {
          try {
            byte[] dataReceived = udpClient.Receive (ref groupEndPoint);
            if (log.IsTraceEnabled) {
              for (var i = 0; i < dataReceived.Length; ++i) {
                log.Trace ($"Run: received [{i}]={dataReceived[i]}");
              }
            }
            m_callback (dataReceived);
          }
          catch (Exception ex) {
            log.Error ("Run: Received return and exception but try to continue", ex);
          }
        }
      }
    }

    /// <summary>
    /// Try to ask the Run method to stop (for example to end the thread)
    /// </summary>
    public void Stop ()
    {
      m_stopRequested = true;
    }
  }
}
