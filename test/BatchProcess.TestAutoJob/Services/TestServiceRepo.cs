using BatchProcess.AutoJob;
using BatchProcess.AutoJob.Runtime;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace BatchProcess.TestAutoJob.Services
{
    public class TestServiceRepo
    {
        [Fact]
        public void Instance_with_default()
        {
            //arrange
            IServiceRepo serviceRepo = null;

            //act
            serviceRepo = ServiceRepo.Instance;

            //assert
            Assert.NotNull(serviceRepo);
        }

        [Fact]
        public void Provider_with_default()
        {
            //arrange
            IServiceProvider servicePro = null;

            //act
            servicePro = ServiceRepo.Instance.Provider;

            //assert
            Assert.NotNull(servicePro);
        }

        [Fact]
        public void GetService_for_default_types()
        {
            //arrange
            IServiceRepo svcRepo = ServiceRepo.Instance;

            //act
            var notificationMgr = svcRepo.GetService(typeof(INotificationManager<JobId>));
            var anotherMgr = svcRepo.GetService(typeof(INotificationManager<JobId>));

            var handler = svcRepo.GetService(typeof(IHookHandler<JobId>));
            var sRuntime = svcRepo.GetService(typeof(SequentialRuntime));
            var tRuntime = svcRepo.GetService(typeof(TaskRuntime));
            var thread = svcRepo.GetService(typeof(IWorkflowThread<JobResult>));
            var anotherThread = svcRepo.GetService(typeof(IWorkflowThread<JobResult>));
            var sHost = svcRepo.GetService(typeof(IWorkflowHost<SequentialRuntime>));
            var tHost = svcRepo.GetService(typeof(IWorkflowHost<TaskRuntime>));

            //assert
            Assert.NotNull(notificationMgr);
            Assert.NotNull(anotherMgr);
            Assert.Same(notificationMgr, anotherMgr);
            Assert.IsType(notificationMgr.GetType(), anotherMgr);
            Assert.NotNull(handler);
            Assert.NotNull(sRuntime);
            Assert.NotNull(tRuntime);
            Assert.NotNull(thread);
            Assert.NotSame(thread, anotherThread);
            Assert.IsType(thread.GetType(), anotherThread);
            Assert.NotNull(sHost);
            Assert.NotNull(tHost);
        }

        [Fact]
        public void GetService_should_return_null()
        {
            //arrange
            IServiceRepo svcRepo = ServiceRepo.Instance;

            //act
            var refNull = svcRepo.GetService(typeof(Xunit.Assert));
           
            //assert
            Assert.Null(refNull);
        }

        [Fact]
        public void GetService_with_null_check()
        {
            //arrange
            IServiceRepo svcRepo = ServiceRepo.Instance;

            //act
            //assert
            Assert.Throws<ArgumentNullException>(() => svcRepo.GetService(null));
        }

        [Fact]
        public void GetServiceOf_should_return_null()
        {
            //arrange
            IServiceRepo svcRepo = ServiceRepo.Instance;

            //act
            var refNull = svcRepo.GetServiceOf<Xunit.Assert>();
            var refNull1 = svcRepo.GetServiceOf<object>();

            //assert
            Assert.Null(refNull);
            Assert.Null(refNull1);
        }

        [Fact]
        public void GetServiceOf_for_default_types()
        {
            //arrange
            IServiceRepo svcRepo = ServiceRepo.Instance;

            //act
            var notificationMgr = svcRepo.GetServiceOf<INotificationManager<JobId>>();
            var anotherMgr = svcRepo.GetServiceOf<INotificationManager<JobId>>();

            var handler = svcRepo.GetServiceOf<IHookHandler<JobId>>();
            var sRuntime = svcRepo.GetServiceOf<SequentialRuntime>();
            var tRuntime = svcRepo.GetServiceOf<TaskRuntime>();
            var thread = svcRepo.GetServiceOf<IWorkflowThread<JobResult>>();
            var anotherThread = svcRepo.GetServiceOf<IWorkflowThread<JobResult>>();
            var sHost = svcRepo.GetServiceOf<IWorkflowHost<SequentialRuntime>>();
            var tHost = svcRepo.GetServiceOf<IWorkflowHost<TaskRuntime>>();

            //assert
            Assert.NotNull(notificationMgr);
            Assert.NotNull(anotherMgr);
            Assert.Same(notificationMgr, anotherMgr);
            Assert.IsType(notificationMgr.GetType(), anotherMgr);
            Assert.NotNull(handler);
            Assert.NotNull(sRuntime);
            Assert.NotNull(tRuntime);
            Assert.NotNull(thread);
            Assert.NotSame(thread, anotherThread);
            Assert.IsType(thread.GetType(), anotherThread);
            Assert.NotNull(sHost);
            Assert.NotNull(tHost);
        }

        [Fact]
        public void Static_CreateOrRepo_with_invalid_ServiceProvider()
        {
            //arrange
            //act
            //assert
            Assert.Throws<ArgumentNullException>(() => ServiceRepo.CreateOrRepo(null));
        }

        [Fact]
        public void Static_CreateOrRepo_with_custom_ServiceProvider()
        {
            //arrange
            var pro = new Mock<IServiceProvider>();

            //act
            var svcPro = ServiceRepo.CreateOrRepo(pro.Object);

            //assert
            Assert.NotNull(svcPro);
            Assert.Same(svcPro, ServiceRepo.Instance);
        }

        [Fact]
        public void Static_CreateOrRepo_with_valid_ServiceProvider()
        {
            //arrange
            var pro = new Mock<IServiceProvider>();
            var id = Helper.GetJobId();
            var job = Helper.GetFakeJob().Object;
            pro.Setup(p => p.GetService(It.Is<Type>(t => t == typeof(JobId)))).Returns(id).Verifiable();
            pro.Setup(p => p.GetService(It.Is<Type>(t => t == typeof(IFakeJob)))).Returns(job).Verifiable();

            //act
            var svcPro = ServiceRepo.CreateOrRepo(pro.Object);

            var svcId = ServiceRepo.Instance.GetServiceOf<JobId>();
            var svcJob = ServiceRepo.Instance.GetServiceOf<IFakeJob>();

            //assert
            Assert.NotNull(svcPro);
            Assert.Same(svcPro, ServiceRepo.Instance);
            Assert.NotNull(svcId);
            Assert.NotNull(svcJob);
            Assert.Same(svcId, id);
            Assert.Same(svcJob, job);
        }

        [Fact]
        public void Static_CreateOrRepo_expect_override_behaviour()
        {
            //arrange
            var pro = new Mock<IServiceProvider>();
            var workflowRuntime = new Mock<IWorkflowHost<TaskRuntime>>().Object;
            pro.Setup(p => p.GetService(It.Is<Type>(t => t == typeof(IWorkflowHost<TaskRuntime>)))).Returns(workflowRuntime).Verifiable();
            
            //act
            var svcPro = ServiceRepo.CreateOrRepo(pro.Object);

            var svcWorkflow = ServiceRepo.Instance.GetServiceOf<IWorkflowHost<TaskRuntime>>();
            var svcWorkflowAnother = ServiceRepo.Instance.GetService(typeof(IWorkflowHost<TaskRuntime>));

            //assert
            Assert.NotNull(svcPro);
            Assert.Same(svcPro, ServiceRepo.Instance);
            Assert.Same(workflowRuntime, svcWorkflow);
            Assert.Same(workflowRuntime, svcWorkflowAnother);
            pro.Verify(p => p.GetService(It.Is<Type>(t => t == typeof(IWorkflowHost<TaskRuntime>))),
                Times.Exactly(2));
        }
    }
}
