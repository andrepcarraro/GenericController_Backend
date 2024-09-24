namespace GenericController_Backend.Entity;
public class PIDController(ControlParameters controlParameters)
{
    public double LastError { get; private set; } = 0;
    public double PrevError { get; private set; } = 0;
    public double Integral { get; private set; } = 0;

    private bool _autoModeState = controlParameters.AutoMode;

    // Sa�da atual do controlador
    private double _currentOutput = 0;

    public double Compute(double processVariable)
    {
        double error = AdjustValueToScale(controlParameters.SetPoint) - AdjustValueToScale(processVariable);

        if (!controlParameters.IsDirect)
            error = -error;  // A��o reversa

        if (controlParameters.AutoMode)
        {
            double proportional = controlParameters.Kp * error;

            // Calcular Ki e Kd
            double Ki = controlParameters.Kp / controlParameters.Ti;
            double Kd = controlParameters.Kp * controlParameters.Td;

            // Integra��o
            Integral += Ki * error;

            if (_autoModeState != controlParameters.AutoMode)
            {
                _autoModeState = controlParameters.AutoMode;
                AdjustIntegralForBumpless();
            }

            // Derivada com base nos dois �ltimos erros
            double derivative = Kd * (error * LastError + PrevError);

            // Sa�da incremental (somando proporcional, integral e derivada)
            _currentOutput = proportional + Integral + derivative;

            // Atualiza��o dos erros
            PrevError = LastError;
            LastError = error;

            // Aplicar os limites e mapear a sa�da para o intervalo definido
            return _currentOutput > 100.0 ? 100.0 : _currentOutput;
        }
        else
        {
            if (_autoModeState != controlParameters.AutoMode)
                _autoModeState = controlParameters.AutoMode;

            // Modo manual, usar sa�da direta do operador
            _currentOutput = controlParameters.ManualOutput;
            return controlParameters.ManualOutput > 100.0 ? 100.0 : controlParameters.ManualOutput;
        }
    }

    // Fun��o para ajustar a integral na troca para modo autom�tico (bumpless)
    private void AdjustIntegralForBumpless()
    {
        double error = controlParameters.SetPoint - _currentOutput;
        if (!controlParameters.IsDirect)
            error = -error;

        // Ajustar a integral para que a sa�da n�o tenha um salto quando trocamos para modo autom�tico
        Integral = _currentOutput - controlParameters.Kp * error;
    }

    private double AdjustValueToScale(double value)
    {
        var percent = value * 100.0 / controlParameters.MaxOutput;
        return Math.Max(0.0, percent);
    }
}

public class ControlParameters
{
    public double Kp { get; set; }  // Proporcional
    public double Ti { get; set; }  // Integral
    public double Td { get; set; }  // Derivativo

    // Limites de 0% e 100%
    public double MinOutput { get; set; }  // Representa 0%
    public double MaxOutput { get; set; }  // Representa 100%

    // Modo autom�tico ou manual
    public bool AutoMode { get; set; } = true;

    // A��o direta ou reversa
    public bool IsDirect { get; set; } = true;

    // SetPoint
    public double SetPoint { get; set; }

    // Sa�da atual no modo manual
    public double ManualOutput { get; set; } = 0;
}
