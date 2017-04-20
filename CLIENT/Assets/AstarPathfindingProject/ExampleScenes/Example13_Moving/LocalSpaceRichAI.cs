using UnityEngine;
using System.Collections;
using Pathfinding;

/** RichAI for local space (pathfinding on moving graphs).
 *
 * What this script does is that it fakes graph movement.
 * It can be seen in the example scene called 'Moving' where
 * a character is pathfinding on top of a moving ship.
 * The graph does not actually move in that example
 * instead there is some 'cheating' going on.
 *
 * When requesting a path, we first transform
 * the start and end positions of the path request
 * into local space for the object we are moving on
 * (e.g the ship in the example scene), then when we get the
 * path back, they will still be in these local coordinates.
 * When following the path, we will every Update transform
 * the coordinates of the waypoints in the path to global
 * coordaintes so that we can follow them.
 * This assumes that the object that we are moving on
 * was at the origin (0,0,0) when the graph was
 * scanned, otherwise it will not have the correct
 * alignment.
 *
 * This functionality is only implemented for the RichAI
 * script, however it should not be hard to
 * use the same approach for other movement scripts.
 *
 * \astarpro
 */
[HelpURL("http://arongranberg.com/astar/docs/class_pathfinding_1_1_local_space_rich_a_i.php")]
public class LocalSpaceRichAI : RichAI {
	/** Root of the object we are moving on */
	public LocalSpaceGraph graph;

	public override void UpdatePath () {
		canSearchPath = true;
		waitingForPathCalc = false;
		Path p = seeker.GetCurrentPath();

		//Cancel any eventual pending pathfinding request
		if (p != null && !seeker.IsDone()) {
			p.Error();
			// Make sure it is recycled. We won't receive a callback for this one since we
			// replace the path directly after this
			p.Claim(this);
			p.Release(this);
		}

		waitingForPathCalc = true;
		lastRepath = Time.time;

		Matrix4x4 m = graph.GetMatrix();

		seeker.StartPath(m.MultiplyPoint3x4(tr.position), m.MultiplyPoint3x4(target.position));
	}

	protected override Vector3 UpdateTarget (RichFunnel fn) {
		Matrix4x4 m = graph.GetMatrix();
		Matrix4x4 mi = m.inverse;


		Debug.DrawRay(m.MultiplyPoint3x4(tr.position), Vector3.up*2, Color.red);
		Debug.DrawRay(mi.MultiplyPoint3x4(tr.position), Vector3.up*2, Color.green);

		buffer.Clear();

		/* Current position. We read and write to tr.position as few times as possible since doing so
		 * is much slower than to read and write from/to a local variable
		 */
		Vector3 position = tr.position;
		bool requiresRepath;

		// Update, but first convert our position to graph space, then convert the result back to world space
		var positionInGraphSpace = m.MultiplyPoint3x4(position);
		positionInGraphSpace = fn.Update(positionInGraphSpace, buffer, 2, out lastCorner, out requiresRepath);
		position = mi.MultiplyPoint3x4(positionInGraphSpace);

		Debug.DrawRay(position, Vector3.up*3, Color.black);

		// convert the result to world space from graph space
		for (int i = 0; i < buffer.Count; i++) {
			buffer[i] = mi.MultiplyPoint3x4(buffer[i]);
			Debug.DrawRay(buffer[i], Vector3.up*3, Color.yellow);
		}

		if (requiresRepath && !waitingForPathCalc) {
			UpdatePath();
		}

		return position;
	}
}
