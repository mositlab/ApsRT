using Rodana.Core.Session;
using Rodana.Core.Settings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Rodana.Core.DataErrors;

namespace Rodana.Core.Hoppers
{
    public class HoppersHandler<InputDataType, OrderSubType>
    {

        Session<InputDataType, OrderSubType> session { get; init; }

        public List<(string Name, HopperAttribute Data, IHopperInterface<InputDataType, OrderSubType> Interface)> Hoppers { get; init; }
        public HoppersHandler(Session<InputDataType, OrderSubType> session)
        {
            this.session = session;
            Hoppers = new();
            FindAllHoppers();
        }

        void FindAllHoppers()
        {
            var asm = Assembly.GetAssembly(typeof(IHopperInterface<InputDataType, OrderSubType>));
            var hoppers = asm.GetTypes()
                .Where(x =>
                x.CustomAttributes.Any(y => y.AttributeType == typeof(HopperAttribute)) // ищем те типы, у кого есть нужный атрибут
                //&&
                //x.GetInterfaces().Any(y => y == typeof(IHopperInterface<InputDataType, OrderSubType>)) // ещё одно ограничение, чтобы он был унаследован
                )
                .ToList();

            Dictionary<string, (int pri, IHopperInterface<InputDataType, OrderSubType> intr, HopperAttribute attr)> data = new();
            foreach (var el in hoppers)
            {
                var attr = el.GetCustomAttribute<HopperAttribute>();

                Type t = el;

                Type[] genericTypeParameters = typeof(IHopperInterface<InputDataType, OrderSubType>).GenericTypeArguments;

                t = t.MakeGenericType(genericTypeParameters);

                var inst = Activator.CreateInstance(t) as IHopperInterface<InputDataType, OrderSubType>;
                if (!data.ContainsKey(el.Name/*attr.Name*/))
                {
                    data.Add(el.Name/*attr.Name*/, (0, inst, attr));
                }
            }
            var keys = data.Keys.ToArray();


            void add(string what)
            {
                var dic = data[what];
                dic.pri++;
                data[what] = dic;
                if (dic.attr.NeededHoppers != null)
                {
                    foreach (var prev in dic.attr.NeededHoppers)
                    {
                        add(prev);
                    }
                }
            }

            foreach (var key in keys)
            {
                add(key);
            }
            var data2 = data.OrderByDescending(x => x.Value.pri).ThenBy(x => x.Key).Select(x => (x.Key, x.Value.intr, x.Value.attr)).ToList();
            foreach (var el in data2)
            {
                Hoppers.Add((el.Key, el.attr, el.intr));
            }
        }

        public int LoadAllHoppersAsync(Dictionary<string, string[]> Paths)
        {
            if (Paths == null || Paths.Count == 0) return -1;
            byte CurrentModuleLoading = 0;
            foreach (var module in Hoppers)
            {
                //var module = ModulesList.ElementAt(i);//.Value;
                if (!Paths.ContainsKey(module.Name) || Paths[module.Name].Length == 0 || module.Data.LoadedFlag)
                {
                    continue;//return;
                }

                try
                {
                    //ModuleActions(module.Value, paths[module.Key]);
                }
                catch (Exception ex)
                {
                    try
                    {
                        session.DataErrors.AddNewPartiaError(-CurrentModuleLoading - 50, DataErrors.NewDataErrorType.CorruptedDataFile, new DataErrorStruct<InputDataType>(module.Data.Label, ex.Message));
                        var t = new System.Diagnostics.StackTrace(true);
                        var mes =
                            $"\r\nMessage: {ex.Message}" +
                            $"\r\nSource: {ex.Source}" +
                            $"\r\nStack trace: {ex.StackTrace}" +
                            $"\r\nStack trace 2: {t}\r\n";
                        session.AssignedLogger.LogQueue.Enqueue($"Ошибка с файлами модуля {module.Name}: {mes}");
                    }
                    catch
                    {
                    }
                    //return CurrentModuleLoading;
                }
                CurrentModuleLoading++;
            }
            return -1;
        }
    }
}
