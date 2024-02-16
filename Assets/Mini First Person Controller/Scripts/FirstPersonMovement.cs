using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class FirstPersonMovement : NetworkBehaviour
{
    public NetworkVariable<float> speed = new NetworkVariable<float>(5);

    [Header("Running")]
    public NetworkVariable<bool> canRun = new NetworkVariable<bool>(true);
    public bool IsRunning { get; private set; }
    public NetworkVariable<float> runSpeed = new NetworkVariable<float>(9);
    public KeyCode runningKey = KeyCode.LeftShift;

    Rigidbody rigidbody;
    /// <summary> Functions to override movement speed. Will use the last added override. </summary>
    public List<System.Func<float>> speedOverrides = new List<System.Func<float>>();

    //interactions
    private RaycastHit hit;
    private GameObject heldObject;
    private float throwForceMax = 5000f;
    private float ChargeLevel = 0f;
    private int ChargeSpeed = 1000;

    public override void OnNetworkSpawn()
    {
        if (!IsLocalPlayer){
            return;
        }
        // Get the rigidbody on this.
        rigidbody = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        if (!IsLocalPlayer){
            return;
        }
        // Update IsRunning from input.
        IsRunning = canRun.Value && Input.GetKey(runningKey);

        // Get targetMovingSpeed.
        float targetMovingSpeed = IsRunning ? runSpeed.Value : speed.Value;
        if (speedOverrides.Count > 0)
        {
            targetMovingSpeed = speedOverrides[speedOverrides.Count - 1]();
        }

        // Get targetVelocity from input.
        Vector2 targetVelocity =new Vector2( Input.GetAxis("Horizontal") * targetMovingSpeed, Input.GetAxis("Vertical") * targetMovingSpeed);

        // Apply movement.
        rigidbody.velocity = transform.rotation * new Vector3(targetVelocity.x, rigidbody.velocity.y, targetVelocity.y);
        throwObject();
        dropHeldObject();
        handleInteraction();
    }

    void handleInteraction(){
        float interactDistance = 4f;
        if (hit.collider != null){
            hit.collider.gameObject.GetComponent<Highlight>()?.ToggleHighlight(false);
        }
        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, interactDistance)){
            if (hit.collider.CompareTag("Interactable") && heldObject == null){
                hit.collider.gameObject.GetComponent<Highlight>()?.ToggleHighlight(true);
                if (Input.GetKey(KeyCode.E) && hit.collider.CompareTag("Interactable")){
                    pickUpObject(hit.collider.gameObject);
                    hit.collider.gameObject.GetComponent<RunScriptObject>()?.RunAttachedScript();
                }
            }

        }
    }

    void dropHeldObject(){
        if (heldObject && Input.GetKey(KeyCode.F)){
            heldObject.transform.SetParent(null);
            heldObject.GetComponent<Rigidbody>().isKinematic = false;
            heldObject = null;
        }
        return;
    }

    void throwObject(){
        if (heldObject && Input.GetKey(KeyCode.Mouse0) && ChargeLevel < throwForceMax){
            if (ChargeLevel < throwForceMax){
                ChargeLevel += ChargeSpeed * Time.deltaTime;
            }
            //shake object
            heldObject.transform.localPosition = heldObject.transform.localPosition + new Vector3(Random.Range(-0.1f, 0.1f), Random.Range(-0.1f, 0.1f), Random.Range(-0.05f, 0.05f));
        }
        if (heldObject && !Input.GetKey(KeyCode.Mouse0) && ChargeLevel != 0f){
            GameObject throwingObject = heldObject;
            heldObject = null;
            throwingObject.transform.SetParent(null);
            throwingObject.GetComponent<Rigidbody>().isKinematic = false;
            throwingObject.GetComponent<Rigidbody>().AddForce(Camera.main.transform.forward * ChargeLevel);
            ChargeLevel = 0f;
        }
        return;
    }

    void pickUpObject(GameObject obj){
        if (obj.GetComponent<Rigidbody>() != null){
            heldObject = obj;
            heldObject.transform.SetParent(Camera.main.transform);
            heldObject.GetComponent<Rigidbody>().isKinematic = true;
        }
        return;
    }
}