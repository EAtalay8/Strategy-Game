using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KaradotGames_General {
    public interface ISelectable
    {
        string GetName();
        GameObject GetGameObject();
        bool IsSelected{get;set;}
    }
}