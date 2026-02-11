using Azure;
using ApsRT.Core.Partias.PartiasConnectorEngine;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uco = UniversalPack.Common.CodedObject<System.Guid, string>;
using ApsRT.Core.Taras.TaraSearchEngine;

namespace ApsRT.Core.Partias.PartiasClasses
{
    public class PartiaClass
    {
        #region Основное
        public List< // СНАЧАЛА стадия
            List< // потом на каждой стадии могут быть разные этапы (например, на одном этапе по три жилы разных цветов)
                PartiaModelling // но так как они могут быть объединены, то это тоже является списком, и у каждого списка своя тара
                >> OpersHere;

        //Dictionary<int, float> NaladLengthsNow = new Dictionary<int, float>();

        //void UpdateNaladLengthsHere()
        //{
        //    foreach (var sdvig in OpersHere[^1])
        //    {

        //    }
        //}

        public DateTime PossibleDeadline;
        public bool IsRestricted;
        public bool IsElementar = true;
        public int MergeFlag = 0;
        public Guid MyGuid; // гуид для партии пф

        public float LastK = 0;

        public int GPsCountNow = 1;

        public int MaxThereCountForLongestOpers = -1;

        public (List<Stages.Stage> Modelling, int LevelIndex) GetLongestOpersForPostCheck()
        {
            List<Stages.Stage> best = null;
            int time = -1;
            int besti = -1;
            for (int i = 0; i < OpersHere.Count; i++)
            {
                for (int j = 0; j < OpersHere[i].Count; j++)
                {
                    var y = OpersHere[i][j];

                    switch (y.Opers[0].SessionLink.SettingsController.SpecialSettings.UsedPartiaMergingRestriction)
                    {
                        case Settings.SpecialSettings.PartiaMergingRestrictionType.Level2_Multiplies2:
                            {
                                // А как мне НЕСКОЛЬКО этапов найти то? - а в пост-чекинге там функция ищет все этапы на этом уровне с этой номенклатурой, так что всё путём.
                                if (y.Opers[0].Level != 2) continue;

                                besti = i;
                                best = y.Opers;

                                break;
                            }
                        default:
                            {

                                if (y.Opers[0].UsesVspomTara || y.ZadelCode != 0 || y.Opers[0].IsVspom || y.Opers[0].IsLast) continue;

                                var maxT = y.Opers.Max(x => x.LongestDuration);
                                if (maxT > time)
                                {
                                    best = y.Opers;
                                    time = maxT;
                                    besti = i;
                                }

                                break;
                            }
                    }

                }
            }
            return (best == null ? null : new(best), besti);
        }

        public Dictionary<int, PartiaModelling> SeeSdvigsAsDic() => PartiasConnectorEngine.PartiasConnectorEngine.SeeSdvigsAsDic(OpersHere);
        //{
        //    var list = OpersHere.SelectMany(x => x.SelectMany(y => y.Opers.Select(z => (z.ID, y))));
        //    return list.ToDictionary(x => x.ID, x => x.y);
        //}

        public void GetDataFromOtherPartia(PartiaClass what)
        {
            OpersHere = what.OpersHere;
            PossibleDeadline = what.PossibleDeadline;
            IsRestricted = what.IsRestricted;
            MyGuid = what.MyGuid;
            MergeFlag = what.MergeFlag;
            IsElementar = what.IsElementar;
            LastK = what.LastK;
            GPsCountNow = what.GPsCountNow;
            MaxThereCountForLongestOpers = what.MaxThereCountForLongestOpers;
        }

        public PartiaClass(List<List<PartiaModelling>> opersHere, DateTime deadline /*List<int> modelsHere, List<string> ordersHere*//*, Color? usedColor = null*/, bool isRestricted, int mergeFlag, Guid pfGuid, int maxThereCountForLongestOpers, int GPsCount = 1/*, int longestModsMaxCount = int.MaxValue*/)
        {
            OpersHere = opersHere;
            //ModelsHere = modelsHere;
            //OrdersHere = ordersHere;
            PossibleDeadline = deadline;
            IsRestricted = isRestricted;
            MyGuid = pfGuid;
            MergeFlag = mergeFlag;
            //UsedColor = usedColor.HasValue ? usedColor.Value : Color.White;
            //if (!IsRestricted) UpdateNaladLengthsHere();
            GPsCountNow = GPsCount;
            MaxThereCountForLongestOpers = maxThereCountForLongestOpers;
            //LongestModsMaxCount = longestModsMaxCount;
        }

        public override string ToString()
        {
            var ids = OpersHere.SelectMany(x => x.SelectMany((y, INDEX) => y.Opers.Select(z => $"{z.Index} [{INDEX}]")));
            var error = (ids.Distinct().Count() != ids.Count()) ? "!!!" : "";
            return error + String.Format(" #{0:X} ({1} шт.) - {2}", GetHashCode(), ids.Count(), OpersHere[0][0].Opers[0].OrderLink.OrderID);// + ": " + string.Join("; ", OrdersHere);
        }

        public void Sort()
        {
            Dictionary<int, int> IDToGroup = new();

            foreach (var lvl in OpersHere[^1])
            {
                var gr = 0;
                foreach (var lvl0 in lvl.Opers)
                {
                    gr = setIt(lvl0.Index, gr) + 1;
                    int setIt(int id, int group)
                    {
                        if (!IDToGroup.ContainsKey(id))
                        {
                            IDToGroup.Add(id, group);
                        }

                        if (lvl0.SessionLink.MasterDataLinks.StagesMasterData.AllStages.TryGetValue(id, out var op) && op.Prevs.Count > 0)
                        {
                            foreach (var id2 in op.Prevs)
                            {
                                group = setIt(id2, group) + 1;
                            }
                        }
                        return group;
                    }
                }
            }

            for (var i = 0; i < OpersHere.Count; i++)
            {
                OpersHere[i] = OpersHere[i].OrderBy(x => IDToGroup.TryGetValue(x.Opers[0].Index, out var gr) ? gr : -1).ToList();
            }
        }
        #endregion

        #region Оптимизатор подразделений
        void OptimizeGroupSubdivision(PartiaModelling[] sdvigs, PartiaModelling[][] PossibleDatas, PartiasConnectorEngineResult previousData)
        {
            HashSet<int> ignoredSdvigs = new HashSet<int>(sdvigs.Length);
            var sessionLink = sdvigs[0].Opers[0].SessionLink;
            var SubdivisionsTransfersMasterData = sessionLink.MasterDataLinks.SubdivisionsTransfersMasterData;
            var CentersData = sessionLink.MasterDataLinks.CentersMasterData.CentersData;
            while (ignoredSdvigs.Count < sdvigs.Length)
            {
                // Найдём самое долгое время
                int Index = -1;
                int maxTime = 0;
                for (int i = 0; i < sdvigs.Length; i++)
                {
                    if (ignoredSdvigs.Contains(i)) continue; // Игнорируемое - игнорируем

                    if (PossibleDatas[i].Length == 0) // Если решения для них нет другого, то просто их игнорим
                    {
                        ignoredSdvigs.Add(i);
                        continue;
                    }

                    int TimeHere =

                        (i == 0 ? 0 : SubdivisionsTransfersMasterData.GetSubdivisionTransferTime(CentersData[sdvigs[i - 1].RC].Subdivision, CentersData[sdvigs[i].RC].Subdivision))

                        + sdvigs[i].Opers.Sum(x => x.GetDuration(sdvigs[i].RC))

                        + ((i < sdvigs.Length - 1) ? SubdivisionsTransfersMasterData.GetSubdivisionTransferTime(CentersData[sdvigs[i].RC].Subdivision, CentersData[sdvigs[i + 1].RC].Subdivision) : 0);

                    if (maxTime < TimeHere)
                    {
                        maxTime = TimeHere;
                        Index = i;
                    }

                }

                if (Index == -1) break; // Хз как такое возможно, но выходим

                // А теперь поищем минимальный РЦ, у самого жорного сдвига здесь.
                PartiaModelling bestSol = null;
                foreach (var solution in PossibleDatas[Index])
                {
                    var rc = solution.RC;
                    var timeWithHim = (Index == 0 ? 0 : SubdivisionsTransfersMasterData.GetSubdivisionTransferTime(CentersData[sdvigs[Index - 1].RC].Subdivision, CentersData[rc].Subdivision))

                        + sdvigs[Index].Opers.Sum(x => x.GetDuration(sdvigs[Index].RC))

                        + ((Index < sdvigs.Length - 1) ? SubdivisionsTransfersMasterData.GetSubdivisionTransferTime(CentersData[rc].Subdivision, CentersData[sdvigs[Index + 1].RC].Subdivision) : 0);

                    // Нужно строгое неравенство, чтоб он на бесконечный цикл не завис
                    if (timeWithHim < maxTime)
                    {
                        bestSol = solution;
                        maxTime = timeWithHim;
                    }
                }

                if (bestSol == null || bestSol.RC == sdvigs[Index].RC) // Если я ничего не нашел, либо это тот же РЦ, то считаем, что результат плох
                {
                    ignoredSdvigs.Add(Index);
                }
                else
                {
                    ignoredSdvigs.Clear(); // Стираем всё, ведь я нашёл результат
                    PartiasConnectorEngine.PartiasConnectorEngine.updateDataBcsSuccess(sdvigs[Index], bestSol.Tara, bestSol.RC, bestSol.Max, bestSol.Format, bestSol.PlacesCountMultiplierToRewrite, previousData); // Записываем новый РЦ
                }

                if (Index == -1) break; // Хз как такое возможно, но выходим
            }
        }

        public void TaraDecreaser74_110()
        {
            Dictionary<int, Uco> recommendedTara = new();
            PartiasGeneratorEngine.PartiasGeneratorEngine.Checks74_110(OpersHere, recommendedTara, false);
            var engineData = new PartiasGeneratorEngine.PartiasGeneratorEngineData(null, OpersHere[0][0].Opers[0].SessionLink);
            engineData.BestSubdivision = PartiasConnectorEngine.PartiasConnectorEngine.GetBestSubdivisionHere(PartiasConnectorEngine.PartiasConnectorEngine.SeeSdvigsAsList(OpersHere));
            engineData.Color = OpersHere[0][0].Opers[0].Color;
            engineData.RestrictionsVia74110 = recommendedTara;
            for (int i = 0; i < OpersHere.Count; i++)
            {
                for (int j = 0; j < OpersHere[i].Count; j++)
                {
                    OpersHere[i][j] = PartiasGeneratorEngine.PartiasGeneratorEngine.checkThisList(OpersHere[i][j], engineData, false/*, finalResult*/);
                }
            }
        }

        public void SubdivisionsOptimizer(PartiasConnectorEngineResult previousData)
        {
            var dics = SeeSdvigsAsDic();
            for (var CurrentLevel = 0; CurrentLevel < OpersHere.Count; CurrentLevel++)
            {
                if (OpersHere[CurrentLevel].Count == 1) continue;

                // Разделим их по виду операции, номенклатуре и повиву
                var sdvigGroups = OpersHere[CurrentLevel].GroupBy(x => (x.Opers[0].Nomenklature, x.Opers[0].OperationVid, x.Opers[0].Poviv));

                // Пройдемся по всем группам
                foreach (var group in sdvigGroups) // Пройдемся по каждой группе
                {
                    var toAnalyse = group.ToArray(); // превращаем в массив
                    PartiaModelling[][] results = new PartiaModelling[toAnalyse.Length][]; // Подберём для каждого сдвига, список РЦ, на которые он гарантированно может встать
                    for (int i = 0; i < toAnalyse.Length; i++)
                    {
                        var currentSdvig = toAnalyse[i];
                        var centers = currentSdvig.Opers[0].PossibleCenters ?? new List<Uco>() { currentSdvig.Opers[0].OriginalCenter };
                        results[i] = centers.Select(center =>
                        {
                            var b = Taras.TaraSearchEngine.TaraSearchEngine.SumIsPossible(currentSdvig, out var res, out _, new SumIsPossibleOptions(false)
                            {
                                PrevsInfo = previousData.IDToTara,
                                NextRCsMustBeThese = TaraSearchEngine.FindNeededNextRCs(currentSdvig.Opers, (index) => dics.TryGetValue(index, out var s) ? s.RC : null),
                                //.SelectMany(x => x.Nexts)
                                //.Select(x => dics.TryGetValue(x, out var s) ? s : null)
                                //.Where(x => x != null)
                                //.Select(x => x.RC)
                                //.ToHashSet(),
                            });
                            if (b)
                            {
                                return new PartiaModelling(null, res.TaraName, res.Max, res.used_format, res.FinalRC, null, 0, null);
                            }
                            else
                            {
                                return null;
                            }
                        }).Where(x => x != null).ToArray();
                    }
                    OptimizeGroupSubdivision(toAnalyse, results, previousData);
                }
            }
        }
        #endregion

    }
}
