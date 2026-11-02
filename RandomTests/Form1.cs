using System.Data.Common;
using System.Runtime.InteropServices;

namespace RandomTests
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private object? _connection;

        private static void ReleaseComObject(object? value)
        {
            if (value != null && Marshal.IsComObject(value))
            {
                try { Marshal.FinalReleaseComObject(value); } catch { }
            }
        }

        private static string BuildConnectionString() =>
            $"Srvr=\"172.31.0.33:1841\";Ref=\"ZMK_MES13_RECOVER\";Usr=\"ТрушинВА\";Pwd=\"\";";

        private void button1_Click(object sender, EventArgs e)
        {

            Type? connectorType = Type.GetTypeFromProgID("V83.COMConnector");
            if (connectorType == null)
                throw new InvalidOperationException("Не установлена 64-битная платформа 1С или не зарегистрирован V83.COMConnector.");

            object? connector = null;

            connector = Activator.CreateInstance(connectorType)
                ?? throw new InvalidOperationException("Не удалось создать COMConnector 1С.");
            dynamic dynamicConnector = connector;
            object newConnection = dynamicConnector.Connect(BuildConnectionString());
            //ReleaseComObject(dynamicConnector);
            _connection = newConnection;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            dynamic connection = _connection ?? throw new InvalidOperationException("MES не подключена.");
            object? query = null;
            object? result = null;
            object? selection = null;
            try
            {
                query = connection.NewObject("Query");
                dynamic dynamicQuery = query;
                dynamicQuery.Text = @"

ВЫБРАТЬ ПЕРВЫЕ 10
    ОчередьОпераций.МаршрутнаяКарта КАК МаршрутнаяКарта,
    МаршрутнаяКарта.Ссылка КАК СсылкаДокумента
ИЗ
    РегистрСведений.оупОчередьОперацийМаршрутныхКарт КАК ОчередьОпераций
    ВНУТРЕННЕЕ СОЕДИНЕНИЕ Документ.оупМаршрутнаяКартаПроизводства КАК МаршрутнаяКарта
    ПО ОчередьОпераций.МаршрутнаяКарта = МаршрутнаяКарта.Ссылка

";
                result = dynamicQuery.Execute();
                dynamic dynamicResult = result;
                selection = dynamicResult.Select();
                dynamic dynamicSelection = selection;
                var centers = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                while (dynamicSelection.Next())
                {
                    Console.WriteLine(dynamicSelection.Заказ);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Console.WriteLine($"HRESULT: 0x{ex.HResult:X8}");

                if (ex.InnerException != null)
                    Console.WriteLine(ex.InnerException.ToString());

                //throw;
            }
            finally
            {
                ReleaseComObject(selection);
                ReleaseComObject(result);
                ReleaseComObject(query);
            }
        }
    }
}
