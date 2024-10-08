namespace GenericController_Backend.Entity;
public class PIDController
{
    public ControlParameters _controlParameters;

    public PIDController(ControlParameters controlParameters)
    {
        _controlParameters = controlParameters;
        _controlParameters.AutoModeState = _controlParameters.AutoMode;
    }
    private double lastOutput = 0.0;  // m(k-1)
    private double previousError = 0.0;  // e(k-1)
    private double previousControl1 = 0.0;  // c(k-1)
    private double previousControl2 = 0.0;  // c(k-2)

    // Saída atual do controlador
    private double _currentOutput = 0.0;

    public double Compute(double processVariable)
    {
        double error = AdjustValueToScale(_controlParameters.SetPoint) - AdjustValueToScale(processVariable);

        if (!_controlParameters.IsDirect)
            error = -error;  // Ação reversa

        if (_controlParameters.AutoMode)
        {

            if (_controlParameters.AutoModeState != _controlParameters.AutoMode)
            {
                _controlParameters.AutoModeState = _controlParameters.AutoMode;
                AdjustForBumpless();
            }

            // Saída incremental (somando proporcional, integral e derivada)
            _currentOutput = Calculate(error, _currentOutput);

            // Aplicar os limites e mapear a saída para o intervalo definido
            return _currentOutput > 1.0 ? 1.0 : _currentOutput;
        }
        else
        {
            if (_controlParameters.AutoModeState != _controlParameters.AutoMode)
                _controlParameters.AutoModeState = _controlParameters.AutoMode;

            // Modo manual, usar saída direta do operador
            _currentOutput = _controlParameters.ManualOutput;
            return _controlParameters.ManualOutput > 1.0 ? 1.0 : _controlParameters.ManualOutput;
        }
    }

    public double Calculate(double currentError, double currentControl)
    {
        // Evitar divisão por zero na integral (Ti não deve ser zero)
        double integralTerm = 0;
        if (_controlParameters.Ti > 0)
        {
            integralTerm = (1 / _controlParameters.Ti) * currentError;
        }

        // Evitar valores extremos no termo derivativo
        double derivativeTerm = 0;
        double controlDifference = currentControl - 2 * previousControl1 + previousControl2;

        if (Math.Abs(controlDifference) > 1e-10)  // Evitar diferenças muito pequenas
        {
            derivativeTerm = _controlParameters.Td * controlDifference;
        }

        // Calcular m(k) com os três termos (proporcional, integral, derivativo)
        double mK = _controlParameters.Kp * (currentError - previousError)
                    + integralTerm
                    + derivativeTerm
                    + lastOutput;

        if (mK > 1.0)
          mK = 1.0;

        // Limitar o valor de mK para evitar overflow ou valores fora dos limites
        if (double.IsInfinity(mK) || double.IsNaN(mK) || mK < 0.0)
        {
            mK = lastOutput;  // Se mK for infinito ou NaN, manter o último valor válido
        }

        // Atualize os valores para a próxima iteração
        previousError = currentError;
        previousControl2 = previousControl1;
        previousControl1 = currentControl;
        lastOutput = mK;

        return mK;
    }

    // Função para ajustar a integral na troca para modo automático (bumpless)
    private void AdjustForBumpless()
    {
        previousError = 0;
        previousControl2 = 0;
        previousControl1 = 0;
        lastOutput = _currentOutput;
    }

    private double AdjustValueToScale(double value)
    {
        if (_controlParameters.MaxOutput == 0)
            return 0.0;

        var percent = value * 100.0 / _controlParameters.MaxOutput;

        var treatPercent = percent > 100.0 ? 1 : percent / 100;
        return treatPercent;
    }

    public void UpdateControllerParameters(ControlParameters controlParameters)
    {
        _controlParameters = controlParameters;
        _controlParameters.AutoModeState = _controlParameters.AutoMode;
    }

    public void ChangeMode(bool isAutoMode)
    {AutoModeState
        _controlParameters. = isAutoMode;
        _controlParameters.AutoMode = isAutoMode;
    }

    public void ChangeManualOutput(double manualOutput)
    {
        _controlParameters.ManualOutput = manualOutput;
    }

    public void resetParameters()
    {
        lastOutput = 0.0;  // m(k-1)
        previousError = 0.0;  // e(k-1)
        previousControl1 = 0.0;  // c(k-1)
        previousControl2 = 0.0;  // c(k-2)

        // Saída atual do controlador
        _currentOutput = 0.0;
    }
}

public class ControlParameters
{
    public double Kp { get; set; }  // Proporcional
    public double Ti { get; set; }  // Integral
    public double Td { get; set; }  // Derivativo

    // Limites de 0% e 100%
    public double MinOutput { get; set; }   // Representa 0%
    public double MaxOutput { get; set; }  // Representa 100%

    // Modo automático ou manual
    public bool AutoMode { get; set; } = true;

    // Modo automático ou manual
    public bool AutoModeState { get; set; } = true;

  // Ação direta ou reversa
  public bool IsDirect { get; set; } = true;

    // SetPoint
    public double SetPoint { get; set; }

    // Saída atual no modo manual
    public double ManualOutput { get; set; } = 0.0;

    //Tempo entre as interações do controlador em milissegundos
    public int CycleTime { get; set; }
}

