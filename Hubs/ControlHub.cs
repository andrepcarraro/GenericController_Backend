using GenericController_Backend.Entity;
using GenericController_Backend.Services;
using Microsoft.AspNetCore.SignalR;

namespace GenericController_Backend.Hubs;

public class ControlHub(SimulatorService simulatorService, PIDController PidController) : Hub
{
    ControlParameters ControlParameters = new ControlParameters();

    public override async Task OnConnectedAsync()
    {
        Console.WriteLine($"Client connected: {Context.ConnectionId}");

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception exception)
    {
        Console.WriteLine($"Client disconnected: {Context.ConnectionId}");
        simulatorService.StopSimulation();

        await base.OnDisconnectedAsync(exception);
    }

    public async Task SendProcessVariable(double processVariable)
    {
        // Compute the new output based on the process variable
        double output = PidController.Compute(processVariable);

        // Broadcast the computed output to all connected clients
        await Clients.All.SendAsync("ReceiveOutput", output);
    }

    public async Task SetControlParameters(double kp, double ti, double td, double minOutput, double maxOutput, bool autoMode, bool isDirect, double setPoint, double manualOutput, int cycleTime)
    {
        var controlParameters = new ControlParameters()
        {
            Kp = kp,
            Ti = ti,
            Td = td,
            MinOutput = minOutput,
            MaxOutput = maxOutput,
            AutoMode = autoMode,
            IsDirect = isDirect,
            SetPoint = setPoint,
            ManualOutput = manualOutput,
            CycleTime = cycleTime
        };

        ControlParameters = controlParameters;
        PidController.UpdateControllerParameters(controlParameters);

        await Clients.All.SendAsync("ControlParametersUpdated", true);
    }


    public async Task StartSimulation()
    {
        simulatorService.StartSimulation();
        await Clients.All.SendAsync("StartSimulation", true);
    }

    public async Task StopSimulation()
    {
        simulatorService.StopSimulation();
        await Clients.All.SendAsync("StopSimulation", true);
    }

    public async Task SetProcessVariable(double processVariable)
    {
        simulatorService.ProcessVariable = processVariable;

        await Clients.All.SendAsync("ProcessVariableUpdated", true);
    }
}
