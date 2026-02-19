using TMPro;
using UnityEngine;

public class PlayerDebugger : MonoBehaviour
{
    [Header("Debug References")]
    [SerializeField] private TMP_Text username;
    private DebugSection BuildSection()
    {
        DebugSection section = new DebugSection(
            $"Player: {username.text}",
            () =>
            {
                return
                    $"Position: {transform.position}\n";
            });

        return section;
    }
}
