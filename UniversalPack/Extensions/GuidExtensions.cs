using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UniversalPack.Common.DataLoader;

namespace UniversalPack.Extensions
{
    public static class GuidExtensions
    {

        public static void Swap<T>(ref T var1, ref T var2)
        {
            T temp = var1;
            var1 = var2;
            var2 = temp;
        }

        static Guid RearrangeGuidEndian(Guid guid)
        {
            var bytes = guid.ToByteArray();
            Swap(ref bytes[0], ref bytes[3]);
            Swap(ref bytes[1], ref bytes[2]);
            Swap(ref bytes[4], ref bytes[5]);
            Swap(ref bytes[6], ref bytes[7]);
            return new Guid(bytes);
        }

        static Guid RearrangeGuidCustomZMK(Guid guid, bool ToInput)
        {
            var bytes = guid.ToByteArray().Reverse().ToArray();
            Swap(ref bytes[8], ref bytes[15]);
            Swap(ref bytes[9], ref bytes[14]);
            Swap(ref bytes[10], ref bytes[13]);
            Swap(ref bytes[11], ref bytes[12]);
            return new Guid(bytes);
        }

        /// <summary>
        /// Обрабатывает объект, превращая его в гуид.
        /// </summary>
        /// <param name="input">Объект для обработки</param>
        /// <param name="guidArrrangeType">Тип, как обрабатывать получившийся поток байтов, как их переставлять местами./param>
        /// <param name="ToInput">Эти входные данные или выходные? Кастомное решение не является двусторонним, вынуждая делать два алгоритма перестановки и уточнять, куда идёт гуид.</param>
        /// <returns></returns>
        public static Guid ProcessGuid(this object input, GuidArrrangeType guidArrrangeType, bool ToInput)
        {
            switch (guidArrrangeType)
            {
                case GuidArrrangeType.CustomZMK:
                    {
                        if (input is Guid guid) return RearrangeGuidCustomZMK(guid, ToInput);
                        if (input is string str) return string.IsNullOrEmpty(str) ? Guid.Empty : RearrangeGuidCustomZMK(Guid.Parse(str), ToInput);
                        break;
                    }
                case GuidArrrangeType.Endian:
                    {
                        if (input is Guid guid) return RearrangeGuidEndian(guid);
                        if (input is string str) return string.IsNullOrEmpty(str) ? Guid.Empty : RearrangeGuidEndian(Guid.Parse(str));
                        break;
                    }
                default:
                    {
                        if (input is Guid guid) return guid;
                        if (input is string str) return string.IsNullOrEmpty(str) ? Guid.Empty : Guid.Parse(str);
                        break;
                    }
            }
            return Guid.Empty;
        }
    }
}
