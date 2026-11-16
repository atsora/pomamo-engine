// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Lemoine.Model;
using Lemoine.ModelDAO;

namespace Lemoine.Business.Operation
{
  /// <summary>
  /// The operation revisions of this major version, until they get a table of their own
  ///
  /// A revision is an operation: the revisions of an operation are the operations that share
  /// its base name, the first one with no suffix or the suffix " - rev 1" (revision 1), the
  /// next ones with the suffix " - rev N" (revision N). The suffix " revN" of the first
  /// revisions that were created is still recognized
  ///
  /// The active revision is the one that is not archived, the highest one if several are
  /// not, the highest one again if all of them are. Between two operations with the same
  /// revision number, the most recent one comes last
  ///
  /// TODO: once the operation revisions have a table of their own, read them from it, and
  /// remove this class
  /// </summary>
  public static class OperationRevisions
  {
    // " - rev N" (spaces optional around the dash and before the number), or the former " revN"
    static readonly Regex REVISION_SUFFIX = new Regex (@"^(?<base>.+?)\s*-\s*rev\s*(?<number>[0-9]+)$|^(?<base>.+) rev(?<number>[0-9]+)$",
      RegexOptions.Compiled | RegexOptions.IgnoreCase);

    /// <summary>
    /// Base name and revision number of an operation name
    /// </summary>
    /// <param name="name">nullable</param>
    /// <returns>revision 1 when the name carries no suffix, a null base name if name is null</returns>
    public static (string BaseName, int Number) Parse (string name)
    {
      if (name is null) {
        return (null, 1);
      }
      var match = REVISION_SUFFIX.Match (name);
      return match.Success && int.TryParse (match.Groups["number"].Value, out var number)
        ? (match.Groups["base"].Value, number)
        : (name, 1);
    }

    /// <summary>
    /// Name of a revision
    /// </summary>
    /// <param name="baseName">not null</param>
    /// <param name="number"></param>
    /// <returns></returns>
    public static string GetName (string baseName, int number) =>
      1 == number ? baseName : $"{baseName} - rev {number}";

    /// <summary>
    /// The revisions of an operation, the operation itself included, by revision number, then
    /// from the oldest to the most recent one
    ///
    /// An operation with no name has no other revision
    ///
    /// To be run inside a session
    /// </summary>
    /// <param name="operation">not null</param>
    /// <returns>not empty</returns>
    public static async Task<IList<IOperation>> GetRevisionsAsync (IOperation operation)
    {
      var (baseName, _) = Parse (operation.Name);
      if (baseName is null) {
        return new List<IOperation> { operation };
      }
      var revisions = (await ModelDAOHelper.DAOFactory.OperationDAO.FindByNameStartAsync (baseName))
        .Where (x => string.Equals (Parse (x.Name).BaseName, baseName, StringComparison.InvariantCultureIgnoreCase))
        .ToList ();
      if (!revisions.Any (x => x.Id == operation.Id)) {
        revisions.Add (operation);
      }
      return revisions
        .OrderBy (x => Parse (x.Name).Number)
        .ThenBy (x => x.CreationDateTime)
        .ThenBy (x => x.Id)
        .ToList ();
    }

    /// <summary>
    /// The active revision among the revisions of an operation
    /// </summary>
    /// <param name="revisions">not empty, by revision number</param>
    /// <returns>not null</returns>
    public static IOperation GetActive (IList<IOperation> revisions) =>
      revisions.LastOrDefault (x => !x.ArchiveDateTime.HasValue) ?? revisions.Last ();

    /// <summary>
    /// The active revision of an operation
    ///
    /// To be run inside a session
    /// </summary>
    /// <param name="operation">not null</param>
    /// <returns>not null</returns>
    public static async Task<IOperation> GetActiveAsync (IOperation operation) =>
      GetActive (await GetRevisionsAsync (operation));
  }
}
