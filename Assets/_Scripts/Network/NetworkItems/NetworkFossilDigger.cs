using Fusion;
using UnityEngine;
using UnityEngine.Serialization;

public class NetworkFossilDigger : NetworkBehaviour
{
    [Header("Digger Settings")] 
    [SerializeField] private int maxCharges = 2;
    [SerializeField] private float autoDigDelay = 2f;
    [SerializeField] private float digTime = 2f;
    [SerializeField] private float digRange = 3f;
    [SerializeField] private GameObject fossilPrefab;
    [SerializeField] private float heightOffset = 0.01f;

    [Header("Visuals")] 
    [SerializeField] private Transform fossilSpawnPos;
    [SerializeField] private MeshRenderer[] batteryLights;
    
    [Networked] private TickTimer autoStartDigTimer { get; set; }
    [Networked] private TickTimer digProgressTimer { get; set; }
    [Networked] private NetworkBool isDigging { get; set; }
    [Networked] private int pendingFossilIndex { get; set; }
    [Networked, HideInInspector] private int CurrentCharges { get; set; }
    
    private NetworkFossilManager fossilManager;
    private int lastSeenFossilIndex = -1;
    
    
    public override void Spawned()
    {
        fossilManager = FindFirstObjectByType<NetworkFossilManager>();
        if (Object.HasStateAuthority) CurrentCharges = maxCharges;
    }
    
    public override void FixedUpdateNetwork()
    {
        if (!Object.HasStateAuthority) return;
        
        // Keep model grounded
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 2f))
        {
            float targetY = hit.point.y + heightOffset;
            transform.position = new Vector3(transform.position.x, targetY, transform.position.z);
                    
            // Align rotation to slope
            transform.rotation = Quaternion.FromToRotation(transform.up, hit.normal) * transform.rotation;
        }

        // Handle active digging process
        if (isDigging)
        {
            if (digProgressTimer.Expired(Runner)) CompleteDig();
            return; // Don't check for new fossils while digging
        }
        
        // Handle automatic detection and countdown
        if (CurrentCharges > 0)
        {
            CheckForAutoDig();
        }
    }

    private void CheckForAutoDig()
    {
        int currentIndex = fossilManager.GetClosestFossilIndex(transform.position, out Vector3 fossilPos);
        float dist = (currentIndex != -1) ? Vector3.Distance(transform.position, fossilPos) : float.MaxValue;

        // If we are in range of a valid fossil
        if (currentIndex != -1 && dist <= digRange)
        {
            // If this is a NEW fossil, reset everything
            if (currentIndex != lastSeenFossilIndex)
            {
                lastSeenFossilIndex = currentIndex;
                autoStartDigTimer = TickTimer.CreateFromSeconds(Runner, autoDigDelay);
                print("New fossil detected. Starting verification...");
            }

            // If the timer isn't running and hasn't expired yet, start it (when you get in range initially)
            else if (autoStartDigTimer.IsRunning == false && autoStartDigTimer.Expired(Runner) == false)
            {
                autoStartDigTimer = TickTimer.CreateFromSeconds(Runner, autoDigDelay);
                print("Auto dig verifying...");
            }

            // Only start digging if the timer is finished
            if (autoStartDigTimer.Expired(Runner))
            {
                StartDigging(currentIndex);
            }
        }
        else
        {
            // Reset if we move away or no fossils are near
            lastSeenFossilIndex = -1;
            autoStartDigTimer = TickTimer.None;
        }
    }
    
    private void StartDigging(int fossilIndex)
    {
        print("Starting the 2 second dig process..");
        
        isDigging = true;
        pendingFossilIndex = fossilIndex;
        digProgressTimer = TickTimer.CreateFromSeconds(Runner, digTime);
        
        CurrentCharges--;
        
        autoStartDigTimer = TickTimer.None; // Reset the auto-timer
    }

    private void CompleteDig()
    {
        print("Done!");
        isDigging = false;
        fossilManager.RPC_ClearFossil(pendingFossilIndex);
        Runner.Spawn(fossilPrefab, fossilSpawnPos.position, Quaternion.identity);
        lastSeenFossilIndex = -1;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("RechargeStation"))
            CurrentCharges = maxCharges;
    }
    
    public override void Render()
    {
        // Determine the base color based on charges
        Color chargeColor0 = (CurrentCharges >= 1) ? Color.green : Color.red;
        Color chargeColor1 = (CurrentCharges >= 2) ? Color.green : Color.red;

        // 2. Check if we are in the "Verification" phase (Waiting to dig)
        bool isVerifying = autoStartDigTimer.IsRunning;

        if (isVerifying)
        {
            // Create a fast flashing effect using a sine wave
            float flash = Mathf.PingPong(Time.time * 2f, 1f);
            chargeColor0 = Color.Lerp(chargeColor0, Color.yellow, flash);
            chargeColor1 = Color.Lerp(chargeColor1, Color.yellow, flash);
        }
        else if (isDigging)
        {
            // While digging, change color to something else
            chargeColor0 = Color.white;
            chargeColor1 = Color.white;
        }

        // 3. Apply the colors
        ApplyLightStyle(batteryLights[0], chargeColor0);
        ApplyLightStyle(batteryLights[1], chargeColor1);
    }

    private void ApplyLightStyle(MeshRenderer rend, Color col)
    {
        rend.material.SetColor("_BaseColor", col);
        rend.material.SetColor("_EmissionColor", col * 3f); // Brighter for visibility
    }
}