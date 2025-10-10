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

        public static void Launch()
        {
            Console.WriteLine("LineUpBasic 模式尚未实现，按回车返回主菜单。");
            Console.ReadLine();
            Console.Clear();
        }
    }
}
