namespace GenericController_Backend.Entity;
public class PIDController
{
  private ControlParameters _controlParameters;

  public PIDController(ControlParameters controlParameters)
  {
    _controlParameters = controlParameters;
    _autoModeState = _controlParameters.AutoMode;
  }
  private double lastOutput = 0.0;  // m(k-1)
  private double previousError = 0.0;  // e(k-1)
  private double previousControl1 = 0.0;  // c(k-1)
  private double previousControl2 = 0.0;  // c(k-2)

  private bool _autoModeState;

  // Saída atual do controlador
  private double _currentOutput = 0.0;

  public double Compute(double processVariable)
  {
    double error = AdjustValueToScale(_controlParameters.SetPoint) - AdjustValueToScale(processVariable);

    if (!_controlParameters.IsDirect)
      error = -error;  // Ação reversa

    if (_controlParameters.AutoMode)
    {

      if (_autoModeState != _controlParameters.AutoMode)
      {
        _autoModeState = _controlParameters.AutoMode;
        AdjustForBumpless();
      }

      // Saída incremental (somando proporcional, integral e derivada)
      _currentOutput = Calculate(error, _currentOutput);

      // Aplicar os limites e mapear a saída para o intervalo definido
      return _currentOutput > 1.0 ? 1.0 : _currentOutput;
    }
    else
    {
      if (_autoModeState != _controlParameters.AutoMode)
        _autoModeState = _controlParameters.AutoMode;

      // Modo manual, usar saída direta do operador
      _currentOutput = _controlParameters.ManualOutput;
      return _controlParameters.ManualOutput > 1.0 ? 1.0 : _controlParameters.ManualOutput;
    }
  }

  public double Calculate(double currentError, double currentControl)
  {
    // Calcule m(k)
    double mK = _controlParameters.Kp * (currentError - previousError)
                + (1 / _controlParameters.Ti) * currentError
                + (_controlParameters.Td / (currentControl - 2 * previousControl1 + previousControl2))
                + lastOutput;

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
    _autoModeState = _controlParameters.AutoMode;
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

  // Ação direta ou reversa
  public bool IsDirect { get; set; } = true;

  // SetPoint
  public double SetPoint { get; set; }

  // Saída atual no modo manual
  public double ManualOutput { get; set; } = 0;
}

