
namespace WMSSolution.Core.Job
{
    /// <summary>
    /// base interface
    /// using for the worker task have the schedule
    /// 
    /// </summary>
    public interface IJob
    {
        /// <summary>
        /// Unique identifier for the recurring job
        /// </summary>
        string JobId { get; }

        /// <summary>
        /// cron for set time 
        /// to apply run the task
        /// </summary>
        string CronExpression { get; }

        /// <summary>
        /// Execute
        /// </summary>
        /// <returns></returns>
        Task Execute();
    }
}
