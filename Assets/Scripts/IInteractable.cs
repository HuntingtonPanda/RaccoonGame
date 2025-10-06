using System;

// General interface for any interactable in the game (only trash bins for now)
public interface IInteractable
{
    void Interact();

    bool canInteract();
}
