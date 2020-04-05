using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using HzNS.MdxLib.MDict.Tool;
using HzNS.MdxLib.models;

namespace HzNS.MdxLib.MDict
{
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    [SuppressMessage("ReSharper", "FieldCanBeMadeReadOnly.Global")]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    [SuppressMessage("ReSharper", "ConvertToConstant.Global")]
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    public partial class MDictLoader
    {
        public bool dumpAllKeywords = true;

        protected ulong UnitLen => (ulong) (DictHeader.GeneratedByEngineVersion == "2.0" ? 8 : 4);
        protected ulong HeaderEndPos => 4 + HeaderBytes;
        protected ulong MdictIndexBeginPos => HeaderEndPos; // + 4 bytes Seed, + 4 bytes CRC
        protected ulong MdictIndexEndPos => MdictIndexBeginPos + 4 + 5 * UnitLen + 4; // 
        protected ulong Seg1BeginPos => MdictIndexEndPos; // point to the magic-number
        protected ulong Seg1EndPos => Seg1BeginPos + _seg0.Seg1Length;
        public ulong Seg2BeginPos => Seg1EndPos;
        public ulong Seg2EndPos => Seg2BeginPos + _seg0.Seg2RawLength;
        public ulong ContentIndexTableBeginPos => Seg2EndPos;

        public ulong ContentIndexTableEndPos =>
            (DictLargeContentIndexTable?.IndexTableLength ?? (ulong) 0) + 4 * UnitLen + ContentIndexTableBeginPos;

        
        public override string Query(string word)
        {
            // Log($"-- Query : '{word}'");

            var idx = findInLevel1(word);
            if (idx < 0) return string.Empty;

            const int bufSize = 32768;
            using var fs = new FileStream(this.DictFileName, FileMode.Open, FileAccess.Read, FileShare.Read, bufSize);

            var w = word;
            if (!DictHeader.KeyCaseSensitiveBool)
                w = word.ToUpperInvariant();

            var kw1 = _seg1.IndexList[idx];
            var l2 = readLevel2Block(idx, kw1, fs);
            KwIndex2 result = null; // = l2.Find(kw2 => kw2.Keyword == word);
            var size = -1;
            for (var x = 0; x < l2.Count; x++)
            {
                if (l2[x].Keyword.ToUpperInvariant() != w) continue;

                result = l2[x];
                if (x < l2.Count - 1)
                {
                    var nextOne = l2[x + 1];
                    size = (int) (nextOne.RelOffsetUL - result.RelOffsetUL);
                }

                break;
            }

            if (result == null) return string.Empty;

            Console.WriteLine($"<!-- kw1 block #{idx}: {kw1} -->");
            Console.WriteLine($"<!-- kw2 found: {result} @ kw1 block #{idx} -->");

            Console.WriteLine($"<!-- unzipped offset: 0x{result.RelOffsetUL:X8}|{result.RelOffsetUL} -->");
            foreach (var ci in DictLargeContentIndexTable.Indexes)
            {
                if (result.RelOffsetUL > ci.OffsetUncomp + ci.UncompressedSize) continue;

                Console.WriteLine(
                    $"<!-- ctt: block found ({ci}) | Seg2ContentBlockOffset = 0x{_seg2.Seg2ContentBlockOffset:X8}, ContentIndexTableEndPos = 0x{ContentIndexTableEndPos:X8} -->");

                var block = seekAndLoadCttBlock(fs, ci);
                var ofs = (int) (result.RelOffsetUL - ci.OffsetUncomp);
                var data = new byte[size > 0 ? size : block.Length - ofs];
                Array.Copy(block, ofs, data, 0, data.Length);
                Console.WriteLine(
                    $"<!-- ctt entry '{word}': ofs = 0x{ofs:X8}/{ofs}, len = 0x{data.Length:X8}/{data.Length} -->");

                var html = DictHeader.LanguageMode.GetString(data, 0, data.Length);
                html = Normalize(html);
                return html;
            }

            return string.Empty;
        }

        private string Normalize(string src)
        {
            if (src.StartsWith("@@@LINK="))
            {
                var word = src.Substring(3 + 4 + 1).TrimEnd('\n', '\r', ' ', '\t', '\0');
                Console.WriteLine($"<!-- forwarding to '{word}' -->");
                return Query(word);
            }

            return src;
        }

        #region readLevel2Block

        private KwIndex2List readLevel2Block(int idx, KwIndex1 kw1, Stream fs)
        {
            var pos = Seg2BeginPos;
            // ulong unzipped = 0;
            for (var kwiIdx = 0; kwiIdx < idx; kwiIdx++)
            {
                var kwi = _seg1.IndexList[kwiIdx];
                pos += kwi.CompressedSize;
                // unzipped += kwi.UncompressedSize;
                // Console.WriteLine(
                // $"seg 1, block {kwiIdx}, end pos = {pos}/0x{pos:X8} | unzipped offset: {unzipped} | kwi = {kwi}");
            }

            // Console.WriteLine($"seg 1, block {idx}, start pos = {pos}/0x{pos:X8} | kwi = {kw1}");

            var list2 = new KwIndex2List();

            fs.Seek((long) pos, SeekOrigin.Begin);
            var magicNum = readUInt32(fs);
            var j2 = readUInt32(fs);
            var rawData = new byte[kw1.CompressedSize - 8];
            fs.Read(rawData, 0, rawData.Length);

            if (magicNum == 0x02000000)
            {
                #region InflaterDecompress

                try
                {
                    // var txt = CompressUtil.InflaterDecompress(rawData, 0, rawData.Length, false);
                    // TODO need review and debug
                    var txt = Zipper.InflateBufferWithPureZlib(rawData, rawData.Length);

                    #region log & trying to parse 2nd level indexes

                    var ofs = 0;
                    while (ofs < txt.Length)
                    {
                        var kwi2 = new KwIndex2 {RelOffsetUL = readUInt64(txt, ofs)};
                        ofs += 8;
                        var ofs0 = ofs;
                        if (Equals(DictHeader.LanguageMode, Encoding.Unicode))
                        {
                            uint x9 = 1;
                            while (x9 != 0)
                            {
                                x9 = readUInt16(txt, ofs);
                                ofs += 2;
                            }
                        }
                        else
                        {
                            while (txt[ofs] != 0) ofs++;
                            ofs++;
                        }

                        kwi2.Keyword = DictHeader.LanguageMode.GetString(txt, ofs0, ofs - ofs0).TrimEnd('\0');
                        list2.Add(kwi2);
                    }

                    #endregion
                }
                catch (Exception ex)
                {
                    ErrorLog(ex.ToString());
                    throw;
                }

                #endregion
            }
            else if (magicNum == 0x01000000)
            {
                #region LZO.Decompress (V2.0 & V1.2)

                #region v1.2: 提取二级索引表

                var ofs = 0;
                var cZipped = rawData[ofs]; //预览一个word
                var zipped = cZipped != (byte) 0; //如果头一个byte为0，则实际上该块未压缩。
                byte[] decompressedData;

                if (!zipped)
                {
                    decompressedData = rawData;
                }
                else
                {
                    #region TODO: unzip

                    decompressedData = new byte[kw1.UncompressedSize];
                    var ok = false;
                    // var in_len = rawData.Length - 3;
                    // int out_len = BitConverter.ToInt16(rawData, 1);

                    // if (!ok)
                    {
                        try
                        {
                            Zipper.MiniLzoDecompress(rawData, 0, (int) kw1.CompressedSize, decompressedData);
                            ok = decompressedData.Length == (int) kw1.UncompressedSize;
                            // Debug.Assert(ok, "M-dict 1.2 lzo decompressed failed.");
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine(ex.ToString());
                        }
                    }

                    if (!ok)
                    {
                        ErrorLog("BAD, 1.1");
                    }

                    #endregion
                }

                #region 解释二级索引表

                while (ofs < (int) kw1.UncompressedSize)
                {
                    var kwi2 = new KwIndex2 {RelOffsetUL = readUInt32(decompressedData, ofs)};
                    ofs += 4; // if (x1 < 0) { Debug.WriteLine(">>> ???"); }
                    var ofs0 = ofs;
                    if (Equals(DictHeader.LanguageMode, Encoding.Unicode))
                    {
                        uint x9 = 1;
                        while (x9 != 0)
                        {
                            x9 = readUInt16(decompressedData, ofs);
                            ofs += 2;
                        }
                    }
                    else
                    {
                        while (decompressedData[ofs] != 0) ofs++;
                        ofs++;
                    }

                    kwi2.Keyword = DictHeader.LanguageMode.GetString(decompressedData, ofs0, ofs - ofs0)
                        .TrimEnd('\0');
                    list2.Add(kwi2);
                }

                #endregion

                #endregion

                #endregion
            }
            else if (magicNum == 0)
            {
                #region log & trying to parse 2nd level indexes

                var ofs = 0;
                while (ofs < rawData.Length)
                {
                    var kwi2 = new KwIndex2 {RelOffsetUL = readUInt32(rawData, ofs)};
                    ofs += 4;
                    var ofs0 = ofs;
                    if (Equals(DictHeader.LanguageMode, Encoding.Unicode))
                    {
                        uint x9 = 1;
                        while (x9 != 0)
                        {
                            x9 = readUInt16(rawData, ofs);
                            ofs += 2;
                        }
                    }
                    else
                    {
                        while (rawData[ofs] != 0) ofs++;
                        ofs++;
                    }

                    kwi2.Keyword = DictHeader.LanguageMode.GetString(rawData, ofs0, ofs - ofs0).TrimEnd('\0');
                    list2.Add(kwi2);
                    Log($"    > {kwi2}");
                }

                #endregion
            }
            else
            {
                throw new Exception(
                    $"提取KWIndex2[]时，期望正确的算法标志0x2000000/0x1000000，然而遇到了{magicNum}/0x{magicNum:X}");
            }

            return list2;
        }

        #endregion

        private int findInLevel1(string word)
        {
            try
            {
                // var idx = _seg1.Matcher.Match(word);
                // return idx;

                var w = word;
                if (!DictHeader.KeyCaseSensitiveBool)
                    w = word.ToUpperInvariant();

                for (var kwiIdx = 0; kwiIdx < _seg1.IndexList.Length; kwiIdx++)
                {
                    var kwi = _seg1.IndexList[kwiIdx];
                    if (DictHeader.KeyCaseSensitiveBool)
                    {
                        if (string.CompareOrdinal(word, kwi.KeywordEnd) < 0)
                            return kwiIdx;
                    }
                    else
                    {
                        if (string.CompareOrdinal(w, kwi.KeywordEnd.ToUpperInvariant()) < 0)
                            return kwiIdx;
                    }
                }
            }
            catch (Exception)
            {
                // ignored
            }

            return -1;
        }

        #region seekAndLoadCttBlock

        private byte[] seekAndLoadCttBlock(Stream fs, ContentIndex ci)
        {
            var ofs = ContentIndexTableEndPos + 8 + ci.Offset;
            fs.Seek((long) ofs, SeekOrigin.Begin);
            Console.WriteLine();
            Console.WriteLine(
                $"<!-- seek to 0x{fs.Position:X8}/{fs.Position}, file length = 0x{fs.Length:X8}/{fs.Length} -->");
            Console.WriteLine();

            #region read this content table

            var magicNumber = _seg2.MagicNumber;
            var rawData = new byte[ci.CompressedSize];
            var r = fs.Read(rawData, 0, rawData.Length);
            if (_seg2.MagicNumber == 0)
            {
                if (ci.CompressedSize == ci.UncompressedSize)
                    return rawData;
                magicNumber = 0x01000000;
            }

            #endregion

            #region unzip it

            if (magicNumber == 0x02000000)
            {
                #region InflaterDecompress, Passed

                try
                {
                    // var txt = CompressUtil.InflaterDecompress(rawData, 0, rawData.Length);
                    var txt = Zipper.InflateBufferWithPureZlib(rawData, rawData.Length);

                    var path = $"{Util.StripPathExt(DictFileName)}.ctt.blk.ofs.{ofs}.txt";
                    writeFile(path, txt);

                    // var b = new byte[ci.UncompressedSize];
                    // Array.Copy(txt, (int) kwi2.CIUncompOffset, b, 0, (int) kwi2.CIUncompLength);
                    // return b;

                    return txt;
                }
                catch (Exception ex)
                {
                    ErrorLog(ex.ToString());
                    throw;
                }

                #endregion
            }
            else if (magicNumber == 0x01000000)
            {
                //对于1.2版本格式，可能并未压缩，故如果LZO失败，则将rawdata原样返回

                #region 试图进行 LZO Decompress。

                //Lzo1x解压缩
                var ok = false;
                var _decompData = new byte[ci.UncompressedSize];
                var _decompSize = 0;

                // if (!ok)
                {
                    try
                    {
                        Zipper.MiniLzoDecompress(rawData, 0, (int) ci.CompressedSize - 8, _decompData);
                        ok = _decompData.Length == (int) ci.UncompressedSize;

                        // var b = new byte[ci.UncompressedSize];
                        // Array.Copy(_decompData, (int) kwi2.CIUncompOffset, b, 0, (int) kwi2.CIUncompLength);
                        // return b;
                        return _decompData;
                    }
                    catch (Exception ex1)
                    {
                        ErrorLog(ex1.ToString());
                    }
                }

                if (!ok)
                {
                    try
                    {
                        // 1.2版本格式，cti数据块的长度为CompressedSize-8，数据块的末尾8个字节含义暂时未知。
                        //byte[] xyz = new byte[rawdata.Length - 8];
                        //Array.Copy(rawdata, xyz, xyz.Length);
                        Zipper.MiniLzoDecompress(rawData, 0, rawData.Length - 8, _decompData);
                        // _decompSize = Zipper.MiniLzoDecompress(rawdata, 0, rawdata.Length - 8, _decompData);
                        //xyz = new byte[8];
                        //byte[] _decompData1 = new byte[100];
                        //Array.Copy(rawdata, ci.CompressedSize-8, xyz, 0, xyz.Length);
                        //_decompSize = Zipper.MiniLzoDecompress(xyz, _decompData1);

                        #region debug

                        //if (k < 1)
                        //{
                        //    string path = string.Format("{0}.seg2.cti.{1:D5}.unz", CompressUtil.stripPathExt(fs.Name), k);
                        //    write_file(path, _decompData, _decompSize);
                        //    //write_file(CompressUtil.stripPathExt(fs.Name) + ".seg2.cti." + k + ".unz", _decompData, _decompSize);
                        //}

                        #endregion

                        if (_decompSize == (int) ci.UncompressedSize)
                            return _decompData;

                        // var b = new byte[kwi2.CIUncompLength];
                        // Array.Copy(_decompData, (int) kwi2.CIUncompOffset, b, 0, (int) kwi2.CIUncompLength);
                        // return b;
                        // //byte[] b = new byte[_decompSize];
                        // //Array.Copy(_decompData, b, _decompSize);
                        // //return b;
                        return _decompData;
                    }
                    catch (Exception ex)
                    {
                        //if (this.DictHeader.GeneratedByEngineVersion == "2.0")
                        {
                            ErrorLog(ex.ToString());
                        }
                    }
                }

                if (!ok)
                {
                    try
                    {
                        Zipper.MiniLzoDecompress(rawData, 0, (int) ci.CompressedSize, _decompData);
                        // _decompData = LZOHelper.LZOCompressor.Decompress(rawdata, 0, (int) ci.UncompressedSize);
                        //write_file(path, _decompData, _decompData.Length);
                        ok = _decompData.Length == (int) ci.UncompressedSize;
                    }
                    catch (Exception ex1)
                    {
                        ErrorLog(ex1.ToString());
                        throw;
                    }
                }

                #endregion

                //此外，1.2版本格式也可能采用了第二种压缩方式，这种情况下ci.UncompressedSize!=ci.CompressedSize
                //if (this.DictHeader.GeneratedByEngineVersion == "1.2")
                if (ci.UncompressedSize == ci.CompressedSize)
                {
                    //Version 1.2: 数据内容并未压缩故可直接取用。
                    return rawData;
                }
                else
                {
                    return rawData;
                }
            }
            else
            {
                throw new Exception($"提取KWIndex时，期望正确的算法标志0x2000000/0x1000000，然而遇到了{_seg2.MagicNumber}");
            }

            #endregion
        }

        #endregion
    }
}