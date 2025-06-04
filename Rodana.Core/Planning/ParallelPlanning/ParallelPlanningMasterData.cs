using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uco = UniversalPack.Common.CodedObject<System.Guid, string>;

namespace Rodana.Core.Planning.ParallelPlanning
{
    public class ParallelPlanningMasterData
    {

        public void ClearAll()
        {
            ParallelLoadingsVariants.Clear();
            GuidToCharactersList.Clear();
            CharacteristicsToParID.Clear();
            ParIDParams.Clear();
        }

        /// <summary>
        /// У типа пар. планирования есть всякие данные. Вот они
        /// </summary>
        internal Dictionary<int, ParallelPlanningParamClass> ParIDParams = new();

        /// <summary>
        /// Словарь, который позволяет с помощью гуидов обращаться к набору характеристик при загрузке.
        /// </summary>
        internal Dictionary<Uco, CharacteristicsClass> GuidToCharactersList = new();

        /// <summary>
        /// Словарь, который на основании характеристик выдаёт айдишник пар моделирования. Нужен при загрузке.
        /// </summary>
        internal Dictionary<Uco,
            Dictionary<Uco,
                Dictionary<CharacteristicsClass, int>
                >
            > CharacteristicsToParID = new();

        /// <summary>
        /// Получает номер пар планирования по существующим сырым данным
        /// </summary>
        /// <param name="rawData">Сырые данные из таблицы</param>
        /// <returns>Номер планирования. Если ничего не нашёл, даёт -1.</returns>
        internal int GetParPlanID(Uco rc, Uco vid, Stages.Stage stage)
        {
            if (CharacteristicsToParID.TryGetValue(rc, out var lvl0))
            {
                if (lvl0.TryGetValue(vid, out var chars0))
                {
                    foreach (var chars in chars0)
                    {
                        if (chars.Key.IsEqualWith(stage))
                        {
                            return chars.Value; // Получаем айдишник
                        }
                    }
                }
            }
            return -1;
        }

        /// <summary>
        /// Тут перечислены все варианты параллельной загрузки, список тар и количество в них, и это - для каждого РЦ и ТО
        /// </summary>
        internal Dictionary<Uco, // РЦ
                Dictionary<Uco, // тех операция
                    Dictionary<int, // ID пар. планирования
                        Dictionary<Uco, // имя варианта загрузки
                            ParallelPlanningTaraInfo // тара и ее количество в этом варианте, + количество вообще всей 
                            >
                        >
                    >
                > ParallelLoadingsVariants = new();

        /// <summary>
        /// Получает количество свободных ячеек для пар. планирования.
        /// Если >0 - есть куда поставить. 
        /// Если 0 - то получится поставить под завязку.
        /// Если -1 - то поставить невозможно.
        /// </summary>
        /// <param name="RC"></param>
        /// <param name="VidOp"></param>
        /// <param name="ParID">ИД параллельного планирования из таблицы</param>
        /// <param name="myTotalCount"></param>
        /// <param name="TaraIn">Принимающие тары</param>
        /// <param name="OneMoreTaraIn">Выбранная тара</param>
        /// <param name="TaraOut">Отдающие тары</param>
        /// <returns></returns>
        internal int GetMostFreeTara(
            Uco RC,
            Uco VidOp,
            int ParID,
            int myTotalCount,
            Dictionary<Uco, int> TaraIn,
            Uco OneMoreTaraIn,
            out Dictionary<Uco, int> TaraOut)
        {
            TaraOut = null;
            if (ParID == -1)
            {
                return -1;
            }

            if (TaraIn.Count == 0)
            {
                return -1;
            }

            if (ParallelLoadingsVariants.TryGetValue(RC, out var workCenterLevel)
                &&
                workCenterLevel.TryGetValue(VidOp, out var techOperationLevel))
            {
                if (techOperationLevel.TryGetValue(ParID, out var parallelPlanTable))
                {
                    //var parallelPlanTable = techOperationLevel[ParID];

                    var maxCount = -1;

                    //if (!OneMoreTaraIn.IsEmpty)
                    //{
                    //    myTotalCount++; // Прибавляем
                    //}

                    // Здесь - несколько вариантов загрузки
                    foreach (var tableElement in parallelPlanTable)
                    {
                        int maxCountHere = -1;
                        Dictionary<Uco, int> goodResult = null;

                        // сначала чекаем, есть ли макс. количество
                        if (tableElement.Value.MaxCapacity > 0)
                        {
                            // С ним. Значит достаточно чекнуть количество.
                            goodResult = tableElement.Value.List; // просто перетаскиваем
                            maxCountHere = tableElement.Value.MaxCapacity - myTotalCount; // берём макс количество здесь, отнимая у него уже существующее количество
                        }
                        else
                        {
                            // Без него. Ищем по старинке
                            maxCountHere = tableElement.Value.TotalCount; // Количество тут
                            if (tableElement.Value.TotalCount >= myTotalCount) // Здесь элементов достаточно, чтобы всё вместить? (микрооптимизация)
                            {
                                //(!OneMoreTaraIn.IsEmpty && TaraInEl.Key == OneMoreTaraIn ? 1 : 0)
                                if (OneMoreTaraIn.IsEmpty || tableElement.Value.List.ContainsKey(OneMoreTaraIn))
                                {
                                    goodResult = tableElement.Value.List.ToDictionary(x => x.Key, x => x.Value); // копируем
                                                                                                                 // Нужно пройтись по всем нашим тарам из входного массива.

                                    foreach (var TaraInEl in TaraIn)
                                    {
                                        var val = TaraInEl.Value;// + (!OneMoreTaraIn.IsEmpty && TaraInEl.Key == OneMoreTaraIn ? 1 : 0);
                                        if (tableElement.Value.List.TryGetValue(TaraInEl.Key, out var countHere) // Внутри варианта загрузки есть одна из наших тар?
                                            &&
                                            countHere >= val // И её количество в БД не меньше нашей?
                                            )
                                        {
                                            //var delta = countHere - TaraInEl.Value;
                                            goodResult[TaraInEl.Key] -= val;
                                            if (goodResult[TaraInEl.Key] < 0)
                                            {
                                                // Вылезла за пределы. Уходим.
                                                maxCountHere = -1;
                                                break;
                                            }
                                            else
                                            {
                                                maxCountHere -= val;
                                                if (maxCountHere < 0)
                                                {
                                                    // Вылезла за пределы. Уходим.
                                                    maxCountHere = -1;
                                                    break;
                                                }
                                            }
                                        }
                                        else
                                        {
                                            // Нет? Ну тогда прекращаем этот цикл и maxCountHere = -1 - этот вариант НЕ подходит.
                                            maxCountHere = -1;
                                            break;
                                        }
                                    }
                                }
                                else
                                {
                                    maxCountHere = -1;
                                }
                            }
                            else
                            {
                                maxCountHere = -1;
                            }
                        }

                        if (maxCountHere > maxCount)
                        {
                            if (!OneMoreTaraIn.IsEmpty) // очевидно, тут тара найдена, поэтому напоследок чекаем, чтобы для этой доп. тары нашлось местечко.
                            {
                                if (goodResult.ContainsKey(OneMoreTaraIn)) // Тут тара эта есть? Если нет, то это неподходящий вариант
                                {
                                    if (tableElement.Value.MaxCapacity > 0)
                                    {
                                        maxCountHere--;
                                        if (maxCountHere < 0)
                                        {
                                            // Вылезла за пределы. Уходим.
                                            maxCountHere = -1;
                                            //break;
                                        }
                                    }
                                    else
                                    {
                                        goodResult[OneMoreTaraIn]--;
                                        if (goodResult[OneMoreTaraIn] < 0)
                                        {
                                            // Вылезла за пределы. Уходим.
                                            maxCountHere = -1;
                                            //break;
                                        }
                                        else
                                        {
                                            maxCountHere--;
                                            if (maxCountHere < 0)
                                            {
                                                // Вылезла за пределы. Уходим.
                                                maxCountHere = -1;
                                                //break;
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    maxCountHere = -1;
                                }
                            }
                            if (maxCountHere > maxCount) // По итогу чекаем ещё раз, ибо в 99% случаев OneMoreTaraIn НЕ пуста.
                            {
                                TaraOut = goodResult;
                                maxCount = maxCountHere;
                            }
                        }
                    }
                    return maxCount;
                }
                else
                {
                    ;
                }
            }
            return -1;
        }
    }
}
