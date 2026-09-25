using ApsRT.Core.DataErrors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uco = UniversalPack.Common.CodedObject<System.Guid, string>;
using OrderID = UniversalPack.Common.CodedObjectVersioned<System.Guid, string, string>;

namespace ApsRT.Core.Orders
{
    public class OrdersMasterData
    {

        public Session.Session SessionLink { get; }

        public OrdersMasterData(Session.Session sessionLink)
        {
            SessionLink = sessionLink;
        }

        public Dictionary<Guid, PartiaPFFlags> PFPartiasData = new();

        public Dictionary<OrderID, OrderBaseModel> AllOrders = new Dictionary<OrderID, OrderBaseModel>();

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

            try
            {
                finalOrderModel.ShortName = $"{finalOrderModel.OrderID.Object.Label.Split(' ')[3]} ({finalOrderModel.OrderID.Version})";
            }
            catch
            {

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

                finalOrderModel.PartiaPFFlags = PFPartiasData[pfPartia.Value];

                var partia = finalOrderModel.PartiaPFFlags;
                if (!SessionLink.SettingsController.CurrentSettings.Demos_AllPartiasAreFree)
                {
                    partia.Fixed |= stage.Fixed;
                    partia.IsFact |= stage.IsFact;
                    partia.IsUserHandled |= stage.IsUserHandled;
                    partia.VPZ |= stage.VPZ;
                }
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
            var koeffs = SessionLink.SettingsController.CurrentSettings.OrderCoeffs[priority];
            if (days < koeffs.Level1.UpTo)
            {
                return Math.Pow(days, koeffs.Level1.A) * koeffs.Level1.K;
            }
            else
            {
                if (days < koeffs.Level2.UpTo)
                {
                    return Math.Pow(days, koeffs.Level2.A) * koeffs.Level2.K;
                }
                else
                {
                    return Math.Pow(days, koeffs.Level3.A) * koeffs.Level3.K;
                }
            }
        }

        public string NoMergeFilename = "NoMergeExceptions.txt";
        public void ProcessOrdersNoMerge()
        {
            if (!File.Exists(NoMergeFilename)) File.WriteAllLines(NoMergeFilename, Array.Empty<string>());

            var reader = new StreamReader(NoMergeFilename);

            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                try
                {
                    line = line.Trim().ToLower();
                    var foundOrders = AllOrders.Where(x => x.Key.Object.Label.ToLower() == line);
                    foreach (var order in foundOrders)
                    {
                        order.Value.WillBeNoMergeExcepted = true;

                        foreach (var opIndex in order.Value.RelatedOpers)
                        {
                            var op = SessionLink.MasterDataLinks.StagesMasterData.AllStages[opIndex];
                            var str = $"op {opIndex}: {op.GUID} [{op.StageVersion}] - {op.Registrator} - НеОбъединять: ЛОЖЬ";
                            op.IsLast = false;
                            op.IsFact = false;
                            Console.WriteLine(str);
                            SessionLink.AssignedLogger.LogQueue.Enqueue(str + "\r\n");
                        }

                        var str2 = $"No Merge: Заказ '{order.Key}' - убраны флаги 'НеОбъединять'.";
                        Console.WriteLine(str2);
                        SessionLink.AssignedLogger.LogQueue.Enqueue(str2 + "\r\n");
                    }
                }
                catch
                {
                    Console.WriteLine($"Exceptions: Неизвестный заказ '{line}'");
                    SessionLink.AssignedLogger.LogQueue.Enqueue($"Exceptions: Неизвестный заказ '{line}'.\r\n");
                }
            }
            reader.Close();
        }


        public string ExceptionsFilename = "Exceptions.txt";
        public void ProcessOrdersExceptions()
        {
            if (!File.Exists(ExceptionsFilename)) File.WriteAllLines(ExceptionsFilename, Array.Empty<string>());

            var reader = new StreamReader(ExceptionsFilename);

            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                try
                {
                    line = line.Trim().ToLower();
                    var foundOrders = AllOrders.Where(x => x.Key.Object.Label.ToLower() == line);
                    foreach (var order in foundOrders)
                    {
                        order.Value.WillBeExcepted = true;
                        Console.WriteLine($"Exceptions: Заказ '{order.Key}' помечен исключённым.");
                        SessionLink.AssignedLogger.LogQueue.Enqueue($"Exceptions: Заказ '{order.Key}' помечен исключённым.\r\n");
                    }
                }
                catch
                {
                    Console.WriteLine($"Exceptions: Неизвестный заказ '{line}'");
                    SessionLink.AssignedLogger.LogQueue.Enqueue($"Exceptions: Неизвестный заказ '{line}'.\r\n");
                }
            }
            reader.Close();
        }

    }
}
