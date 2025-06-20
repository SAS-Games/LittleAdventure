using UnityEngine;
using SAS.Utilities;

public class DialogueSystem : MonoBehaviour
{
    [SerializeField] private DialogueConfig _config;
    [SerializeField] private TextAsset _inkJSON;
    [SerializeField] private SerializableInterface<IDialogueWidget> _view;
    [SerializeField] private SerializableInterface<IInkMetaParser> metaParser;

    private DialogueModel _model;
    public DialoguePresenter Presenter{get; private set;}

    private void Start()
    {
        _model = new DialogueModel(_config, metaParser.Value);
        Presenter = new DialoguePresenter(_model, _view.Value);
        StartDialogue();
    }

    public void StartDialogue() => Presenter.StartDialogue(_inkJSON);
    public void OnContinueClicked() => Presenter.ContinueDialogue();
}