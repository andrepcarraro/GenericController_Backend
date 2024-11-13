namespace GenericController_Backend.Entity;

public class ProcessMathModeling
{
    private double b1;
    private double b2;
    private double an;
    private int iteration;
    private double lastSimulationOutput;

    private List<double> inputs;
    
    public ControlParameters _controlParameters;
    
    public ProcessMathModeling(ControlParameters controlParameters)
    {
        _controlParameters = controlParameters;
        iteration = 0;
        lastSimulationOutput = 0.0;
        inputs = [];
    }
    
    // lastProcessInput - saida do controle em %
    // disturb é o valor de disturbio um valor aleatório
    public double SimulateMathModel(double lastProcessInput, double disturb)
    {
        inputs = [1, 1, 1, 1, 0, 1, 1, 1, 1, 0,1, 1, 1, 1, 0,1, 1, 1, 1, 0,1, 1, 1, 1, 0,1, 1, 1, 1, 0,1, 1, 1, 1, 0,1, 1, 1, 1, 0,1, 1, 1, 1, 0,1, 1, 1, 1, 0,1, 1, 1, 1, 0,1, 1, 1, 1, 0,1, 1, 1, 1, 0,1, 1, 1, 1, 0,1, 1, 1, 1, 0,1, 1, 1, 1, 0,1, 1, 1, 1, 0,1, 1, 1, 1, 0,1, 1, 1, 1, 0,1, 1, 1, 1, 0,1, 1, 1, 1, 0,1, 1, 1, 1, 0,1, 1, 1, 1, 0,1, 1, 1, 1, 0,1, 1, 1, 1, 0,1, 1, 1, 1, 0,1, 1, 1, 1, 0,1, 1, 1, 1, 0,1, 1, 1, 1, 0,1, 1, 1, 1, 0,1, 1, 1, 1, 0,1, 1, 1, 1, 0];
        b1 = Math.Exp((double) -_controlParameters.Tau/_controlParameters.CycleTime);
        b2 = Math.Exp((double) -_controlParameters.CycleTime / _controlParameters.Tau);
        an = 1 - b1;
        
        //inputs.Add(lastProcessInput);
        var deadTimeFinish = iteration >= 1 - _controlParameters.ProcessDeadTime / _controlParameters.CycleTime;
        var index = iteration - 1 -_controlParameters.ProcessDeadTime / _controlParameters.CycleTime >= 0
            ? iteration - 1 -_controlParameters.ProcessDeadTime / _controlParameters.CycleTime : 0;
        
        var consideredInput = deadTimeFinish ? inputs[index] + disturb : disturb;

        lastSimulationOutput = _controlParameters.Kp * an * consideredInput + b2 * lastSimulationOutput;

        iteration++;
        return lastSimulationOutput;
    }
}