using Newtonsoft.Json;
using UniversalPack.Common;

namespace Rodana.Core.Centers
{
    public class CenterBaseModel
    {

        public bool AbleToVspom { get; set; } // умеет ли обрабатывать вспомогательные

        internal bool NotChangeTara;
        internal bool IsFromTable;

        internal bool IsWithRechanges;
        internal UniversalBinaryStorage PossibleRechanges;
        internal bool HasAdditionalActions;
        internal UniversalBinaryStorage PossibleAdditionalActions;

        internal bool CanBeParallel;
        public int? MaxMinutesAwaiting; // макс время ожидания

        public int RowIndex;

    }
}
