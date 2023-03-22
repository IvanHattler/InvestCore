using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvestCore.Domain.Models
{
    public class PortfolioInvestmentModel
    {
        public decimal DividendsAndCouponsOverall { get; set; }
        public decimal IISBonusesOverall { get; set; }
        public decimal DepositsOverall { get; set; }
    }
}
