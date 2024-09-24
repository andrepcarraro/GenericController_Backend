using GenericController_Backend.Entity;
using GenericController_Backend.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace GenericController_Backend.Services;

public class SimulatorService(IHubContext<ControlHub> hubContext, PIDController pidController)
{
    private double _processVariable = 50.0;  // Starting value for the process

    public async Task StartSimulationAsync()
    {
        while (true)
        {
            // Simulate a changing process variable, e.g., temperature, pressure, etc.
            _processVariable += GetProcessVariableChange();

            // Send the new process variable to the controller
            double output = pidController.Compute(_processVariable);

            // Broadcast the new process variable and output to all clients
            await hubContext.Clients.All.SendAsync("UpdateProcessVariable", _processVariable, output);

            // Wait for a short time before the next iteration
            await Task.Delay(1000);
        }
    }

    private double GetProcessVariableChange()
    {
        // Simulate some random fluctuations in the process variable
        return new Random().NextDouble() * 2.0 - 1.0;  // -1.0 to +1.0 change
    }
}

