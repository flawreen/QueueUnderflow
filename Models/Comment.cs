using System.ComponentModel.DataAnnotations;

namespace QueueUnderflow.Models
{
    public class Comment
    {
        [Key]
        public int Id { get; set; }
        [Required(ErrorMessage = "Continutul este obligatoriu")]
        public string Content { get; set; }
        public DateTime Date { get; set; }
        public int TopicId { get; set; }
        public virtual Topic Topic { get; set; }
    }
}
