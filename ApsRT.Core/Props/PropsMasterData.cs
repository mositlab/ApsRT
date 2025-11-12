using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApsRT.Core.Props
{
    public class PropsMasterData
    {
        public PropsMasterData(Session.Session session)
        {
            this.session = session;
        }

        Session.Session session { get; init; }

        public Dictionary<string, int> OpersPropNumberToIndex = new();
        public Dictionary<string, int> OpersPropGuidToIndex = new();
        public Dictionary<string, int> OpersPropStringToIndex = new();

        public Dictionary<string, int> MatsPropNumberToIndex = new();
        public Dictionary<string, int> MatsPropGuidToIndex = new();
        public Dictionary<string, int> MatsPropStringToIndex = new();

        public bool GenerateLinks(string propName, int tableType, out PropsLinkerClass linker)
        {
            if (string.IsNullOrEmpty(propName))
            {
                linker = new PropsLinkerClass(propName, -1, -1, -1, -1);
                return false;
            }

            int valI, valG, valS;
            if (tableType == 1)
            {
                valI = MatsPropNumberToIndex.TryGetValue(propName, out var i) ? i : -1;
                valG = MatsPropGuidToIndex.TryGetValue(propName, out var g) ? g : -1;
                valS = MatsPropStringToIndex.TryGetValue(propName, out var s) ? s : -1;
            }
            else
            {
                valI = OpersPropNumberToIndex.TryGetValue(propName, out var i) ? i : -1;
                valG = OpersPropGuidToIndex.TryGetValue(propName, out var g) ? g : -1;
                valS = OpersPropStringToIndex.TryGetValue(propName, out var s) ? s : -1;
            }

            linker = new PropsLinkerClass(propName, tableType, valI, valG, valS);
            return valI != -1 || valG != -1 || valS != -1;
        }

        /*
        0 float
        1 guid
        2 string
         */
        public void AddProp(PropsDataLoaderModel model)
        {
            string key = model.PropName;
            byte itsType = model.PropType;
            int tableType = model.TableType;
            switch (itsType)
            {
                case 255:
                    {
                        Console.WriteLine($"{key} - empty type!");
                        session.DataErrors.AddNewPartiaError(DataErrors.NewDataErrorType.EmptyPropType, key);
                        break;
                    }
                case 0: WorkWith(tableType == 1 ? MatsPropNumberToIndex : OpersPropNumberToIndex, key); break;
                case 1: WorkWith(tableType == 1 ? MatsPropGuidToIndex : OpersPropGuidToIndex, key); break;
                case 2: WorkWith(tableType == 1 ? MatsPropStringToIndex : OpersPropStringToIndex, key); break;
                case 3:
                    {
                        Console.WriteLine($"{key} - unknown type!");
                        session.DataErrors.AddNewPartiaError(DataErrors.NewDataErrorType.UnknownPropType, key);
                        break;
                    }
            }
        }

        void WorkWith(Dictionary<string, int> propsDic, string toAdd)
        {
            if (propsDic.ContainsKey(toAdd))
                return;
            propsDic.Add(toAdd, propsDic.Count); // Индекс будет постепенно возрастать, что позволит обращаться к индексам реквизитов и использовать массивы в дальнейшем.
        }

    }
}
