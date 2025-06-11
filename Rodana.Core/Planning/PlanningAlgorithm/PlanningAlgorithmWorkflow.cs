using Rodana.Core.Planning.PlanningCalculatorEngine;
using Rodana.Core.Session;
using Rodana.Core.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rodana.Core.Planning.PlanningAlgorithm
{
    public class PlanningAlgorithmWorkflow
    {
        internal object[] RawData;

        int[] ComboList; // перечисление способов оптимизации
        int CurrentComboParam = 0; // какой сейчас выбран способ оптимизации в массиве
        int CurrCycle = 0; // Текущий цикл
        internal int currentMode = 0; // текущий режим, по факту равен ComboList[CurrentComboParam]
        internal int currDynCounter { get; private set; }
        internal int maxDynCounter { get; private set; }
        internal bool FoundForDynamic = false;

        internal int Generation = 0, Best = 0;

        int GensMax = 0;
        int CyclesMax = 0;
        double MinutesD = 0;

        bool AsCombo = false;

        internal DateTime LastTime;

        internal TimeSpan LastRechangesForCombo;
        internal TimeSpan LastProductionForCombo;
        internal double LastProstoiForCombo;
        internal double LastDaysForCombo;
        internal double LastBadOffsetsForCombo;
        TimeSpan LastMostRCForCombo;

        internal PlanningCalculatorEngineResult origValue;
        internal PlanningCalculatorEngineResult BestValue;

        internal bool NeverGraphs;

        internal bool CheckRestrictions(PlanningCalculatorEngineResult t)
        {
            var val = ((t.RechangesSeconds * 100.0 / LastRechangesForCombo) - 100.0);
            var R = double.IsNaN(val) || val <= LoadedData.MethodsData[10];

            val = ((t.ProductionSeconds * 100.0 / LastProductionForCombo) - 100.0);
            var R2 = double.IsNaN(val) || val <= LoadedData.MethodsData[11];

            val = ((t.ProstoiSeconds.TotalMinutes * 100.0 / LastProstoiForCombo) - 100.0);
            var R3 = double.IsNaN(val) || val <= LoadedData.MethodsData[12];

            val = ((t.DaysRuinedValue * 100.0 / LastDaysForCombo) - 100.0);
            var R4 = double.IsNaN(val) || val <= LoadedData.MethodsData[13];

            //val = ((t.MostRCSeconds * 100.0 / LastMostRCForCombo) - 100.0);
            //var R5 = double.IsNaN(val) || val <= LoadedData.MethodsData[14];

            val = ((t.BadOffsets * 100.0 / LastBadOffsetsForCombo) - 100.0);
            var R7 = double.IsNaN(val) || val <= LoadedData.MethodsData[20];

            return R && R2 && R3 && R4 && R7;
        }

        public Session.Session sessionLink { get; }

        SettingsClass LoadedData => sessionLink.SettingsController.CurrentSettings;

        internal PlanningAlgorithmWorkflow(Session.Session session, int[] comboList, bool asCombo, int generations, int cycles, double minutes, PlanningCalculatorEngineResult originalValue, int rawDataCount)
        {
            RawData = new object[rawDataCount];
            ComboList = comboList;
            GensMax = generations;
            CyclesMax = cycles;
            MinutesD = minutes * 100;
            AsCombo = asCombo;
            LastTime = DateTime.Now;
            currDynCounter = 0;
            maxDynCounter = 0;

            origValue = originalValue;
            BestValue = originalValue;

            LastRechangesForCombo = originalValue.RechangesSeconds;
            LastProductionForCombo = originalValue.ProductionSeconds;
            LastProstoiForCombo = originalValue.ProstoiSeconds.TotalMinutes;
            LastDaysForCombo = originalValue.DaysRuinedValue;
            LastBadOffsetsForCombo = originalValue.BadOffsets;
            LastMostRCForCombo = originalValue.MostRCSeconds;

            SetNewDynCounter();//SetComboParams(CurrentComboParam, ComboList, ref currentMode, ref currDynCounter, ref maxDynCounter);
        }

        internal Action<int> ProgressChanged;

        internal bool CanContinue()
        {
            try
            {
                if (MinutesD > 0) // по минутам
                {
                    var mins = (DateTime.Now - start).TotalMinutes * 100;
                    ProgressChanged?.Invoke((int)mins);
                    //if (!IsSilent)
                    //{
                    //    This.CalcProgressBar.Value = (int)mins;
                    //}

                    if (mins >= MinutesD)
                    {
                        return false;
                    }
                }

                if (CyclesMax > 0)
                {
                    ProgressChanged?.Invoke(CurrCycle);
                    //if (!IsSilent)
                    //{
                    //    This.CalcProgressBar.Value = CurrCycle;
                    //}

                    if (CurrCycle >= CyclesMax)
                    {
                        return false;
                    }
                }

                if (GensMax > 0)
                {
                    ProgressChanged?.Invoke(Generation);
                    //if (!IsSilent)
                    //{
                    //    This.CalcProgressBar.Value = Generation;
                    //}

                    if (Generation >= GensMax)
                    {
                        return false;
                    }
                }
            }
            catch
            {
                return false;
            }
            return true;
        }

        internal void CheckOutput()
        {
            if (IsSilent
                && (DateTime.Now - LastTime).TotalSeconds > 30) // Если я работаю втихую, и уже прошло полминуты
            {
                if (FoundSmth)
                {
                    Output(false, true, "ranges manual.csv", "orders.csv", OutputFile, "rechanges.csv", "mods.csv", "naladLengths.csv");
                    ConcurrentLogger.AddToLogQueue.Enqueue($"[{DateTime.Now}]: better solution exported. gn {Generation}. cl {CurrCycle}: tr {BestValue.TaraErrorsCount}, nl {BestValue.CatchedErrorsCount}.\r\n");
                    FoundSmth = false;
                }
                else
                {
                    ConcurrentLogger.AddToLogQueue.Enqueue($"[{DateTime.Now}]: no solution... gn {Generation}. cl {CurrCycle}: tr {BestValue.TaraErrorsCount}, nl {BestValue.CatchedErrorsCount}.\r\n");
                }

                LastTime = DateTime.Now; // Независимо, нашел или нет, задаем новое время проверки
            }
        }

        /// <summary>
        /// Новая итерация
        /// </summary>
        /// <param name="BestValue"></param>
        internal bool NextIteration()
        {
            if (ComboList.Length > 1)
            {
                if (AsCombo)
                {
                    currDynCounter++; // Увеличиваем, несмотря ни на что
                }
                else
                {
                    if (FoundForDynamic && currentMode != 6)
                    {
                        currDynCounter = 0;
                    }
                    else
                    {
                        currDynCounter++;
                    }
                }

                if (currDynCounter == maxDynCounter)
                {
                    CurrCycle++;
                    LastRechangesForCombo = BestValue.RechangesSeconds; // обновляем для нового этапа
                    LastProductionForCombo = BestValue.ProductionSeconds; // обновляем для нового этапа
                    LastProstoiForCombo = BestValue.ProstoiSeconds.TotalMinutes; // обновляем для нового этапа
                    LastDaysForCombo = BestValue.DaysRuinedValue;
                    LastBadOffsetsForCombo = BestValue.BadOffsets;
                    LastMostRCForCombo = BestValue.MostRCSeconds;
                    CurrentComboParam++;
                    if (CurrentComboParam == ComboList.Length)
                    {
                        CurrentComboParam = 0;
                    }

                    SetNewDynCounter();//SetComboParams(CurrentComboParam, ComboList, ref currentMode, ref currDynCounter, ref maxDynCounter);
                }
            }


            if (!IsSilent && !FoundForDynamic && !NeverGraphs)
            {
                Graphs.AddNewFailedPoint();
            }


            CheckOutput();

            Generation++;

            return CanContinue();
        }

        Random _rand = new Random();

        /// <summary>
        /// Обновляет динамический счётчик и выдаёт новый режим работы
        /// </summary>
        internal void SetNewDynCounter()
        {
            currDynCounter = 0;
            currentMode = ComboList[CurrentComboParam];

            if (LoadedData.UseOnlyMaxCombo)
            {
                maxDynCounter = currentMode switch
                {
                    1 => (int)LoadedData.MethodsData[1],
                    2 => (int)LoadedData.MethodsData[3],
                    3 => (int)LoadedData.MethodsData[5],
                    4 => (int)LoadedData.MethodsData[7],
                    6 => (int)LoadedData.MethodsData[16],
                    7 => (int)LoadedData.MethodsData[19],
                    _ => (int)LoadedData.MethodsData[9],
                };
            }
            else
            {
                maxDynCounter = currentMode switch
                {
                    1 => _rand.Next((int)LoadedData.MethodsData[0], (int)LoadedData.MethodsData[1]),
                    2 => _rand.Next((int)LoadedData.MethodsData[2], (int)LoadedData.MethodsData[3]),
                    3 => _rand.Next((int)LoadedData.MethodsData[4], (int)LoadedData.MethodsData[5]),
                    4 => _rand.Next((int)LoadedData.MethodsData[6], (int)LoadedData.MethodsData[7]),
                    6 => _rand.Next((int)LoadedData.MethodsData[15], (int)LoadedData.MethodsData[16]),
                    7 => _rand.Next((int)LoadedData.MethodsData[18], (int)LoadedData.MethodsData[19]),
                    _ => _rand.Next((int)LoadedData.MethodsData[8], (int)LoadedData.MethodsData[9]),
                };
            }
        }
    }
}
