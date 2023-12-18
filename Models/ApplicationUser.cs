using Microsoft.AspNetCore.Identity;

namespace QueueUnderflow.Models
{
    public class ApplicationUser : IdentityUser
    { 
        // un user poate posta mai multe raspunsuri
        public virtual ICollection<Answer>? Answers { get; set; }

        // un user poate posta mai multe discutii
        public virtual ICollection<Discussion>? Discussions { get; set; }


        // se pot adauga si atribute suplimentare
        public string? FirstName { get; set; }

        public string? LastName { get; set; }
    }
}
