﻿using GenericController_Backend.Entity;
using GenericController_Backend.Hubs;
using Microsoft.AspNetCore.SignalR;
using System.Threading;

namespace GenericController_Backend.Services;

public class SimulatorService(IHubContext<ControlHub> hubContext, PIDController pidController)
{
  private double _processVariable = 50.0;  // Starting value for the process
  public int TimeBetweenSimulations = 1000;
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
    while (!cancellationToken.IsCancellationRequested)
    {
      // Simulate a changing process variable, e.g., temperature, pressure, etc.
      _processVariable += GetProcessVariableChange();

      // Send the new process variable to the controller
      double output = pidController.Compute(_processVariable);

      if (output is not double.NaN)
        await hubContext.Clients.All.SendAsync("Output", _processVariable, output);

      // Wait for a short time before the next iteration
      await Task.Delay(TimeBetweenSimulations);
    }
  }

  public void StopSimulation()
  {
    _cancellationTokenSource.Cancel(); // Cancel the task if needed
  }

  private double GetProcessVariableChange()
  {
    // Simulate some random fluctuations in the process variable
    return new Random().NextDouble() * 2.0 - 1.0;  // -1.0 to +1.0 change
  }
}

