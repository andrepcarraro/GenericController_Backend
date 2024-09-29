using GenericController_Backend.Entity;
using GenericController_Backend.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace GenericController_Backend.Services;

public class SimulatorService(IHubContext<ControlHub> hubContext, PIDController pidController)
{
    public double ProcessVariable = 1;  // Starting value for the process
    private Task simulationTask;
    private CancellationTokenSource _cancellationTokenSource;

    public void StartSimulation()
    {
        // Prevent starting a new simulation if one is already running
        if (simulationTask != null && !simulationTask.IsCompleted)
        {
            return; // Simulation is already running
        }

        _cancellationTokenSource = new CancellationTokenSource();

        simulationTask = Task.Run(() => StartSimulationAsync(_cancellationTokenSource.Token));
    }

    private async Task StartSimulationAsync(CancellationToken cancellationToken)
    {
        double output = 0.0;
        while (!cancellationToken.IsCancellationRequested)
        {
            // Send the new process variable to the controller
            output = pidController.Compute(ProcessVariable);

            if (output is not double.NaN)
            {
                await hubContext.Clients.All.SendAsync("Output", new { output = AdjustOutputToScale(output), ProcessVariable, pidController._controlParameters.SetPoint });
            }

            // Wait for a short time before the next iteration
            await Task.Delay(pidController._controlParameters.CycleTime);
        }
    }

    public void StopSimulation()
    {
        if (_cancellationTokenSource != null)
            _cancellationTokenSource.Cancel(); // Cancel the task if needed
    }

    private double AdjustOutputToScale(double value)
    {
        return value * pidController._controlParameters.MaxOutput;
    }
}

