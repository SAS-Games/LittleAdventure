using Ink;
using Ink.Runtime;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace SAS.DialogueSystem.EditorTools
{
    internal sealed class DialogueStoryPreviewPlayer
    {
        private readonly List<DialogueStoryPreviewLine> _history = new List<DialogueStoryPreviewLine>();
        private readonly List<string> _compileMessages = new List<string>();
        private Story _story;

        public IReadOnlyList<DialogueStoryPreviewLine> History => _history;
        public IReadOnlyList<string> CompileMessages => _compileMessages;
        public IReadOnlyList<Choice> Choices => _story?.currentChoices ?? EmptyChoices;
        public bool IsPlaying => _story != null;
        public bool CanContinue => _story != null && _story.canContinue;

        private static readonly List<Choice> EmptyChoices = new List<Choice>();

        public bool Play(DialogueStoryDraft draft, out string error)
        {
            Stop();
            error = string.Empty;

            if (draft == null)
            {
                error = "No draft selected.";
                return false;
            }

            DialogueStoryValidator.EnsureDraftShape(draft);
            var hierarchyErrors = DialogueStoryValidator.GetSectionHierarchyErrors(draft);
            if (hierarchyErrors.Count > 0)
            {
                error = string.Join("\n", hierarchyErrors);
                return false;
            }

            var inkText = DialogueInkBuilder.Build(draft);
            var sourceFolder = DialogueStoryAssetService.GetOutputFolderPath(draft);
            var absoluteSourceFolder = System.IO.Path.GetFullPath(sourceFolder);

            var compiler = new Compiler(inkText, new Compiler.Options
            {
                sourceFilename = $"{InkSanitizer.SanitizeFileName(draft.outputFileName)}.ink",
                fileHandler = new PreviewFileHandler(absoluteSourceFolder),
                errorHandler = OnCompilerMessage
            });

            try
            {
                _story = compiler.Compile();
            }
            catch (Exception exception)
            {
                Stop();
                error = exception.Message;
                return false;
            }

            if (_story == null)
            {
                error = _compileMessages.Count > 0 ? string.Join("\n", _compileMessages) : "Ink compile failed.";
                return false;
            }

            BindPreviewExternalFunctions(_story, inkText);
            Continue();
            return true;
        }

        public void Stop()
        {
            _story = null;
            _history.Clear();
            _compileMessages.Clear();
        }

        public void Continue()
        {
            if (_story == null)
                return;

            while (_story.canContinue)
            {
                var text = _story.Continue().Trim();
                var tags = _story.currentTags == null
                    ? new List<string>()
                    : new List<string>(_story.currentTags);

                if (!string.IsNullOrWhiteSpace(text) || tags.Count > 0)
                {
                    _history.Add(new DialogueStoryPreviewLine
                    {
                        text = text,
                        tags = tags
                    });
                }

                if (_story.currentChoices.Count > 0)
                    break;
            }
        }

        public void Choose(int choiceIndex)
        {
            if (_story == null)
                return;

            if (choiceIndex < 0 || choiceIndex >= _story.currentChoices.Count)
                return;

            var choice = _story.currentChoices[choiceIndex];
            _history.Add(new DialogueStoryPreviewLine
            {
                text = $"> {choice.text}",
                tags = new List<string>(),
                isChoice = true
            });

            _story.ChooseChoiceIndex(choiceIndex);
            Continue();
        }

        private void OnCompilerMessage(string message, ErrorType errorType)
        {
            _compileMessages.Add($"{errorType}: {message}");
        }

        private static void BindPreviewExternalFunctions(Story story, string inkText)
        {
            foreach (var externalMethod in GetExternalMethodNames(inkText))
                story.BindExternalFunctionGeneral(externalMethod, args => 0);
        }

        private static IEnumerable<string> GetExternalMethodNames(string inkText)
        {
            var names = new HashSet<string>();
            var matches = Regex.Matches(inkText, @"^\s*EXTERNAL\s+([A-Za-z_][A-Za-z0-9_]*)\s*\(", RegexOptions.Multiline);
            foreach (Match match in matches)
                names.Add(match.Groups[1].Value);

            return names;
        }

        private sealed class PreviewFileHandler : IFileHandler
        {
            private readonly string _rootDirectory;

            public PreviewFileHandler(string rootDirectory)
            {
                _rootDirectory = rootDirectory;
            }

            public string ResolveInkFilename(string includeName)
            {
                return new FileInfo(System.IO.Path.Combine(_rootDirectory, includeName).Replace('\\', '/')).FullName;
            }

            public string LoadInkFileContents(string fullFilename)
            {
                return File.ReadAllText(fullFilename);
            }
        }
    }

    internal sealed class DialogueStoryPreviewLine
    {
        public string text = string.Empty;
        public List<string> tags = new List<string>();
        public bool isChoice;
    }
}
