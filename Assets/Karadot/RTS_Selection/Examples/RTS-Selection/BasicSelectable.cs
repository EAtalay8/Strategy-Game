using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KaradotGames_General {
    public class BasicSelectable : MonoBehaviour, ISelectable {

        //ISelectable
        [SerializeField]
        bool isSelected = false;

        public bool IsSelected {
            get => isSelected;
            set {
                isSelected = value;
                ChangeMaterialBySelection ();
            }
        }

        private void ChangeMaterialBySelection () {
            if (isSelected) {
                GetComponent<Renderer> ().material = SelectedMat;
            } else {
                GetComponent<Renderer> ().material = NormalMat;
            }
        }

        public GameObject GetGameObject () {
            return gameObject;
        }

        public string GetName () {
            return transform.name;
        }

        [SerializeField]
        Material NormalMat, SelectedMat;

        Material material;

        public Vector3 pos;

        // Monobehaviour
        void Start () {
            material = GetComponent<Renderer> ().material;
            pos = transform.position;
        }

        void Update () {
            if (pos != null && Vector3.Distance (transform.position, pos) > 2f) {
                Vector3 dir = pos-transform.position;
                dir = dir.normalized;
                transform.position += dir*10 * Time.deltaTime;
            }
        }

    }
}