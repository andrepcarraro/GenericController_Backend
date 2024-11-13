using GenericController_Backend.Entity;
using GenericController_Backend.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace GenericController_Backend.Services;

public class SimulatorService(IHubContext<ControlHub> hubContext, PIDController pidController)
{
    private Task simulationTask;
    private CancellationTokenSource _cancellationTokenSource;
    public double lastProcessOutput;
    
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
        lastProcessOutput = lastProcessOutput > 0.0 ? lastProcessOutput : 0.0;
        var processMathModeling =  new ProcessMathModeling(pidController._controlParameters);
        while (!cancellationToken.IsCancellationRequested)
        {
            var processVariable =
                AdjustOutputToScale(processMathModeling.SimulateMathModel(lastProcessOutput, pidController._controlParameters.Disturb));
            
            // Send the new process variable to the controller
            lastProcessOutput = pidController.Compute(processVariable);

            if (lastProcessOutput is not double.NaN)
            {
                await hubContext.Clients.All.SendAsync("Output", new { output = AdjustOutputToScale(lastProcessOutput), processVariable, pidController._controlParameters.SetPoint });
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

