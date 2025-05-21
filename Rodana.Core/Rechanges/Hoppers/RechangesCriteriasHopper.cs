using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UniversalPack.Common;
using UniversalPack.Common.Planning;
using UniversalPack.Extensions;

namespace InfimumMES
{
    [Hopper(Label = "Критерии переналадок", LoadWhenOnlyPart = true, IsDirectory = false, IsMultiple = false, UsesGuid = false)]
    internal class RechangesCriteriasHopper : IHopperInterface
    {
        public void ActionFile(string filename)
        {

        }

        public void AfterLoading()
        {
            RechangesMasterData.ExistingCriterias = list.ToArray();
        }

        List<RechangeUniversalCriteria> list;

        public void BeforeAllLoading(string[] filenames)
        {
            list = new();
        }

        public void BeforeLoading(string filename, HopperReadOptions options)
        {

        }

        public void OnLoading(int rowIndex, IDataReader data, HopperReadOptions options)
        {
            (string, string)[] columnNames = null;
            var columnValue = data["ИменаСтолбцовТаблицыОперацийЗначения"].ConvertToOrDefault(string.Empty);
            //ИменаСтолбцовТаблицыОперацийМетки
            if (!string.IsNullOrEmpty(columnValue))
            {
                string[] values = columnValue.Split(',');
                var label = data["ИменаСтолбцовТаблицыОперацийМетки"].ConvertToOrDefault(string.Empty);
                string[] labels = string.IsNullOrEmpty(label) ? null : label.Split(',');
                columnNames = values.Select((x, index) => (x.Trim(), labels?[index].Trim())).ToArray();
            }
            RechangeUniversalCriteria criteria = new RechangeUniversalCriteria(
                data["Наименование"].ConvertToOrDefault(string.Empty),
                data["ЭтоЧисло"].ConvertToOrDefault(string.Empty).IsYes(),
                data["ЭтоДиапазон"].ConvertToOrDefault(string.Empty).IsYes(),
                data["ЭтоСтрока"].ConvertToOrDefault(string.Empty).IsYes(),
                data["ЭтоИдентификатор"].ConvertToOrDefault(string.Empty).IsYes(),
                data["Многозначный"].ConvertToOrDefault(string.Empty).IsYes(),
                data["Сортировать"].ConvertToOrDefault(string.Empty).IsYes(),
                data["ЭтоСсылка"].ConvertToOrDefault(string.Empty).IsYes(),
                data["Динамичный"].ConvertToOrDefault(string.Empty).IsYes(),
                columnNames
                );
            list.Add(criteria);
        }
    }
}
