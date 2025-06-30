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
using ApsRT.Core.Planning.PlanningClasses;
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
                    Dictionary<RequiredNaladCondition, // В зависимости от запуска
                        Dictionary<Vector2, NaladLengthValue> // и ещё по сечению смотрим
                        >
                    >
                >
            > AllNaladLengths = new();

        public void ClearAll()
        {
            AllNaladLengths.Clear();
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

            var length0 = (float)model.Length;
            var edIzm = model.DimensionUnitClassificator;
            NaladLengthValue length = new NaladLengthValue(length0, edIzm);

            var from = (float)model.ParameterFrom;
            var to = (float)model.ParameterTo;
            Vector2 sech = new Vector2(from, to);

            if (model.IsFirst)
            {
                add(RequiredNaladCondition.FirstLaunch);
            }

            if (model.IsNext)
            {
                add(RequiredNaladCondition.AnyLaunch);
            }

            void add(RequiredNaladCondition cond)
            {
                if (!AllNaladLengths[rc][oper][nomen].ContainsKey(cond))
                {
                    AllNaladLengths[rc][oper][nomen].Add(cond, new());
                }

                if (!AllNaladLengths[rc][oper][nomen][cond].ContainsKey(sech))
                {
                    AllNaladLengths[rc][oper][nomen][cond].Add(sech, length);
                }
            }
        }


        int criteriaIndex = -1;

        public void SetNeededParamName(string rawCriteriaName)
        {
            criteriaIndex = SessionLink.MasterDataLinks.RechangesMasterData.GetCriteriaIndexByName(rawCriteriaName);
        }

        internal NaladLengthValue GetNaladLengthFromMasterData(Stage op, Uco chosenRC, RequiredNaladCondition whichCondition)
        {
            var emptyNomen = Uco.EmptyObject;

            NaladLengthValue tryIt(Dictionary<RequiredNaladCondition, // В зависимости от запуска
                        Dictionary<Vector2, NaladLengthValue> // и ещё по сечению смотрим
                        > conds)
            {
                if (conds.TryGetValue(whichCondition, out var sechs))
                {
                    var myParam = criteriaIndex == -1 ? 0 : (op.RechangeParameters[criteriaIndex].GetFloat(0) ?? 0); // получаем сечение (или любое другое что нам надо)
                    foreach (var sech in sechs)
                    {
                        if (myParam >= sech.Key.X && myParam <= sech.Key.Y)
                        {
                            return sech.Value;
                        }
                    }
                }
                return new(0, new(Guid.Empty, DimensionUnit.m));
            }

            if (AllNaladLengths.TryGetValue(chosenRC, out var lvl0)
                &&
                lvl0.TryGetValue(op.OperationVid, out var lvl1))
            {
                if (lvl1.TryGetValue(op.Nomenklature, out var conds)) return tryIt(conds); // С заполненной номенклатурой
                if (lvl1.TryGetValue(Uco.EmptyObject, out var conds2)) return tryIt(conds2); // Попробуем по пустой номенклатуре
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

        internal GottenNaladLengthStruct GetNaladLengthsForCurrentStageOnly(RechangesEngineResult RechangesList, Stage op, Uco choosenCenter, PlanningModelling lastMod)
        {
            NaladLengthValue naladLengthSum = new(0, new(Guid.Empty, DimensionUnit.m));// NaladLengths.GetNaladLengthByDataFast(op, choosenCenter, NaladLengths.RequiredNaladCondition.FirstLaunch);

            NaladLengthValue firstNL;
            NaladLengthValue otherNL = new NaladLengthValue(0, new(Guid.Empty, DimensionUnit.m));

            bool thisNLIsFirst = false;

            if (lastMod == null // Это вообще первый запуск здесь
                || (RechangesList != null && RechangesList.ProvokesNaladLengthLaunch)
                )
            {
                // Первому запуску быть
                firstNL = op.GetNaladLengthFromCache(choosenCenter, RequiredNaladCondition.FirstLaunch);
                //firstNL = NaladLengths.NaladLengthsConverted(firstNL, op.EdIzmClassificator, op.KgKm); // Переводим её в нужные единицы измерений
                naladLengthSum = firstNL;
                if (op.PlacesCount > 1)
                {
                    otherNL = op.GetNaladLengthFromCache(choosenCenter, RequiredNaladCondition.AnyLaunch);
                    //otherNL = NaladLengths.NaladLengthsConverted(otherNL, op.EdIzmClassificator, op.KgKm); // Переводим её в нужные единицы измерений
                    naladLengthSum += otherNL.Value * (op.PlacesCount - 1); // Переведённая уже верна
                }
                thisNLIsFirst = true;
            }
            else
            {
                otherNL = op.GetNaladLengthFromCache(choosenCenter, RequiredNaladCondition.AnyLaunch);
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
