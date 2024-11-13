using GenericController_Backend.Entity;
using GenericController_Backend.Services;
using Microsoft.AspNetCore.SignalR;

namespace GenericController_Backend.Hubs;

public class ControlHub(SimulatorService simulatorService, PIDController PidController) : Hub
{
    bool IsAutoMode = true;
    double ManualOutput = 0.0;

    public override async Task OnConnectedAsync()
    {
        Console.WriteLine($"Client connected: {Context.ConnectionId}");
        simulatorService.lastProcessOutput = 0.0;
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

    public async Task SetControlParameters(double kp, double ti, double td, double minOutput, double maxOutput, bool isDirect, double setPoint, int cycleTime, int tau, double disturb, int deadCycles)
    {
        var controlParameters = new ControlParameters()
        {
            Kp = kp,
            Ti = ti,
            Td = td,
            MinOutput = minOutput,
            MaxOutput = maxOutput,
            AutoMode = IsAutoMode,
            IsDirect = isDirect,
            SetPoint = setPoint,
            ManualOutput = ManualOutput,
            CycleTime = cycleTime,
            Tau = tau,
            Disturb = disturb,
            ProcessDeadTime = deadCycles
        };

        PidController.UpdateControllerParameters(controlParameters);
        PidController.resetParameters();
        simulatorService.lastProcessOutput = 0.0;
        await Clients.All.SendAsync("ControlParametersUpdated", controlParameters);
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
    
    public async Task ChangeMode(bool isAutoMode)
    {
        IsAutoMode = isAutoMode;

        PidController.ChangeMode(isAutoMode);

        await Clients.All.SendAsync("modeChanged", isAutoMode);
    }

    public async Task SetManualOutput(double manualOutput)
    {
        ManualOutput = manualOutput;

        PidController.ChangeManualOutput(manualOutput);

        await Clients.All.SendAsync("manualOutputUpdated", manualOutput);
    }
}
