using ApsRT.Core.Centers.Hoppers;
using ApsRT.Core.Hoppers;
using ApsRT.Core.Rechanges;
using ApsRT.Core.Rechanges.Hoppers;
using ApsRT.Core.Stages.Hoppers;
using ApsRT.Core.Stages.MasterData;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using UniversalPack.Common.DataLoader;
using UniversalPack.Extensions;

namespace ApsRT.Core.NaladLengths
{
    [Hopper(Label = "Наладочные длины", IsDirectory = false, IsMultiple = false, UsesGuid = false, MustBeFilledWithData = true, /*LoadWhenOnlyZadel = false,*/
        NeededHoppers = new[] { nameof(StagesHopper), nameof(AlternativeCentersHopper), nameof(RechangesCriteriasHopper) })]
    public class NaladLengthsHopper : IHopperInterface
    {

        NaladLengthsMasterData NaladLengthsMasterData;

        public void ActionFile(string filename)
        {

        }

        public void AfterLoading()
        {

        }

        public void BeforeAllLoading(Session.Session session, string[] filenames)
        {
            NaladLengthsMasterData = session.MasterDataLinks.NaladLengthsMasterData;
            NaladLengthsMasterData.PrepareForLoading();
        }

        public void BeforeLoading(string filename, DataLoaderOptions options)
        {

        }

        public void OnLoading(int rowIndex, IDataReader data, DataLoaderOptions options)
        {
            var model = new NaladLengthDataLoaderModel();
            model.LoadFromDataRecord(data, options);

            var array = new (double ParameterFrom, double ParameterTo)[NaladLengthsMasterData.criteriaIndexes.Count];

            for (int i = 0; i < NaladLengthsMasterData.criteriaIndexes.Count; i++)
            {
                var critIndex = NaladLengthsMasterData.criteriaIndexes[i];
                var criteriaName = NaladLengthsMasterData.SessionLink.MasterDataLinks.RechangesMasterData.ExistingCriterias[critIndex].Name;

                var from = data[criteriaName + "От"].ConvertToOrDefault<string>().RemoveWhitespaces().ConvertToFloatByAnyCost() ?? 0;
                var to = data[criteriaName + "До"].ConvertToOrDefault<string>().RemoveWhitespaces().ConvertToFloatByAnyCost() ?? 0;
                array[i] = (from, to);

            }
            model.Params = array;

            NaladLengthsMasterData.AddNaladLengthData(model);
        }
    }
}
