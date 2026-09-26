#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

// Editor 폴더에 넣는다.
// Project 창에서 Animator Controller를 선택한 뒤 Tools > Animation > Nest Walk & Run Blend Trees 실행.
public static class LocomotionTreeBuilder
{
    // 실제 Animator의 상태 이름과 파라미터 이름에 맞게 바꾼다.
    private const string WALK_STATE = "Walk Blend Tree";
    private const string RUN_STATE = "Run Blend Tree";
    private const string LOCOMOTION_STATE = "Move Blend Tree";
    private const string RUN_PARAMETER = "Run";
    private const int LAYER_INDEX = 0; // Base Layer

    [MenuItem("Tools/Animation/Nest Walk & Run Blend Trees")]
    private static void Build()
    {
        AnimatorController controller = Selection.activeObject as AnimatorController;
        if (controller == null)
        {
            Debug.LogError("Project 창에서 Animator Controller를 선택한 뒤 실행하세요.");
            return;
        }

        AnimatorStateMachine machine = controller.layers[LAYER_INDEX].stateMachine;
        AnimatorState walkState = FindState(machine, WALK_STATE);
        AnimatorState runState = FindState(machine, RUN_STATE);

        if (walkState == null || runState == null
            || walkState.motion is not BlendTree walkTree
            || runState.motion is not BlendTree runTree)
        {
            Debug.LogError($"'{WALK_STATE}', '{RUN_STATE}' 상태를 찾지 못했거나 블렌드 트리가 아닙니다. 상태 이름을 확인하세요.");
            return;
        }

        // 1. 새 상태와 부모 1D 블렌드 트리를 만든다.
        AnimatorState locomotion = controller.CreateBlendTreeInController(LOCOMOTION_STATE, out BlendTree parent, LAYER_INDEX);
        parent.blendType = BlendTreeType.Simple1D;
        parent.blendParameter = RUN_PARAMETER;
        parent.useAutomaticThresholds = false;

        // 2. 기존 트리를 복사하지 않고 그대로 자식으로 연결한다.
        parent.AddChild(walkTree, 0.0f);
        parent.AddChild(runTree, 1.0f);

        // 3. 기존 상태를 지울 때 트리까지 함께 지워지지 않도록 참조를 먼저 끊는다.
        walkState.motion = null;
        runState.motion = null;

        EditorUtility.SetDirty(controller);
        EditorUtility.SetDirty(parent);
        AssetDatabase.SaveAssets();

        Selection.activeObject = locomotion;
        Debug.Log($"'{LOCOMOTION_STATE}' 상태를 만들었습니다. 기존 '{WALK_STATE}', '{RUN_STATE}'로 향하던 전환을 '{LOCOMOTION_STATE}'로 옮긴 뒤 두 상태를 지우세요.");
    }

    private static AnimatorState FindState(AnimatorStateMachine machine, string name)
    {
        foreach (ChildAnimatorState child in machine.states)
        {
            if (child.state.name == name)
            {
                return child.state;
            }
        }
        return null;
    }
}
#endif