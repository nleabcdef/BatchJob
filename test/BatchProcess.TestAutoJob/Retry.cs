using System;
using Xunit;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using BatchProcess.AutoJob;
using Moq;

namespace BatchProcess.TestAutoJob
{
    public class TestRetry
    {
        [Fact]
        public void Retry_5_times()
        {
            //arrange
            var job = Helper.GetFakeJob(null, JobStatus.CompletedWithError);
            Retry<IFakeJob> retryJob = new Retry<IFakeJob>(job.Object, 5, TimeSpan.FromMilliseconds(1));

            //act
            var rslt = retryJob.Doable();

            //assert
            Assert.True(retryJob.Times == 5);
            Assert.True(retryJob.Interval.Milliseconds == 1);
            Assert.True(rslt.Status == JobStatus.CompletedWithError);
            job.Verify(j => j.Doable(), Times.Exactly(5));
            Assert.All(retryJob.RetryResults, r => r.Status.ToString().Equals(JobStatus.CompletedWithError.ToString()));
        }

        [Fact]
        public void Retry_success_very_first_time()
        {
            //arrange
            var job = Helper.GetFakeJob();
            Retry<IFakeJob> retryJob = new Retry<IFakeJob>(job.Object, 5, TimeSpan.FromMilliseconds(1));

            //act
            var rslt = retryJob.Doable();

            //assert
            Assert.True(retryJob.Times == 5);
            Assert.True(retryJob.Interval.Milliseconds == 1);
            Assert.True(rslt.Status == JobStatus.Completed);
            job.Verify(j => j.Doable(), Times.Once());
            Assert.True(retryJob.RetryResults.Count == 1);
            Assert.True(retryJob.RetryResults.FirstOrDefault().Status == JobStatus.Completed);
        }

        [Fact]
        public void Retry_5_times_with_failed_validation()
        {
            //arrange
            var job = Helper.GetFakeJob(null, JobStatus.CompletedWithError);
            Func<IJobContext, ValidationResult> validate = (ctx) => {
                return ValidationResult.NotValid;
            };
            Retry<IFakeJob> retryJob = new Retry<IFakeJob>(job.Object, 5, TimeSpan.FromMilliseconds(1), validate);

            //act
            var rslt = retryJob.Doable();

            //assert
            Assert.True(retryJob.Times == 5);
            Assert.True(retryJob.Interval.Milliseconds == 1);
            Assert.True(rslt.Status == JobStatus.CompletedWithError);
            job.Verify(j => j.Doable(), Times.Exactly(1));
            Assert.All(retryJob.RetryResults, r => r.Status.ToString().Equals(JobStatus.CompletedWithError.ToString()));
        }

        [Fact]
        public void Retry_5_times_with_validation_has_exception()
        {
            //arrange
            var job = Helper.GetFakeJob(null, JobStatus.CompletedWithError);
            Func<IJobContext, ValidationResult> validate = (ctx) => {
                int i = 0;
                i = 10/i;
                return ValidationResult.Valid;
            };
            Retry<IFakeJob> retryJob = new Retry<IFakeJob>(job.Object, 5, TimeSpan.FromMilliseconds(1), validate);

            //act
            var rslt = retryJob.Doable();

            //assert
            Assert.True(retryJob.Times == 5);
            Assert.True(retryJob.Interval.Milliseconds == 1);
            Assert.True(rslt.Status == JobStatus.CompletedWithError);
            job.Verify(j => j.Doable(), Times.Exactly(1));
            Assert.All(retryJob.RetryResults, r => r.Status.ToString().Equals(JobStatus.CompletedWithError.ToString()));
        }

        [Fact]
        public void Retry_10_times_with_successful_validation()
        {
            //arrange
            var job = Helper.GetFakeJob(null, JobStatus.CompletedWithError);
            Func<IJobContext, ValidationResult> validate = (ctx) => {
                return ValidationResult.Valid;
            };
            Retry<IFakeJob> retryJob = new Retry<IFakeJob>(job.Object, 10, TimeSpan.FromMilliseconds(1), validate);

            //act
            var rslt = retryJob.Doable();

            //assert
            Assert.True(retryJob.Times == 10);
            Assert.True(retryJob.Interval.Milliseconds == 1);
            Assert.True(rslt.Status == JobStatus.CompletedWithError);
            job.Verify(j => j.Doable(), Times.Exactly(10));
            Assert.All(retryJob.RetryResults, r => r.Status.ToString().Equals(JobStatus.CompletedWithError.ToString()));
        }

        [Fact]
        public void Retry_Exception_when_no_retry()
        {
            //arrange
            var job = Helper.GetFakeJob();

            //assert
            Assert.Throws<ArgumentOutOfRangeException>(() => new Retry<IFakeJob>(job.Object, 1, TimeSpan.FromMilliseconds(1)));
            
        }

    }
}
