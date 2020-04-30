using NotionSharp.Lib.ApiV3.Model;

namespace NotionSharp.Lib.ApiV3.Results
{
    public class LoadPageChunkResult
    {
        public Cursor Cursor { get; set; }
        public RecordMap RecordMap { get; set; }
    }
}
