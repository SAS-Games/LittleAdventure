using System;
using System.Linq;
using NUnit.Framework;

namespace SAS.DialogueSystem.EditorTools
{
    /// <summary>
    /// Protects the canonical Inky-to-Unity metadata contract.
    /// </summary>
    public class DialogueMetadataParserTests
    {
        [Test]
        public void ParserPreservesCustomTagsAndReportsDuplicateScalarFields()
        {
            var line = DialogueMetadataParser.ParseLine(
                "Hello.",
                new[] { "id:first", "mood:calm", "id:second" },
                DialogueMetadataSchema.Canonical);

            Assert.AreEqual("second", line.LineId);
            Assert.AreEqual("calm", line.GetTagValues("mood")[0]);
            Assert.IsTrue(line.Diagnostics.Any(item => item.Code == "duplicate-field"));
        }

        [Test]
        public void CanonicalParticipantFieldsAreOrderIndependent()
        {
            var line = DialogueMetadataParser.ParseLine(
                "We should leave.",
                new[]
                {
                    "portrait:happy",
                    "listener_portrait:concerned",
                    "animation:TalkHappy",
                    "listener:bob",
                    "speaker:alice",
                    "listener_animation:Listen",
                    "id:line.leave",
                    "locale:dialogue.leave",
                    "audio:alice_leave_01",
                    "mood:urgent"
                },
                DialogueMetadataSchema.Canonical);

            Assert.AreEqual("line.leave", line.LineId);
            Assert.AreEqual("dialogue.leave", line.Locale);
            Assert.AreEqual("alice_leave_01", line.AudioInfoId);
            Assert.AreEqual("alice", line.CurrentSpeakerId);
            Assert.AreEqual("bob", line.ListenerId);
            Assert.AreEqual("urgent", line.GetTagValues("mood")[0]);

            Assert.IsTrue(line.TryGetParticipant("speaker", out var speaker));
            Assert.AreEqual("alice", speaker.CharacterId);
            Assert.AreEqual("happy", speaker.PortraitKey);
            Assert.AreEqual("TalkHappy", speaker.AnimationKey);

            Assert.IsTrue(line.TryGetParticipant("listener", out var listener));
            Assert.AreEqual("bob", listener.CharacterId);
            Assert.AreEqual("concerned", listener.PortraitKey);
            Assert.AreEqual("Listen", listener.AnimationKey);
        }

        [Test]
        public void MonologueAndNarrationDoNotRequireAListener()
        {
            var monologue = DialogueMetadataParser.ParseLine(
                "I need to think.",
                new[] { "speaker:alice", "portrait:concerned" },
                DialogueMetadataSchema.Canonical);
            var narration = DialogueMetadataParser.ParseLine(
                "The wind moved through the hall.",
                new[] { "mood:quiet" },
                DialogueMetadataSchema.Canonical);

            Assert.AreEqual("alice", monologue.CurrentSpeakerId);
            Assert.AreEqual(string.Empty, monologue.ListenerId);
            Assert.AreEqual(1, monologue.Participants.Count);

            Assert.AreEqual(string.Empty, narration.CurrentSpeakerId);
            Assert.AreEqual(0, narration.Participants.Count);
            Assert.IsTrue(narration.HasTag("mood"));
        }

        [Test]
        public void CustomParticipantRolesUseMatchingRoleFields()
        {
            var line = DialogueMetadataParser.ParseLine(
                "What did you see?",
                new[]
                {
                    "participant.interviewer:maya",
                    "participant.interviewer.name:Detective Maya",
                    "participant.interviewer.portrait:focused",
                    "participant.interviewer.animation:Question"
                },
                DialogueMetadataSchema.Canonical);

            Assert.IsTrue(line.TryGetParticipant("interviewer", out var participant));
            Assert.AreEqual("maya", participant.CharacterId);
            Assert.AreEqual("Detective Maya", participant.DisplayName);
            Assert.AreEqual("focused", participant.PortraitKey);
            Assert.AreEqual("Question", participant.AnimationKey);
            Assert.AreEqual(string.Empty, line.CurrentSpeakerId);
        }

        [Test]
        public void GenericParticipantDetailsRequireAnExplicitRoleId()
        {
            var line = DialogueMetadataParser.ParseLine(
                "Malformed line.",
                new[] { "participant.interviewer.portrait:focused" },
                DialogueMetadataSchema.Canonical);

            Assert.IsTrue(line.HasErrors);
            Assert.IsTrue(line.Diagnostics.Any(item => item.Code == "participant-id-missing"));
            Assert.AreEqual(0, line.Participants.Count);
        }

        [Test]
        public void StandardParticipantDetailsRequireTheirRoleId()
        {
            var line = DialogueMetadataParser.ParseLine(
                "Malformed line.",
                new[] { "portrait:focused", "listener_animation:Listen" },
                DialogueMetadataSchema.Canonical);

            Assert.IsTrue(line.HasErrors);
            Assert.AreEqual(
                2,
                line.Diagnostics.Count(item => item.Code == "participant-id-missing"));
            Assert.AreEqual(0, line.Participants.Count);
        }

        [Test]
        public void LegacyCompoundSpeakerMetadataIsRejected()
        {
            var line = DialogueMetadataParser.ParseLine(
                "Legacy line.",
                new[] { "speaker:id::npc, name::KAIROS, anim::Talk" },
                DialogueMetadataSchema.Canonical);

            Assert.IsTrue(line.HasErrors);
            Assert.IsTrue(line.Diagnostics.Any(item => item.Code == "invalid-participant-id"));
            Assert.AreEqual(0, line.Participants.Count);
        }

        [Test]
        public void ChoiceMetadataAndCustomTagsRemainAccessible()
        {
            var choice = DialogueMetadataParser.ParseLine(
                "Ask about the gate",
                new[]
                {
                    "id:choice.ask_gate",
                    "locale:choice.ask_gate",
                    "analytics_event:gate.ask"
                },
                DialogueMetadataSchema.Canonical);

            Assert.AreEqual("choice.ask_gate", choice.LineId);
            Assert.AreEqual("choice.ask_gate", choice.Locale);
            Assert.AreEqual("gate.ask", choice.GetTagValues("analytics_event")[0]);
            Assert.AreEqual(0, choice.Participants.Count);
        }

        [Test]
        public void ConfigurableProfileMapsProjectTagNamesToRuntimeSemantics()
        {
            var schema = new DialogueMetadataSchema(
                "line_id",
                "loc_key",
                "ui_layout",
                "voice",
                "speaker",
                "listener",
                new[]
                {
                    new DialogueParticipantTagSchema(
                        "speaker",
                        "actor",
                        "actor_name",
                        "face",
                        "motion"),
                    new DialogueParticipantTagSchema(
                        "listener",
                        "target",
                        "target_name",
                        "target_face",
                        "target_motion")
                },
                new DialogueGenericParticipantTagSchema(
                    true,
                    "cast.",
                    ".display",
                    ".face",
                    ".motion"));

            var line = DialogueMetadataParser.ParseLine(
                "Welcome.",
                new[]
                {
                    "line_id:greeting.01",
                    "loc_key:dialogue.greeting.01",
                    "ui_layout:left",
                    "voice:mira_default",
                    "actor:mira",
                    "actor_name:Captain Mira",
                    "face:happy",
                    "motion:Talk",
                    "target:player",
                    "mood:friendly"
                },
                schema);

            Assert.AreEqual("greeting.01", line.LineId);
            Assert.AreEqual("dialogue.greeting.01", line.Locale);
            Assert.AreEqual("left", line.LayoutAnim);
            Assert.AreEqual("mira_default", line.AudioInfoId);
            Assert.AreEqual("mira", line.CurrentSpeakerId);
            Assert.AreEqual("player", line.ListenerId);
            Assert.AreEqual("friendly", line.GetTagValues("mood")[0]);
            Assert.IsTrue(line.TryGetParticipant("speaker", out var speaker));
            Assert.AreEqual("Captain Mira", speaker.DisplayName);
            Assert.AreEqual("happy", speaker.PortraitKey);
            Assert.AreEqual("Talk", speaker.AnimationKey);
        }

        [Test]
        public void ConfigurableGenericParticipantPatternSupportsProjectNamespaces()
        {
            var schema = new DialogueMetadataSchema(
                null,
                null,
                null,
                null,
                "lead",
                null,
                Array.Empty<DialogueParticipantTagSchema>(),
                new DialogueGenericParticipantTagSchema(
                    true,
                    "cast.",
                    ".display",
                    ".face",
                    ".motion"));

            var line = DialogueMetadataParser.ParseLine(
                "What happened?",
                new[]
                {
                    "cast.lead:maya",
                    "cast.lead.display:Detective Maya",
                    "cast.lead.face:focused",
                    "cast.lead.motion:Question"
                },
                schema);

            Assert.AreEqual("maya", line.CurrentSpeakerId);
            Assert.IsTrue(line.TryGetParticipant("lead", out var participant));
            Assert.AreEqual("Detective Maya", participant.DisplayName);
            Assert.AreEqual("focused", participant.PortraitKey);
            Assert.AreEqual("Question", participant.AnimationKey);
        }

        [Test]
        public void MetadataSchemaRejectsConflictingTagBindings()
        {
            var exception = Assert.Throws<ArgumentException>(() => new DialogueMetadataSchema(
                "key",
                "key",
                null,
                null,
                "speaker",
                null,
                Array.Empty<DialogueParticipantTagSchema>()));

            StringAssert.Contains("assigned to both", exception.Message);
        }

        [Test]
        public void StoryWriterImporterReadsCanonicalSpeakerFields()
        {
            InkTagParser.ParseTextAndTags(
                "Hello. # speaker:alice # speaker_name:Alice # portrait:happy # animation:Talk # listener:bob",
                out var text,
                out var tags);

            Assert.AreEqual("Hello.", text);
            Assert.IsTrue(tags.useSpeaker);
            Assert.AreEqual("alice", tags.speakerId);
            Assert.AreEqual("Alice", tags.speakerName);
            Assert.AreEqual("happy", tags.portraitKey);
            Assert.AreEqual("Talk", tags.speakerAnimation);
            Assert.IsTrue(tags.customTags.Exists(tag => tag.key == "listener" && tag.value == "bob"));
            Assert.AreEqual(
                " #speaker:alice #speaker_name:Alice #portrait:happy #animation:Talk #listener:bob",
                InkTagWriter.BuildTagSuffix(tags));
        }
    }
}
