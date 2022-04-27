using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace KaradotGames_General {

    public static class Selection {

        public static GameObject selected = null;
        public static List<GameObject> selections = new List<GameObject> ();

        static bool isMultiSelect = false;

        static List<GameObject> boxSelectionTemp = new List<GameObject> ();

        static GameObject tempLocator;

        static LayerMask groundLayer;

        static Material boxMat;

        static RectTransform uiElement;

        static SelectionUIType uiType;

        static bool canStart = false;

        static List<GameObject>[] groups = new List<GameObject>[8];

        static int activeGroup = -1;

        //Events

        public delegate void SelectionEvents (GameObject obj);
        public delegate void GroupEvents (int groupID);
        public delegate void ClearingEvents ();

        public static SelectionEvents event_SelectionAdded;
        public static SelectionEvents event_SelectionRemoved;

        public static GroupEvents event_GroupUpdate;

        public static event ClearingEvents event_SelectionCleared;

        /// <summary>
        /// Init With 3D Preview Of Selection Area
        /// <para>Set Your Ground Object in Different Layer</para>
        /// <para>Also, you can set Material for Selection Area</para>
        /// </summary>
        /// <param name="detectionLayer">Ground Layer For Detection</param>
        /// <param name="selectorMaterial">3D Area Preview Material(Transparent Material Recommended</param>
        public static void Init (LayerMask detectionLayer, Material selectorMaterial) {

            if (selectorMaterial == null) {
                Debug.LogError ("Add Material for 3d Selection preview");
                return;
            }

            groundLayer = detectionLayer;
            boxMat = selectorMaterial;

            //create object for BoxCast
            tempLocator = GameObject.CreatePrimitive (PrimitiveType.Cube);
            tempLocator.GetComponent<Renderer> ().material = selectorMaterial;
            GameObject.Destroy (tempLocator.GetComponent<BoxCollider> ());

            tempLocator.transform.localScale = Vector3.zero;

            uiType = SelectionUIType.GameObject;

            PopulateEmptyGroups ();

            canStart = true;
        }

        /// <summary>
        /// Init With UI Preview Of Selection Area
        /// <para>Set Your Ground Object in Different Layer</para>
        /// <para>You should Create UI Object and Assign It</para>
        /// </summary>
        /// <param name="detectionLayer">Ground Layer For Detection</param>
        /// <param name="selectorUI">UI GameObject</param>
        public static void Init (LayerMask detectionLayer, RectTransform selectorUI) {

            if (selectorUI == null) {
                Debug.LogError ("Add UI element for ui preview");
                return;
            }
            //Set UI Type 
            uiType = SelectionUIType.UI;

            groundLayer = detectionLayer;
            uiElement = selectorUI;
            //Reset UI Position and Size
            uiElement.transform.position = Vector2.zero;
            uiElement.sizeDelta = Vector2.zero;

            //create object for BoxCast
            tempLocator = new GameObject ();
            tempLocator.transform.localScale = Vector3.zero;

            PopulateEmptyGroups ();

            canStart = true;
        }

        //create empty groups for Init
        static void PopulateEmptyGroups () {
            for (int i = 0; i < groups.Length; i++) {
                groups[i] = new List<GameObject> ();
            }
        }
        /// <summary>
        /// Put Selected Objects to  group
        /// <para>Currently 9(ids between 0-8) group supported</para>
        /// </summary>
        /// <param name="groupID"></param>
        public static void SetGroup (int groupID) {
            if (selections.Count == 0) {
                groups[groupID].Clear ();
            } else {
                List<GameObject> temp = new List<GameObject> ();

                for (int i = 0; i < selections.Count; i++) {
                    temp.Add (selections[i]);
                }
                groups[groupID] = temp;
            }
            if (event_GroupUpdate != null)
                event_GroupUpdate (groupID);
        }

        /// <summary>
        /// Clear Selection and Set by group
        /// </summary>
        /// <param name="groupID"></param>
        public static void SelectGroup (int groupID) {
            ClearSelections ();

            for (int elementId = 0; elementId < groups[groupID].Count; elementId++) {
                AddElement (groups[groupID][elementId]);
            }
        }

        /// <summary>
        /// Get Group Element Count by id
        /// </summary>
        /// <param name="groupID"></param>
        /// <returns></returns>
        public static int GetGroupElementCount (int groupID) {
            return groups[groupID].Count;
        }

        /// <summary>
        /// Get Group Element as List
        /// </summary>
        /// <param name="groupID"></param>
        /// <returns></returns>
        public static List<GameObject> GetGroup (int groupID) {
            return groups[groupID];
        }

        /// <summary>
        /// List of Non Empty Group IDs
        /// </summary>
        /// <returns></returns>
        public static List<int> GetNonEmptyGroups () {
            List<int> nonEmpties = new List<int> ();

            for (int i = 0; i < groups.Length; i++) {
                if (groups[i].Count > 0) {
                    nonEmpties.Add (i);
                }
            }

            return nonEmpties;
        }

        //Add ISelectable Type Element for public and private
        /// <summary>
        /// <para>Use for directly adding and Selectable Object to active selection list</para>
        ///<para> Can Be Used For Custom Interaction</para>
        /// <para>Can be use automaticly assign objects to player control</para>
        /// </summary>
        /// <param name="selectable"></param>
        public static void AddElement (ISelectable selectable) {
            if (!canStart) {
                Debug.LogError ("Use Selection.Init() to set variables correctly");
                return;
            }
            if (selectable == null)
                return;
            selectable.IsSelected = true;
            selections.Add (selectable.GetGameObject ());
            selected = selectable.GetGameObject ();
            if (event_SelectionAdded != null)
                event_SelectionAdded (selected);
        }

        //Add Gameobject for private
        static void AddElement (GameObject selectableObj) {
            AddElement (selectableObj.GetComponent<ISelectable> ());
        }

        /// <summary>
        /// Selecting And Adding Single Element To Selection
        /// <para>Using Raycast</para>
        /// <para>For Your Own Custom Addding You Can Use Selection.AddElement()</para>
        /// </summary>
        /// <returns></returns>
        public static GameObject Select () {
            if (!canStart) {
                Debug.LogError ("Use Selection.Init() to set variables correctly");
                return null;
            }
            if (selections != null && !isMultiSelect) {
                ClearSelections ();
            }
            selected = GetGO ();
            if (selected != null) {
                AddElement (selected);
            }
            return selected;
        }

        //Raycast and get ISelectableObject
        static GameObject GetGO () {
            Vector3 mPos = Input.mousePosition;
            Camera cam = Camera.main;
            GameObject tempSelected = null;
            if (cam == null) {
                Debug.LogError ("Need Main Camera in Scene!! \n<size=10>Solution:Add \"MainCamera\" tag Camera GameObject</size>");
            } else {
                RaycastHit hit;
                Ray ray = cam.ScreenPointToRay (mPos);
                if (Physics.Raycast (ray, out hit)) {
                    if (hit.transform.GetComponent<ISelectable> () != null) {
                        tempSelected = hit.transform.gameObject;
                        //hit.transform.GetComponent<ISelectable> ().IsSelected = true;
                    } else {
                        if (!isMultiSelect) {
                            ClearSelections ();
                        }
                    }
                }
            }
            return tempSelected;
        }

        /// <summary>
        /// Adding Selected Object to List
        /// <para>Using Raycasting</para>
        /// </summary>
        /// <returns></returns>
        public static List<GameObject> AddSelection () {
            if (!canStart) {
                Debug.LogError ("Use Selection.Init() to set variables correctly");
                return null;
            }
            isMultiSelect = true;
            if (selected != null && selections.Count == 0) {
                selections.Add (selected);
            }
            GameObject tempSelected = Select ();
            if (tempSelected != null && !selections.Contains (tempSelected))
                selections.Add (selected);
            isMultiSelect = false;
            return selections;
        }

        /// <summary>
        /// Remove Selected Object
        /// <para>Using Raycast</para>
        /// </summary>
        /// <returns></returns>
        public static GameObject RemoveSelection () {
            if (!canStart) {
                Debug.LogError ("Use Selection.Init() to set variables correctly");
                return null;
            }
            GameObject objToRemove = GetGO ();
            if (objToRemove == null)
                return null;

            RemoveObject (objToRemove.GetComponent<ISelectable> ());
            return objToRemove;
        }

        /// <summary>
        /// Use this for custom interactions, Built-in Function is Selection.RemoveSelection()
        /// </summary>
        /// <returns></returns>
        public static void RemoveObject (ISelectable selectableToRemove) {
            GameObject objToRemove = selectableToRemove.GetGameObject ();
            objToRemove.GetComponent<ISelectable> ().IsSelected = false;
            if (objToRemove != null && selections.Count != 0) {
                selections.Remove (objToRemove);
            }
            if (event_SelectionRemoved != null)
                event_SelectionRemoved (objToRemove);
        }

        /// <summary>
        /// Clear All Selected Elements
        /// </summary>
        public static void ClearSelections () {
            if (!canStart) {
                Debug.LogError ("Use Selection.Init() to set variables correctly");
                return;
            }
            if (selected != null) {
                selected.GetComponent<ISelectable> ().IsSelected = false;
            }
            foreach (GameObject obj in selections) {
                obj.GetComponent<ISelectable> ().IsSelected = false;
            }
            selected = null;
            selections.Clear ();
            if (event_SelectionCleared != null) {
                event_SelectionCleared ();
            }
        }

        /// <summary>
        /// IMPORTANT!Only changes temporarily, Use Selection.CompleteBoxSelection() to complete box selection
        /// <para>Gets two Mouse Position And Convert to World Space</para>
        /// <para>Creates Square Shaped Area And Use BoxCast to Get Selectable Objects</para>
        /// </summary>
        /// <param name="mPosStart"></param>
        /// <param name="mPosEnd"></param>
        public static void UpdateBoxSelectParams (Vector2 mPosStart, Vector2 mPosEnd) {
            if (!canStart) {
                Debug.LogError ("Use Selection.Init() to set variables correctly");
                return;
            }
            Camera cam = Camera.main;
            Ray startRay = cam.ScreenPointToRay (mPosStart);
            Ray endRay = cam.ScreenPointToRay (mPosEnd);

            Vector3 startPoint = Vector3.zero, endPoint = Vector3.zero;
            RaycastHit hit;
            if (Physics.Raycast (startRay, out hit, Mathf.Infinity, groundLayer)) {
                startPoint = hit.point;
            }

            if (Physics.Raycast (endRay, out hit, Mathf.Infinity, groundLayer)) {
                endPoint = hit.point;
            }

            if (uiType == SelectionUIType.UI) {
                float width = mPosEnd.x - mPosStart.x;
                float height = mPosEnd.y - mPosStart.y;
                uiElement.sizeDelta = new Vector2 (Mathf.Abs (width), Mathf.Abs (height));
                uiElement.anchoredPosition = mPosStart + new Vector2 (width / 2, height / 2);

            }

            //create selection box borders
            float scaleX = Mathf.Abs (startPoint.x - endPoint.x);
            float scaleY = Mathf.Abs (startPoint.y - startPoint.y);
            scaleY = Mathf.Clamp (scaleY, 1f, Mathf.Infinity);
            float scaleZ = Mathf.Abs (startPoint.z - endPoint.z);

            tempLocator.transform.localScale = new Vector3 (scaleX, scaleY, scaleZ);
            tempLocator.transform.position = Vector3.Lerp (startPoint, endPoint, .5f);

            Collider[] hitColliders = Physics.OverlapBox (tempLocator.transform.position, tempLocator.transform.localScale / 2, Quaternion.identity);
            Debug.DrawLine (tempLocator.transform.position, tempLocator.transform.position + tempLocator.transform.localScale / 2);
            List<GameObject> toGameObj = new List<GameObject> ();

            //return if not collide any object
            if (hitColliders.Length == 0)
                return;

            foreach (Collider col in hitColliders) {
                toGameObj.Add (col.gameObject);
            }

            foreach (GameObject obj in toGameObj) {
                ISelectable selectable = obj.GetComponent<ISelectable> ();

                if (selectable != null) {
                    if (!boxSelectionTemp.Contains (obj)) {
                        boxSelectionTemp.Add (obj);
                        selectable.IsSelected = true;
                    }
                }
            }

            List<GameObject> toRemove = new List<GameObject> ();
            foreach (GameObject bSelected in boxSelectionTemp) {
                if (!toGameObj.Contains (bSelected)) {
                    toRemove.Add (bSelected);
                    bSelected.GetComponent<ISelectable> ().IsSelected = false;
                }
            }

            foreach (GameObject r in toRemove) {
                boxSelectionTemp.Remove (r);
            }
        }

        /// <summary>
        /// IMPORTANT! Before call this function, you should use Selection.UpdateBoxSelectParams() for initialize box selection
        /// <para> Uses BoxCast to get object in selected area</para>
        /// </summary>
        public static void CompleteBoxSelection () {

            if (!canStart) {
                Debug.LogError ("Use Selection.Init() to set variables correctly");
                return;
            }

            boxSelectionTemp.Clear ();

            if (uiElement != null) {

                uiElement.sizeDelta = Vector2.zero;
                uiElement.anchoredPosition = Vector2.zero;
            }
            //return if selection area size small
            if (tempLocator.transform.lossyScale.magnitude < .5f)
                return;

            Collider[] hitColliders = Physics.OverlapBox (tempLocator.transform.position, tempLocator.transform.localScale / 2, Quaternion.identity);

            tempLocator.transform.localScale = Vector3.zero;
            List<GameObject> result = new List<GameObject> ();

            bool isEmpty = true;
            foreach (Collider col in hitColliders) {
                ISelectable selectable = col.gameObject.GetComponent<ISelectable> ();
                if (selectable != null) {
                    AddElement (selectable);
                    isEmpty = false;
                }
            }

            if (isEmpty)
                return;
            selected = selections[selections.Count - 1];
        }
    }

    public enum SelectionUIType {
        GameObject,
        UI
    }
}