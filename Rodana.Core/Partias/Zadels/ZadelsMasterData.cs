using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uco = UniversalPack.Common.CodedObject<System.Guid, string>;
using OrderID = UniversalPack.Common.CodedObjectVersioned<System.Guid, string, string>;
using Microsoft.Extensions.Options;

namespace Rodana.Core.Partias.Zadels
{

    public class ZadelsMasterData
    {

        public Session.Session SessionLink { get; }

        public ZadelsMasterData(Session.Session session)
        {
            SessionLink = session;
        }

        internal Dictionary<Guid, // гуид партии пф
            Dictionary<OrderID, // ГПшка
                Dictionary<int, // конкретный УРОВЕНЬ
                    Dictionary<Uco, // номенклатура
                        Dictionary<int, // номер повива (0 = -1, т.е. любой)
                                        //Dictionary<Guid, // гуид этапа (тут всегда пустой гуид один, если НЕ конкретный этап
                                Dictionary<Guid, ConcreteZadel> // список конкретных заделов, которые тут перечислены
                                >
                        //>
                        >
                    >
                >
            > ZadelsData = new();

        internal Dictionary<Guid, ConcreteZadel> LinearZadelsDictionary = new();

        internal void AddZadelData(ZadelDataLoaderModel model)
        {

            // 1. Сначала гуид партии. Если он не задан, значит это глюк таблицы - пустая строка.
            if (model.PartiaPF.HasValue)
            {
                var partiaPF = model.PartiaPF.Value;
                if (!ZadelsData.ContainsKey(partiaPF))
                {
                    ZadelsData.Add(partiaPF, new());
                }

                // 2. Превращаем в ГП
                var finalEtap = model.StageGuid;
                var OperationsDataSet = SessionLink.MasterDataLinks.StagesMasterData;
                if (OperationsDataSet.GUIDToIndex.TryGetValue(finalEtap, out var id) && OperationsDataSet.AllStages.TryGetValue(id, out var op))
                {
                    var order = op.OrderLink.OrderID;
                    if (!ZadelsData[partiaPF].ContainsKey(order))
                    {
                        ZadelsData[partiaPF].Add(order, new());
                    }

                    // 3. Уровень
                    var level = model.Level;
                    if (!ZadelsData[partiaPF][order].ContainsKey(level))
                    {
                        ZadelsData[partiaPF][order].Add(level, new());
                    }

                    // 4. Номенклатура
                    var nomen = model.Nomenklature;
                    if (!ZadelsData[partiaPF][order][level].ContainsKey(nomen))
                    {
                        ZadelsData[partiaPF][order][level].Add(nomen, new());
                    }

                    // 5. Номер повива
                    int poviv = model.Poviv;
                    if (!ZadelsData[partiaPF][order][level][nomen].ContainsKey(poviv))
                    {
                        ZadelsData[partiaPF][order][level][nomen].Add(poviv, new());
                    }

                    // 6. Последнее - сам задел
                    var zadelGuid = model.ZadelGuid;

                    if (zadelGuid != Guid.Empty)
                    {
                        if (!LinearZadelsDictionary.ContainsKey(zadelGuid))
                        {
                            var tara = new ZadelDataLoaderModel().Tara;
                            var newZadel = new ConcreteZadel(tara);
                            newZadel.ZadelStatus = ZadelStatus.CannotBeFound;
                            LinearZadelsDictionary.Add(zadelGuid, newZadel);
                        }

                        var zadel = LinearZadelsDictionary[zadelGuid]; // берем ссылку на задел и добавляем инфу про ГП в него
                        //zadel.RelatedGPs.Add(order);

                        //if (!USE_CONCRETE_ETAPS_GUIDS) finalEtap = Guid.Empty;

                        if (!zadel.WhereCanUsed.ContainsKey(finalEtap)) // Если такого задела нет, то мы его создаём
                        {
                            zadel.WhereCanUsed.Add(finalEtap, model.Count);
                        }

                        if (!ZadelsData[partiaPF][order][level][nomen][poviv]/*[finalEtap]*/.ContainsKey(zadelGuid)) // Если такого задела нет, то мы его создаём
                        {
                            ZadelsData[partiaPF][order][level][nomen][poviv]/*[finalEtap]*/.Add(zadelGuid, zadel);
                        }

                    }
                }
            }
        }

        internal void ClearAll()
        {
            ZadelsData.Clear();
            LinearZadelsDictionary.Clear();
        }
        
        string ZadelToString(ZadelStatus x) => x switch
        {
            ZadelStatus.CannotBeFound => "Не найдены подходящие этапы для вставки.",
            ZadelStatus.SussesfullyUsed => "Успешно использовано.",
            ZadelStatus.NotEnoughCount => "Недостаточно длины.",
            ZadelStatus.AnotherUsed => "Потребность закрыта другим заделом.",
            ZadelStatus.HigherZadelUsed => "Использован вышестоящий задел.",
        };

        //"used_zadels.csv"
        internal void OutputZadelsToFile(string Filename)
        {
            var sw = new StreamWriter(Filename, false, Encoding.UTF8);
            //GUID (исходного этапа имеется в виду) и ИдентификаторОбщегоЭтапа
            sw.WriteLine("ИдентификаторЗадела;GUID;НомерМоделирования;ИдентификаторОбщегоЭтапа;КоличествоИспользовано;Причина");
            foreach (var zadel in LinearZadelsDictionary)
            {
                if (zadel.Value.WhereIsUsed.Count > 0)
                {
                    foreach (var etap in zadel.Value.WhereIsUsed)
                    {
                        var opLink = SessionLink.MasterDataLinks.StagesMasterData.AllStages[etap.operID];
                        var modGuid = opLink.IsCustomModelling ? opLink.CustomModelling : opLink.MesModelling.Code;
                        sw.WriteLine($"{zadel.Key};{opLink.GUID};{modGuid};{(opLink.EasyFlag.HasValue ? opLink.EasyFlag.Value : Guid.Empty)};{etap.count};"); // последняя скобка это пустота
                    }
                }
                else
                {
                    foreach (var etap in zadel.Value.WhereCanUsed)
                    {
                        sw.WriteLine($"{zadel.Key};{etap.Key};{Guid.Empty};{Guid.Empty};0;{ZadelToString(zadel.Value.ZadelStatus)}");
                    }
                }
            }
            sw.Close();
        }
    }
}
