﻿using System;
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
    public class ExerciseController : Controller
    {
        private string _connectionString;

        public ExerciseController(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("DefaultConnection");
        }



        // GET: Exercise
        [HttpGet]
        public IEnumerable<Exercise> GetAllExercises(string include, string q)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    if (include == "students")
                    {
                        cmd.CommandText = @"SELECT e.Id AS ExerciseId, e.Name AS ExerciseName, e.Language, 
                                            s.FirstName, s.LastName, s.SlackHandle, s.CohortId, s.Id AS StudentId
                                            FROM Exercises e
                                            LEFT JOIN StudentExercises se ON e.Id = se.ExerciseId
                                            LEFT JOIN Students s on s.Id = se.StudentId";
                        SqlDataReader reader = cmd.ExecuteReader();
                        Dictionary<int, Exercise> exercises = new Dictionary<int, Exercise>();
                        while (reader.Read())
                        {
                            int exerciseId = reader.GetInt32(reader.GetOrdinal("ExerciseId"));
                            if (!exercises.ContainsKey(exerciseId))
                            {
                                Exercise newExercise = new Exercise()
                                {
                                    Id = exerciseId,
                                    Name = reader.GetString(reader.GetOrdinal("ExerciseName")),
                                    Language = reader.GetString(reader.GetOrdinal("Language"))
                                };
                                exercises.Add(exerciseId, newExercise);
                            }
                            if (!reader.IsDBNull(reader.GetOrdinal("StudentId")))
                            {
                                Student aStudent = new Student()
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("StudentId")),
                                    FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                                    LastName = reader.GetString(reader.GetOrdinal("LastName")),
                                    SlackHandle = reader.GetString(reader.GetOrdinal("SlackHandle")),
                                    CohortId = reader.GetInt32(reader.GetOrdinal("CohortId"))
                                };
                                exercises[exerciseId].AssignedStudents.Add(aStudent);
                            }
                        }
                        reader.Close();
                        return exercises.Values;
                    }
                    else if (q != null)
                    {
                        cmd.CommandText = @"SELECT Id, Name, Language FROM Exercises
                                            WHERE Name LIKE @searchString OR Language LIKE @searchString";
                        cmd.Parameters.Add(new SqlParameter("@searchString", "%" + q + "%"));
                        SqlDataReader reader = cmd.ExecuteReader();
                        List<Exercise> exercises = new List<Exercise>();
                        while (reader.Read())
                        {
                            Exercise newExercise = new Exercise()
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                Name = reader.GetString(reader.GetOrdinal("Name")),
                                Language = reader.GetString(reader.GetOrdinal("Language"))
                            };
                            exercises.Add(newExercise);
                        }
                        reader.Close();
                        return exercises;
                    }
                    else
                    {
                        cmd.CommandText = "SELECT Id, Name, Language FROM Exercises";
                        SqlDataReader reader = cmd.ExecuteReader();
                        List<Exercise> exercises = new List<Exercise>();
                        while (reader.Read())
                        {
                            Exercise newExercise = new Exercise()
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                Name = reader.GetString(reader.GetOrdinal("Name")),
                                Language = reader.GetString(reader.GetOrdinal("Language"))
                            };
                            exercises.Add(newExercise);
                        }
                        reader.Close();
                        return exercises;
                    }
                }
            }
        }

        // GET: Single exercise
        [HttpGet("{id}", Name = "GetExercise")]
        public IActionResult GetSingleExercise(int id)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT id, name, language 
                                    FROM Exercises
                                    WHERE id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    SqlDataReader reader = cmd.ExecuteReader();
                    Exercise anExercise = null;
                    if (reader.Read())
                    {
                        anExercise = new Exercise()
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("id")),
                            Name = reader.GetString(reader.GetOrdinal("name")),
                            Language = reader.GetString(reader.GetOrdinal("language"))
                        };
                    }
                    reader.Close();
                    if (anExercise == null)
                    {
                        return NotFound();
                    }
                    return Ok(anExercise);
                    
                }
            }
        }

        // POST: Exercise/Create
        [HttpPost]

        public async Task<IActionResult> Post([FromBody] Exercise newExercise)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"INSERT INTO Exercises (name, language) 
                                        OUTPUT INSERTED.Id
                                        VALUES (@name, @language)";
                    cmd.Parameters.Add(new SqlParameter("@name", newExercise.Name));
                    cmd.Parameters.Add(new SqlParameter("@language", newExercise.Language));

                    int newId = (int)cmd.ExecuteScalar();
                    newExercise.Id = newId;
                    return CreatedAtRoute("GetExercise", new { id = newId }, newExercise);
                }
            }
        }

        [HttpPut("{id}")]
        // EDIT: Exercise/Edit/5
        public async Task<IActionResult> Update([FromRoute] int id, [FromBody] Exercise updatedExercise)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"UPDATE Exercises 
                                        SET name = @name, language = @language
                                        WHERE id = @id";
                    cmd.Parameters.Add(new SqlParameter("@name", updatedExercise.Name));
                    cmd.Parameters.Add(new SqlParameter("@language", updatedExercise.Language));
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
        // DELETE: Exercise/Delete/5
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"DELETE FROM Exercises 
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