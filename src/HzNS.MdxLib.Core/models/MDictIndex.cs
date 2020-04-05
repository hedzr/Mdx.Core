using System.ComponentModel;
using System.Text;

namespace HzNS.MdxLib.models
{
    /// <summary>
    /// 
    /// </summary>
    [TypeConverter(typeof(DictionarySeg0Converter)), Description("展开以查看应用程序的拼写选项。")]
    public class MDictIndex
    {
        public uint maybeCRC32 { get; set; }
        public ulong CountOfSeg1Indexes { get; set; }
        public ulong TotalEntries { get; set; }
        public ulong Seg1UncompressedSize { get; set; }

        /// <summary>
        /// Seg1Length - 8 是正确的长度，注意此长度应该是小于330000的
        /// Seg1是一组索引：解密解压后为一级索引表。
        /// 紧随Seg1结尾的数据块由一级索引表进行索引，该数据块内容为全部关键字。
        /// </summary>
        public ulong Seg1Length { get; set; }

        public override string ToString()
        {
            var sb = new StringBuilder(
                $"TotalEntries: {TotalEntries}\nCountOfSeg1Indexes: {CountOfSeg1Indexes}\n" +
                $"Seg1UncompressedSize: {Seg1UncompressedSize}\n" +
                $"Seg1Length: {Seg1Length}\n" +
                $"Seg2RawLength: {Seg2RawLength}\n" +
                $"");

            return sb.ToString();
        }

        /// <summary>
        /// Seg2相对于Seg1结束位置的偏移量。
        /// Seg2是一组索引：二级索引表。
        /// 二级索引表所指向的数据块紧随Seg2的末端，该数据块内容为全部关键字所关联的正文内容。
        ///
        /// = Seg1 Raw Length
        /// </summary>
        public ulong Seg2RawLength { get; set; }

        //public List<long> Values { get; set; }
        //public long Seg1Length { get; set; }
        //public int DataBlockLength { get; set; }
        public uint i8NeverUsed { get; set; }
        public uint MagicNumber { get; set; } //33554422, magic number
        public byte[] Seed { get; set; }

        //[Browsable(false)]
        //public int Count { get { return Values == null ? -1 : Values.Count; } }

        //public long TotalEntries { get { return Values == null || Values.Count != 5 ? 0 : Values[1]; } }
        //public long __s { get { return Values == null || Values.Count != 5 ? 0 : Values[2]; } }
        ///// <summary>
        ///// Seg1Length - 8 是正确的长度，注意此长度应该是小于330000的
        ///// Seg1是一组索引：解密解压后为一级索引表。
        ///// 紧随Seg1结尾的数据块由一级索引表进行索引，该数据块内容为全部关键字。
        ///// </summary>
        //public long Seg1Length { get { return Values == null || Values.Count != 5 ? 0 : Values[3]; } }
        ///// <summary>
        ///// Seg2相对于Seg1结束位置的偏移量。
        ///// Seg2是一组索引：二级索引表。
        ///// 二级索引表所指向的数据块紧随Seg2的末端，该数据块内容为全部关键字所关联的正文内容。
        ///// </summary>
        //public long Seg2Offset { get { return Values == null || Values.Count != 5 ? 0 : Values[4]; } }
    }
}