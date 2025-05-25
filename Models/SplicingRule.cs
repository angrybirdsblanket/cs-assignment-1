using System.ComponentModel.DataAnnotations;

namespace PokemonPocket.Models
{
    public class SplicingRule
    {
        [Key]
        public int Id { get; set; }
        public string parentAName { get; set; }
        public int parentACount { get; set; }
        public string parentBName { get; set; }
        public int parentBCount { get; set; }
        public string childName { get; set; }
    }
}
