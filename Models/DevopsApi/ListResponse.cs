using System.Collections.Generic;
using Newtonsoft.Json;

namespace Homecloud.Models.DevopsApi
{
    internal class ListResponse<T>
    {
        public int Count { get; set; }
        public IEnumerable<T> Value { get; set; }
    }
}