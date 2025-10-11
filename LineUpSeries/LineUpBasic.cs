using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LineUpSeries
{
    public sealed class LineUpBasic : Game
    {
        public override string Name => "LineUpBasic";
        public LineUpBasic() : base() { }
        public override void Launch()
        {
            Console.WriteLine("==== LineUpBasic ====");
        }
    }
}
