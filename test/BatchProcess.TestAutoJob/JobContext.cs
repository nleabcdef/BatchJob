using BatchProcess.AutoJob;
using System;
using Xunit;
using System.Linq;

namespace BatchProcess.TestAutoJob
{
    public class TestJobContext
    {
        [Fact]
        public void SetValue()
        {
            //arrange
            IJobContext context = new JobContext(Helper.GetJobId());

            //act
            context.SetValue("", "key-1");
            context.SetValue(new {  value = "value-1"}, "key-2");
            context.SetValue<object>(null, "Key-3");
            context.SetValue(3, "Key-4");
            context.SetValue<object>(null, "Key-4");

            //assert
            Assert.NotNull(context.ParentJobId);
            Assert.Empty(context.ProcessedJobs);
            
        }

        [Fact]
        public void SetValue_when_exception()
        {
            //arrange
            IJobContext context = new JobContext(Helper.GetJobId());

            //assert
            Assert.NotNull(context.ParentJobId);
            Assert.Throws<ArgumentNullException>(() => context.SetValue("-", " "));
            Assert.Throws<ArgumentNullException>(() => context.SetValue("-", ""));
            Assert.Throws<ArgumentNullException>(() => context.SetValue("-", null));

        }

        [Fact]
        public void GetValue()
        {
            //arrange
            IJobContext context = new JobContext(Helper.GetJobId());

            //act
            context.SetValue("", "key-1");
            context.SetValue(new { value = "value-1" }, "key-2");
            context.SetValue<object>(null, "Key-3");
            context.SetValue(3, "Key-4");
            context.SetValue<object>(null, "Key-4");

            //assert
            Assert.NotNull(context.ParentJobId);
            Assert.Empty(context.ProcessedJobs);
            Assert.True(context.GetValue<string>("key-1") == "");
            Assert.True(context.GetValue<object>("key-2") != null);
            Assert.True(context.GetValue<object>("key-3") == null);
            Assert.True(context.GetValue<object>("key-4") == null);
            Assert.True(context.GetValue<object>("key-5") == null);

        }

        [Fact]
        public void GetValue_when_exception()
        {
            //arrange
            IJobContext context = new JobContext(Helper.GetJobId());

            //act
            context.SetValue<object>(null, "Key-3");

            //assert
            Assert.NotNull(context.ParentJobId);
            Assert.Empty(context.ProcessedJobs);
            Assert.Throws<ArgumentNullException>(() => context.GetValue<string>(""));
            Assert.Throws<ArgumentNullException>(() => context.GetValue<string>(null));
        }

        [Fact]
        public void AddToProcessed()
        {
            //arrange
            IJobContext context = new JobContext(Helper.GetJobId());

            //act
            var id1 = Helper.GetJobId();
            var id2 = Helper.GetJobId();
            var id3 = Helper.GetJobId();
            context.AddToProcessed(id1);
            context.AddToProcessed(id2);
            context.AddToProcessed(id3);

            //assert
            Assert.NotNull(context.ParentJobId);
            Assert.NotEmpty(context.ProcessedJobs);
            Assert.True(context.ProcessedJobs.Count == 3);
            Assert.All(context.ProcessedJobs, job => job.Name.Equals(id1.Name));

        }

        [Fact]
        public void AddToProcessed_when_exception()
        {
            //arrange
            IJobContext context = new JobContext(Helper.GetJobId());

            //assert
            Assert.NotNull(context.ParentJobId);
            Assert.Throws<ArgumentNullException>(() => context.AddToProcessed(null));
        }
    }
}
