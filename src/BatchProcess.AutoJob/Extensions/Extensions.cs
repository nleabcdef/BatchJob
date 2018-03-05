using System;
using System.Collections.Generic;
using System.Text;

namespace BatchProcess.AutoJob.Extensions
{
    public static class Ext
    {
        /// <summary>
        /// Converts IAutomatedJob to Retry, with given times and timespan..etc
        /// </summary>
        /// <param name="job">IAutomatedJob</param>
        /// <param name="times">no of times, the IAutomatedJob will be retried</param>
        /// <param name="interval">time span between retry</param>
        /// <param name="doBeforeRetry">validation before retry</param>
        /// <returns></returns>
        public static Retry<IAutomatedJob> ConvertToRetry(this IAutomatedJob job, int times, TimeSpan interval, Func<IJobContext, ValidationResult> doBeforeRetry = null)
        {
            return new Retry<IAutomatedJob>(job, times, interval, doBeforeRetry);
        }

        /// <summary>
        /// Runs IAutomatedJob with specified times
        /// </summary>
        /// <param name="job">IAutomatedJob</param>
        /// <param name="times">no of times, the IAutomatedJob will be repeated</param>
        /// <param name="doBeforeRepeat">validation</param>
        /// <returns></returns>
        public static IAutomatedJob Repeat(this IAutomatedJob job, int times, Func<IJobContext, ValidationResult> doBeforeRepeat = null)
        {
            return new Retry<IAutomatedJob>(job, times, TimeSpan.Zero, doBeforeRepeat);
        }
    }
    
}
