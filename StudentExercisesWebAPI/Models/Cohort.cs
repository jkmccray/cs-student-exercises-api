using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace StudentExercisesWebAPI.Models
{
    public class Cohort
    {
        public int Id { get; set; }

        [StringLength(11, MinimumLength = 5)]
        [Required]
        public string Name { get; set; }
        public List<Student> StudentList { get; set; } = new List<Student>();
        public List<Instructor> InstructorList { get; set; } = new List<Instructor>();
    }
}