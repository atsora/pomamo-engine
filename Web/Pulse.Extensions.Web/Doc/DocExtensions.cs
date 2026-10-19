// Copyright (C) 2026 Atsora Solutions

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lemoine.Core.Log;
using Lemoine.Extensions.Web.Doc;

namespace Pulse.Extensions.Web.Doc
{
  /// <summary>
  /// Get the <see cref="IDocExtension"/> implementations that manage a document path
  /// </summary>
  public static class DocExtensions
  {
    static readonly ILog log = LogManager.GetLogger (typeof (DocExtensions).FullName);

    /// <summary>
    /// Get the <see cref="IDocExtension"/> implementations that manage the specified path,
    /// the one with the highest score first
    /// </summary>
    /// <param name="path">not null and not empty</param>
    /// <returns>not null, empty if no implementation manages the path</returns>
    public static async Task<IList<IDocExtension>> GetExtensionsAsync (string path)
    {
      if (string.IsNullOrEmpty (path)) {
        log.Fatal ("GetExtensionsAsync: path is null or empty");
        throw new ArgumentNullException (nameof (path));
      }

      var request = new Lemoine.Business.Extension.ParameterExtensions<IDocExtension, string> (
        path, x => x, (extension, x) => extension.Initialize (x));
      return (await Lemoine.Business.ServiceProvider.GetAsync (request))
        .OrderByDescending (extension => extension.Score)
        .ToList ();
    }
  }
}
