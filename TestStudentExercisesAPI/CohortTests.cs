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
    public class CohortTests
    {
        [Fact]
        public async Task Test_Get_All_Cohorts()
        {
            using (var client = new APIClientProvider().Client)
            {
                /*
                    ARRANGE
                */


                /*
                    ACT
                */
                var response = await client.GetAsync("/api/cohort");


                string responseBody = await response.Content.ReadAsStringAsync();
                var cohortList = JsonConvert.DeserializeObject<List<Cohort>>(responseBody);

                /*
                    ASSERT
                */
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.True(cohortList.Count > 0);
            }
        }

        [Fact]
        public async Task Test_Create_Cohort()
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

                // Construct a new cohort object to be sent to the API
                Cohort newCohort = new Cohort
                {
                    Name = "Cohort 38"
                };

                // Serialize the C# object into a JSON string
                var cohortAsJSON = JsonConvert.SerializeObject(newCohort);


                /*
                    ACT
                */

                // Use the client to send the request and store the response
                var response = await client.PostAsync(
                    "/api/cohort",
                    new StringContent(cohortAsJSON, Encoding.UTF8, "application/json")
                );

                // Store the JSON body of the response
                string responseBody = await response.Content.ReadAsStringAsync();

                // Deserialize the JSON into an instance of Student
                var addedCohort = JsonConvert.DeserializeObject<Cohort>(responseBody);


                /*
                    ASSERT
                */

                Assert.Equal(HttpStatusCode.Created, response.StatusCode);
                Assert.Equal("Cohort 38", addedCohort.Name);
            }
        }

        [Fact]
        public async Task Test_Modify_Cohort()
        {
            // New last name to change to and test
            string newName = "Cohort 39";

            using (var client = new APIClientProvider().Client)
            {
                /*
                    PUT section
                */
                Cohort modifiedCohort = new Cohort
                {
                    Name = newName,
                };
                var modifiedCohortAsJSON = JsonConvert.SerializeObject(modifiedCohort);

                var response = await client.PutAsync(
                    "/api/cohort/8",
                    new StringContent(modifiedCohortAsJSON, Encoding.UTF8, "application/json")
                );
                string responseBody = await response.Content.ReadAsStringAsync();

                Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);


                /*
                    GET section
                    Verify that the PUT operation was successful
                */
                var getCohort = await client.GetAsync("/api/cohort/8");
                getCohort.EnsureSuccessStatusCode();

                string getCohortBody = await getCohort.Content.ReadAsStringAsync();
                Cohort newCohort = JsonConvert.DeserializeObject<Cohort>(getCohortBody);

                Assert.Equal(HttpStatusCode.OK, getCohort.StatusCode);
                Assert.Equal(newName, newCohort.Name);
            }
        }

        [Fact]
        public async Task Test_Delete_Cohort()
        {
            using (var client = new APIClientProvider().Client)
            {
                /*
                    ARRANGE
                */


                /*
                    ACT
                */

                var response = await client.DeleteAsync("/api/cohort/7");

                /*
                    ASSERT
                */
                Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            }
        }
    }
}
