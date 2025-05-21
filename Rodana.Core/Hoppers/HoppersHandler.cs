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
using UniversalPack;

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
                    ModuleActions(module.Data, module.Interface, Paths[module.Name]);
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

        public Action<HopperAttribute, IHopperInterface<InputDataType, OrderSubType>> OnStartLoadingEvent;
        public Action<HopperAttribute, IHopperInterface<InputDataType, OrderSubType>> OnLoadingEvent;
        public Action<HopperAttribute, IHopperInterface<InputDataType, OrderSubType>> OnEndLoadingEvent;

        public void ModuleActions(HopperAttribute hopperData, IHopperInterface<InputDataType, OrderSubType> hopperInterface, string[] paths)
        {
            if (session.SessionOptions.PartOrZadel && !hopperData.LoadWhenOnlyPart) // если только партии (ну или заделы), то ничего не грузим
            {
                session.AssignedLogger.LogQueue.Enqueue($"[{DateTime.Now}]: module '{hopperData.Label}' ignored bsc ONLY PART.\r\n");
                hopperData.LoadedFlag = true;
                OnEndLoadingEvent?.Invoke(hopperData, hopperInterface);
                return;
            }

            if (hopperData.NeededHoppers != null)
            {
                for (var i = 0; i < hopperData.NeededHoppers.Length; i++)
                {
                    var name = hopperData.NeededHoppers[i];
                    if (!ModulesList[n].LoadedFlag)
                    {
                        session.AssignedLogger.LogQueue.Enqueue($"Сначала загрузите модуль '{ModulesList[n].HopperData.Label}'.");
                    }
                }
            }
            hopperInterface.BeforeAllLoading(session, paths);

            for (var z = 0; z < paths.Length; z++)
            {
                var path = paths[z];

                if (hopperData.IsDirectory)
                {
                    // это папочная хрень
                    var filesthere = Directory.GetFiles(path);
                    hopper.Bar.Maximum = filesthere.Length;
                    hopper.Bar.Minimum = 0;
                    hopper.Bar.Value = 0;
                    foreach (var file in filesthere)
                    {
                        hopperInterface.ActionFile(file);
                        hopper.Bar.Value++;
                    }
                }
                else
                {
                    // получение данных из SQL
                    if (path.StartsWith("SQL:"))
                    {
                        // Создание подключения
                        ModuleActionSQL(hopper, path, Show);
                    }
                    else
                    {
                        ModuleActionFile(hopper, path, Show);
                    }
                }
            }
            hopperInterface.AfterLoading();

            session.AssignedLogger.LogQueue.Enqueue($"[{DateTime.Now}]: module '{hopperData.Label}' loaded.\r\n");
            hopperData.LoadedFlag = true;
            OnEndLoadingEvent?.Invoke(hopperData, hopperInterface);
        }

    }
}
