// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Lemoine.Settings
{
  /// <summary>
  /// Loads PNG assets embedded in the assembly that owns a settings item or UI.
  /// </summary>
  public static class EmbeddedImageLoader
  {
    /// <summary>
    /// Load an embedded PNG and detach the returned bitmap from its resource stream.
    /// </summary>
    public static Image Load (Type ownerType, string name)
    {
      var assembly = ownerType.Assembly;
      var suffix = "." + name + ".png";
      var exactName = (ownerType.Namespace ?? assembly.GetName ().Name) + suffix;
      var resourceName = assembly.GetManifestResourceNames ()
        .Where (x => x.EndsWith (suffix, StringComparison.OrdinalIgnoreCase))
        .OrderByDescending (x => string.Equals (x, exactName, StringComparison.OrdinalIgnoreCase))
        .ThenBy (x => x.Length)
        .FirstOrDefault ();

      if (resourceName == null) {
        return null;
      }

      using (Stream stream = assembly.GetManifestResourceStream (resourceName)) {
        if (stream == null) {
          return null;
        }
        using (var image = Image.FromStream (stream)) {
          return new Bitmap (image);
        }
      }
    }

    /// <summary>
    /// Replace a designer image-list stream with explicit 32-bit images.
    /// </summary>
    public static void FillImageList (ImageList imageList, Type ownerType, Size size, params string[] names)
    {
      imageList.Images.Clear ();
      imageList.ColorDepth = ColorDepth.Depth32Bit;
      imageList.ImageSize = size;

      foreach (var name in names) {
        var image = Load (ownerType, name);
        if (image == null) {
          throw new InvalidOperationException ($"Embedded image '{name}.png' was not found for {ownerType.FullName}");
        }
        imageList.Images.Add (name + ".png", image);
      }
    }
  }
}
