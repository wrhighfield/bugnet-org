using BugNET.BLL;
using BugNET.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace IssueGenerator.Helpers
{
    /// <summary>
    /// Returns random data about supplied Project parameters
    /// </summary>
    internal class RandomProjectData
    {
        private IEnumerable<ITUser> users;
        private IEnumerable<Category> categories;
        private IEnumerable<Milestone> milestones;
        private IEnumerable<Priority> priorities;
        private IEnumerable<IssueType> issueTypes;
        private IEnumerable<Resolution> resolutions;
        private IEnumerable<Status> statuses;

        public RandomProjectData SetProject(Project project)
        {
            users = UserManager.GetUsersByProjectId(project.Id);
            categories = CategoryManager.GetByProjectId(project.Id);
            milestones = MilestoneManager.GetByProjectId(project.Id);
            priorities = PriorityManager.GetByProjectId(project.Id);
            issueTypes = IssueTypeManager.GetByProjectId(project.Id);
            resolutions = ResolutionManager.GetByProjectId(project.Id);
            statuses = StatusManager.GetByProjectId(project.Id);
            return this;
        }

        public string GetUserName() =>
            users.OrderBy(_ => Guid.NewGuid()).FirstOrDefault()?.UserName;

        public Category GetCategory() =>
            categories.OrderBy(_ => Guid.NewGuid()).First();

        public Milestone GetMilestone() =>
            milestones.OrderBy(_ => Guid.NewGuid()).First();

        public Priority GetPriority() =>
            priorities.OrderBy(_ => Guid.NewGuid()).First();

        public IssueType GetIssueType() =>
            issueTypes.OrderBy(_ => Guid.NewGuid()).First();

        public Resolution GetResolution() =>
            resolutions.OrderBy(_ => Guid.NewGuid()).First();

        public Status GetStatus() =>
            statuses.OrderBy(_ => Guid.NewGuid()).First();
    }
}
