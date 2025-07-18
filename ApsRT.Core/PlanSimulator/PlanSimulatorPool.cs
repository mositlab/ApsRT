using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uco = UniversalPack.Common.CodedObject<System.Guid, string>;
using UcoDU = UniversalPack.Common.CodedObject<System.Guid, UniversalPack.DimensionUnit>;
using OrderID = UniversalPack.Common.CodedObjectVersioned<System.Guid, string, string>;
using static System.Reflection.Metadata.BlobBuilder;
using ZLinq;
using ApsRT.Core.NaladLengths;
using UniversalPack;
using ApsRT.Core.Stages;
using Newtonsoft.Json.Linq;
using ApsRT.Core.Planning;

namespace ApsRT.Core.PlanSimulator
{
    public class PlanSimulatorPool
    {
        public Session.Session SessionLink { get; }

        // Собственно блоки
        public List<PlanSimulatorBlock> Blocks;

        // Тут хранится инфа о центрах
        public FrozenDictionary<Uco, PlanSimulatorCenterData[]> CentersData;

        public int BestPoolIndex = 0;

        public FrozenDictionary<OrderID, PlanSimulatorOrderData[]> OrdersData;

        public PlanSimulatorPool(Session.Session session, List<PlanSimulatorBlock> blocks, DateTime dateOfStart, int PoolSize)
        {
            SessionLink = session;

            var alreadyCreatedMods = blocks.AsValueEnumerable().SelectMany(x => x.Lists);

            var innerData = alreadyCreatedMods.SelectMany(y => y.Stages);

            var stagesDic = innerData.ToFrozenDictionary(x => x.Stage.Index, x => x);

            var allCents = alreadyCreatedMods.SelectMany(x => x.PossibleRCsHere).Distinct();
            var centersData = new Dictionary<Uco, PlanSimulatorCenterData[]>(allCents.Count());
            foreach (var rcID in allCents)
            {
                var array = new PlanSimulatorCenterData[PoolSize];
                for (int i = 0; i <  array.Length; i++)
                {
                    array[i] = PlanSimulatorEngine.PlanSimulatorEngine.FillCenterWithSomeData(null, rcID, dateOfStart);
                }
                centersData.Add(rcID, array);
            }
            CentersData = centersData.ToFrozenDictionary(); 

            HashSet<Guid> partiasCodes = innerData.Select(z => z.Stage.GetOrderOrPartiaCode()).ToHashSet();
            HashSet<OrderID> ordersCodes = innerData.Select(z => z.Stage.OrderLink.OrderID).ToHashSet();

            // Тут хранится инфа о заказах
            OrdersData = ordersCodes.ToFrozenDictionary(x => x, x =>
            {
                var array = new PlanSimulatorOrderData[PoolSize];
                for (int i = 0; i < array.Length; i++)
                {
                    array[i] = new PlanSimulatorOrderData(DateTime.MinValue, -1, 0, 0);
                }
                return array;
            });

            FrozenDictionary<Guid, PlanSimulatorPartiaRecalcData[]> PartiasToRecalc = partiasCodes.ToFrozenDictionary(x => x, x =>
            {
                var array = new PlanSimulatorPartiaRecalcData[PoolSize];
                for (int i = 0; i < array.Length; i++)
                {
                    array[i] = new PlanSimulatorPartiaRecalcData(false, 0);
                }
                return array;
            });

            FrozenDictionary<Guid, PlanSimulatorLastSubsData[]> LastSubs = null;
            if (SessionLink.SettingsController.SpecialSettings.CalculateSubdivisionsTransfers)
            {
                LastSubs = partiasCodes.ToFrozenDictionary(x => x, x =>
                {
                    var array = new PlanSimulatorLastSubsData[PoolSize];
                    for (int i = 0; i < array.Length; i++)
                    {
                        array[i] = new PlanSimulatorLastSubsData(Uco.EmptyObject, -1);
                    }
                    return array;
                });
            }


            Blocks = blocks;

            var res0 = Parallel.For(0, blocks.Count, index =>
            {
                var block = blocks[index];
                block.PoolLink = this;
                for (int m = 0; m < block.Lists.Count; m++)
                {
                    var mod = block.Lists[m];
                    mod.BlockLink = block;
                    mod.NaladLengthData = new PlanSimulatorNaladLengthData[PoolSize];
                    for (int i = 0; i < PoolSize; i++)
                    {
                        var length = new GottenNaladLengthStruct();
                        mod.NaladLengthData[i] = new PlanSimulatorNaladLengthData(-1, length);
                    }

                    mod.MinDatesData = new PlanSimulatorDatesData[PoolSize];
                    for (int i = 0; i < PoolSize; i++)
                    {
                        mod.MinDatesData[i] = new PlanSimulatorDatesData(DateTime.MinValue, -1, -1, -1);
                    }

                    mod.Ranks = new int[PoolSize];

                    mod.PrevMods = mod.StagesAsNormal.AsValueEnumerable().SelectMany(x => x.Prevs).Where(x => stagesDic.ContainsKey(x)).Select(x => stagesDic[x].ModLink).Distinct().ToList();
                    mod.NextMods = mod.StagesAsNormal.AsValueEnumerable().SelectMany(x => x.Nexts).Where(x => stagesDic.ContainsKey(x)).Select(x => stagesDic[x].ModLink).Distinct().ToList();

                    for (int s = 0; s < mod.Stages.Count; s++)
                    {
                        var stage = mod.Stages[s];
                        stage.ModLink = mod;
                        stage.OrderData = OrdersData[stage.Stage.OrderLink.OrderID];
                        var partiaCode = stage.Stage.GetOrderOrPartiaCode();
                        stage.NaladLengthData = new PlanSimulatorNaladLengthData[PoolSize];
                        for (int i = 0; i < PoolSize; i++)
                        {
                            var length = new GottenNaladLengthStruct();
                            stage.NaladLengthData[i] = new PlanSimulatorNaladLengthData(-1, length);
                        }
                        stage.PartiaRecalcData = PartiasToRecalc[partiaCode];
                        if (LastSubs != null) stage.LastSubsData = LastSubs[partiaCode];
                        stage.PreviousStages = stage.Stage.Prevs.AsValueEnumerable().Where(x => stagesDic.ContainsKey(x)).Select(x => stagesDic[x]).ToList();
                    }
                }
                (var center, _, _) = block.GetCenterData(0);
                for (int i = 0; i < PoolSize; i++)
                {
                    block.SetCenter(center, i);
                }
            });
            while (!res0.IsCompleted)
            {
                ;
            }

        }



        public static List<PlanSimulatorBlock> CopyBlocksSwallow(List<PlanSimulatorBlock> Blocks)
        {
            return new List<PlanSimulatorBlock>(Blocks);
        }

        public static List<PlanSimulatorBlock> CopyBlocksDeep(List<PlanSimulatorBlock> Blocks, bool WithTara)
        {
            //var dt0 = DateTime.Now;
            var opersCopy = new PlanSimulatorBlock[Blocks.Count];//new List<OpersBlock>(Blocks.Count);
            //for (var i = 0; i < Blocks.Count; i++)
            var res0 = Parallel.For(0, Blocks.Count, i =>
            {
                var mod = new List<PlanSimulatorMod>(Blocks[i].Lists.Count); // Создаём список моделирований (заготовку)
                for (var j = 0; j < Blocks[i].Lists.Count; j++)
                {
                    var NEWLIST = Blocks[i].Lists[j];// new ModellingOperWithTime(new List<Operation>(Blocks[i].Lists[j].OpersList), Blocks[i].Lists[j].AllTime); // Копируем список операций и создаем моделирование
                    mod.Add(NEWLIST);
                }
                //var block = new PlanSimulatorBlock(mod, Blocks[i].ActualRC, Blocks[i].FillKoeff, WithTara/*, Blocks[i].CanBeBroken*/);
                //if (!WithTara)
                //{
                //    block.ExistingTaraHere = Blocks[i].ExistingTaraHere;
                //    block.HowMuchPlaceHere = Blocks[i].HowMuchPlaceHere;
                //    block.FillKoeff = Blocks[i].FillKoeff;
                //}
                ////opersCopy.Add(block); // Добавляем блок, у которого за основу взяты параметры
                //opersCopy[i] = block;
            });
            while (!res0.IsCompleted)
            {
                ;
            }
            //var dt1 = DateTime.Now;
            var res = opersCopy.ToList();
            //Console.WriteLine($"copy length: {dt1-dt0}");
            return res;

            //var opersCopy = new OpersBlock[Blocks.Count];
            //Parallel.For(0, Blocks.Count, i =>
            //{
            //    var mod = new List<ModellingOper>(Blocks[i].Lists.Count); // Создаём список моделирований (заготовку)
            //    for (int j = 0; j < Blocks[i].Lists.Count; j++)
            //    {
            //        var NEWLIST = Blocks[i].Lists[j];// new ModellingOperWithTime(new List<Operation>(Blocks[i].Lists[j].OpersList), Blocks[i].Lists[j].AllTime); // Копируем список операций и создаем моделирование
            //        mod.Add(NEWLIST);
            //    }
            //    opersCopy[i] = new OpersBlock(mod, Blocks[i].ActualRC/*, Blocks[i].CanBeBroken*/); // Добавляем блок, у которого за основу взяты параметры
            //});
            //return new(opersCopy);
        }


        internal int Find(int Index, int Start, bool Back)
        {
            for (var i = Start; Back ? i >= 0 : i < Blocks.Count;)
            {
                for (var j = 0; j < Blocks[i].Lists.Count; j++)
                {
                    for (var jj = 0; jj < Blocks[i].Lists[j].StagesAsNormal.Count; jj++)
                    {
                        if (Blocks[i].Lists[j].StagesAsNormal[jj].Index == Index)
                        {
                            return i;
                        }
                    }
                }

                if (Back)
                {
                    i--;
                }
                else
                {
                    i++;
                }
            }
            return -1;
        }

        /// <summary>
        ///  Перенос
        /// </summary>
        /// <returns>Минимальный и максимальные индексы пула, куда можно поставить блок (включительно, в рандомайзерах надо будет прибавлять к max единицу).</returns>
        internal (int MIN, int MAX, int Prev, int Next) FindDiapazonsLegacy(PlanSimulatorBlock opertoIns, int PoolIndex, Uco onRC, int wannaThisPlace, bool calcPrevNexts, bool onlyCalcPrevNexts, bool plusOne, out bool Errors, HashSet<int> doneOpers = null)
        {
            var MIN = opertoIns.Lists[0].StagesAsNormal[0].VPZ || opertoIns.Lists[0].StagesAsNormal[0].IsFact ? SessionLink.AssignedPlanningAlgorithm.GLOBAL_FIXED_MIN : SessionLink.AssignedPlanningAlgorithm.GLOBAL_VPZ_MAX; // минимум - никаких операций нет, поэтому -1. Если будет хоть одна операция, то вставляем строго после нее.
            var MAX = opertoIns.Lists[0].StagesAsNormal[0].VPZ || opertoIns.Lists[0].StagesAsNormal[0].IsFact ? SessionLink.AssignedPlanningAlgorithm.GLOBAL_VPZ_MAX : Blocks.Count; // максимум - включительно, включая последний же индекс. Insert вставит его ПЕРЕД этим индексом, поэтому всё норм.

            // Определяем границы
            Errors = false;
            int Prev = -1;
            int Next = -1;
            if (calcPrevNexts)
            {
                Prev = wannaThisPlace == 0 ? -1 : Blocks.FindLastIndex(wannaThisPlace - 1, x => x.GetCenterData(PoolIndex).Center == onRC/*opertoIns.ActualRC*/);
                Next = Blocks.FindIndex(wannaThisPlace + (plusOne ? 1 : 0), x => x.GetCenterData(PoolIndex).Center == onRC/*opertoIns.ActualRC*/);
            }

            if (onlyCalcPrevNexts) return (-1, -1, Prev, Next);

            for (int ChoosenLine = 0; ChoosenLine < opertoIns.Lists.Count; ChoosenLine++)
            {
                for (var i = 0; i < opertoIns.Lists[ChoosenLine].StagesAsNormal.Count; i++)
                {
                    // Смотрим предыдущие
                    for (var j = 0; j < opertoIns.Lists[ChoosenLine].StagesAsNormal[i].Prevs.Count; j++)
                    {
                        var prev = opertoIns.Lists[ChoosenLine].StagesAsNormal[i].Prevs[j];
                        //var n = opersCopy.FindIndex(x => x.Lists.Any(y => y.Any(z => z.Index == prev)));//
                        //var n = opersCopy.OperIDToPos[opertoIns.Lists[ChoosenLine].OpersList[i].Prevs[j]];//
                        var n = Find(prev, wannaThisPlace == -1 ? (Blocks.Count - 1) : (wannaThisPlace - 1), true);
                        if (doneOpers != null && doneOpers.Contains(prev) && n == -1) Errors = true;
                        if (n > -1 && n >= MIN)
                        {
                            MIN = n + 1;
                        }
                    }
                    // Теперь следующие
                    for (var j = 0; j < opertoIns.Lists[ChoosenLine].StagesAsNormal[i].Nexts.Count; j++)
                    {
                        var next = opertoIns.Lists[ChoosenLine].StagesAsNormal[i].Nexts[j];
                        //var n = opersCopy.FindIndex(x => x.Lists.Any(y => y.Any(z => z.Index == next)));
                        //var n = opersCopy.OperIDToPos[opertoIns.Lists[ChoosenLine].OpersList[i].Nexts[j]];
                        var n = Find(next, wannaThisPlace == -1 ? 0 : wannaThisPlace, false);
                        if (doneOpers != null && doneOpers.Contains(next) && n == -1) Errors = true;
                        if (n > -1 && n < MAX)
                        {
                            MAX = n;
                        }
                    }
                }
            }
            return (MIN, MAX, Prev, Next);
        }

        #region УЙМА всяких методов, которые по хорошему надо разгрести и убрать
        /// <summary>
        /// Словарь, связывающий моделирование и класс с инфой про даты занятой моделированием тары.
        /// Нужен для быстрого доступа к информации моделирования во время построения плана (так как оперирует номерами моделирования).
        /// НЕ ОБНОВЛЯЕТСЯ ПРИ ИЗМЕНЕНИИ ПЛАНА. В CalculateIt и G (17.05.2024) - бесполезен.
        /// </summary>
        Dictionary<int, OperationLineTime> OperToLineTime = new();

        /// <summary>
        /// Словарь, который позволяет хранить списки с временными отрезками, с фильтрацией по таре.
        /// В отличие от OperToLineTime, не оперирует номерами моделирований, так как используется методом GetTaraCountAt,
        /// которому не нужны номера, но нужен быстрый подсчёт количества временных отрезков.
        /// НЕ ОБНОВЛЯЕТСЯ ПРИ ИЗМЕНЕНИИ ПЛАНА. В CalculateIt и G (17.05.2024) - бесполезен.
        /// </summary>
        Dictionary<Uco, List<OperationLineTime>> StructuredTaraDataLine = new();
        internal void ClearTaraTimeLineDics()
        {
            OperToLineTime.Clear();
            StructuredTaraDataLine.Clear();
        }
        /// <summary>
        /// Добавляет новую запись о временном отрезке тары в словари.
        /// </summary>
        /// <param name="mod">Моделирование, которое надо добавить и обновить записи по таре.</param>
        /// <param name="start">Дата начала, его нельзя узнать из структуры мода, надо передавать.</param>
        /// <returns></returns>
        internal void AddInfoTaraTimeLine(PlanSimulatorMod mod, Uco center, DateTime start, string comment)
        {
            // 1. Обновляем в словаре информацию о предках, если таковые есть.
            // TODO: а если предков нет, то что тогда? уточнить у алены. Проблема бдует в том, что у меня нет инфы об отдающей таре.

            var end = start + mod.AllTime[center];
            if (mod.PrevMods.Count > 0)
            {
                foreach (var prev in mod.PrevMods)
                {
                    if (OperToLineTime.TryGetValue(prev.ModID, out var cl)) // класс такой есть? проверяем на всякий случай - если тара пустая, то ее может и не оказаться.
                    {
                        cl.uncheckedKids--;
                        cl.end = cl.end > end ? cl.end : end; // TODO: кажется у андрея была сравнивающая хрень для дат
                    }
                }
            }

            // 2. Добавляем инфу
            var line = new OperationLineTime(start, mod.NextMods.Count, comment); // создаём новый класс временного отрезка
            if (mod.NextMods.Count == 0)
            {
                line.end = end; // Если у меня нет детей (значит я последняя), то тут же и задаём последнюю дату.
            }

            var tara = mod.FirstOperJust.CurrentTara; // получаем тару текущего мода (достаточно глянуть на первый этап)
            if (!tara.IsEmpty)
            {
                OperToLineTime.Add(mod.ModID, line); // в один словарь добавляем
                if (!StructuredTaraDataLine.ContainsKey(tara))
                {
                    StructuredTaraDataLine.Add(tara, new());
                }

                StructuredTaraDataLine[tara].Add(line); // ну и в другой словарь - для метода "GetTaraCountAt"
            }
        }

        internal int GetTaraCountAt(Uco tara, DateTime time)
        {
            // TODO: оптимизировать ее так, чтобы она не весь список шерстила, а лишь N раз, где N - макс количество тары из базы данных.
            // взять у андрея.
            if (StructuredTaraDataLine.TryGetValue(tara, out var list))
            {
                if (list.Count == 0)
                {
                    return 0;
                }

                var count = list
                        .Where(x =>

                        time >= x.start // больше или равно старту
                        &&
                        (x.uncheckedKids > 0 || time <= x.end) // не все дети проверены (значит не снята тара), ЛИБО - меньше или равно концу (в диапазоне)

                        )
                        .Count();
                return count;
            }
            return 0;
        }
        #endregion
    }
}
