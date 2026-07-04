#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

public class SwordSkillSetupEditor : EditorWindow
{
    [MenuItem("Tools/Sword Skill/Setup Animations")]
    public static void ShowWindow()
    {
        GetWindow<SwordSkillSetupEditor>("Sword Skill Animator Setup");
    }

    private AnimatorController targetController;
    private AnimationClip chargeClip;
    private AnimationClip fireClip;

    private void OnGUI()
    {
        GUILayout.Label("Auto-Setup Animator cho Skill Lửa", EditorStyles.boldLabel);
        
        targetController = (AnimatorController)EditorGUILayout.ObjectField("Animator Controller", targetController, typeof(AnimatorController), false);
        chargeClip = (AnimationClip)EditorGUILayout.ObjectField("Animation Charge", chargeClip, typeof(AnimationClip), false);
        fireClip = (AnimationClip)EditorGUILayout.ObjectField("Animation Fire", fireClip, typeof(AnimationClip), false);

        if (GUILayout.Button("Setup Animator Parameters & States"))
        {
            if (targetController == null)
            {
                EditorUtility.DisplayDialog("Lỗi", "Vui lòng chọn một Animator Controller (VD: Warrior_Blue.controller).", "OK");
                return;
            }

            SetupAnimator(targetController, chargeClip, fireClip);
            EditorUtility.DisplayDialog("Thành công", $"Đã setup thành công cho {targetController.name}!", "OK");
        }
    }

    private static void SetupAnimator(AnimatorController controller, AnimationClip charge, AnimationClip fire)
    {
        // 1. Thêm Parameters
        AddParameter(controller, "SkillCharge", AnimatorControllerParameterType.Trigger);
        AddParameter(controller, "SkillFire", AnimatorControllerParameterType.Trigger);
        AddParameter(controller, "IsChargingSkill", AnimatorControllerParameterType.Bool);

        // 2. Lấy Layer 0
        var rootStateMachine = controller.layers[0].stateMachine;

        // 3. Tạo/Tìm States
        AnimatorState chargeState = null;
        AnimatorState fireState = null;
        
        foreach (var state in rootStateMachine.states)
        {
            if (state.state.name == "SkillCharge") chargeState = state.state;
            if (state.state.name == "SkillFire") fireState = state.state;
        }

        if (chargeState == null)
        {
            chargeState = rootStateMachine.AddState("SkillCharge");
            // Mặc định ở giữa
            chargeState.name = "SkillCharge";
        }
        if (charge != null) chargeState.motion = charge;

        if (fireState == null)
        {
            fireState = rootStateMachine.AddState("SkillFire");
        }
        if (fire != null) fireState.motion = fire;

        // 4. Tạo Transitions
        // AnyState -> SkillCharge (khi gọi trigger SkillCharge)
        bool hasChargeTransition = false;
        foreach (var trans in rootStateMachine.anyStateTransitions)
        {
            if (trans.destinationState == chargeState) hasChargeTransition = true;
        }
        if (!hasChargeTransition)
        {
            var transition = rootStateMachine.AddAnyStateTransition(chargeState);
            transition.AddCondition(AnimatorConditionMode.If, 0, "SkillCharge");
            transition.duration = 0.1f;
        }

        // SkillCharge -> SkillFire (khi gọi trigger SkillFire)
        bool hasFireTransition = false;
        foreach (var trans in chargeState.transitions)
        {
            if (trans.destinationState == fireState) hasFireTransition = true;
        }
        if (!hasFireTransition)
        {
            var transition = chargeState.AddTransition(fireState);
            transition.AddCondition(AnimatorConditionMode.If, 0, "SkillFire");
            transition.duration = 0.1f;
        }

        // SkillFire -> Exit (sau khi kết thúc animation)
        bool hasExitTransition = false;
        foreach (var trans in fireState.transitions)
        {
            if (trans.isExit) hasExitTransition = true;
        }
        if (!hasExitTransition)
        {
            var transition = fireState.AddExitTransition();
            transition.hasExitTime = true;
            transition.exitTime = 0.8f; // Chờ chạy 80% animation rồi thoát về Idle
            transition.duration = 0.1f;
        }

        EditorUtility.SetDirty(controller);
        AssetDatabase.SaveAssets();
    }

    private static void AddParameter(AnimatorController controller, string paramName, AnimatorControllerParameterType type)
    {
        foreach (var param in controller.parameters)
        {
            if (param.name == paramName) return; // Đã tồn tại
        }
        controller.AddParameter(paramName, type);
    }
}
#endif
