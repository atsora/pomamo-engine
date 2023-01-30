// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Org.BouncyCastle.Asn1;

namespace Lemoine.Model
{
  /// <summary>
  /// Package detail
  /// </summary>
  public class PackageDetail
  {
    /// <summary>
    /// Version of the sequence detail data structure
    /// </summary>
    public int Version { get; set; } = 1;

    /// <summary>
    /// Associated tags
    /// </summary>
    public IList<string> Tags { get; set; } = new List<string> ();
  }

  /// <summary>
  /// Description of IPackage.
  /// </summary>
  public interface IPackage : IVersionable
  {
    /// <summary>
    /// Name
    /// </summary>
    string Name { get; set; }

    /// <summary>
    /// Unique name of the package
    /// </summary>
    string IdentifyingName { get; }

    /// <summary>
    /// Details
    /// </summary>
    string Description { get; set; }

    /// <summary>
    /// Activated
    /// </summary>
    bool Activated { get; set; }

    /// <summary>
    /// Version of the plugin
    /// </summary>
    int NumVersion { get; set; }

    /// <summary>
    /// Package details, including associated tags
    /// </summary>
    PackageDetail Detail { get; set; }
  }

  /// <summary>
  /// Extension to <see cref="IPackage"/>
  /// </summary>
  public static class PackageExtensions
  {
    /// <summary>
    /// Associated tags
    /// </summary>
    /// <param name="package"></param>
    /// <returns></returns>
    public static IList<string> GetTags (this IPackage package) => package.Detail?.Tags ?? new List<string> ();

    /// <summary>
    /// Set tags
    /// </summary>
    /// <param name="package"></param>
    /// <param name="tags"></param>
    public static void SetTags (this IPackage package, IEnumerable<string> tags)
    {
      if (!tags.Any ()) {
        return;
      }
      if (null == package.Detail) {
        package.Detail = new PackageDetail ();
      }
      package.Detail.Tags = tags.ToList ();
    }

    /// <summary>
    /// Add tags
    /// </summary>
    /// <param name="package"></param>
    /// <param name="tags"></param>
    public static void AddTags (this IPackage package, params string[] tags)
    {
      if (null == package.Detail) {
        package.Detail = new PackageDetail ();
      }
      foreach (var tag in tags) {
        package.Detail.Tags.Add (tag);
      }
    }
  }
}
