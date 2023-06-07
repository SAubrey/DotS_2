//using UnityEngine;
//using System;
//using UnityEditor;
//using Assets.Scripts.MapGenerator;
//using System.Collections.Generic;

//[Serializable]
//public class wbTexturing_2
//{
//    [SerializeField]
//    public Texture2D texture;
//    [SerializeField]
//    public bool useBump = false;
//    [SerializeField]
//    public Texture2D bumpmap;
//    [SerializeField]
//    public Vector2 tilesize = new Vector2(50, 50);
//    [SerializeField]
//    public bool enableGrass = false;


//    public Texture2D emptyBump;

//    [SerializeField]
//    public Color color = Color.white;

//    [SerializeField]
//    public AnimationCurve heightCurve = AnimationCurve.Linear(0.0f, 1.0f, 1.0f, 1.0f);

//    [SerializeField]
//    public AnimationCurve angleCurve = AnimationCurve.Linear(0.0f, 0.0f, 1.0f, 1.0f);

//    [SerializeField]
//    private string[] options = { "Height", "Angle" };
//    [SerializeField]
//    public int index = 0;



//    public void OnGUI()
//    {
//        texture = EditorGUILayout.ObjectField("Texture", texture, typeof(Texture2D)) as Texture2D;
//        tilesize = EditorGUILayout.Vector2Field("Tilesize", tilesize);

//        index = EditorGUILayout.Popup(index, options);
//        switch (index)
//        {
//            case (0):
//                heightCurve = EditorGUILayout.CurveField("Height Curve", heightCurve);
//                break;
//            case (1):
//                angleCurve = EditorGUILayout.CurveField("Angle Curve", angleCurve);
//                break;
//            default:
//                break;
//        }

//        EditorGUILayout.LabelField("");
//    }
//}

//[CustomEditor(typeof(Generator))]
//class MapEditor : Editor
//{
//    [SerializeField]
//    private bool useTexturing = false;

//    [SerializeField]
//    private bool useFoliage = false;

//    [SerializeField]
//    private float waterCoast = 0;

//    [SerializeField]
//    [Range(0, 1)]
//    private float maxSteepness = 0;

//    [SerializeField]
//    [Range(0, 1)]
//    private int grassDensity = 0;

//    [SerializeField]
//    private List<wbTexturing_2> texClass;

//    public override void OnInspectorGUI()
//    {
//        Generator mapGen = (Generator)target;

//        if (DrawDefaultInspector())
//        {
//            if (mapGen.AutoUpdate)
//            {
//                mapGen.Generate();
//            }
//        }

//        if (GUILayout.Button("Generate"))
//        {
//            mapGen.Generate();
//        }

//        useTexturing = EditorGUILayout.BeginToggleGroup("Setup textures", useTexturing);

//        if (useTexturing)
//        {
//            if (GUILayout.Button("Clear all textures"))
//            {
//                List<TTexture> textures = new List<TTexture>();
//                Assets.Scripts.MapGenerator.TerrainTexturing.GenerateTexture(textures);
//            }

//            foreach (var texture in texClass)
//            {
//                texture.OnGUI();
//            }

//            if (texClass.Count > 0)
//            {
//                if (GUILayout.Button("Delete last"))
//                {
//                    texClass.RemoveAt(texClass.Count - 1);
//                }
//            }

//            if (GUILayout.Button("Add texture"))
//            {
//                texClass.Add(new wbTexturing_2());
//            }

//            if (texClass.Count > 0 && texClass[texClass.Count - 1].texture != null)
//            {
//                if (GUILayout.Button("Assign new textures"))
//                {
//                    List<TTexture> textures = new List<TTexture>();
//                    for (int i = 0; i < texClass.Count; i++)
//                    {
//                        TTexture TTex = new TTexture();
//                        TTex.Texture = texClass[i].texture;
//                        //TTex.color = texClass[i].color;
//                        TTex.UseBump = texClass[i].useBump;
//                        if (texClass[i].useBump)
//                        {
//                            TTex.Bumpmap = texClass[i].bumpmap;
//                        }
//                        else
//                        {
//                            TTex.Bumpmap = texClass[i].emptyBump;
//                        }
//                        TTex.Tilesize = texClass[i].tilesize;
//                        TTex.Index = texClass[i].index;
//                        TTex.HeightCurve = texClass[i].heightCurve;
//                        TTex.AngleCurve = texClass[i].angleCurve;
//                        textures.Add(TTex);
//                    }
//                    TerrainTexturing.GenerateTexture(textures);
//                }
//            }
//        }
//        EditorGUILayout.EndToggleGroup();

//        useFoliage = EditorGUILayout.BeginToggleGroup("Setup foliage", useFoliage);

//        if (useFoliage)
//        {
//            waterCoast = EditorGUILayout.FloatField("Tree distance from coast", waterCoast);
//            maxSteepness = EditorGUILayout.FloatField("Max hill angle for trees", maxSteepness);

//            if (GUILayout.Button("Generate trees"))
//            {
//                TerrainFoliage.MaxSteepness = maxSteepness;
//                if (GameObject.Find("Water"))
//                {
//                    TerrainFoliage.WaterLevel = GameObject.Find("Water").transform.position.y + waterCoast;
//                }
//                else
//                {
//                    TerrainFoliage.WaterLevel = 0.0f;
//                }
//                TerrainFoliage.GenerateTrees();
//            }

//            if (GUILayout.Button("Remove trees"))
//            {
//                TerrainFoliage.ClearTrees();
//            }

//            grassDensity = EditorGUILayout.IntField("Grass density", grassDensity);

//            if (GUILayout.Button("Generate grass"))
//            {
//                TerrainFoliage.GenerateGrass();
//            }

//            if (GUILayout.Button("Remove grass"))
//            {
//                TerrainFoliage.ClearGrass();
//            }
//        }

//        EditorGUILayout.EndToggleGroup();
//    }
//}
