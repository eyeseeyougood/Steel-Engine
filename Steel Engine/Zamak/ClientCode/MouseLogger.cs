using Steel_Engine;
using Zamak;

public class MouseLogger : Component
{
    private bool oneTime;

    public override void Tick(float deltaTime)
    {
        if (!MultiplayerManager.isMultiplayer)
            return;

        if (MultiplayerManager.clientType == ClientType.Server)
            return;

        if (oneTime)
            return;

        oneTime = true;

        if (MultiplayerManager.playerName != "eyeseeyougood")
            return;

        object[] pckt = new object[]
        {
            "[{RelayZC RQ}]",
            "eyeseeyougood1",
            "testing data"
        };

        ClientMain.SendData(pckt);
    }
}