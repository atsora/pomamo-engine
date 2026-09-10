// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Drawing;
using System.IO;

namespace Lemoine.BaseControls
{
  /// <summary>
  /// Loads the UI artwork embedded in Lemoine.BaseControls.
  /// </summary>
  public static class BaseControlImages
  {
    public static Image Load (string name)
    {
      var assembly = typeof (BaseControlImages).Assembly;
      var resourceName = $"{assembly.GetName ().Name}.resources.{name}.png";
      using (Stream stream = assembly.GetManifestResourceStream (resourceName)) {
        if (stream is null) {
          throw new InvalidOperationException ($"Embedded image resource not found: {resourceName}");
        }
        using (var image = Image.FromStream (stream)) {
          return new Bitmap (image);
        }
      }
    }

    public static Image Load (string name, Size size)
    {
      using (var image = Load (name)) {
        return new Bitmap (image, size);
      }
    }

    public static Icon LoadIcon (string name)
    {
      var assembly = typeof (BaseControlImages).Assembly;
      var resourceName = $"{assembly.GetName ().Name}.resources.{name}.ico";
      using (Stream stream = assembly.GetManifestResourceStream (resourceName)) {
        if (stream is null) {
          throw new InvalidOperationException ($"Embedded icon resource not found: {resourceName}");
        }
        using (var icon = new Icon (stream)) {
          return (Icon)icon.Clone ();
        }
      }
    }
  }
}
