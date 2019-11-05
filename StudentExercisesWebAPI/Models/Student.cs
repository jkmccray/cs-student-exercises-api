using System.Collections.Generic;

namespace StudentExercisesWebAPI.Models
{
    public class Student : NSSPerson
    {
        public List<Exercise> Exercises { get; set; } = new List<Exercise>();
    }
}