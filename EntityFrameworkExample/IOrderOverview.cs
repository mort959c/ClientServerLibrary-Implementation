using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityFrameworkExample
{
    public interface IOrderOverview
    {
        string Name { get; }
        string OverviewUnitPrice { get; }
        int Quantity { get; }
        decimal Total { get; }
    }
}
