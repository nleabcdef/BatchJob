using BatchProcess.AutoJob;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Xunit;

namespace BatchProcess.TestAutoJob.Hooks
{
    public class TestNotificationManager
    {
        [Fact]
        public void Ctor_withDefault()
        {
            //arrange
            INotificationManager<JobId> notificationMgr = null;

            //act
            notificationMgr = new NotificationManager();

            //assert
            Assert.NotNull(notificationMgr);
        }

        [Fact]
        public void RegisterHook_with_unique_handlers()
        {
            //arrange
            INotificationManager<JobId> notificationMgr = null;
            List<Guid> registered = new List<Guid>();
            //act
            notificationMgr = new NotificationManager();

            registered.Add(notificationMgr.RegisterHook(Helper.GetJobId(), MessageType.All, GetHanlder()));
            registered.Add(notificationMgr.RegisterHook(Helper.GetJobId(), MessageType.Error, GetHanlder()));
            registered.Add(notificationMgr.RegisterHook(Helper.GetJobId(), MessageType.Debug, GetHanlder()));
            registered.Add(notificationMgr.RegisterHook(Helper.GetJobId(), MessageType.Info, GetHanlder()));
            registered.Add(notificationMgr.RegisterHook(Helper.GetJobId(), MessageType.Warning, GetHanlder()));

            //assert

            Assert.All(registered, r => Assert.False(r == default(Guid)));
            Assert.True(registered.Distinct().Count() == 5);
        }

        [Fact]
        public void RegisterHook_reuse_handlers()
        {
            //arrange
            INotificationManager<JobId> notificationMgr = null;
            List<Guid> registered = new List<Guid>();
            //act
            notificationMgr = new NotificationManager();

            var jobid = Helper.GetJobId();
            var handler = GetHanlder();

            registered.Add(notificationMgr.RegisterHook(Helper.GetJobId(), MessageType.All, GetHanlder()));
            registered.Add(notificationMgr.RegisterHook(jobid, MessageType.Error, handler));
            registered.Add(notificationMgr.RegisterHook(jobid, MessageType.Debug, handler));
            registered.Add(notificationMgr.RegisterHook(Helper.GetJobId(), MessageType.Info, GetHanlder()));
            registered.Add(notificationMgr.RegisterHook(Helper.GetJobId(), MessageType.Warning, GetHanlder()));

            //assert

            Assert.All(registered, r => Assert.False(r == default(Guid)));
            Assert.True(registered.Distinct().Count() == 5);
        }

        [Fact]
        public void RegisterHook_with_duplicate_hooks()
        {
            //arrange
            INotificationManager<JobId> notificationMgr = null;
            List<Guid> registered = new List<Guid>();

            //act
            notificationMgr = new NotificationManager();

            var jobid = Helper.GetJobId();
            var handler = GetHanlder();

            registered.Add(notificationMgr.RegisterHook(jobid, MessageType.Error, handler));
            registered.Add(notificationMgr.RegisterHook(Helper.GetJobId(), MessageType.All, GetHanlder()));
            registered.Add(notificationMgr.RegisterHook(jobid, MessageType.Error, handler));
            registered.Add(notificationMgr.RegisterHook(Helper.GetJobId(), MessageType.Info, GetHanlder()));
            registered.Add(notificationMgr.RegisterHook(jobid, MessageType.Error, handler));

            //assert

            Assert.All(registered, r => Assert.False(r == default(Guid)));
            Assert.True(registered.Distinct().Count() == 3);
        }

        [Fact]
        public void RegisterHook_with_invalid_handler()
        {
            //arrange
            INotificationManager<JobId> notificationMgr = null;

            //act
            notificationMgr = new NotificationManager();
            var jobId = Helper.GetJobId();

            //assert

            Assert.Throws<ArgumentNullException>(() => notificationMgr.RegisterHook(jobId, MessageType.All, null));
            Assert.Throws<ArgumentNullException>(() => notificationMgr.RegisterHook(jobId, MessageType.All, default(IHookHandler<JobId>)));
        }

        [Fact]
        public void RegisterHook_with_invalid_recieverKey()
        {
            //arrange
            INotificationManager<JobId> notificationMgr = null;

            //act
            notificationMgr = new NotificationManager();
            var handler = GetHanlder();

            //assert

            Assert.Throws<ArgumentNullException>(() => notificationMgr.RegisterHook(null, MessageType.All, handler));
            Assert.Throws<ArgumentNullException>(() => notificationMgr.RegisterHook(default(JobId), MessageType.All, handler));
        }

        [Fact]
        public void IsRegistered_should_return_true()
        {
            //arrange
            INotificationManager<JobId> notificationMgr = null;
            List<Guid> registered = new List<Guid>();
            List<bool> actuals = new List<bool>();

            //act
            notificationMgr = new NotificationManager();

            var jobid = Helper.GetJobId();
            var handler = GetHanlder();

            registered.Add(notificationMgr.RegisterHook(jobid, MessageType.Error, handler));
            registered.Add(notificationMgr.RegisterHook(Helper.GetJobId(), MessageType.All, GetHanlder()));
            registered.Add(notificationMgr.RegisterHook(jobid, MessageType.Error, handler));
            registered.Add(notificationMgr.RegisterHook(Helper.GetJobId(), MessageType.Info, GetHanlder()));
            registered.Add(notificationMgr.RegisterHook(jobid, MessageType.Error, handler));

            foreach (var guid in registered.Distinct())
                actuals.Add(notificationMgr.IsRegistered(guid));

            //assert

            Assert.All(registered, r => Assert.False(r == default(Guid)));
            Assert.True(registered.Distinct().Count() == 3);
            Assert.All(actuals, r => r.Equals(true));
        }

        [Fact]
        public void IsRegistered_with_invalid_id()
        {
            //arrange
            INotificationManager<JobId> notificationMgr = null;

            //act
            notificationMgr = new NotificationManager();

            //assert

            Assert.Throws<ArgumentNullException>(() => notificationMgr.IsRegistered(default(Guid)));
        }

        [Fact]
        public void IsRegistered_should_return_false()
        {
            //arrange
            INotificationManager<JobId> notificationMgr = null;

            //act
            notificationMgr = new NotificationManager();
            var actual = notificationMgr.IsRegistered(Guid.NewGuid());

            //assert

            Assert.False(actual);
        }

        [Fact]
        public void RemoveHook_with_registered_id()
        {
            //arrange
            INotificationManager<JobId> notificationMgr = null;

            //act
            notificationMgr = new NotificationManager();

            var jobid = Helper.GetJobId();
            var handler = GetHanlder();

            var id = notificationMgr.RegisterHook(jobid, MessageType.Error, handler);
            notificationMgr.RemoveHook(id);

            var actual = notificationMgr.IsRegistered(id);

            //assert

            Assert.False(actual);
        }

        [Fact]
        public void RemoveHook_with_non_registered_id()
        {
            //arrange
            INotificationManager<JobId> notificationMgr = null;

            //act
            notificationMgr = new NotificationManager();

            var jobid = Helper.GetJobId();
            var handler = GetHanlder();

            var id = notificationMgr.RegisterHook(jobid, MessageType.Error, handler);
            notificationMgr.RemoveHook(Guid.NewGuid());

            var actual = notificationMgr.IsRegistered(id);

            //assert

            Assert.True(actual);
        }

        [Fact]
        public void RemoveHook_with_invalid_id()
        {
            //arrange
            INotificationManager<JobId> notificationMgr = null;

            //act
            notificationMgr = new NotificationManager();

            //assert

            Assert.Throws<ArgumentNullException>(() => notificationMgr.RemoveHook(default(Guid)));
        }

        [Fact]
        public void PushAsync_with_valid_hook()
        {
            //arrange
            INotificationManager<JobId> notificationMgr = null;
            var handler = GetMockHandler();
            var id = Helper.GetJobId();
            notificationMgr = new NotificationManager();

            //act
            var msgInfo = new MessageHook("hook message", "info-unit-testing", MessageType.Info);
            var msgDebug = new MessageHook("hook message", "debug-unit-testing", MessageType.Debug);
            var msgWarning = new MessageHook("hook message", "warning-unit-testing", MessageType.Warning);
            var msgError = new MessageHook("hook message", "error-unit-testing", MessageType.Error);

            notificationMgr.RegisterHook(id, MessageType.All, handler.Object);
            notificationMgr.PushAsync(id, msgInfo);
            notificationMgr.PushAsync(id, msgDebug);
            notificationMgr.PushAsync(id, msgWarning);
            notificationMgr.PushAsync(id, msgError);

            //assert

            handler.Verify(h => h.DoHandle(It.Is<JobId>(p => p.Equals(id)), It.IsAny<MessageHook>()), Times.Exactly(4));
            handler.Verify(h => h.DoHandle(It.Is<JobId>(p => p.Equals(id)), It.Is<MessageHook>(mh => mh.Equals(msgInfo))), Times.Once());
            handler.Verify(h => h.DoHandle(It.Is<JobId>(p => p.Equals(id)), It.Is<MessageHook>(mh => mh.Equals(msgDebug))), Times.Once());
            handler.Verify(h => h.DoHandle(It.Is<JobId>(p => p.Equals(id)), It.Is<MessageHook>(mh => mh.Equals(msgWarning))), Times.Once());
            handler.Verify(h => h.DoHandle(It.Is<JobId>(p => p.Equals(id)), It.Is<MessageHook>(mh => mh.Equals(msgError))), Times.Once());
        }

        [Fact]
        public void PushAsync_with_few_valid_hooks()
        {
            //arrange
            INotificationManager<JobId> notificationMgr = null;
            var handlerInfoOnly = GetMockHandler();
            var handlerErrorOnly = GetMockHandler();
            var id = Helper.GetJobId();
            notificationMgr = new NotificationManager();

            //act
            var msgInfo = new MessageHook("hook message", "info-unit-testing", MessageType.Info);
            var msgError = new MessageHook("hook message", "error-unit-testing", MessageType.Error);

            notificationMgr.RegisterHook(id, MessageType.Info, handlerInfoOnly.Object);
            notificationMgr.RegisterHook(id, MessageType.Error, handlerErrorOnly.Object);
            notificationMgr.PushAsync(id, msgInfo);
            notificationMgr.PushAsync(id, msgError);

            //assert

            handlerInfoOnly.Verify(h => h.DoHandle(It.Is<JobId>(p => p.Equals(id)), It.IsAny<MessageHook>()), Times.Once());
            handlerErrorOnly.Verify(h => h.DoHandle(It.Is<JobId>(p => p.Equals(id)), It.IsAny<MessageHook>()), Times.Once());

            handlerInfoOnly.Verify(h => h.DoHandle(It.Is<JobId>(p => p.Equals(id)), It.Is<MessageHook>(mh => mh.Equals(msgInfo))), Times.Once());
            handlerErrorOnly.Verify(h => h.DoHandle(It.Is<JobId>(p => p.Equals(id)), It.Is<MessageHook>(mh => mh.Equals(msgError))), Times.Once());
        }

        [Fact]
        public void PushAsync_100_messages_for_a_hook()
        {
            //arrange
            INotificationManager<JobId> notificationMgr = null;
            var handlerInfoOnly = GetMockHandler();
            var id = Helper.GetJobId();
            notificationMgr = new NotificationManager();

            //act
            var msgInfo = new MessageHook("hook message", "info-unit-testing", MessageType.Info);
            var msgError = new MessageHook("hook message", "error-unit-testing", MessageType.Error);

            notificationMgr.RegisterHook(id, MessageType.Info, handlerInfoOnly.Object);

            for (int i = 0; i < 100; i++)
            {
                notificationMgr.PushAsync(id, msgInfo);
            }

            //assert

            handlerInfoOnly.Verify(h => h.DoHandle(It.Is<JobId>(p => p.Equals(id)), It.IsAny<MessageHook>()), Times.Exactly(100));
            handlerInfoOnly.Verify(h => h.DoHandle(It.Is<JobId>(p => p.Equals(id)), It.Is<MessageHook>(mh => mh.Equals(msgInfo))), Times.Exactly(100));
        }

        [Fact]
        public void PushAsync_100_messages_for_random_or_invalid_jobId()
        {
            //arrange
            INotificationManager<JobId> notificationMgr = null;
            var handlerInfoOnly = GetMockHandler();
            var id = Helper.GetJobId();
            notificationMgr = new NotificationManager();

            //act
            var msgInfo = new MessageHook("hook message", "info-unit-testing", MessageType.Info);
            var msgError = new MessageHook("hook message", "error-unit-testing", MessageType.Error);

            notificationMgr.RegisterHook(id, MessageType.Info, handlerInfoOnly.Object);
            notificationMgr.PushAsync(id, msgInfo);

            for (int i = 0; i < 100; i++)
            {
                notificationMgr.PushAsync(Helper.GetJobId(), msgError); //random sender key
            }

            Thread.Sleep(2500);

            //assert

            handlerInfoOnly.Verify(h => h.DoHandle(It.Is<JobId>(p => p.Equals(id)), It.IsAny<MessageHook>()), Times.Once());
            handlerInfoOnly.Verify(h => h.DoHandle(It.Is<JobId>(p => p.Equals(id)), It.Is<MessageHook>(mh => mh.Equals(msgInfo))), Times.Once());
        }

        [Fact]
        public void PushAsync_with_invalid_senderkey_and_message()
        {
            //arrange
            INotificationManager<JobId> notificationMgr = null;
            notificationMgr = new NotificationManager();
            var msg = new MessageHook("hook message", "info-unit-testing", MessageType.Info);
            var id = Helper.GetJobId();

            //act
            //assert

            Assert.Throws<ArgumentNullException>(() => notificationMgr.PushAsync(null, null));
            Assert.Throws<ArgumentNullException>(() => notificationMgr.PushAsync(null, msg));
            Assert.Throws<ArgumentNullException>(() => notificationMgr.PushAsync(id, null));
        }

        private IHookHandler<JobId> GetHanlder()
        {
            return GetMockHandler().Object;
        }

        private Mock<IHookHandler<JobId>> GetMockHandler()
        {
            var handler = new Mock<IHookHandler<JobId>>();
            handler.Setup(h => h.Id).Returns(GetId());
            handler.Setup(h => h.Name).Returns(GetName());
            handler.Setup(h => h.DoHandle(It.IsAny<JobId>(), It.IsAny<MessageHook>())).Verifiable();

            return handler;
        }

        private string GetId() => Guid.NewGuid().ToString().Substring(0, 10);

        private string GetName() => "Default-Hanlder";
    }
}