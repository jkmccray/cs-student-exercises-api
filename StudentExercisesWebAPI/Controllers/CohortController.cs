using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using StudentExercisesWebAPI.Models;

namespace StudentExercisesWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CohortController : Controller
    {
        private IConfiguration _config;
        private SqlConnection Connection
        {
            get
            {
                return new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            }
        }

        public CohortController(IConfiguration config)
        {
            _config = config;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                            SELECT s.Id AS StudentId, 
	                           s.FirstName AS StudentFirstName, 
	                           s.LastName AS StudentLastName, 
                               s.SlackHandle AS StudentSlack, 
							   c.Id AS CohortId,
	                           c.Name AS CohortName,
                               i.FirstName AS InstructorFirstName, 
	                           i.LastName AS InstructorLastName,
                        	   i.SlackHandle AS InstructorSlack,
	                           i.Id AS InstructorId
                                    FROM Cohorts c LEFT JOIN Students s ON s.CohortId = c.id
                                    LEFT JOIN Instructors i ON i.CohortId = c.id";

                    SqlDataReader reader = cmd.ExecuteReader();

                    List<Cohort> cohorts = new List<Cohort>();

                    while (reader.Read())
                    {
                        Cohort newCohort = null;
                        int cohortId = reader.GetInt32(reader.GetOrdinal("CohortId"));
                        if (!cohorts.Any(c => c.Id == cohortId))
                        {
                            newCohort = new Cohort()
                            {
                                Id = cohortId,
                                Name = reader.GetString(reader.GetOrdinal("CohortName"))
                            };

                            cohorts.Add(newCohort);
                        }

                        if (!reader.IsDBNull(reader.GetOrdinal("StudentId")))
                        {
                            int studentId = reader.GetInt32(reader.GetOrdinal("StudentId"));
                            if (!cohorts[cohortId - 1].StudentList.Any(s => s.Id == studentId))
                            {
                                Student newStudent = new Student()
                                {
                                    Id = studentId,
                                    FirstName = reader.GetString(reader.GetOrdinal("StudentFirstName")),
                                    LastName = reader.GetString(reader.GetOrdinal("StudentLastName")),
                                    SlackHandle = reader.GetString(reader.GetOrdinal("StudentSlack"))
                                };
                                cohorts[cohortId - 1].StudentList.Add(newStudent);
                            };
                        }

                        if (!reader.IsDBNull(reader.GetOrdinal("InstructorId")))
                        {
                            int instructorId = reader.GetInt32(reader.GetOrdinal("InstructorId"));
                            if (!cohorts[cohortId - 1].InstructorList.Any(i => i.Id == instructorId))
                            {
                                Instructor newInstructor = new Instructor()
                                {
                                    Id = instructorId,
                                    FirstName = reader.GetString(reader.GetOrdinal("InstructorFirstName")),
                                    LastName = reader.GetString(reader.GetOrdinal("InstructorLastName")),
                                    SlackHandle = reader.GetString(reader.GetOrdinal("InstructorSlack"))
                                };
                                cohorts[cohortId - 1].InstructorList.Add(newInstructor);
                            };
                        }
                    }

                    reader.Close();

                    return Ok(cohorts);
                }
            }
        }

        // Get single cohort based on id
        [HttpGet("{id}", Name = "GetCohort")]
        public async Task<IActionResult> GetCohort(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                            SELECT s.Id AS StudentId, 
	                           s.FirstName AS StudentFirstName, 
	                           s.LastName AS StudentLastName, 
                               s.SlackHandle AS StudentSlack, 
                               c.Id AS CohortId, 
	                           c.Name AS CohortName,
                               i.FirstName AS InstructorFirstName, 
	                           i.LastName AS InstructorLastName,
                        	   i.SlackHandle AS InstructorSlack,
	                           i.Id AS InstructorId
                                    FROM Cohorts c LEFT JOIN Students s ON s.CohortId = c.id
                                    LEFT JOIN Instructors i ON i.CohortId = c.id
                              WHERE c.Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    SqlDataReader reader = cmd.ExecuteReader();

                    Cohort aCohort = null;

                    while (reader.Read())
                    {
                        if (aCohort == null)
                        {
                            aCohort = new Cohort()
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("CohortId")),
                                Name = reader.GetString(reader.GetOrdinal("CohortName"))
                            };
                        }

                        if (!reader.IsDBNull(reader.GetOrdinal("StudentId")))
                        {
                            int studentId = reader.GetInt32(reader.GetOrdinal("StudentId"));
                            if (!aCohort.StudentList.Any(s => s.Id == studentId))
                            {
                                Student newStudent = new Student()
                                {
                                    Id = studentId,
                                    FirstName = reader.GetString(reader.GetOrdinal("StudentFirstName")),
                                    LastName = reader.GetString(reader.GetOrdinal("StudentLastName")),
                                    SlackHandle = reader.GetString(reader.GetOrdinal("StudentSlack"))
                                };
                                aCohort.StudentList.Add(newStudent);
                            };

                        }

                        if (!reader.IsDBNull(reader.GetOrdinal("InstructorId")))
                        {
                            int instructorId = reader.GetInt32(reader.GetOrdinal("InstructorId"));
                            if (!aCohort.InstructorList.Any(i => i.Id == instructorId))
                            {
                                Instructor newInstructor = new Instructor()
                                {
                                    Id = instructorId,
                                    FirstName = reader.GetString(reader.GetOrdinal("InstructorFirstName")),
                                    LastName = reader.GetString(reader.GetOrdinal("InstructorLastName")),
                                    SlackHandle = reader.GetString(reader.GetOrdinal("InstructorSlack"))
                                };
                                aCohort.InstructorList.Add(newInstructor);
                            };
                        }
                    }

                    if (aCohort == null)
                    {
                        return NotFound();
                    }

                    reader.Close();
                    return Ok(aCohort);
                }
            }
        }

        // POST: Cohort/Create
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Cohort newCohort)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"INSERT INTO Cohorts (Name) 
                                        OUTPUT INSERTED.Id
                                        VALUES (@name)";
                    cmd.Parameters.Add(new SqlParameter("@name", newCohort.Name));

                    int newId = (int)cmd.ExecuteScalar();
                    newCohort.Id = newId;
                    return CreatedAtRoute("GetStudent", new { id = newId }, newCohort);
                }
            }
        }

        [HttpPut("{id}")]
        // EDIT: Cohort/Edit/5
        public async Task<IActionResult> Update([FromRoute] int id, [FromBody] Cohort updatedCohort)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"UPDATE Cohorts 
                                        SET Name = @name
                                        WHERE id = @id";
                    cmd.Parameters.Add(new SqlParameter("@name", updatedCohort.Name));
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
        // DELETE: Cohort/Delete/5
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"DELETE FROM Cohorts 
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