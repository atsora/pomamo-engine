// Copyright (C) 2025 Atsora Solutions

using Lemoine.Core.Log;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Lemoine.IO
{
  /// <summary>
  /// Extension to System.IO.File
  /// </summary>
  public class File
  {
    static readonly ILog slog = LogManager.GetLogger (typeof (File).FullName);

    /// <summary>
    /// Is the file ready for reading (not locked by another process)
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static bool IsFileReady (string path)
    {
      try {
        using (var stream = System.IO.File.Open (path, FileMode.Open, FileAccess.Read, FileShare.None)) {
          return stream.Length > 0; // file is ready
        }
      }
      catch (IOException) { // File is still locked
        return false;
      }
    }

    /// <summary>
    /// Wait for a file to be ready for reading
    /// </summary>
    /// <param name="path"></param>
    /// <param name="cancellationToken"></param>
    /// <param name="sleepTimeMs"></param>
    /// <returns>false if it was interrupted by cancellationToken, true if the file is ready</returns>
    public static bool WaitForFileReady (string path, CancellationToken cancellationToken, int sleepTimeMs = 500)
    {
      while (!cancellationToken.IsCancellationRequested) {
        if (IsFileReady (path)) {
          return true;
        }
        Lemoine.Threading.WaitMethods.Sleep (TimeSpan.FromMilliseconds (sleepTimeMs), cancellationToken);
      }
      return false;
    }

    /// <summary>
    /// Wait for a file to be ready for reading
    /// </summary>
    /// <param name="path"></param>
    /// <param name="cancellationToken"></param>
    /// <param name="timeout"></param>
    /// <param name="sleepTimeMs"></param>
    /// <returns>false if it was interrupted by cancellationToken or the timeout was reached, true is the file is ready</returns>
    public static bool WaitForFileReady (string path, CancellationToken cancellationToken, TimeSpan timeout, int sleepTimeMs = 500)
    {
      if (Lemoine.Threading.WaitMethods.Sleep (timeout, cancellationToken, () => IsFileReady (path), TimeSpan.FromMilliseconds (sleepTimeMs))) {
        if (slog.IsDebugEnabled) {
          slog.Debug ($"WaitForFileReady: timeout reached for {path}");
        }
        return false;
      }
      else {
        if (cancellationToken.IsCancellationRequested) {
          if (slog.IsDebugEnabled) {
            slog.Debug ($"WaitForFileReady: cancellation requested for {path}");
          }
          return false;
        }
        else {
          if (slog.IsDebugEnabled) {
            slog.Debug ($"WaitForFileReady: file {path} is ready");
          }
          return true;
        }
      }
    }
  }
}
