using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class TerrainConverter : MonoBehaviour
{
    public Terrain targetTerrain;
    public GameObject runeStonePrototypePrefab; // Terrain에 등록된 프리팹
    public GameObject replacementPrefab;        // 생성할 GameObject
    public Transform rootParent;

    [ContextMenu("Convert and Remove Trees")]
    public void ConvertAndRemove()
    {
        if (targetTerrain == null || runeStonePrototypePrefab == null || replacementPrefab == null)
        {
            Debug.LogError("Terrain, 대상 Tree Prefab, 변환 Prefab이 하나 이상 누락되었습니다.");
            return;
        }

        TerrainData data = targetTerrain.terrainData;
        TreePrototype[] prototypes = data.treePrototypes;
        TreeInstance[] instances = data.treeInstances;

        int targetIndex = -1;
        for (int i = 0; i < prototypes.Length; i++)
        {
            if (prototypes[i].prefab == runeStonePrototypePrefab)
            {
                targetIndex = i;
                break;
            }
        }

        if (targetIndex == -1)
        {
            Debug.LogError("지정한 프리팹이 Terrain Tree Prototype에 없습니다.");
            return;
        }

        Vector3 terrainPos = targetTerrain.transform.position;
        List<TreeInstance> keptInstances = new();
        int convertedCount = 0;

        for (int i = 0; i < instances.Length; i++)
        {
            TreeInstance inst = instances[i];

            if (inst.prototypeIndex == targetIndex)
            {
                Vector3 worldPos = Vector3.Scale(inst.position, data.size) + terrainPos;

                GameObject obj = PrefabUtility.InstantiatePrefab(replacementPrefab) as GameObject;
                obj.transform.position = worldPos;
                obj.transform.rotation = Quaternion.Euler(0, inst.rotation * Mathf.Rad2Deg, 0);
                obj.transform.localScale = Vector3.one * inst.widthScale;

                if (rootParent != null)
                    obj.transform.SetParent(rootParent);

                convertedCount++;
            }
            else
            {
                keptInstances.Add(inst);
            }
        }

        
        Undo.RegisterFullObjectHierarchyUndo(targetTerrain.gameObject, "Convert Trees");

        
        data.treeInstances = keptInstances.ToArray();

        Debug.Log($"총 {convertedCount}개의 '{runeStonePrototypePrefab.name}' 트리를 GameObject로 변환하고 Terrain에서 제거했습니다.");
    }


}
