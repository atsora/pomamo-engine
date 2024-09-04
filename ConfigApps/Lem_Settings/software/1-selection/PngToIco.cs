// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace Lem_Settings
{
  /// <summary>
  /// Provides helper methods for imaging
  /// https://gist.github.com/darkfall/1656050
  /// 
  /// Note:
  /// Because it uses some platform extensions (System.Drawing),
  /// this can't be put in Lemoine.Core.
  /// 
  /// It is only used in Lem_Settings for now, so it can remain here.
  /// 
  /// If in the future, it needs to be used in some other applications,
  /// then it can probably be moved in Lemoine.Mswin.dll
  /// </summary>
  public static class PngToIco
  {
    /// <summary>
    /// Converts a PNG image to an icon (ico)
    /// </summary>
    /// <param name="input">The input stream</param>
    /// <param name="output">The output stream</param>
    /// <returns>Whether or not the icon was succesfully generated</returns>
    public static bool ConvertToIcon (Stream input, Stream output)
    {
      // Prepare a binary writer and the input bitmap
      var inputBitmap = (Bitmap)Bitmap.FromStream (input);
      var iconWriter = new BinaryWriter (output);
      if (inputBitmap == null || output == null || iconWriter == null) {
        return false;
      }

      // Convert the input bitmap into several sizes, in memory streams
      var sizes = new[] { 16, 32, 48, 64, 128 };
      var memoryStreams = new Dictionary<int, MemoryStream> ();
      foreach (int size in sizes) {
        memoryStreams[size] = new MemoryStream ();
        var bitmap = new Bitmap (inputBitmap, new Size (size, size));
        bitmap.Save (memoryStreams[size], ImageFormat.Png);
      }

      // Write the header
      iconWriter.Write ((short)0); // 0-1 reserved, 0
      iconWriter.Write ((short)1); // 2-3 image type, 1 = icon, 2 = cursor
      iconWriter.Write ((short)(memoryStreams.Count)); // 4-5 number of images

      // Write the image information
      int pos = 6 + sizes.Length * 16;
      foreach (int size in sizes) {
        // 0-1 image width and height
        iconWriter.Write ((byte)size);
        iconWriter.Write ((byte)size);

        // 2 number of colors
        iconWriter.Write ((byte)0); // 0: no color palette

        // 3 reserved
        iconWriter.Write ((byte)0);

        // 4-5 color planes
        iconWriter.Write ((short)0);

        // 6-7 bits per pixel
        iconWriter.Write ((short)32);

        // 8-11 size of image data
        int length = (int)memoryStreams[size].Length;
        iconWriter.Write ((int)length);

        // 12-15 offset of image data
        iconWriter.Write ((int)pos);

        pos += length;
      }

      // Store the images
      foreach (int size in sizes) {
        iconWriter.Write (memoryStreams[size].ToArray ());
      }

      iconWriter.Flush ();

      return true;
    }

    /// <summary>
    /// Converts a PNG image to an icon (ico)
    /// </summary>
    /// <param name="inputPath">The input path</param>
    /// <param name="outputPath">The output path</param>
    /// <returns>Whether or not the icon was succesfully generated</returns>
    public static bool ConvertToIcon (string inputPath, string outputPath)
    {
      using (var inputStream = new FileStream (inputPath, FileMode.Open))
      using (var outputStream = new FileStream (outputPath, FileMode.OpenOrCreate)) {
        return ConvertToIcon (inputStream, outputStream);
      }
    }

    /// <summary>
    /// Converts a PNG image to an icon (ico)
    /// </summary>
    /// <param name="image">Image object</param>
    /// <returns>ico byte array / null for error</returns>
    public static byte[] ConvertToIcon (Image image)
    {
      var inputStream = new MemoryStream ();
      image.Save (inputStream, ImageFormat.Png);
      inputStream.Seek (0, SeekOrigin.Begin);
      var outputStream = new MemoryStream ();
      if (!ConvertToIcon (inputStream, outputStream)) {
        return null;
      }
      return outputStream.ToArray ();
    }
  }
}
