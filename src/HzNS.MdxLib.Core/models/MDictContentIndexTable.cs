using System.Collections.Generic;
using System.ComponentModel;

namespace HzNS.MdxLib.models
{
    [TypeConverter(typeof(DictionarySeg0Converter)), Description("展开以查看应用程序的拼写选项。")]
    public class MDictContentIndexTable
    {
        /// <summary>
        /// 决定了解压缩采用什么算法
        /// </summary>
        public uint MagicNumber { get; set; }

        public List<byte> IndexesRawData { get; set; }

        [Browsable(false)] public int RawCount => IndexesRawData?.Count ?? -1;

        /// <summary>
        /// 索引表，每两个long值表示一个索引入口，指向一条正文内容。
        /// </summary>
        //public long[] Indexes { get; set; }
        public ContentIndex[] Indexes { get; set; }

        /// <summary>
        /// 索引个数(Indexes每两个long值代表一个索引项目)
        /// </summary>
        public ulong Count { get; set; }

        public ulong L2 { get; set; }

        /// <summary>
        /// 索引表本身的块长度
        /// </summary>
        public ulong IndexTableLength { get; set; }

        public ulong L4 { get; set; }

        /// <summary>
        /// 索引表尾部的文件偏移量，也即内容块的起始文件偏移
        /// </summary>
        public ulong Seg2ContentBlockOffset { get; set; }
    }
}