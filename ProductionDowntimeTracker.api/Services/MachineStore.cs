using ProductionDowntimeTracker.api.Models;
using ProductionDowntimeTracker.api.Services;


namespace ProductionDowntimeTracker.api.Services

{
    public class MachineStore
    {
        private readonly List<Machine> _machines = new()
        {
            new Machine
            {
                Id = 1,
                Name = "Vstřikolis 01",
                IsRunning = true
            },

            new Machine
            {
                Id = 2,
                Name = "Montážní linka 02",
                IsRunning = false
            }
        };

        public IReadOnlyList<Machine> GetAll()
        {
            return _machines;
        }
        public Machine? GetById (int id)
        {
            return _machines.FirstOrDefault(machine => machine.Id == id);
        }
        public Machine Add(CreateMachineRequest request)
        {
            int newId = _machines.Count == 0
            ? 1
            : _machines.Max(machine => machine.Id) + 1;

            Machine newMachine = new Machine
            {
                Id = newId,
                Name = request.Name,
                IsRunning = request.IsRunning
            };

            _machines.Add(newMachine);

            return newMachine;
        }

        public Machine? Update(int id, UpdateMachineRequest request)
        {
            Machine? machine = _machines.FirstOrDefault(
                m => m.Id == id);

            if (machine is null)
            {
                return null;
            }

            machine.Name = request.Name;
            machine.IsRunning = request.IsRunning;

            return machine;
                
        }

        public bool Delete (int id)
        {
            Machine? machine = _machines.FirstOrDefault(m => m.Id == id);

            if (machine is null)
            {
                return false;
            }

            _machines.Remove(machine);

            return true;
        }

    }
}
