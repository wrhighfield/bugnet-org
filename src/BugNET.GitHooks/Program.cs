using System;

namespace BugNET.GitHooks
{
    public static class Program
    {
        /// <summary>
        /// Mains the specified args.
        /// </summary>
        /// <param name="args">The args.</param>
        public static void Main(string[] args)
        {

            log4net.Config.XmlConfigurator.Configure();
            var logger = log4net.LogManager.GetLogger("Main");

            //Console.WriteLine(IssueTrackerIntegration.GetRepositoryName(@"F:\SVN\Repositories\MyRepo"));
            //Console.ReadLine();

            try
            {
                if (string.Compare("post-commit", args[0], StringComparison.OrdinalIgnoreCase) != 0) return;
                logger.Info("Starting post-commit...");

                var repository = args[1];
                var revision = args[2];

                logger.InfoFormat("Executing IssueTrackerIntegration.UpdateIssueTrackerFromRevision(\"{0}\", \"{1}\")", repository, revision);
                var integration = new IssueTrackerIntegration();
                integration.UpdateIssueTrackerFromRevision(repository, revision);
                logger.Info("Finished IssueTrackerIntegration.UpdateIssueTrackerFromRevision\n");
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("An error occurred: {0} \n\n {1}", ex.Message, ex.StackTrace);
            }
        }
    }
}
