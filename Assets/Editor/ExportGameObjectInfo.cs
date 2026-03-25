using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text;

public class ExportGameObjectInfo : Editor
{
    [MenuItem("Tools/Export Selected GameObject Info")]
    static void Export()
    {
        GameObject selected = Selection.activeGameObject;
        if (selected == null)
        {
            Debug.LogError("No GameObject selected!");
            return;
        }

        StringBuilder sb = new StringBuilder();
        sb.AppendLine("=== GameObject: " + selected.name + " ===");
        sb.AppendLine("Active: " + selected.activeSelf);
        sb.AppendLine("Layer: " + LayerMask.LayerToName(selected.layer));
        sb.AppendLine("Tag: " + selected.tag);
        sb.AppendLine("Position: " + selected.transform.position);
        sb.AppendLine("Scale: " + selected.transform.localScale);
        sb.AppendLine("");
        sb.AppendLine("=== Components ===");

        foreach (Component c in selected.GetComponents<Component>())
        {
            sb.AppendLine("--- " + c.GetType().Name + " ---");

            // Rigidbody2D
            if (c is Rigidbody2D rb)
            {
                sb.AppendLine("  gravityScale: " + rb.gravityScale);
                sb.AppendLine("  bodyType: " + rb.bodyType);
                sb.AppendLine("  simulated: " + rb.simulated);
            }

            // Collider2D
            if (c is Collider2D col)
            {
                sb.AppendLine("  enabled: " + col.enabled);
                sb.AppendLine("  isTrigger: " + col.isTrigger);
                sb.AppendLine("  offset: " + col.offset);
                if (c is BoxCollider2D box)
                    sb.AppendLine("  size: " + box.size);
                if (c is CircleCollider2D circle)
                    sb.AppendLine("  radius: " + circle.radius);
                if (c is CapsuleCollider2D cap)
                    sb.AppendLine("  size: " + cap.size);
            }

            // SpriteRenderer
            if (c is SpriteRenderer sr)
            {
                sb.AppendLine("  sprite: " + (sr.sprite != null ? sr.sprite.name : "NULL"));
                sb.AppendLine("  enabled: " + sr.enabled);
                sb.AppendLine("  sortingLayer: " + sr.sortingLayerName);
                sb.AppendLine("  sortingOrder: " + sr.sortingOrder);
            }

            // TopDownNPC
            if (c is TopDownNPC npc)
            {
                sb.AppendLine("  currentState: " + npc.currentState);
                sb.AppendLine("  moveSpeed: " + npc.moveSpeed);
                sb.AppendLine("  yourInterface: " + (npc.yourInterface != null ? npc.yourInterface.name : "NULL"));
            }

            // PatientController
            if (c is PatientController pc)
            {
                sb.AppendLine("  data: " + (pc.data != null ? pc.data.name : "NULL"));
            }

            // PatientHealth
            if (c is PatientHealth ph)
            {
                sb.AppendLine("  currentHealth: " + ph.currentHealth);
                sb.AppendLine("  isDead: " + ph.isDead);
            }
        }

        string path = Application.dataPath + "/exported_" + selected.name + ".txt";
        File.WriteAllText(path, sb.ToString());
        Debug.Log("Exported to: " + path);
        AssetDatabase.Refresh();
    }
}