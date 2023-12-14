using System;

namespace Task3
{
    public class CarEventArgs : EventArgs
    {
        public string? OldName { get; set; }
        public string? NewName { get; set; }
        public bool Cancel { get; set; }
    }

    public class Car
    {
        private string? _name;

        public event EventHandler<CarEventArgs>? NameChanging;
        public event EventHandler<CarEventArgs>? NameChanged;

        public string? Name
        {
            get => _name;
            set
            {
                if (_name != value)
                {
                    CarEventArgs args = new() { OldName = _name, NewName = value };
                    OnNameChanging(args);

                    if (!args.Cancel)
                    {
                        _name = value;
                        OnNameChanged(new CarEventArgs { OldName = args.OldName, NewName = args.NewName });
                    }
                }
            }
        }

        protected virtual void OnNameChanging(CarEventArgs e)
        {
            NameChanging?.Invoke(this, e);
        }

        protected virtual void OnNameChanged(CarEventArgs e)
        {
            NameChanged?.Invoke(this, e);
        }
    }

    public class Program
    {
        public static void Main()
        {
            Car myCar = new();

            myCar.NameChanging += HandleCarNameChanging;
            myCar.NameChanged += HandleCarNameChanged;

            myCar.Name = "МійАвтомобіль1";

            myCar.Name = "МійАвтомобіль2";

            Console.ReadLine();
        }

        private static void HandleCarNameChanging(object? sender, CarEventArgs e)
        {
            Console.WriteLine($"Зміна назви автомобіля з '{e.OldName}' на '{e.NewName}'.");
            Console.Write("Прийняти зміни? (Y/N): ");

            string? response = Console.ReadLine()?.ToUpper();

            e.Cancel = !(response == "Y");
        }

        private static void HandleCarNameChanged(object? sender, CarEventArgs e)
        {
            if (sender != null)
            {
                Console.WriteLine($"Назва автомобіля змінена: з '{e.OldName}' на '{e.NewName}'.");
            }
            else
            {
                Console.WriteLine("Null!");
            }
        }
    }


}