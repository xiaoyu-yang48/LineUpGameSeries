using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LineUpSeries
{
    public class ChangeCell
    {
        public List<Cell> Cells { get; } = new List<Cell>();
        public void Add(Cell cell) => Cells.Add(cell);
    }
}
