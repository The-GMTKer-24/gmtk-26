using System;
using UnityEngine;

public class TutorialDoorTrigger : MonoBehaviour
{
    [SerializeField] private GameObject enemyToSpawn;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private TutorialDoor tutorialDoor;
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            GameObject spawned =  Instantiate(enemyToSpawn, spawnPoint.transform.position, Quaternion.identity);
            spawned.AddComponent<TutorialGnome>();
            spawned.GetComponent<TutorialGnome>().callbackDoor = tutorialDoor;
            tutorialDoor.CloseDoors();
            gameObject.SetActive(false);
            // Destroy(gameObject);
        }
    }
}
