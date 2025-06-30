using ApsRT.Core.Rechanges.Hoppers;
using ApsRT.Core.Rechanges.RechangesEngine;
using ApsRT.Core.Stages;

namespace ApsRT.Core.Rechanges
{
    /// <summary>
    /// Универсальный ритерий переналадки. Содержит данные о том, как с этим критерием работать
    /// </summary>
    public class RechangeUniversalCriteria
    {
        public RechangeUniversalCriteria(RechangesCriteriaDataLoaderModel model)
        {
            Name = model.Name;
            CanBeNumber = model.CanBeNumber;
            CanBeDiapazon = model.CanBeDiapazon;
            CanBeString = model.CanBeString;
            CanBeGuid = model.CanBeGuid;
            CanBeMultiple = model.CanBeMultiple;
            Sort = model.Sort;
            IsLink = model.IsLink;
            IsDynamic = model.IsDynamic;

            if (IsLink)
            {
                LinkDataObtainer = CustomRechangeCriteriaMethods.MethodsArray[model.MethodLinkIndex];
                LinkValuesCountObtainer = CustomRechangeCriteriaMethods.MethodsValuesCountArray[model.MethodLinkIndex];
            }

            (string, string)[] columnNames = null;
            var columnValue = model.CalumnValueNames;
            //ИменаСтолбцовТаблицыОперацийМетки
            if (!string.IsNullOrEmpty(columnValue))
            {
                string[] values = columnValue.Split(',');
                var label = model.CalumnLabelNames;
                string[] labels = string.IsNullOrEmpty(label) ? null : label.Split(',');
                columnNames = values.Select((x, index) => (x.Trim(), labels?[index].Trim())).ToArray();
            }

            StagesTableColumnsNames = columnNames;
        }

        /// <summary>
        /// Имя критерия
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Список имён столбцов, откуда в таблице операций брать данные
        /// </summary>
        public (string ValueColumn, string LabelColumn)[] StagesTableColumnsNames { get; }

        /// <summary>
        /// Может быть числом? Как например сечение
        /// </summary>
        public bool CanBeNumber { get; }

        /// <summary>
        /// Может быть диапазоном? Как например диаметры и сечение
        /// </summary>
        public bool CanBeDiapazon { get; }

        /// <summary>
        /// Может быть строкой? Как, к примеру, вид металла или красители
        /// </summary>
        public bool CanBeString { get; }

        /// <summary>
        /// Может ли быть идентификатором, как например тех операция или изоляция.
        /// Можно сюда передать кодовый объект без имени (только гуид), но пока что не было примеров производств, где так надо.
        /// Будут сравниваться только гуиды
        /// </summary>
        public bool CanBeGuid { get; }

        /// <summary>
        /// Возможна ли многозначная проверка
        /// </summary>
        public bool CanBeMultiple { get; }

        /// <summary>
        /// Нужно ли сортировать значения - как например в материалах
        /// </summary>
        public bool Sort { get; }

        /// <summary>
        /// Вместо хранения значений, этап хранит у себя функцию, с помощью которой выдаст объект, который придётся распаковать, и вот с ним уже можно будет работать.
        /// </summary>
        public bool IsLink { get; }

        /// <summary>
        /// Непосредственно сама ссылка, позволяющая получить данные
        /// </summary>
        public Func<Stage, RechangesEngineData, Type, int, object> LinkDataObtainer;

        public Func<Stage, int> LinkValuesCountObtainer;

        /// <summary>
        /// У него постоянно меняются значения? В таком случае наличие этого критерия заставить АПС хранить данные не в статичном, а в динамичном кэше, а так же постоянно сравнивать диаметры
        /// </summary>
        public bool IsDynamic { get; }
    }

}
