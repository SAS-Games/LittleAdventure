using System;
using Ink;
using NUnit.Framework;

namespace SAS.DialogueSystem.Tests
{
    public class DialogueSessionTests
    {
        [Test]
        public void SessionOwnsLineChoiceAndCompletionTransitions()
        {
            var session = CreateSession(@"
Hello. # speaker:alice # listener:bob
* [Ask about the gate. # id:choice.ask_gate]
    The gate is locked.
    -> END
");

            var line = session.Continue();
            Assert.AreEqual(DialogueStepKind.Line, line.Kind);
            Assert.Throws<InvalidOperationException>(() => session.Continue());
            Assert.IsTrue(session.CompleteLinePresentation(line.Line));
            Assert.AreEqual(DialogueSessionState.PresentingChoices, session.State);
            Assert.IsTrue(session.TryChoose(0));

            var answer = session.Continue();
            Assert.AreEqual(DialogueStepKind.Line, answer.Kind);
            Assert.IsTrue(session.CompleteLinePresentation(answer.Line));
            Assert.AreEqual(DialogueAdvanceAction.ContinueStory, session.GetAdvanceAction());
            Assert.AreEqual(DialogueStepKind.Completed, session.Continue().Kind);
            Assert.AreEqual(DialogueSessionState.Exiting, session.State);
        }

        [Test]
        public void StaleLineCannotCompleteTheCurrentLine()
        {
            var session = CreateSession("First.\nSecond.\n-> END");
            var first = session.Continue().Line;
            Assert.IsTrue(session.CompleteLinePresentation(first));
            var second = session.Continue().Line;
            Assert.IsFalse(session.CompleteLinePresentation(first));
            Assert.AreSame(second, session.CurrentLine);
        }

        private static DialogueSession CreateSession(string source)
        {
            var story = new Compiler(source).Compile();
            Assert.IsNotNull(story);
            return new DialogueSession(story, DialogueMetadataSchema.Canonical);
        }
    }
}