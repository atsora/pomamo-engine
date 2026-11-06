// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Lemoine.Model
{
  /// <summary>
  /// Model of table dockind
  ///
  /// Kind of a document: NC main program, NC sub-program, drawing, setup instructions, ...
  /// </summary>
  public interface IDocKind : IVersionable, IDisplayable
  {
    /// <summary>
    /// Name
    ///
    /// Either the name or the translation key is set. An empty string is converted to null
    /// </summary>
    string Name { get; set; }

    /// <summary>
    /// Translation key
    ///
    /// Either the name or the translation key is set. An empty string is converted to null
    /// </summary>
    string TranslationKey { get; set; }

    /// <summary>
    /// Name if set, else the translation of the translation key in the current UI culture
    /// </summary>
    string NameOrTranslation { get; }

    /// <summary>
    /// Name of the icon that represents the kind of document, for the clients
    ///
    /// nullable
    /// </summary>
    string Icon { get; set; }
  }
}
