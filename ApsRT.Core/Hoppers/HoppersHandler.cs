using Session = ApsRT.Core.Session.Session;
using ApsRT.Core.Settings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using ApsRT.Core.DataErrors;
using UniversalPack;
using ApsRT.Core.Session;
using UniversalPack.Common.DataLoader;
using UniversalPack.Extensions;
using System.Data;
using System.Data.SqlClient;
using UniversalPack.Configuration;
using UniversalPack.Common;
using System.Diagnostics.Metrics;

namespace ApsRT.Core.Hoppers
{
    public class HoppersHandler 
    {

        Session.Session session { get; init; }

        public List<(string Name, HopperAttribute Data, IHopperInterface  Interface)> Hoppers { get; init; }
        Dictionary<string, int> HoppersIndexes { get; init; }
        public HoppersHandler(Session.Session session)
        {
            this.session = session;
            Hoppers = new();
            HoppersIndexes = new();
            FindAllHoppers();
        }

        void FindAllHoppers()
        {
            var asm = Assembly.GetAssembly(typeof(IHopperInterface ));
            var hoppers = asm.GetTypes()
                .Where(x =>
                x.CustomAttributes.Any(y => y.AttributeType == typeof(HopperAttribute)) // ищем те типы, у кого есть нужный атрибут
                &&
                x.GetInterfaces().Any(y => y == typeof(IHopperInterface )) // ещё одно ограничение, чтобы он был унаследован
                )
                .ToList();

            Dictionary<string, (int pri, IHopperInterface  intr, HopperAttribute attr)> data = new();
            foreach (var el in hoppers)
            {
                var attr = el.GetCustomAttribute<HopperAttribute>();
                var inst = Activator.CreateInstance(el) as IHopperInterface;
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
            int i = 0;
            foreach (var el in data2)
            {
                el.attr.HopperIndex = i;
                Hoppers.Add((el.Key, el.attr, el.intr));
                HoppersIndexes.Add(el.Key, i);
                i++;
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
                        session.DataErrors.AddNewPartiaError(-CurrentModuleLoading - 50, DataErrors.NewDataErrorType.CorruptedDataFile, new DataErrorStruct(module.Data.Label, ex.Message));
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

        public event Action<HopperAttribute, IHopperInterface, int> OnStartLoadingEvent;
        public event Action<HopperAttribute, IHopperInterface, int> OnLoadingEvent;
        public event Action<HopperAttribute, IHopperInterface, int> OnEndLoadingEvent;

        public void ModuleActions(HopperAttribute hopperData, IHopperInterface hopperInterface, string[] paths)
        {
            var dt0 = DateTime.Now;
            Console.WriteLine($"hopper: {hopperData.Label}, {dt0}");

            hopperData.LoadedFlag = false;

            if (session.SessionOptions.PartOrZadel && !hopperData.LoadWhenOnlyPart) // если только партии (ну или заделы), то ничего не грузим
            {
                session.AssignedLogger.LogQueue.Enqueue($"[{DateTime.Now}]: module '{hopperData.Label}' ignored bsc ONLY PART.\r\n");
                hopperData.LoadedFlag = true;
                OnEndLoadingEvent?.Invoke(hopperData, hopperInterface, 0);
                return;
            }

            if (hopperData.NeededHoppers != null)
            {
                for (var i = 0; i < hopperData.NeededHoppers.Length; i++)
                {
                    var name = hopperData.NeededHoppers[i];
                    var index = HoppersIndexes[name];
                    if (!Hoppers[index].Data.LoadedFlag)
                    {
                        session.AssignedLogger.LogQueue.Enqueue($"Сначала загрузите модуль '{Hoppers[index].Data.Label}'.");
                    }
                }
            }

            session.SessionOptions.GetPreparedLaunchPath(out var sessionPath);

            for (int i = 0; i < paths.Length; i++)
            {
                if (paths[i].StartsWith("SQL:")) continue;
                paths[i] = Path.IsPathRooted(paths[i]) ? paths[i] : (sessionPath + paths[i]);
            }

            hopperInterface.BeforeAllLoading(session, paths);

            for (var z = 0; z < paths.Length; z++)
            {
                var path = paths[z];

                if (hopperData.IsDirectory)
                {
                    // это папочная хрень
                    var filesthere = Directory.GetFiles(path);
                    OnStartLoadingEvent?.Invoke(hopperData, hopperInterface, filesthere.Length);
                    int i = 0;
                    foreach (var file in filesthere)
                    {
                        hopperInterface.ActionFile(file);
                        OnLoadingEvent?.Invoke(hopperData, hopperInterface, i);
                        i++;//hopper.Bar.Value++;
                    }
                }
                else
                {
                    // получение данных из SQL
                    if (path.StartsWith("SQL:"))
                    {
                        // Создание подключения
                        ModuleActionSQL(hopperData, hopperInterface, path);
                    }
                    else
                    {
                        ModuleActionFile(hopperData, hopperInterface, path);
                    }
                }
            }
            hopperInterface.AfterLoading();

            session.AssignedLogger.LogQueue.Enqueue($"[{DateTime.Now}]: module '{hopperData.Label}' loaded.\r\n");
            hopperData.LoadedFlag = true;
            OnEndLoadingEvent?.Invoke(hopperData, hopperInterface, 0);

            var dt1 = DateTime.Now;
            Console.WriteLine($"hopper: {hopperData.Label}, {dt1}");
            Console.WriteLine($"total time = {dt1-dt0}");
        }

        public void ModuleActionFile(HopperAttribute hopperData, IHopperInterface hopperInterface, string file)
        {
            session.AssignedLogger.LogQueue.Enqueue("Текущий файл: " + file + "\r\n");
            // неблокирующее чтение файла
            using (var stream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete))
            {
                session.AssignedLogger.LogQueue.Enqueue($"[{DateTime.Now}] reader creating\r\n");
                // выбираем читалку на основе расширения файла
                var filetype = Path.GetExtension(file).ToLower();

                var options = new DataLoaderOptions(filetype switch
                {
                    ".xlsx" => SourceType.XLSX,
                    ".txt" => SourceType.TXT,
                    ".csv" => SourceType.CSV,
                    _ => throw new NotSupportedException(file)
                });

                var reader = filetype switch
                {
                    ".xlsx" => stream.CreateExcelReader(false),
                    ".txt" or ".csv" => stream.CreateCsvReader(false),
                    _ => throw new NotSupportedException(file)
                };


                hopperInterface.BeforeLoading(file, options);

                session.AssignedLogger.LogQueue.Enqueue($"[{DateTime.Now}] reader ON\r\n");
                using (reader)
                {
                    OnStartLoadingEvent?.Invoke(hopperData, hopperInterface, reader.RecordsAffected);

                    var i = 1;
                    var counter = 0;

                    while (reader.Read())
                    {
                        hopperInterface.OnLoading(i, reader, options);

                        OnLoadingEvent?.Invoke(hopperData, hopperInterface, counter);

                        i++;

                        counter++;
                        //hopper.Bar.Value = i;

                        if (counter % 2000 == 0) // пришлось сюда переместить - ради лога. Будь проклят, 28.05 (медленный сервак).
                        {
                            session.AssignedLogger.LogQueue.Enqueue($"[{DateTime.Now}] line: {i}\r\n");
                        }

                    }
                    OnEndLoadingEvent?.Invoke(hopperData, hopperInterface, i);
                }
            }
        }

        public void ModuleActionSQL(HopperAttribute hopperData, IHopperInterface hopperInterface, string path)
        {
            // поскльку выше идёт проверка на .StartsWith("SQL:")
            // для получения имени файла срезаем 4 символа 
            // с начала строки
            var file = new FileInfo(path[4..].Trim());

            if (!file.Exists)
            {
                throw new FileNotFoundException($"Не найден файл конфигурации для получания данных из SQL: \"{file.FullName}\"", file.FullName);
            }

            if (!file.TryReadAsJson<MsSqlDataSourceConfig>(out var config, out var exception))
            {
                throw new ApplicationException($"Не удалось прочитать файл конфигурации \"{file.FullName}\"", exception);
            }

            // счётчик строк
            var counter = 0;

            using (var connection = new SqlConnection(config.ConnectionString))
            {
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = config.Sql;
                    command.CommandType = CommandType.Text;

                    connection.SafeOpen();

                    using (var reader = command.ExecuteReader())
                    using (var wrapper = new SqlDataReaderWrapper(reader))
                    {
                        var options = new DataLoaderOptions(SourceType.SQL);

                        //options.ArrangeGuidBytes = true; // Почему-то это надо для АПС, чтоб биты переставлялись

                        hopperInterface.BeforeLoading(file.FullName, options);


                        OnStartLoadingEvent?.Invoke(hopperData, hopperInterface, reader.RecordsAffected);


                        while (wrapper.Read())
                        {
                            hopperInterface.OnLoading(counter, wrapper, options);

                            OnLoadingEvent?.Invoke(hopperData, hopperInterface, counter);

                            counter++;

                            //if (show)
                            //{
                            //    hopper.Bar.Value = rowCounter;
                            //}
                            if (counter % 2000 == 0) // пришлось сюда переместить - ради лога. Будь проклят, 28.05 (медленный сервак).
                            {
                                session.AssignedLogger.LogQueue.Enqueue($"[{DateTime.Now}] line: {counter}\r\n");
                            }
                        }
                    }

                    connection.SafeClose();
                }
            }

            OnEndLoadingEvent?.Invoke(hopperData, hopperInterface, counter);
        }
    }
}
