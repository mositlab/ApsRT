using Azure;
using Rodana.Core.Rechanges.RechangesEngine;
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
using Rodana.Core.Stages;
using Rodana.Core.Planning.PlanningClasses;

namespace Rodana.Core.NaladLengths
{
    public class NaladLengthsMasterData
    {

        internal static Dictionary<Uco, // рц
            Dictionary<Uco, // тех операция
                Dictionary<Uco, // номенклатура
                    Dictionary<RequiredNaladCondition, // В зависимости от запуска
                        Dictionary<Vector2, NaladLengthValue> // и ещё по сечению смотрим
                        >
                    >
                >
            > AllNaladLengths = new();


        internal NaladLengthValue GetNaladLengthFromMasterData(Stage op, Uco chosenRC, RequiredNaladCondition whichCondition)
        {
            var emptyNomen = new Uco(Guid.Empty, "");

            NaladLengthValue tryIt(Dictionary<RequiredNaladCondition, // В зависимости от запуска
                        Dictionary<Vector2, NaladLengthValue> // и ещё по сечению смотрим
                        > conds)
            {
                if (conds.TryGetValue(whichCondition, out var sechs))
                {
                    var mySech = op.RechangeParameters[2].GetFloat(0) ?? 0; // получаем сечение
                    foreach (var sech in sechs)
                    {
                        if (mySech >= sech.Key.X && mySech <= sech.Key.Y)
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
