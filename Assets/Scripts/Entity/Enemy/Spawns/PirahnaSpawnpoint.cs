using Photon.Pun;

public class PirahnaSpawnpoint : EnemySpawnpoint
{
    private PiranhaPlantController plant;

    public void Start()
    {
        plant = GetComponent<PiranhaPlantController>();
    }

    public override bool AttemptSpawning()
    {
        plant.photonView.RPC("Respawn", RpcTarget.All);
        return true;
    }
}