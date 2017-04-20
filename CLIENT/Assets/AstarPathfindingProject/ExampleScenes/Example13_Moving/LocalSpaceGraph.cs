using UnityEngine;
using System.Collections;

/** Helper for LocalSpaceRichAI */
[HelpURL("http://arongranberg.com/astar/docs/class_pathfinding_1_1_local_space_graph.php")]
public class LocalSpaceGraph : MonoBehaviour {
	protected Matrix4x4 originalMatrix;

	void Start () {
		originalMatrix = transform.localToWorldMatrix;
	}

	public Matrix4x4 GetMatrix ( ) {
		return transform.worldToLocalMatrix * originalMatrix;
	}
}
