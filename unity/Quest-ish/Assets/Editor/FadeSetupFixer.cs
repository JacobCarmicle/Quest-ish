#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.UI;

public static class FadeSetupFixer
{
    private const string TriggerName = "FadeOut";
    private const string FadeInStateName = "FadeIn";
    private const string FadeOutStateName = "FadeOut";

    [MenuItem("Tools/Scene Fade/Fix Animator Setup (FadeIn -> FadeOut)")]
    public static void FixFadeAnimatorSetup()
    {
        // Prefer selected object; otherwise try to find "FadeImage"
        GameObject fadeImageGO = Selection.activeGameObject;

        if (fadeImageGO == null || fadeImageGO.GetComponent<Image>() == null)
        {
            var found = GameObject.Find("FadeImage");
            if (found != null) fadeImageGO = found;
        }

        if (fadeImageGO == null)
        {
            Debug.LogError("❌ Could not find FadeImage. Select your FadeImage object in the Hierarchy and run again.");
            return;
        }

        var animator = fadeImageGO.GetComponent<Animator>();
        if (animator == null || animator.runtimeAnimatorController == null)
        {
            Debug.LogError("❌ FadeImage has no Animator or no Controller assigned. Assign your FadeController to FadeImage and run again.");
            return;
        }

        var controller = animator.runtimeAnimatorController as AnimatorController;
        if (controller == null)
        {
            Debug.LogError("❌ The assigned controller is not an AnimatorController (expected .controller).");
            return;
        }

        // Recommended: make FadeImage not block clicks
        var img = fadeImageGO.GetComponent<Image>();
        if (img != null)
        {
            img.raycastTarget = false;
            EditorUtility.SetDirty(img);
        }

        // Ensure Trigger parameter exists
        EnsureTriggerParameter(controller, TriggerName);

        // Use base layer
        var sm = controller.layers[0].stateMachine;

        // Find states by name
        var fadeInState = FindState(sm, FadeInStateName);
        var fadeOutState = FindState(sm, FadeOutStateName);

        if (fadeInState == null || fadeOutState == null)
        {
            Debug.LogError($"❌ Could not find required states '{FadeInStateName}' and/or '{FadeOutStateName}' in the controller. Create them (or rename) and run again.");
            return;
        }

        // Set FadeIn as default
        sm.defaultState = fadeInState;

        // Remove WRONG transition FadeOut -> FadeIn (and any other transitions from FadeOut to FadeIn)
        RemoveTransitions(fadeOutState, fadeInState);

        // Ensure correct transition FadeIn -> FadeOut on Trigger
        EnsureTransition(
            from: fadeInState,
            to: fadeOutState,
            triggerName: TriggerName,
            allowDuplicate: false
        );

        // Optional: allow re-triggering FadeOut (FadeOut -> FadeOut)
        EnsureTransition(
            from: fadeOutState,
            to: fadeOutState,
            triggerName: TriggerName,
            allowDuplicate: false
        );

        EditorUtility.SetDirty(controller);
        AssetDatabase.SaveAssets();

        Debug.Log("✅ Fade Animator fixed: Default=FadeIn, Transition=FadeIn -> FadeOut on Trigger 'FadeOut', removed FadeOut -> FadeIn, RaycastTarget disabled.");
    }

    private static void EnsureTriggerParameter(AnimatorController controller, string triggerName)
    {
        foreach (var p in controller.parameters)
            if (p.name == triggerName && p.type == AnimatorControllerParameterType.Trigger)
                return;

        controller.AddParameter(triggerName, AnimatorControllerParameterType.Trigger);
    }

    private static AnimatorState FindState(AnimatorStateMachine sm, string stateName)
    {
        foreach (var child in sm.states)
            if (child.state != null && child.state.name == stateName)
                return child.state;

        return null;
    }

    private static void RemoveTransitions(AnimatorState from, AnimatorState to)
    {
        var transitions = from.transitions;
        for (int i = transitions.Length - 1; i >= 0; i--)
        {
            if (transitions[i].destinationState == to)
                from.RemoveTransition(transitions[i]);
        }
    }

    private static void EnsureTransition(AnimatorState from, AnimatorState to, string triggerName, bool allowDuplicate)
    {
        // Check if already exists with correct condition
        foreach (var t in from.transitions)
        {
            if (t.destinationState != to) continue;

            bool hasCorrectTrigger = false;
            foreach (var c in t.conditions)
            {
                if (c.mode == AnimatorConditionMode.If && c.parameter == triggerName)
                {
                    hasCorrectTrigger = true;
                    break;
                }
            }

            if (hasCorrectTrigger && !allowDuplicate)
            {
                // Ensure settings are correct
                t.hasExitTime = false;
                t.hasFixedDuration = true;
                t.duration = 0f;
                return;
            }
        }

        // Create it
        var trans = from.AddTransition(to);
        trans.hasExitTime = false;
        trans.hasFixedDuration = true;
        trans.duration = 0f;
        trans.AddCondition(AnimatorConditionMode.If, 0f, triggerName);
    }
}
#endif
