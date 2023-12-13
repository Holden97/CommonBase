using CommonBase;
using System.Collections.Generic;
using UnityEngine;

namespace CommonBase
{
    public class BoidsManager : MonoBehaviour
    {
        public GameObject boidAgent;
        public int totalAgentsCount = 100;
        public List<BoidAgent> boids;

        private void Awake()
        {
            ObjectPoolManager.Instance.CreatePool(200, boidAgent, "boidAgent");

            for (int i = 0; i < totalAgentsCount; i++)
            {
                var go = ObjectPoolManager.Instance.GetNextObject("boidAgent");
                go.transform.position = Random.insideUnitCircle * 30;
                boids.Add(go.GetComponent<BoidAgent>());
                go.GetComponent<BoidAgent>().InitPos(go.transform.position);
            }
        }

        private void Update()
        {
            foreach (var boid in boids)
            {
                boid.Tick(boids, Time.deltaTime);
            }

        }
    }

}
