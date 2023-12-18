using System.ComponentModel.DataAnnotations;

namespace QueueUnderflow.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }
        [Required(ErrorMessage = "The name of the category is required")]
        public string CategoryName { get; set; }
        public virtual ICollection<Discussion>? Discussions { get; set; }
    }
}
