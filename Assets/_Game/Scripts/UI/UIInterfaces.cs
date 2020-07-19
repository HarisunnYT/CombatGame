using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IInteractableMessage
{
    bool Interactable { get; set; }
    int NonInteractableIndex { get; set; }

    string GetInteractableMessage();

    string GetNonInteractableMessage();
}
