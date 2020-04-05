using System.Collections.Generic;
using System.ComponentModel;
using HzNS.MdxLib.MDict.Tool;

namespace HzNS.MdxLib.models
{
    [TypeConverter(typeof(DictionarySeg0Converter)), Description("展开以查看应用程序的拼写选项。")]
    public class MDictKwIndexTables
    {
        /// <summary>
        /// 加密后的索引块首先被缓存于此；随后解密并解压后的索引块仍然放在这里。
        /// </summary>
        public List<byte> IndexesRawData { get; set; }

        [Browsable(false)] public int RawCount => IndexesRawData?.Count ?? -1;

        /// <summary>
        /// = Seg1
        /// </summary>
        public KwIndex1[] IndexList { get; set; }
        public KwIndexMap KwIndexMap { get; set; }
        /// <summary>
        /// = Seg2
        /// </summary>
        public KwIndex2[] IndexList2 { get; set; }

        public int TotalEntries => IndexList2?.Length ?? 0;

        /// <summary>
        /// 这个数据结构用于进行快速的起始字符串匹配。
        /// 当用户键入一个字符串序列时，Matcher能够快速地匹配到最接近的一条关键字(从KWIndex2中)，并返回其下标值
        /// </summary>
        public FastRobustMatcher<int> Matcher { get; set; }
    }
}