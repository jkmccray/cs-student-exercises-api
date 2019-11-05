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
    public class InstructorController : ControllerBase
    {
        private IConfiguration _config;

        private SqlConnection Connection
        {
            get
            {
                return new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            }
        }

        public InstructorController(IConfiguration config)
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
                            SELECT i.Id, i.FirstName, i.LastName, i.SlackHandle, i.Specialty,
                                   i.CohortId, c.Name AS CohortName
                              FROM Instructors i INNER JOIN Cohorts c ON i.CohortId = c.id";

                    SqlDataReader reader = cmd.ExecuteReader();

                    List<Instructor> instructors = new List<Instructor>();

                    while (reader.Read())
                    {
                        Instructor newInstructor= new Instructor()
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                            LastName = reader.GetString(reader.GetOrdinal("LastName")),
                            SlackHandle = reader.GetString(reader.GetOrdinal("SlackHandle")),
                            Specialty = reader.GetString(reader.GetOrdinal("Specialty")),
                            CohortId = reader.GetInt32(reader.GetOrdinal("CohortId")),
                            Cohort = new Cohort()
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("CohortId")),
                                Name = reader.GetString(reader.GetOrdinal("CohortName")),
                            }
                        };

                        instructors.Add(newInstructor);
                    }

                    reader.Close();
                    return Ok(instructors);

                }
            }
        }

        // Get single instructor based on id
        [HttpGet("{id}", Name = "GetInstructor")]
        public async Task<IActionResult> GetOneInstructor(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                            SELECT i.Id, i.FirstName, i.LastName, i.SlackHandle, i.Specialty,
                                   i.CohortId, c.Name AS CohortName
                              FROM Instructors i INNER JOIN Cohorts c ON i.CohortId = c.id
                              WHERE i.Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    SqlDataReader reader = cmd.ExecuteReader();

                    Instructor anInstructor = null;
                    if (reader.Read())
                    {
                        anInstructor = new Instructor()
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                            LastName = reader.GetString(reader.GetOrdinal("LastName")),
                            SlackHandle = reader.GetString(reader.GetOrdinal("SlackHandle")),
                            Specialty = reader.GetString(reader.GetOrdinal("Specialty")),
                            CohortId = reader.GetInt32(reader.GetOrdinal("CohortId")),
                            Cohort = new Cohort()
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("CohortId")),
                                Name = reader.GetString(reader.GetOrdinal("CohortName")),
                            }
                        };
                    }
                    if (anInstructor == null)
                    {
                        return NotFound();
                    }
                    reader.Close();
                    return Ok(anInstructor);
                }
            }
        }

        // POST: Instructor/Create
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Instructor newInstructor)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"INSERT INTO Instructors (FirstName, LastName, SlackHandle, CohortId, Specialty) 
                                            OUTPUT INSERTED.Id
                                            VALUES (@firstName, @lastName, @slackHandle, @cohortId, @specialty)";
                    cmd.Parameters.Add(new SqlParameter("@firstName", newInstructor.FirstName));
                    cmd.Parameters.Add(new SqlParameter("@lastName", newInstructor.LastName));
                    cmd.Parameters.Add(new SqlParameter("@slackHandle", newInstructor.SlackHandle));
                    cmd.Parameters.Add(new SqlParameter("@specialty", newInstructor.Specialty));
                    cmd.Parameters.Add(new SqlParameter("@cohortId", newInstructor.CohortId));

                    int newId = (int)cmd.ExecuteScalar();
                    newInstructor.Id = newId;
                    return CreatedAtRoute("GetInstructor", new { id = newId }, newInstructor);
                }
            }
        }

        [HttpPut("{id}")]
        // EDIT: Instructor/
        public async Task<IActionResult> Update([FromRoute] int id, [FromBody] Instructor modifiedInstructor)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"UPDATE Instructors
                                        SET FirstName = @firstName, LastName = @lastName, SlackHandle = @slackHandle,
                                            CohortId = @cohortId, Specialty = @specialty
                                        WHERE Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@firstName", modifiedInstructor.FirstName));
                    cmd.Parameters.Add(new SqlParameter("@lastName", modifiedInstructor.LastName));
                    cmd.Parameters.Add(new SqlParameter("@slackHandle", modifiedInstructor.SlackHandle));
                    cmd.Parameters.Add(new SqlParameter("@cohortId", modifiedInstructor.CohortId));
                    cmd.Parameters.Add(new SqlParameter("@specialty", modifiedInstructor.Specialty));
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
        // DELETE: Instructor/Delete/5
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