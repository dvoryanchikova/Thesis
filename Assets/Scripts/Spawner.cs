using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    public GameObject prefabToSpawn;
    public float radiusToSpawnObjects;
    public int[] numberOfObjectsToSpawnOnLevel;
    public string tagName;
    public GameObject targetObject;

    private List<Vector3> spawnPoints = new List<Vector3>();
    private List<Boid> spawnedBoids = new List<Boid>();
    private List<BoxCollider> boidsBoxColliders = new List<BoxCollider>();

    public Color colorForEnemy;
    

    public void Spawn()
    {
        Boid leaderBoid = null;
        GameObject target = Instantiate(targetObject, transform.position, transform.rotation);
        target.GetComponent<RandomMovement>().setNewStartPos(transform.position);
        target.GetComponent<MeshRenderer>().material.color = colorForEnemy;

        for (int i = 0; i < numberOfObjectsToSpawnOnLevel[GameManager.instance.currentLevel]; i++)
        {
            Vector3 posToSpawn = transform.position + new Vector3(Random.Range(0, radiusToSpawnObjects), 0, Random.Range(0, radiusToSpawnObjects));
            while (spawnPoints.Contains(posToSpawn))
                posToSpawn = transform.position + new Vector3(Random.Range(0, radiusToSpawnObjects), 0, Random.Range(0, radiusToSpawnObjects));
            spawnPoints.Add(posToSpawn);
            GameObject go = Instantiate(prefabToSpawn);
            go.transform.position = posToSpawn;
            go.name = prefabToSpawn.name + " " + i.ToString();
            go.tag = tagName;
            go.GetComponent<MeshRenderer>().material.color = colorForEnemy;

            Boid goBoidScript = go.GetComponent<Boid>();
            goBoidScript.movementTarget = target.transform;
            if (leaderBoid == null)
            {
                goBoidScript.leaderBoid = leaderBoid = goBoidScript;
                goBoidScript.isLeader = true;
            }
            else
            {
                goBoidScript.leaderBoid = leaderBoid;
            }

            BoxCollider bc = go.GetComponent<BoxCollider>();
            if (!boidsBoxColliders.Contains(bc))
                boidsBoxColliders.Add(bc);
            if (!spawnedBoids.Contains(goBoidScript))
                spawnedBoids.Add(goBoidScript);
        }

        foreach (Boid boid in spawnedBoids)
        {
            foreach (BoxCollider bc in boidsBoxColliders)
            {
                boid.boids.Add(bc);   //если просто присвоить лист, то это будет сылка и у всех боидов будет сылка на один и тот же лист, что может неплохо, но сейчас не сработает             
            }
        }
    }
}
