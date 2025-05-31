//Ivan Dochev 241836X
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

        public SplicingRule() { }

        public SplicingRule(string parentAName, int parentACount, string parentBName, int parentBCount, string childName)
        {
            this.parentAName = parentAName;
            this.parentACount = parentACount;
            this.parentBName = parentBName;
            this.parentBCount = parentBCount;
            this.childName = childName;
        }
    }
}
