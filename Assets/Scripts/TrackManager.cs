using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts;
using UnityEngine;

public class TrackManager : MonoBehaviour
{
    public static TrackManager Instance;

    public CarManager PrototypeCar;
    public float MaxCheckpointDelay;

    private List<RaceCar> cars = new List<RaceCar>();
    private Checkpoint[] checkpoints;
    
    /// <summary>
    /// The length of the current track in Unity units (accumulated distance between successive checkpoints).
    /// </summary>
    public float TrackLength { get; private set; }

    
    private CarManager bestCar = null;
    /// <summary>
    /// The current best car (furthest in the track).
    /// </summary>
    public CarManager BestCar
    {
        get { return bestCar; }
        private set
        {
            if (bestCar != value)
            {
                if (BestCar != null)
                    BestCar.UpdateCarColor(Color.white);
                if (value != null)
                    value.UpdateCarColor(Color.green);

                bestCar = value;
                if (BestCarChanged != null)
                    BestCarChanged(bestCar);
            }
        }
    }
    /// <summary>
    /// Event for when the best car has changed.
    /// </summary>
    public event Action<CarManager> BestCarChanged;

    void Awake () {
	    if (Instance != null)
	    {
	        Debug.LogError("Multiple track managers detected");
	        return;
	    }
	    Instance = this;

        checkpoints = GetComponentsInChildren<Checkpoint>();
        CalculateCheckpointPercentages();
    }
	
	void Update () {

	    //Update reward for each enabled car on the track
        for (int i = 0; i < cars.Count; i++)
	    {
	        RaceCar car = cars[i];
	        if (car.Car.enabled)
	        {
	            car.Car.CurrentCompletionReward = GetCompletePerc(car.Car, ref car.CheckpointIndex);

                //Update best
                if (BestCar == null || car.Car.CurrentCompletionReward >= BestCar.CurrentCompletionReward)
                    BestCar = car.Car;
            }
	    }
    }


    public void SetCarAmount(int amount)
    {
        //Check arguments
        if (amount < 0) throw new ArgumentException("Amount may not be less than zero.");

        if (amount == cars.Count) return;

        if (amount > cars.Count)
        {
            //Add new cars
            for (int toBeAdded = amount - cars.Count; toBeAdded > 0; toBeAdded--)
            {
                GameObject carCopy = Instantiate(PrototypeCar.gameObject);
                cars.Add(new RaceCar(carCopy.GetComponent<CarManager>()));
                carCopy.SetActive(true);
            }
        }
        else if (amount < cars.Count)
        {
            //Remove existing cars
            for (int toBeRemoved = cars.Count - amount; toBeRemoved > 0; toBeRemoved--)
            {
                RaceCar last = cars[cars.Count - 1];
                cars.RemoveAt(cars.Count - 1);

                Destroy(last.Car.gameObject);
            }
        }
    }

    public void Restart()
    {
        foreach (var car in cars)
        {
            car.Car.Restart();
            car.CheckpointIndex = 1;
        }
        BestCar = null;
    }

    /// <summary>
    /// Returns an Enumerator for iterator through all cars currently on the track.
    /// </summary>
    public IEnumerator<CarManager> GetCarEnumerator()
    {
        for (int i = 0; i < cars.Count; i++)
            yield return cars[i].Car;
    }

    /// <summary>
    /// Calculates the percentage of the complete track a checkpoint accounts for. This method will
    /// also refresh the <see cref="TrackLength"/> property.
    /// </summary>
    private void CalculateCheckpointPercentages()
    {
        checkpoints[0].AccumulatedDistance = 0; //First checkpoint is start
        //Iterate over remaining checkpoints and set distance to previous and accumulated track distance.
        for (int i = 1; i < checkpoints.Length; i++)
        {
            checkpoints[i].DistanceToPrevious = Vector2.Distance(checkpoints[i].transform.position, checkpoints[i - 1].transform.position);
            checkpoints[i].AccumulatedDistance = checkpoints[i - 1].AccumulatedDistance + checkpoints[i].DistanceToPrevious;
        }

        //Set track length to accumulated distance of last checkpoint
        TrackLength = checkpoints[checkpoints.Length - 1].AccumulatedDistance;

        //Calculate reward value for each checkpoint
        for (int i = 1; i < checkpoints.Length; i++)
        {
            checkpoints[i].RewardValue = (checkpoints[i].AccumulatedDistance / TrackLength) - checkpoints[i - 1].AccumulatedReward;
            checkpoints[i].AccumulatedReward = checkpoints[i - 1].AccumulatedReward + checkpoints[i].RewardValue;
        }
    }

    // Calculates the completion percentage of given car with given completed last checkpoint.
    // This method will update the given checkpoint index accordingly to the current position.
    private float GetCompletePerc(CarManager car, ref uint curCheckpointIndex)
    {
        //Already all checkpoints captured
        if (curCheckpointIndex >= checkpoints.Length)
            return 1;

        //Calculate distance to next checkpoint
        float checkPointDistance = Vector2.Distance(car.transform.position, checkpoints[curCheckpointIndex].transform.position);

        //Check if checkpoint can be captured
        if (checkPointDistance <= checkpoints[curCheckpointIndex].CaptureRadius)
        {
            curCheckpointIndex++;
            car.CheckpointCaptured(); //Inform car that it captured a checkpoint
            return GetCompletePerc(car, ref curCheckpointIndex); //Recursively check next checkpoint
        }
        else
        {
            //Return accumulated reward of last checkpoint + reward of distance to next checkpoint
            return checkpoints[curCheckpointIndex - 1].AccumulatedReward + checkpoints[curCheckpointIndex].GetRewardValue(checkPointDistance);
        }
    }
}
