using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LineUpSeries
{
    internal class LineUpSpin : Game
    {
        public override string Name => "LineUpSpin";
        public LineUpSpin() : base() { }
        public override void Launch()
        {
            Console.WriteLine("==== LineUpSpin ====");
        }
    }
}
