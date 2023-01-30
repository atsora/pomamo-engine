// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Data;
using System.Diagnostics;
using Lemoine.Core.Log;
using Migrator.Framework;

namespace Lemoine.GDBMigration
{
  /// <summary>
  /// Migration 802: Support the specific field DescriptionOrTranslation in display
  /// </summary>
  [Migration (802)]
  public class DisplayDescriptionOrTranslation : MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger (typeof (DisplayDescriptionOrTranslation).FullName);

    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      Database.ExecuteNonQuery (@"
CREATE OR REPLACE FUNCTION displayextract (tablename NAME, pattern TEXT, prefix TEXT DEFAULT '$1.') RETURNS text AS
$BODY$
DECLARE
  _foreigntablepattern TEXT[];
  _foreigntable NAME;
  _foreignpattern TEXT;
BEGIN
  IF pattern='NameOrTranslation' THEN
    RETURN 'trim(nulltoempty(displayfromnametranslationkey (' || prefix || tablename || 'name, ' || prefix || tablename || 'translationkey)))';
  ELSIF pattern='DescriptionOrTranslation' THEN
    RETURN 'trim(nulltoempty(displayfromnametranslationkey (' || prefix || tablename || 'description, ' || prefix || tablename || 'descriptiontranslationkey)))';
  ELSIF pattern='Size' THEN
    RETURN E'\'(\' || ' || displayextract (tablename, 'Diameter4') || E'|| \' \' ||' || displayextract (tablename, 'Radius4') || E'|| \')\'';
  ELSIF pattern SIMILAR TO '\w+' THEN
    RETURN 'trim(nulltoempty(' || prefix || tablename || pattern || '::text))';
  ELSE
    _foreigntablepattern := regexp_matches(pattern, '(\w+)\.(\w+)');
    _foreigntable := _foreigntablepattern[1];
    _foreignpattern := _foreigntablepattern[2];
    RETURN 'nulltoempty((SELECT ' || displayextract (_foreigntable::name, _foreignpattern::text, (_foreigntable || '.')::text) || ' FROM ' || _foreigntable || ' WHERE ' || _foreigntable || 'id=' || prefix || _foreigntable || 'id)::text)';
  END IF;
END;
$BODY$ LANGUAGE 'plpgsql' IMMUTABLE;
");
    }

    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
    }
  }
}
