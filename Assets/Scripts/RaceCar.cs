namespace Assets.Scripts
{
    public class RaceCar
    {
        public CarManager Car;
        public uint CheckpointIndex;

        public RaceCar(CarManager car = null, uint checkpointIndex = 1)
        {
            Car = car;
            CheckpointIndex = checkpointIndex;
        }
    }
}
