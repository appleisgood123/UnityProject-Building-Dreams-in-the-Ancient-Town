using UnityEngine;
using System.Collections.Generic;

public static class TreeRemover
{
    public static void RemoveTreesAtPosition(Vector3 position, float radius = 2f)
    {
        Terrain[] terrains = Terrain.activeTerrains;
        if (terrains == null || terrains.Length == 0) return;

        foreach (Terrain terrain in terrains)
        {
            TerrainData terrainData = terrain.terrainData;
            if (terrainData == null) continue;

            // 世界坐标转地形局部坐标
            Vector3 localPos = terrain.transform.InverseTransformPoint(position);
            if (localPos.x < 0 || localPos.x > terrainData.size.x ||
                localPos.z < 0 || localPos.z > terrainData.size.z)
                continue;

            float xNorm = localPos.x / terrainData.size.x;
            float zNorm = localPos.z / terrainData.size.z;
            float radiusNorm = radius / Mathf.Max(terrainData.size.x, terrainData.size.z);

            TreeInstance[] trees = terrainData.treeInstances;
            List<TreeInstance> newTrees = new List<TreeInstance>();
            foreach (var tree in trees)
            {
                Vector3 treePos = new Vector3(tree.position.x, 0, tree.position.z);
                Vector3 targetPos = new Vector3(xNorm, 0, zNorm);
                if (Vector3.Distance(treePos, targetPos) > radiusNorm)
                    newTrees.Add(tree);
            }

            if (newTrees.Count != trees.Length)
            {
                terrainData.treeInstances = newTrees.ToArray();
                terrain.Flush();
            }
        }
    }
}