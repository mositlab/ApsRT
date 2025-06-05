using Rodana.Core.Rechanges.RechangesEngine;
using Rodana.Core.Stages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace Rodana.Core.Rechanges
{
    public static class CustomRechangeCriteriaMethods
    {

        public static
        Func<Stage, RechangesEngineData, Type, int, object>[] MethodsArray = new Func<Stage, RechangesEngineData, Type, int, object>[7]
        {
            GetOperVid,
            GetPlacesCount,
            GetProstoi,
            GetOtdTara,
            GetVspomTaraCount,
            GetNormTaraCount,
            GetAllOtdTaraCount,
        };

        public static
        Func<Stage, int>[] MethodsValuesCountArray = new Func<Stage, int>[7]
        {
            (x) => 1,
            (x) => 1,
            (x) => 1,
            (x) => 1,
            (x) => 1,
            (x) => 1,
            (x) => 1
        };


        private static object GetNormTaraCount(Stage operation, RechangesEngineData data, Type type, int arg4)
        {
            var ALL = operation.SessionLink.MasterDataLinks.StagesMasterData.AllStages;
            if (type == typeof(Guid?)) return null;
            if (operation.PrevsNormCount == 0)
            {
                if (type == typeof(string))
                    return operation.GhostNormPrevsPlacesCount.ToString();
                return (float?)operation.GhostNormPrevsPlacesCount;
            }
            var list = operation.Prevs.Where(x => !ALL[x].IsVspom && !ALL[x].UsesVspomTara);
            var sum = (float?)(operation.GhostNormPrevsPlacesCount + list.Sum(x => ALL[x].PlacesCount));
            if (type == typeof(string)) return sum.ToString();
            return sum;
        }

        private static object GetAllOtdTaraCount(Stage operation, RechangesEngineData data, Type type, int arg4)
        {
            var ALL = operation.SessionLink.MasterDataLinks.StagesMasterData.AllStages;
            if (type == typeof(Guid?)) return null;
            var result = operation.GhostVspomPrevsPlacesCount + operation.GhostNormPrevsPlacesCount; // суммирует от предков-призраков места
            if (operation.PrevsNormCount == 0)
            {
                if (type == typeof(string))
                    return result.ToString();
                return (float?)result;
            }
            var list = operation.Prevs.Where(x => !ALL[x].IsVspom);
            var sum = (float?)(result + list.Sum(x => ALL[x].PlacesCount)); // Сумма от призраков + сумма от существующих

            if (type == typeof(string)) return sum.ToString();
            return sum;
        }

        private static object GetVspomTaraCount(Stage operation, RechangesEngineData data, Type type, int arg4)
        {
            var ALL = operation.SessionLink.MasterDataLinks.StagesMasterData.AllStages;
            if (type == typeof(Guid?)) return null;
            if (operation.PrevsNormCount == 0)
            {
                if (type == typeof(string))
                    return operation.GhostVspomPrevsPlacesCount.ToString();
                return (float?)operation.GhostVspomPrevsPlacesCount;
            }
            var list = operation.Prevs.Where(x => !ALL[x].IsVspom && ALL[x].UsesVspomTara);
            var sum = (float?)(operation.GhostVspomPrevsPlacesCount + list.Sum(x => ALL[x].PlacesCount));
            if (type == typeof(string)) return sum.ToString();
            return sum;
        }

        private static object GetOtdTara(Stage operation, RechangesEngineData data, Type type, int arg4)
        {
            var ALL = operation.SessionLink.MasterDataLinks.StagesMasterData.AllStages;
            if (type == typeof(float?)) return null;
            if (operation.PrevsNormCount == 0) return (Guid?)null;
            var first = operation.Prevs.FirstOrDefault(x => !ALL[x].IsVspom && !ALL[x].UsesVspomTara, -1);
            if (first == -1)
            {
                if (type == typeof(string)) return null;
                return (Guid?)null;
            }
            var tara = ALL[first].CurrentTara;
            if (tara.IsEmpty)
            {
                if (type == typeof(string)) return null;
                return (Guid?)null;
            }
            if (type == typeof(string)) return tara.Label;
            return tara.Code;
        }


        /*
         Как получать данные таким образом? Ну например во: 
         */

        // Получит вид операции
        static object GetOperVid(Stage link, RechangesEngineData engineData, Type type, int valueIndex)
        {
            if (type == typeof(float?)) return null;
            if (type == typeof(string)) return link.OperationVid.Label;
            return link.OperationVid.Code;
        }

        // Получит количество мест
        static object GetPlacesCount(Stage which, RechangesEngineData engineData, Type type, int valueIndex)
        {
            if (type == typeof(Guid?)) return null;
            if (type == typeof(string)) return which.PlacesCount.ToString();
            return (float)which.PlacesCount;
        }

        // Получит часы простоя
        static object GetProstoi(Stage which, RechangesEngineData engineData, Type type, int valueIndex)
        {
            if (type == typeof(Guid?)) return null;
            if (engineData == null)
            {
                if (type == typeof(string)) return "0";
                return .0f;
            }
            float value = (float)(engineData.ProstoiData.MinValue - engineData.CenterData.LastDate).TotalHours;
            value = MathF.Max(0, value);
            if (type == typeof(string)) return value.ToString();
            return value;
        }

    }
}
