using Rodana.Core.Common;

namespace Rodana.Core.DataErrors
{
    public struct DataErrorStruct<InputDataType>
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
        internal string PartiaSettings; // список настроек партий
        internal Guid? PartiaGuid;
        internal List<Guid> customEtapsGuids;
        internal CodedObject<InputDataType>? customRC, customRCroute;
        internal CodedObject<InputDataType>? customNomen, customProd, customVid;
        internal List<CodedObject<InputDataType>> RCsOnThisEtap; // Набор РЦ на текущей операции
        internal List<CodedObject<InputDataType>> TarasOnThisEtap; // Тары на текущем этапе - Тип тары приемная на текущей операции
        internal List<(CodedObject<InputDataType> Tara, bool WasVspom)> NeededOtdTarasOnThisEtap; // Требуемые отдающие тары здесь - Отдающая тара на текущей операции
        internal List<(CodedObject<InputDataType> Tara, double Max)> MaxNamotka; // максимальная намотка
        internal List<CodedObject<InputDataType>> MaxPlusUsedTaras; // Пересечение максимальных намоток и применяемых тар - Подходящая тара с максимальной намотки
        internal List<CodedObject<InputDataType>> RCsOnNextEtap; // РЦ на след уровне
        internal List<CodedObject<InputDataType>> TarasOnNextEtap; // тары на след. уровне

        internal List<CodedObject<InputDataType>> RCsOnPrevEtap; // Набор РЦ на текущей операции
        internal List<CodedObject<InputDataType>> TarasOnPrevEtap; // Тары на текущем этапе - Тип тары приемная на текущей операции

        public DataErrorStruct(string dataToInsert) : this(dataToInsert, null, null, null, null, null, null, null, null) { }
        public DataErrorStruct(string dataToInsert, string moreData) : this(dataToInsert, null, null, null, null, null, null, null, moreData) { }

        public DataErrorStruct(
            string dataToInsert,
            List<CodedObject<InputDataType>> rCsOnThisEtap,
            List<CodedObject<InputDataType>> tarasOnThisEtap,
            List<(CodedObject<InputDataType> Tara, bool WasVspom)> neededOtdTarasOnThisEtap,
            List<(CodedObject<InputDataType> Tara, double Max)> maxNamotka,
            List<CodedObject<InputDataType>> maxPlusUsedTaras,
            List<CodedObject<InputDataType>> rCsOnNextEtap,
            List<CodedObject<InputDataType>> tarasOnNextEtap,
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
