using UniversalPack.Common;
using Uco = UniversalPack.Common.CodedObject<System.Guid, string>;

namespace ApsRT.Core.DataErrors
{
    public struct DataErrorStruct
    {
        /*
         * Набор РЦ на текущей операции ☑
         * Подходящая тара с максимальной намотки ☑
         * Отдающая тара на текущей операции ☑
         * Тип тары приемная на текущей операции; ☑
         * максимальная намотка ☑
         * Пересечение макс намотки и применяемой тары
         * Набор РЦ на предыдущей операции
         * Тип  отдающей тары на предыдущей операции,вспомогательная истина/ложь;максимальная намотка
         * Следующий этап
         * Набор РЦ на следующей операции
         * Тип отдающей тары на следующей операции;максимальная намотка
         * Количество на уровне в партии
        */
        internal string DataToInsert; // строка, которую можно вставить в основное сообщение
        internal string TechData; // строка, которую можно вставить в основное сообщение
        internal double Count;
        internal double NaladLength;
        internal string PartiaSettings; // список настроек партий
        internal Guid? PartiaGuid;
        internal List<(Guid Code, int Version)> customEtapsGuids;
        internal Uco? customRC, customRCroute;
        internal Uco? customNomen, customProd, customVid;
        internal List<Uco> RCsOnThisEtap; // Набор РЦ на текущей операции
        internal List<Uco> TarasOnThisEtap; // Тары на текущем этапе - Тип тары приемная на текущей операции
        internal List<(Uco Tara, bool WasVspom)> NeededOtdTarasOnThisEtap; // Требуемые отдающие тары здесь - Отдающая тара на текущей операции
        internal List<(Uco Tara, double Max)> MaxNamotka; // максимальная намотка
        internal List<Uco> MaxPlusUsedTaras; // Пересечение максимальных намоток и применяемых тар - Подходящая тара с максимальной намотки
        internal List<Uco> RCsOnNextEtap; // РЦ на след уровне
        internal List<Uco> TarasOnNextEtap; // тары на след. уровне

        internal List<Uco> RCsOnPrevEtap; // Набор РЦ на текущей операции
        internal List<Uco> TarasOnPrevEtap; // Тары на текущем этапе - Тип тары приемная на текущей операции

        public DataErrorStruct(string dataToInsert) : this(dataToInsert, null, null, null, null, null, null, null, null) { }
        public DataErrorStruct(string dataToInsert, string moreData) : this(dataToInsert, null, null, null, null, null, null, null, moreData) { }

        public DataErrorStruct(
            string dataToInsert,
            List<Uco> rCsOnThisEtap,
            List<Uco> tarasOnThisEtap,
            List<(Uco Tara, bool WasVspom)> neededOtdTarasOnThisEtap,
            List<(Uco Tara, double Max)> maxNamotka,
            List<Uco> maxPlusUsedTaras,
            List<Uco> rCsOnNextEtap,
            List<Uco> tarasOnNextEtap,
            string moreData = null
            )
        {
            DataToInsert = dataToInsert;
            RCsOnThisEtap = rCsOnThisEtap;
            TarasOnThisEtap = tarasOnThisEtap;
            NeededOtdTarasOnThisEtap = neededOtdTarasOnThisEtap;
            MaxNamotka = maxNamotka;
            MaxPlusUsedTaras = maxPlusUsedTaras;
            TechData = moreData;
            RCsOnNextEtap = rCsOnNextEtap;
            TarasOnNextEtap = tarasOnNextEtap;
            Count = 0;
            NaladLength = 0;
            PartiaSettings = null;
            PartiaGuid = null;
            customEtapsGuids = null;
            customRC = null;
            customRCroute = null;
            RCsOnPrevEtap = null;
            TarasOnPrevEtap = null;
            customNomen = customProd = customVid = null;
        }
    }

}
