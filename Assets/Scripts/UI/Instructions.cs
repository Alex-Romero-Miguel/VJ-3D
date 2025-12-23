using TMPro;
using Unity.Burst.Intrinsics;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.Rendering.ProbeAdjustmentVolume;

public class InstructionsUI : MonoBehaviour
{
    [SerializeField] private TMP_Text pageCounterText;
    [SerializeField] private TMP_Text bodyText;

    private int currentPage;

    private string[] pages =
    {
        "The aim of the game is to get the bloc to fall into the square hole at the end of each stage. \n" +
            "There are 10 stages to complete. ",

        "To move the block around the world, use WASD or the arrow keys. \n" +
            "Be careful not to fall of the edges, the level will be restarted if this happens.",

        "Bridges and switches are located in many levels. The switches are activated when they are pressed down by the block. \n" +
            "You do not need to stay resting on the switch to keep bridges close. ",

        "There are two types of switches: ‘Heavy’ X-shaped ones and ‘Soft’ round ones.\n" +
            "Each press on the switch will swap the bridges from open to closed to open each time it is used.",


        "Soft switches are activated when any part of you block presses it. \n" +
            "Hard switches require much more pressure. so your block must be standing on its end to activate it.", 
            
        "Orange tiles are more fragile than the rest of the land. \n" +
            "If you block stands up vertically on an orange tile, the tile will give way and your block will fall. ",

        "There's a third switch type ({ }): it teleports your block while splitting it into two smaller ones.\n" +
            "You can control them separately, and they merge back when placed next to each other.",

        "You can switch between the small blocks with the spacebar. \n" +
            "They can activate soft switches but not heavy ones, and only the full block can enter the exit hole to finish the stage.",

        "Your progress is saved after each level.\n" +
            "If you have a saved game, Resume loads it; otherwise, you start from level 0.\n" +
            "Enjoy!"


    };

    private void OnEnable()
    {
        currentPage = 0;
        Refresh();
    }

    public void Next()
    {
        currentPage = Mathf.Min(currentPage + 1, pages.Length - 1);
        Refresh();
    }

    public void Prev()
    {
        currentPage = Mathf.Max(currentPage - 1, 0);
        Refresh();
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }

    private void Refresh()
    {
        pageCounterText.text = $"{currentPage + 1}/{pages.Length}";
        bodyText.text = pages[currentPage];
    }
}
