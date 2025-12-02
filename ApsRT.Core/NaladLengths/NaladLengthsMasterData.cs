using Azure;
using ApsRT.Core.Rechanges.RechangesEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using UniversalPack;
using Uco = UniversalPack.Common.CodedObject<System.Guid, string>;
using UcoDU = UniversalPack.Common.CodedObject<System.Guid, UniversalPack.DimensionUnit>;
using OrderID = UniversalPack.Common.CodedObjectVersioned<System.Guid, string, string>;
using ApsRT.Core.Stages;
using Microsoft.Extensions.Options;
using Google.Protobuf.WellKnownTypes;
using System.Xml.Linq;

namespace ApsRT.Core.NaladLengths
{
    public class NaladLengthsMasterData
    {

        public Session.Session SessionLink { get; }

        public NaladLengthsMasterData(Session.Session sessionLink)
        {
            SessionLink = sessionLink;
        }

        internal Dictionary<Uco, // рц
            Dictionary<Uco, // тех операция
                Dictionary<Uco, // номенклатура
                    Dictionary<Uco, // хар-ка
                        Dictionary<RequiredNaladCondition, // В зависимости от запуска
                            NaladLengthDataNode // сборник с данными
                            >
                        >
                    >
                >
            > AllNaladLengths = new();

        public void PrepareForLoading()
        {
            AllNaladLengths.Clear();
            criteriaIndexes = SessionLink.MasterDataLinks.RechangesMasterData.ExistingCriterias
                .Select((x, index) => (val: x, index: index))
                .Where(x => x.val.UsedInNaladLengths)
                .Select(x => x.index)
                .ToList();
        }

        public void AddNaladLengthData(NaladLengthDataLoaderModel model)
        {
            // 1. РабочийЦентр
            var rc = model.Center;
            if (!AllNaladLengths.ContainsKey(rc))
            {
                AllNaladLengths.Add(rc, new());
            }

            // 2. ВидОперации
            var oper = model.OperationType;

            if (oper.IsEmpty)
            {
                return;
            }

            if (!AllNaladLengths[rc].ContainsKey(oper))
            {
                AllNaladLengths[rc].Add(oper, new());
            }

            // 3. Номенклатура
            var nomen = model.Nomenklature;
            if (!AllNaladLengths[rc][oper].ContainsKey(nomen))
            {
                AllNaladLengths[rc][oper].Add(nomen, new());
            }

            // 4. Характеристика
            var chars = model.Characteristics;
            if (!AllNaladLengths[rc][oper][nomen].ContainsKey(chars))
            {
                AllNaladLengths[rc][oper][nomen].Add(chars, new());
            }

            var length0 = (float)model.Length;
            var edIzm = model.DimensionUnitClassificator;
            NaladLengthValue length = new NaladLengthValue(length0, edIzm);

            if (model.IsFirst)
            {
                add(RequiredNaladCondition.FirstLaunch);
            }

            if (model.IsNext)
            {
                add(RequiredNaladCondition.OtherLaunch);
            }

            void add(RequiredNaladCondition cond)
            {
                if (!AllNaladLengths[rc][oper][nomen][chars].ContainsKey(cond)) AllNaladLengths[rc][oper][nomen][chars].Add(cond, new());

                var currentNode = AllNaladLengths[rc][oper][nomen][chars][cond];

                for (int i = 0; i < model.Params.Length; i++)
                {
                    var currParams = model.Params[i];
                    Vector2 vector = new Vector2((float)currParams.ParameterFrom, (float)currParams.ParameterTo);
                    if (currentNode.NextLevel == null) currentNode.NextLevel = new Dictionary<Vector2, NaladLengthDataNode>();
                    if (currentNode.NextLevel.TryGetValue(vector, out var nextNode))
                    {
                        currentNode = nextNode; // Если эта штука есть, то возвращаем её
                    }
                    else
                    {
                        var newT = new NaladLengthDataNode();
                        currentNode.NextLevel.Add(vector, newT);
                        currentNode = newT; // Переходим на следующий уровень
                    }
                }

                currentNode.Value = length;

            }
        }


        public List<int> criteriaIndexes = new ();

        internal NaladLengthValue GetMinimumNaladLengthFromMasterData(Stage op)
        {
            //NaladLengthValue final = new NaladLengthValue();
            //final.Value = float.MaxValue;
            var list = op.PossibleCenters ?? new List<Uco>() { op.OriginalCenter };

            // Берём худшую длину со всех возможных РЦ, но при этом она должна быть наименьшей для этого РЦ
            var worstLength = list.Max(center =>
            {
                var value1 = GetNaladLengthFromMasterData(op, center, RequiredNaladCondition.FirstLaunch);
                //if (final.Value > value1.Value) final = value1;
                var value2 = GetNaladLengthFromMasterData(op, center, RequiredNaladCondition.OtherLaunch);
                //if (final.Value > value2.Value) final = value2;
                if (value2.Value > value1.Value) return value1;
                return value2;
            });

            worstLength.Value *= op.PlacesCount; // Умножаем на количество мест

            return NaladLengthsConverted(worstLength, op.EdIzmClassificator, op.KgKm);
        }

        internal NaladLengthValue GetNaladLengthFromMasterData(Stage op, Uco chosenRC, RequiredNaladCondition whichCondition)
        {
            var emptyNomen = Uco.EmptyObject;

            NaladLengthValue tryIt(Dictionary<RequiredNaladCondition, // В зависимости от запуска
                        NaladLengthDataNode // сборник с данными
                        > conds)
            {
                if (conds.TryGetValue(whichCondition, out var firstNode))
                {
                    for (int i = 0; i < criteriaIndexes.Count; i++)
                    {
                        int criteriaIndex = criteriaIndexes[i];

                        var value =
                            op.SessionLink.MasterDataLinks.RechangesMasterData.ExistingCriterias[criteriaIndex].CanBeNumber
                            ? (op.RechangeParameters[criteriaIndex].GetFloat(0) ?? 0)
                            : (float.TryParse(op.RechangeParameters[criteriaIndex].GetString(0), out var parsed) ? parsed : 0);

                        bool found = false;
                        foreach (var sech in firstNode.NextLevel)
                        {
                            if (value >= sech.Key.X && value <= sech.Key.Y)
                            {
                                firstNode = sech.Value; // Перезаписываем значение
                                found = true;
                                break;
                            }
                        }
                        if (!found)
                        {
                            return new(0, new(Guid.Empty, DimensionUnit.m)); // Если тут ничего нет, то просто выходим с пустым.
                        }

                    }

                    return firstNode.Value.HasValue ? firstNode.Value.Value : new(0, new(Guid.Empty, DimensionUnit.m));

                }
                return new(0, new(Guid.Empty, DimensionUnit.m));
            }

            if (AllNaladLengths.TryGetValue(chosenRC, out var lvl0))
            {
                if (lvl0.TryGetValue(op.OperationVid, out var lvl1))
                {
                    if (lvl1.TryGetValue(op.Nomenklature, out var conds))
                    {
                        if (conds.TryGetValue(op.Characteristics, out var condsF)) return tryIt(condsF); // С заполненной номенклатурой
                        if (conds.TryGetValue(Uco.EmptyObject, out var condsF2)) return tryIt(condsF2); // С заполненной номенклатурой
                    }
                    if (lvl1.TryGetValue(Uco.EmptyObject, out var conds2))
                    {
                        if (conds2.TryGetValue(op.Characteristics, out var condsF)) return tryIt(condsF); // Попробуем по пустой номенклатуре
                        if (conds2.TryGetValue(Uco.EmptyObject, out var condsF2)) return tryIt(condsF2); // Попробуем по пустой номенклатуре
                    }
                }
                else
                {
                    if (lvl0.TryGetValue(Uco.EmptyObject, out var lvl11))
                    {
                        if (lvl11.TryGetValue(op.Nomenklature, out var conds))
                        {
                            if (conds.TryGetValue(op.Characteristics, out var condsF)) return tryIt(condsF); // С заполненной номенклатурой
                            if (conds.TryGetValue(Uco.EmptyObject, out var condsF2)) return tryIt(condsF2); // С заполненной номенклатурой
                        }
                        if (lvl11.TryGetValue(Uco.EmptyObject, out var conds2))
                        {
                            if (conds2.TryGetValue(op.Characteristics, out var condsF)) return tryIt(condsF); // Попробуем по пустой номенклатуре
                            if (conds2.TryGetValue(Uco.EmptyObject, out var condsF2)) return tryIt(condsF2); // Попробуем по пустой номенклатуре
                        }
                    }
                }
            }
            //    &&
            //    (lvl1.TryGetValue(op.Nomenklatura, out var conds) ^ lvl1.TryGetValue(emptyNomen, out conds)) // одно из этих вещей должно быть равно
            //    &&
            //    conds.TryGetValue(whichCondition, out var sechs))
            //{
            //    var mySech = op.Rechanges[4]; // получаем сечение
            //    foreach (var sech in sechs)
            //    {
            //        if (mySech >= sech.Key.X && mySech <= sech.Key.Y)
            //        {
            //            return sech.Value;
            //        }
            //    }
            //}

            return new(0, new(Guid.Empty, DimensionUnit.m));
        }

        public bool ItsFirstLaunch(RechangesEngineResult RechangesList, PlanSimulator.PlanSimulatorMod lastMod)
        {
            return lastMod == null // Это вообще первый запуск здесь
                || (RechangesList != null && RechangesList.ProvokesNaladLengthLaunch);
        } 

        internal GottenNaladLengthStruct GetNaladLengthsForCurrentStageOnly(Stage op, Uco choosenCenter, bool asFirstLaunch)
        {
            NaladLengthValue naladLengthSum = new(0, new(Guid.Empty, DimensionUnit.m));// NaladLengths.GetNaladLengthByDataFast(op, choosenCenter, NaladLengths.RequiredNaladCondition.FirstLaunch);

            NaladLengthValue firstNL;
            NaladLengthValue otherNL = new NaladLengthValue(0, new(Guid.Empty, DimensionUnit.m));

            bool thisNLIsFirst = false;

            if (asFirstLaunch)
            {
                // Первому запуску быть
                firstNL = op.GetNaladLengthFromCache(choosenCenter, RequiredNaladCondition.FirstLaunch);
                //firstNL = NaladLengths.NaladLengthsConverted(firstNL, op.EdIzmClassificator, op.KgKm); // Переводим её в нужные единицы измерений
                naladLengthSum = firstNL;
                if (op.PlacesCount > 1)
                {
                    otherNL = op.GetNaladLengthFromCache(choosenCenter, RequiredNaladCondition.OtherLaunch);
                    //otherNL = NaladLengths.NaladLengthsConverted(otherNL, op.EdIzmClassificator, op.KgKm); // Переводим её в нужные единицы измерений
                    naladLengthSum += otherNL.Value * (op.PlacesCount - 1); // Переведённая уже верна
                }
                thisNLIsFirst = true;
            }
            else
            {
                otherNL = op.GetNaladLengthFromCache(choosenCenter, RequiredNaladCondition.OtherLaunch);
                //otherNL = NaladLengths.NaladLengthsConverted(otherNL, op.EdIzmClassificator, op.KgKm); // Переводим её в нужные единицы измерений
                firstNL = otherNL;
                naladLengthSum = new(otherNL.Value * op.PlacesCount, otherNL.EdIzm);
            }
            return new(firstNL, otherNL, new(), naladLengthSum, thisNLIsFirst);
        }

        internal NaladLengthValue NaladLengthsConverted(NaladLengthValue nalad, UcoDU EdIzmClassificator, float? KgKm)
        {
            //if (op.EdIzmClassificator == DimensionUnit.m) return nalad;
            //return op.KgKm.HasValue ? (nalad * op.KgKm.Value / 1000f) : nalad;
            if (EdIzmClassificator == nalad.EdIzm) return nalad;

            // А вот если не совпадают, то во
            if (EdIzmClassificator.Label == DimensionUnit.kg && KgKm.HasValue) return new NaladLengthValue(nalad.Value * KgKm.Value / 1000f, EdIzmClassificator);
            if (EdIzmClassificator.Label == DimensionUnit.m && KgKm.HasValue) return new NaladLengthValue(nalad.Value / KgKm.Value * 1000f, EdIzmClassificator);

            return nalad; // В противном случае хз
        }
    }
}
