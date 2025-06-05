using Rodana.Core.DataErrors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uco = UniversalPack.Common.CodedObject<System.Guid, string>;
using OrderID = UniversalPack.Common.CodedObjectVersioned<System.Guid, string, string>;

namespace Rodana.Core.Orders
{
    public class OrdersMasterData
    {

        internal Dictionary<Guid, PartiaPFFlags> PFPartiasData = new();

        internal Dictionary<OrderID, OrderBaseModel> AllOrders = new Dictionary<OrderID, OrderBaseModel>();

        public OrderBaseModel ProcessOrderData(Stages.Stage stage, OrderBaseModel orderModel)
        {
            OrderBaseModel finalOrderModel;
            if (AllOrders.TryGetValue(orderModel.OrderID, out var ord))
            {
                finalOrderModel = ord;
            }
            else
            {
                AllOrders.Add(orderModel.OrderID, orderModel);
                finalOrderModel = orderModel;
            }

            Guid? pfPartia = finalOrderModel.PartiaPF;
            if (pfPartia.HasValue && pfPartia == Guid.Empty)
            {
                pfPartia = finalOrderModel.PartiaPF = null;
            }
            if (pfPartia.HasValue)
            {
                if (!PFPartiasData.ContainsKey(pfPartia.Value))
                {
                    var flags = new PartiaPFFlags();
                    flags.RelatedGPs = new HashSet<OrderID>(); // Создаём пустой список
                    PFPartiasData.Add(pfPartia.Value, flags);
                }

                var partia = PFPartiasData[pfPartia.Value];
                partia.Fixed |= stage.Fixed;
                partia.IsFact |= stage.IsFact;
                partia.IsUserHandled |= stage.IsUserHandled;
                partia.VPZ |= stage.VPZ;
                partia.RelatedGPs.Add(orderModel.OrderID); // Добавляем эту ГПшку в список тех, к кому он относится
            }

            return finalOrderModel;
        }

    }
}
