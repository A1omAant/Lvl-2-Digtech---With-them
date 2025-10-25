using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;


public class GrassPainter : MonoBehaviour
{
    [Header("Grass Painting Settings Proceedural")]
    public List<GameObject> ShortGrass;
    public List<GameObject> MediumGrass;
    public List<GameObject> TallGrass;
    public Transform ParentForGrass;
    public float density = 0.5f;
    public float scaleMin = 0.8f;
    public float scaleMax = 1.2f;
    public float colourVariation = 0.1f;
    public int seed = 0;
    public float sampleStep = 1f;
    public Vector2 perlinScale = new Vector2(0.1f,0.1f);


    [Header("Height Rules")]
    public float minHeight = 0f;
    public float maxHeight = 1f;

    [Header("target terrain")]
    public List<MeshFilter> targetTerrainMesh;
    public List<MeshCollider> targetTerrainMeshCollider;




    public void ClearGrass(){
        if(ParentForGrass == null) return;
        for(int i = ParentForGrass.childCount - 1; i >= 0; i--){
            DestroyImmediate(ParentForGrass.GetChild(i).gameObject);
        }
    }

    public void PaintGrass(){

        if(ShortGrass.Count == 0 || MediumGrass.Count == 0 || TallGrass.Count == 0 || targetTerrainMesh == null || ParentForGrass == null) {
            Debug.LogWarning("GrassPainter: Missing references, cannot paint grass.");
            return;
        }

        Random.InitState(seed);
        ClearGrass();
        for(int MeshIndex = 0; MeshIndex < targetTerrainMesh.Count; MeshIndex++){
            
        

            Mesh mesh = targetTerrainMesh[MeshIndex].sharedMesh;
            Vector3[] vertices = mesh.vertices;
            Vector3 terrainScale = targetTerrainMesh[MeshIndex].transform.lossyScale;
            Vector3 terrainPosition = targetTerrainMesh[MeshIndex].transform.position;
            Quaternion terrainRotation = targetTerrainMesh[MeshIndex].transform.rotation;

            

            Bounds bounds = mesh.bounds;

            for(float x = bounds.min.x; x<bounds.max .x; x+=sampleStep){
                for(float z = bounds.min.z; z<bounds.max.z; z+=sampleStep){

                    float perlinValue = Mathf.PerlinNoise((x + seed) * perlinScale.x, (z + seed) * perlinScale.y);
                    if(Random.value > perlinValue * density) continue;

                    Vector3 samplePoint = Vector3.Scale(new Vector3(x, bounds.max.y + 1f, z), terrainScale) + terrainPosition;
                    Ray ray = new Ray(samplePoint, Vector3.down);
                    RaycastHit hit = new RaycastHit();
                    if(targetTerrainMeshCollider[MeshIndex] != null){
                        if(!targetTerrainMeshCollider[MeshIndex].Raycast(ray, out hit, bounds.size.y + 2f)) continue;
                    }else{

                        float NearestY = float.MinValue;
                        foreach(Vector3 vertex in vertices){
                            Vector3 worldV = terrainRotation * Vector3.Scale(vertex, terrainScale) + terrainPosition;
                            if(Mathf.Abs(worldV.x - (x + terrainPosition.x)) < sampleStep/2f &&
                            Mathf.Abs(worldV.z - (z + terrainPosition.z)) < sampleStep/2f){
                                if(worldV.y > NearestY){
                                    NearestY = worldV.y;
                                    break;
                                }
                            }
                        }
                        if(NearestY == float.MinValue) continue;
                        hit.point = new Vector3(samplePoint.x, NearestY, samplePoint.z);

                    }


                    if(hit.point.y < minHeight || hit.point.y > maxHeight) continue;

                    //if(Random.value > density) continue;
                    List <GameObject> ChosenCategory;
                    if(perlinValue < 0.33f){
                        ChosenCategory = ShortGrass;
                    }else if(perlinValue < 0.66f){
                        ChosenCategory = MediumGrass;
                    }else{
                        ChosenCategory = TallGrass;
                    }

                    GameObject grassPrefab = ChosenCategory[Random.Range(0, ChosenCategory.Count)];
                    GameObject grass = (GameObject)PrefabUtility.InstantiatePrefab(grassPrefab, ParentForGrass);

                    grass.transform.position = hit.point;
                    grass.transform.rotation = Quaternion.Euler(0f, Random.Range(0f,360f), 0f);
                    float randomScale = Random.Range(scaleMin, scaleMax);
                    grass.transform.localScale = Vector3.one * randomScale;

                    Renderer rend = grass.GetComponent<Renderer>();
                    if(rend != null && rend.sharedMaterial != null && rend.sharedMaterial.HasProperty("_Color")){
                        Color baseColor = rend.sharedMaterial.color;
                        float colorOffset = Random.Range(-colourVariation, colourVariation);
                        Color finalColor = baseColor + new Color(colorOffset, colorOffset, colorOffset, 0f);
                        rend.sharedMaterial.color = finalColor;
                    }



                    
                }
            }

            Debug.Log("Grass painting complete.");
        }

    }

    #if UNITY_EDITOR
    [CustomEditor(typeof(GrassPainter))]
    public class GrassPainterEditor : Editor{

        public override void OnInspectorGUI(){
            DrawDefaultInspector();

            GrassPainter grassPainter = (GrassPainter)target;
            if(GUILayout.Button("Clear Grass")){
                grassPainter.ClearGrass();
            }
            if(GUILayout.Button("Paint Grass")){
                grassPainter.PaintGrass();
            }
        }
    }
    #endif


}
