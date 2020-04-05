using System.Diagnostics.CodeAnalysis;

namespace HzNS.MdxLib.models
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class CssEntry
    {
        internal int _index;

        public int Index
        {
            get => _index;
            set => _index = value;
        }

        public string Begin { get; set; }
        public string End { get; set; }
    }
}