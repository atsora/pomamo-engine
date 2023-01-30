// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using Lemoine.Core.Cache;
using Lemoine.Model;
using Lemoine.ModelDAO;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lemoine.Business.UnitTests.Operation
{
  [TestFixture]
  class SequenceStandardTime_UnitTest
  {
    /// <summary>
    /// Tests GetRangeAroundSuccess service of DateTimeService (successful response)
    /// </summary>
    [Test]
    public void Test ()
    {
      ISequence sequence1;
      ISequence sequence3;
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
      using (IDAOTransaction transaction = session.BeginTransaction ()) {
        try {
          sequence1 = ModelDAOHelper.DAOFactory.SequenceDAO
            .FindById (1);
          sequence3 = ModelDAOHelper.DAOFactory.SequenceDAO
            .FindById (3);
        }
        finally {
          transaction.Rollback ();
        }
      }

      {
        var request = new Lemoine.Business.Operation.SequenceStandardTime (sequence1);
        { // Without cache
          var standardTime = ServiceProvider.Get (request);
          Assert.AreEqual (TimeSpan.FromHours (1), standardTime);
        }
        { // With cache
          var standardTime = ServiceProvider.Get (request);
          Assert.AreEqual (TimeSpan.FromHours (1), standardTime);
        }
      }

      {
        var request = new Lemoine.Business.Operation.SequenceStandardTime (sequence3);
        { // Without cache
          var standardTime = ServiceProvider.Get (request);
          Assert.AreEqual (null, standardTime);
        }
        { // With cache
          var standardTime = ServiceProvider.Get (request);
          Assert.AreEqual (null, standardTime);
        }
      }
    }
  }
}
