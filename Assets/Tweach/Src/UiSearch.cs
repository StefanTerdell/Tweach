﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Tweach
{

    public class UiSearch : MonoBehaviour
    {
        public void Search(string query)
        {
            if (query.Trim() == string.Empty)
            {
                PropagateMatchDownwards(Tweach.gameObjectReferences);
            }
            else
            {
                MatchDownwards(query, Tweach.gameObjectReferences);
            }

            Tweach.GetUiInstantiatorWithSettings().FillHierarchy(Tweach.gameObjectReferences);
        }

        void MatchDownwards(string query, List<GameObjectReference> gameObjectReferences)
        {
            foreach (var gameObjectReference in gameObjectReferences)
            {
                gameObjectReference.matchesSearchQuery = false;

                if (gameObjectReference.GetName().ToUpperInvariant().Contains(query.ToUpperInvariant()))
                    PropagateMatchUpwards(gameObjectReference);
                
                if (gameObjectReference.childGameObjectReferences != null)
                    MatchDownwards(query, gameObjectReference.childGameObjectReferences);
            }
        }

        void PropagateMatchUpwards(GameObjectReference gameObjectReference)
        {
            gameObjectReference.matchesSearchQuery = true;
            gameObjectReference.expanded = true;

            if (gameObjectReference.parentGameObjectReference != null)
                PropagateMatchUpwards(gameObjectReference.parentGameObjectReference);
        }

        void PropagateMatchDownwards(List<GameObjectReference> gameObjectReferences)
        {
            foreach (var gameObjectReference in gameObjectReferences)
            {
                gameObjectReference.matchesSearchQuery = true;
                gameObjectReference.expanded = false;

                if (gameObjectReference.childGameObjectReferences != null)
                    PropagateMatchDownwards(gameObjectReference.childGameObjectReferences);
            }
        }
    }
}