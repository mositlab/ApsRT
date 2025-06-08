using Rodana.Core.Partias.PartiasAlgorithm;
using Rodana.Core.Partias.TaraSearchEngine;
using Rodana.Core.Planning.PlanningClasses;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rodana.Core.Planning.PlanningAlgorithm
{
    public class PlanningAlgorithm
    {

        public PlanningAlgorithmStage CurrentStage { get; private set; }

        public Session.Session SessionLink { get; }

        public PlanningAlgorithm(Session.Session session)
        {
            SessionLink = session;
            CurrentStage = PlanningAlgorithmStage.None;
        }

        public DateTime MinAcceptableDate { get; set; }// => FIXED_START_TIME.HasValue ? FIXED_START_TIME.Value : MIN_DATE_TIME_AFTER_FIX;
    
        public List<OpersBlock> BestBlocksList { get; set; }

        public void PrepareEverything()
        {

        }
    }
}
