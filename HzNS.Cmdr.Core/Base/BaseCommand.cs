using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using HzNS.Cmdr.Builder;

namespace HzNS.Cmdr.Base
{
    [SuppressMessage("ReSharper", "ConvertToAutoPropertyWhenPossible")]
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    public class BaseCommand : BaseOpt, ICommand
    {
        private List<ICommand> _subCommands;
        private List<IFlag> _flags;

        public List<ICommand> SubCommands
        {
            get => _subCommands;
            set => _subCommands = value;
        }

        public List<IFlag> Flags
        {
            get => _flags;
            set => _flags = value;
        }

        public ICommand AddCommand(ICommand cmd)
        {
            if (_subCommands == null)
                _subCommands = new List<ICommand>();

            cmd.Owner = this;
            _subCommands.Add(cmd);
            return this;
        }

        public ICommand AddFlag(IFlag flag)
        {
            if (_flags == null)
                _flags = new List<IFlag>();

            flag.Owner = this;
            _flags.Add(flag);
            return this;
        }
    }
}