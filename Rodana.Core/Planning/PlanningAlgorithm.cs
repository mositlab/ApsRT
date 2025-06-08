using Rodana.Core.Partias.TaraSearchEngine;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rodana.Core.Planning
{
    public class PlanningAlgorithm
    {

        public DateTime MinAcceptableDate { get; set; }// => FIXED_START_TIME.HasValue ? FIXED_START_TIME.Value : MIN_DATE_TIME_AFTER_FIX;
    }
}
