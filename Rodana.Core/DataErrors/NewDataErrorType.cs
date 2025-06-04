namespace Rodana.Core.DataErrors
{
    public enum NewDataErrorType : byte
    {
        NoProduction,
        NoNomenklature,
        NoVidOper,

        NoNomenklatureEmptyProd,
        NoVidOperEmptyProd,

        NoGP, // гп не задана

        Restricted_VPZ,
        Restricted_Fact,
        Restricted_NoReplan,

        NoOrigTara, // не задана конечная тара

        NoChangeTaraNoPriem, // нет приемной тары при "не менять тару"
        NoChangeTaraNoPriemVspom, // То же, но с вспомогательной
        NoChangeTaraMoreThanOne, // апс откопал 2+ "не менять тару

        NonPositiveLength, // отрицательная длительность
        BadStartTimeFact, // дата начала факт

        SomeFileIsOpened, // открыты файлы
        CorruptedDataFile, // повреждены файлы с данными

        NotEnoughMaxOnTaras, // тара найдена, но не хватает макс намотки
        NotEnoughMaxOnTarasVspom, // то же, но вспом
        NoTaraNMaxIntersection, // Не найдена основная тара для рассчитанного количества с учетом выбранной тары по макс намотке и приемной таре по цепочке по таре.
        NoTaraNMaxIntersectionVspom, // то же, но вспом
        NoPriemNOtdIntersection, // Нарушение цепочки приемная-отдающая тара по основной таре
        NoPriemNOtdIntersectionVspom, // то же, но со вспомогательной
        NoPriemNOtdIntersectionNoChange, // то же, но с признаком не менять тару

        NoPriemOrigTara, // тут не найдена приемная отдающая тара
        NoOtdOrigTara, // нет РЦ с нужной отдающей тарой
        NoPriemOrigTaraOnNextEtap, // на след этапе не найдена приемная отдающая тара

        WasNotAbleToMerge, // не смог соединить на 1 уровне
        WasNotAbleToMergeNoSettings, // то же, но без настроек
        WasNotAbleToMergeMax, // не влезло на одну
        WasNotAbleToMergeDontKnow, // хз почему

        NoAllNeededOtdTaras, // нет ни одного рц у которого есть все нужные отдающие тары
        NoAllNeededOtdTarasWithCheck, // то же, с доп проверкой
    }

}
