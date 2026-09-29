// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace Lemoine.JobControls
{
  static class JobControlImages
  {
    static readonly string[] s_treeImageNames = {
      "workorder",
      "project",
      "component",
      "intermediateworkpiece",
      "operation",
      "job",
      "part",
      "simpleoperation",
      "path",
      "sequence",
    };

    public static void FillTreeImageList (ImageList imageList, bool includeSearch)
    {
      imageList.Images.Clear ();
      foreach (var name in s_treeImageNames) {
        // ImageList keeps the Image instances until its native handle is created.
        // Do not dispose them here: ownership is transferred to the ImageList.
        imageList.Images.Add ($"{name}.png", Load (name));
      }
      if (includeSearch) {
        imageList.Images.Add ("zoom.png", Load ("zoom"));
      }
    }

    static Image Load (string name)
    {
      var assembly = typeof (JobControlImages).Assembly;
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
  }
}
