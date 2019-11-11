using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using StudentExercisesWebAPI.Models;

namespace StudentExercisesWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StudentController : ControllerBase
    {
        private IConfiguration _config;

        private SqlConnection Connection
        {
            get
            {
                return new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            }
        }

        public StudentController(IConfiguration config)
        {
            _config = config;
        }

        // Get all students
        [HttpGet]
        public async Task<IActionResult> Get(string include)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    if (include == "exercise")
                    {
                        cmd.CommandText = @"
                                SELECT s.Id, s.FirstName, s.LastName, s.SlackHandle, 
                                       s.CohortId, c.Name AS CohortName,
                                       se.ExerciseId, e.Name AS ExerciseName, e.Language
                                  FROM Students s INNER JOIN Cohorts c ON s.CohortId = c.id
                                       LEFT JOIN StudentExercises se on se.StudentId = s.id
                                       LEFT JOIN Exercises e on se.ExerciseId = e.Id";

                        SqlDataReader reader = cmd.ExecuteReader();

                        Dictionary<int, Student> students = new Dictionary<int, Student>();

                        while (reader.Read())
                        {
                            int studentId = reader.GetInt32(reader.GetOrdinal("Id"));
                            if (!students.ContainsKey(studentId))
                            {
                                Student newStudent = new Student()
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                    FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                                    LastName = reader.GetString(reader.GetOrdinal("LastName")),
                                    SlackHandle = reader.GetString(reader.GetOrdinal("SlackHandle")),
                                    CohortId = reader.GetInt32(reader.GetOrdinal("CohortId")),
                                    Cohort = new Cohort()
                                    {
                                        Id = reader.GetInt32(reader.GetOrdinal("CohortId")),
                                        Name = reader.GetString(reader.GetOrdinal("CohortName")),
                                    }
                                };

                                students.Add(studentId, newStudent);
                            }

                            Student fromDictionary = students[studentId];

                            if (!reader.IsDBNull(reader.GetOrdinal("ExerciseId")))
                            {
                                Exercise anExercise = new Exercise()
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("ExerciseId")),
                                    Name = reader.GetString(reader.GetOrdinal("ExerciseName")),
                                    Language = reader.GetString(reader.GetOrdinal("Language"))
                                };
                                fromDictionary.Exercises.Add(anExercise);
                            }
                        }
                        reader.Close();

                        return Ok(students.Values);
                    }
                    else
                    {
                        cmd.CommandText = @"
                                SELECT s.Id, s.FirstName, s.LastName, s.SlackHandle, 
                                       s.CohortId, c.Name AS CohortName
                                  FROM Students s INNER JOIN Cohorts c ON s.CohortId = c.id";

                        SqlDataReader reader = cmd.ExecuteReader();

                        List<Student> students = new List<Student>();

                        while (reader.Read())
                        {
                            int studentId = reader.GetInt32(reader.GetOrdinal("Id"));
                            Student newStudent = new Student()
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                                LastName = reader.GetString(reader.GetOrdinal("LastName")),
                                SlackHandle = reader.GetString(reader.GetOrdinal("SlackHandle")),
                                CohortId = reader.GetInt32(reader.GetOrdinal("CohortId")),
                                Cohort = new Cohort()
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("CohortId")),
                                    Name = reader.GetString(reader.GetOrdinal("CohortName")),
                                }
                            };

                            students.Add(newStudent);
                        }
                        reader.Close();

                        return Ok(students);
                    }
                }
            }
        }

        // Get single student based on id
        [HttpGet("{id}", Name = "GetStudent")]
        public async Task<IActionResult> GetStudent(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                            SELECT s.Id, s.FirstName, s.LastName, s.SlackHandle, 
                                   s.CohortId, c.Name AS CohortName,
                                   se.ExerciseId, e.Name AS ExerciseName, e.Language
                              FROM Students s INNER JOIN Cohorts c ON s.CohortId = c.id
                                   LEFT JOIN StudentExercises se on se.StudentId = s.id
                                   LEFT JOIN Exercises e on se.ExerciseId = e.Id
                              WHERE s.Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    SqlDataReader reader = cmd.ExecuteReader();

                    Student aStudent = null;
                    if (reader.Read())
                    {
                        aStudent = new Student()
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                            LastName = reader.GetString(reader.GetOrdinal("LastName")),
                            SlackHandle = reader.GetString(reader.GetOrdinal("SlackHandle")),
                            CohortId = reader.GetInt32(reader.GetOrdinal("CohortId")),
                            Cohort = new Cohort()
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("CohortId")),
                                Name = reader.GetString(reader.GetOrdinal("CohortName"))
                            },
                        };

                        if (!reader.IsDBNull(reader.GetOrdinal("ExerciseId")))
                        {
                            Exercise anExercise = new Exercise()
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("ExerciseId")),
                                Name = reader.GetString(reader.GetOrdinal("ExerciseName")),
                                Language = reader.GetString(reader.GetOrdinal("Language"))
                            };
                            aStudent.Exercises.Add(anExercise);
                        }
                    }
                    if (aStudent == null)
                    {
                        return NotFound();
                    }
                    reader.Close();
                    return Ok(aStudent);
                }
            }
        }

        // POST: Student/Create
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Student newStudent)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"INSERT INTO Students (FirstName, LastName, SlackHandle, CohortId) 
                                        OUTPUT INSERTED.Id
                                        VALUES (@firstName, @lastName, @slackHandle, @cohortId)";
                    cmd.Parameters.Add(new SqlParameter("@firstName", newStudent.FirstName));
                    cmd.Parameters.Add(new SqlParameter("@lastName", newStudent.LastName));
                    cmd.Parameters.Add(new SqlParameter("@slackHandle", newStudent.SlackHandle));
                    cmd.Parameters.Add(new SqlParameter("@cohortId", newStudent.CohortId));

                    int newId = (int)cmd.ExecuteScalar();
                    newStudent.Id = newId;
                    return CreatedAtRoute("GetStudent", new { id = newId }, newStudent);
                }
            }
        }

        [HttpPut("{id}")]
        // EDIT: Student/Edit/5
        public async Task<IActionResult> Update([FromRoute] int id, [FromBody] Student updatedStudent)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"UPDATE Students 
                                        SET FirstName = @firstName, LastName = @lastName, SlackHandle = @slackHandle, 
                                            CohortId = @cohortId
                                        WHERE id = @id";
                    cmd.Parameters.Add(new SqlParameter("@firstName", updatedStudent.FirstName));
                    cmd.Parameters.Add(new SqlParameter("@lastName", updatedStudent.LastName));
                    cmd.Parameters.Add(new SqlParameter("@slackHandle", updatedStudent.SlackHandle));
                    cmd.Parameters.Add(new SqlParameter("@cohortId", updatedStudent.CohortId));
                    cmd.Parameters.Add(new SqlParameter("@id", id));

                    int rowsAffected = cmd.ExecuteNonQuery();
                    if (rowsAffected > 0)
                    {
                        return new StatusCodeResult(StatusCodes.Status204NoContent);
                    }
                    throw new Exception("No rows affected");
                }
            }
        }

        [HttpDelete("{id}")]
        // DELETE: Student/Delete/5
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"DELETE FROM Students 
                                        WHERE id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));

                    int rowsAffected = cmd.ExecuteNonQuery();
                    if (rowsAffected > 0)
                    {
                        return new StatusCodeResult(StatusCodes.Status204NoContent);
                    }
                    throw new Exception("No rows affected");
                }
            }
        }
    }
}