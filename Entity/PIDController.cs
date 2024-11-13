namespace GenericController_Backend.Entity;
public class PIDController
{
    public volatile ControlParameters _controlParameters;

    public PIDController(ControlParameters controlParameters)
    {
        _controlParameters = controlParameters;
    }
    private double lastOutput = 0.0;  // m(k-1)
    private double previousError = 0.0;  // e(k-1)
    private double previousControl1 = 0.0;  // c(k-1)
    private double previousControl2 = 0.0;  // c(k-2)

    // Sa�da atual do controlador
    private double _currentOutput = 0.0;

    public double Compute(double processVariable)
    {
        var error = AdjustValueToScale(_controlParameters.SetPoint) - AdjustValueToScale(processVariable);

        if (!_controlParameters.IsDirect)
            error = -error;  // controle reverso

        if (_controlParameters.AutoMode)
        {
            // Saida incremental (somando proporcional, integral e derivada)
            _currentOutput = Calculate(error, _currentOutput);

            // Aplicar os limites e mapear a sa�da para o intervalo definido
            return _currentOutput > 1.0 ? 1.0 : _currentOutput;
        }
        else
        {
            // Modo manual, usar sa�da direta do operador
            _currentOutput = _controlParameters.ManualOutput;
            previousError = error;
            previousControl2 = previousControl1;
            previousControl1 = _currentOutput;
            lastOutput = _currentOutput;
            
            return _controlParameters.ManualOutput > 1.0 ? 1.0 : _controlParameters.ManualOutput;
        }
    }

    public double Calculate(double currentError, double currentControl)
    {
        // Evitar divis�o por zero na integral (Ti n�o deve ser zero)
        double integralTerm = 0;
        if (_controlParameters.Ti > 0)
        {
            integralTerm = (1 / _controlParameters.Ti) * currentError;
        }

        // Evitar valores extremos no termo derivativo
        double derivativeTerm = 0;
        double controlDifference = currentControl - 2 * previousControl1 + previousControl2;

        if (Math.Abs(controlDifference) > 1e-10)  // Evitar diferen�as muito pequenas
        {
            derivativeTerm = _controlParameters.Td * controlDifference;
        }

        // Calcular m(k) com os tr�s termos (proporcional, integral, derivativo)
        double mK = _controlParameters.Kp * (currentError - previousError)
                    + integralTerm
                    + derivativeTerm
                    + lastOutput;

        if (mK > 1.0)
          mK = 1.0;

        // Limitar o valor de mK para evitar overflow ou valores fora dos limites
        if (double.IsInfinity(mK) || double.IsNaN(mK) || mK < 0.0)
        {
            mK = lastOutput;  // Se mK for infinito ou NaN, manter o �ltimo valor v�lido
        }

        // Atualize os valores para a pr�xima itera��o
        previousError = currentError;
        previousControl2 = previousControl1;
        previousControl1 = currentControl;
        lastOutput = mK;

        return mK;
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
    }

    public void ChangeMode(bool isAutoMode)
    {
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

        // Sa�da atual do controlador
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

    // Modo autom�tico ou manual
    public bool AutoMode { get; set; } = true;
    

    // A��o direta ou reversa
    public bool IsDirect { get; set; } = true;

    // SetPoint
    public double SetPoint { get; set; }

    // Sa�da atual no modo manual
    public double ManualOutput { get; set; } = 0.0;

    //Tempo entre as intera��es do controlador em milissegundos
    public int CycleTime { get; set; }
    
    //Tau - Constante de tempo de resposta do processo
    public int Tau { get; set; }
    
    //Valor aplicado do disturbio ao processo
    public double Disturb { get; set; }
    
    //Valor que indica o numero de ciclos que o processo demora para começar a responder ao controle
    public int ProcessDeadTime { get; set; }
}

