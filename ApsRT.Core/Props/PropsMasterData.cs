using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApsRT.Core.Props
{
    public class PropsMasterData
    {
        public Dictionary<string, int> PropStringToIndex = new();

        public Dictionary<string, int> PropNumberToIndex = new();

        public Dictionary<string, int> PropGuidToIndex = new();

        public bool GenerateLinks(string propName, out PropsLinkerClass linker)
        {
            if (string.IsNullOrEmpty(propName))
            {
                linker = new PropsLinkerClass(propName, -1, -1, -1);
                return false;
            }

            var valI = PropNumberToIndex.TryGetValue(propName, out var i) ? i : -1;
            var valG = PropGuidToIndex.TryGetValue(propName, out var g) ? g : -1;
            var valS = PropStringToIndex.TryGetValue(propName, out var s) ? s : -1;

            linker = new PropsLinkerClass(propName, valI, valG, valS);
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
            switch (itsType)
            {
                case 255:
                    {
                        Console.WriteLine($"{key} - empty type!");
                        break;
                    }
                case 0: WorkWith(PropNumberToIndex, key); break;
                case 1: WorkWith(PropGuidToIndex, key); break;
                case 2: WorkWith(PropStringToIndex, key); break;
                case 3:
                    {
                        Console.WriteLine($"{key} - unknown type!");
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
