using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using HzNS.Cmdr;
using HzNS.Cmdr.Base;
using HzNS.Cmdr.Logger.Serilog;
using HzNS.MdxLib.MDict;
using mdx.Cmd;

namespace mdx
{
    /// <summary>
    ///
    /// ll
    /// </summary>
    [SuppressMessage("ReSharper", "ArrangeTypeMemberModifiers")]
    [SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
    [SuppressMessage("ReSharper", "ArrangeTypeModifiers")]
    [SuppressMessage("ReSharper", "CommentTypo")]
    class Program
    {
        static void Main(string[] args)
        {
            // Cmdr: A CommandLine Arguments Parser
            Cmdr.NewWorker(RootCmd.New(
                        new AppInfo {AppName = "mdx-tool",}, (root) =>
                        {
                            root.AddCommand(new Command
                            {
                                Short = "lkp", Long = "lookup", Description = "dictionary lookup tool",
                                TailArgs = "<Mdx Files (*.mdx;*.mdd)> <word-pattern>",
                                Action = (lookupAction)
                            });

                            root.AddCommand(new Command
                            {
                                Short = "ls", Long = "list",
                                Description = "list a dictionary entries and dump for debugging",
                                TailArgs = "<Mdx Files (*.mdx;*.mdd)>",
                                Action = (listAction)
                            });

                            root.AddCommand(new Command {Short = "t", Long = "tags", Description = "tags operations"}
                                .AddCommand(new TagsAddCmd())
                                .AddCommand(new TagsRemoveCmd())
                                // .AddCommand(new TagsAddCmd { }) // dup-test
                                .AddCommand(new TagsListCmd())
                                .AddCommand(new TagsModifyCmd())
                            );
                        }), // <- RootCmd
                    // Options ->
                    (w) =>
                    {
                        w.SetLogger(SerilogBuilder.Build((logger) =>
                        {
                            logger.EnableCmdrLogInfo = false;
                            logger.EnableCmdrLogTrace = false;
                        }));

                        // w.EnableDuplicatedCharThrows = true;
                    })
                .Run(args);

            // HzNS.MdxLib.Core.Open("*.mdx,mdd,sdx,wav,png,...") => mdxfile
            // mdxfile.Preload()
            // mdxfile.GetEntry("beta") => entryInfo.{item,index}
            // mdxfile.Find("a")           // "a", "a*b", "*b"
            // mdxfile.Close()
            // mdxfile.Find()
            // mdxfile.Find()
            // mdxfile.Find()

            // Log.CloseAndFlush();
            // Console.ReadKey();
        }

        // ReSharper disable once InconsistentNaming
        private static void lookupAction(IBaseWorker w, IBaseOpt cmd, IEnumerable<string> args)
        {
            var enumerable = args.ToList();
            var mdxFile = enumerable.ElementAtOrDefault(0);
            var word = enumerable.ElementAtOrDefault(1);
            if (string.IsNullOrWhiteSpace(mdxFile))
            {
                w.ShowHelpScreen(w, enumerable);
                // w.ErrorPrint("no valid mdx file specified.");
                return;
            }

            var mdx = new MDictLoader(mdxFile);
            if (!mdx.Process())
            {
                // w.ErrorPrint($"CANNOT load and process the mdx file: {mdxFile}.");
                return;
            }

            Console.WriteLine($"<!--\n\nHeader: \n\n{mdx.DictHeader}\n\nIndex: \n\n{mdx.DictIndex}\n\n-->\n");

            if (!string.IsNullOrWhiteSpace(word))
            {
                // Console.WriteLine($"Lookup for word '{word}'...");
                // Console.WriteLine(word);
                var s = mdx.Query(word);
                Console.WriteLine(s);
            }
        }

        // ReSharper disable once InconsistentNaming
        private static void listAction(IBaseWorker w, IBaseOpt cmd, IEnumerable<string> args)
        {
            var enumerable = args.ToList();
            var mdxFile = enumerable.ElementAtOrDefault(0);
            var word = enumerable.ElementAtOrDefault(1);
            if (string.IsNullOrWhiteSpace(mdxFile))
            {
                w.ShowHelpScreen(w, enumerable);
                // w.ErrorPrint("no valid mdx file specified.");
                return;
            }

            Console.WriteLine($"{mdxFile} / {word}");
            var mdx = new MDictLoader(mdxFile) {PreloadAll = true};
            if (!mdx.Process())
            {
                // w.ErrorPrint($"CANNOT load and process the mdx file: {mdxFile}.");
                return;
            }

            Console.WriteLine($"Header: \n\n{mdx.DictHeader}\n\nIndex: \n\n{mdx.DictIndex}\n\n");
        }
    }
}