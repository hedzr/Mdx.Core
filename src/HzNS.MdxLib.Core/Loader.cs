using System;
using System.IO;
using HzNS.MdxLib.MDict.Tool;
using HzNS.MdxLib.models;

namespace HzNS.MdxLib
{
    #region Dictionary File Loader

    public abstract class Loader : IDisposable
    {
        // ReSharper disable once MemberCanBeProtected.Global
        public string DictFileName { get; private set; }

        public DictionaryXmlHeader DictHeader { get; private set; }

        protected void SetDictionaryXmlHeader(string xml)
        {
            //if(DictionaryXmlHeader==null){
            this.DictHeader = (DictionaryXmlHeader) Util.FromXml(xml, typeof(DictionaryXmlHeader));
            if (this.DictHeader == null) return;

            if (string.IsNullOrEmpty(this.DictHeader.Title) ||
                DictHeader.Title.IndexOf("No HTML", StringComparison.Ordinal) >= 0)
            {
                this.DictHeader.Title = Path.GetFileNameWithoutExtension(DictFileName);
            }

            //}
        }

        protected bool HeaderIsValidate;
        protected ulong HeaderBytes;
        // ReSharper disable once MemberCanBeProtected.Global
        public bool IsLibraryData { get; internal set; }

        public abstract MDictIndex DictIndex { get; set; }
        public abstract MDictKwIndexTables DictKwIndexTables { get; set; }
        public abstract MDictContentIndexTable DictLargeContentIndexTable { get; set; }

        //public virtual Dictionary<string, DictItem> DictItems { get; set; }

        public virtual Loader Open(string dictFileName)
        {
            this.DictFileName = dictFileName;
            //this.DictItems = new Dictionary<string, DictItem>();
            this.DictHeader = null;
            return this;
        }

        protected virtual Loader Shutdown()
        {
            return this;
        }

        protected virtual void Log(string s)
        {
        } //自动追加回车换行

        protected virtual void LogString(string s)
        {
        }

        protected virtual void ErrorLog(string s)
        {
        }

        /// <summary>
        /// Detect the dictionary header whether it's valid or not.
        /// </summary>
        /// <returns></returns>
        public virtual bool TestIntegrity()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Open and process the header of a mdx/mdd dictionary file
        /// </summary>
        /// <returns></returns>
        public virtual bool Process()
        {
            return true;
        }

        public virtual string Query(string word)
        {
            return string.Empty;
        }
        
        public virtual byte[] LoadContentBytesByKeyword(KwIndex2 kwi2)
        {
            throw new NotImplementedException();
        }

        public virtual string LoadContentByKeyword(KwIndex2 kwi2)
        {
            throw new NotImplementedException();
        }

        public static Loader CreateInstance(string dictFileName)
        {
            if (!File.Exists(dictFileName))
                return null;

            var ext = Path.GetExtension(dictFileName).ToLower();
            if (ext != ".mdx" && ext != ".mdd") return null;

            Loader l = new MDict.MDictLoader();
            l.Open(dictFileName);
            return l;
        }

        #region IDisposable Members

        public void Dispose()
        {
            // Console.WriteLine("Loader.Dispose()");
            Shutdown();
        }

        #endregion
    }

    #endregion
}