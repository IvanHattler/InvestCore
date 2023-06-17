using System.ComponentModel.DataAnnotations.Schema;

namespace InvestCore.DataLayer.Entities
{
    public class IdentityEntity
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; }
    }
}
