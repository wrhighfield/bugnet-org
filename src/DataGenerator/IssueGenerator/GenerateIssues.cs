using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using IssueGenerator.Helpers;
using BugNET.Entities;
using BugNET.BLL;

namespace IssueGenerator
{
    [TestClass]
    public class GenerateIssues
    {
        /// <summary>
        /// Number of issues to create
        /// </summary>
        private const int NumberOfIssuesToCreate = 1000;
        
        [TestMethod]
        public void CreateRandomIssues()
        {                        
            var projects = ProjectManager.GetAllProjects();

            if (!projects.Any()) return;

            var project = projects.OrderBy(_ => Guid.NewGuid()).First();

            var startIssueCount = IssueManager.GetByProjectId(project.Id).Count;

            var randomProjectData = new RandomProjectData().SetProject(project);

            for (var i = 0; i < NumberOfIssuesToCreate; i++)
            {
                // Get Random yet valid data for the current project
                var category = randomProjectData.GetCategory();
                var milestone = randomProjectData.GetMilestone();
                var status = randomProjectData.GetStatus();
                var priority = randomProjectData.GetPriority();
                var issueType = randomProjectData.GetIssueType();
                var resolution = randomProjectData.GetResolution();

                var assigned = randomProjectData.GetUserName();
                // creator is also the owner
                var creatorUserName = randomProjectData.GetUserName();

                var date = GetRandomDate();

                var iss = new Issue
                { 
                    Id =  0, 
                    ProjectId = project.Id,
                    Title = RandomStrings.RandomString(30),
                    Description = RandomStrings.RandomString(250),
                    CategoryId = category.Id, 
                    PriorityId = priority.Id, 
                    StatusId = status.Id, 
                    IssueTypeId = issueType.Id,
                    MilestoneId = milestone.Id, 
                    AffectedMilestoneId = milestone.Id, 
                    ResolutionId = resolution.Id,
                    CreatorUserName = creatorUserName,
                    LastUpdateUserName = creatorUserName,
                    OwnerUserName = assigned,
                    AssignedUserName = assigned,
                    DateCreated = date,
                    LastUpdate = date,
                    DueDate = GetRandomDate(),
                    Disabled = false,
                    TimeLogged = RandomNumber(1, 24),
                    Votes = 0
                         
                };
                try
                {
                    IssueManager.SaveOrUpdate(iss);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }

            var endIssueCount = IssueManager.GetByProjectId(project.Id).Count;
            Assert.IsTrue(endIssueCount == startIssueCount + NumberOfIssuesToCreate);
        }

        private static DateTime GetRandomDate()
        {
            var timeSpan = DateTime.Today - DateTime.Today.AddMonths(-2);
            var randomTest = new Random();
            var newSpan = new TimeSpan(0, randomTest.Next(0, (int)timeSpan.TotalMinutes), 0);
            var newDate = DateTime.Today + newSpan;
            return newDate;
        }

        private static int RandomNumber(int min, int max)
        {
            var random = new Random();
            return random.Next(min, max);
        }
    }
}
