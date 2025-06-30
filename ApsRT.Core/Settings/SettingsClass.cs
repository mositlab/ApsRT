using Newtonsoft.Json;
using ApsRT.Core.Orders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApsRT.Core.Settings
{

    /// <summary>
    /// Класс настроек. В настройках содержится масса информации, как АПС должна будет работать.
    /// </summary>
    public class SettingsClass
    {

        public static SettingsClass OriginalSettings = new SettingsClass()
        {
            OriginalDurationTime = 1,
            SaveOnSessionEnd = true,
            MaxDaysBetweenPartias = 30,
            OrderCoeffs = new OrderFunctionData[3]
            {
                new OrderFunctionData()
                {
                    Level1 = new OrderFunctionLevelData(1, 1, 1),
                    Level2 = new OrderFunctionLevelData(2, 1, 1),
                    Level3 = new OrderFunctionLevelData(1, 1, 1),
                },
                new OrderFunctionData()
                {
                    Level1 = new OrderFunctionLevelData(1, 1, 1),
                    Level2 = new OrderFunctionLevelData(2, 1, 1),
                    Level3 = new OrderFunctionLevelData(1, 1, 1),
                },
                new OrderFunctionData()
                {
                    Level1 = new OrderFunctionLevelData(1, 1, 1),
                    Level2 = new OrderFunctionLevelData(2, 1, 1),
                    Level3 = new OrderFunctionLevelData(1, 1, 1),
                }
            },
            VKoeffs = new double[] { 0, 0, 0, 1000000, 1000000, 0, 0, 0, 1, 100, 0.1, 0.3, 10000, 1, 0, 0, 0, 0, 0, 0, 0, 500, 0, 0, -100000000, 0, 0, 10000 },
            CriteriasList = new int[] { 4, 6, 5, 3, 1, 2 },
            CriteriasPossibleWorse = new double[] {15, 15, 15, 15, 15, 15, 15 },
            CriteriasMinIterations = new int[] { 100, 100, 100, 100, 100, 100, 100 },
            CriteriasMaxIterations = new int[] { 200, 200, 200, 200, 200, 200, 200 }
        };


        #region Для алгоритмов планирования

        [JsonProperty]
        public double[] VKoeffs { get; set; }

        [JsonProperty]
        public int[] CriteriasList { get; set; }

        [JsonProperty]
        public int IterationsCount { get; set; }

        [JsonProperty]
        public double MinutesCount { get; set; }

        [JsonProperty]
        public int CyclesCount { get; set; }

        [JsonProperty]
        public bool UseMinutes { get; set; }

        [JsonProperty]
        public bool UseCycles { get; set; }

        [JsonProperty]
        public OrderFunctionData[] OrderCoeffs { get; set; }

        [JsonProperty]
        public bool UseOnlyMaxCriteriasSettings { get; set; }

        [JsonProperty]
        public double[] CriteriasPossibleWorse { get; set; }

        [JsonProperty]
        public int[] CriteriasMinIterations { get; set; }

        [JsonProperty]
        public int[] CriteriasMaxIterations { get; set; }

        [JsonProperty]
        public int UsedCriteria { get; set; }

        #endregion


        [JsonProperty]
        public Dictionary<string, string[]> HoppersPaths { get; set; }

        [JsonProperty]
        public bool SaveOnSessionEnd { get; set; }

        [JsonProperty]
        public int OriginalDurationTime { get; set; }

        [JsonProperty]
        public bool CreateKillMe { get; set; }

        [JsonProperty]
        public bool CreateStopMe { get; set; }

        [JsonProperty]
        public bool HideConsole { get; set; }

        [JsonProperty]
        public bool AutoDataLoad { get; set; }

        [JsonProperty]
        public bool UsePseudoTara { get; set; }

        [JsonProperty]
        public bool LogErrors { get; set; }

        [JsonProperty]
        public double MaxDaysBetweenPartias { get; set; }

        [JsonProperty]
        public FormTheme CurrentTheme { get; set; }
    }
}
