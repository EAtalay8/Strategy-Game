using System.Collections;
using System.Collections.Generic;
using KaradotGames_General;
using UnityEngine;
namespace KaradotGames_General {
    public class RTSSelectionTest : MonoBehaviour {
        Vector2 mStart, mEnd;

        [SerializeField]
        LayerMask selectionGroundLayer;

        [SerializeField]
        Material selectionMat;

        [SerializeField]
        RectTransform selectionUiPrefab = null;

        [SerializeField]
        BasicSelectable startElement;

        [SerializeField]
        List<BasicSelectable> startElements;

        List<int> nonEmptyGroups = new List<int> ();

        void Start () {

            //Selection.Init (selectionGroundLayer, selectionMat);
            Selection.Init (selectionGroundLayer, selectionUiPrefab);

            //Events
            Selection.event_GroupUpdate += UpdateGroups;
            Selection.event_SelectionAdded += SelectionAdded;
            Selection.event_SelectionRemoved += SelectionRemoved;
            Selection.event_SelectionCleared += SelectionCleared;

            //Adding Single Element and Multiple Elements Manually
            //Selection.AddElement(startElement);
            /*
            foreach (BasicSelectable element in startElements) {
                Selection.AddElement (element);
            }*/

        }
        void Update () {
            if (Input.GetMouseButtonDown (0)) {
                if (Input.GetKey (KeyCode.LeftShift)) {
                    //add new selected to list
                    List<GameObject> temp = Selection.AddSelection ();
                } else if (Input.GetKey (KeyCode.LeftControl)) {
                    //remove selected object
                    Selection.RemoveSelection ();
                } else {
                    //select single
                    GameObject test = Selection.Select ();

                }
                mStart = Input.mousePosition;
            }

            if (Input.GetMouseButton (0)) {
                mEnd = Input.mousePosition;
                if (Vector2.Distance (mStart, mEnd) >.5f)
                    Selection.UpdateBoxSelectParams (mStart, mEnd);
            }

            if (Input.GetMouseButtonUp (0)) {
                Selection.CompleteBoxSelection ();
            }

            if (Input.GetKeyDown (KeyCode.Space)) {
                RaycastHit hit;
                Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);

                if (Physics.Raycast (ray, out hit)) {
                    ISelectable selectable = hit.transform.GetComponent<ISelectable> ();
                    if (selectable != null) {
                        if (selectable.IsSelected) {
                            Selection.RemoveObject (selectable);
                        } else {
                            Selection.AddElement (selectable);
                        }
                    }
                }
            }

            if (Input.GetMouseButtonDown (1)) {

                RaycastHit hit;
                Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);
                if (Physics.Raycast (ray, out hit)) {
                    foreach (GameObject selected in Selection.selections) {
                        selected.GetComponent<BasicSelectable> ().pos = hit.point;
                    }
                }
            }

            if (Input.GetKey (KeyCode.LeftShift)) {
                if (Input.GetKeyDown (KeyCode.Alpha1)) {
                    Selection.SetGroup (1);
                }
                if (Input.GetKeyDown (KeyCode.Alpha2)) {
                    Selection.SetGroup (2);
                }
                if (Input.GetKeyDown (KeyCode.Alpha3)) {
                    Selection.SetGroup (3);
                }
            } else {
                if (Input.GetKeyDown (KeyCode.Alpha1)) {
                    Selection.SelectGroup (1);
                }

                if (Input.GetKeyDown (KeyCode.Alpha2)) {
                    Selection.SelectGroup (2);
                }
                if (Input.GetKeyDown (KeyCode.Alpha3)) {
                    Selection.SelectGroup (3);
                }
            }
        }

        //Event Functions
        void SelectionAdded (GameObject obj) {
            Debug.Log ("Added:" + obj.name);
        }

        void SelectionCleared () {
            Debug.Log ("Selections Cleared");
        }
        void SelectionRemoved (GameObject obj) {
            Debug.Log ("Removed:" + obj.name);
        }
        void UpdateGroups (int groupID) {
            Debug.Log ("Group " + groupID + " updated");
            nonEmptyGroups = Selection.GetNonEmptyGroups ();
        }
        
        void OnGUI () {
            for (int i = 0; i < nonEmptyGroups.Count; i++) {
                int groupId = nonEmptyGroups[i];
                if (GUI.Button (new Rect (10, (i + 1) * 50, 75, 50), "Group" + groupId + "\n Count:" + Selection.GetGroupElementCount (groupId))) {
                    Selection.SelectGroup (groupId);
                }
            }
        }
    }
}