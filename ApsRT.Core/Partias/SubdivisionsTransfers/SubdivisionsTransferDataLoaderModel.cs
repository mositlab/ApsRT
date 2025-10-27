using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UniversalPack.Common;
using ApsRT.Core.Session;
using ApsRT.Core.Settings;
using InfimumMES;
using ApsRT.Core.Rechanges.RechangesEngine;
using ApsRT.Core.Rechanges;
using ApsRT.Core.Rechanges.MasterData;
using Uco = UniversalPack.Common.CodedObject<System.Guid, string>;
using UcoDU = UniversalPack.Common.CodedObject<System.Guid, UniversalPack.DimensionUnit>;
using UniversalPack.Common.DataLoader;
using UniversalPack.Attributes;
using UniversalPack;
using ApsRT.Core.Orders;
using ApsRT.Core.NaladLengths;
using System.Drawing;

namespace ApsRT.Core.Partias.SubdivisionsTransfers
{
    public class SubdivisionsTransferDataLoaderModel : BaseDataLoaderModel
    {
        private static readonly ICollection<FieldConverterInfo> _fieldConverters;

        static SubdivisionsTransferDataLoaderModel()
        {
            _fieldConverters = DefineFieldConverters<SubdivisionsTransferDataLoaderModel>();
        }

        protected override IEnumerable<FieldConverterInfo> FieldConverters => _fieldConverters;

        [DataRecordCodedObjectColumn("ПодразделениеИсточник_GUID", "ПодразделениеИсточник")]
        public Uco SubFrom { get; set; }

        [DataRecordCodedObjectColumn("ПодразделениеПриемник_GUID", "ПодразделениеПриемник")]
        public Uco SubTo { get; set; }

        [DataRecordColumn("ДлительностьПеремещения")]
        public int Duration { get; set; }
    }
}
