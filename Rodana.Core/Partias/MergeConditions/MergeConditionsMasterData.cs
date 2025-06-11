using Azure;
using Rodana.Core.Props;
using Rodana.Core.Stages;
using System.Linq;
using System.Text;
using UniversalPack.Extensions;
using Uco = UniversalPack.Common.CodedObject<System.Guid, string>;

namespace Rodana.Core.Partias.MergeConditions
{
    /// <summary>
    /// Данные параметров обединения
    /// </summary>
    public class MergeConditionsMasterData
    {
        /// <summary>
        /// Набор данных условий объединения
        /// </summary>
        public IDictionary<(Guid, Guid), MergeConditionsBaseModel> MergeConditions { get; } = new Dictionary<(Guid, Guid), MergeConditionsBaseModel>();

        /// <summary>
        /// Индекс условий объединения по GUID типа тех. операции и подразделения
        /// </summary>
        public Dictionary<Guid, Dictionary<Guid, MergeConditionsBaseModel>> MergeConditionsByOperationTypeGuid { get; private set; }

        public MergeConditionsBaseModel GetMergeConditionSafeByOperVidAndSub(Guid operVid, Guid sub)
        {
            if (MergeConditionsByOperationTypeGuid == null) return null;
            if (MergeConditionsByOperationTypeGuid.TryGetValue(operVid, out var subs))
            {
                if (subs == null) return null;
                if (subs.TryGetValue(sub, out var result)) return result;
                else
                {
                    if (subs.TryGetValue(Guid.Empty, out var result2)) return result2;
                }
                return null;
            }
            return null;
        }

        /// <summary>
        /// Коллекция GUID видов тех. операций, для которых не нужно проверять намотки
        /// </summary>
        ICollection<Guid> DoNotCheckWindingsByOperationTypeGuid { get; set; }
        ICollection<(Guid, Guid)> DoNotCheckWindingsByOperationTypeGuidWithSub { get; set; }

        public bool DoNotCheckWindingsSafe(Guid operVid, Guid sub)
        {
            if (DoNotCheckWindingsByOperationTypeGuidWithSub != null
                && DoNotCheckWindingsByOperationTypeGuidWithSub.Contains((operVid, sub)))
                return true;

            if (DoNotCheckWindingsByOperationTypeGuid == null)
                return false;
            else
                return DoNotCheckWindingsByOperationTypeGuid.Contains(operVid);
        }


        public IDictionary<string, PropsLinkerClass> MergeConditionsGlobalList { get; } = new Dictionary<string, PropsLinkerClass>();

        /// <summary>
        /// Очистка набора данных
        /// </summary>
        public void ClearMergeConditionsList()
        {
            MergeConditionsGlobalList.Clear();
        }


        /// <summary>
        /// Очистка набора данных
        /// </summary>
        public void ClearMergeConditions()
        {
            MergeConditions.Clear();
            MergeConditionsByOperationTypeGuid?.Clear();
            DoNotCheckWindingsByOperationTypeGuid?.Clear();
        }

        /// <summary>
        /// Построение индексов
        /// </summary>
        public void MakeIndexes()
        {
            MergeConditionsByOperationTypeGuid?.Clear();
            MergeConditionsByOperationTypeGuid = MergeConditions
                .GroupBy(x => x.Key.Item1)
                .ToDictionary(x => x.Key, x => x.ToDictionary(y => y.Key.Item2, y => y.Value));

            DoNotCheckWindingsByOperationTypeGuid?.Clear();
            DoNotCheckWindingsByOperationTypeGuid = MergeConditions
                .Where(e => e.Value.DoNotCheckWindings && e.Value.Subdivision.IsEmpty)
                .Select(e => e.Key.Item1)
                .Distinct()
                .ToHashSet();

            DoNotCheckWindingsByOperationTypeGuidWithSub?.Clear();
            DoNotCheckWindingsByOperationTypeGuidWithSub = MergeConditions
                .Where(e => e.Value.DoNotCheckWindings && !e.Value.Subdivision.IsEmpty)
                .Select(e => e.Key)
                .Distinct()
                .ToHashSet();
        }

        #region Метод для проверки, обращающийся к НСИ для выяснения, могут ли объединяться эти два этапчика

        //internal static BitArray[] OpersAreEqualCash;
        //internal static BitArray[] OpersAreEqualCashChecked;

        bool MainChecks(Stage op1, Stage op2, out string reason, out string smallreason)
        {
            reason = "";
            smallreason = "";
            // если у хотя бы одной операции "не объединять" - выходим
            if (op1.IsLast)
            {
                reason = $"у {op1.Index} флаг не объед";
                smallreason = "флаг НеОбъединять";
                return false;
            }
            if (op2.IsLast)
            {
                reason = $"у {op2.Index} флаг не объед";
                smallreason = "флаг НеОбъединять";
                return false;
            }

            if (op1.Fixed)
            {
                reason = $"у {op1.Index} флаг не переплан";
                smallreason = "флаг НеПерепланировать";
                return false;
            }
            if (op2.Fixed)
            {
                reason = $"у {op2.Index} флаг не переплан";
                smallreason = "флаг НеПерепланировать";
                return false;
            }

            if (op1.IsFact)
            {
                reason = $"у {op1.Index} отражениеФакт";
                smallreason = "флаг отражениеФакт";
                return false;
            }
            if (op2.IsFact)
            {
                reason = $"у {op2.Index} отражениеФакт";
                smallreason = "флаг отражениеФакт";
                return false;
            }

            if (op1.VPZ != op2.VPZ)
            {
                reason = $"у {op1.Index} и {op2.Index} не равны впз";
                smallreason = "не равны ВПЗ";
                return false;
            }

            if (op1.IsFictive != op2.IsFictive)
            {
                reason = $"у {op1.Index} и {op2.Index} не равны фиктивности";
                smallreason = "не равны фиктивности";
                return false;
            }

            if (op1.OperationVid != op2.OperationVid)
            {
                reason = $"у {op1.Index} и {op2.Index} не равны виды тех. операций";
                smallreason = "не равны виды тех. операций";
                return false;
            }

            if (op1.RejectedPositionsGuid != op2.RejectedPositionsGuid)
            {
                reason = $"у {op1.Index} и {op2.Index} не равны Идентификаторы Забракованных Позиций";
                smallreason = "не равны Идентификаторы Забракованных Позиций";
                return false;
            }

            return true;
        }

        /// <summary>
        /// Проверка, могут ли быть объединены две операции
        /// </summary>
        /// <param name="op1"></param>
        /// <param name="op2"></param>
        /// <param name="reason"></param>
        /// <returns></returns>
        public bool StagesEqualityCheck(Stage op1, Stage op2, out string reason, out string smallreason, out bool hasSettings, bool forObshEtaps = false)
        {
            // результат проверки объединения
            bool result;
            var mergeCondition = GetMergeConditionSafeByOperVidAndSub(op1.OperationVid.Code, op1.Subdivision.Code);

            if (mergeCondition != null)
            {
                // определяем критерии по условиям объединения
                // если нашли по виду тех. операции
                result = CheckByCondition(op1, op2, mergeCondition, out reason, out smallreason, forObshEtaps);
                hasSettings = true;
                if (!result) return false;
            }
            else
            {
                // иначе проверяем старым способом
                reason = smallreason = $"Для вида тех. операции {op1.OperationVid} не задан набор условий.";
                hasSettings = false;
                return false;
            }

            if (!MainChecks(op1, op2, out var str, out var str2))
            {
                //if (!forObshEtaps)
                //{
                //    OpersAreEqualCash[op1.Index][op2.Index] = false;
                //    OpersAreEqualCash[op2.Index][op1.Index] = false;
                //}
                reason = str;
                smallreason = str2;
                hasSettings = true;
                return false;
            }
            // кэшируем результат и выходим
            //if (!forObshEtaps)
            //{
            //    OpersAreEqualCash[op1.Index][op2.Index] = result;
            //    OpersAreEqualCash[op2.Index][op1.Index] = result;
            //}

            return result;
        }

        /// <summary>
        /// Проверка критериев объединения операций новым методом
        /// </summary>
        /// <param name="op1">Операция</param>
        /// <param name="op2">Операция</param>
        /// <param name="condition">Условия для проверки</param>
        /// <param name="reason">Причина необъединения</param>
        /// <returns></returns>
        /// <remarks>
        /// Почему некоторые критерии проверяются по переналадкам (пояснение от Влада):
        /// потому что в массиве содержится упрощенная инфа не о переналадках, 
        /// а о ИНФЕ для переналадок: напряжение, сечение и тд. 
        /// в виде хэшей или индексов
        /// </remarks>
        bool CheckByCondition(Stage op1, Stage op2, MergeConditionsBaseModel condition, out string reason, out string smallreason, bool forObshEtaps = false)
        {
            var reasonBuilder = new StringBuilder($"для вида тех. операции {condition.OperationType} задан набор условий у {op1.GUID} и {op2.GUID}");

            // проверка группы заменяемости
            if (condition.CheckWorkCenterRoute || forObshEtaps)
            {
                var op1Centers = op1.PossibleCenters ?? new List<Uco> { op1.OriginalCenter };
                var op2Centers = op2.PossibleCenters ?? new List<Uco> { op2.OriginalCenter };

                if (!op1Centers.Intersect(op2Centers).Any())
                {
                    reasonBuilder.Append("нет пересечения по РЦ");
                    reason = reasonBuilder.ToString();
                    smallreason = "нет пересечения по РЦ";
                    return false;
                }
            }

            // проверка материалов
            if (op1.StageMaterialsInfo == null)
            {
                reasonBuilder.Append($"у {op1.Index} пустой материал");
                reason = reasonBuilder.ToString();
                smallreason = "пустой материал";
                return false;
            }
            if (op2.StageMaterialsInfo == null)
            {
                reasonBuilder.Append($"у {op2.Index} пустой материал");
                reason = reasonBuilder.ToString();
                smallreason = "пустой материал";
                return false;
            }

            // сравнение материалов
            if (condition.CheckMaterials && !op1.StageMaterialsInfo.IsEqualTo(op2.StageMaterialsInfo))
            {
                //reasonBuilder.Append($"разные материалы - {op1.MaterialLink.Datas.Select(x => $"'{x.Material}' (хар. {x.Charac})").Join("; ")} и {op2.MaterialLink.Datas.Select(x => $"'{x.Material}' (хар. {x.Charac})").Join("; ")}");
                //reason = reasonBuilder.ToString();
                //smallreason = "разные материалы";

                reasonBuilder.Append($"разные материалы - {op1.StageMaterialsInfo.UsedMaterials.Select(x => $"'{x.Nomenklature}' (хар. {x.Characteristics})").Join("; ")} и {op2.StageMaterialsInfo.UsedMaterials.Select(x => $"'{x.Nomenklature}' (хар. {x.Characteristics})").Join("; ")}");
                reason = reasonBuilder.ToString();
                smallreason = "разные материалы";
                return false;
            }

            if (condition.PropsConditions != null)
            {
                for (int i = 0; i < condition.PropsConditions.Count; i++)
                {
                    var prop = condition.PropsConditions[i];

                    // Проверим все три возможных типа, если они тут есть
                    if (

                        (prop.NumberIndex > -1 && op1.NumberProps[prop.NumberIndex] != op2.NumberProps[prop.NumberIndex])
                        ||
                        (prop.GuidIndex > -1 && op1.GuidProps[prop.GuidIndex] != op2.GuidProps[prop.GuidIndex])
                        ||
                        (prop.StringIndex > -1 && op1.StringProps[prop.StringIndex] != op2.StringProps[prop.StringIndex])

                        )
                    {

                        reasonBuilder.Append($"не равен реквизит {prop.PropLabel}");
                        reason = reasonBuilder.ToString();
                        smallreason = $"не равен реквизит {prop.PropLabel}";
                        return false;
                    }
                }
            }

            smallreason = reason = string.Empty;

            return true;
        }
        #endregion
    }
}
