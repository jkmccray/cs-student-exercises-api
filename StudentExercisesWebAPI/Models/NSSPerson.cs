using System.ComponentModel.DataAnnotations;


namespace StudentExercisesWebAPI.Models
{
    public class NSSPerson
    {
        public int Id { get; set; }

        [StringLength(50, MinimumLength = 2)]
        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        [StringLength(50, MinimumLength = 3)]
        [Required]
        public string SlackHandle { get; set; }

        [Required]
        public int CohortId { get; set; }
        public Cohort Cohort { get; set; }
    }
}