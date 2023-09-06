using UnityEngine;

public class Memo : MonoBehaviour
{
#if UNITY_EDITOR
	[SerializeField] string _memo;
	[SerializeField] UnityEditor.MessageType _messageType = UnityEditor.MessageType.Info;

	// memoを編集するEditor拡張
	[UnityEditor.CustomEditor(typeof(Memo))]
	class MemoEditor : UnityEditor.Editor
	{
		[SerializeField] bool _editing = false;

		public override void OnInspectorGUI()
		{
			if (_editing)
			{
				DisplayEditor();
			}
			else
			{
				DisplayMemo();
			}
		}

		void DisplayMemo()
		{
			var memo = (Memo)target;
			UnityEditor.EditorGUILayout.HelpBox(memo._memo, memo._messageType);
			UnityEditor.EditorGUILayout.Space(30);
			if (GUILayout.Button("Edit"))
			{
				_editing = true;
			}
		}

		void DisplayEditor()
		{
			var memo = (Memo)target;
			memo._memo = UnityEditor.EditorGUILayout.TextArea(memo._memo);
			UnityEditor.EditorGUILayout.Space(10);
			memo._messageType = (UnityEditor.MessageType)UnityEditor.EditorGUILayout.EnumPopup("MessageType", memo._messageType);
			UnityEditor.EditorGUILayout.Space(30);
			if (GUILayout.Button("End Edit"))
			{
				_editing = false;
			}
		}
	}
#endif
}
