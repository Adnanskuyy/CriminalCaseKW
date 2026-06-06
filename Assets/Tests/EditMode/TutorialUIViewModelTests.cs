#nullable enable
using System;
using CriminalCase2.ViewModels;
using NUnit.Framework;

namespace CriminalCase2.Tests
{
    public class TutorialUIViewModelTests
    {
        [Test]
        public void RequestClose_RaisesCloseRequested()
        {
            var vm = new TutorialUIViewModel();
            int calls = 0;
            vm.CloseRequested += () => calls++;

            vm.RequestClose();

            Assert.AreEqual(1, calls);
        }

        [Test]
        public void RequestReplayVideo_RaisesReplayVideoRequested()
        {
            var vm = new TutorialUIViewModel();
            int calls = 0;
            vm.ReplayVideoRequested += () => calls++;

            vm.RequestReplayVideo();

            Assert.AreEqual(1, calls);
        }

        [Test]
        public void RequestClose_DoesNotRaiseReplayVideoRequested()
        {
            var vm = new TutorialUIViewModel();
            int replayCalls = 0;
            vm.ReplayVideoRequested += () => replayCalls++;

            vm.RequestClose();

            Assert.AreEqual(0, replayCalls);
        }

        [Test]
        public void RequestReplayVideo_DoesNotRaiseCloseRequested()
        {
            var vm = new TutorialUIViewModel();
            int closeCalls = 0;
            vm.CloseRequested += () => closeCalls++;

            vm.RequestReplayVideo();

            Assert.AreEqual(0, closeCalls);
        }

        [Test]
        public void MultipleSubscriptions_AllReceiveEvents()
        {
            var vm = new TutorialUIViewModel();
            int calls1 = 0, calls2 = 0;
            vm.CloseRequested += () => calls1++;
            vm.CloseRequested += () => calls2++;

            vm.RequestClose();

            Assert.AreEqual(1, calls1);
            Assert.AreEqual(1, calls2);
        }

        [Test]
        public void Dispose_IsIdempotent()
        {
            var vm = new TutorialUIViewModel();

            Assert.DoesNotThrow(() => vm.Dispose());
            Assert.DoesNotThrow(() => vm.Dispose());
        }

        [Test]
        public void RequestClose_AfterDispose_ThrowsObjectDisposedException()
        {
            var vm = new TutorialUIViewModel();
            vm.Dispose();

            Assert.Throws<ObjectDisposedException>(() => vm.RequestClose());
        }

        [Test]
        public void RequestReplayVideo_AfterDispose_ThrowsObjectDisposedException()
        {
            var vm = new TutorialUIViewModel();
            vm.Dispose();

            Assert.Throws<ObjectDisposedException>(() => vm.RequestReplayVideo());
        }
    }
}
