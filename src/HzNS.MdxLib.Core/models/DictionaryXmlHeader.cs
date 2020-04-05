using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Xml.Serialization;

namespace HzNS.MdxLib.models
{
    public enum RegisterByEnum
    {
        EMail,
        DeviceId
    }

    /// <summary>
    /// MDict字典文件mdx,mdd使用了这个Unicode Text的Xml文本头部
    /// </summary>
    [XmlRoot("Dictionary")]
    [TypeConverter(typeof(DictionaryXmlHeaderConverter)), Description("展开以查看应用程序的拼写选项。")]
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    [SuppressMessage("ReSharper", "ReturnTypeCanBeEnumerable.Global")]
    [SuppressMessage("ReSharper", "ConvertToAutoPropertyWithPrivateSetter")]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    [SuppressMessage("ReSharper", "ConvertToAutoPropertyWhenPossible")]
    [SuppressMessage("ReSharper", "ReturnTypeCanBeEnumerable.Global")]
    public class DictionaryXmlHeader
    {
        public override string ToString()
        {
            var sb = new StringBuilder(
                $"Title: {Title}\nDescription: {Description}\n" +
                $"GeneratedByEngineVersion: {GeneratedByEngineVersion}\n" +
                $"RequiredEngineVersion: {RequiredEngineVersion}\nEncrypted: {EncryptedInt}\n" +
                $"Compat: {Compat}\nCompact: {Compact}\nDataSourceFormat: {DataSourceFormat}\n" +
                $"StripKey: {StripKey}\nKeyCaseSensitive: {KeyCaseSensitive}\n" +
                $"Encoding: {Encoding}\nCreated At: {CreationDate}\n");
            foreach (var ce in CssList)
            {
                sb.Append($"{ce.Index}: {ce.Begin} .. {ce.End}\n");
            }

            return sb.ToString();
        }

        /// <summary>
        /// 2.0 / 1.2
        /// </summary>
        [XmlAttribute]
        public string GeneratedByEngineVersion { get; set; }

        /// <summary>
        /// 2.0 / 1.2
        /// </summary>
        [XmlAttribute]
        public string RequiredEngineVersion { get; set; }

        /// <summary>
        /// “Html”
        /// </summary>
        [XmlAttribute]
        public string Format { get; set; }

        /// <summary>
        /// Yes/No
        /// </summary>
        [XmlAttribute]
        public string KeyCaseSensitive
        {
            get => _keyCaseSensitive ? "Yes" : "No"; /*internal*/
            set => _keyCaseSensitive = (value == "Yes");
        }

        private bool _keyCaseSensitive;

        [XmlIgnore]
        public bool KeyCaseSensitiveBool
        {
            get => _keyCaseSensitive;
            set => _keyCaseSensitive = value;
        }

        /// <summary>
        /// Yes/No
        /// </summary>
        [XmlAttribute]
        public string StripKey
        {
            get => _stripKey ? "Yes" : "No"; /*internal*/
            set => _stripKey = (value == "Yes");
        }

        private bool _stripKey;

        [XmlIgnore]
        public bool StripKeyBool
        {
            get => _stripKey;
            set => _stripKey = value;
        }

        /// <summary>
        /// 0:
        /// 1:
        /// 2: 
        /// 3: 使用一个字典创建者密钥来生成字典时，将设定此值。字典发布者利用此机制可以选择注册码方式发放字典。
        /// </summary>
        [XmlAttribute]
        public string Encrypted { get; set; }

        [XmlIgnore]
        public int EncryptedInt
        {
            get
            {
                if (Encrypted.ToLower() == "yes") throw new Exception("加密方式为Yes，但我们并不能支持该模式！");
                int.TryParse(Encrypted, out var r);
                return r;
            }
        }

        [XmlIgnore] public bool EncryptedBool => EncryptedInt == 3 || EncryptedInt == 2;

        /// <summary>
        /// 字典用户应该采用的注册方式
        /// RegisterBy: "Email", ""
        /// </summary>
        [XmlAttribute]
        public RegisterByEnum RegisterBy { get; set; }

        //[XmlIgnore]
        //public bool RegisterByEmail { get { return RegisterBy == "EMail"; } }
        [XmlAttribute] public string Description { get; set; }
        [XmlAttribute] public string Title { get; set; }

        /// <summary>
        /// "GBK" is default, ...
        /// 
        /// LangMode int:
        /// 1: utf-8
        /// 2: unicode "utf-16LE"
        /// 3: big5
        /// others: gbk
        /// </summary>
        [XmlAttribute]
        public string Encoding { get; set; }

        public Encoding LanguageMode => System.Text.Encoding.GetEncoding(Encoding);

        /// <summary>
        /// 2011-4-11
        /// </summary>
        [XmlAttribute]
        public string CreationDate { get; set; }

        /// <summary>
        /// Yes/No
        /// </summary>
        [XmlAttribute]
        public string Compact
        {
            get => _compact ? "Yes" : "No"; /*internal*/
            set => _compact = (value == "Yes");
        }

        private bool _compact;

        [XmlIgnore]
        public bool CompactBool
        {
            get => _compact;
            set => _compact = value;
        }

        /// <summary>
        /// Yes/No
        /// </summary>
        [XmlAttribute]
        public string Compat
        {
            get => _compat ? "Yes" : "No"; /*internal*/
            set => _compat = (value == "Yes");
        }

        private bool _compat;

        [XmlIgnore]
        public bool CompatBool
        {
            get => _compat;
            set => _compat = value;
        }

        /// <summary>
        /// Yes/No
        /// </summary>
        [XmlAttribute]
        public string Left2Right
        {
            get => _left2Right ? "Yes" : "No"; /*internal*/
            set => _left2Right = (value == "Yes");
        }

        private bool _left2Right;

        [XmlIgnore]
        public bool Left2RightBool
        {
            get => _left2Right;
            set => _left2Right = value;
        }

        [XmlAttribute] public int DataSourceFormat { get; set; }

        [XmlAttribute]
        public string StyleSheet
        {
            get => _styleSheet;
            set => SetStyleSheet(value);
        }

        private string _styleSheet;
        private List<CssEntry> _cssList;

        [XmlIgnore] public List<CssEntry> CssList => _cssList;

        public void SetStyleSheet(string s)
        {
            _styleSheet = s;
            _cssList = new List<CssEntry>();
            if (string.IsNullOrEmpty(s))
                return;

            var lines = s.Split(new[] {"\r\n", "\n", "\r"}, StringSplitOptions.None);
            int ln;
            for (ln = 0; ln < lines.Length;)
            {
                CssEntry e = new CssEntry();
                if (!int.TryParse(lines[ln], out e._index)) break;
                ln++;
                e.Begin = lines[ln++];
                e.End = lines[ln++];
                _cssList.Add(e);
            }
        }
    }


    ///// <summary>
    ///// 压缩块的压缩信息
    ///// </summary>
    //[TypeConverter(typeof(DictionarySeg0Converter)), Description("展开以查看应用程序的拼写选项。")]
    //public class DictionarySeg3
    //{
    //    public List<int> Values { get; set; }

    //    [Browsable(false)]
    //    public int Count { get { return Values == null ? -1 : Values.Count; } }

    //    public int ZippedLength { get { return Values == null || Values.Count < 13 ? 0 : Values[Values.Count - 4]; } }
    //    public int UnzippedLength { get { return Values == null || Values.Count < 13 ? 0 : Values[Values.Count - 2]; } }
    //}
}