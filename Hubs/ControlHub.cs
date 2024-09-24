using GenericController_Backend.Entity;
using GenericController_Backend.Services;
using Microsoft.AspNetCore.SignalR;

namespace GenericController_Backend.Hubs;

public class ControlHub(PIDController pidController, SimulatorService simulatorService) : Hub
{
    public async Task SendProcessVariable(double processVariable)
    {
        // Compute the new output based on the process variable
        double output = pidController.Compute(processVariable);

        // Broadcast the computed output to all connected clients
        await Clients.All.SendAsync("ReceiveOutput", output);
    }

    public async Task SetControlParameters(ControlParameters controlParameters)
    {
        // Update the controller with new control parameters
        pidController = new PIDController(controlParameters);
        await Clients.All.SendAsync("ControlParametersUpdated", controlParameters);
    }
}
