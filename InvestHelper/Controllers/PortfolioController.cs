using InvestCore.DataLayer;
using InvestCore.DataLayer.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InvestHelper.Controllers
{
    [Route("[controller]/[action]")]
    [ApiController]
    public class PortfolioController : ControllerBase
    {
        private readonly IDbContextFactory<BaseDbContext> _dbContextFactory;

        public PortfolioController(IDbContextFactory<BaseDbContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory ?? throw new ArgumentNullException(nameof(dbContextFactory));
        }

        [HttpGet]
        public IActionResult Index()
        {
            using (var context = _dbContextFactory.CreateDbContext())
            {
                var portfolios = context.Portfolios.ToList();
                return Ok();
            }
        }
    }
}
