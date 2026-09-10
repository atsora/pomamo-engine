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
        using (var image = Load (name)) {
          imageList.Images.Add ($"{name}.png", image);
        }
      }
      if (includeSearch) {
        using (var image = Load ("zoom")) {
          imageList.Images.Add ("zoom.png", image);
        }
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
