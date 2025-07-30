using System.Collections.Generic;

namespace EMTestTask.Models
{
    public class AdvertisingServiceUpload
    {
        public int Result { get; set; } = 0;
        public string Message { get; set; } = string.Empty;
        public long LinesTotal { get; set; } = 0;
        public List<long> LinesErrorNums { get; set; } = [];
        public long LinesWithErrors { get { return this.LinesErrorNums.Count; } }
    }
}
