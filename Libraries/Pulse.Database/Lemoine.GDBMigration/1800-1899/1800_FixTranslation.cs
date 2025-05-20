// Copyright (C) 2025 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Data;
using Lemoine.Core.Log;
using Migrator.Framework;

namespace Lemoine.GDBMigration
{
  /// <summary>
  /// Migration 1800:
  /// </summary>
  [Migration (1800)]
  public class FixTranslation : MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger (typeof (FixTranslation).FullName);

    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      FixTranslationFunctions ();
    }

    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
    }

    void FixTranslationFunctions ()
    {
      Database.ExecuteNonQuery (@"
CREATE OR REPLACE FUNCTION public.displayfromnametranslationkeylocale(
    name character varying,
    translationkey_param character varying,
    locale_param character varying)
  RETURNS character varying AS
$BODY$
DECLARE
  display varchar;
BEGIN
  IF ( name is not null ) THEN
    IF name <> '''' AND name <> '' THEN
      RETURN name;
    END IF;
  END IF;
  IF ( translationkey_param is null ) THEN
    RETURN name;
  END IF;
  
  SELECT into display translationvalue FROM translation WHERE translation.translationkey = translationkey_param AND translation.locale = locale_param;
  IF FOUND THEN
    RETURN display;
  END IF;
  SELECT into display translationvalue FROM translation WHERE translation.translationkey = translationkey_param AND char_length(locale_param) > 2 AND translation.locale = substring(locale_param for 2);
  IF FOUND THEN
    RETURN display;
  END IF;
  SELECT into display translationvalue FROM translation WHERE translation.translationkey = translationkey_param AND translation.locale = '';
  IF FOUND THEN
    RETURN display;
  END IF;
  RETURN name;
END;
$BODY$
  LANGUAGE plpgsql STABLE
  COST 100;
");
      Database.ExecuteNonQuery (@"
CREATE OR REPLACE FUNCTION configvalueextract (
    configvalue character varying)
  RETURNS character varying AS
$BODY$
  SELECT regexp_replace (configvalue, '</?\w+/?>', '', 'g');
$BODY$ LANGUAGE SQL IMMUTABLE;
");
      Database.ExecuteNonQuery (@"
CREATE OR REPLACE FUNCTION displayfromnametranslationkey(
    name_param character varying,
    translationkey_param character varying)
  RETURNS character varying AS
$BODY$
  WITH a AS (
    SELECT 1 as n, displayfromnametranslationkeylocale (name_param, translationkey_param, configvalueextract(configvalue)) as v
    FROM config
    WHERE configkey='i18n.locale.default'
    UNION
    SELECT 2 as n, displayfromnametranslationkeylocale (name_param, translationkey_param, '') as v
  )
  SELECT v FROM a ORDER BY n ASC LIMIT 1;
$BODY$ LANGUAGE SQL STABLE;
");
      Database.ExecuteNonQuery (@"
CREATE OR REPLACE FUNCTION displayfromnametranslationkeylist() RETURNS SETOF NAME AS
$BODY$
  SELECT unnest(ARRAY['machinemode'::name,'machinestatetemplate'::name,'goaltype'::name,'eventlevel'::name,'role'::name,'machinemonitoringtype'::name,
    'machineobservationstate'::name,'operationtype'::name,'unit'::name,'workorderstatus'::name,'componenttype'::name,
    'productionstate'::name]) AS tablename;
$BODY$ LANGUAGE SQL IMMUTABLE;
"); // reason, reasongroup, field are manage differently
      Database.ExecuteNonQuery (@"
CREATE OR REPLACE FUNCTION displayfromnametranslationkeyupdate () RETURNS void AS
$BODY$
BEGIN
  PERFORM displayfromnametranslationkeyupdate (displayfromnametranslationkeylist)
  FROM displayfromnametranslationkeylist ();
END;
$BODY$ LANGUAGE 'plpgsql' VOLATILE;
");
    }
  }
}
