using System.ComponentModel.DataAnnotations;

namespace QueueUnderflow.Models
{
    public class Answer
    {
        [Key]
        public int Id { get; set; }
        [Required(ErrorMessage = "The content of the answer is required")]
        public string Content { get; set; }
        public DateTime Date { get; set; }
        public int DiscussionId { get; set; }
        //ar fi trebuit sa fie DiscussionId
        //nu stiu exact cum sa schimb, doar scriu denumirea noua si refac Migratia?
        public virtual ICollection<Discussion> Discussions { get; set; }

        public virtual ApplicationUser User { get; set; }
    }
}
