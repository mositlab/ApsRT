using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rodana.Core.Rechanges.RechangesEngine
{
    public static class RechangesEngineExtensions
    {

        internal static string AnyElseLabel = "Любой другой";
        internal static string AnythingLabel = "Любой";
        internal static string NothingLabel = "Отсутствует";

        internal static bool IsNothingRechange(this string str) => str == NothingLabel;
        internal static bool IsAnythingRechange(this string str) => str == AnythingLabel;
        internal static bool IsAnyElseRechange(this string str) => str == AnyElseLabel;
        internal static bool IsAnyElseOrAnythingRechange(this string str) => str.IsAnyElseRechange() || str.IsAnythingRechange();
        internal static bool IsSomeValueRechange(this string str) => !(str.IsAnyElseRechange() || str.IsAnythingRechange() || str.IsNothingRechange());
    }
}
