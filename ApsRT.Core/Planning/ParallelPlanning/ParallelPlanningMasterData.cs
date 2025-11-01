using ApsRT.Core.Centers.MasterData;
using ApsRT.Core.Props;
using ApsRT.Core.Taras;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uco = UniversalPack.Common.CodedObject<System.Guid, string>;

namespace ApsRT.Core.Planning.ParallelPlanning
{
    public class ParallelPlanningMasterData
    {
        public Session.Session SessionLink { get; }

        public ParallelPlanningMasterData(Session.Session session)
        {
            SessionLink = session;
        }

        public void ClearAll()
        {
            ParallelLoadingsVariants.Clear();
            GuidToCharactersList.Clear();
            CharacteristicsToParID.Clear();
            ParIDParams.Clear();
        }

        public void AddData(ParallelPlanningDataLoaderModel model)
        {
            var CentersMasterData = SessionLink.MasterDataLinks.CentersMasterData;
            var PropsMasterData = SessionLink.MasterDataLinks.PropsMasterData;
            // 1. Сначала РЦ. Если он не задан, значит это глюк таблицы - пустая строка.
            if (!model.CenterName.IsEmpty)
            {
                if (!ParallelLoadingsVariants.ContainsKey(model.CenterName))
                {
                    ParallelLoadingsVariants.Add(model.CenterName, new Dictionary<Uco, Dictionary<int, Dictionary<Uco, ParallelPlanningTaraInfo>>>());
                }

                // 1.1 - добавляем флаг возможности параллелизма в характеристики РЦ
                if (CentersMasterData.CentersData.TryGetValue(model.CenterName, out var cls))
                {
                    cls.CanBeParallel = true;
                }

                // 2. Вид операции
                if (!ParallelLoadingsVariants[model.CenterName].ContainsKey(model.OperationVid))
                {
                    ParallelLoadingsVariants[model.CenterName].Add(model.OperationVid, new Dictionary<int, Dictionary<Uco, ParallelPlanningTaraInfo>>());
                }

                // 3. Идентификатор параллельной загрузки
                if (!ParallelLoadingsVariants[model.CenterName][model.OperationVid].ContainsKey(model.ParallelID))
                {
                    ParallelLoadingsVariants[model.CenterName][model.OperationVid].Add(model.ParallelID, new Dictionary<Uco, ParallelPlanningTaraInfo>());
                }

                // 3.1 Дельта и "Не Повышать Тару"
                if (!ParIDParams.ContainsKey(model.ParallelID))
                {
                    if (!model.Delta.HasValue) model.Delta = 999999;
                    ParIDParams.Add(model.ParallelID, new ParallelPlanningParamClass(model.Delta.Value, model.NotIncreaseTara));
                }

                // 4. Вариант загрузки
                if (!ParallelLoadingsVariants[model.CenterName][model.OperationVid][model.ParallelID].ContainsKey(model.Variant))
                {
                    var taraInfo = new ParallelPlanningTaraInfo();

                    if (SessionLink.SettingsController.CurrentSettings.UsePseudoTara)
                    {
                        taraInfo.List.Add(TarasMasterData.Pseudotara1, 1);
                        taraInfo.List.Add(TarasMasterData.Pseudotara2, 1);
                        taraInfo.List.Add(TarasMasterData.Pseudotara3, 1);
                        taraInfo.TotalCount += 3;
                    }

                    ParallelLoadingsVariants[model.CenterName][model.OperationVid][model.ParallelID].Add(model.Variant, taraInfo);
                }

                // 5. Тара и её количество для варианта загрузки
                var vari = ParallelLoadingsVariants[model.CenterName][model.OperationVid][model.ParallelID][model.Variant];
                if (!vari.List.ContainsKey(model.Tara) && model.TaraCount.HasValue)
                {
                    vari.List.Add(model.Tara, model.TaraCount.Value);
                    vari.TotalCount += model.TaraCount.Value;
                    vari.MaxCapacity = model.MaxCapacity ?? 0;
                }
                ParallelLoadingsVariants[model.CenterName][model.OperationVid][model.ParallelID][model.Variant] = vari;

                // 6. Теперь набор характеристик
                var nabor = model.CharsList;
                var rc = model.CenterName;
                var operVid = model.OperationVid;
                if (!GuidToCharactersList.ContainsKey(nabor)) // Если характеристики такой нет, добавляем
                {
                    var chr = new CharacteristicsClass(nabor);
                    GuidToCharactersList.Add(nabor, chr);

                }

                if (!CharacteristicsToParID.ContainsKey(rc))
                {
                    CharacteristicsToParID.Add(rc, new());
                }

                if (!CharacteristicsToParID[rc].ContainsKey(operVid))
                {
                    CharacteristicsToParID[rc].Add(operVid, new());
                }

                var characters = GuidToCharactersList[nabor]; // Получаем эту характеристику в любом случае - её надо заполнить

                CharacteristicsToParID[rc][operVid].TryAdd(characters, model.ParallelID); // Добавляем и в этот словарь - его там точно не было.

                // 7. В характеристику пихаем конкретную характеристику

                // 7.1 Оставшиеся столбцы - для характеристики
                if (!characters.ListOfChars.ContainsKey(model.Char))
                {
                    if (PropsMasterData.GenerateLinks(model.ColumnName, out var linker))
                    {
                        characters.ListOfChars.Add(model.Char, new ConcreteCharacteristic(model.Min ?? 0, model.Max ?? 0, linker, model.Value, model.ValueGUID ?? Guid.Empty)); // Добавляем
                    }
                    else
                    {
                        Console.WriteLine($"can't generate prop link for '{model.ColumnName}'");
                    }
                }
            }
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
