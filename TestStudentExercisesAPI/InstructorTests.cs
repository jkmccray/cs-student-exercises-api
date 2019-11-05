using Newtonsoft.Json;
using StudentExercisesWebAPI.Models;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace TestStudentExercisesAPI
{
    public class InstructorTests
    {
        [Fact]
        public async Task Test_Get_All_Instructors()
        {
            using (var client = new APIClientProvider().Client)
            {
                /*
                    ARRANGE
                */


                /*
                    ACT
                */
                var response = await client.GetAsync("/api/instructor");


                string responseBody = await response.Content.ReadAsStringAsync();
                var instructorList = JsonConvert.DeserializeObject<List<Instructor>>(responseBody);

                /*
                    ASSERT
                */
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.True(instructorList.Count > 0);
            }
        }

        [Fact]
        public async Task Test_Create_Instructor()
        {
            /*
                Generate a new instance of an HttpClient that you can
                use to generate HTTP requests to your API controllers.
                The `using` keyword will automatically dispose of this
                instance of HttpClient once your code is done executing.
            */
            using (var client = new APIClientProvider().Client)
            {
                /*
                    ARRANGE
                */

                // Construct a new instructor object to be sent to the API
                Instructor newInstructor = new Instructor
                {
                    FirstName = "Rubeus",
                    LastName = "Hagrid",
                    SlackHandle = "gamekeepr",
                    Specialty = "Care of Magical Creatures",
                    CohortId = 5
                };

                // Serialize the C# object into a JSON string
                var instructorAsJSON = JsonConvert.SerializeObject(newInstructor);


                /*
                    ACT
                */

                // Use the client to send the request and store the response
                var response = await client.PostAsync(
                    "/api/instructor",
                    new StringContent(instructorAsJSON, Encoding.UTF8, "application/json")
                );

                // Store the JSON body of the response
                string responseBody = await response.Content.ReadAsStringAsync();

                // Deserialize the JSON into an instance of Instructor
                var addedInstructor = JsonConvert.DeserializeObject<Instructor>(responseBody);


                /*
                    ASSERT
                */

                Assert.Equal(HttpStatusCode.Created, response.StatusCode);
                Assert.Equal("Rubeus", addedInstructor.FirstName);
                Assert.Equal("Hagrid", addedInstructor.LastName);
                Assert.Equal("gamekeepr", addedInstructor.SlackHandle);
                Assert.Equal("Care of Magical Creatures", addedInstructor.Specialty);
                Assert.Equal(5, addedInstructor.CohortId);
            }
        }

        [Fact]
        public async Task Test_Modify_Instructor()
        {
            // New last name to change to and test
            string newSlackHandle = "potionsmaster";

            using (var client = new APIClientProvider().Client)
            {
                /*
                    PUT section
                */
                Instructor modifiedInstructor= new Instructor
                {
                   FirstName = "Horace",
                   LastName = "Slughorn",
                   CohortId = 6,
                   Specialty = "Potions",
                   SlackHandle = newSlackHandle

                };
                var modifiedInstructorAsJSON = JsonConvert.SerializeObject(modifiedInstructor);

                var response = await client.PutAsync(
                    "/api/instructor/9",
                    new StringContent(modifiedInstructorAsJSON, Encoding.UTF8, "application/json")
                );
                string responseBody = await response.Content.ReadAsStringAsync();

                Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);


                /*
                    GET section
                    Verify that the PUT operation was successful
                */
                var getInstructor = await client.GetAsync("/api/instructor/9");
                getInstructor.EnsureSuccessStatusCode();

                string getInstructorBody = await getInstructor.Content.ReadAsStringAsync();
                Instructor updatedInstructor = JsonConvert.DeserializeObject<Instructor>(getInstructorBody);

                Assert.Equal(HttpStatusCode.OK, getInstructor.StatusCode);
                Assert.Equal(newSlackHandle, updatedInstructor.SlackHandle);
            }
        }

        [Fact]
        public async Task Test_Delete_Instructor()
        {
            using (var client = new APIClientProvider().Client)
            {
                /*
                    ARRANGE
                */


                /*
                    ACT
                */

                var response = await client.DeleteAsync("/api/instructor/7");

                /*
                    ASSERT
                */
                Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            }
        }
    }
}
