using UnityEngine;

public class trashbin : MonoBehaviour, IInteractable
{

    public bool isOpened { get; private set; }
    public string trashbinID { get; private set; }

    // Use later for changing sprite of trash can after its opened
    public Sprite openedSprite;

    // Generate unique ID for each trash bin to keep track of which bin were opened
    void Start()
    {
        trashbinID ??= GlobalHelper.GenerateUniqueID(gameObject);
       
    }


    public bool canInteract()
    {
        return !isOpened;
    }

    // If the bin hasn't been opened yet, open it
    public void Interact()
    {
        if (!canInteract()) return;
        openBin();
    }

    // Set the bin status to be opened (add sprite change later)
    private void openBin()
    {
        setOpened(true);

    }

    public void setOpened(bool opened)
    {
        if (isOpened = opened)
        {
            GetComponent<SpriteRenderer>().sprite = openedSprite;
        }
    }

}
