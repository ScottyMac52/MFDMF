namespace MFDMF_Models.Models
{
    using System.Collections.Generic;

    public class DirectoryItem : Item
    {
        public IEnumerable<Item> Items { get; set; }

        public DirectoryItem()
        {
            Items = new List<Item>();
        }
    }
}
