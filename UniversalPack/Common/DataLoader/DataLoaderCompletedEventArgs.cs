using UniversalPack.Models.DataLoader;
using System;
using System.Collections.Generic;

namespace UniversalPack.Common.DataLoader
{
    public class DataLoaderCompletedEventArgs<TLoaderModel> : EventArgs
        where TLoaderModel : BaseDataLoaderModel
    {
        public ICollection<TLoaderModel> Models { get; init; }
    }
}
