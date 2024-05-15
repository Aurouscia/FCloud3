using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCloud3.WikiPreprocessor.Options.SubOptions
{
    public class ImplantsHandleOptions
    {
        public Func<string, string?> HandleImplant { get; private set; }
        private readonly ParserBuilder _master;
        public ImplantsHandleOptions(ParserBuilder master)
        {
            HandleImplant = x => x;
            _master = master;
        }

        public ParserBuilder AddImplantsHandler(Func<string,string?> handleImplant)
        {
                var original = HandleImplant;
                HandleImplant = (x) =>
                {
                    string? newAnswer = handleImplant(x);
                    if (newAnswer == x || newAnswer is null)
                        return original(x);
                    return newAnswer;
                };
            return _master;
        }
    }
}
