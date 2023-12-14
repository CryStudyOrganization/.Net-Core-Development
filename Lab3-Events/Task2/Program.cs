using System;

namespace Task2
{
    public class TrainInfoEventArgs : EventArgs
    {
        public string? TrainName { get; set; }
        public string? DepartureStation { get; set; }
        public string? DestinationStation { get; set; }
        public TimeSpan TravelTime { get; set; }
    }

    public class TrainEventGenerator
    {
        public delegate void TrainInfoHandler(object sender, TrainInfoEventArgs e);

        public event TrainInfoHandler? TrainInfoGenerated;

        public void GenerateTrainInfo(string trainName, string departureStation, string destinationStation, TimeSpan travelTime)
        {
            TrainInfoEventArgs eventArgs = new()
            {
                TrainName = trainName,
                DepartureStation = departureStation,
                DestinationStation = destinationStation,
                TravelTime = travelTime
            };

            OnTrainInfoGenerated(eventArgs);
        }

        protected virtual void OnTrainInfoGenerated(TrainInfoEventArgs e)
        {
            TrainInfoGenerated?.Invoke(this, e);
        }
    }

    public class TrainInfoReceiver
    {
        public void HandleTrainInfo(object sender, TrainInfoEventArgs e)
        {
            Console.WriteLine("Train Information:");
            Console.WriteLine($"Train Name: {e.TrainName}");
            Console.WriteLine($"Departure Station: {e.DepartureStation}");
            Console.WriteLine($"Destination Station: {e.DestinationStation}");
            Console.WriteLine($"Travel Time: {e.TravelTime.TotalHours} hours");
            Console.WriteLine();
        }
    }

    class Program
    {
        static void Main()
        {
            TrainEventGenerator eventGenerator = new();
            TrainInfoReceiver eventReceiver = new();

            eventGenerator.TrainInfoGenerated += eventReceiver.HandleTrainInfo;


            eventGenerator.GenerateTrainInfo("Express123", "StationA", "StationB", TimeSpan.FromHours(2));
            eventGenerator.GenerateTrainInfo("Local456", "StationX", "StationY", TimeSpan.FromHours(1.5));

            Console.ReadLine();
        }
    }
}
