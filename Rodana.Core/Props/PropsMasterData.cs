using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rodana.Core.Props
{
    public class PropsMasterData
    {
        internal Dictionary<string, int> PropStringToIndex = new();

        internal Dictionary<string, int> PropNumberToIndex = new();

        internal Dictionary<string, int> PropGuidToIndex = new();

        internal PropsLinkerClass GenerateLinks(string name)
        {
            var valI = PropNumberToIndex.TryGetValue(name, out var i) ? i : -1;
            var valG = PropGuidToIndex.TryGetValue(name, out var g) ? g : -1;
            var valS = PropStringToIndex.TryGetValue(name, out var s) ? s : -1;

            return new PropsLinkerClass(name, valI, valG, valS);
        }

        /*
        0 float
        1 guid
        2 string
         */
        internal void AddProp(string key, byte itsType)
        {
            switch (itsType)
            {
                case 0: WorkWith(PropNumberToIndex, key); break;
                case 1: WorkWith(PropGuidToIndex, key); break;
                case 2: WorkWith(PropStringToIndex, key); break;
            }
        }

        void WorkWith(Dictionary<string, int> propsDic, string toAdd)
        {
            if (propsDic.ContainsKey(toAdd)) return;
            propsDic.Add(toAdd, propsDic.Count); // Индекс будет постепенно возрастать, что позволит обращаться к индексам реквизитов и использовать массивы в дальнейшем.
        }

    }
}
