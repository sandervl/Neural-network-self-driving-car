using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CarManager : MonoBehaviour
{
    private static int idGenerator = 0;
    /// <summary>
    /// Returns the next unique id in the sequence.
    /// </summary>
    private static int NextID
    {
        get { return idGenerator++; }
    }

    private CarMovement carMovement;
    private List<Sensor> sensors;
    private List<SpriteRenderer> spriteRenderers;
    private bool isAlive;
    public float[] CurrentControlInputs { get; set; }
    public Agent Agent { get; set; }

    public float CurrentCompletionReward
    {
        get { return Agent.Genotype.Evaluation; }
        set { Agent.Genotype.Evaluation = value; }
    }

    private float timeSinceLastCheckpoint;
    
    void Awake()
    {
        carMovement = GetComponent<CarMovement>();
        sensors = GetComponentsInChildren<Sensor>().ToList();
        spriteRenderers = GetComponentsInChildren<SpriteRenderer>().ToList();
    }

    void Start()
    {
        isAlive = true;
        name = "Car (" + NextID + ")";
    }
    void Update()
    {
        timeSinceLastCheckpoint += Time.deltaTime;
    }

    void FixedUpdate()
    {
        var nnOutputs = Agent.FNN.ProcessInputs(sensors.Select(s => s.ReportDistance).ToArray());
        CurrentControlInputs = nnOutputs.Select(i => (float)i).ToArray();
        carMovement.SetInputs(CurrentControlInputs);

        if (timeSinceLastCheckpoint > TrackManager.Instance.MaxCheckpointDelay)
        {
            Die();
        }
    }

    public void Restart()
    {
        transform.position = Vector3.zero;
        transform.rotation = Quaternion.identity;
        Agent.Reset();
        timeSinceLastCheckpoint = 0;
        SetAlive(true);
    }

    void OnCollisionEnter2D()
    {
        Die();
    }

    void Die()
    {
        Agent.Kill();
        SetAlive(false);
    }

    void SetAlive(bool value)
    {
        isAlive = value;
        carMovement.enabled = value;
        foreach (var sensor in sensors)
        {
            sensor.gameObject.SetActive(value);
        }
        UpdateCarColor(Color.white);
        enabled = value;
    }

    public void CheckpointCaptured()
    {
        timeSinceLastCheckpoint = 0;
    }

    public void UpdateCarColor(Color color)
    {
        if (!isAlive)
            color = Color.black;

        foreach (var spriteRenderer in spriteRenderers)
        {
            spriteRenderer.color = color;
        }
    }
}
