namespace Rodana.Core.Rechanges
{
    /// <summary>
    /// Универсальный ритерий переналадки. Содержит данные о том, как с этим критерием работать
    /// </summary>
    internal class RechangeUniversalCriteria
    {
        public RechangeUniversalCriteria(string name, bool canBeNumber, bool canBeDiapazon, bool canBeString, bool canBeGuid, bool canBeMultiple, bool sort, bool isLink, bool isDynamic, (string ValueColumn, string LabelColumn)[] stagesTableColumnsNames)
        {
            Name = name;
            CanBeNumber = canBeNumber;
            CanBeDiapazon = canBeDiapazon;
            CanBeString = canBeString;
            CanBeGuid = canBeGuid;
            CanBeMultiple = canBeMultiple;
            Sort = sort;
            IsLink = isLink;
            StagesTableColumnsNames = stagesTableColumnsNames;
            IsDynamic = isDynamic;
        }

        /// <summary>
        /// Имя критерия
        /// </summary>
        internal string Name { get; }

        /// <summary>
        /// Список имён столбцов, откуда в таблице операций брать данные
        /// </summary>
        internal (string ValueColumn, string LabelColumn)[] StagesTableColumnsNames { get; }

        /// <summary>
        /// Может быть числом? Как например сечение
        /// </summary>
        internal bool CanBeNumber { get; }

        /// <summary>
        /// Может быть диапазоном? Как например диаметры и сечение
        /// </summary>
        internal bool CanBeDiapazon { get; }

        /// <summary>
        /// Может быть строкой? Как, к примеру, вид металла или красители
        /// </summary>
        internal bool CanBeString { get; }

        /// <summary>
        /// Может ли быть идентификатором, как например тех операция или изоляция.
        /// Можно сюда передать кодовый объект без имени (только гуид), но пока что не было примеров производств, где так надо.
        /// Будут сравниваться только гуиды
        /// </summary>
        internal bool CanBeGuid { get; }

        /// <summary>
        /// Возможна ли многозначная проверка
        /// </summary>
        internal bool CanBeMultiple { get; }

        /// <summary>
        /// Нужно ли сортировать значения - как например в материалах
        /// </summary>
        internal bool Sort { get; }

        /// <summary>
        /// Вместо хранения значений, этап хранит у себя функцию, с помощью которой выдаст объект, который придётся распаковать, и вот с ним уже можно будет работать.
        /// </summary>
        internal bool IsLink { get; }

        /// <summary>
        /// У него постоянно меняются значения? В таком случае наличие этого критерия заставить АПС хранить данные не в статичном, а в динамичном кэше, а так же постоянно сравнивать диаметры
        /// </summary>
        internal bool IsDynamic { get; }
    }

}
