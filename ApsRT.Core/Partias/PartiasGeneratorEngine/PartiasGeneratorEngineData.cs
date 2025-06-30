using ApsRT.Core.Partias.PartiasClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uco = UniversalPack.Common.CodedObject<System.Guid, string>;
using OrderID = UniversalPack.Common.CodedObjectVersioned<System.Guid, string, string>;
using System.Drawing;

namespace ApsRT.Core.Partias.PartiasGeneratorEngine
{
    public class PartiasGeneratorEngineData
    {

        public List<Stages.Stage> Stages;

        public Color Color;

        public Session.Session Session;

        public HashSet<int> DoneIDS = new();

        public Dictionary<int, Uco> prevs = new();

        public Dictionary<int, PartiaModelling> OpersToSdvigs = new();

        // прямолинейный способ хранить предыдущую тару, ДО назначения задельных тар. Когда заделы перезаписывают всякое, то возвращаются тары отсюда. Вместо пустых.
        public Dictionary<int, (Uco Tara, double namot)> OriginallyTaraWasThis = new();

        public Dictionary<PartiaModelling, Uco> RestrictionsVia74110 = new();
    }
}
