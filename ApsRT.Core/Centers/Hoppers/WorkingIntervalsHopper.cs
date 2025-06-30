using ApsRT.Core.Centers.MasterData;
using ApsRT.Core.Hoppers;
using ApsRT.Core.Partias.Leftovers;
using ApsRT.Core.Stages.Hoppers;
using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UniversalPack.Common.DataLoader;
using Uco = UniversalPack.Common.CodedObject<System.Guid, string>;

namespace ApsRT.Core.Centers.Hoppers
{
    [Hopper(Label = "График доступности РЦ", IsDirectory = false, IsMultiple = false, UsesGuid = false, LoadWhenOnlyZadel = false,
        NeededHoppers = new[] { nameof(StagesHopper) })]
    public class WorkingIntervalsHopper : IHopperInterface
    {

        WorkingIntervalsMasterData WorkingIntervalsMasterData;

        Dictionary<Uco, List<(DateTime from, DateTime to)>> WorkingIntervalsTemp = new();

        void LoadLunchTime(Uco Center, DateTime from, DateTime to)
        {
            if (WorkingIntervalsTemp.ContainsKey(Center))
            {
                WorkingIntervalsTemp[Center].Add((from, to));
            }
            else
            {
                WorkingIntervalsTemp.Add(Center, new() { (from, to) });
            }
        }

        void IHopperInterface.ActionFile(string filename)
        {

        }

        void IHopperInterface.AfterLoading()
        {
            Dictionary<Uco, List<(DateTime When, TimeSpan Time, int Type)>> WorkingIntervalsTemp2 = new();
            foreach (var unSortedCenter in WorkingIntervalsTemp)
            {
                var center = unSortedCenter.Value.OrderBy(x => x.from).ToArray();
                for (var i = 0; i < center.Length - 1; i++)
                {
                    var start = center[i].to;
                    var end = center[i + 1].from;
                    var span = end - start;
                    if (WorkingIntervalsTemp2.TryGetValue(unSortedCenter.Key, out var list))
                    {
                        list.Add((start, span, 1));
                    }
                    else
                    {
                        WorkingIntervalsTemp2.Add(unSortedCenter.Key, new() { (start, span, 1) });
                    }
                }
            }
            WorkingIntervalsMasterData.CentersFixingTime = WorkingIntervalsTemp2.ToFrozenDictionary();
            WorkingIntervalsTemp = null;
        }

        void IHopperInterface.BeforeAllLoading(Session.Session session, string[] filenames)
        {
            WorkingIntervalsMasterData = session.MasterDataLinks.WorkingIntervalsMasterData;
            WorkingIntervalsMasterData.ClearAll();
        }

        void IHopperInterface.BeforeLoading(string filename, DataLoaderOptions options)
        {

        }

        void IHopperInterface.OnLoading(int rowIndex, IDataReader data, DataLoaderOptions options)
        {
            var model = new WorkingIntervalDataLoaderModel();
            model.LoadFromDataRecord(data, options);
            LoadLunchTime(model.CenterName, model.Start, model.End);
        }
    }
}
