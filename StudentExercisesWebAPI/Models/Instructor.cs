namespace StudentExercisesWebAPI.Models
{
    public class Instructor : NSSPerson
    {
        public string Specialty { get; set; }
        public void AssignStudentAnExercise(Student student, Exercise exercise) {
            student.Exercises.Add(exercise);
        }
    }
}