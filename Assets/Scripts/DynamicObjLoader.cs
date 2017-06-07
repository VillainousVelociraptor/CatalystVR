﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class DynamicObjLoader : MonoBehaviour {

    public void Start()
    {


        LoadOBJ("CAVEkiosk_SiteData/EastMound/Catal_East Mound.obj");


    }


    public static GameObject LoadOBJ(string filePath, Transform parent = null)
    {

        List<Material> loadedMaterials = new List<Material>();

        List<Vector3> vertices = new List<Vector3>();
        List<Vector3> normals = new List<Vector3>();
        List<int> faces = new List<int>();
        List<Vector2> texCoords = new List<Vector2>();

        string[] objText = File.ReadAllLines(filePath);

        foreach (string line in objText)
        {

            string[] lineVals = line.Split(' ', '/');

            switch (lineVals[0])
            {

                case "v":
                    Vector3 vertex = new Vector3(float.Parse(lineVals[1]), float.Parse(lineVals[2]), float.Parse(lineVals[2]));
                    vertices.Add(vertex);
                    break;

                case "n":
                    Vector3 normal = new Vector3(float.Parse(lineVals[1]), float.Parse(lineVals[2]), float.Parse(lineVals[2]));
                    normals.Add(normal);
                    break;

                case "f":
                    
                    for (int i = 1; i < lineVals.Length; i++)
                    {

                        faces.Add(int.Parse(lineVals[i]));

                    }

                    break;

                case "vt":

                    Vector2 texCoord = new Vector2(float.Parse(lineVals[1]), float.Parse(lineVals[2]));
                    texCoords.Add(texCoord);
                    break;

                case "mtllib":

                    string fileName = lineVals[1];

                    for (int i = 2; i < lineVals.Length; i++)
                    {

                        fileName += " ";
                        fileName += lineVals[i];

                    }

                    loadedMaterials = LoadMaterial(fileName, Path.GetDirectoryName(filePath));

                    break;

            }
        }


        List<Mesh> createdMeshes = CreateMeshes(vertices, normals, faces, texCoords);

        GameObject parentObject = new GameObject();

        foreach (Mesh mesh in createdMeshes)
        {

            GameObject newObj = new GameObject();
            newObj.transform.localPosition = Vector3.zero;
            newObj.transform.localRotation = Quaternion.identity;

            MeshFilter meshfilter = newObj.AddComponent<MeshFilter>();
            meshfilter.mesh = mesh;

            MeshRenderer renderer = newObj.AddComponent<MeshRenderer>();

            renderer.materials = loadedMaterials.ToArray();

            newObj.transform.parent = parentObject.transform;

        }

        return parentObject;

    }

    public static List<Mesh> CreateMeshes(List<Vector3> vertices, List<Vector3> normals, List<int> indices, List<Vector2> texCoords)
    {

        List<Mesh> meshes = new List<Mesh>();

        Mesh activeMesh = new Mesh();
        List<Vector3> activeVertices = new List<Vector3>();
        List<int> activeIndices = new List<int>();
        List<Vector3> activeNormals = new List<Vector3>();
        List<Vector2> activeTexCoords = new List<Vector2>();
        Dictionary<int, int> dict = new Dictionary<int, int>();

        
        // Iterate through all the face indices
        for (int i = 0; i < indices.Count; i++)
        {

            // Unity's vertex max is 65k vertices. Every time we hit that, create a new mesh.
            if (activeVertices.Count >= 65000)
            {

                activeMesh.SetVertices(activeVertices);
                activeMesh.SetNormals(activeNormals);
                activeMesh.SetUVs(0, activeTexCoords);
                activeMesh.SetIndices(activeIndices.ToArray(), MeshTopology.Triangles, 0);
                meshes.Add(activeMesh);

                activeMesh = new Mesh();
                dict.Clear();
                activeVertices.Clear();
                activeNormals.Clear();
                activeTexCoords.Clear();
                activeIndices.Clear();

            }

            // This is the original vertex/normal/texcoords index
            int originalIndex = indices[i];

            // This will be the new index of the vertex
            int newIndex = 0;

            // If we've already encountered this index, no need to create a new Vertex
            if (dict.ContainsKey(originalIndex))
            {

                // Grab the new, updated index
                newIndex = dict[originalIndex];

            }

            // If we haven't yet encountered this index in this mesh, we need to create a new vertex.
            else
            {

                newIndex = activeVertices.Count;

                dict.Add(originalIndex, newIndex);

                Vector3 newVertex = vertices[originalIndex];
                Vector3 newNormal = normals[originalIndex];
                Vector2 newTexCoord = texCoords[originalIndex];

                activeVertices.Add(newVertex);
                activeNormals.Add(newNormal);
                activeTexCoords.Add(newTexCoord);

            }

            // Save the new index in our list of faces.
            activeIndices.Add(newIndex);

        }

        return meshes;

    }

    public static List<Material> LoadMaterial(string materialName, string directory)
    {

        List<Material> materials = new List<Material>();

        string matFilePath = directory + "/" + materialName;

        if (!File.Exists(matFilePath))
        {

            Debug.LogErrorFormat("Could not find material file {0} in directory {1}", materialName, directory);
            return null;

        }

        Material matToLoad = new Material(Shader.Find("Standard"));

        string[] matFile = File.ReadAllLines(matFilePath);

        Material activeMaterial = new Material(Shader.Find("Standard (Specular setup)"));

        foreach (string line in matFile)
        {

            string[] lineVals = line.Split(' ');

            List<string> strippedLineVals = new List<string>();

            foreach (string str in lineVals)
            {
                if (!string.IsNullOrEmpty(str) && str != " ")
                {
                    strippedLineVals.Add(str);
                }
            }

            if (strippedLineVals.Count == 0)
            {
                continue;
            }

            lineVals = strippedLineVals.ToArray();

            string propertyName = "";
            int propertyid;

            switch (lineVals[0])
            {

                case "newmtl":

                    materials.Add(activeMaterial);
                    activeMaterial = new Material(Shader.Find("Standard (Specular setup)"));
                    activeMaterial.name = lineVals[1];
                    break;

                case "Ka":
                    Debug.LogFormat("Args: ({0} {1} {2})", lineVals[1], lineVals[2], lineVals[3]);
                    Color ambientColor = new Color(float.Parse(lineVals[1]), float.Parse(lineVals[2]), float.Parse(lineVals[3]));
                    propertyName = "_AmbientColor";
                    propertyid = Shader.PropertyToID(propertyName);
                    activeMaterial.SetColor(propertyid, ambientColor);
                    break;

                case "Kd":
                    Color diffuseColor = new Color(float.Parse(lineVals[1]), float.Parse(lineVals[2]), float.Parse(lineVals[3]));
                    propertyName = "_Color";
                    propertyid = Shader.PropertyToID(propertyName);
                    Color existingColor = activeMaterial.GetColor(propertyid);
                    diffuseColor.a = existingColor.a;
                    activeMaterial.SetColor(propertyid, diffuseColor);
                    break;

                case "Ks":
                    Color specularColor = new Color(float.Parse(lineVals[1]), float.Parse(lineVals[2]), float.Parse(lineVals[3]));
                    propertyName = "_SpecColor";
                    propertyid = Shader.PropertyToID(propertyName);
                    activeMaterial.SetColor(propertyid, specularColor);
                    break;

                case "d":
                    float dissolve = float.Parse(lineVals[1]);
                    propertyName = "_Color";
                    propertyid = Shader.PropertyToID(propertyName);
                    Color currentColor = activeMaterial.GetColor(propertyid);
                    Color newColor = new Color(currentColor.r, currentColor.g, currentColor.b, dissolve);
                    activeMaterial.SetColor(propertyid, newColor);
                    break;

                case "Ns":
                    propertyName = "_SpecularHighlights";
                    propertyid = Shader.PropertyToID(propertyName);
                    float specularHighlight = float.Parse(lineVals[1]);
                    activeMaterial.SetFloat(propertyid, specularHighlight);
                    break;

                case "illium":
                    break;

                case "map_Kd":

                    string textureName = lineVals[1];

                    for (int i = 2; i < lineVals.Length; i++)
                    {

                        textureName += " ";
                        textureName += lineVals[i];

                    }

                    Texture2D texture = LoadTexture(textureName, directory);
                    activeMaterial.mainTexture = texture;
                    break;

            }
        }

        return materials;

    }

    public static Texture2D LoadTexture(string texturePath, string directory)
    {

        Texture2D texToLoad = new Texture2D(1, 1);

        string filePath = directory + "/" + texturePath;

        if (!File.Exists(filePath))
        {

            Debug.LogErrorFormat("Could not find texture {0} in directory {1}", texturePath, directory);
            return null;
   
        }


        byte[] imageData = File.ReadAllBytes(filePath);
        texToLoad.LoadImage(imageData);

        return texToLoad;

    }
}