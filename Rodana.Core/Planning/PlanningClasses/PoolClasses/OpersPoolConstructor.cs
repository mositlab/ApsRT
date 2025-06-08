using Azure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uco = UniversalPack.Common.CodedObject<System.Guid, string>;
using UcoDU = UniversalPack.Common.CodedObject<System.Guid, UniversalPack.DimensionUnit>;
using OrderID = UniversalPack.Common.CodedObjectVersioned<System.Guid, string, string>;

namespace Rodana.Core.Planning.PlanningClasses.PoolClasses
{
    public class OpersPoolConstructor
    {
        public Dictionary<Uco, CenterInPlanData> centersData = null;
        public PlanningModelling[] alreadyCreatedMods = null;
        public (Stages.Stage Oper, int ModID)[] opsModsAlreadyCreated = null;
        public HashSet<Guid> partiasCodes = null;
        public HashSet<Guid> modCodes = null;
        public HashSet<OrderID> ordersCodes = null;

        public OpersPoolConstructor(List<OpersBlock> ops, DateTime TimeOfStart)
        {
            alreadyCreatedMods = ops.SelectMany(x => x.Lists).ToArray();

            var innerData = opsModsAlreadyCreated = alreadyCreatedMods.SelectMany(y => y.OpersList.Select(z => (Oper: z, y.ModID))).ToArray();

            partiasCodes = innerData.Select(z => z.Oper.GetOrderOrPartiaCode()).ToHashSet();

            modCodes = innerData.Select(z => z.Oper.IsCustomModelling ? z.Oper.CustomModelling : z.Oper.MesModelling.Code).Where(x => x != Guid.Empty).ToHashSet();

            ordersCodes = innerData.Select(z => z.Oper.OrderLink.OrderID).ToHashSet();

            var centsIDs = ops.Select(x => x.ActualRC).Distinct();
            centsIDs = centsIDs.Union(ops.SelectMany(x => x.Lists.Select(y => y.OpersList[0].PossibleCenters).Where(x => x != null).SelectMany(x => x)));
            var i = 0;
            centersData = new Dictionary<Uco, CenterInPlanData>(centsIDs.Count());
            foreach (var rcID in centsIDs)
            {
                centersData.Add(rcID, PlanningCalculatorEngine.PlanningCalculatorEngine.FillCenterWithSomeData(null, rcID, TimeOfStart));
                i++;
            }
        }
    }
}
