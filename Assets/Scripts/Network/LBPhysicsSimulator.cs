using Mirror;
using UnityEngine;

public class LBPhysicsSimulator : MonoBehaviour
{
    PhysicsScene physicsScene;
    
    bool simulatePhysicsScene;

    private void Awake() {
        if(NetworkServer.active){
            physicsScene = gameObject.scene.GetPhysicsScene();
            simulatePhysicsScene = physicsScene.IsValid() && physicsScene != Physics.defaultPhysicsScene;
        }
    }

    private void FixedUpdate()
    {
        if(!NetworkServer.active) return;

        if(simulatePhysicsScene) physicsScene.Simulate(Time.fixedDeltaTime);
    }
}
