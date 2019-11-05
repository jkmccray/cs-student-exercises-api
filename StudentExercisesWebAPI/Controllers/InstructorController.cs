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
    public class InstructorController : Controller
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
        [HttpGet("{id}")]
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
        public void Post([FromBody] Instructor newInstructor)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"INSERT INTO Students (firstName, lastName, slackHandle, cohortId, cohort, specialty) 
                                            VALUES (@firstName, @lastName, @slackHandle, @cohortId, @cohort, @specialty)";
                    cmd.Parameters.Add(new SqlParameter("@firstName", newInstructor.FirstName));
                    cmd.Parameters.Add(new SqlParameter("@lastName", newInstructor.LastName));
                    cmd.Parameters.Add(new SqlParameter("@slackHandle", newInstructor.SlackHandle));
                    cmd.Parameters.Add(new SqlParameter("@cohortId", newInstructor.CohortId));
                    cmd.Parameters.Add(new SqlParameter("@cohort", newInstructor.Cohort));
                    cmd.Parameters.Add(new SqlParameter("@exercises", newInstructor.Specialty));

                    cmd.ExecuteNonQuery();
                }
            }
        }

        // EDIT: Instructor/
        [HttpPost]
        public void Update([FromBody] Instructor newInstructor)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"UPDATE Students
                                        SET firstName = @firstName, lastName = @lastName, slackHandle = @slackHandle,
                                            cohortId = @cohortId, cohort = @cohort, specialty = @specialty)";
                    cmd.Parameters.Add(new SqlParameter("@firstName", newInstructor.FirstName));
                    cmd.Parameters.Add(new SqlParameter("@lastName", newInstructor.LastName));
                    cmd.Parameters.Add(new SqlParameter("@slackHandle", newInstructor.SlackHandle));
                    cmd.Parameters.Add(new SqlParameter("@cohortId", newInstructor.CohortId));
                    cmd.Parameters.Add(new SqlParameter("@cohort", newInstructor.Cohort));
                    cmd.Parameters.Add(new SqlParameter("@exercises", newInstructor.Specialty));

                    cmd.ExecuteNonQuery();
                }
            }
        }


        // DELETE: Instructor/Delete/5
        public void Delete([FromBody] int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"DELETE FROM Students 
                                        WHERE id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));

                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}