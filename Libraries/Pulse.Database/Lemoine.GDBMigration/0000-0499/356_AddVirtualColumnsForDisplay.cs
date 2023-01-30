// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Data;
using Lemoine.Core.Log;
using Migrator.Framework;

namespace Lemoine.GDBMigration
{
  /// <summary>
  /// Migration 356:
  /// </summary>
  [Migration(356)]
  public class AddVirtualColumnsForDisplay: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (AddVirtualColumnsForDisplay).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      // Retrofit that to allow new default values
      SetMinSequence (TableName.REASON_GROUP, ColumnName.REASON_GROUP_ID, 100);
      SetMinSequence (TableName.REASON, ColumnName.REASON_ID, 100);
      
      Database.ExecuteNonQuery (@"
INSERT INTO display(displaytable, displaypattern)
SELECT 'Component', ''
WHERE NOT EXISTS (SELECT 1 FROM display WHERE displaytable='Component' AND displayvariant IS NULL)
");
      Database.ExecuteNonQuery (@"
INSERT INTO display(displaytable, displaypattern)
SELECT 'IntermediateWorkPiece', ''
WHERE NOT EXISTS (SELECT 1 FROM display WHERE displaytable='IntermediateWorkPiece' AND displayvariant IS NULL)
");
      
      OperationIntermediateWorkPieceIdUp ();
      IwpComponentIdUp ();
      OperationComponentIdUp ();
      IwpCiwpcodeUp ();
      IwpCiwporderUp ();
      OperationSlotCiwpcodeUp ();
      OperationSlotCiwporderUp ();
      OperationIntermediateWorkPiecesUp ();
      IwpComponentsUp ();
      OperationComponentsUp ();
      
      Database.ExecuteNonQuery (@"
UPDATE display
SET displaypattern='<%Component.Display%> <%Code%>'
WHERE displaytable='Operation'
  AND displayvariant='Long'");

      Database.ExecuteNonQuery (@"SELECT displayfromnametranslationkeyupdate ();");
      Database.ExecuteNonQuery (@"SELECT displayupdate ();");
    }

    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      Database.ExecuteNonQuery (@"
UPDATE display
SET displaypattern='<%Name%>'
WHERE displaytable='Operation'
  AND displayvariant='Long'");

      OperationComponentsDown ();
      IwpComponentsDown ();
      OperationIntermediateWorkPiecesDown ();
      OperationSlotCiwpcodeDown ();
      OperationSlotCiwporderDown ();
      IwpCiwporderDown ();
      IwpCiwpcodeDown();
      OperationComponentIdDown ();
      IwpComponentIdDown ();
      OperationIntermediateWorkPieceIdDown ();
    }
    
    void OperationIntermediateWorkPieceIdUp ()
    {
      AddVirtualColumn (TableName.OPERATION, ColumnName.INTERMEDIATE_WORK_PIECE_ID, "integer",
                       @"
WITH singleintermediateworkpiece AS (
  SELECT MIN(intermediateworkpieceid) AS intermediateworkpieceid
  FROM public.intermediateworkpiece
  WHERE operationid=$1.operationid
  GROUP BY operationid
  HAVING COUNT(*) = 1
)
SELECT (array_append(array_agg(intermediateworkpieceid), NULL))[1] AS intermediateworkpieceid
FROM singleintermediateworkpiece
");
    }
    
    void OperationIntermediateWorkPieceIdDown ()
    {
      DropVirtualColumn (TableName.OPERATION, ColumnName.INTERMEDIATE_WORK_PIECE_ID);
    }
    
    void IwpComponentIdUp ()
    {
      AddVirtualColumn (TableName.INTERMEDIATE_WORK_PIECE, ColumnName.COMPONENT_ID, "integer",
                       @"
WITH singlecomponent AS (
  SELECT MIN(componentid) AS componentid
  FROM public.componentintermediateworkpiece
  WHERE intermediateworkpieceid=$1.intermediateworkpieceid
  GROUP BY intermediateworkpieceid
  HAVING COUNT(*) = 1
)
SELECT (array_append(array_agg(componentid), NULL))[1] AS componentid
FROM singlecomponent
");
    }
    
    void IwpComponentIdDown ()
    {
      DropVirtualColumn (TableName.INTERMEDIATE_WORK_PIECE, ColumnName.COMPONENT_ID);
    }

    void OperationComponentIdUp ()
    {
      AddVirtualColumn (TableName.OPERATION, ColumnName.COMPONENT_ID, "integer",
                       @"
WITH singlecomponent AS (
  SELECT MIN(componentid) AS componentid
  FROM public.intermediateworkpiece NATURAL JOIN public.componentintermediateworkpiece
  WHERE operationid=$1.operationid
  GROUP BY operationid
  HAVING COUNT(*) = 1
)
SELECT (array_append(array_agg(componentid), NULL))[1] AS componentid
FROM singlecomponent
");
    }
    
    void OperationComponentIdDown ()
    {
      DropVirtualColumn (TableName.OPERATION, ColumnName.COMPONENT_ID);
    }
    
    void IwpCiwpcodeUp ()
    {
      AddVirtualColumn (TableName.INTERMEDIATE_WORK_PIECE, "ciwpcode", "citext",
                       @"
WITH singleciwpcode AS (
  SELECT MIN(intermediateworkpiececodeforcomponent) AS ciwpcode
  FROM public.componentintermediateworkpiece
  WHERE intermediateworkpieceid=$1.intermediateworkpieceid
  GROUP BY intermediateworkpieceid
  HAVING COUNT(*) = 1
)
SELECT (array_append(array_agg(ciwpcode), NULL))[1] AS ciwpcode
FROM singleciwpcode
");
    }
    
    void IwpCiwpcodeDown ()
    {
      DropVirtualColumn (TableName.INTERMEDIATE_WORK_PIECE, "ciwpcode");
    }

    void IwpCiwporderUp ()
    {
      AddVirtualColumn (TableName.INTERMEDIATE_WORK_PIECE, "ciwporder", "integer",
                       @"
WITH singleciwporder AS (
  SELECT MIN(intermediateworkpieceorderforcomponent) AS ciwporder
  FROM public.componentintermediateworkpiece
  WHERE intermediateworkpieceid=$1.intermediateworkpieceid
  GROUP BY intermediateworkpieceid
  HAVING COUNT(*) = 1
)
SELECT (array_append(array_agg(ciwporder), NULL))[1] AS ciwporder
FROM singleciwporder
");
    }
    
    void IwpCiwporderDown ()
    {
      DropVirtualColumn (TableName.INTERMEDIATE_WORK_PIECE, "ciwporder");
    }

    void OperationSlotCiwpcodeUp ()
    {
      AddVirtualColumn (TableName.OPERATION_SLOT, "ciwpcode", "citext",
                       @"
WITH singleciwpcode AS (
  SELECT MIN(intermediateworkpiececodeforcomponent) AS ciwpcode
  FROM public.componentintermediateworkpiece NATURAL JOIN public.intermediateworkpiece
  WHERE operationid=$1.operationid
    AND componentid=$1.componentid
  GROUP BY operationid, componentid
  HAVING COUNT(*) = 1
)
SELECT (array_append(array_agg(ciwpcode), NULL))[1] AS ciwpcode
FROM singleciwpcode
");
    }
    
    void OperationSlotCiwpcodeDown ()
    {
      DropVirtualColumn (TableName.OPERATION_SLOT, "ciwpcode");
    }

    void OperationSlotCiwporderUp ()
    {
      AddVirtualColumn (TableName.OPERATION_SLOT, "ciwporder", "integer",
                       @"
WITH singleciwporder AS (
  SELECT MIN(intermediateworkpieceorderforcomponent) AS ciwporder
  FROM public.componentintermediateworkpiece NATURAL JOIN public.intermediateworkpiece
  WHERE operationid=$1.operationid
    AND componentid=$1.componentid
  GROUP BY operationid, componentid
  HAVING COUNT(*) = 1
)
SELECT (array_append(array_agg(ciwporder), NULL))[1] AS ciwporder
FROM singleciwporder
");
    }
    
    void OperationSlotCiwporderDown ()
    {
      DropVirtualColumn (TableName.OPERATION_SLOT, "ciwporder");
    }
    
    void OperationIntermediateWorkPiecesUp ()
    {
      AddVirtualColumn (TableName.OPERATION, "intermediateworkpieces", "text",
                       @"
WITH intermediateworkpieces AS (
  SELECT intermediateworkpiece.intermediateworkpiecedisplay
  FROM public.intermediateworkpiece
  WHERE operationid=$1.operationid
)
SELECT array_to_string(array(SELECT intermediateworkpiecedisplay FROM intermediateworkpieces), ',') AS intermediateworkpieces
");
    }
    
    void OperationIntermediateWorkPiecesDown ()
    {
      DropVirtualColumn (TableName.OPERATION, "intermediateworkpieces");
    }
    
    void IwpComponentsUp ()
    {
      AddVirtualColumn (TableName.INTERMEDIATE_WORK_PIECE, "components", "text",
                       @"
WITH components AS (
  SELECT component.componentdisplay
  FROM public.componentintermediateworkpiece NATURAL JOIN public.component
  WHERE intermediateworkpieceid=$1.intermediateworkpieceid
)
SELECT array_to_string(array(SELECT componentdisplay FROM components), ',') AS components
");
    }
    
    void IwpComponentsDown ()
    {
      DropVirtualColumn (TableName.INTERMEDIATE_WORK_PIECE, "components");
    }

    void OperationComponentsUp ()
    {
      AddVirtualColumn (TableName.OPERATION, "components", "text",
                       @"
WITH components AS (
  SELECT component.componentdisplay
  FROM public.intermediateworkpiece NATURAL JOIN public.componentintermediateworkpiece NATURAL JOIN public.component
  WHERE operationid=$1.operationid
)
SELECT array_to_string(array(SELECT componentdisplay FROM components), ',') AS components
");
    }
    
    void OperationComponentsDown ()
    {
      DropVirtualColumn (TableName.OPERATION, "components");
    }
    
  }
}
