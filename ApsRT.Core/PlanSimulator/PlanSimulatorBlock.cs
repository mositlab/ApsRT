
using Uco = UniversalPack.Common.CodedObject<System.Guid, string>;
using UcoDU = UniversalPack.Common.CodedObject<System.Guid, UniversalPack.DimensionUnit>;
using OrderID = UniversalPack.Common.CodedObjectVersioned<System.Guid, string, string>;
using ZLinq;
using ApsRT.Core.Rechanges.MasterData;
using ApsRT.Core.Stages;
using System.Collections.Frozen;
using ApsRT.Core.Centers;
using ApsRT.Core.Planning.PlanningAlgorithm;

namespace ApsRT.Core.PlanSimulator
{
    public class PlanSimulatorBlock
    {
        public PlanSimulatorPool PoolLink;
        public int[] BlockIndex;

        public List<PlanSimulatorMod> Lists;
        Uco[] ActualRC;
        CenterBaseModel[] CenterModelData;
        PlanSimulatorCenterData[] CenterDataLinks;

        public PlanSimulatorBlock[] PreviousBlockOnThisRC;
        public PlanSimulatorBlock[] NextBlockOnThisRC;
        //public DateTime[] DateOfStart;
        //public DateTime[] DateOfEnd;
        //public PlanSimulatorCenterData[] CenterDataLocalCopy;

        public (Uco Center, PlanSimulatorCenterData CenterDataLink, CenterBaseModel CenterModelDataLink) GetCenterData(int PoolIndex)
        {
            return (ActualRC[PoolIndex], CenterDataLinks[PoolIndex], CenterModelData[PoolIndex]);
        }

        public void MoveCenterData(int fromIndex, int toIndex)
        {
            //ActualRC[toIndex] = ActualRC[fromIndex];
            var center = ActualRC[fromIndex];
            if (center != ActualRC[toIndex]) SetCenter(center, toIndex);
            //CenterDataLinks[toIndex] = CenterDataLinks[fromIndex]; - это НЕ ТРОГАЕМ, тут же класс, он сам обновляется. Точнее, трогаем, но вот центр надо поставить другой, они ж могут заменяться, и классы соответственно другие.
            //CenterModelData[toIndex] = CenterModelData[fromIndex];
        }

        public void SetCenter(Uco Center, int PoolIndex)
        {
            ActualRC[PoolIndex] = Center;
            CenterDataLinks[PoolIndex] = PoolLink.CentersData[Center][PoolIndex];
            CenterModelData[PoolIndex] = PoolLink.SessionLink.MasterDataLinks.CentersMasterData.CentersData[Center];
        }

        public void UpdatePossibleRCsHere()
        {
            PossibleRCsHere = null; // Создаём рц, которые могут быть заюзаны для всех операций
            foreach (var list in Lists)
            {
                if (PossibleRCsHere == null)
                    PossibleRCsHere = new(list.PossibleRCsHere);
                else
                    PossibleRCsHere = PossibleRCsHere.AsValueEnumerable().Intersect(list.PossibleRCsHere).ToHashSet();

                // А вот здесь ещё и проверим, чтобы КАЖДЫЙ мод из группы "на одном рц" тоже мог работать здесь
                if (list.FirstOperJust.OnOneRC
                && PoolLink.ModIDToOnOneRCGroupID.TryGetValue(list.ModID, out var dataID)
                && PoolLink.OnOneRCGroupData.TryGetValue(dataID, out var data))
                {
                    PossibleRCsHere = PossibleRCsHere.AsValueEnumerable().Intersect(data.PossibleRCs).ToHashSet();
                }

            }
        }

        public HashSet<Uco> PossibleRCsHere
        {
            get; private set;
        }

        FrozenDictionary<Uco, TimeSpan> _maxTime;
        int lastCount = 0;

        public TimeSpan MaxTime(Uco center)
        {
            if (lastCount != Lists.Count)
            {
                _maxTime = /*Lists[0].*/PossibleRCsHere.ToFrozenDictionary(center => center, center => Lists.Max(x => x.AllTime[center]));
                lastCount = Lists.Count;
            }
            return _maxTime[center];
            //Lists.Count == 1 ? Lists [0].AllTime : ; // Макс. время для вывода надо, вместо разных, в случае, если тут параллелизм
        }

        public override string ToString()
        {
            return $"[{string.Join(", ", Lists.Select(x => x.ModID + (x.IsFixed ? "F" : "") + (x.IsFactOrVPZ ? "V" : "")

            ))}] на РЦ: {ActualRC[0].Label}";
        }

        public PlanSimulatorBlock(List<PlanSimulatorMod> lists, Uco ActualRC, int PoolSize, float ParrKoeff, bool CreateTara)
        {
            Lists = lists;
            this.BlockIndex = new int[PoolSize];
            this.ActualRC = new Uco[PoolSize];
            this.CenterDataLinks = new PlanSimulatorCenterData[PoolSize];
            this.CenterModelData = new CenterBaseModel[PoolSize];
            this.PreviousBlockOnThisRC = new PlanSimulatorBlock[PoolSize];
            this.NextBlockOnThisRC = new PlanSimulatorBlock[PoolSize];
            //this.DateOfStart = new DateTime[PoolSize];
            //this.DateOfEnd = new DateTime[PoolSize];
            //this.CenterDataLocalCopy = new PlanSimulatorCenterData[PoolSize];
            this.ActualRC[0] = ActualRC; // Потом будет назначено всем, в пуле.
            FillKoeff = ParrKoeff;
            //UpdatePossibleRCsHere();
            if (CreateTara)
            {
                var tmp1 = lists.Select(x => x.FirstOperJust.CurrentTara).Where(x => !x.IsEmpty);//.GroupBy(x => x);
                ExistingTaraHere = new();
                foreach (var el in tmp1)
                {
                    if (!ExistingTaraHere.ContainsKey(el))
                    {
                        ExistingTaraHere.Add(el, 0);
                    }

                    ExistingTaraHere[el]++;
                }
            }
        }

        /// <summary>
        /// Какие тут тары и в каком количестве есть.
        /// </summary>
        public Dictionary<Uco, int> ExistingTaraHere = new();

        /// <summary>
        /// Насколько заполнен блок. От 0 до 1.
        /// -1 значит, что этот блок в принципе не поддерживает параллелизм, бладаря этому лезть в словарь рабочих центров не надо - микрооптимизация.
        /// </summary>
        public float FillKoeff = -1;

        /// <summary>
        /// Сколько ещё тар можно вместить.
        /// Понадобится для небольшой оптимизации - если количество моделирований (оно же количество тар в блоке) больше свободного места тут, то блок просто не будет предложен.
        /// </summary>
        public int HowMuchPlaceHere = 0;

        // TODO: для моделирований списки детей и предков есть. Теперь не мешало бы устроить оптимизацию и поблочно.




        /*
        public PlanSimulatorTable TableLink;
        public int BlockIndexInLine;
        public Uco ActualCenter;

        public PlanSimulatorBlock PreviousBlockInLine;
        public PlanSimulatorBlock NextBlockInLine;

        public DateTime StartTime;
        public DateTime EndTime;

        public Rechanges.RechangesEngine.RechangesEngineResult RechangesEngineResult;

        public List<PlanSimulatorBlock> PreviousBlocksInChronology;
        public List<PlanSimulatorBlock> NextBlocksInChronology;

        public List<PlanSimulatorMod> Modellings;

        public HashSet<int> AllStagesHere;

        public void RememberAllStagesHere()
        {
            AllStagesHere = Modellings.AsValueEnumerable().SelectMany(x => x.Stages.Select(y => y.Stage.Index)).Distinct().ToHashSet();
        }

        public DateTime? WorstMinDateOfStart { get; private set; }

        //public DateTime GetWorstMinDateOfStart()
        //{
        //    return TableLink.StartDate;
        //    var res = PreviousBlocksInChronology.Count > 0 ? PreviousBlocksInChronology.AsParallel().Max(x => x.EndTime) : TableLink.StartDate;
        //    if (PreviousBlockInLine != null && PreviousBlockInLine.EndTime > res) res = PreviousBlockInLine.EndTime;
        //    return res;
        //}

        DateTime? WorstMinDateOfStartToUpdate;

        public void WantThisWorstMinDateOfStart(DateTime start)
        {
            if (!WorstMinDateOfStartToUpdate.HasValue || WorstMinDateOfStartToUpdate.Value < start) WorstMinDateOfStartToUpdate = start;
        }

        /// <summary>
        /// Эта функция нужна, чтобы обновить блок извне.
        /// </summary>
        /// <param name="newStartDate">У какого-то предыдущего блока сменилась дата окончания, и это может повлиять на текущий, поэтому надо поработать с датами.</param>
        /// <param name="newRechanges">У кого-то из пред. блоков возникло что-то, из-за чего следует перепроверить переналадку.</param>
        /// <param name="newAdditionalParams">У предыдущего блока изменились доп. параметры.</param>
        public void NeedToRecalc(bool newStartDate, bool newRechanges, bool newAdditionalParams)
        {
            bool willNeedToRecalcNextStartDates = false;
            bool willNeedToRecalcNextRechanges = false;
            bool willNeedToRecalcNextAdditionalParams = false;

            if (newStartDate)
            {
                if (WorstMinDateOfStartToUpdate.HasValue)
                {
                    WorstMinDateOfStart = WorstMinDateOfStartToUpdate.Value;
                    WorstMinDateOfStartToUpdate = null;
                }

                var lastTimeOnThatLine = PreviousBlockInLine != null ? PreviousBlockInLine.EndTime : TableLink.StartDate;
                var worstTime = WorstMinDateOfStart ?? TableLink.StartDate;
                if (worstTime > lastTimeOnThatLine)
                {
                    // Вот здесь простой
                    var prostoiLength = worstTime - lastTimeOnThatLine;

                    lastTimeOnThatLine += prostoiLength; // По итогу добавляем
                }

                StartTime = lastTimeOnThatLine;


                DateTime newEndTime = StartTime.AddHours(1);

                // По итогу, если что-то обновилось, то мы смотрим:

                if (newEndTime != EndTime)
                {
                    EndTime = newEndTime;
                    willNeedToRecalcNextStartDates = true;
                }
            }

            if (newRechanges)
            {
                Stage operationA = PreviousBlockInLine?.Modellings[0].Stages[0].Stage;
                var currentMod = Modellings[0];
                Stage operationB = currentMod.Stages[0].Stage;

                // Найдём переналадочку
                var rechangesResult = operationA == null ? null : operationB.SessionLink.MasterDataLinks.RechangesMasterData.FindRechanges(new(PlanningCalculatorEngine.PlanningCalculatorEngine.FillCenterWithSomeData(null, ActualCenter, TableLink.StartDate), new PlanningClasses.PoolClasses.DatesPoolClass(WorstMinDateOfStart ?? DateTime.Now, 0, -1, -1), operationA, operationB, ActualCenter));
            }


            // У следующих блоков придется что-нибудь пересчитать?
            bool needToRecalcNextBlocksWithAnything = willNeedToRecalcNextStartDates || willNeedToRecalcNextRechanges || willNeedToRecalcNextAdditionalParams;
            if (needToRecalcNextBlocksWithAnything)
            {
                if (NextBlockInLine != null)
                {
                    // Следующему блоку в линии придется пересчитать всё, если есть потребность.
                    //NextBlockInLine.NeedToRecalc(willNeedToRecalcNextStartDates, willNeedToRecalcNextRechanges, willNeedToRecalcNextAdditionalParams);
                    TableLink.AddNeedToRecalc(NextBlockInLine, willNeedToRecalcNextStartDates, willNeedToRecalcNextStartDates, willNeedToRecalcNextAdditionalParams);
                    NextBlockInLine.WantThisWorstMinDateOfStart(EndTime);
                }

                //return;

                // Теперь смотрим детей
                for (int i = 0; i < NextBlocksInChronology.Count; i++)
                {
                    var nextBlock = NextBlocksInChronology[i];
                    if (nextBlock == NextBlockInLine) continue; // Этот же самый блок, если такое когда-нибудь произойдет, не считаем - для него выше есть чек.

                    if (nextBlock.ActualCenter == ActualCenter)
                    {
                        // Если ребенок находится на том же РЦ, то во-первых, это довольно странно, а во вторых, ему кроме даты и переналадок, следует в том числе проверить доп. условия, потому что они меняются.
                        // На всякий случай я это вынес в отдельные фигурные скобки.
                        TableLink.AddNeedToRecalc(nextBlock, willNeedToRecalcNextStartDates, willNeedToRecalcNextStartDates, willNeedToRecalcNextAdditionalParams);
                    }
                    else
                    {
                        // У блоков из другого РЦ максимум что надо проверить - это начало работы, ну и переналадку - на всякий случай, если прогревы сработают. Доп условия чекать не надо, потому что они там подтягиваются с другого РЦ.
                        TableLink.AddNeedToRecalc(nextBlock, willNeedToRecalcNextStartDates, willNeedToRecalcNextStartDates, false);
                    }
                    nextBlock.WantThisWorstMinDateOfStart(EndTime);
                }
            }

        }
        */
    }

}