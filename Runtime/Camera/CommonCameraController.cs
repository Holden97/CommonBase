using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;

//https://github.com/BhavyamGoyal/CameraRotateAroundObject-Unity
//https://blog.51cto.com/u_15273495/2914640

namespace CommonBase
{
    public class CommonCameraController : MonoSingleton<CommonCameraController>
    {
        private Vector3 target;//获取旋转目标
        public float speed = 1;
        private bool openZoom;

        public Vector3Int maxRotationLimitation;

        public void SetTarget(Vector3 target)
        {
            this.target = target;
        }


        private void camerarotate() //摄像机围绕目标旋转操作
        {
            if (target == null) return;
            //也可以使用Transfomr.RotateAround
            var mouse_x = Input.GetAxis("Mouse X");//获取鼠标X轴移动
            var mouse_y = -Input.GetAxis("Mouse Y");//获取鼠标Y轴移动
            if (Input.GetKey(KeyCode.Mouse1))
            {
                transform.Translate(Vector3.left * (mouse_x * 15f) * Time.deltaTime);
                transform.Translate(Vector3.up * (mouse_y * 15f) * Time.deltaTime);
            }
            if (Input.GetKey(KeyCode.Mouse0))
            {
                RotateAround(target, Vector3.up, mouse_x * 5);
                RotateAround(target, transform.right, mouse_y * 5);
            }
            ApplyLimitation();
        }
        private void camerazoom() //摄像机滚轮缩放
        {
            if (Input.GetAxis("Mouse ScrollWheel") > 0)
                transform.Translate(Vector3.forward * 0.5f);
            if (Input.GetAxis("Mouse ScrollWheel") < 0)
                transform.Translate(Vector3.forward * -0.5f);
        }

        private void Update()
        {
            if (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt))
            {
                camerarotate();
            }
            if (openZoom)
            {
                camerazoom();
            }

        }

        private void ApplyLimitation()
        {
            //var minX = MathF.Min(maxRotationLimitation.x, transform.rotation.x);
            //var minY = MathF.Min(maxRotationLimitation.y, transform.rotation.y);
            //var minZ = MathF.Min(maxRotationLimitation.z, transform.rotation.z);
            //transform.eulerAngles = new Vector3(minX, minY, minZ);
        }

        void RotateAround(Vector3 center, Vector3 axis, float angle)
        {
            Vector3 pos = transform.position;
            Quaternion rot = Quaternion.AngleAxis(angle, axis);
            Vector3 dir = pos - center; //计算从圆心指向摄像头的朝向向量
            dir = rot * dir;            //旋转此向量
            transform.position = center + dir;//移动摄像机位置
            var myrot = transform.rotation;
            //transform.rotation *= Quaternion.Inverse(myrot) * rot *myrot;//设置角度另一种方法
            transform.rotation = rot * myrot; //设置角度

        }
    }
}
