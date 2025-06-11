using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uco = UniversalPack.Common.CodedObject<System.Guid, string>;
using UcoDU = UniversalPack.Common.CodedObject<System.Guid, UniversalPack.DimensionUnit>;
using OrderID = UniversalPack.Common.CodedObjectVersioned<System.Guid, string, string>;
using Rodana.Core.Planning.PlanningClasses.PoolClasses;
using Rodana.Core.NaladLengths;
using UniversalPack;

namespace Rodana.Core.Planning.PlanningClasses
{
    public class OpersPool
    {

        public Session.Session SessionLink { get; }

        // Собственно блоки
        public List<OpersBlock> Blocks;

        // Тут хранится инфа о заказах
        public FrozenDictionary<OrderID, OrdersPoolClass> OrdersData;

        // Тут хранится инфа о том, какая операция когда может выполниться минимум
        public FrozenDictionary<int, DatesPoolClass> DatesData;

        // Тут хранится инфа о наладочных длинах
        public FrozenDictionary<int, GottenNaladLengthPoolClass> NaladLengthsData;

        // Эта же инфа, но о моделированиях (где нужна сумма)
        public FrozenDictionary<Guid, GottenNaladLengthPoolClass> NaladLengthsDataAsMods;

        // Тут хранится инфа о наладочных длинах
        public FrozenDictionary<Guid, LengthsNeedToRecalcPoolClass> NaladLengthsNeedToRecalc;

        // Тут хранится инфа о центрах
        public Dictionary<Uco, CenterInPlanData> CentersData;

        // словарь для сопоставления
        public FrozenDictionary<int, PlanningModelling> IDsToMods;


        public FrozenDictionary<int, int> OperIDsToModIDs;


        public FrozenDictionary<Guid, LastSubdivisionsPoolClass> LastSubdivisionsData;

        public OpersPool(Session.Session sessionLink, List<OpersBlock> blocks,
           FrozenDictionary<OrderID, OrdersPoolClass> ordersData,
           FrozenDictionary<int, DatesPoolClass> datesData,
           FrozenDictionary<int, GottenNaladLengthPoolClass> naladLengthsData,
           FrozenDictionary<Guid, GottenNaladLengthPoolClass> naladLengthsDataMods,
           FrozenDictionary<Guid, LengthsNeedToRecalcPoolClass> naladLengthsRecreate,
           FrozenDictionary<int, PlanningModelling> iDsToMods,
           FrozenDictionary<int, int> operIDsToModIDs,
           FrozenDictionary<Guid, LastSubdivisionsPoolClass> lastSubdivisionsData,
           Dictionary<Uco, CenterInPlanData> centersData)
        {
            Blocks = blocks;
            DatesData = datesData;
            OrdersData = ordersData;
            NaladLengthsData = naladLengthsData;
            NaladLengthsDataAsMods = naladLengthsDataMods;
            IDsToMods = iDsToMods;
            OperIDsToModIDs = operIDsToModIDs;
            CentersData = centersData;
            NaladLengthsNeedToRecalc = naladLengthsRecreate;
            LastSubdivisionsData = lastSubdivisionsData;
            SessionLink = sessionLink;
        }

        public OpersPool(Session.Session sessionLink, List<OpersBlock> blocks, OpersPoolConstructor constructor, DateTime start)
        {
            SessionLink = sessionLink;

            Blocks = blocks;

            // Формирует здесь словарь для простоев
            DatesData = constructor.alreadyCreatedMods.ToFrozenDictionary(x => x.ModID, x => new DatesPoolClass(DateTime.MinValue, -1, -1, -1));

            // Формирует здесь словарь для заказов
            OrdersData = constructor.ordersCodes.ToFrozenDictionary(x => x, x => new OrdersPoolClass(DateTime.MinValue, -1, time: 0.0, timeWithK: 0.0));

            IDsToMods = constructor.alreadyCreatedMods.ToFrozenDictionary(x => x.ModID, x => x);

            OperIDsToModIDs = constructor.opsModsAlreadyCreated.ToFrozenDictionary(x => x.Oper.Index, x => x.ModID);

            if (sessionLink.SettingsController.SpecialSettings.CalculateSubdivisionsTransfers)
            {
                LastSubdivisionsData = constructor.partiasCodes.ToFrozenDictionary(x => x, x => new LastSubdivisionsPoolClass(Uco.EmptyObject, -1));
            }

            NaladLengthsDataRecreate(constructor);

            // Формирует здесь словарь для центров
            CentersData = constructor.centersData.ToDictionary(x => x.Key, x => PlanningCalculatorEngine.PlanningCalculatorEngine.FillCenterWithSomeData(null, x.Key, start));
        }

        public void NaladLengthsDataRecreate(OpersPoolConstructor constructor)
        {
            NaladLengthsData = constructor.opsModsAlreadyCreated.ToFrozenDictionary(x => x.Oper.Index, x => new GottenNaladLengthPoolClass(-1, new GottenNaladLengthStruct(
                new NaladLengthValue(0, new(Guid.Empty, DimensionUnit.m)),
                new NaladLengthValue(0, new(Guid.Empty, DimensionUnit.m)),
                new NaladLengthValue(0, new(Guid.Empty, DimensionUnit.m)),
                new NaladLengthValue(0, new(Guid.Empty, DimensionUnit.m)),
                false)));


            NaladLengthsNeedToRecalc = constructor.partiasCodes.ToFrozenDictionary(x => x, x => new LengthsNeedToRecalcPoolClass(false, -1));

            NaladLengthsDataAsMods = constructor.modCodes.ToFrozenDictionary(x => x, x => new GottenNaladLengthPoolClass(-1, new GottenNaladLengthStruct(
                new NaladLengthValue(0, new(Guid.Empty, DimensionUnit.m)),
                new NaladLengthValue(0, new(Guid.Empty, DimensionUnit.m)),
                new NaladLengthValue(0, new(Guid.Empty, DimensionUnit.m)),
                new NaladLengthValue(0, new(Guid.Empty, DimensionUnit.m)),
                false)));
        }
    }
}
