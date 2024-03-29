using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Connoreaster
{
    [RequireComponent(typeof(LineRenderer))]
    public class FreeCamCutter : MonoBehaviour
    {
        // LINE RENDERER VARIABLES
        private Camera mainCam;
        private Vector3 _POINTA;
        private Vector3 _POINTB;
        private LineRenderer _LINE;

        // MESH CUTTING VARIABLES
        private Plane slicePlane;
        private Vector3 mouseDownPos;
        private Vector3 mouseUpPos;

        private void Awake()
        {
            mainCam = Camera.main; // Get the main camera

            _LINE = GetComponent<LineRenderer>(); // Get the line renderer
            _LINE.startWidth = 0.01f; // Set the line width
            _LINE.endWidth = 0.01f; // Set the line width
        }

        // Update is called once per frame
        void Update()
        {
            DrawLine();
        }

        /// <summary>
        /// Track the mouse position and input to draw the cutting line
        /// </summary>
        private void DrawLine()
        {
            Vector3 mousePos = Input.mousePosition; // Get the mouse position
            mousePos.z = -mainCam.transform.position.z; // Set the z position to the camera's z position

            if (Input.GetMouseButtonDown(0)) // If the left mouse button is pressed
            {
                _POINTA = mainCam.ScreenToWorldPoint(mousePos); // Set the first point to the mouse position
                _LINE.enabled = true; // Disable the line renderer

                Ray ray = mainCam.ScreenPointToRay(mousePos); // Create a ray from the mouse position
                if (Physics.Raycast(ray, out RaycastHit hit)) // If the ray hits something
                {
                    mouseDownPos = hit.point; // Set the mouse down position to the hit point
                }
            }

            if (Input.GetMouseButton(0)) // If the left mouse button is held down
            {
                _POINTB = mainCam.ScreenToWorldPoint(mousePos); // Set the second point to follow the mouse position

                _LINE.SetPosition(0, _POINTA); // Set the first point of the line
                _LINE.SetPosition(1, _POINTB); // Set the second point of the line

                _LINE.startColor = Color.red; // Set the line color
                _LINE.endColor = Color.red; // Set the line color
            }

            if (Input.GetMouseButtonUp(0)) // If the left mouse button is released
            {
                _POINTB = mainCam.ScreenToWorldPoint(mousePos); // Set the final location of the second point to the current mouse position

                _LINE.SetPosition(0, _POINTA); // Set the first point of the line
                _LINE.SetPosition(1, _POINTB); // Set the second point of the line
                _LINE.enabled = false; // Disable the line renderer

                Ray ray = mainCam.ScreenPointToRay(mousePos); // Create a ray from the mouse position
                if (Physics.Raycast(ray, out RaycastHit hit)) // If the ray hits something
                {
                    mouseUpPos = hit.point; // Set the mouse up position to the hit point
                }

                Slice(); // Call the slice method
            }
        }

        /// <summary>
        /// Slice the object using a created plane
        /// </summary>
        private void Slice()
        {
            Vector3 planePoint = (mouseDownPos + mouseUpPos) / 2; // Get the center point of the line
            Vector3 planeNormal = Vector3.Cross((mouseDownPos - mouseUpPos), mouseDownPos - mainCam.transform.position).normalized; // Get the normal of the plane
            Quaternion planeRotation = Quaternion.FromToRotation(Vector3.up, planeNormal); // Get the rotation of the plane

            Collider[] colliders = Physics.OverlapBox(planePoint, new Vector3(1f, 0.01f, 1f), planeRotation); // Get all colliders within the plane

            foreach (Collider hitGameObject in colliders)
            {
                MeshFilter meshFilter = hitGameObject.GetComponent<MeshFilter>(); // Get the mesh filter

                if (meshFilter != null)
                {
                    Cut(hitGameObject.gameObject, planePoint, planeNormal);
                }
            }
        }

        /// <summary>
        /// Cut the mesh using a plane
        /// </summary>
        public void Cut(GameObject hitGameObject, Vector3 planePoint, Vector3 planeNormal)
        {
            // Ensure the hit gameObject is sliceable
            if (hitGameObject.tag != "Sliceable")
            {
                return;
            }

            slicePlane = new Plane(hitGameObject.transform.InverseTransformDirection(-planeNormal), hitGameObject.transform.InverseTransformPoint(planePoint)); // Create a new plane

            MeshCutCalculations calc = new MeshCutCalculations(); // Create a new mesh cut calculations object
            calc.CallScript(hitGameObject, slicePlane); // Call the mesh cut calculations script
        }
    }
}
