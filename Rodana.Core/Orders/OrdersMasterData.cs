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

        public Session.Session SessionLink { get; }

        public OrdersMasterData(Session.Session sessionLink)
        {
            SessionLink = sessionLink;
        }

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

            finalOrderModel.RelatedOpers.Add(stage.Index);

            return finalOrderModel;
        }

        /// <summary>
        /// Подсчёт дедлайна для заказа
        /// </summary>
        /// <param name="daysG"></param>
        /// <param name="ordernG"></param>
        /// <param name="priority"></param>
        /// <returns></returns>
        public double CalcDeadline(double days, OrderID ordernG, int priority)
        {
            var koeffs = SessionLink.SettingsController.CurrentSettings.OrderCoeffs[priority > 2 ? 2 : priority];
            if (days < koeffs.Level1.To)
            {
                return Math.Pow(days, koeffs.Level1.A) * koeffs.Level1.K;
            }
            else
            {
                if (days >= koeffs.Level2.From && days < koeffs.Level2.To)
                {
                    return Math.Pow(days, koeffs.Level2.A) * koeffs.Level2.K;
                }
                else
                {
                    return Math.Pow(days, koeffs.Level3.A) * koeffs.Level3.K;
                }
            }
        }

    }
}
