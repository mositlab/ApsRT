using UniversalPack.Extensions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace UniversalPack.Common.DataLoader
{
    /// <summary>
    /// Опции загрузчика
    /// </summary>
    public sealed class DataLoaderOptions
    {
        /// <summary>
        /// Список имён файлов, разделённый разрывом строки
        /// </summary>
        [JsonIgnore]
        public string FileNames
        {
            get => Files.Join(Environment.NewLine);
            set
            {
                Files.Clear();
                Files.AddRange(value.Split('\r', '\n').Where(e => !string.IsNullOrWhiteSpace(e)));
            }
        }

        /// <summary>
        /// Список файлов
        /// </summary>
        [JsonProperty]
        public ICollection<string> Files { get; internal set; }

        public DataLoaderOptions()
        {
            Files = new List<string>();
        }
    }
}
