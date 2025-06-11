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

        int[] CriteriasList => sessionLink.SettingsController.CurrentSettings.CriteriasList; // перечисление способов оптимизации

        int CurrentComboParam = 0; // какой сейчас выбран способ оптимизации в массиве
        int CurrCycle = 0; // Текущий цикл
        internal int currentMode = 0; // текущий режим, по факту равен ComboList[CurrentComboParam]
        internal int currDynCounter { get; private set; }
        internal int maxDynCounter { get; private set; }
        internal bool FoundForDynamic = false;

        public int Generation = 0;

        int GensMax = 0;
        int CyclesMax = 0;
        double MinutesD = 0;

        internal DateTime LastTime;

        internal PlanningCalculatorEngineResult origValue;
        internal PlanningCalculatorEngineResult BestValue;

        //internal bool NeverGraphs;

        internal bool CheckRestrictions(PlanningCalculatorEngineResult t)
        {

            var val = ((t.DaysRuinedValue * 100.0 / BestValue.DaysRuinedValue) - 100.0);
            var R1 = double.IsNaN(val) || val <= LoadedData.CriteriasPossibleWorse[0];

            val = ((t.RechangesSeconds * 100.0 / BestValue.RechangesSeconds) - 100.0);
            var R2 = double.IsNaN(val) || val <= LoadedData.CriteriasPossibleWorse[1];

            val = ((t.ProstoiSeconds.TotalMinutes * 100.0 / BestValue.ProstoiSeconds.TotalMinutes) - 100.0);
            var R3 = double.IsNaN(val) || val <= LoadedData.CriteriasPossibleWorse[2];

            val = ((t.ProductionSeconds * 100.0 / BestValue.ProductionSeconds) - 100.0);
            var R4 = double.IsNaN(val) || val <= LoadedData.CriteriasPossibleWorse[3];

            val = ((t.BadOffsets * 100.0 / BestValue.BadOffsets) - 100.0);
            var R5 = double.IsNaN(val) || val <= LoadedData.CriteriasPossibleWorse[4];

            return R1 && R2 && R3 && R4 && R5;
        }

        public Session.Session sessionLink { get; }

        SettingsClass LoadedData => sessionLink.SettingsController.CurrentSettings;

        internal PlanningAlgorithmWorkflow(Session.Session session, int generations, int cycles, double minutes, PlanningCalculatorEngineResult originalValue, int rawDataCount)
        {
            sessionLink = session;
            RawData = new object[rawDataCount];
            GensMax = generations;
            CyclesMax = cycles;
            MinutesD = minutes * 100;
            LastTime = DateTime.Now;
            currDynCounter = 0;
            maxDynCounter = 0;

            origValue = originalValue;
            BestValue = originalValue;

            SetNewDynCounter();//SetComboParams(CurrentComboParam, ComboList, ref currentMode, ref currDynCounter, ref maxDynCounter);
        }

        //internal Action<int> ProgressChanged;

        internal bool CanContinue()
        {
            try
            {
                if (MinutesD > 0) // по минутам
                {
                    var mins = (DateTime.Now - sessionLink.AssignedPlanningAlgorithm.MinAcceptableDate).TotalMinutes * 100;
                    if (mins >= MinutesD)
                    {
                        return false;
                    }
                }

                if (CyclesMax > 0)
                {
                    if (CurrCycle >= CyclesMax)
                    {
                        return false;
                    }
                }

                if (GensMax > 0)
                {
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
            if (sessionLink.SessionOptions.IsSilent
                && (DateTime.Now - LastTime).TotalSeconds > 30) // Если я работаю втихую, и уже прошло полминуты
            {
                if (sessionLink.AssignedPlanningAlgorithm.FoundSmth)
                {
                    sessionLink.SessionOutputEngine.Output();//Output(false, true, "ranges manual.csv", "orders.csv", OutputFile, "rechanges.csv", "mods.csv", "naladLengths.csv");
                    sessionLink.AssignedLogger.LogQueue.Enqueue($"[{DateTime.Now}]: better solution exported. gn {Generation}. cl {CurrCycle}: tr {BestValue.TaraErrorsCount}, nl {0/*BestValue.CatchedErrorsCount*/}.\r\n");
                    sessionLink.AssignedPlanningAlgorithm.FoundSmth = false;
                }
                else
                {
                    sessionLink.AssignedLogger.LogQueue.Enqueue($"[{DateTime.Now}]: no solution... gn {Generation}. cl {CurrCycle}: tr {BestValue.TaraErrorsCount}, nl {0/*BestValue.CatchedErrorsCount*/}.\r\n");
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
            if (CriteriasList.Length > 1)
            {
                if (LoadedData.UsedCriteria == -1)
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
                    CurrentComboParam++;
                    if (CurrentComboParam == CriteriasList.Length)
                    {
                        CurrentComboParam = 0;
                    }

                    SetNewDynCounter();//SetComboParams(CurrentComboParam, ComboList, ref currentMode, ref currDynCounter, ref maxDynCounter);
                }
            }


            //if (!IsSilent && !FoundForDynamic && !NeverGraphs)
            //{
            //    Graphs.AddNewFailedPoint();
            //}


            CheckOutput();

            Generation++;

            return CanContinue();
        }

        /// <summary>
        /// Обновляет динамический счётчик и выдаёт новый режим работы
        /// </summary>
        internal void SetNewDynCounter()
        {
            currDynCounter = 0;
            currentMode = CriteriasList[CurrentComboParam];

            if (LoadedData.UseOnlyMaxCriteriasSettings)
            {
                maxDynCounter = LoadedData.CriteriasMaxIterations[currentMode];
            }
            else
            {
                maxDynCounter = Random.Shared.Next(LoadedData.CriteriasMinIterations[currentMode], LoadedData.CriteriasMaxIterations[currentMode]);
            }
        }
    }
}
