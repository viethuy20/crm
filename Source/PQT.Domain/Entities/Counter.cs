using System.ComponentModel.DataAnnotations.Schema;

namespace PQT.Domain.Entities
{
    public class Counter : EntityBase
    {
        public Counter()
        {
        }

        public Counter(string name, int value)
        {
            Name = name;
            Value = value;
        }

        public string Name { get; set; }
        public int Value { get; set; }
    }
}
