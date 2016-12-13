using System.Collections.Generic;
using Chequered.State;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Chequered.Test
{
    [TestClass]
    public class StateTest
    {
        [TestMethod]
        public void StateManager_Constructed_CallsEnterOnStartState()
        {
            var firstMock = new Mock<IState>();
            var secondMock = new Mock<IState>();

            var stateManager = new StateManager<string, IState>(
                new Dictionary<string, IState>
                {
                    {"first", firstMock.Object},
                    {"second", secondMock.Object}
                },
                "first");

            firstMock.Verify(o => o.Enter(), Times.Once);
            secondMock.Verify(o => o.Enter(), Times.Never);
        }

        [TestMethod]
        public void StateManagerChangeState_Called_CallsEnterAndLeave()
        {
            var firstMock = new Mock<IState>();
            var secondMock = new Mock<IState>();
            var stateManager = new StateManager<string, IState>(
                new Dictionary<string, IState>
                {
                    {"first", firstMock.Object},
                    {"second", secondMock.Object}
                },
                "first");

            stateManager.ChangeState("second");

            firstMock.Verify(o => o.Leave(), Times.Once);
            secondMock.Verify(o => o.Enter(), Times.Once);
        }

        [TestMethod]
        public void StateManagerPushPopState_CalledBoth_ChangesBackToSameState()
        {
            var firstMock = new Mock<IState>();
            var secondMock = new Mock<IState>();
            var stateManager = new StateManager<string, IState>(
                new Dictionary<string, IState>
                {
                    {"first", firstMock.Object},
                    {"second", secondMock.Object}
                },
                "first");

            stateManager.PushState("second");
            stateManager.PopState();

            firstMock.Verify(o => o.Enter(), Times.Exactly(2)); //on construction and on pop
            firstMock.Verify(o => o.Leave(), Times.Once);
            secondMock.Verify(o => o.Enter(), Times.Once);
            secondMock.Verify(o => o.Leave(), Times.Once);
        }
    }
}