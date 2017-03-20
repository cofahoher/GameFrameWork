using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Combat
{
    public class CameraController : IDestruct
    {
        RenderWorld m_render_world;
        Camera m_camera_cmp;
        Transform m_camera_tf;

        public CameraController(RenderWorld render_world)
        {
        }

        public void Destruct()
        {
        }

        public void SetCameraUnityObject(GameObject camera_go)
        {
            m_camera_cmp = camera_go.GetComponent<Camera>();
            m_camera_tf = camera_go.transform;

            m_camera_tf.position = new Vector3(0, 10, -25);
            m_camera_tf.LookAt(Vector3.zero);
        }
    }
}