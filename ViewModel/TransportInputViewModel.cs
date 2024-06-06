namespace BIA601_HW.ViewModel
{
    public class TransportInputViewModel
    {
        public int NumberOfCargos { get; set; }
        public List<CargoInput> Cargos { get; set; } = new List<CargoInput>();
        public int NumberOfAddresses { get; set; }
        public List<AddressInput> Addresses { get; set; } = new List<AddressInput>();
        public int NumberOfTrucks { get; set; }
        public List<TruckInput> Trucks { get; set; } = new List<TruckInput>();
    }

    public class CargoInput
    {
        public string CargoName { get; set; }
        public double CargoWeight { get; set; }
        public double CargoValue { get; set; }
    }

    public class AddressInput
    {
        public int AddressNumber { get; set; }
        public List<DistanceInput> Distances { get; set; } = new List<DistanceInput>();
    }

    public class DistanceInput
    {
        public int ToAddress { get; set; }
        public double Distance { get; set; }
    }

    public class TruckInput
    {
        public string TruckName { get; set; }
        public double TruckCapacity { get; set; }
        public List<CargoInput> Cargos { get; set; } = new List<CargoInput>();
        public List<int> Route { get; set; } = new List<int>();
    }
}
