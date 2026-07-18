using System;
using Ink;
using NUnit.Framework;

namespace SAS.DialogueSystem.EditorTools
{
    public class DialogueSessionTests
    {
        [Test]
        public void SessionOwnsLineChoiceAndCompletionTransitions()
        {
            var session = CreateSession(@"
Hello. # speaker:alice # listener:bob
* [Ask about the gate.] # id:choice.ask_gate
    The gate is locked.
    -> END
");

            var lineStep = session.Continue();
            Assert.AreEqual(DialogueStepKind.Line, lineStep.Kind);
            Assert.AreEqual(DialogueSessionState.PresentingLine, session.State);
            Assert.AreEqual("alice", lineStep.Line.CurrentSpeakerId);
            Assert.Throws<InvalidOperationException>(() => session.Continue());

            Assert.IsTrue(session.CompleteLinePresentation(lineStep.Line));
            Assert.AreEqual(DialogueSessionState.PresentingChoices, session.State);
            Assert.AreEqual(DialogueAdvanceAction.None, session.GetAdvanceAction());

            Assert.IsTrue(session.TryChoose(0));
            Assert.AreEqual(DialogueSessionState.Starting, session.State);

            var answerStep = session.Continue();
            Assert.AreEqual(DialogueStepKind.Line, answerStep.Kind);
            Assert.IsTrue(session.CompleteLinePresentation(answerStep.Line));
            Assert.AreEqual(DialogueSessionState.WaitingForAdvance, session.State);
            Assert.AreEqual(DialogueAdvanceAction.ContinueStory, session.GetAdvanceAction());

            var completedStep = session.Continue();
            Assert.AreEqual(DialogueStepKind.Completed, completedStep.Kind);
            Assert.AreEqual(DialogueSessionState.Exiting, session.State);
        }

        [Test]
        public void ChoiceOnlyStoryDoesNotCreateAnEmptyLine()
        {
            var session = CreateSession(@"
* [Continue.]
    Finished.
    -> END
");

            var step = session.Continue();

            Assert.AreEqual(DialogueStepKind.Choices, step.Kind);
            Assert.AreEqual(DialogueSessionState.PresentingChoices, session.State);
            Assert.IsNull(session.CurrentLine);
            Assert.AreEqual(1, session.Story.currentChoices.Count);
        }

        [Test]
        public void StaleLineCompletionCannotAdvanceTheCurrentLine()
        {
            var session = CreateSession(@"
First.
Second.
-> END
");

            var first = session.Continue().Line;
            Assert.IsTrue(session.CompleteLinePresentation(first));
            var second = session.Continue().Line;

            Assert.IsFalse(session.CompleteLinePresentation(first));
            Assert.AreEqual(DialogueSessionState.PresentingLine, session.State);
            Assert.AreSame(second, session.CurrentLine);
            Assert.IsTrue(session.CompleteLinePresentation(second));
        }

        [Test]
        public void InvalidChoiceCannotMutateSessionState()
        {
            var session = CreateSession(@"
* [Continue.]
    -> END
");

            session.Continue();

            Assert.IsFalse(session.TryChoose(-1));
            Assert.IsFalse(session.TryChoose(10));
            Assert.AreEqual(DialogueSessionState.PresentingChoices, session.State);
        }

        private static DialogueSession CreateSession(string source)
        {
            var story = new Compiler(source).Compile();
            Assert.IsNotNull(story, "Ink test source failed to compile.");
            return new DialogueSession(story, DialogueMetadataSchema.Canonical);
        }
    }
}
