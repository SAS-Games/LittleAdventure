using SAS.StringTest;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class CharacterAnimatorDriver : MonoBehaviour
{
    [SerializeField] AnimationActionDatabase database;
    [SerializeField] float bufferTime = 0.15f;

    Animator animator;

    AnimationActionConfig currentAction;
    float lockTimer;
    bool actionActive;

    static readonly int SpeedHash = Animator.StringToHash("Speed");
    static readonly int VerticalVelHash = Animator.StringToHash("VerticalVelocity");
    static readonly int GroundedHash = Animator.StringToHash("IsGrounded");

    // ------------------------------------------------
    // BUFFER
    // ------------------------------------------------
    struct BufferedAction
    {
        public string id;
        public AnimationCategory category;
        public float expireTime;
    }

    BufferedAction? buffered;

    // ------------------------------------------------
    void Awake()
    {
        animator = GetComponent<Animator>();
        database.Initialize();
    }

    void Update()
    {
        UpdateTimers();
        CheckActionCompletion();
        ProcessBufferedAction();
    }

    // ------------------------------------------------
    // LOCOMOTION (CALL FROM CHARACTER CONTROLLER)
    // ------------------------------------------------
    public void UpdateLocomotion(float speed, float verticalVelocity, bool grounded)
    {
        // ✅ ONLY block locomotion for hard override states
        if (actionActive &&
            currentAction != null &&
            currentAction.category == AnimationCategory.Override)
            return;

        animator.SetFloat(SpeedHash, speed);
        animator.SetFloat(VerticalVelHash, verticalVelocity);
        animator.SetBool(GroundedHash, grounded);
    }

    // ------------------------------------------------
    void UpdateTimers()
    {
        if (lockTimer > 0f)
            lockTimer -= Time.deltaTime;
    }

    // ------------------------------------------------
    // REQUEST (FSM CALLS THIS)
    // ------------------------------------------------
    public void RequestAction(string id)
    {
        var config = database.Get(id);

        if (buffered == null || CanReplaceBuffered(buffered.Value.category, config.category))
        {
            buffered = new BufferedAction
            {
                id = id,
                category = config.category,
                expireTime = Time.time + bufferTime
            };
        }
    }

    // ------------------------------------------------
    // BUFFER PROCESS
    // ------------------------------------------------
    void ProcessBufferedAction()
    {
        if (buffered == null)
            return;

        var b = buffered.Value;

        if (Time.time > b.expireTime)
        {
            buffered = null;
            return;
        }

        if (PlayAction(b.id))
            buffered = null;
    }

    // ------------------------------------------------
    // PLAY ACTION
    // ------------------------------------------------
    bool PlayAction(string id)
    {
        var next = database.Get(id);

        // lock check
        if (lockTimer > 0f &&
            currentAction != null &&
            !currentAction.canBeInterrupted)
            return false;

        // category rule
        if (currentAction != null &&
            !CanTransition(currentAction.category, next.category))
            return false;

        currentAction = next;
        actionActive = true;
        lockTimer = next.lockDuration;

        animator.CrossFade(next.animatorStateName, next.blendTime, next.layer);

        return true;
    }

    // ------------------------------------------------
    // AUTO RELEASE ACTION
    // ------------------------------------------------
    void CheckActionCompletion()
    {
        if (!actionActive || currentAction == null)
            return;

        if (currentAction.isLooping)
            return;

        var info = animator.GetCurrentAnimatorStateInfo(currentAction.layer);

        if (!animator.IsInTransition(currentAction.layer) &&
            info.normalizedTime >= 1f)
        {
            actionActive = false;
            currentAction = null;
        }
    }

    // ------------------------------------------------
    // CATEGORY RULES
    // ------------------------------------------------
    bool CanTransition(AnimationCategory current, AnimationCategory next)
    {
        if (current == AnimationCategory.Override)
            return next == AnimationCategory.Override;

        if (current == AnimationCategory.Damage)
            return next == AnimationCategory.Damage ||
                   next == AnimationCategory.Override;

        if (current == AnimationCategory.Combat)
            return next != AnimationCategory.Locomotion;

        if (current == AnimationCategory.Ability)
            return next != AnimationCategory.Locomotion;

        return true;
    }

    bool CanReplaceBuffered(AnimationCategory current, AnimationCategory next)
    {
        if (next == AnimationCategory.Override) return true;
        if (next == AnimationCategory.Damage) return true;

        if (next == AnimationCategory.Combat)
            return current != AnimationCategory.Damage;

        if (next == AnimationCategory.Ability)
            return current == AnimationCategory.Locomotion;

        return false;
    }

    // ------------------------------------------------
    // DEBUG
    // ------------------------------------------------
    public string Debug_CurrentAction => currentAction != null ? currentAction.actionId.ToString() : "None";

    public string Debug_Category => currentAction != null ? currentAction.category.ToString() : "None";

    public float Debug_LockTimer => lockTimer;

    public string Debug_BufferedAction => buffered?.id.ToString() ?? "None";

    public string Debug_AnimatorState
    {
        get
        {
            var clips = animator.GetCurrentAnimatorClipInfo(0);
            if (clips.Length == 0) return "None";
            return clips[0].clip.name;
        }
    }
}