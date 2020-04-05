using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using HzNS.Cmdr;
using HzNS.Cmdr.Base;
using HzNS.Cmdr.Internal.Base;
using HzNS.MdxLib.MDict;

namespace mdx.Cmd
{
    public class RootCmd : BaseRootCommand, IAction
    {
        private RootCmd(IAppInfo appInfo) : base(appInfo)
        {
        }

        public static RootCmd New(IAppInfo appInfo, params Action<RootCmd>[] opts)
        {
            var r = new RootCmd(appInfo);

            foreach (var opt in opts)
            {
                opt(r);
            }

            return r;
        }

        public void Invoke(IBaseWorker w, IEnumerable<string> remainsArgs)
        {
            var count = 0;
            foreach (var filename in remainsArgs)
            {
                count++;
                if (string.IsNullOrEmpty(filename)) continue;

                w.log.logInfo($"loading {filename} ...");
                using var l = new MDictLoader(filename);
                try
                {
                    l.Process();
                    Console.WriteLine($"header: {l.DictHeader}");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.ToString());
                    // throw;
                }
                finally
                {
                    w.ParsedCount++;
                    // w.log.Information($"#{w.ParsedCount} parsed.");

                    // l.Dispose();
                }
            }

            if (count == 0)
            {
                w.ShowHelpScreen(w, remainsArgs.ToArray());
                return;
            }

            if (w.ParsedCount == 0)
                w.log.logWarning(null, "Nothing to parsed.");
        }
    }
}